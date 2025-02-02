using UnityEngine;
using UnityEngine.UI;

namespace Evolia.GameScene
{
    /// <summary>
    /// ニッチの領域を表示する
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class NicheArea : MonoBehaviour
    {
        [SerializeField]
        private Image center;
        [SerializeField]
        private Image top;
        [SerializeField]
        private Image right;
        [SerializeField]
        private Image bottom;
        [SerializeField]
        private Image left;
        [SerializeField]
        private Image topLeft;
        [SerializeField]
        private Image topRight;
        [SerializeField]
        private Image bottomRight;
        [SerializeField]
        private Image bottomLeft;

        public void SetColor(Color color)
        {
            center.color = color;
            top.color = color;
            right.color = color;
            bottom.color = color;
            left.color = color;
            topLeft.color = color;
            topRight.color = color;
            bottomRight.color = color;
            bottomLeft.color = color;

        }
        public void SetArea(float[] xs, float[] ys)
        {
            transform.localPosition = new Vector3( (xs[0]+xs[3])/2, (ys[0]+ys[3]) / 2, 0);
            GetComponent<RectTransform>().sizeDelta = new Vector3(xs[3] - xs[0], ys[3] - ys[0], 0);

            float xMin = (xs[1] - xs[0]) / (xs[3] - xs[0]);
            float yMin = (ys[1] - ys[0]) / (ys[3] - ys[0]);
            float xMax = (xs[2] - xs[0]) / (xs[3] - xs[0]);
            float yMax = (ys[2] - ys[0]) / (ys[3] - ys[0]);

            center.rectTransform.anchorMin = new Vector2(xMin, yMin);
            center.rectTransform.anchorMax = new Vector2(xMax, yMax);

            top.rectTransform.anchorMin = new Vector2(xMin, yMax);
            top.rectTransform.anchorMax = new Vector2(xMax, 1);

            right.rectTransform.anchorMin = new Vector2(xMax, yMin);
            right.rectTransform.anchorMax = new Vector2(1, yMax);

            bottom.rectTransform.anchorMin = new Vector2(xMin, 0);
            bottom.rectTransform.anchorMax = new Vector2(xMax, yMin);

            left.rectTransform.anchorMin = new Vector2(0, yMin);
            left.rectTransform.anchorMax = new Vector2(xMin, yMax);

            topLeft.rectTransform.anchorMax = new Vector2(xMin, 1);
            topLeft.rectTransform.anchorMin = new Vector2(0, yMax);

            topRight.rectTransform.anchorMin = new Vector2(xMax, yMax);

            bottomLeft.rectTransform.anchorMax = new Vector2(xMin, yMin);

            bottomRight.rectTransform.anchorMin = new Vector2(xMax, 0);
            bottomRight.rectTransform.anchorMax = new Vector2(1, yMin);
        }
    }

}