using Evolia.GameScene;
using Evolia.Model;
using Evolia.Shared;
using NUnit.Framework.Internal;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Evolia.CreatePlanetScene
{
    public class CreatePlanetController : MonoBehaviour
    {
        public static Model.Universe.PlanetInfo info;

        [SerializeField]
        public float previewSpeed = 0.02f;

        [SerializeField]
        Animator cameraAnimator;

        [SerializeField]
        SceneFader fader;

        [SerializeField]
        MoveTo cameraSlide;

        [SerializeField]
        UIDocument ui;

        [SerializeField]
        GameObject sun;

        [SerializeField]
        GameObject planet;

        [SerializeField]
        SpriteRenderer water1;

        [SerializeField]
        SpriteRenderer water2;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Awake()
        {
            ui.rootVisualElement.Q("hidden-params").style.display = DisplayStyle.None;

            ui.rootVisualElement.Q<Toggle>("show-hidden-toggle").RegisterValueChangedCallback(OnValueChangedShowHidden);
            ui.rootVisualElement.Q<Button>("start-button").clicked += OnClickStart;
            ui.rootVisualElement.Q<Button>("random-button").clicked += OnClickRandom;

        }

        public void Start()
        {
            OnOrientationChange();

            UpdatePreview(1.0f);
        }


        public void OnOrientationChange()
        {
            if (Screen.width > Screen.height)
            {
                cameraSlide.transform.position = new Vector3(0, 0, 0);
                Camera.main.fieldOfView = 30;
            }
            else
            {
                cameraSlide.transform.position = new Vector3(-4, -3, 0);
                Camera.main.fieldOfView = 60;
            }

        }
        public void Update()
        {
            UpdatePreview(previewSpeed);
        }

        private void UpdatePreview(float weight)
        {
            sun.transform.localScale = Vector3.one *
                Mathf.Lerp(sun.transform.localScale.x,
                Map(0.9f, 1.1f, ui.rootVisualElement.Q<Slider>("solar-luminosity-slider")),
                weight);

            planet.transform.localScale = Vector3.one *
                Mathf.Lerp(
                    planet.transform.localScale.x,
                     Map(0.5f, 2.0f, ui.rootVisualElement.Q<Slider>("size-slider")),
                weight);

            planet.transform.localPosition = new Vector3(
                Mathf.Lerp(planet.transform.localPosition.x,
                Map(-2, 0, ui.rootVisualElement.Q<Slider>("solar-distance-slider")), weight), 0, 0);

            planet.transform.localRotation = Quaternion.Euler(0, 0,
                Mathf.Lerp(planet.transform.localRotation.eulerAngles.z,
                Map(0, 80, ui.rootVisualElement.Q<Slider>("axis-tilt-slider")),
                weight));

            water1.color = new Color(1, 1, 1,
                Mathf.Lerp(water1.color.a,
                Mathf.InverseLerp(0, 1, ui.rootVisualElement.Q<Slider>("water-slider").value),
                weight));
            water2.color = new Color(1, 1, 1,
                Mathf.Lerp(water2.color.a, Mathf.InverseLerp(1, 2, ui.rootVisualElement.Q<Slider>("water-slider").value),
                weight));
        }

        private float Map(float min, float max, Slider slider)
        {
            return Mathf.Lerp(min, max, Mathf.InverseLerp(slider.lowValue, slider.highValue, slider.value));
        }

        private void OnValueChangedShowHidden(ChangeEvent<bool> evt)
        {
            ui.rootVisualElement.Q("hidden-params").style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnClickRandom()
        {
            Randomize(ui.rootVisualElement.Q<Slider>("size-slider"));
            Randomize(ui.rootVisualElement.Q<Slider>("solar-distance-slider"));
            Randomize(ui.rootVisualElement.Q<Slider>("solar-luminosity-slider"));
            Randomize(ui.rootVisualElement.Q<Slider>("axis-tilt-slider"));
            Randomize(ui.rootVisualElement.Q<Slider>("water-slider"));
        }

        private void Randomize(Slider slider)
        {
            slider.value = UnityEngine.Random.Range(slider.lowValue, slider.highValue);
        }

        private void OnClickStart()
        {
            GameController.planet = CreatePlanet();

            cameraSlide.from = cameraSlide.transform.position;
            cameraSlide.target = this.planet;
            cameraSlide.timeElapsed = 0;

            cameraAnimator.SetTrigger("Out");

            fader.FadeScene("GameScene/GameScene", Color.white);
        }

        private Planet CreatePlanet()
        {
            Model.Planet planet = new Model.Planet();

            planet.id = info == null ? 0 : info.id;
            planet.name = info.name;

            float scale = ui.rootVisualElement.Q<Slider>("size-slider").value;
            float solarLuminosityFactor = ui.rootVisualElement.Q<Slider>("solar-luminosity-slider").value;
            float solarDistanceFactor = ui.rootVisualElement.Q<Slider>("solar-distance-slider").value;
            float axisTilt = ui.rootVisualElement.Q<Slider>("axis-tilt-slider").value * MathF.PI / 180;
            float waterFactor = ui.rootVisualElement.Q<Slider>("water-slider").value;

            planet.Init(scale, solarLuminosityFactor, solarDistanceFactor, axisTilt, waterFactor);

            return planet;
        }

    }

}