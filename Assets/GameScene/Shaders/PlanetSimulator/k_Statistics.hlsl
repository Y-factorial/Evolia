#include "h_Structs.hlsl"
#include "h_Variables.hlsl"
#include "h_PhysicalConstants.hlsl"
#include "h_Functions.hlsl"

#define REDUCTION_SIZE 64

groupshared float temperatureSum[REDUCTION_SIZE];
groupshared float oceanDepthSum[REDUCTION_SIZE];
groupshared float photosynthesisSum[REDUCTION_SIZE];
groupshared float respirationSum[REDUCTION_SIZE];
groupshared uint oceanTileCount[REDUCTION_SIZE];

[numthreads(REDUCTION_SIZE, 1, 1)]
void StatisticsReductRow1(uint2 groupId : SV_GroupID, uint2 groupThreadId : SV_GroupThreadID)
{
    // 横方向に1段階目の統計を取る
    // 1グループに付き REDUCTION_SIZE マスを合計するので、結果は width/REDUCTION_SIZE 個作られる。
    uint resultCount = ceil(size.x * 1.0f / REDUCTION_SIZE);

    uint localX = groupThreadId.x; // ワークグループ内のスレッドID
    uint y = groupId.y; // 行インデックス、全行別のグループとして動くので、グループIDが行インデックスになる
    uint x = groupId.x * REDUCTION_SIZE + localX; // 複数グループに分かれて動くので、グループ内のスレッドIDを加算してグローバルインデックスを計算

    if (y >= size.y)
    {
        return;
    }
    
    // グローバルインデックス計算
    uint xy = y * size.x + x;
    
    // シェアードメモリにデータを読み込み
    temperatureSum[localX] = (x < size.x) ? tile.temperature : 0.0;
    oceanDepthSum[localX] = (x < size.x) ? max(0, planet.ocean.seaLevel - tile.elevation) : 0.0;
    photosynthesisSum[localX] = (x < size.x) ?
        col[tile.microbe.species].photosynthesis
        + col[tile.plant.species].photosynthesis
        + col[tile.animal.species].photosynthesis : 0.0;
    respirationSum[localX] = (x < size.x) ?
        col[tile.microbe.species].respiration
        + col[tile.plant.species].respiration  
        + col[tile.animal.species].respiration : 0.0;
    oceanTileCount[localX] = (x < size.x) ? (tile.elevation < planet.ocean.seaLevel ? 1 : 0) : 0;

    // 並列リダクションで行全体の合計を計算
    for (uint stride = REDUCTION_SIZE / 2; stride > 0; stride /= 2)
    {
        // バリア同期
        GroupMemoryBarrierWithGroupSync();

        if (localX < stride)
        {
            temperatureSum[localX] += temperatureSum[localX + stride];
            oceanDepthSum[localX] += oceanDepthSum[localX + stride];
            photosynthesisSum[localX] += photosynthesisSum[localX + stride];
            respirationSum[localX] += respirationSum[localX + stride];
            oceanTileCount[localX] += oceanTileCount[localX + stride];

        }
    }

    // スレッド0が結果を書き込む
    if (localX == 0)
    {
        statistics1[y * resultCount + groupId.x].temperatureSum = temperatureSum[0];
        statistics1[y * resultCount + groupId.x].oceanDepthSum = oceanDepthSum[0];
        statistics1[y * resultCount + groupId.x].photosynthesisSum = photosynthesisSum[0];
        statistics1[y * resultCount + groupId.x].respirationSum = respirationSum[0];
        statistics1[y * resultCount + groupId.x].oceanTileCount = oceanTileCount[0];
        
    }
}

