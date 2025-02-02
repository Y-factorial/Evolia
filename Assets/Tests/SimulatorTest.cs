using Evolia.GameScene;
using Evolia.Model;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.TestTools;

namespace Evolia.Tests
{
    public class ArrangeAxis
    {
        public readonly NicheAxis niche;

        public readonly float maxEdge;

        public readonly float minEdge;

        public readonly float deltaEdge;

        public readonly float deltaMiddle;

        public ArrangeAxis(NicheAxis niche, float maxEdge, float minEdge, float deltaEdge, float deltaMiddle)
        {
            this.niche = niche;
            this.maxEdge = maxEdge;
            this.minEdge = minEdge;
            this.deltaEdge = deltaEdge;
            this.deltaMiddle = deltaMiddle;
        }


        public static readonly ArrangeAxis Temperature = new ArrangeAxis(NicheAxis.Temperature, 10, 4, 0.5f, 0.5f);

        public static readonly ArrangeAxis Elevation = new ArrangeAxis(NicheAxis.Elevation, 2000, 600, 50, 50);
    }

    public abstract class NicheArranger
    {
        public abstract float Population { get; }

        public abstract string Name { get; }

        public abstract float GetPreference(NicheAxis niche, int index);

        public abstract void MovePreference(NicheAxis niche, int index, float delta);

        public virtual void AutoArrange(ArrangeAxis axis, float average, NicheArranger lower, NicheArranger upper)
        {
            Debug.Log($"平均値 {Name} {Population} vs {average} = {Population / average * 100}%");

            if (Population < average / 1.02f)
            {
                // 少なすぎるので増やす

                if (lower != null && (upper == null || lower.Population > upper.Population))
                {
                    // 下の方が大きいので、下を侵食する
                    ArrangePreference(axis, 0, -1, lower);
                }
                else
                {
                    // 上の方が大きいので、上を侵食する
                    ArrangePreference(axis, 3, 1, upper);
                }
            }
            else if (Population > average * 1.02f)
            {
                // 大きすぎるので減らす

                if (lower != null && (upper == null || lower.Population < upper.Population))
                {
                    // 下の方が小さいので、下から撤退する
                    ArrangePreference(axis, 0, 1, lower);
                }
                else
                {
                    // 上の方が小さいので、上から撤退する
                    ArrangePreference(axis, 3, -1, upper);
                }
            }
        }

        public virtual void ArrangePreference(ArrangeAxis axis, int edge, int sign, NicheArranger neighbor)
        {
            int middle = edge == 0 ? 1 : 2;
            int rmiddle = 3 - middle;
            int redge = 3 - edge;

            float deltaEdge = sign * axis.deltaEdge;
            float deltaMiddle = sign * axis.deltaMiddle;

            float predictedEdgeAbs = Mathf.Abs(GetPreference(axis.niche, edge) + deltaEdge - GetPreference(axis.niche, middle));

            if (predictedEdgeAbs >= axis.minEdge && predictedEdgeAbs <= axis.maxEdge)
            {
                Debug.Log($"{Name} の {axis.niche} の {edge} を {deltaEdge} 動かす");

                // 最低値を下げることで様子を見る
                MovePreference(axis.niche, edge, deltaEdge);
            }
            else
            {
                Debug.Log($"{Name} の {axis.niche} の {middle} を {deltaMiddle} 動かす");

                MovePreference(axis.niche, middle, deltaMiddle);
                MovePreference(axis.niche, edge, deltaMiddle);

                // ということは、次のグループの最適値の上限も下げる
                if (neighbor != null)
                {
                    Debug.Log($"併せて {neighbor.Name} の {axis.niche} の {rmiddle} を {deltaMiddle} 動かす");

                    neighbor.MovePreference(axis.niche, rmiddle, deltaMiddle);
                    neighbor.MovePreference(axis.niche, redge, deltaMiddle);
                }
            }
        }

        public static GroupArranger CreateGroup(string name, ArrangeAxis axis, List<float> populations, params object[] children)
        {
            return new GroupArranger(name, axis, CreateSpeciesList(populations, children));
        }

