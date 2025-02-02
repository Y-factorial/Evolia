using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Evolia.GameScene
{
    /// <summary>
    /// グラフの背景
    /// </summary>
    public class GraphBackground : MonoBehaviour
    {
        [SerializeField]
        List<RectTransform> landscapeGridlines;

        [SerializeField]
        List<RectTransform> portraitGridlines;

        public string unit = "";

        public bool landscape;

        public void Start()
        {
            foreach(RectTransform gridline in landscapeGridlines)
            {
                gridline.gameObject.SetActive(landscape);
            }
            foreach (RectTransform gridline in portraitGridlines)
            {
                gridline.gameObject.SetActive(!landscape);
            }
        }

        public void SetMax(float max)
        {
            List<RectTransform> gridlines = landscape ? landscapeGridlines : portraitGridlines;

            // 10, 5, 2, 1 のうち、max を超える最大値を取る

            float[] measures = GetMinimumMeasure(max, 0.1f);

            for(int i=0;i<measures.Length; ++i)
            {
                float ratio = measures[i] / max;
                Color color = new Color(0, 0, 0, Mathf.Sqrt(Mathf.Clamp( (ratio-0.1f)/0.5f, 0, 1)));
                string label = measures[i].ToString(measures[i] < 1 ? "#,0.0##" : "#,0")+ unit;

                foreach(TMP_Text text in gridlines[i].GetComponentsInChildren<TMP_Text>())
                {
                    text.text = label;
                    text.color = color;
                }
                foreach (Image image in gridlines[i].GetComponentsInChildren<Image>())
                {
                    image.color = color;
                }

                gridlines[i].anchorMin = landscape ? new Vector2(0, ratio) : new Vector2(ratio, 0);
                gridlines[i].anchorMax = landscape ? new Vector2(1, ratio) : new Vector2(ratio, 1);
            }
        }

        private float[] GetMinimumMeasure(float max, float d)
        {
            if (max < d)
            {
                return new float[] { d, d / 2, d / 5, d / 10 };
            }
            else if (max < d * 2)
            {
                return new float[] { d * 2, d, d / 2, d / 5 };
            }
            else if (max < d * 5)
            {
                return new float[] { d * 5, d * 2, d, d / 2 };
            }
            else
            {
                return GetMinimumMeasure(max, d * 10);
            }
        }
    }


}