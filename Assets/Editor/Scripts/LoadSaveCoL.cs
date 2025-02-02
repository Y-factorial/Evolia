using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Evolia.Model;
using Evolia.GameScene;
using System.Linq;
using System.IO;

namespace Evolia.Editor
{

    /// <summary>
    /// 生物コレクションをロード、セーブする。
    /// </summary>
    public class LoadSaveCoL : MonoBehaviour
    {
        /// <summary>
        /// メニューが有効かの判定。
        /// テキストファイルならOKとする。
        /// </summary>
        /// <returns></returns>
        [MenuItem("Assets/Evolia/Load CoL", true)]
        [MenuItem("Assets/Evolia/Save CoL", true)]
        public static bool ValidateLoadPreferences()
        {
            return Selection.activeObject is TextAsset;
        }

        [MenuItem("Assets/Evolia/Load CoL")]
        public static void LoadCoL()
        {
            // いったん古いリストをクリア
            CoL.instance.Clear();

            List<Dictionary<string, string>> datasheet = new(LoadTsv((Selection.activeObject as TextAsset).text));

            // まずリストの順に生物を追加
            // 進化先がリスト内に確実にあることを保証するため。
            foreach (Dictionary<string, string> props in datasheet)
            {
                CoL.instance.Add(props["name"]);
            }

            // それからプロパティを読み取る
            foreach (Dictionary<string, string> props in datasheet)
            {
                PropsToSpec(props, ref CoL.instance[props["name"]]);
            }

            // 逆向きの突然変異を設定する
            foreach (Dictionary<string, string> props in datasheet)
            {
                SetReverseTransform(CoL.instance[props["name"]]);

            }

            EditorUtility.SetDirty(CoL.instance);
            AssetDatabase.SaveAssets();
        }

        private static IEnumerable<Dictionary<string,string>> LoadTsv(string text)
        {
            List<string> header = null;

            foreach (List<string> values in 
                text.Split('\n') // 改行区切りで
                .Select(line=>line.Split('\t').Select(value=>value.Trim()).ToList()) // 各行をタブ区切りで、前後の空白を除去
                .Where(values=>values.Count!=0 && values[0]!="") // 空行を除去
                )
            {
                if (header == null)
                {
                    // 最初の行はヘッダ
                    header = values;
                }
                else
                {
                    // それ以外はデータ
                    yield return header.Zip(values, (key, value) => new { key, value }).ToDictionary(prop => prop.key, prop => prop.value);
                }
            }

        }


