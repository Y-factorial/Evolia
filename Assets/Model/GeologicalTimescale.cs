
using System;
using UnityEngine;

namespace Evolia.Model
{

    public enum GeologicalTimescale
    {
        // 冥王代
        Hadean,

        // 太古代

        // 約40億～36億年前  原太古代 (Eoarchean)	最初の生命が誕生（光合成前）
        Eoarchean,
        // 約36億～32億年前  古太古代 (Paleoarchean)	シアノバクテリアが光合成を開始
        Paleoarchean,
        // 約32億～28億年前  中太古代 (Mesoarchean)	縞状鉄鉱層の形成が顕著化
        Mesoarchean,
        // 約28億～25億年前  新太古代 (Neoarchean)	酸素濃度が徐々に増加（GOEの前段階）
        Neoarchean,

        // 原生代

        // 約25億～23億年前  古原生代 (Paleoproterozoic)	Great Oxygenation Eventが発生、大気に酸素蓄積
        Paleoproterozoic,
        // 約16億年前 ～ 約10億年前 中原生代 Mesoproterozoic 真核生物が登場
        Mesoproterozoic,
        // 約10億年前 ～ 約5.4億年前 新原生代 (Neoproterozoic)
        Neoproterozoic,

        // 約6億年前 ～ 約5億5000万年前	-  エディアカラ紀 多細胞生物の出現（エディアカラ生物）- 珪藻、藻類、海綿など
        Ediacaran,

        // 古生代
        // カンブリア紀 約5億4100万年前 ～ 約4億8500万年前	- 現代の動物門の多くが出現（カンブリア爆発）- トリロバイト- アノマロカリスなどの節足動物
        Cambrian,
        //  オルドビス紀     約4億8500万年前 ～ 約4億4400万年前	- 無顎類（顎のない魚類）- サンゴ、腕足動物、コノドント（歯状化石）
        Ordovician,
        // シルル紀  約4億4400万年前 ～ 約4億1900万年前	- 顎口類（顎を持つ魚類） - 最初の陸上植物（クックソニアなど）
        Silurian,
        // デボン紀  約4億1900万年前 ～ 約3億5900万年前	- 硬骨魚、サメなどの魚類 - 両生類の祖先（イクチオステガなど） - シダ植物
        Devonian,
        // 石炭紀  約3億5900万年前 ～ 約2億9900万年前	- 爬虫類（最初の卵生動物）- 巨大シダ植物、裸子植物
        Carboniferous,
        // ペルム紀 約2億9900万年前 ～ 約2億5100万年前	- 哺乳類型爬虫類（単弓類） - 初期の昆虫、裸子植物
        Permian,

        // 中生代
        Mesozonic,

        // 三畳紀  約2億5100万年前 ～ 約2億年前	- 恐竜の祖先（最初の恐竜）- 初期の哺乳類- 裸子植物の繁栄
        //Triassic,
        //ジュラ紀  約2億年前 ～ 約1億4500万年前	- 大型恐竜の繁栄（ブラキオサウルス、アロサウルスなど）- 始祖鳥- 裸子植物が引き続き優勢
        //Jurassic,
        // 白亜紀    約1億4500万年前 ～ 約6600万年前	- 被子植物（花を持つ植物）の出現- ティラノサウルス、トリケラトプスなどの恐竜- 白亜紀末の大量絶滅
        //Cretaceous,

        // 新生代
        Cenozonic,

        // 古第三紀    約6600万年前 ～ 約2300万年前	- 哺乳類と鳥類が多様化 - 被子植物の繁栄 - 原始的な霊長類の出現
        //Paleogene,

        // 新第三紀     約2300万年前 ～ 約258万年前	- 現代型の哺乳類と鳥類- 初期のヒト亜科（類人猿）- 草原生態系の拡大
        //Neogene,

        // 第四紀    約258万年前 ～ 現在	- 現生人類（ホモ・サピエンス）の出現- 大型哺乳類の進化（マンモスなど）- 氷期と間氷期の繰り返し
        //Quaternary,

