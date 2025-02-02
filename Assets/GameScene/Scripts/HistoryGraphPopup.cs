using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Evolia.GameScene
{
    /// <summary>
    /// 履歴グラフのポップアップ
    /// </summary>
    public class HistoryGraphPopup : Popup
    {
        [SerializeField]
        private TMP_Text subTitleLabel;

        [SerializeField]
        private TMP_Dropdown modeDropdown;

        [SerializeField]
        private TMP_Dropdown timeRangeDropdown;

        [SerializeField]
        private List<HistoryGraph> graphs;

        public override void Show()
        {
            gameObject.SetActive(true);

            CreateContent();
        }

        public override void Hide()
        {
            gameObject.SetActive(false);
        }


        private void CreateContent()
        {
            HistoryGraphMode mode = GetHistoryMode();
            
            subTitleLabel.text = $" ― {mode.Label()}";

            foreach (HistoryGraph graph in graphs)
            {
                bool show = graph.mode == mode;

                graph.gameObject.SetActive(show);
                if (show)
                {
                    graph.xRange = GetTimeRange();
                    graph.UpdateContent();
                }
            }
        }

        private long GetTimeRange()
        {
            switch (timeRangeDropdown.value)
            {
                case 0:
                    return 0;
                case 1:
                    return 3600;
                case 2:
                    return 600;
                case 3:
                    return 60;
                default:
                    throw new NotImplementedException($"TimeRange {timeRangeDropdown.value} is not implemented");
            }
        }

        private HistoryGraphMode GetHistoryMode()
        {
            switch (modeDropdown.value)
            {
                case 0:
                    return HistoryGraphMode.Atmosphere;
                case 1:
                    return HistoryGraphMode.Temperature;
                case 2:
                    return HistoryGraphMode.Metabolism;
                case 3:
                    return HistoryGraphMode.Population;
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