        private static void PropsToSpec(Dictionary<string, string> props, ref Species spec)
        {
            spec.layer = Enum.Parse<LifeLayer>(props["layer"]);
            spec.timescale = Enum.Parse<GeologicalTimescale>(props["timescale"]);
            spec.palette = int.Parse(props["palette"]);
            spec.scale = float.Parse(props["scale"]);
            spec.variantType = int.Parse(props["variantType"]);

            spec.competitiveness = float.Parse(props["competitiveness"]);
            spec.evolutionSpeed = float.Parse(props["evolutionSpeed"]);

            spec.growthSpeed = float.Parse(props["growthSpeed"]);
            spec.reproductAt = float.Parse(props["reproductAt"]);
            spec.mobility = float.Parse(props["mobility"]);
            spec.densityCapacity = float.Parse(props["densityCapacity"]);

            spec.produce.soil = float.Parse(props["produceSoil"]);
            spec.produce.leaves = float.Parse(props["produceLeaves"]);
            spec.produce.honey = float.Parse(props["produceHoney"]);
            spec.produce.fruits = float.Parse(props["produceFruits"]);
            spec.produce.meats = float.Parse(props["produceMeats"]);

            spec.consume.soil = float.Parse(props["consumeSoil"]);
            spec.consume.leaves = float.Parse(props["consumeLeaves"]);
            spec.consume.honey = float.Parse(props["consumeHoney"]);
            spec.consume.fruits = float.Parse(props["consumeFruits"]);
            spec.consume.meats = float.Parse(props["consumeMeats"]);

            spec.photosynthesis = float.Parse(props["photosynthesis"]);
            spec.respiration = float.Parse(props["respiration"]);

            spec.preferences[(int)NicheAxis.Elevation][0] = new Vector2(
                float.Parse(props["elevationPref0"]),
                float.Parse(props["elevationVal0"])
                );
            spec.preferences[(int)NicheAxis.Elevation][1] = new Vector2(
                float.Parse(props["elevationPref1"]),
                float.Parse(props["elevationVal1"])
                );
            spec.preferences[(int)NicheAxis.Elevation][2] = new Vector2(
                float.Parse(props["elevationPref2"]),
                float.Parse(props["elevationVal2"])
                );
            spec.preferences[(int)NicheAxis.Elevation][3] = new Vector2(
                float.Parse(props["elevationPref3"]),
                float.Parse(props["elevationVal3"])
                );

            spec.preferences[(int)NicheAxis.O2][0] = new Vector2(
                float.Parse(props["o2Pref0"]),
                float.Parse(props["o2Val0"])
                );
            spec.preferences[(int)NicheAxis.O2][1] = new Vector2(
                float.Parse(props["o2Pref1"]),
                float.Parse(props["o2Val1"])
                );
            spec.preferences[(int)NicheAxis.O2][2] = new Vector2(
                float.Parse(props["o2Pref2"]),
                float.Parse(props["o2Val2"])
                );
            spec.preferences[(int)NicheAxis.O2][3] = new Vector2(
                float.Parse(props["o2Pref3"]),
                float.Parse(props["o2Val3"])
                );

            spec.preferences[(int)NicheAxis.Temperature][0] = new Vector2(
                float.Parse(props["temperaturePref0"]) + PhysicalConstants.CelsiusZero,
                float.Parse(props["temperatureVal0"])
                );
            spec.preferences[(int)NicheAxis.Temperature][1] = new Vector2(
                float.Parse(props["temperaturePref1"]) + PhysicalConstants.CelsiusZero,
                float.Parse(props["temperatureVal1"])
                );
            spec.preferences[(int)NicheAxis.Temperature][2] = new Vector2(
                float.Parse(props["temperaturePref2"]) + PhysicalConstants.CelsiusZero,
                float.Parse(props["temperatureVal2"])
                );
            spec.preferences[(int)NicheAxis.Temperature][3] = new Vector2(
                float.Parse(props["temperaturePref3"]) + PhysicalConstants.CelsiusZero,
                float.Parse(props["temperatureVal3"])
                );

            spec.preferences[(int)NicheAxis.Humidity][0] = new Vector2(
                float.Parse(props["humidityPref0"]),
                float.Parse(props["humidityVal0"])
                );
            spec.preferences[(int)NicheAxis.Humidity][1] = new Vector2(
                float.Parse(props["humidityPref1"]),
                float.Parse(props["humidityVal1"])
                );
            spec.preferences[(int)NicheAxis.Humidity][2] = new Vector2(
                float.Parse(props["humidityPref2"]),
                float.Parse(props["humidityVal2"])
                );
            spec.preferences[(int)NicheAxis.Humidity][3] = new Vector2(
                float.Parse(props["humidityPref3"]),
                float.Parse(props["humidityVal3"])
                );

            spec.transforms[0] = props["transform0"] != "" ? CoL.instance[props["transform0"]].id : 0;
            spec.transforms[1] = props["transform1"] != "" ? CoL.instance[props["transform1"]].id : 0;
            spec.transforms[2] = props["transform2"] != "" ? CoL.instance[props["transform2"]].id : 0;
            spec.transforms[3] = props["transform3"] != "" ? CoL.instance[props["transform3"]].id : 0;

            spec.uniteWith = props["uniteWith"] != "" ? CoL.instance[props["uniteWith"]].id : 0;
            spec.uniteTo = props["uniteTo"] != "" ? CoL.instance[props["uniteTo"]].id : 0;
        }