        public static List<NicheArranger> CreateSpeciesList(List<float> populations, params object[] children)
        {
            List<NicheArranger> result = new List<NicheArranger>();
            foreach (object child in children)
            {
                if (child is NicheArranger childArranger)
                {
                    result.Add(childArranger);
                }
                else if (child is string childName)
                {
                    result.Add(CreateSpecies(childName, populations));
                }
            }

            return result;
        }

        public static SpeciesArranger CreateSpecies(string name, List<float> populations)
        {
            int id = CoL.instance[name].id;
            return new SpeciesArranger(id, populations[id]);
        }

    }

    public class SpeciesArranger : NicheArranger
    {
        private int id;

        private float population;

        public SpeciesArranger(int id, float population)
        {
            this.id = id;
            this.population = population;
        }

        public override float Population => population;

        public override string Name => CoL.instance.names[id];

        public override float GetPreference(NicheAxis niche, int index)
        {
            return CoL.instance[id].preferences[(int)niche][index].x;
        }

        public override void MovePreference(NicheAxis niche, int index, float delta)
        {
            CoL.instance[id].preferences[(int)niche][index].x += delta;
        }
    }

    public class GroupArranger : NicheArranger
    {
        private string name;

        private ArrangeAxis axis;

        private List<NicheArranger> children;

        public GroupArranger(string groupName, ArrangeAxis axis, List<NicheArranger> children)
        {
            this.name = groupName;
            this.axis = axis;
            this.children = children;
        }

        public override float Population =>  children.Select( (spec)=>spec.Population).Average();

        public override string Name => name;

        public override float GetPreference(NicheAxis niche, int index)
        {
            return children[0].GetPreference(niche, index);
        }

        public override void MovePreference(NicheAxis niche, int index, float delta)
        {
            for (int i = 0; i < children.Count; i++)
            {
                children[i].MovePreference(niche, index, delta);
            }
        }

        public override void AutoArrange(ArrangeAxis axis, float average, NicheArranger upper, NicheArranger lower)
        {
            base.AutoArrange(axis, average, upper, lower);

            ArrangeChildren();
        }

        public void ArrangeChildren()
        {
            for (int i = 0; i < children.Count; ++i)
            {
                NicheArranger lower = i == 0 ? null : children[i - 1];
                NicheArranger upper = i == this.children.Count - 1 ? null : this.children[i + 1];

                this.children[i].AutoArrange(this.axis, this.Population, lower, upper);
            }
        }

    }

    public class SimulatorTest
    {
        [UnityTest, Timeout(36000000)] // 10時間
        public IEnumerator 温度適性を自動調整()
        {
            // 10 = 1万秒 = 3時間
            for (int i = 0; i < 30; ++i)
            {
                Debug.Log($"[SimulatorTest] Start Step. i:{i}");
                yield return DoStep();
            }
        }

        private IEnumerator<object> DoStep()
        {
            // populations[spec][n] = population
            List<List<float>> populations = CoL.instance.species.Select((spec) => new List<float>()).ToList();

            List<List<int>> ranks = CoL.instance.species.Select((spec) => new List<int>()).ToList();

            int count = 100;
            yield return DoLoop(count, populations, ranks);

            ArrangeNiches(populations);

            ArrangeRanks(ranks);

            EditorUtility.SetDirty(CoL.instance);
            AssetDatabase.SaveAssets();
        }

