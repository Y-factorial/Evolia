using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using Evolia.Model.Algorithm;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.Threading;
using Evolia.GameScene;
using Evolia.Shared;
using System.Linq;

namespace Evolia.Model
{


    [Serializable]
    public class Planet
    {
        public static string SaveFile(int id) => $"{Application.persistentDataPath}/planet_{id:0000}.json";

        public static string ThumbnailFile(int id) => $"{Application.persistentDataPath}/planet_{id:0000}_world.png";

        [SerializeField]
        public int id;

        [SerializeField]
        public string name = "Earth";

        [SerializeField]
        public PlanetData data;

        public ref Atmosphere atmosphere => ref data.atmosphere;

        public ref Orbit orbit => ref data.orbit;

        public ref Ocean ocean => ref data.ocean;

        public ref Statistics statistics => ref data.statistics;

        [SerializeField]
        public System.Random random = new System.Random();

        [SerializeField]
        public GeologicalTimescale timescale;

        [SerializeField]
        public long age;

        [SerializeField]
        public long tick;

        [SerializeField]
        public long ageToNextDiastrophism;

        [SerializeField]
        public Vector2Int size;

        [SerializeField]
        public Tile[] tiles;

        [SerializeField]
        public History history = new();

        [SerializeField]
        public float[] updowns;

        [SerializeField]
        public List<LogEntry> log = new List<LogEntry>();

        [SerializeField]
        public uint[] populations = new uint[CoL.MAX_SPECIES];

        [SerializeField]
        public float[] evolutionScores = new float[CoL.MAX_SPECIES];

        public static Planet Load(int id)
        {
            string saveFile = SaveFile(id);
            Debug.Log($"[Planet] Load {saveFile}");

            // Does it exist?
            if (File.Exists(saveFile))
            {
                string fileContents = File.ReadAllText(saveFile);
                return JsonUtility.FromJson<Planet>(fileContents);
            }
            else
            {
                Planet planet = new();
                planet.id = id;
                return planet;
            }
        }

        public static Texture2D LoadThumbnail(int id)
        {
            string thumbnailFile = ThumbnailFile(id);

            Debug.Log($"[Planet] LoadThumbnail {thumbnailFile}");

            if (File.Exists(thumbnailFile))
            {
                byte[] bytes = File.ReadAllBytes(thumbnailFile);
                Texture2D texture = new Texture2D(2, 2); // 初期サイズは仮
                texture.LoadImage(bytes);

                return texture;
            }
            else
            {
                return null;
            }
        }

        public void AsyncSave(RawImage minimap, Action callback)
        {
            string saveFile = SaveFile(this.id);
            string thumbnailFile = ThumbnailFile(this.id);

            Debug.Log($"[Planet] AsyncSave {saveFile} {thumbnailFile}");

            // 測ってないけどたぶん高速
            Universe universe = Universe.Load();
            Universe.PlanetInfo info = universe.GetPlanet(id);
            info.age = age;
            info.lastPlayedAt = DateTime.Now.Ticks;
            universe.lastPlayedPlanetId = id;
            universe.Save();

            // 十分高速
            byte[] bytes = TextureToPng(minimap);

            SynchronizationContext context = SynchronizationContext.Current;

            Task.Run(() =>
            {
                // 1.5 sec
                string fileContents = JsonUtility.ToJson(this, true);

                // 0.3 sec
                File.WriteAllText(saveFile, fileContents);

                // 十分高速
                File.WriteAllBytes(thumbnailFile, bytes);


                context.Post(_ => { 
                    callback?.Invoke();
                }, null);
            });
        }

        private byte[] TextureToPng(RawImage minimap)
        {
            RenderTexture currentRT = RenderTexture.active;

            RenderTexture renderedTexture = new RenderTexture(minimap.texture.width, minimap.texture.height, 0);

            Graphics.Blit(minimap.texture, renderedTexture, minimap.material);

            // RenderTextureからTexture2Dを作成
            Texture2D texture2D = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);