        private static void SetReverseTransform(in Species spec)
        {

            // 全ての突然変異について
            for (int i = 0; i < spec.transforms.Length; ++i)
            {
                // 前向きの進化なら
                if (spec.transforms[i] > spec.id)
                {
                    ref Species next = ref CoL.instance[spec.transforms[i]];

                    // 進化先の種に逆向きの突然変異を追加
                    for (int j = 0; j < next.transforms.Length; ++j)
                    {
                        if (next.transforms[j] == 0)
                        {
                            next.transforms[j] = spec.id;
                            break;
                        }
                    }
                }
            }

            if (spec.uniteTo != 0)
            {
                // 融合があるなら

                ref Species next = ref CoL.instance[spec.uniteTo];

                // 融合先の生物に逆向きの突然変異を追加
                for (int j = 0; j < next.transforms.Length; ++j)
                {
                    if (next.transforms[j] == 0)
                    {
                        next.transforms[j] = spec.id;
                        break;
                    }
                }
            }
        }

        [MenuItem("Assets/Evolia/Save CoL")]
        public static void SaveCoL()
        {
            // 全てのデータをTSV形式で保存
            List<Dictionary<string, string>> datasheet = new(CoL.instance.species.Select(spec => SpecToProps(spec)));

            // ヘッダを保存
            List<string> header = datasheet[0].Keys.ToList();

            List<string> lines = new();

            // ヘッダをタブ区切りで
            lines.Add(string.Join("\t", header));
            // 各行をヘッダと同じ順番でタブ区切りで
            lines.AddRange(datasheet.Select(props => string.Join("\t", header.Select(key => props[key]))));

            File.WriteAllLines(AssetDatabase.GetAssetPath(Selection.activeObject), lines);

        }

