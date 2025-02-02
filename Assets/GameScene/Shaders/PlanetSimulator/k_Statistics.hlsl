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
    // ��������1�i�K�ڂ̓��v�����
    // 1�O���[�v�ɕt�� REDUCTION_SIZE �}�X�����v����̂ŁA���ʂ� width/REDUCTION_SIZE �����B
    uint resultCount = ceil(size.x * 1.0f / REDUCTION_SIZE);

    uint localX = groupThreadId.x; // ���[�N�O���[�v���̃X���b�hID
    uint y = groupId.y; // �s�C���f�b�N�X�A�S�s�ʂ̃O���[�v�Ƃ��ē����̂ŁA�O���[�vID���s�C���f�b�N�X�ɂȂ�
    uint x = groupId.x * REDUCTION_SIZE + localX; // �����O���[�v�ɕ�����ē����̂ŁA�O���[�v���̃X���b�hID�����Z���ăO���[�o���C���f�b�N�X���v�Z

    if (y >= size.y)
    {
        return;
    }
    
    // �O���[�o���C���f�b�N�X�v�Z
    uint xy = y * size.x + x;
    
    // �V�F�A�[�h�������Ƀf�[�^��ǂݍ���
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

    // ���񃊃_�N�V�����ōs�S�̂̍��v���v�Z
    for (uint stride = REDUCTION_SIZE / 2; stride > 0; stride /= 2)
    {
        // �o���A����
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

    // �X���b�h0�����ʂ���������
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
    // ��������2�i�K�ڂ̓��v�����
    // 1��ڂ� width/REDUCTION_SIZE �ɏW�񂳂�Ă���̂ŁA����������1�ɂ܂Ƃ߂�B

    uint y = groupId.y; // �s�C���f�b�N�X
    uint x = groupThreadId.x; // ���[�N�O���[�v���̃X���b�hID
    uint maxX = ceil(size.x * 1.0f / REDUCTION_SIZE);
    
    if (y >= size.y)
    {
        return;
    }
    
    // �V�F�A�[�h�������Ƀf�[�^��ǂݍ���
    temperatureSum[x] = (x < maxX) ? statistics1[y * maxX + x].temperatureSum : 0.0;
    oceanDepthSum[x] = (x < maxX) ? statistics1[y * maxX + x].oceanDepthSum : 0.0;
    photosynthesisSum[x] = (x < maxX) ? statistics1[y * maxX + x].photosynthesisSum : 0.0;
    respirationSum[x] = (x < maxX) ? statistics1[y * maxX + x].respirationSum : 0.0;
    oceanTileCount[x] = (x < maxX) ? statistics1[y * maxX + x].oceanTileCount : 0;

    // ���񃊃_�N�V�����ōs�S�̂̍��v���v�Z
    for (uint stride = REDUCTION_SIZE / 2; stride > 0; stride /= 2)
    {
        // �o���A����
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

    // �X���b�h0�����ʂ���������
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
    // �c������1�i�K�ڂ̓��v�����
    // 1�O���[�v�ɕt�� REDUCTION_SIZE �}�X�����v����̂ŁA���ʂ� height/REDUCTION_SIZE �����B

    uint localY = groupThreadId.y; // ���[�N�O���[�v���̃X���b�hID
    uint y = groupId.y * REDUCTION_SIZE + localY;

    // �V�F�A�[�h�������Ƀf�[�^��ǂݍ���
    temperatureSum[localY] = (y < size.y) ? statistics2[y].temperatureSum : 0.0;
    oceanDepthSum[localY] = (y < size.y) ? statistics2[y].oceanDepthSum : 0.0;
    photosynthesisSum[localY] = (y < size.y) ? statistics2[y].photosynthesisSum : 0.0;
    respirationSum[localY] = (y < size.y) ? statistics2[y].respirationSum : 0.0;
    oceanTileCount[localY] = (y < size.y) ? statistics2[y].oceanTileCount : 0;

    // ���񃊃_�N�V�����ōs�S�̂̍��v���v�Z
    for (uint stride = REDUCTION_SIZE / 2; stride > 0; stride /= 2)
    {
        // �o���A����
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

    // �X���b�h0�����ʂ���������
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
    // �c������2�i�K�ڂ̓��v�����
    // 1��ڂ� height/REDUCTION_SIZE �ɏW�񂳂�Ă���̂ŁA����������1�ɂ܂Ƃ߂�B

    uint y = groupThreadId.y;
    
    uint maxY = ceil(size.y * 1.0f / REDUCTION_SIZE);

    // �V�F�A�[�h�������Ƀf�[�^��ǂݍ���
    temperatureSum[y] = (y < maxY) ? statistics3[y].temperatureSum : 0.0;
    oceanDepthSum[y] = (y < maxY) ? statistics3[y].oceanDepthSum : 0.0;
    photosynthesisSum[y] = (y < maxY) ? statistics3[y].photosynthesisSum : 0.0;
    respirationSum[y] = (y < maxY) ? statistics3[y].respirationSum : 0.0;
    oceanTileCount[y] = (y < maxY) ? statistics3[y].oceanTileCount : 0;

    // ���񃊃_�N�V�����ōs�S�̂̍��v���v�Z
    for (uint stride = REDUCTION_SIZE / 2; stride > 0; stride /= 2)
    {
        // �o���A����
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

    // �X���b�h0�����ʂ���������
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
        // ���n�ȊO�͐����Ă��d�����Ȃ�
        
        int humidity = clamp(tile.humidity * ENV_STATISTICS_SIZE, 0, ENV_STATISTICS_SIZE-1);
    
        InterlockedAdd(envStatistics[humidity], 1);
    }
}