            // RenderTextureの内容をTexture2Dにコピー
            RenderTexture.active = renderedTexture;
            texture2D.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
            texture2D.Apply();

            // Texture2DをPNGにエンコード
            byte[] bytes = texture2D.EncodeToPNG();
            GameObject.Destroy(texture2D);

            RenderTexture.active = currentRT;
            renderedTexture.Release();

            return bytes;
        }

        public void Init(float scale, float solarLuminosityFactor, float solarDistanceFactor, float axisTilt, float waterFactor)
        {
            orbit.solarLuminosity = solarLuminosityFactor * EarthConstants.SolarLuminosity;
            orbit.solarDistance = solarDistanceFactor * EarthConstants.SolarDistance;
            orbit.radius = scale * EarthConstants.Radius;
            orbit.axisTilt = axisTilt;
            orbit.gravity = EarthConstants.Gravity;

            // H2O はすべて大気中にあった
            atmosphere.h2oMass = waterFactor * EarthConstants.WaterMass * scale * scale;

            // CO2 は10気圧程度あった
            atmosphere.co2Mass = (EarthConstants.O2Mass + EarthConstants.N2Mass) * 10 * scale * scale;

            // N2 は特に面白いこともないので、地球の大気圧と同じくらいにしておく
            atmosphere.n2Mass = EarthConstants.N2Mass * scale * scale;

            // O2 はまだ存在しなかった
            atmosphere.o2Mass = 0;

            // 海はまだ存在しなかった
            ocean.seaLevel = EarthConstants.MarianaTrenchAltitude;
            ocean.oceanMass = 0;
            ocean.mineral = atmosphere.co2Mass * 32/44 - EarthConstants.O2Mass * scale * scale;
            ocean.oxydizedMineral = 0;

            size.y = (int)(EarthConstants.Height * scale) / 8 * 8; // 8の倍数にしておく
            size.x = size.y * 2;
            InitGeometry();

            // 地質年代
            timescale = GeologicalTimescale.Hadean;
        }


        private void InitGeometry()
        {
            tiles = new Tile[size.x * size.y];

            GeographicalRandom grand = new GeographicalRandom(new float[size.x, size.y], random, 20 + random.Next(80));
            grand.Next(EarthConstants.MarianaTrenchAltitude, EarthConstants.EverestAltitude);

            for (int x = 0; x < size.x; ++x)
            {
                int oy = (int)(Mathf.PerlinNoise1D(x / 10.0f) * 3);

                for (int y = 0; y < size.y; ++y)
                {
                    int ox = (int)(Mathf.PerlinNoise1D(1000 + y / 10.0f) * 3);

                    tiles[x + y * size.x].elevation = grand[ (x+ox+size.x)%size.x, (y+oy+size.y)%size.y];
                    tiles[x + y * size.x].temperature = 2000 - grand[(x + ox + size.x) % size.x, (y + oy + size.y) % size.y] / 10;

                    ocean.seaLevel = Math.Min(ocean.seaLevel, tiles[x + y * size.x].elevation);
                }
            }

            updowns = new float[size.x * size.y];

            Diastrophism();
        }

        public void Diastrophism()
        {
            GeographicalRandom grand = new GeographicalRandom(new float[size.x, size.y], random, 20+random.Next(80));
            grand.Next(-1, 1);

            for (int x = 0; x < size.x; ++x)
            {
                int oy = (int)(Mathf.PerlinNoise1D(x / 10.0f) * 3);

                for (int y = 0; y < size.y; ++y)
                {
                    int ox = (int)(Mathf.PerlinNoise1D(1000 + y / 10.0f) * 3);

                    updowns[x + y * size.x] = grand[(x + ox + size.x) % size.x, (y + oy + size.y) % size.y];
                }
            }

            // 1～2億年後に次の地形変動
            ageToNextDiastrophism = (long)((random.NextDouble()+1) * 100000000.0f);
        }

