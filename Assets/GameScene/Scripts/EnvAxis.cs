using Evolia.Model;
using System.Collections.Generic;
using UnityEngine;

namespace Evolia.GameScene
{
    /// <summary>
    /// 環境グラフの軸
    /// </summary>
    public class EnvAxis : MonoBehaviour
    {
        [SerializeField]
        private EnvIcon envIconPrefab;

        [SerializeField]
        public bool yAxis;

        private List<EnvIcon> icons = new();

        public void SetMode(NicheAxis mode, float min = float.NaN, float max = float.NaN)
        {
            SetMode(mode.ToStatistics(), min, max);
        }

        public void SetMode(EnvStatisticsMode mode, float min = float.NaN, float max = float.NaN)
        {
            float minVal = mode.MinValue();
            float maxVal = mode.MaxValue();

            if (float.IsNaN(min))
            {
                min = minVal;
            }
            if (float.IsNaN(max))
            {
                max = maxVal;
            }

            for (int i = 0; i < 16; i++)
            {
                float val = minVal + (maxVal-minVal) * i / 16;

                if (icons.Count <= i)
                {
                    icons.Add(Instantiate(envIconPrefab, transform));
                }

                EnvIcon envIcon = icons[i];
                envIcon.SetEnv(mode, val);

                // val=min なら 0, val=max なら 1
                float p = (val - min) / (max - min);

                RectTransform iconTransform = envIcon.GetComponent<RectTransform>();
                if (yAxis)
                {
                    iconTransform.anchorMin = new Vector2(0.5f, p + 0.5f / 16);
                    iconTransform.anchorMax = new Vector2(0.5f, p + 0.5f / 16);
                }
                else
                {
                    iconTransform.anchorMin = new Vector2(p + 0.5f / 16, 0.5f);
                    iconTransform.anchorMax = new Vector2(p + 0.5f/16, 0.5f);
                }
            }
        }
    }

}