[numthreads(REDUCTION_SIZE, 1, 1)]
void StatisticsReductRow2(uint2 groupId : SV_GroupID, uint2 groupThreadId : SV_GroupThreadID)
{
    // 横方向に2段階目の統計を取る
    // 1回目で width/REDUCTION_SIZE 個に集約されているので、それをさらに1つにまとめる。

    uint y = groupId.y; // 行インデックス
    uint x = groupThreadId.x; // ワークグループ内のスレッドID
    uint maxX = ceil(size.x * 1.0f / REDUCTION_SIZE);
    
    if (y >= size.y)
    {
        return;
    }
    
    // シェアードメモリにデータを読み込み
    temperatureSum[x] = (x < maxX) ? statistics1[y * maxX + x].temperatureSum : 0.0;
    oceanDepthSum[x] = (x < maxX) ? statistics1[y * maxX + x].oceanDepthSum : 0.0;
    photosynthesisSum[x] = (x < maxX) ? statistics1[y * maxX + x].photosynthesisSum : 0.0;
    respirationSum[x] = (x < maxX) ? statistics1[y * maxX + x].respirationSum : 0.0;
    oceanTileCount[x] = (x < maxX) ? statistics1[y * maxX + x].oceanTileCount : 0;

    // 並列リダクションで行全体の合計を計算
    for (uint stride = REDUCTION_SIZE / 2; stride > 0; stride /= 2)
    {
        // バリア同期
        GroupMemoryBarrierWithGroupSync();

        if (x < stride)
        {
            temperatureSum[x] += temperatureSum[x + stride];
            oceanDepthSum[x] += oceanDepthSum[x + stride];
            photosynthesisSum[x] += photosynthesisSum[x + stride];
            respirationSum[x] += respirationSum[x + stride];
            oceanTileCount[x] += oceanTileCount[x + stride];
        }
    }

    // スレッド0が結果を書き込む
    if (x == 0)
    {
        statistics2[y].temperatureSum = temperatureSum[0];
        statistics2[y].oceanDepthSum = oceanDepthSum[0];
        statistics2[y].photosynthesisSum = photosynthesisSum[0];
        statistics2[y].respirationSum = respirationSum[0];
        statistics2[y].oceanTileCount = oceanTileCount[0];
    }
}


[numthreads(1, REDUCTION_SIZE, 1)]
void StatisticsReductCol1(uint2 groupId : SV_GroupID, uint2 groupThreadId : SV_GroupThreadID)
{
    // 縦方向に1段階目の統計を取る
    // 1グループに付き REDUCTION_SIZE マスを合計するので、結果は height/REDUCTION_SIZE 個作られる。

    uint localY = groupThreadId.y; // ワークグループ内のスレッドID
    uint y = groupId.y * REDUCTION_SIZE + localY;

    // シェアードメモリにデータを読み込み
    temperatureSum[localY] = (y < size.y) ? statistics2[y].temperatureSum : 0.0;
    oceanDepthSum[localY] = (y < size.y) ? statistics2[y].oceanDepthSum : 0.0;
    photosynthesisSum[localY] = (y < size.y) ? statistics2[y].photosynthesisSum : 0.0;
    respirationSum[localY] = (y < size.y) ? statistics2[y].respirationSum : 0.0;
    oceanTileCount[localY] = (y < size.y) ? statistics2[y].oceanTileCount : 0;

    // 並列リダクションで行全体の合計を計算
    for (uint stride = REDUCTION_SIZE / 2; stride > 0; stride /= 2)
    {
        // バリア同期
        GroupMemoryBarrierWithGroupSync();

        if (localY < stride)
        {
            temperatureSum[localY] += temperatureSum[localY + stride];
            oceanDepthSum[localY] += oceanDepthSum[localY + stride];
            photosynthesisSum[localY] += photosynthesisSum[localY + stride];
            respirationSum[localY] += respirationSum[localY + stride];
            oceanTileCount[localY] += oceanTileCount[localY + stride];
        }
    }

    // スレッド0が結果を書き込む
    if (localY == 0)
    {
        statistics3[groupId.y].temperatureSum = temperatureSum[0];
        statistics3[groupId.y].oceanDepthSum = oceanDepthSum[0];
        statistics3[groupId.y].photosynthesisSum = photosynthesisSum[0];
        statistics3[groupId.y].respirationSum = respirationSum[0];
        statistics3[groupId.y].oceanTileCount = oceanTileCount[0];
    }
}