        public void OnTick(float deltaTick)
        {
            UpdateEvolutionScores(deltaTick);

            UpdateTimescale();

            LogStatisticsHistory();

            LogEventHistory();
        }

        private void UpdateTimescale()
        {
            int threshold = 2;

            foreach (Species spec in CoL.instance.species)
            {
                if (populations[spec.id] >= threshold && spec.layer != LifeLayer.Plant) // 植物の登場時期は適当なので考慮しない
                {
                    timescale = spec.timescale.Max(timescale);
                }
            }

            // 約32億～28億年前  中太古代 (Mesoarchean)	縞状鉄鉱層の形成が顕著化
            if (ocean.oxydizedMineral > ocean.mineral * 0.1f) timescale = GeologicalTimescale.Mesoarchean.Max(timescale);

            // 約28億～25億年前  新太古代 (Neoarchean)	酸素濃度が徐々に増加（GOEの前段階）
            if (atmosphere.o2Mass / atmosphere.Mass >= 0.005f) timescale = GeologicalTimescale.Neoarchean.Max(timescale);

            // 原生代

            // 約25億～23億年前  古原生代 (Paleoproterozoic)	Great Oxygenation Eventが発生、大気に酸素蓄積
            if (atmosphere.o2Mass/atmosphere.Mass >= 0.01f) timescale = GeologicalTimescale.Paleoproterozoic.Max(timescale);


            if (age >= 10e9 ) timescale = GeologicalTimescale.Terminalean.Max(timescale);

            if (evolutionScores.Max()>=100) timescale = GeologicalTimescale.Holocene.Max(timescale);
        }

        private void UpdateEvolutionScores(float deltaTick)
        {
            float max = evolutionScores.Max();
            if (max < 100)
            {
                // まだどの生物も文明を持ていない時のみ進化スコアを加算

                foreach (Species spec in CoL.instance.species)
                {
                    if (spec.evolutionSpeed != 0 && populations[spec.id]!=0)
                    {
                        if (evolutionScores[spec.id] == 0)
                        {
                            // 進化したばかりっぽい
                            // 進化元のスコアを引き継ぐ
                            for (int i = 0; i < spec.transforms.Length; ++i)
                            {
                                evolutionScores[spec.id] = Mathf.Max(evolutionScores[spec.id], evolutionScores[spec.transforms[i]]);
                            }
                        }
                        evolutionScores[spec.id] += Mathf.Sqrt(populations[spec.id]) * spec.evolutionSpeed * deltaTick;
                    }
                }
            }
        }

