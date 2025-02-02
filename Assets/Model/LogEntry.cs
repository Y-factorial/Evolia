using Evolia.GameScene;
using System;
using UnityEngine;

namespace Evolia.Model
{
    [Serializable]
    public struct LogEntry
    {
        [SerializeField]
        public GeologicalTimescale timescale;

        [SerializeField]
        public long age;

        [SerializeField]
        public long tick;

        [SerializeField]
        public LogType type;

        [SerializeField]
        public int param1;

        public LogEntry(GeologicalTimescale timescale, long age, long tick, LogType type, int param1)
        {
            this.timescale = timescale;
            this.age = age;
            this.tick = tick;
            this.type = type;
            this.param1 = param1;
        }

        public string GetNewsMessage(Planet planet)
        {
            switch (type)
            {
                case LogType.PlanetFormation:
                    return $"原始惑星円盤の中で微粒子が重力によって集まり、惑星{planet.name}が形成されました。";
                case LogType.SurfaceFormation:
                    return "高温のマグマが冷えて固体の地殻が形成されていきます。";
                case LogType.OceanFormation:
                    return "地表の冷却が進行し、降水によって海洋が形成されました。";
                case LogType.BandedIronFormation:
                    return "海洋中の鉄分が酸素と結合し、縞状鉄鉱層が形成されています。";
                case LogType.PreGreatOxidationEvent:
                    return "大気中の酸素濃度が徐々に増加しています。";
                case LogType.GreatOxidationEvent:
                    return "生命の活動によって酸素が大気中に放出され、大気組成が大きく変化しています。";
                case LogType.Speciation:
                    switch (param1)
                    {
                        case SpecialSpecies.LUCA:
                            return $"原始海洋において、有機分子が複雑に結合し、最初の生命が誕生しました。";
                        case SpecialSpecies.Cyanobacteria:
                            return $"{CoL.instance.names[param1]}が誕生し、光合成を開始しました。";
                        case SpecialSpecies.AlphaProteoBacteria:
                            return $"{CoL.instance.names[param1]}が誕生し、酸素をエネルギーとして利用し始めました。";
                        case SpecialSpecies.LECA:
                            return $"原始的な細胞に核を持つ構造が生まれ、真核生物が誕生しました。";
                        case SpecialSpecies.Sponge:
                            return $"複数の細胞が集まり、それぞれが異なる役割を持つ多細胞生物が誕生しました。";
                        default:
                            return $"{CoL.instance.names[param1]}が誕生しました。";
                    }
                case LogType.EmergenceOfTerrestrialPlants:
                    return $"光合成を行う植物が水中から陸上に進出しました。";
                case LogType.EmergenceOfTerrestrialAnimals:
                    return $"動物が水中から陸上へと進出し始めています。";
                case LogType.Civilization:
                    return $"{CoL.instance.names[param1]}が文明を築きました。";
                case LogType.SolarInflation:
                    return $"太陽が膨張を始め、地球の表面温度が上昇しています。";
                case LogType.OceanEvaporation:
                    return $"高温により海洋が蒸発し始めています。";
                case LogType.EndOfLife:
                    return $"全ての生物が絶滅しました。";
                case LogType.SurfaceMeltdown:
                    return $"高温により地表が融解しています。";
                default:
                    throw new NotImplementedException($"type {type} is not implemented");
            }
        }

        public string GetAgeText()
        {
            long oku = age / 100000000L;
            long man = age / 10000 % 10000;
            long nen = age % 10000;
            string text = "";
            if (oku != 0)
            {
                text += $"{oku}億";
            }
            if (man != 0)
            {
                text += $"{man}万";
            }
            if (nen!=0 || text.Length==0)
            {
                text += $"{nen}";
            }
            return $"{text}年";
        }

        public string GetLogMessage(Planet planet)
        {
            switch (type)
            {
                case LogType.PlanetFormation:
                    return $"惑星{planet.name}誕生";
                case LogType.SurfaceFormation:
                    return "地表の形成";
                case LogType.OceanFormation:
                    return "海洋の形成";
                case LogType.BandedIronFormation:
                    return "縞状鉄鉱層の形成";
                case LogType.PreGreatOxidationEvent:
                    return "酸素濃度の増加";
                case LogType.GreatOxidationEvent:
                    return "大酸化イベント";
                case LogType.Speciation:
                    switch (param1)
                    {
                        case SpecialSpecies.LUCA:
                            return $"最初の生命の誕生";
                        case SpecialSpecies.LECA:
                            return $"最初の真核生物の誕生";
                        case SpecialSpecies.Sponge:
                            return $"最初の多細胞生物の誕生";
                        default:
                            return $"{CoL.instance.names[param1]}の誕生";
                    }
                case LogType.EmergenceOfTerrestrialPlants:
                    return $"植物が陸上に進出";
                case LogType.EmergenceOfTerrestrialAnimals:
                    return $"動物が陸上に進出";
                case LogType.Civilization:
                    return $"{CoL.instance.names[param1]}文明の誕生";
                case LogType.SolarInflation:
                    return $"太陽の膨張";
                case LogType.OceanEvaporation:
                    return $"海洋の蒸発";
                case LogType.EndOfLife:
                    return $"生命の終わり";
                case LogType.SurfaceMeltdown:
                    return $"地表の融解";
                default:
                    throw new NotImplementedException($"type {type} is not implemented");
            }
        }
    }
}