        // 完新世
        Holocene,

        // 終末代
        Terminalean
    }

    public static class GeologicEraExt
    {
        public static GeologicalTimescale Max(this GeologicalTimescale v, GeologicalTimescale other)
        {
            return v > other ? v : other;
        }

        public static long AgeSpeed(this GeologicalTimescale v)
        {
            switch (v)
            {
                // 冥王代
                case GeologicalTimescale.Hadean:
                    return 10000000;
                // 太古代
                case GeologicalTimescale.Eoarchean:
                case GeologicalTimescale.Paleoarchean:
                case GeologicalTimescale.Mesoarchean:
                case GeologicalTimescale.Neoarchean:
                    return 3000000;
                // 原生代
                case GeologicalTimescale.Paleoproterozoic:
                case GeologicalTimescale.Mesoproterozoic:
                case GeologicalTimescale.Neoproterozoic:
                    return 2000000;
                case GeologicalTimescale.Ediacaran:
                    return 100000;
                // 古生代
                case GeologicalTimescale.Cambrian:
                case GeologicalTimescale.Ordovician:
                case GeologicalTimescale.Silurian:
                case GeologicalTimescale.Devonian:
                case GeologicalTimescale.Carboniferous:
                case GeologicalTimescale.Permian:
                    return 100000;
                // 中生代
                case GeologicalTimescale.Mesozonic:
                    return 100000;
                // 新生代
                case GeologicalTimescale.Cenozonic:
                    return 20000;
                // 完新世
                case GeologicalTimescale.Holocene:
                    return 10;
                // 終末代
                case GeologicalTimescale.Terminalean:
                    return 1;
                default:
                    throw new NotImplementedException($"GeologicalTimescale {v} is not implemented");
            }
        }

        public static float GeologicalSpeedFactor(this GeologicalTimescale v)
        {
            switch (v)
            {
                // 冥王代
                case GeologicalTimescale.Hadean:
                    return 1f;
                // 太古代
                case GeologicalTimescale.Eoarchean:
                case GeologicalTimescale.Paleoarchean:
                case GeologicalTimescale.Mesoarchean:
                case GeologicalTimescale.Neoarchean:
                    return 1f;
                // 原生代
                case GeologicalTimescale.Paleoproterozoic:
                case GeologicalTimescale.Mesoproterozoic:
                case GeologicalTimescale.Neoproterozoic:
                    return 0.1f; // 地殻変動が速すぎて生物がついていけないので、0.5倍にしている
                case GeologicalTimescale.Ediacaran:
                    return 1.0f;
                // 古生代
                case GeologicalTimescale.Cambrian:
                case GeologicalTimescale.Ordovician:
                case GeologicalTimescale.Silurian:
                case GeologicalTimescale.Devonian:
                case GeologicalTimescale.Carboniferous:
                case GeologicalTimescale.Permian:
                    return 1.0f; // 馬鹿正直にタイムスケール通りにすると、地形が全く動かなくなって面白くないので10倍にしている
                // 中生代
                case GeologicalTimescale.Mesozonic:
                    return 1.0f; // 馬鹿正直にタイムスケール通りにすると、地形が全く動かなくなって面白くないので10倍にしている
                // 新生代
                case GeologicalTimescale.Cenozonic:
                    return 1.0f;
                // 完新世
                case GeologicalTimescale.Holocene:
                    return 1.0f;
                // 終末代
                case GeologicalTimescale.Terminalean:
                    return 1;
                default:
                    throw new NotImplementedException($"GeologicalTimescale {v} is not implemented");
            }
        }

