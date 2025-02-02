using Evolia.Model;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Evolia.GameScene
{
    /// <summary>
    /// 生物の詳細情報を表示するポップアップ
    /// </summary>
    public class SpeciesInfoPopup : Popup
    {
        private Species species;

        private Variant variant;

        [SerializeField]
        private LifeIcon icon;

        [SerializeField]
        private TMP_Text nameLabel;

        [SerializeField]
        private TMP_Text descriptionLabel;

        [SerializeField]
        private TMP_Text variantLabel;

        [SerializeField]
        private TMP_Text nicheO2Label;

        [SerializeField]
        private TMP_Text nicheElevationLabel;

        [SerializeField]
        private TMP_Text nicheTemperatureLabel;

        [SerializeField]
        private TMP_Text nicheHumidityLabel;

        public void Show(in Species species, Variant variant)
        {
            this.species = species;
            this.variant = variant;

            Show();
        }

        public override void Show()
        {
            gameObject.SetActive(true);

            CreateContent();
        }

        public override void Hide()
        {
            gameObject.SetActive(false);
        }

        private void CreateContent()
        {
            icon.SetTile(species, 1, variant);

            nameLabel.text = CoL.instance.names[species.id];

            descriptionLabel.text = CoL.instance.names[species.id]+"です。";

            string variantName = variant.Name();

            List<string> variantTraits = new();

            if (variant.DPref(NicheAxis.Elevation) > 0)
            {
                variantTraits.Add("原種より高地を好むようです。");
            }
            if (variant.DPref(NicheAxis.Elevation) < 0)
            {
                variantTraits.Add("原種より低地を好むようです。");
            }

            if (variant.DPref(NicheAxis.Temperature) > 0)
            {
                variantTraits.Add("原種より暖かい地を好むようです。");
            }
            if (variant.DPref(NicheAxis.Temperature) < 0)
            {
                variantTraits.Add("原種より寒冷地を好むようです。");
            }
            if (variant.DPref(NicheAxis.Humidity) > 0)
            {
                variantTraits.Add("原種より湿地を好むようです。");
            }
            if (variant.DPref(NicheAxis.Humidity) < 0)
            {
                variantTraits.Add("原種より乾燥地を好むようです。");
            }

            if (variantName != "")
            {
                string text = $"{variantName}\n";
                foreach(string trait in variantTraits)
                {
                    text += $"　・{trait}\n";
                }
                variantLabel.text = text;
            }
            else
            {
                variantLabel.text = "";
            }

            nicheO2Label.text = $"{O2ToString(species.preferences[(int)NicheAxis.O2][1].x)} ～ {O2ToString(species.preferences[(int)NicheAxis.O2][2].x)}";
            nicheElevationLabel.text = $"{ElevationToString(species.preferences[(int)NicheAxis.Elevation][1].x + variant.DPref(NicheAxis.Elevation))} ～ {ElevationToString(species.preferences[(int)NicheAxis.Elevation][2].x + variant.DPref(NicheAxis.Elevation))}";
            nicheTemperatureLabel.text = $"{TemperatureToString(species.preferences[(int)NicheAxis.Temperature][1].x + variant.DPref(NicheAxis.Temperature))} ～ {TemperatureToString(species.preferences[(int)NicheAxis.Temperature][2].x + variant.DPref(NicheAxis.Temperature))}";
            nicheHumidityLabel.text = $"{HumidityToString(species.preferences[(int)NicheAxis.Humidity][1].x + variant.DPref(NicheAxis.Humidity))} ～ {HumidityToString(species.preferences[(int)NicheAxis.Humidity][2].x + variant.DPref(NicheAxis.Humidity))}";

            UpdateContent();
        }

        private string O2ToString(float value)
        {
            return $"{value * 100:0.####}%";
        }

        private string ElevationToString(float value)
        {
            return $"{value:#,0}m";
        }

        private string TemperatureToString(float value)
        {
            return $"{value - PhysicalConstants.CelsiusZero:#,0.#}℃";
        }
        private string HumidityToString(float value)
        {
            return $"{Mathf.Clamp01(value) * 100:0.##}%";
        }


        private void UpdateContent()
        {
        }

        public void OnClickClose()
        {
            Hide();
        }

    }

}