        private void ArrangeNiches(List<List<float>> populations)
        {

            // 外れ値を削るため、中央より上の25%の平均のみ採用
            // 全く生物が登場しない場合や、季節的な理由で数が激減している場合があるため、そこそこうまくいっている時の数字のみを採用する
            List<float> averages = populations.Select((values) => values.OrderBy((f) => f).Skip(values.Count / 2).Take(values.Count / 4).Average()).ToList();

            // 海の動物
            GroupArranger aquatics = NicheArranger.CreateGroup("水生動物", ArrangeAxis.Temperature, averages,
                NicheArranger.CreateGroup("魚類/棘皮動物", ArrangeAxis.Elevation, averages, "ウニ", "ヒトデ", "ナマコ", "ウミユリ", "魚類"),
                NicheArranger.CreateGroup("甲殻類", ArrangeAxis.Elevation, averages, "カニ", "エビ", "カイアシ"),
                NicheArranger.CreateGroup("軟体動物", ArrangeAxis.Elevation, averages, "タコ", "イカ", "巻貝", "ツノガイ"),
                NicheArranger.CreateGroup("刺胞動物", ArrangeAxis.Elevation, averages, "クラゲ", "イソギンチャク", "サンゴ")
            );
            aquatics.ArrangeChildren();

            // 陸の動物
            GroupArranger terrestrials = NicheArranger.CreateGroup("陸生動物", ArrangeAxis.Temperature, averages,
                NicheArranger.CreateGroup("脊椎動物", ArrangeAxis.Elevation, averages, "両生類", "爬虫類",
                    NicheArranger.CreateGroup("恐竜/哺乳類", ArrangeAxis.Temperature, averages, "哺乳類", "恐竜"), "鳥類"),
                NicheArranger.CreateGroup("昆虫", ArrangeAxis.Elevation, averages, "トンボ", "ハチ", "チョウ"),
                NicheArranger.CreateGroup("環形動物", ArrangeAxis.Elevation, averages, "ゴカイ", "ヒル", "ミミズ")
            );

            terrestrials.ArrangeChildren();

            AlignOldSpecies();
        }

        private void ArrangeRanks(List<List<int>> ranks)
        {
            List<float> points = ranks.Select((values) => values.Count == 0 ? 0 : values.Select((num) => 100.0f / num).Average()).ToList();

            //ArrangeRank(points, "カイメン", 3.5f);
            //ArrangeRank(points, "ディッキンソニア", 3.5f);
            ArrangeRank(points, "クラゲ", 11);
            ArrangeRank(points, "イソギンチャク", 12);
            ArrangeRank(points, "サンゴ", 13);
            //ArrangeRank(points, "キンベレラ", 3.5f);
            //ArrangeRank(points, "プラナリア", 3.5f);
            ArrangeRank(points, "ゴカイ", 11);
            ArrangeRank(points, "ヒル", 12);
            ArrangeRank(points, "ミミズ", 14);
            ArrangeRank(points, "ツノガイ", 12);
            ArrangeRank(points, "巻貝", 14);
            ArrangeRank(points, "イカ", 16);
            ArrangeRank(points, "タコ", 20);
            //ArrangeRank(points, "レアンコイリア", 3.5f);
            ArrangeRank(points, "カイアシ", 11);
            ArrangeRank(points, "エビ", 12);
            ArrangeRank(points, "カニ", 14);
            ArrangeRank(points, "トンボ", 11);
            ArrangeRank(points, "ハチ", 12);
            ArrangeRank(points, "チョウ", 14);
            //ArrangeRank(points, "イカリア", 3.5f);
            ArrangeRank(points, "ウミユリ", 10);
            ArrangeRank(points, "ナマコ", 11);
            ArrangeRank(points, "ヒトデ", 12);
            ArrangeRank(points, "ウニ", 14);
            //ArrangeRank(points, "ピカイア", 3.5f);
            ArrangeRank(points, "魚類", 16);
            ArrangeRank(points, "両生類", 18);
            ArrangeRank(points, "爬虫類", 20);
            ArrangeRank(points, "恐竜", 22);
            ArrangeRank(points, "鳥類", 24);
            ArrangeRank(points, "哺乳類", 28);

        }

        private void ArrangeRank(List<float> points, string name, float target)
        {
            float point = points[CoL.instance[name].id];

            float diff = target - point;

            if (Mathf.Abs(diff) >= 1.0f)
            {
                // 0.5 以上ターゲットから差がある
                float unit = 0.000001f;

                float delta = Mathf.Clamp( Mathf.Floor(diff), -5, 5 );


                CoL.instance[name].evolutionSpeed += unit * delta;

                Debug.Log($"{name} のポイント {point} -> {target} のため、進化スピードを {delta} 増やす -> {CoL.instance[name].evolutionSpeed}");
            }
        }

