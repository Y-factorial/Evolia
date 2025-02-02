using Evolia.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Evolia.GameScene
{
    /// <summary>
    /// 環境統計情報を表示するポップアップ
    /// </summary>
    public class EnvStatisticsPopup : Popup
    {
        [SerializeField]
        private PlanetSimulator simulator;

        [SerializeField]
        private TMP_Text subTitleLabel;

        [SerializeField]
        private TMP_Dropdown modeDropdown;

        [SerializeField]
        private GraphBackground background;

        [SerializeField]
        private LineGraph graph;

        [SerializeField]
        private EnvAxis axis;

        [SerializeField]
        private float minValue = 5;

        [SerializeField]
        private float marginFactor = 1.05f;

        [SerializeField]
        private float smoothingFactor = 0.1f;

        private Planet planet => GameController.planet;

        public override void Show()
        {
            gameObject.SetActive(true);

            CreateContent();
        }

        public override void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Update()
        {
            // スムージングしたいので、Updateのタイミングでも更新
            UpdateGraphMax(true);
        }

        public void OnTick()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            // 値が変わったはずなので、グラフを更新
            UpdateContent(true);
        }

        private void CreateContent()
        {
            EnvStatisticsMode statisticsMode = GetStatisticsMode();

            subTitleLabel.text = $" ― {statisticsMode.Label()}";

            axis.SetMode(statisticsMode);
            graph.color = statisticsMode.Color();

            background.unit = "%";

            UpdateContent(false);

        }

        private uint[] values = new uint[PlanetSimulator.ENV_STATISTICS_SIZE];

        private float prevMax = 10;

        private void UpdateContent(bool smoothing)
        {

            EnvStatisticsMode statisticsMode = GetStatisticsMode();

            this.simulator.ReadEnvStatistics(this.values, statisticsMode, () =>
            {
                if (this == null)
                {
                    // 非同期処理が終わった後に、このオブジェクトが破棄されている可能性がある
                    return;
                }

                graph.points = ValueToPercent(values);

                UpdateGraphMax(smoothing);

                graph.SetVerticesDirty();
            });
        }

        private void UpdateGraphMax(bool smoothing)
        {
            if (graph.points == null)
            {
                return;
            }

            float max = Mathf.Max(minValue,
                graph.points.Skip(1).Take(graph.points.Count() - 2).Select(pt=>pt.y)
                .DefaultIfEmpty(0).Max())*marginFactor;

            if (smoothing)
            {
                max = Mathf.Lerp(prevMax, max, smoothingFactor);
            }

            prevMax = max;

            graph.xMin = 0;
            graph.xMax = PlanetSimulator.ENV_STATISTICS_SIZE - 1;
            graph.yMin = 0;
            graph.yMax = max;

            background.SetMax(max);
        }

        private IEnumerable<Vector2> ValueToPercent(uint[] result)
        {
            for (int i = 0; i < result.Length; ++i)
            {
                yield return new Vector2(i, result[i]* 100.0f / (planet.size.x * planet.size.y) );
            }
        }


        private EnvStatisticsMode GetStatisticsMode()
        {
            switch (modeDropdown.value)
            {
                case 0:
                    return EnvStatisticsMode.Elevation;
                case 1:
                    return EnvStatisticsMode.Temperature;
                case 2:
                    return EnvStatisticsMode.Humidity;
                default:
                    throw new NotImplementedException($"HistoryMode {modeDropdown.value} is not implemented");
            }
        }

        public void OnParamChanged()
        {
            CreateContent();
        }

        public void OnClickClose()
        {
            Hide();
        }

    }

}