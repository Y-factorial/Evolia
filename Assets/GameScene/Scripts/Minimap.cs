using Evolia.Model;
using Evolia.Shared;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Evolia.GameScene
{

    /// <summary>
    /// ミニマップ
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrder.AFTER_WORLDMAP)]
    public class Minimap : MonoBehaviour
    {
        [SerializeField]
        private GameController controller;

        [SerializeField]
        private PlanetSimulator simulator;

        [SerializeField]
        private UIDocument ui;

        [SerializeField]
        private PlanetView planetView;

        [SerializeField]
        private RawImage minimap;

        [SerializeField]
        private RectTransform scopeRect;

        [SerializeField]
        private RectTransform scopeRect2;

        private Planet planet => GameController.planet;

        public RawImage Image => minimap;

        public void Awake()
        {
            SetMipmap(minimap.material);
            minimap.texture = simulator.SurfaceTexture;
            minimap.uvRect = new Rect(0, 0, (float)(planet.size.x) / minimap.texture.width, (float)(planet.size.y) / minimap.texture.height);

            ui.rootVisualElement.Q("minimap").RegisterCallback<GeometryChangedEvent>(e =>
                StartCoroutine(ScreenUtils.Fit2UICoroutine(minimap.rectTransform, ui.rootVisualElement.Q("minimap")))
                );
        }

        private void SetMipmap(Material material)
        {
            Texture2D texture = (Texture2D)material.GetTexture("_TileTex");

            if (texture.mipmapCount == 1)
            {
                return;
            }


            int tileSize = Mathf.Max(texture.width / material.GetInt("_PaletteCountX"), texture.height / material.GetInt("_PaletteCountY"));

            int mipLevel = (int)Mathf.Log(tileSize, 2);
            GenerateMipMap(texture, mipLevel);
            texture.Apply(false);
        }

        private void GenerateMipMap(Texture2D targetTexture, int minimamMipLevel)
        {
            RenderTexture mip = ScaleTexture(targetTexture);
            for (int level = 1; level <= minimamMipLevel; ++level)
            {
                Graphics.CopyTexture(mip, 0, 0, targetTexture, 0, level);
                RenderTexture nextMip = ScaleTexture(mip);
                mip.Release();
                mip = nextMip;
            }
            mip.Release();

            targetTexture.requestedMipmapLevel = minimamMipLevel;
        }

        public RenderTexture ScaleTexture(Texture sourceTexture)
        {
            // 新しい解像度を計算
            int newWidth = sourceTexture.width / 2;
            int newHeight = sourceTexture.height / 2;

            // RenderTextureを準備
            RenderTexture renderTexture = new RenderTexture(newWidth, newHeight, 0);

            RenderTexture active = RenderTexture.active;
            RenderTexture.active = renderTexture;

            // 元のテクスチャを描画
            Graphics.Blit(sourceTexture, renderTexture);

            RenderTexture.active = active;
            return renderTexture;
        }



        public void Start()
        {
            UpdateScopeRect();
        }

        public void Update()
        {
            List<string> allKeywords = SmoothingMode.Vertex.Keywords();
            List<string> enabledKeywords = Options.smoothSurface.Keywords();

            foreach (string keyword in allKeywords)
            {
                if (enabledKeywords.Contains(keyword))
                {
                    minimap.material.EnableKeyword(keyword);
                }
                else
                {
                    minimap.material.DisableKeyword(keyword);
                }
            }

        }

        public void UpdateScopeRect()
        {
            // スクリーン座標での地図の左下と右上
            Vector3 slb = ScreenUtils.LocalToScreenPoint(planetView.gameObject, (Vector3)(planetView.GetComponent<RectTransform>().rect.min));
            Vector3 srt = ScreenUtils.LocalToScreenPoint(planetView.gameObject, (Vector3)(planetView.GetComponent<RectTransform>().rect.max));

            // カメラ位置
            Vector3 camLocal = ScreenUtils.ScreenToLocalPoint(planetView.gameObject, new Vector2(Screen.width / 2, Screen.height / 2));
            Vector2 scalePlanetLocalToUiLocal = minimap.rectTransform.rect.size / planetView.GetComponent<RectTransform>().rect.size;

            // 表示サイズ
            Vector2 viewSize = new Vector2(Screen.width, Screen.height);
            Vector2 scaleViewToUiLocal = minimap.rectTransform.rect.size / new Vector2(srt.x - slb.x, srt.y - slb.y);

            scopeRect.localPosition = camLocal * scalePlanetLocalToUiLocal;
            ScreenUtils.SetSize(scopeRect, viewSize * scaleViewToUiLocal);

            scopeRect2.localPosition = camLocal * scalePlanetLocalToUiLocal - new Vector2(minimap.rectTransform.rect.width, 0);
            ScreenUtils.SetSize(scopeRect2, viewSize * scaleViewToUiLocal);
        }

        public void OnClicked()
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();
            Vector2 localPosInMiniMap = ScreenUtils.ScreenToLocalPoint(minimap.gameObject, screenPos);

            // 左上が 0,0 で、右下が size.x, size.y になるような座標
            Vector2 focus = (localPosInMiniMap - minimap.rectTransform.rect.min) / minimap.rectTransform.rect.size * planet.size;

            controller.ScrollTo(new Vector2Int((int)focus.x, (int)focus.y), false);
        }

        public void OnScreenOrientationChange()
        {
            UpdateScopeRect();
        }
    }

}