        [UnityTest]
        public IEnumerator 古い生物を現代の生物に合わせる()
        {
            AlignOldSpecies();

            EditorUtility.SetDirty(CoL.instance);
            AssetDatabase.SaveAssets();

            yield return null;
        }

        private void AlignOldSpecies()
        {

            // 原始的な生物の調整
            // ディッキンソニア
            Align("ディッキンソニア", "カニ", NicheAxis.Elevation, 3);

            // キンベレラ
            Align("キンベレラ", "ツノガイ", NicheAxis.Temperature, 0);
            Align("キンベレラ", "エビ", NicheAxis.Elevation, 0);
            Align("キンベレラ", "イソギンチャク", NicheAxis.Elevation, 3);

            // プラナリア
            Align("プラナリア", "ツノガイ", NicheAxis.Temperature, 0);
            Align("プラナリア", "ツノガイ", NicheAxis.Elevation, 0);

            // レアンコイリア
            Align("レアンコイリア", "エビ", NicheAxis.Temperature, 0);
            Align("レアンコイリア", "エビ", NicheAxis.Temperature, 3);
            Align("レアンコイリア", "エビ", NicheAxis.Elevation, 0);

            // イカリア
            Align("イカリア", "ヒトデ", NicheAxis.Temperature, 3);
            Align("イカリア", "ヒトデ", NicheAxis.Elevation, 0);
            Align("イカリア", "ナマコ", NicheAxis.Elevation, 3);

            // ピカイア
            Align("ピカイア", "ウミユリ", NicheAxis.Temperature, 3);
            Align("ピカイア", "ウミユリ", NicheAxis.Elevation, 0);
        }

        private void Align(string target, string reference, NicheAxis niche, int edge)
        {
            int middle = edge== 0 ? 1 : 2;

            ref Species t = ref CoL.instance[target];
            ref Species r = ref CoL.instance[reference];

            t.preferences[(int)niche][edge].x = r.preferences[(int)niche][edge].x;
            t.preferences[(int)niche][middle].x = r.preferences[(int)niche][middle].x;
        }

        [UnityTest, Timeout(1800000)]
        public IEnumerator 統計情報を取得()
        {
            // populations[spec][n] = population
            List<List<float>> populations = CoL.instance.species.Select((spec) => new List<float>()).ToList();

            List<List<int>> ranks = CoL.instance.species.Select((spec) => new List<int>()).ToList();

            int count = 100;
            yield return DoLoop(count, populations, ranks);
        }

        public class TimeResult
        {
            public int N;
            public string timescale;
            public long age;
            public float tick;
            public TimeResult(int n, GeologicalTimescale timescale, long age, float tick)
            {
                N = n;
                this.timescale = $"{(int)timescale:00}_{timescale.Name()}";
                this.age = age;
                this.tick = tick;
            }
        }

        public class SpeciesResult
        {
            public int N;
            public string habitat;
            public string species;
            public float population;
            public float evolution;
            public SpeciesResult(int n, in Species spec, float population, float evolution)
            {
                N = n;
                this.habitat = (spec.preferences[(int)NicheAxis.Elevation][1].x + spec.preferences[(int)NicheAxis.Elevation][2].x) / 2 >= 0 ? "land" : "ocean";
                this.species = $"{spec.id:00}_{CoL.instance.names[spec.id]}";
                this.population = population;
                this.evolution = evolution;
            }
        }

        public class RankResult
        {
            public int N;
            public string species;
            public int rank;
            public RankResult(int n, in Species spec, int rank)
            {
                N = n;
                this.species = $"{spec.id:00}_{CoL.instance.names[spec.id]}";
                this.rank = rank;
            }
        }

