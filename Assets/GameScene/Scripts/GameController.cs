using UnityEngine;
using Evolia.Model;
using UnityEngine.UI;
using Evolia.Shared;
using System;
using UnityEngine.Events;

namespace Evolia.GameScene
{

    [DefaultExecutionOrder(ExecutionOrder.CONTROLLER)]
    public class GameController : MonoBehaviour
    {
        public static Planet planet;

        [SerializeField]
        private UnityEvent<bool> OnSaveStateChanged;

        [SerializeField]
        private UnityEvent OnModeChanged;

        [SerializeField]
        private UnityEvent OnFocusChanged;

        [SerializeField]
        private PlanetSimulator simulator;

        [SerializeField]
        private PlanetView planetView;

        [SerializeField]
        private Minimap minimap;

        [SerializeField]
        private PlanetInfo planetInfo;

        [SerializeField]
        private TileInfo tileInfo;

        [SerializeField]
        private SmoothScroll scroll;

        [SerializeField]
        private BioPod bioPod;

        [SerializeField]
        private Toast toast;

        [SerializeField]
        private SceneFader fader;

        private Vector2Int focus;

        private ClickMode clickMode;

        [SerializeField]
        public CoL col;

        public void Awake()
        {

#if UNITY_EDITOR
            //planet = null;
#endif
            if (planet == null)
            {
                planet = new Planet();
                planet.Init(1.0f, 1.0f, 1.0f, 24 * Mathf.PI / 180, 1.0f);
            }
        }

        public void Start()
        {
            SetFocus(planet.size / 2);

            InvokeRepeating(nameof(AutoSave), 60, 60);
        }

        private void AutoSave()
        {
            Save();
        }

        public void ReturnToMainMenu()
        {
            Save(() =>
            {
                fader.FadeScene("MainMenuScene/MainMenuScene", Color.white);
            });
        }
        public void OnExit()
        {
            Save( () =>
            {
                QuitOnEsc.Exit();
            });
        }

        private void Save(Action callback = null)
        {
            OnSaveStateChanged?.Invoke(true);

            simulator.ReadTiles(() =>
            {
                if (this == null)
                {
                    // 非同期処理中にオブジェクトが破棄されることがある
                    return;
                }

                planet.AsyncSave(minimap.Image, () => { OnSaveStateChanged?.Invoke(false); callback?.Invoke(); });
            });
        }

        public void OnClick(Vector2 pos)
        {
            Vector3 lpos = ScreenUtils.ScreenToLocalPoint(planetView.gameObject, pos);

            Vector2 tile = ((Vector2)lpos - planetView.GetComponent<RectTransform>().rect.min) / planetView.tileSize.x;

            Vector2Int clickPos = new Vector2Int(
                    ((int)tile.x + planet.size.x)%planet.size.x,
                    Mathf.Clamp((int)tile.y, 0, planet.size.y - 1));

            switch (clickMode)
            {
                case ClickMode.Focus:
                    SetFocus(clickPos);
                    break;
                case ClickMode.Abduction:
                    bioPod.Pick(clickPos);
                    break;
                case ClickMode.Introduction:
                    bioPod.Put(clickPos);
                    SetClickMode(ClickMode.Focus);
                    break;
                case ClickMode.Meteor:
                    simulator.EventMeteor(clickPos);
                    break;
                case ClickMode.Volcano:
                    simulator.EventVolcano(clickPos);
                    break;
                case ClickMode.Collapse:
                    simulator.EventCollapse(clickPos);
                    break;
                case ClickMode.XRay:
                    simulator.EventCosmicRay(clickPos);
                    break;
                default:
                    throw new NotImplementedException($"clickMode {clickMode} is not implemented");

            }
        }

        public Vector2Int GetFocus()
        {
            return focus;
        }

        public void SetFocus(Vector2Int focus)
        {
            if (this.focus == focus)
            {
                return;
            }

            this.focus = focus;

            planetView.SetFocus(this.focus);

            OnFocusChanged?.Invoke();
        }

