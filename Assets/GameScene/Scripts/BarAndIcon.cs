using UnityEngine;

namespace Evolia.GameScene
{
    /// <summary>
    /// 棒グラフに使う棒とアイコン
    /// </summary>
    public class BarAndIcon : MonoBehaviour
    {
        [SerializeField]
        private RectTransform bar;

        [SerializeField]
        private LifeIcon icon;

        [SerializeField]
        private bool horizontal;

        public float Length
        {
            get
            {
                // 横向きなら長さは幅、そうでないなら高さ
                return horizontal ? bar.anchorMax.x : bar.anchorMax.y;
            }
            set
            {
                // 横向きなら長さは幅、そうでないなら高さ
                bar.gameObject.SetActive(value > 0);
                if (horizontal)
                {
                    bar.anchorMax = new Vector2(value, bar.anchorMax.y);
                }
                else
                {
                    bar.anchorMax = new Vector2(bar.anchorMax.x, value);
                }
            }
        }

        public LifeIcon Icon => icon;

        public float IconSpace
        {
            get
            {
                RectTransform barRange = bar.parent.GetComponent<RectTransform>();
                if (horizontal)
                {
                    return barRange.offsetMin.x;
                }
                else
                {
                    return barRange.offsetMin.y;
                }
            }
        }
    }

}