        private IEnumerator<object> DoLoop(int count, List<List<float>> populations, List<List<int>> ranks)
        {
            List<TimeResult> timeResult = new();
            List<SpeciesResult> speciesResult = new();
            List<RankResult> rankResult = new();

            for (int n = 0; n < count; ++n)
            {
                yield return DoSimulate(n, populations, ranks, timeResult, speciesResult, rankResult);
            }

            if (Directory.Exists(Application.dataPath + $"/Tests/Output") == false)
            {
                Directory.CreateDirectory(Application.dataPath + $"/Tests/Output");
            }
            File.WriteAllLines(Application.dataPath + $"/Tests/Output/TimeResult_{DateTime.Now:yyyyMMddHHmm}.tsv", ToText(timeResult, "\t"));
            File.WriteAllLines(Application.dataPath + $"/Tests/Output/SpeciesResult_{DateTime.Now:yyyyMMddHHmm}.tsv", ToText(speciesResult, "\t"));
            File.WriteAllLines(Application.dataPath + $"/Tests/Output/RankResult_{DateTime.Now:yyyyMMddHHmm}.tsv", ToText(rankResult, "\t"));
        }

        private IEnumerator<object> DoSimulate(int n, List<List<float>> populations, List<List<int>> ranks, List<TimeResult> timeResult, 
            List<SpeciesResult> speciesResult, List<RankResult> rankResult)
        {
            Debug.Log($"[SimulatorTest] Start. n:{n}");

            Planet planet = new Planet();
            planet.Init(1.0f, 1.0f, 1.0f, 24 * Mathf.PI / 180, 1.0f);
            GameController.planet = planet;

            var obj = new GameObject();
            obj.SetActive(false);
            PlanetSimulator simulator = obj.AddComponent<PlanetSimulator>();
            simulator.computeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/GameScene/Shaders/PlanetSimulator.compute");
            simulator.speed = GameSpeed.SuperFast;
            simulator.headless = true;
            simulator.Awake();

            GeologicalTimescale timescale = GeologicalTimescale.Hadean;

            float[] averagePopulations = new float[CoL.MAX_SPECIES];

            using (IEnumerator<object> e = simulator.CalcThread())
            {

                while (e.MoveNext() && planet.tick < 15000)
                {
                    while (timescale < planet.timescale)
                    {
                        timescale = timescale + 1;
                        timeResult.Add(new TimeResult(n, timescale, planet.age, planet.tick));
                    }

                    foreach (Species spec in CoL.instance.species)
                    {
                        averagePopulations[spec.id] = (averagePopulations[spec.id]*9 + planet.populations[spec.id]) / 10;
                    }

                    if (planet.evolutionScores.Max() >= 100)
                    {
                        for (int i = 0; i < planet.evolutionScores.Length; i++)
                        {
                            if (planet.evolutionScores[i] >= 100)
                            {
                                Debug.Log($"[SimulatorTest] Winner. n:{n}, max: {CoL.instance.names[i]}");
                                goto EndOfLoop;
                            }
                        }
                    }

                    yield return null;
                }
            }

            EndOfLoop:

            while (timescale < GeologicalTimescale.Cenozonic)
            {
                timescale = timescale + 1;
                timeResult.Add(new TimeResult(n, timescale, planet.age, planet.tick));
            }

            timeResult.Add(new TimeResult(n, GeologicalTimescale.Terminalean, planet.age, planet.tick));

            yield return new WaitForSeconds(1);

            foreach (Species spec in CoL.instance.species)
            {
                populations[spec.id].Add(averagePopulations[spec.id]);

                if (spec.layer == LifeLayer.Animal)
                {
                    speciesResult.Add(new SpeciesResult(n, spec, averagePopulations[spec.id], planet.evolutionScores[spec.id]));

                    int num = planet.evolutionScores.Where((f) => f >= planet.evolutionScores[spec.id]).Count();
                    rankResult.Add(new RankResult(n, spec, num));

                    ranks[spec.id].Add(num);
                }
            }

            simulator.OnDestroy();
            GameObject.DestroyImmediate(obj);
            GameController.planet = null;
        }

        private List<string> ToText<T>(List<T> list, string separator)
        {

            List<string> csv = new();

            csv.Add(string.Join(separator, typeof(T).GetFields().Select((f) => f.Name).ToList()));
            csv.AddRange(
                list.Select((item)=> string.Join(separator, item.GetType().GetFields().Select((f) => f.GetValue(item).ToString()).ToList()))
                );

            return csv;
        }
    }


}