        public static float MetabolismSpeedFactor(this GeologicalTimescale v)
        {
            switch (v)
            {
                // 冥王代
                case GeologicalTimescale.Hadean:
                    return 1f; // 馬鹿正直にタイムスケール通りにすると、大気が一瞬で変わってしまうので、100分の1にしている
                // 太古代
                case GeologicalTimescale.Eoarchean:
                case GeologicalTimescale.Paleoarchean:
                case GeologicalTimescale.Mesoarchean:
                case GeologicalTimescale.Neoarchean:
                    return 0.2f; // 馬鹿正直にタイムスケール通りにすると、大気が一瞬で変わってしまうので、50分の1にしている
                // 原生代
                case GeologicalTimescale.Paleoproterozoic:
                case GeologicalTimescale.Mesoproterozoic:
                    return 0.2f; // 馬鹿正直にタイムスケール通りにすると、大気が一瞬で変わってしまうので、50分の1にしている
                case GeologicalTimescale.Neoproterozoic:
                    return 0.05f; // エディアカラ期への移行に向けて徐々にスピードを落とす。
                case GeologicalTimescale.Ediacaran:
                    return 1f;
                // 古生代
                case GeologicalTimescale.Cambrian:
                case GeologicalTimescale.Ordovician:
                case GeologicalTimescale.Silurian:
                case GeologicalTimescale.Devonian:
                case GeologicalTimescale.Carboniferous:
                case GeologicalTimescale.Permian:
                    return 1f;
                // 中生代
                case GeologicalTimescale.Mesozonic:
                    return 1f;
                // 新生代
                case GeologicalTimescale.Cenozonic:
                    return 1f;
                // 完新世
                case GeologicalTimescale.Holocene:
                    return 1f;
                // 終末代
                case GeologicalTimescale.Terminalean:
                    return 1;
                default:
                    throw new NotImplementedException($"GeologicalTimescale {v} is not implemented");
            }
        }

        public static float HeatSpeed(this GeologicalTimescale v)
        {
            // 時間速度に比例させないので、Factorではなく値そのものを返す

            switch (v)
            {
                // 冥王代
                case GeologicalTimescale.Hadean:
                    return 1000.0f; 
                // 太古代
                case GeologicalTimescale.Eoarchean:
                case GeologicalTimescale.Paleoarchean:
                case GeologicalTimescale.Mesoarchean:
                case GeologicalTimescale.Neoarchean:
                    return 100.0f;
                // 原生代
                case GeologicalTimescale.Paleoproterozoic:
                case GeologicalTimescale.Mesoproterozoic:
                case GeologicalTimescale.Neoproterozoic:
                    return 100.0f; // 夏と冬の寒暖差が10℃程度になるように微調整してある
                case GeologicalTimescale.Ediacaran:
                    return 100.0f;
                // 古生代
                case GeologicalTimescale.Cambrian:
                case GeologicalTimescale.Ordovician:
                case GeologicalTimescale.Silurian:
                case GeologicalTimescale.Devonian:
                case GeologicalTimescale.Carboniferous:
                case GeologicalTimescale.Permian:
                    return 100.0f;
                // 中生代
                case GeologicalTimescale.Mesozonic:
                    return 100.0f;
                // 新生代
                case GeologicalTimescale.Cenozonic:
                    return 100.0f;
                // 完新世
                case GeologicalTimescale.Holocene:
                    return 100.0f;
                // 終末代
                case GeologicalTimescale.Terminalean:
                    return 100.0f;
                default:
                    throw new NotImplementedException($"GeologicalTimescale {v} is not implemented");
            }
        }
        public static string Name(this GeologicalTimescale v)
        {
            switch (v)
            {
                // 冥王代
                case GeologicalTimescale.Hadean: return "冥王代";
                // 太古代
                case GeologicalTimescale.Eoarchean:return "原太古代";
                case GeologicalTimescale.Paleoarchean: return "古太古代";
                case GeologicalTimescale.Mesoarchean: return "中太古代";
                case GeologicalTimescale.Neoarchean: return "新太古代";
                // 原生代
                case GeologicalTimescale.Paleoproterozoic: return "古原生代";
                case GeologicalTimescale.Mesoproterozoic: return "中原生代";
                case GeologicalTimescale.Neoproterozoic: return "新原生代";
                case GeologicalTimescale.Ediacaran: return "エディアカラ紀";
                // 古生代
                case GeologicalTimescale.Cambrian: return "カンブリア紀";
                case GeologicalTimescale.Ordovician: return "オルドビス紀";
                case GeologicalTimescale.Silurian: return "シルル紀";
                case GeologicalTimescale.Devonian: return "デボン紀";
                case GeologicalTimescale.Carboniferous: return "石炭紀";
                case GeologicalTimescale.Permian:return "ペルム紀";
                // 中生代
                case GeologicalTimescale.Mesozonic:return "中生代";
                // 新生代
                case GeologicalTimescale.Cenozonic: return "新生代";
                // 完新世
                case GeologicalTimescale.Holocene: return "完新世";
                // 終末代
                case GeologicalTimescale.Terminalean: return "終末代";
                default:
                    throw new NotImplementedException($"GeologicalTimescale {v} is not implemented");
            }
        }

