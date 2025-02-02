using Evolia.GameScene;
using Evolia.Model;
using Evolia.Shared;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evolia.MainMenuScene
{

    public class MainMenuController : MonoBehaviour
    {
        [SerializeField]
        SceneFader fader;

        [SerializeField]
        UIDocument ui;

        [SerializeField]
        OptionsMenu optionsMenu;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Awake()
        {
            ui.rootVisualElement.Q<Button>("resume-button").clicked += OnClickResume;
            ui.rootVisualElement.Q<Button>("start-button").clicked += OnClickStart;
            ui.rootVisualElement.Q<Button>("option-button").clicked += OnClickOption;
            ui.rootVisualElement.Q<Button>("exit-button").clicked += OnClickExit;
        }

        public void Start()
        {
            Universe universe = Universe.Load();
            if ( universe.lastPlayedPlanetId!=-1 &&
                universe.planets.Find(p => p.id == universe.lastPlayedPlanetId && p.IsSaved ) != null)
            {
                ui.rootVisualElement.Q("resume-button").style.display = DisplayStyle.Flex;
            }
            else
            {
                ui.rootVisualElement.Q("resume-button").style.display = DisplayStyle.None;
            }
        }

        private void OnClickResume()
        {
            Universe universe = Universe.Load();
            GameController.planet = Planet.Load(universe.lastPlayedPlanetId);

            fader.FadeScene("GameScene/GameScene", Color.white);
        }

        private void OnClickStart()
        {
            fader.FadeScene("SelectPlanetScene/SelectPlanetScene", Color.black);
        }

        private void OnClickOption()
        {
            optionsMenu.Show();
        }

        private void OnClickExit()
        {
            QuitOnEsc.Exit();
        }
    }

}