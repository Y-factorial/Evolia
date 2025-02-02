using Evolia.Shared;
using Evolia.Model;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Evolia.GameScene
{
    /// <summary>
    /// 惑星の地表や生物を表示する
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrder.WORLDMAP)]
    public class PlanetView : MonoBehaviour
    {
        [SerializeField]
        private PlanetSimulator simulator;

        [SerializeField]
        private RawImage surfaceMap;

        [SerializeField]
        private RawImage lifeMap;

        [SerializeField]
        private RawImage overlayMap;

        [SerializeField]
        private RectTransform focusRect;

        [SerializeField]
        private RawImage surfaceMap2;

        [SerializeField]
        private RawImage lifeMap2;

        [SerializeField]
        private RawImage overlayMap2;

        [SerializeField]
        private RectTransform focusRect2;

        [SerializeField]
        public Vector2 tileSize = new Vector2(32, 32);

        private Planet planet => GameController.planet;

        public void Awake()
        {
            // テクスチャをもらっておく

            surfaceMap.texture = simulator.SurfaceTexture;
            surfaceMap.uvRect = new Rect(0, 0, (float)(planet.size.x) / surfaceMap.texture.width, (float)(planet.size.y) / surfaceMap.texture.height);
            ScreenUtils.SetSize(surfaceMap.rectTransform, planet.size * tileSize);

            lifeMap.texture = simulator.LifeTexture;
            lifeMap.uvRect = new Rect(0, 0, (float)(planet.size.x) / lifeMap.texture.width, (float)(planet.size.y) / lifeMap.texture.height);
            ScreenUtils.SetSize(lifeMap.rectTransform, planet.size * tileSize);

            overlayMap.texture = simulator.OverlayTexture;
            overlayMap.uvRect = new Rect(0, 0, (float)(planet.size.x) / overlayMap.texture.width, (float)(planet.size.y) / overlayMap.texture.height);
            ScreenUtils.SetSize(overlayMap.rectTransform, planet.size * tileSize);

            // ここから下は無限スクロール用
            // メインのイメージの右にもう一つ同じものを置いておく

            surfaceMap2.texture = surfaceMap.texture;
            surfaceMap2.uvRect = surfaceMap.uvRect;
            ScreenUtils.SetSize(surfaceMap2.rectTransform, planet.size * tileSize);
            surfaceMap2.transform.localPosition = new Vector3(surfaceMap.rectTransform.rect.width, 0, 0);

            lifeMap2.texture = lifeMap.texture;
            lifeMap2.uvRect = lifeMap.uvRect;
            ScreenUtils.SetSize(lifeMap2.rectTransform, planet.size * tileSize);
            lifeMap2.transform.localPosition = new Vector3(lifeMap.rectTransform.rect.width, 0, 0);

            overlayMap2.texture = overlayMap.texture;
            overlayMap2.uvRect = overlayMap.uvRect;
            ScreenUtils.SetSize(overlayMap2.rectTransform, planet.size * tileSize);
            overlayMap2.transform.localPosition = new Vector3(overlayMap.rectTransform.rect.width, 0, 0);

        }

        public void Update()
        {
            surfaceMap.material.SetFloat("_Timer", (float)(Time.timeAsDouble % 60));
            lifeMap.material.SetFloat("_Timer", (float)(Time.timeAsDouble % 60));

            List<string> allKeywords = SmoothingMode.Vertex.Keywords();
            List<string> enabledKeywords = Options.smoothSurface.Keywords();

            foreach (string keyword in allKeywords)
            {
                if (enabledKeywords.Contains(keyword))
                {
                    surfaceMap.material.EnableKeyword(keyword);
                }
                else
                {
                    surfaceMap.material.DisableKeyword(keyword);
                }
            }

            // 画面の中央は 0 ～ size の範囲になるべき

            Vector2 leftCenter = ScreenUtils.WorldToScreenPoint(surfaceMap.rectTransform, surfaceMap.rectTransform.position);
            Vector2 rightCenter = ScreenUtils.WorldToScreenPoint(surfaceMap2.rectTransform, surfaceMap2.rectTransform.position);
            if (Screen.width/2 < leftCenter.x)
            {
                // 世界が右に行き過ぎているので、左に戻す
                float dx = ScreenUtils.ScreenToWorldPoint(surfaceMap.rectTransform, leftCenter).x - ScreenUtils.ScreenToWorldPoint(surfaceMap.rectTransform, rightCenter).x;

                surfaceMap.rectTransform.position += new Vector3(dx, 0, 0);
            }
            else if (Screen.width/2 > rightCenter.x)
            {
                float dx = ScreenUtils.ScreenToWorldPoint(surfaceMap.rectTransform, rightCenter).x - ScreenUtils.ScreenToWorldPoint(surfaceMap.rectTransform, leftCenter).x;

                surfaceMap.rectTransform.position += new Vector3(dx, 0, 0);
            }
        }

        public void SetFocus(Vector2Int focus)
        {
            // 0 -> size が min->max になるような座標

            focusRect.localPosition = new Vector3(
                (focus.x - planet.size.x / 2 + 0.5f) * surfaceMap.rectTransform.rect.width / planet.size.x,
                (focus.y - planet.size.y / 2 + 0.5f) * surfaceMap.rectTransform.rect.height / planet.size.y, 0);

            focusRect2.transform.localPosition = focusRect.localPosition + new Vector3(surfaceMap.rectTransform.rect.width, 0, 0);
        }

        public void ShowOverlay(bool v)
        {
            overlayMap.gameObject.SetActive(v);
            overlayMap2.gameObject.SetActive(v);
        }

        public void ShowLife(bool v)
        {
            lifeMap.gameObject.SetActive(v);
            lifeMap2.gameObject.SetActive(v);
        }
    }

}