        public static Color Color(this GeologicalTimescale v)
        {
            switch (v)
            {
                // 冥王代
                case GeologicalTimescale.Hadean: return new Color(180 / 255.0f, 30 / 255.0f, 141/255.0f);
                // 太古代
                case GeologicalTimescale.Eoarchean: return new Color(215 / 255.0f, 12 / 255.0f, 140 / 255.0f);
                case GeologicalTimescale.Paleoarchean: return new Color(241 / 255.0f, 102 / 255.0f, 167 / 255.0f);
                case GeologicalTimescale.Mesoarchean: return new Color(242 / 255.0f, 134 / 255.0f, 174 / 255.0f);
                case GeologicalTimescale.Neoarchean: return new Color(246 / 255.0f, 172 / 255.0f, 196 / 255.0f);
                // 原生代
                case GeologicalTimescale.Paleoproterozoic: return new Color(241 / 255.0f, 102 / 255.0f, 129 / 255.0f);
                case GeologicalTimescale.Mesoproterozoic: return new Color(251 / 255.0f, 187 / 255.0f, 125 / 255.0f);
                case GeologicalTimescale.Neoproterozoic: return new Color(252 / 255.0f, 186 / 255.0f, 97 / 255.0f);
                case GeologicalTimescale.Ediacaran: return new Color(0.8f*245 / 255.0f, 0.8f * 215 / 255.0f, 0.8f * 134 / 255.0f);
                // 古生代
                case GeologicalTimescale.Cambrian: return new Color(138 / 255.0f, 170 / 255.0f, 120 / 255.0f);
                case GeologicalTimescale.Ordovician: return new Color(0 / 255.0f, 167 / 255.0f, 142 / 255.0f);
                case GeologicalTimescale.Silurian: return new Color(178 / 255.0f, 221 / 255.0f, 202 / 255.0f);
                case GeologicalTimescale.Devonian: return new Color(206 / 255.0f, 156 / 255.0f, 90 / 255.0f);
                case GeologicalTimescale.Carboniferous: return new Color(104 / 255.0f, 174 / 255.0f, 177 / 255.0f);
                case GeologicalTimescale.Permian: return new Color(231 / 255.0f, 101 / 255.0f, 73 / 255.0f);
                // 中生代
                case GeologicalTimescale.Mesozonic: return new Color(71 / 255.0f, 199 / 255.0f, 234 / 255.0f);
                // 新生代
                case GeologicalTimescale.Cenozonic: return new Color(244 / 255.0f, 237 / 255.0f, 47 / 255.0f);
                // 完新世
                case GeologicalTimescale.Holocene: return new Color(0.5f*254 / 255.0f, 0.5f * 229 / 255.0f, 0.5f * 202 / 255.0f);
                // 終末代
                case GeologicalTimescale.Terminalean: return new Color(0 / 255.0f, 0 / 255.0f, 0 / 255.0f);
                default:
                    throw new NotImplementedException($"GeologicalTimescale {v} is not implemented");
            }
        }


    }
}