[numthreads(1, REDUCTION_SIZE, 1)]
void StatisticsReductCol2(uint2 groupThreadId : SV_GroupThreadID)
{
    // 縦方向に2段階目の統計を取る
    // 1回目で height/REDUCTION_SIZE 個に集約されているので、それをさらに1つにまとめる。

    uint y = groupThreadId.y;
    
    uint maxY = ceil(size.y * 1.0f / REDUCTION_SIZE);

    // シェアードメモリにデータを読み込み
    temperatureSum[y] = (y < maxY) ? statistics3[y].temperatureSum : 0.0;
    oceanDepthSum[y] = (y < maxY) ? statistics3[y].oceanDepthSum : 0.0;
    photosynthesisSum[y] = (y < maxY) ? statistics3[y].photosynthesisSum : 0.0;
    respirationSum[y] = (y < maxY) ? statistics3[y].respirationSum : 0.0;
    oceanTileCount[y] = (y < maxY) ? statistics3[y].oceanTileCount : 0;

    // 並列リダクションで行全体の合計を計算
    for (uint stride = REDUCTION_SIZE / 2; stride > 0; stride /= 2)
    {
        // バリア同期
        GroupMemoryBarrierWithGroupSync();

        if (y < stride)
        {
            temperatureSum[y] += temperatureSum[y + stride];
            oceanDepthSum[y] += oceanDepthSum[y + stride];
            photosynthesisSum[y] += photosynthesisSum[y + stride];
            respirationSum[y] += respirationSum[y + stride];
            oceanTileCount[y] += oceanTileCount[y + stride];
        }
    }

    // スレッド0が結果を書き込む
    if (y == 0)
    {
        planet.statistics.temperatureSum = temperatureSum[0];
        planet.statistics.oceanDepthSum = oceanDepthSum[0];
        planet.statistics.photosynthesisSum = photosynthesisSum[0];
        planet.statistics.respirationSum = respirationSum[0];
        planet.statistics.oceanTileCount = oceanTileCount[0];
    }
}


[numthreads(256, 1, 1)]
void PopulationClear(uint id : SV_DispatchThreadID)
{
    populations[id.x] = 0;
}

[numthreads(8, 8, LAYER_COUNT)]
void PopulationCollect(uint3 id : SV_DispatchThreadID)
{
    uint xy = id.y * size.x + id.x;
    
    uint layer = id.z;
    
    if (tile.life.species!=0)
    {
        InterlockedAdd(populations[tile.life.species], 1);
    }
}

[numthreads(ENV_STATISTICS_SIZE, 1, 1)]
void EnvStatisticsClear(uint id : SV_DispatchThreadID)
{
    envStatistics[id.x] = 0;
}

[numthreads(8, 8, 1)]
void EnvStatisticsCollectElevation(uint2 id : SV_DispatchThreadID)
{
    uint xy = id.y * size.x + id.x;
    
    int elevation = clamp((tile.elevation - planet.ocean.seaLevel - -10000) / (10000 - -10000) * ENV_STATISTICS_SIZE, 0, ENV_STATISTICS_SIZE-1);
    
    InterlockedAdd(envStatistics[elevation], 1);
}

[numthreads(8, 8, 1)]
void EnvStatisticsCollectTemperature(uint2 id : SV_DispatchThreadID)
{
    uint xy = id.y * size.x + id.x;
    
    int temperature = clamp((tile.temperature - (CelsiusZero - 10)) / (30 - -10) * ENV_STATISTICS_SIZE, 0, ENV_STATISTICS_SIZE-1);

    InterlockedAdd(envStatistics[temperature], 1);
}

[numthreads(8, 8, 1)]
void EnvStatisticsCollectHumidity(uint2 id : SV_DispatchThreadID)
{
    uint xy = id.y * size.x + id.x;
    
    if (tile.elevation >= planet.ocean.seaLevel)
    {
        // 陸地以外は数えても仕方がない
        
        int humidity = clamp(tile.humidity * ENV_STATISTICS_SIZE, 0, ENV_STATISTICS_SIZE-1);
    
        InterlockedAdd(envStatistics[humidity], 1);
    }
}