        public void OnScroll()
        {
            minimap.UpdateScopeRect();
        }

        public GameSpeed GetSpeed()
        {
            return simulator.speed;
        }

        public void SetSpeed(GameSpeed speed)
        {
            if (simulator.speed == speed)
            {
                return;
            }

            simulator.speed = speed;

            toast.Show($"スピード：{simulator.speed.Message()}");

            OnModeChanged?.Invoke();
        }

        public OverlayMode GetOverlayMode()
        {
            return simulator.overlayMode;
        }

        public void SetOverlayModeHappiness(int species, Variant variant)
        {
            simulator.happinessSpecies = species;
            simulator.happinessVariant = variant;
            SetOverlayMode(OverlayMode.Happiness);
        }

        public void SetOverlayMode(OverlayMode mode)
        {
            if (mode == simulator.overlayMode)
            {
                return;
            }

            simulator.overlayMode = mode;

            if (simulator.overlayMode != OverlayMode.None)
            {
                simulator.RenderNow();
            }

            planetView.ShowOverlay(simulator.overlayMode != OverlayMode.None);


            toast.Show($"オーバーレイ：{simulator.overlayMode.Message()}");

            OnModeChanged?.Invoke();
        }

        public void ShowLife(bool show)
        {
            if ( show== simulator.showLife)
            {
                return;
            }

            simulator.showLife = show;

            if (simulator.showLife)
            {
                simulator.RenderNow();
            }

            planetView.ShowLife(simulator.showLife);

            toast.Show(simulator.showLife ? "生物を表示します" : "生物を非表示にします");

            OnModeChanged?.Invoke();
        }


        public void OnClickPickPod()
        {
            SetClickMode(ClickMode.Introduction);
        }

        public ClickMode GetClickMode()
        {
            return clickMode;
        }

        public void SetClickMode(ClickMode newClickMode)
        {
            if (newClickMode == clickMode)
            {
                return;
            }

            this.clickMode = newClickMode;

            bioPod.Show(clickMode == ClickMode.Abduction || clickMode == ClickMode.Introduction);

            if (clickMode == ClickMode.Introduction)
            {
                SetOverlayModeHappiness(bioPod.PickedLife.species, bioPod.PickedLife.variant);
            }
            else
            {
                if (GetOverlayMode() == OverlayMode.Happiness)
                {
                    SetOverlayMode(OverlayMode.None);
                }
            }

            toast.Show($"インタラクション：{clickMode.Message()}");

            OnModeChanged?.Invoke();
        }

        public bool GetShowLife()
        {
            return simulator.showLife;
        }

        public void ScrollTo(Vector2Int focus, bool zoom)
        {
            if (zoom)
            {
                scroll.ZoomTo(2);
            }

            // 新しい座標を計算、移動量はスクリーン座標なので、ワールド座標に変換する
            Vector2 localPosInWorld = (focus - planet.size * new Vector2(0.5f, 0.5f) + new Vector2(0.5f, 0.5f)) * this.planetView.GetComponent<RectTransform>().rect.size / planet.size;

            // ワールド座標上でのターゲットの位置
            Vector3 worldPosTarget = this.planetView.transform.TransformPoint(localPosInWorld);

            // スクリーン座標上での地図の中央
            Vector3 worldPosCenter = ScreenUtils.ScreenToWorldPoint(planetView.transform, new Vector2(Screen.width / 2, Screen.height / 2));

            // 新しい座標を計算、ターゲットと中央の差は移動量なので、それにマップの現在位置を足している
            Vector3 newPosition = worldPosCenter - worldPosTarget + this.planetView.transform.position;

            scroll.ScrollTo(newPosition, true);
        }

        public void FindLife(Species spec, Action foundCallback = null)
        {
            simulator.FindLife(spec, this.focus, (pos) => {
                SetFocus(pos);
                ScrollTo(pos, true);
                foundCallback?.Invoke();
            }, () => {
                toast.Show($"{CoL.instance.names[spec.id]} は見つかりませんでした。");
            });

        }
    }

}