        private void LogEventHistory()
        {
            // 一番最初に地球形成の記録を残す
            if (log.Count == 0) log.Add(new LogEntry(timescale, 0, 0, LogType.PlanetFormation, 0));

            // 気温が下がってきた
            if (atmosphere.temperature < PhysicalConstants.CelsiusZero + 1000) AddLog(LogType.SurfaceFormation);

            // 1トン以上の海洋がある
            if (ocean.oceanMass > 1000) AddLog(LogType.OceanFormation);

            // 約32億～28億年前  中太古代 (Mesoarchean)	縞状鉄鉱層の形成が顕著化
            if (ocean.oxydizedMineral > ocean.mineral * 0.1f) AddLog(LogType.BandedIronFormation);

            // 約28億～25億年前  新太古代 (Neoarchean)	酸素濃度が徐々に増加（GOEの前段階）
            if (atmosphere.o2Mass / atmosphere.Mass >= 0.005f) AddLog(LogType.PreGreatOxidationEvent);

            // 1%以上の酸素がある
            if (atmosphere.o2Mass / atmosphere.Mass >= 0.01f) AddLog(LogType.GreatOxidationEvent);

            if (timescale == GeologicalTimescale.Terminalean)
            {
                AddLog(LogType.SolarInflation);
                
                // 海の10%以上が蒸発
                if (atmosphere.h2oMass>ocean.oceanMass*0.9f ) AddLog(LogType.OceanEvaporation);

                if (atmosphere.temperature > PhysicalConstants.CelsiusZero + 1000) AddLog(LogType.SurfaceMeltdown);

                if (history.microbePopulation.GetCurrentValue(0) == 0 &&
                    history.plantPopulation.GetCurrentValue(0) == 0 &&
                    history.animalPopulation.GetCurrentValue(0) == 0) AddLog(LogType.EndOfLife);
            }

            int threshold = 2;

            for (int i = 0; i < populations.Length; ++i)
            {
                if (populations[i] > threshold)
                {
                    AddLog(LogType.Speciation, i);
                    if (CoL.instance.species[i].layer == LifeLayer.Plant &&
                        (CoL.instance.species[i].preferences[(int)NicheAxis.Elevation][1].x + CoL.instance.species[i].preferences[(int)NicheAxis.Elevation][2].x > 0))
                    {
                        AddLog(LogType.EmergenceOfTerrestrialPlants);
                    }
                    if (CoL.instance.species[i].layer == LifeLayer.Animal &&
                        (CoL.instance.species[i].preferences[(int)NicheAxis.Elevation][1].x + CoL.instance.species[i].preferences[(int)NicheAxis.Elevation][2].x > 0))
                    {
                        AddLog(LogType.EmergenceOfTerrestrialPlants);
                    }
                }
            }

            for (int i = 0; i < evolutionScores.Length; ++i)
            {
                if (evolutionScores[i] >= 100)
                {
                    AddLog(LogType.Civilization, i);
                }
            }
        }

        private void LogStatisticsHistory()
        {
            history.n2Mass.AddData(age, tick, atmosphere.n2Mass);
            history.o2Mass.AddData(age, tick, atmosphere.o2Mass);
            history.co2Mass.AddData(age, tick, atmosphere.co2Mass);
            history.h2oMass.AddData(age, tick, atmosphere.h2oMass);
            history.temperature.AddData(age, tick, atmosphere.temperature);
            history.photosynthesisMass.AddData(age, tick, statistics.photosynthesisMass);
            history.respirationMass.AddData(age, tick, statistics.respirationMass);

            long microbePopulation = 0;
            long plantPopulation = 0;
            long animalPopulation = 0;
            for(int i = 0; i< CoL.instance.species.Length;++i)
            {
                if (CoL.instance.species[i].layer == LifeLayer.Microbe)
                {
                    microbePopulation += populations[i];
                }
                else if (CoL.instance.species[i].layer == LifeLayer.Plant)
                {
                    plantPopulation += populations[i];
                }
                else if (CoL.instance.species[i].layer == LifeLayer.Animal)
                {
                    animalPopulation += populations[i];
                }
            }
            history.microbePopulation.AddData(age, tick, microbePopulation);
            history.plantPopulation.AddData(age, tick, plantPopulation);
            history.animalPopulation.AddData(age, tick, animalPopulation);
        }

        public bool Found(in Species spec)
        {
            if (Options.godMode)
            {
                return true;
            }
            return LogExists(LogType.Speciation, spec.id);
        }

        public bool LogExists(LogType type, int param1 = 0)
        {
            return log.Exists(log => log.type == type && log.param1 == param1);
        }

        public bool AddLog(LogType type, int param1 = 0)
        {
            if (LogExists(type, param1))
            {
                return false;
            }
            else
            {
                log.Add(new LogEntry(timescale, age, tick, type, param1));
                return true;
            }
        }

        public float MicrobeScale()
        {
            return Options.tierScaling ? Mathf.Clamp01(1 - history.animalPopulation.GetCurrentValue(0) / 100.0f) : 1;
        }
        public float SurfaceArea => 4 * MathF.PI * orbit.radius * orbit.radius;
        public float Pressure => (atmosphere.co2Mass + atmosphere.o2Mass + atmosphere.n2Mass + atmosphere.h2oMass) * orbit.gravity / SurfaceArea;

    }
}