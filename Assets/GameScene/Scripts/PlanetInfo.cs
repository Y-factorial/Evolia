using Evolia.Model;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evolia.GameScene
{
    /// <summary>
    /// 惑星の情報を表示するUI
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrder.AFTER_CONTROLLER)]
    public class PlanetInfo : MonoBehaviour
    {
        [SerializeField]
        private UIDocument ui;

        private bool tickMode = false;

        private Planet planet => GameController.planet;

        public void Awake()
        {
            this.ui.rootVisualElement.Q<Label>("planet-age-label").RegisterCallback<ClickEvent>(evt=>tickMode=!tickMode);

        }

        public void OnTick()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            this.ui.rootVisualElement.Q<Label>("planet-name-label").text = $"{planet.name}";
            this.ui.rootVisualElement.Q<Label>("planet-age-label").text = $"{(tickMode ? planet.tick : planet.age):#,0}";

            float temperature = planet.atmosphere.temperature - PhysicalConstants.CelsiusZero;
            this.ui.rootVisualElement.Q<Label>("planet-temperature-label").text = $"{TwoDigitsString(temperature)}℃";

            float pressure = planet.Pressure * PhysicalConstants.PascalToAtm;

            this.ui.rootVisualElement.Q<Label>("planet-pressure-label").text = $"{TwoDigitsString(pressure)}";


            if (planet.atmosphere.o2Mass / planet.atmosphere.Mass > 0.001)
            {
                this.ui.rootVisualElement.Q<Label>("planet-o2-label").RemoveFromClassList("ppm");
                this.ui.rootVisualElement.Q<Label>("planet-o2-label").text = $"{planet.atmosphere.o2Mass / planet.atmosphere.Mass * 100:#,0.0}%";
            }
            else
            {
                this.ui.rootVisualElement.Q<Label>("planet-o2-label").AddToClassList("ppm");
                this.ui.rootVisualElement.Q<Label>("planet-o2-label").text = $"{planet.atmosphere.o2Mass / planet.atmosphere.Mass * 1000000:#,0}";
            }
            if (planet.atmosphere.co2Mass / planet.atmosphere.Mass > 0.001)
            {
                this.ui.rootVisualElement.Q<Label>("planet-co2-label").RemoveFromClassList("ppm");
                this.ui.rootVisualElement.Q<Label>("planet-co2-label").text = $"{planet.atmosphere.co2Mass / planet.atmosphere.Mass * 100:#,0.0}%";
            }
            else
            {
                this.ui.rootVisualElement.Q<Label>("planet-co2-label").AddToClassList("ppm");
                this.ui.rootVisualElement.Q<Label>("planet-co2-label").text = $"{planet.atmosphere.co2Mass / planet.atmosphere.Mass * 1000000:#,0}";
            }

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