        private static Dictionary<string, string> SpecToProps(in Species spec)
        {
            Dictionary<string, string> props = new()
            {
                ["name"] = CoL.instance.names[spec.id],
                ["layer"] = spec.layer.ToString(),
                ["timescale"] = spec.timescale.ToString(),
                ["palette"] = spec.palette.ToString(),
                ["scale"] = spec.scale.ToString(),
                ["variantType"] = spec.variantType.ToString(),

                ["competitiveness"] = spec.competitiveness.ToString(),
                ["evolutionSpeed"] = spec.evolutionSpeed.ToString("0.########"),

                ["growthSpeed"] = spec.growthSpeed.ToString(),
                ["reproductAt"] = spec.reproductAt.ToString(),
                ["mobility"] = spec.mobility.ToString(),
                ["densityCapacity"] = spec.densityCapacity.ToString(),

                ["produceSoil"] = spec.produce.soil.ToString(),
                ["produceLeaves"] = spec.produce.leaves.ToString(),
                ["produceHoney"] = spec.produce.honey.ToString(),
                ["produceFruits"] = spec.produce.fruits.ToString(),
                ["produceMeats"] = spec.produce.meats.ToString(),

                ["consumeSoil"] = spec.consume.soil.ToString(),
                ["consumeLeaves"] = spec.consume.leaves.ToString(),
                ["consumeHoney"] = spec.consume.honey.ToString(),
                ["consumeFruits"] = spec.consume.fruits.ToString(),
                ["consumeMeats"] = spec.consume.meats.ToString(),

                ["photosynthesis"] = spec.photosynthesis.ToString(),
                ["respiration"] = spec.respiration.ToString(),

                ["elevationPref0"] = spec.preferences[(int)NicheAxis.Elevation][0].x.ToString(),
                ["elevationPref1"] = spec.preferences[(int)NicheAxis.Elevation][1].x.ToString(),
                ["elevationPref2"] = spec.preferences[(int)NicheAxis.Elevation][2].x.ToString(),
                ["elevationPref3"] = spec.preferences[(int)NicheAxis.Elevation][3].x.ToString(),
                ["elevationVal0"] = spec.preferences[(int)NicheAxis.Elevation][0].y.ToString(),
                ["elevationVal1"] = spec.preferences[(int)NicheAxis.Elevation][1].y.ToString(),
                ["elevationVal2"] = spec.preferences[(int)NicheAxis.Elevation][2].y.ToString(),
                ["elevationVal3"] = spec.preferences[(int)NicheAxis.Elevation][3].y.ToString(),
                ["o2Pref0"] = spec.preferences[(int)NicheAxis.O2][0].x.ToString(),
                ["o2Pref1"] = spec.preferences[(int)NicheAxis.O2][1].x.ToString(),
                ["o2Pref2"] = spec.preferences[(int)NicheAxis.O2][2].x.ToString(),
                ["o2Pref3"] = spec.preferences[(int)NicheAxis.O2][3].x.ToString(),
                ["o2Val0"] = spec.preferences[(int)NicheAxis.O2][0].y.ToString(),
                ["o2Val1"] = spec.preferences[(int)NicheAxis.O2][1].y.ToString(),
                ["o2Val2"] = spec.preferences[(int)NicheAxis.O2][2].y.ToString(),
                ["o2Val3"] = spec.preferences[(int)NicheAxis.O2][3].y.ToString(),
                ["temperaturePref0"] = (spec.preferences[(int)NicheAxis.Temperature][0].x - PhysicalConstants.CelsiusZero).ToString(),
                ["temperaturePref1"] = (spec.preferences[(int)NicheAxis.Temperature][1].x - PhysicalConstants.CelsiusZero).ToString(),
                ["temperaturePref2"] = (spec.preferences[(int)NicheAxis.Temperature][2].x - PhysicalConstants.CelsiusZero).ToString(),
                ["temperaturePref3"] = (spec.preferences[(int)NicheAxis.Temperature][3].x - PhysicalConstants.CelsiusZero).ToString(),
                ["temperatureVal0"] = spec.preferences[(int)NicheAxis.Temperature][0].y.ToString(),
                ["temperatureVal1"] = spec.preferences[(int)NicheAxis.Temperature][1].y.ToString(),
                ["temperatureVal2"] = spec.preferences[(int)NicheAxis.Temperature][2].y.ToString(),
                ["temperatureVal3"] = spec.preferences[(int)NicheAxis.Temperature][3].y.ToString(),
                ["humidityPref0"] = spec.preferences[(int)NicheAxis.Humidity][0].x.ToString(),
                ["humidityPref1"] = spec.preferences[(int)NicheAxis.Humidity][1].x.ToString(),
                ["humidityPref2"] = spec.preferences[(int)NicheAxis.Humidity][2].x.ToString(),
                ["humidityPref3"] = spec.preferences[(int)NicheAxis.Humidity][3].x.ToString(),
                ["humidityVal0"] = spec.preferences[(int)NicheAxis.Humidity][0].y.ToString(),
                ["humidityVal1"] = spec.preferences[(int)NicheAxis.Humidity][1].y.ToString(),
                ["humidityVal2"] = spec.preferences[(int)NicheAxis.Humidity][2].y.ToString(),
                ["humidityVal3"] = spec.preferences[(int)NicheAxis.Humidity][3].y.ToString(),

                ["transform0"] = spec.transforms[0] > spec.id ? CoL.instance.names[spec.transforms[0]] : "",
                ["transform1"] = spec.transforms[1] > spec.id ? CoL.instance.names[spec.transforms[1]] : "",
                ["transform2"] = spec.transforms[2] > spec.id ? CoL.instance.names[spec.transforms[2]] : "",
                ["transform3"] = spec.transforms[3] > spec.id ? CoL.instance.names[spec.transforms[3]] : "",
                ["uniteWith"] = spec.uniteWith != 0 ? CoL.instance.names[spec.uniteWith] : "",
                ["uniteTo"] = spec.uniteTo != 0 ? CoL.instance.names[spec.uniteTo] : ""
            };

            return props;
        }
    }


}