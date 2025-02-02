using UnityEngine;
using UnityEngine.UI;

namespace Evolia.Shared
{
    public static class GraphicsUtils
    {
        public static void DrawLine(VertexHelper vh, Vector2 start, Vector2 end, float width, Color color)
        {
            Vector2 perpendicular = (new Vector2(-(end.y - start.y), end.x - start.x)).normalized * (width / 2);

            // 四角形の4つの頂点
            Vector2 v0 = start - perpendicular;
            Vector2 v1 = start + perpendicular;
            Vector2 v2 = end - perpendicular;
            Vector2 v3 = end + perpendicular;

            int idx = vh.currentVertCount;

            // 頂点を追加
            vh.AddVert(v0, color, Vector2.zero);
            vh.AddVert(v1, color, Vector2.zero);
            vh.AddVert(v2, color, Vector2.zero);
            vh.AddVert(v3, color, Vector2.zero);

            // 三角形を追加
            vh.AddTriangle(idx + 0, idx + 1, idx + 2);
            vh.AddTriangle(idx + 2, idx + 1, idx + 3);
        }
    }
}