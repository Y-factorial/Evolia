using Evolia.Shared;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Evolia.GameScene
{
    /// <summary>
    /// 折れ線グラフ
    /// </summary>
    public class LineGraph : MaskableGraphic
    {
        // 線の太さ
        [SerializeField]
        public float lineWidth = 4f;

        public IEnumerable<Vector2> points;

        public float xMin;
        public float xMax;

        public float yMin;
        public float yMax;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (points == null)
            {
                return;
            }

            Vector2? prev = null;

            foreach (Vector2 p in points)
            {
                if (prev.HasValue)
                {
                    if ( (prev.Value.x >= xMin && prev.Value.x < xMax && prev.Value.y >= yMin && prev.Value.y < yMax) ||
                        (p.x >= xMin && p.x < xMax && p.y >= yMin && p.y < yMax))
                    {
                        Vector2 start = TransformToLocal(prev.Value, xMin, xMax, yMin, yMax);
                        Vector2 end = TransformToLocal(p, xMin, xMax, yMin, yMax);
                        GraphicsUtils.DrawLine(vh, start, end, lineWidth, color);
                    }
                }

                prev = p;
            }
        }

        private Vector2 TransformToLocal(Vector2 point, float xMin, float xMax, float yMin, float yMax)
        {
            // 座標をローカルのRectTransformサイズに合わせる
            float x = (point.x - xMin) / (xMax - xMin) * rectTransform.rect.width;
            float y = (point.y - yMin) / (yMax - yMin) * rectTransform.rect.height;
            return new Vector2(x, y);
        }
    }
}