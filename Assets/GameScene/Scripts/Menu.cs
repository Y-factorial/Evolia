using Evolia.Shared;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evolia.GameScene
{
    /// <summary>
    /// メニュー
    /// </summary>
    public class Menu : MonoBehaviour
    {
        [SerializeField]
        private UIDocument ui;

        [SerializeField]
        private GameController controller;

        [SerializeField]
        private GameObject popupContainer;

        [SerializeField]
        private Popup searchLifePopup;

        [SerializeField]
        private Popup populationGraphPopup;

        [SerializeField]
        private Popup historyGraphPopup;

        [SerializeField]
        private Popup phylogeneticTreePopup;

        [SerializeField]
        private Popup nicheChartPopup;

        [SerializeField]
        private Popup envStatisticsPopup;

        [SerializeField]
        private Popup historyLogPopup;

        [SerializeField]
        private Popup evolutionRaceGraph;

        [SerializeField]
        private OptionsMenu optionsMenu;

        [SerializeField]
        private DangerFrame dangerFrame;

        public void Awake()
        {
            foreach (GameSpeed m in Enum.GetValues(typeof(GameSpeed)))
            {
                if ( ui.rootVisualElement.Q<Button>($"speed-button-{m}") is Button button)
                {
                    button.clicked += () => ChangeSpeed(m);
                }
            }

            ui.rootVisualElement.Q<Button>("hamburger-button").clicked += OnClickHamburger;
            ui.rootVisualElement.Q<Button>("options-button").clicked += OnClickOptions;
            ui.rootVisualElement.Q<Button>("return-mainmenu-button").clicked += OnClickReturnToMainMenu;
            ui.rootVisualElement.Q<Button>("exit-button").clicked += OnClickExit;
            ui.rootVisualElement.Q<Button>("close-hamburger-button").clicked += OnClickCloseHamburger;

            ui.rootVisualElement.Q<Button>("search-button").clicked += () => TogglePopup(searchLifePopup);

            ui.rootVisualElement.Q<Button>("overlay-button").clicked += () => ToggleSubMenu("menu-panel-overlay");

            foreach (OverlayMode m in Enum.GetValues(typeof(OverlayMode)))
            {
                if (ui.rootVisualElement.Q<Button>($"overlay-button-{m}") is Button button)
                {
                    button.clicked += () => ToggleOverlay(m);
                }
            }

            ui.rootVisualElement.Q<Button>("overlay-button-HideLife").clicked += () => ShowLife(false);
            ui.rootVisualElement.Q<Button>("overlay-button-ShowLife").clicked += () => ShowLife(true);

            ui.rootVisualElement.Q<Button>("chart-button").clicked += () => ToggleSubMenu("menu-panel-chart");

            ui.rootVisualElement.Q<Button>("chart-button-PopulationGraph").clicked += () => TogglePopup(populationGraphPopup);
            ui.rootVisualElement.Q<Button>("chart-button-HistoryGraph").clicked += () => TogglePopup(historyGraphPopup);
            ui.rootVisualElement.Q<Button>("chart-button-PhylogeneticTree").clicked += () => TogglePopup(phylogeneticTreePopup);
            ui.rootVisualElement.Q<Button>("chart-button-NicheChart").clicked += () => TogglePopup(nicheChartPopup);
            ui.rootVisualElement.Q<Button>("chart-button-EnvStatistics").clicked += () => TogglePopup(envStatisticsPopup);
            ui.rootVisualElement.Q<Button>("chart-button-HistoryLog").clicked += () => TogglePopup(historyLogPopup);
            ui.rootVisualElement.Q<Button>("chart-button-EvolutionRaceGraph").clicked += () => TogglePopup(evolutionRaceGraph);

            ui.rootVisualElement.Q<Button>("cursor-button").clicked += () => ToggleSubMenu("menu-panel-cursor");

            foreach (ClickMode m in Enum.GetValues(typeof(ClickMode)))
            {
                if (ui.rootVisualElement.Q<Button>($"cursor-button-{m}") is Button button)
                {
                    button.clicked += () => ToggleClickMode(m);
                }
            }

        }

        public void Start()
        {
            OnModeChanged();
            OnOptionChanged();
        }

        public void OnOptionChanged()
        {
            ui.rootVisualElement.Q<Button>($"speed-button-{GameSpeed.Fast}").style.display = Options.godMode ? DisplayStyle.None : DisplayStyle.Flex;
            ui.rootVisualElement.Q<Button>($"speed-button-{GameSpeed.SuperFast}").style.display = Options.godMode ? DisplayStyle.Flex : DisplayStyle.None;
            LimitFPS.instance.renderFrameInterval = Options.limitFps ? 4 : 0;
        }

        private void OnClickHamburger()
        {
            ui.rootVisualElement.Q("hamburger-popup").style.display = DisplayStyle.Flex;
        }

        private void OnClickCloseHamburger()
        {
            ui.rootVisualElement.Q("hamburger-popup").style.display = DisplayStyle.None;
        }

        private void OnClickOptions()
        {
            ui.rootVisualElement.Q("hamburger-popup").style.display = DisplayStyle.None;
            optionsMenu.Show();
        }

        private void OnClickReturnToMainMenu()
        {
            controller.ReturnToMainMenu();
        }

        private void OnClickExit()
        {
            controller.OnExit();
        }

        private void ChangeSpeed(GameSpeed speed)
        {
            controller.SetSpeed(speed);
        }

        private void ToggleSubMenu(string name)
        {
            bool active = ui.rootVisualElement.Q(name).style.display == DisplayStyle.Flex;

            HideSubMenuAndPopup();

            if (!active)
            {
                ui.rootVisualElement.Q(name).style.display = DisplayStyle.Flex;
            }
        }


        private void TogglePopup(Popup popup)
        {
            bool active = popup.gameObject.activeSelf;

            HideSubMenuAndPopup();

            if (!active)
            {
                popup.Show();
            }
        }


        private void ToggleOverlay(OverlayMode mode)
        {
            HideSubMenuAndPopup();

            OverlayMode newMode = mode != controller.GetOverlayMode() ? mode : OverlayMode.None;
            controller.SetOverlayMode(newMode);
        }

        private void ShowLife(bool show)
        {
            HideSubMenuAndPopup();

            controller.ShowLife(show);
        }

        private void ToggleClickMode(ClickMode mode)
        {
            HideSubMenuAndPopup();

            ClickMode newMode = mode != controller.GetClickMode() ? mode : ClickMode.Focus;

            controller.SetClickMode(newMode);

        }


        private void HideSubMenuAndPopup()
        {
            ui.rootVisualElement.Query(null, "submenu-panel").ForEach(submenu => submenu.style.display = DisplayStyle.None);

            foreach (Popup popup in popupContainer.GetComponentsInChildren<Popup>())
            {
                popup.Hide();
            }
        }

        public void OnModeChanged()
        {
            UpdateSpeedButtonStates();
            UpdateOverlayButtonStates();
            UpdateCursorButtonStates();

            dangerFrame.Show(controller.GetClickMode().IsDanger());
        }

        private void UpdateSpeedButtonStates()
        {
            UpdateButtonGroupStates("speed-button", controller.GetSpeed());
        }

        private void UpdateOverlayButtonStates()
        {
            UpdateMenuButtonStates("overlay-button", controller.GetOverlayMode());

            ui.rootVisualElement.Q<Button>("overlay-button-HideLife").style.display = controller.GetShowLife() ? DisplayStyle.Flex : DisplayStyle.None;
            ui.rootVisualElement.Q<Button>("overlay-button-ShowLife").style.display = controller.GetShowLife() ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void UpdateCursorButtonStates()
        {
            UpdateMenuButtonStates("cursor-button", controller.GetClickMode());
        }

        private void UpdateMenuButtonStates(string uiKey, Enum currentMode)
        {
            ScreenUtils.ToggleClasses(ui.rootVisualElement.Q<Button>(uiKey), currentMode.GetType(), currentMode);
            UpdateButtonGroupStates(uiKey, currentMode);
        }

        private void UpdateButtonGroupStates(string uiKey, Enum currentMode)
        {
            foreach (Enum mode in Enum.GetValues(currentMode.GetType()))
            {
                if (ui.rootVisualElement.Q<Button>($"{uiKey}-{mode}") is Button button)
                {
                    if (mode.Equals(currentMode))
                    {
                        button.AddToClassList("active");
                    }
                    else
                    {
                        button.RemoveFromClassList("active");
                    }
                }
            }
        }
    }

}