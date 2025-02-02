using Evolia.Shared;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Evolia.Shared
{

    public class OptionsMenu : MonoBehaviour
    {
        [SerializeField]
        UIDocument ui;

        [SerializeField]
        UnityEvent OnChange;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Awake()
        {
            ui.rootVisualElement.Q<Button>("options-close-button").clicked += OnClickCloseOption;

            Options.screenOrienttion = Enum.Parse<ScreenOrientation>(PlayerPrefs.GetString("Screen Orientation", Screen.orientation.ToString()));
            Options.limitFps = PlayerPrefs.GetInt("Limit Fps", 1) == 1;
            Options.smoothSurface = Enum.Parse<SmoothingMode>(PlayerPrefs.GetString("Smooth Surface", SmoothingMode.Vertex.ToString()));
            Options.tierScaling = PlayerPrefs.GetInt("Tier Scaling", 1) == 1;
            Options.godMode = PlayerPrefs.GetInt("God Mode", 0) == 1;

            ui.rootVisualElement.Q<EnumField>("options-screenorientation-enum").value = Options.screenOrienttion;
            ui.rootVisualElement.Q<Toggle>("options-limitfps-toggle").value = Options.limitFps;
            ui.rootVisualElement.Q<EnumField>("options-smoothsurface-enum").value = Options.smoothSurface;
            ui.rootVisualElement.Q<Toggle>("options-tierscaling-toggle").value = Options.tierScaling;
            ui.rootVisualElement.Q<Toggle>("options-godmode-toggle").value = Options.godMode;

            ChangeScreenOrientation();
        }

        public void Show()
        {
            ui.rootVisualElement.Q("options-popup-layer").style.display = DisplayStyle.Flex;
        }

        private void OnClickCloseOption()
        {
            ui.rootVisualElement.Q("options-popup-layer").style.display = DisplayStyle.None;

            Options.screenOrienttion = (ScreenOrientation)ui.rootVisualElement.Q<EnumField>("options-screenorientation-enum").value;
            Options.limitFps = ui.rootVisualElement.Q<Toggle>("options-limitfps-toggle").value;
            Options.smoothSurface = (SmoothingMode)ui.rootVisualElement.Q<EnumField>("options-smoothsurface-enum").value;
            Options.tierScaling = ui.rootVisualElement.Q<Toggle>("options-tierscaling-toggle").value;
            Options.godMode = ui.rootVisualElement.Q<Toggle>("options-godmode-toggle").value;

            PlayerPrefs.SetString("Screen Orientation", Options.screenOrienttion.ToString());
            PlayerPrefs.SetInt("Limit Fps", Options.limitFps ? 1 : 0);
            PlayerPrefs.SetString("Smooth Surface", Options.smoothSurface.ToString());
            PlayerPrefs.SetInt("Tier Scaling", Options.tierScaling ? 1 : 0);
            PlayerPrefs.SetInt("God Mode", Options.godMode ? 1 : 0);

            ChangeScreenOrientation();

            OnChange?.Invoke();
        }

        public void ChangeScreenOrientation()
        {
            Screen.orientation = Options.screenOrienttion;

            Screen.autorotateToPortrait = true;            // 縦長を許可
            Screen.autorotateToLandscapeLeft = true;       // 左向きの横長を許可
            Screen.autorotateToLandscapeRight = true;      // 右向きの横長を許可
            Screen.autorotateToPortraitUpsideDown = true; // 逆向きの縦長を許可
        }
    }

}