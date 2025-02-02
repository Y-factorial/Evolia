using Evolia.Model;
using Evolia.Shared;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Evolia.GameScene
{
    /// <summary>
    /// 今選択中のタイルの情報を表示するUI
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrder.AFTER_WORLDMAP)]
    public class TileInfo : MonoBehaviour
    {
        [SerializeField]
        private PlanetSimulator simulator;

        [SerializeField]
        private GameController controller;

        [SerializeField]
        private UIDocument ui;

        [SerializeField]
        private RectTransform tilePreview;

        [SerializeField]
        private RawImage surfacePreview;

        [SerializeField]
        private RawImage lifePreview;

        [SerializeField]
        private SpeciesInfoPopup speciesInfoPopup;

        private Planet planet => GameController.planet;

        // 表示しているタイルの詳細情報
        private TileDetails tileDetails;

        private ref Tile tile => ref tileDetails.tile;

        public void Awake()
        {
            // 地表と生物のテクスチャをもらっておく
            surfacePreview.texture = simulator.SurfaceTexture;
            lifePreview.texture = simulator.LifeTexture;

            // タイル画像の位置にフィッティングする
            ui.rootVisualElement.Q("tile-image").RegisterCallback<GeometryChangedEvent>(e =>
                StartCoroutine(ScreenUtils.Fit2UICoroutine(tilePreview, ui.rootVisualElement.Q("tile-image")))
                );

            // 幸福度をクリックされたら生物の詳細ポップアップを表示する
            ui.rootVisualElement.Q("tile-happiness").RegisterCallback<ClickEvent>((evt) => OnClickHappiness());

        }

        public void Update()
        {
            if (this.ui.rootVisualElement.Q("tile-happiness").resolvedStyle.opacity == 0)
            {
                // 幸福度が見えなくなったので非表示に
                // opacity=0 だけだとクリックイベントが発生してしまうので display=none にする
                this.ui.rootVisualElement.Q("tile-happiness").style.display = DisplayStyle.None;
            }
            else
            {
                this.ui.rootVisualElement.Q("tile-happiness").style.display = DisplayStyle.Flex;

            }
        }

        private void OnClickHappiness()
        {
            // 今のタイルに生物がいるなら、その生物の詳細情報を表示する

            ref Life life = ref tile.lives[tileDetails.visibleLayer];

            if (life.Exists)
            {
                speciesInfoPopup.Show(CoL.instance.species[life.species],life.variant);
            }
        }

        public void OnClicked()
        {
            // クリックされたら、そのタイルを画面中央に表示する

            controller.ScrollTo(controller.GetFocus(), true);
        }

        public void OnTick()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            UpdateTileInfo();
        }


        public void OnFocusChanged()
        {
            Vector2Int focus = controller.GetFocus();

            // 地表と生物の表示位置を更新
            surfacePreview.uvRect = new Rect((float)(focus.x) / surfacePreview.texture.width, (float)(focus.y) / surfacePreview.texture.height, (float)(1) / surfacePreview.texture.width, (float)(1) / surfacePreview.texture.height);
            lifePreview.uvRect = new Rect((float)(focus.x) / lifePreview.texture.width, (float)(focus.y) / lifePreview.texture.height, (float)(1) / lifePreview.texture.width, (float)(1) / lifePreview.texture.height);

            UpdateTileInfo();
        }

        private void UpdateTileInfo()
        {
            this.simulator.ReadTileDetails(this.controller.GetFocus(), (tileDetails) =>
            {
                if (this == null)
                {
                    // 非同期処理中にオブジェクトが破棄されることがある
                    return;
                }

                this.tileDetails = tileDetails;

                UpdateEnvironments();
                UpdateHappiness();

            });
        }

        private void UpdateHappiness()
        {
            ref Life life = ref tile.lives[tileDetails.visibleLayer];

            if (life.Exists)
            {
                ShowHappiness();
            }
            else
            {
                HideHappiness();
            }
        }

        private void UpdateEnvironments()
        {
            this.ui.rootVisualElement.Q<Label>("tile-elevation-label").text = $"{tile.elevation - planet.ocean.seaLevel:#,0}m";
            this.ui.rootVisualElement.Q<Label>("tile-temperature-label").text = $"{TwoDigitsString(tile.temperature - PhysicalConstants.CelsiusZero)}℃";
            this.ui.rootVisualElement.Q<Label>("tile-humidity-label").text = $"{tile.humidity * 100:#,0.0}%";
        }

        private void ShowHappiness()
        {
            SetHappiness(this.ui.rootVisualElement.Q("tile-happiness-Total"), (int)Mathf.Clamp(tileDetails.totalHappiness * 8, 0, 7));
            SetHappiness(this.ui.rootVisualElement.Q("tile-happiness-O2"), (int)Mathf.Clamp(tileDetails.happiness[(int)NicheAxis.O2] * 8, 0, 7));
            SetHappiness(this.ui.rootVisualElement.Q("tile-happiness-Elevation"), (int)Mathf.Clamp(tileDetails.happiness[(int)NicheAxis.Elevation] * 8, 0, 7));
            SetHappiness(this.ui.rootVisualElement.Q("tile-happiness-Temperature"), (int)Mathf.Clamp(tileDetails.happiness[(int)NicheAxis.Temperature] * 8, 0, 7));
            SetHappiness(this.ui.rootVisualElement.Q("tile-happiness-Humidity"), (int)Mathf.Clamp(tileDetails.happiness[(int)NicheAxis.Humidity] * 8, 0, 7));
            SetHappiness(this.ui.rootVisualElement.Q("tile-happiness-Density"), (int)Mathf.Clamp(tileDetails.densityHappiness * 8, 0, 7));
            SetHappiness(this.ui.rootVisualElement.Q("tile-happiness-Nutrient"), (int)Mathf.Clamp(tileDetails.nutrientHappiness * 8, 0, 7));

            this.ui.rootVisualElement.Q("tile-happiness").style.opacity = 1;
        }

        private void SetHappiness(VisualElement view, int value)
        {
            view.RemoveFromClassList("tile-happiness0");
            view.RemoveFromClassList("tile-happiness1");
            view.RemoveFromClassList("tile-happiness2");
            view.RemoveFromClassList("tile-happiness3");
            view.RemoveFromClassList("tile-happiness4");
            view.RemoveFromClassList("tile-happiness5");
            view.RemoveFromClassList("tile-happiness6");
            view.RemoveFromClassList("tile-happiness7");
            view.AddToClassList($"tile-happiness{value}");
        }

        private void HideHappiness()
        {
            this.ui.rootVisualElement.Q("tile-happiness").style.opacity = 0;
        }


        private static string TwoDigitsString(float value)
        {
            if (value >= 100)
            {
                return value.ToString("#,0");
            }
            else if (value >= 10)
            {
                return value.ToString("#,0.0");
            }
            else
            {
                return value.ToString("#,0.00");
            }
        }
    }

}