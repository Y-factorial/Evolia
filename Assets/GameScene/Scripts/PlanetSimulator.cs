using Evolia.Model;
using Evolia.Shared;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Evolia.GameScene
{

    [DefaultExecutionOrder(ExecutionOrder.SIMULATOR)]
    public class PlanetSimulator : MonoBehaviour
    {

        [SerializeField]
        public ComputeShader computeShader;

        [SerializeField]
        public UnityEvent OnTick;

        public GameSpeed speed = GameSpeed.Normal;

        private CommandBuffer commandBuffer;

        private ComputeBuffer planetBuffer;
        private ComputeBuffer rowsBuffer;
        private ComputeBuffer colBuffer;
        private ComputeBuffer variantsBuffer;
        private ComputeBuffer tilesBuffer;
        private ComputeBuffer populationBuffer;
        private ComputeBuffer updownsBuffer;

        public const int REDUCTION_SIZE = 64;
        private ComputeBuffer statisticsBuffer1; // 512x64
        private ComputeBuffer statisticsBuffer2; // 512x1
        private ComputeBuffer statisticsBuffer3; // 64x1

        public const int ENV_STATISTICS_SIZE = 64;
        private ComputeBuffer envStatisticsBuffer;

        private ComputeBuffer commandRxyBuffer;
        private ComputeBuffer commandLifeBuffer;
        private ComputeBuffer tileDetailsBuffer;

        private RenderTexture surfaceTexture;

        private RenderTexture lifeTexture;

        private RenderTexture overlayTexture;


        public class NamedKernel
        {
            ComputeShader shader;
            string name;
            int handle;
            int threadGroupsX;
            int threadGroupsY;
            int threadGroupsZ;

            public NamedKernel(ComputeShader shader, string name, int threadGroupX, int threadGroupY, int threadGroupZ)
            {
                this.shader = shader;
                this.name = name;
                this.handle = shader.FindKernel(name);
                this.threadGroupsX = threadGroupX;
                this.threadGroupsY = threadGroupY;
                this.threadGroupsZ = threadGroupZ;
            }

            public void SetBuffer(string name, ComputeBuffer buffer)
            {
                shader.SetBuffer(handle, name, buffer);
            }
            public void SetTexture(string name, RenderTexture texture)
            {
                shader.SetTexture(handle, name, texture);
            }

            public void Dispatch(CommandBuffer commandBuffer)
            {
                commandBuffer.BeginSample(name);
                commandBuffer.DispatchCompute(shader, handle, threadGroupsX, threadGroupsY, threadGroupsZ);
                commandBuffer.EndSample(name);
            }

            public string Name => name;
        }

        private List<NamedKernel> calculationKernels = new();
        private Dictionary<string, NamedKernel> allKernels = new();

        public OverlayMode overlayMode = OverlayMode.None;
        public Variant happinessVariant;
        public int happinessSpecies;

        public bool headless = false;
        public bool showLife = true;

        public Planet planet => GameController.planet;

        public void Awake()
        {
            PrepareBuffer();

            PrepareTexture();
            InitKernel();

        }

        private void PrepareBuffer()
        {
            commandBuffer = new CommandBuffer() { name = "Simulator" };

            planetBuffer = new ComputeBuffer(1, UnsafeUtility.SizeOf<PlanetData>());
            planetBuffer.SetData(new PlanetData[] { planet.data });

            rowsBuffer = new ComputeBuffer(planet.size.x, UnsafeUtility.SizeOf<Row>());

            colBuffer = new ComputeBuffer(CoL.instance.species.Length, UnsafeUtility.SizeOf<Species>());
            colBuffer.SetData(CoL.instance.species);

            variantsBuffer = new ComputeBuffer(VariantData.AllData.Length, UnsafeUtility.SizeOf<VariantData>());
            variantsBuffer.SetData(VariantData.AllData);

            tilesBuffer = new ComputeBuffer(planet.size.x * planet.size.y, UnsafeUtility.SizeOf<Tile>());
            tilesBuffer.SetData(planet.tiles);

            updownsBuffer = new ComputeBuffer(planet.size.x * planet.size.y, sizeof(float));
            updownsBuffer.SetData(planet.updowns);

            populationBuffer = new ComputeBuffer(CoL.MAX_SPECIES, sizeof(uint));

            statisticsBuffer1 = new ComputeBuffer(Mathf.CeilToInt(planet.size.x * 1.0f / REDUCTION_SIZE) * planet.size.y, UnsafeUtility.SizeOf<Statistics>()); // 512x64
            statisticsBuffer2 = new ComputeBuffer(1 * planet.size.y, UnsafeUtility.SizeOf<Statistics>()); // 512x1
            statisticsBuffer3 = new ComputeBuffer(1 * Mathf.CeilToInt(planet.size.y * 1.0f / REDUCTION_SIZE), UnsafeUtility.SizeOf<Statistics>()); // 64x1
            envStatisticsBuffer = new ComputeBuffer(ENV_STATISTICS_SIZE, sizeof(uint));

            commandRxyBuffer = new ComputeBuffer(2, sizeof(uint));
            commandLifeBuffer = new ComputeBuffer(1, UnsafeUtility.SizeOf <Life>());
            tileDetailsBuffer = new ComputeBuffer(1, UnsafeUtility.SizeOf<TileDetails>());

        }

        public void Start()
        {
            StartCoroutine(CalcThread());
        }

        public void OnDestroy()
        {
            commandBuffer?.Dispose();

            planetBuffer?.Release();
            rowsBuffer?.Release();
            colBuffer?.Release();
            variantsBuffer?.Release();
            tilesBuffer?.Release();
            populationBuffer?.Release();
            updownsBuffer?.Release();
            envStatisticsBuffer?.Release();
            commandRxyBuffer?.Release();
            commandLifeBuffer?.Release();
            tileDetailsBuffer?.Release();

            statisticsBuffer1?.Release();
            statisticsBuffer2?.Release();
            statisticsBuffer3?.Release();

            surfaceTexture?.Release();
            lifeTexture?.Release();
            overlayTexture?.Release();
        }

        private void PrepareTexture()
        {
            int w = planet.size.x; // NextPowerOf(planet.size.x, 2);
            int h = planet.size.y;// NextPowerOf(planet.size.y, 2);

            surfaceTexture = new RenderTexture(w, h, 0, GraphicsFormat.R16G16B16A16_UNorm);
            surfaceTexture.enableRandomWrite = true;
            surfaceTexture.filterMode = FilterMode.Point;
            surfaceTexture.wrapMode = TextureWrapMode.Repeat;
            surfaceTexture.Create();

            lifeTexture = new RenderTexture(w, h, 0, GraphicsFormat.R8G8B8A8_UNorm);
            lifeTexture.enableRandomWrite = true;
            lifeTexture.filterMode = FilterMode.Point;
            lifeTexture.wrapMode = TextureWrapMode.Repeat;
            lifeTexture.Create();

            overlayTexture = new RenderTexture(w, h, 0, GraphicsFormat.R8G8B8A8_UNorm);
            overlayTexture.enableRandomWrite = true;
            overlayTexture.filterMode = FilterMode.Point;
            overlayTexture.wrapMode = TextureWrapMode.Repeat;
            overlayTexture.Create();
        }

        private void InitKernel()
        {
            calculationKernels.Clear();
            calculationKernels.Add(new NamedKernel(computeShader, "Begin", 1, 1, 1));
            calculationKernels.Add(new NamedKernel(computeShader, "BeginRow", 1, planet.size.y / 8, 1));
            
            calculationKernels.Add(new NamedKernel(computeShader, "EnvCalcDiff", planet.size.x / 8, planet.size.y / 8, 1));
            calculationKernels.Add(new NamedKernel(computeShader, "EnvApplyDiff", planet.size.x / 8, planet.size.y / 8, 1));
            calculationKernels.Add(new NamedKernel(computeShader, "EnvLifeNutrients", planet.size.x / 8, planet.size.y / 8, 1));
            calculationKernels.Add(new NamedKernel(computeShader, "EnvSumNutrients", planet.size.x / 8, planet.size.y / 8, 1));

            calculationKernels.Add(new NamedKernel(computeShader, "LifeGrow", planet.size.x / 8, planet.size.y / 8, 1));
            calculationKernels.Add(new NamedKernel(computeShader, "LifeReproduce", planet.size.x / 8, planet.size.y / 8, 1));

            calculationKernels.Add(new NamedKernel(computeShader, "PopulationClear", 1, 1, 1));
            calculationKernels.Add(new NamedKernel(computeShader, "PopulationCollect", planet.size.x / 8, planet.size.y / 8, 1));

            calculationKernels.Add(new NamedKernel(computeShader, "StatisticsReductRow1", Mathf.CeilToInt(planet.size.x * 1.0f / REDUCTION_SIZE), planet.size.y, 1));
            calculationKernels.Add(new NamedKernel(computeShader, "StatisticsReductRow2", 1, planet.size.y, 1));
            calculationKernels.Add(new NamedKernel(computeShader, "StatisticsReductCol1", 1, Mathf.CeilToInt(planet.size.y * 1.0f / REDUCTION_SIZE), 1));
            calculationKernels.Add(new NamedKernel(computeShader, "StatisticsReductCol2", 1, 1, 1));

            calculationKernels.Add(new NamedKernel(computeShader, "EndRow", 1, planet.size.y / 8, 1));
            calculationKernels.Add(new NamedKernel(computeShader, "End", 1, 1, 1));

            allKernels.Clear();
            foreach (var kernel in calculationKernels)
            {
                allKernels.Add(kernel.Name, kernel);
            }

            List<NamedKernel> commandKernels = new();

            commandKernels.Add(new NamedKernel(computeShader, "EnvStatisticsClear", 1, 1, 1));
            commandKernels.Add(new NamedKernel(computeShader, "EnvStatisticsCollectElevation", planet.size.x / 8, planet.size.y / 8, 1));
            commandKernels.Add(new NamedKernel(computeShader, "EnvStatisticsCollectTemperature", planet.size.x / 8, planet.size.y / 8, 1));
            commandKernels.Add(new NamedKernel(computeShader, "EnvStatisticsCollectHumidity", planet.size.x / 8, planet.size.y / 8, 1));

            commandKernels.Add(new NamedKernel(computeShader, "CommandGetCenterH", 1, 1, 1));
            commandKernels.Add(new NamedKernel(computeShader, "CommandMeteor", planet.size.x / 8, planet.size.y / 8, 1));
            commandKernels.Add(new NamedKernel(computeShader, "CommandVolcano", planet.size.x / 8, planet.size.y / 8, 1));
            commandKernels.Add(new NamedKernel(computeShader, "CommandCollapse", planet.size.x / 8, planet.size.y / 8, 1));
            commandKernels.Add(new NamedKernel(computeShader, "CommandCosmicRay", 1, 1, 1));
            commandKernels.Add(new NamedKernel(computeShader, "CommandAbductLife", 1, 1, 1));
            commandKernels.Add(new NamedKernel(computeShader, "CommandIntroduceLife", 1, 1, 1));
            commandKernels.Add(new NamedKernel(computeShader, "CommandSearchLife", planet.size.x / 8, planet.size.y / 8, 1));
            commandKernels.Add(new NamedKernel(computeShader, "CommandGetTileDetails", 1, 1, 1));

            commandKernels.Add(new NamedKernel(computeShader, "RenderWorld", planet.size.x / 8, planet.size.y / 8, 1));
            commandKernels.Add(new NamedKernel(computeShader, "RenderLife", planet.size.x / 8, planet.size.y / 8, 1));
            commandKernels.Add(new NamedKernel(computeShader, "RenderWind", planet.size.x / 8, planet.size.y / 8, 1));
            commandKernels.Add(new NamedKernel(computeShader, "RenderUpDown", planet.size.x / 8, planet.size.y / 8, 1));
            commandKernels.Add(new NamedKernel(computeShader, "RenderTemperature", planet.size.x / 8, planet.size.y / 8, 1));
            commandKernels.Add(new NamedKernel(computeShader, "RenderHumidity", planet.size.x / 8, planet.size.y / 8, 1));
            commandKernels.Add(new NamedKernel(computeShader, "RenderHappiness", planet.size.x / 8, planet.size.y / 8, 1));

            foreach (var kernel in commandKernels)
            {
                allKernels.Add(kernel.Name, kernel);
            }

            computeShader.SetInts("size", planet.size.x, planet.size.y);

            foreach (var kernel in allKernels.Values)
            {
                kernel.SetBuffer("planets", planetBuffer);
                kernel.SetBuffer("rows", rowsBuffer);
                kernel.SetBuffer("tiles", tilesBuffer);
                kernel.SetBuffer("updowns", updownsBuffer);
                kernel.SetBuffer("col", colBuffer);
                kernel.SetBuffer("variants", variantsBuffer);
                kernel.SetBuffer("populations", populationBuffer);
                kernel.SetBuffer("statistics1", statisticsBuffer1);
                kernel.SetBuffer("statistics2", statisticsBuffer2);
                kernel.SetBuffer("statistics3", statisticsBuffer3);
                kernel.SetBuffer("envStatistics", envStatisticsBuffer);
                kernel.SetBuffer("commandRxy", commandRxyBuffer);
                kernel.SetBuffer("commandLife", commandLifeBuffer);
                kernel.SetBuffer("tileDetails", tileDetailsBuffer);
                kernel.SetTexture("surfaceTexture", surfaceTexture);
                kernel.SetTexture("lifeTexture", lifeTexture);
                kernel.SetTexture("overlayTexture", overlayTexture);

            }
        }


        public IEnumerator<object> CalcThread()
        {
            yield return null;

            while (this != null)
            {
                float nextTiime = CalcAndRender();

                float now = (float)(Time.timeAsDouble % (3600 * 24));

                yield return new WaitForSeconds(Mathf.Max(0, nextTiime - now));
            }
        }

        private float CalcAndRender()
        {
            commandBuffer.Clear();

            float nextTiime = (float)(Time.timeAsDouble % (3600 * 24)) + speed.WaitTime();
            for (int i = 0; i < speed.LoopCount(); ++i)
            {
                EnqueueCalc();
            }

            //EnqueueReadData(speed.LoopCount());

            if (!headless)
            {
                EnqueueRender();
            }

            Graphics.ExecuteCommandBuffer(commandBuffer);

            return nextTiime;
        }

        int randomCount(float rate)
        {
            return (int)rate + (planet.random.NextDouble() < (rate - MathF.Floor(rate)) ? 1 : 0);

        }

        System.Random random = new System.Random();

        private void EnqueueCalc()
        {
            int deltaTick = 1;

            commandBuffer.SetComputeFloatParam(computeShader, "deltaTick", deltaTick);
            commandBuffer.SetComputeFloatParam(computeShader, "age", planet.age);
            this.commandBuffer.SetComputeFloatParam(this.computeShader, "geologicalSpeed", planet.timescale.AgeSpeed() * planet.timescale.GeologicalSpeedFactor());
            this.commandBuffer.SetComputeFloatParam(this.computeShader, "metabolismSpeed", planet.timescale.AgeSpeed() * planet.timescale.MetabolismSpeedFactor());
            this.commandBuffer.SetComputeFloatParam(this.computeShader, "heatSpeed", planet.timescale.HeatSpeed());
            commandBuffer.SetComputeIntParam(computeShader, "seed", random.Next(24 * 3600));

            {
                // 隕石の衝突イベント
                // 1億年に1回とする。これで30億年に30個隕石が落ちる
                int meteorCount = randomCount( 3 * deltaTick * planet.timescale.AgeSpeed() / 1000000000.0f);
                for (int i = 0; i < meteorCount; ++i)
                {
                    EnqueueMeteor(planet.random.Next(planet.size.x), planet.random.Next(planet.size.y));
                }
            }

            // 宇宙線による突然変異
            {
                // ゲームの進行速度を決定するのでかなり重要
                int rayCount = randomCount(10.0f * deltaTick);
                for (int i = 0; i < rayCount; ++i)
                {
                    EnqueueCosmicRay(planet.random.Next(planet.size.x), planet.random.Next(planet.size.y));
                }
            }

            foreach (var kernel in calculationKernels)
            {
                kernel.Dispatch(commandBuffer);
            }

            planet.ageToNextDiastrophism -= (long)(planet.timescale.AgeSpeed() * planet.timescale.GeologicalSpeedFactor());

            // 地殻変動
            if (planet.ageToNextDiastrophism <= 0)
            {
                planet.Diastrophism();
                commandBuffer.SetBufferData(updownsBuffer, planet.updowns);
            }

            commandBuffer.RequestAsyncReadback(populationBuffer, (data) => {
                if (data.done)
                {
                    if (this != null)
                    {
                        data.GetData<uint>().CopyTo(planet.populations);
                    }
                }
            });
            commandBuffer.RequestAsyncReadback(planetBuffer, (data) =>
            {
                if (data.done)
                {
                    if (this != null)
                    {
                        planet.data = data.GetData<PlanetData>()[0];

                        planet.tick += deltaTick;
                        planet.age += planet.timescale.AgeSpeed();

                        planet.OnTick(deltaTick);
                    }

                }
            });
        }


        public void RenderNow()
        {
            commandBuffer.Clear();

            EnqueueRender();

            Graphics.ExecuteCommandBuffer(commandBuffer);

        }

        private void EnqueueRender()
        {
            commandBuffer.SetComputeFloatParam(computeShader, "microbeScale", planet.MicrobeScale());

            allKernels["RenderWorld"].Dispatch(commandBuffer);
            if (showLife)
            {
                allKernels["RenderLife"].Dispatch(commandBuffer);
            }

            if (overlayMode != OverlayMode.None)
            {
                commandBuffer.SetComputeIntParam(computeShader, "renderingSpecies", happinessSpecies);
                commandBuffer.SetComputeIntParam(computeShader, "renderingVariant", (int)happinessVariant);

                allKernels[$"Render{overlayMode}"].Dispatch(commandBuffer);
            }

            commandBuffer.RequestAsyncReadback(planetBuffer, (data) =>
            {
                if (data.done)
                {
                    if (this != null)
                    {
                        planet.data = data.GetData<PlanetData>()[0];

                        OnTick?.Invoke();
                    }
                }
            });
        }

        public void ReadTileDetails(Vector2Int pos, Action<TileDetails> callback)
        {
            commandBuffer.Clear();

            commandBuffer.SetComputeIntParams(computeShader, "commandPos", pos.x, pos.y);
            allKernels["CommandGetTileDetails"].Dispatch(commandBuffer);

            commandBuffer.RequestAsyncReadback(tileDetailsBuffer, (data) =>
            {
                if (data.done)
                {
                    if (this != null)
                    {
                        callback?.Invoke(data.GetData<TileDetails>()[0]);
                    }
                }
            });

            Graphics.ExecuteCommandBuffer(commandBuffer);
        }

        public void ReadEnvStatistics(uint[] result, EnvStatisticsMode mode, Action callback)
        {
            commandBuffer.Clear();

            allKernels["EnvStatisticsClear"].Dispatch(commandBuffer);
            allKernels[$"EnvStatisticsCollect{mode}"].Dispatch(commandBuffer);

            commandBuffer.RequestAsyncReadback(envStatisticsBuffer, (data) =>
            {
                if (data.done)
                {
                    if (this != null)
                    {
                        data.GetData<uint>().CopyTo(result);
                        callback?.Invoke();
                    }
                }
            });


            Graphics.ExecuteCommandBuffer(commandBuffer);

        }

        public void WriteTile(int x, int y)
        {
            tilesBuffer.SetData(planet.tiles, x + y * planet.size.x, x+y*planet.size.x, 1);
        }

        public void ReadTiles(Action callback)
        {
            if (this != null)
            {
                AsyncGPUReadback.Request(tilesBuffer, (data) =>
                {
                    if (data.done)
                    {
                        if (this != null)
                        {
                            data.GetData<Tile>().CopyTo(planet.tiles);

                            callback?.Invoke();
                        }
                    }
                }
                );
            }
        }

        public void EventMeteor(Vector2Int focus)
        {
            commandBuffer.Clear();

            EnqueueMeteor(focus.x, focus.y);

            EnqueueRender();

            Graphics.ExecuteCommandBuffer(commandBuffer);

        }

        private void EnqueueMeteor( int x, int y)
        {
            // 隕石の大きさと座標
            // 大きいほど確率が低い
            float maxSize = planet.size.y / 4;
            float minSize = 5;
            commandBuffer.SetComputeIntParams(computeShader, "commandPos", x, y);
            commandBuffer.SetComputeFloatParam(computeShader, "commandR", Mathf.Lerp(minSize, maxSize, Mathf.Pow((float)planet.random.NextDouble(), 4)));
            allKernels["CommandGetCenterH"].Dispatch(commandBuffer);
            allKernels["CommandMeteor"].Dispatch(commandBuffer);
        }


        public void EventVolcano(Vector2Int focus)
        {
            commandBuffer.Clear();

            EnqueueVolcano(focus.x, focus.y);

            EnqueueRender();

            Graphics.ExecuteCommandBuffer(commandBuffer);
        }

        private void EnqueueVolcano(int x, int y)
        {
            // 火山の大きさと座標
            // 大きいほど確率が低い
            float maxSize = 20;
            float minSize = 10;
            commandBuffer.SetComputeIntParams(computeShader, "commandPos", x, y);
            commandBuffer.SetComputeFloatParam(computeShader, "commandR", Mathf.Lerp(minSize, maxSize, Mathf.Pow((float)planet.random.NextDouble(), 2)));
            commandBuffer.SetComputeFloatParam(computeShader, "commandH", Mathf.Lerp(4000, 6000, (float)planet.random.NextDouble()));
            allKernels["CommandGetCenterH"].Dispatch(commandBuffer);
            allKernels["CommandVolcano"].Dispatch(commandBuffer);
        }

        public void EventCosmicRay(Vector2Int focus)
        {
            commandBuffer.Clear();

            EnqueueCosmicRay(focus.x, focus.y);

            EnqueueRender();

            Graphics.ExecuteCommandBuffer(commandBuffer);
        }

        private void EnqueueCosmicRay(int x, int y)
        {
            commandBuffer.SetComputeIntParams(computeShader, "commandPos", x, y);
            allKernels["CommandCosmicRay"].Dispatch(commandBuffer);
        }

        public void EventCollapse(Vector2Int focus)
        {
            commandBuffer.Clear();

            EnqueueCollapse(focus.x, focus.y);

            EnqueueRender();

            Graphics.ExecuteCommandBuffer(commandBuffer);
        }

        private void EnqueueCollapse(int x, int y)
        {
            // 陥没の大きさと座標
            // 大きいほど確率が低い
            float maxSize = 20;
            float minSize = 10;
            commandBuffer.SetComputeIntParams(computeShader, "commandPos", x, y);
            commandBuffer.SetComputeFloatParam(computeShader, "commandR", Mathf.Lerp(minSize, maxSize, Mathf.Pow((float)planet.random.NextDouble(), 2)));
            commandBuffer.SetComputeFloatParam(computeShader, "commandH", Mathf.Lerp(-4000, -6000, (float)planet.random.NextDouble()));
            allKernels["CommandGetCenterH"].Dispatch(commandBuffer);
            allKernels["CommandCollapse"].Dispatch(commandBuffer);
        }

        public void AbductLife(Vector2Int focus, Action<Life> callback)
        {
            commandBuffer.Clear();

            commandBuffer.SetComputeIntParams(computeShader, "commandPos", focus.x, focus.y);
            allKernels["CommandAbductLife"].Dispatch(commandBuffer);

            commandBuffer.RequestAsyncReadback(commandLifeBuffer, (data) =>
            {
                if (data.done)
                {
                    if (this != null)
                    {
                        callback?.Invoke(data.GetData<Life>()[0]);
                    }
                }
            });


            EnqueueRender();

            Graphics.ExecuteCommandBuffer(commandBuffer);
        }

        public void IntroduceLife(Vector2Int focus, Life life)
        {
            commandBuffer.Clear();

            commandBuffer.SetComputeIntParams(computeShader, "commandPos", focus.x, focus.y);
            commandBuffer.SetBufferData<Life>(commandLifeBuffer, new NativeArray<Life>(new Life[] { life }, Allocator.Temp));
            allKernels["CommandIntroduceLife"].Dispatch(commandBuffer);


            EnqueueRender();

            Graphics.ExecuteCommandBuffer(commandBuffer);
        }

        public void FindLife(in Species spec, Vector2Int focus, Action<Vector2Int> foundCallback, Action notFoundCallback = null)
        {
            commandBuffer.Clear();

            commandBuffer.SetBufferData(commandRxyBuffer, new int[] { int.MaxValue, 0 });
            commandBuffer.SetComputeIntParams(computeShader, "commandPos", focus.x, focus.y);
            commandBuffer.SetComputeFloatParam(computeShader, "commandSpecies", spec.id);
            allKernels["CommandSearchLife"].Dispatch(commandBuffer);

            commandBuffer.RequestAsyncReadback(commandRxyBuffer, (data) =>
            {
                if (data.done)
                {
                    if (this != null)
                    {
                        int[] result = new int[2];
                        data.GetData<int>().CopyTo(result);
                        if (result[0] != int.MaxValue)
                        {
                            int y = (int)(result[1] / planet.size.x);
                            int x = (int)(result[1] % planet.size.x);

                            foundCallback?.Invoke(new Vector2Int(x, y));
                        }
                        else
                        {
                            // 見つからなかった
                            notFoundCallback?.Invoke();
                        }
                    }
                }
            });


            Graphics.ExecuteCommandBuffer(commandBuffer);
        }

        public RenderTexture SurfaceTexture => surfaceTexture;
        public RenderTexture LifeTexture => lifeTexture;

        public RenderTexture OverlayTexture => overlayTexture;
    }
}