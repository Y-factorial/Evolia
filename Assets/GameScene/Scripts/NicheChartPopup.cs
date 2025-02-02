using Evolia.Model;
using Evolia.Shared;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Evolia.GameScene
{
    /// <summary>
    /// ニッチマップを表示するポップアップ
    /// </summary>
    public class NicheChartPopup : Popup
    {
        [SerializeField]
        TMP_Text subTitleLabel;

        [SerializeField]
        GameObject chartContainer;

        [SerializeField]
        TMP_Dropdown filterDropdown;

        [SerializeField]
        TMP_Dropdown xAxisDropdown;

        [SerializeField]
        TMP_Dropdown yAxisDropdown;

        [SerializeField]
        EnvAxis xAxis;

        [SerializeField]
        EnvAxis yAxis;

        [SerializeField]
        LifeIcon lifeIconPrefab;

        [SerializeField]
        NicheArea nicheAreaPrefab;

        Planet planet => GameController.planet;

        [SerializeField]
        public Vector2Int iconSize = new Vector2Int(64, 64);

        private Dictionary<int, LifeIcon> icons = new();
        private Dictionary<int, NicheArea> areas = new();

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
            foreach (LifeIcon icon in icons.Values)
            {
                Destroy(icon.gameObject);
            }
            icons.Clear();

            foreach (NicheArea area in areas.Values)
            {
                Destroy(area.gameObject);
            }
            areas.Clear();

            NicheAxis xAxis = GetAxis(xAxisDropdown);
            NicheAxis yAxis = GetAxis(yAxisDropdown);

            LifeFilter fiter = GetFilter();

            subTitleLabel.text = $" ― {fiter.Label()} ― {yAxis.Label()}×{xAxis.Label()}";

            float xMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMin = float.MaxValue;
            float yMax = float.MinValue;

            // 最低限、全ての生物の中心が入る

            foreach (Species species in CoL.instance.species)
            {
                if (!fiter.Match(species))
                {
                    continue;
                }

                ref FixedArray4<Vector2> xPref = ref species.preferences[(int)xAxis];
                ref FixedArray4<Vector2> yPref = ref species.preferences[(int)yAxis];

                xMin = Mathf.Min(xMin, (xPref[1].x + xPref[2].x) / 2);
                xMax = Mathf.Max(xMax, (xPref[1].x + xPref[2].x) / 2);
                yMin = Mathf.Min(yMin, (yPref[1].x + yPref[2].x) / 2);
                yMax = Mathf.Max(yMax, (yPref[1].x + yPref[2].x) / 2);

            }

            // そのうえで、ニッチの最大値、最小値は、軸の範囲に収まっていれば入れる

            foreach (Species species in CoL.instance.species)
            {
                if (!fiter.Match(species))
                {
                    continue;
                }

                ref FixedArray4<Vector2> xPref = ref species.preferences[(int)xAxis];
                ref FixedArray4<Vector2> yPref = ref species.preferences[(int)yAxis];

                xMin = Mathf.Min(xMin, Mathf.Max(xPref[1].x, xAxis.ToStatistics().MinValue()));
                xMax = Mathf.Max(xMax, Mathf.Min(xPref[2].x, xAxis.ToStatistics().MaxValue()));
                yMin = Mathf.Min(yMin, Mathf.Max(yPref[1].x, yAxis.ToStatistics().MinValue()));
                yMax = Mathf.Max(yMax, Mathf.Min(yPref[2].x, yAxis.ToStatistics().MaxValue()));

            }

            if (xMax == xMin)
            {
                xMax = xMin + 1;
            }
            if (yMax == yMin)
            {
                yMax = yMin + 1;
            }


            this.xAxis.SetMode(xAxis.ToStatistics(), xMin, xMax);
            this.yAxis.SetMode(yAxis.ToStatistics(), yMin, yMax);

            Vector2 chartSize = chartContainer.GetComponent<RectTransform>().rect.size;

            foreach (Species species in CoL.instance.species)
            {
                if (!fiter.Match(species))
                {
                    continue;
                }

                ref FixedArray4<Vector2> xPref = ref species.preferences[(int)xAxis];
                ref FixedArray4<Vector2> yPref = ref species.preferences[(int)yAxis];

                NicheArea nicheArea = Instantiate(nicheAreaPrefab, chartContainer.transform, false);

                nicheArea.SetColor(new Color(0xbf / 255.0f, 0x40 / 255.0f, 0x40 / 255.0f, 1));

                nicheArea.SetArea(new float[] {
                    chartSize.x*(xPref[0].x- xMin)/(xMax- xMin),
                    chartSize.x*(xPref[1].x- xMin)/(xMax- xMin),
                    chartSize.x*(xPref[2].x- xMin)/(xMax- xMin),
                    chartSize.x*(xPref[3].x- xMin)/(xMax- xMin),
                }, new float[] {
                    chartSize.y*(yPref[0].x- yMin)/(yMax- yMin),
                    chartSize.y*(yPref[1].x- yMin)/(yMax- yMin),
                    chartSize.y*(yPref[2].x- yMin)/(yMax- yMin),
                    chartSize.y*(yPref[3].x- yMin)/(yMax- yMin),
                });

                areas.Add(species.id, nicheArea);
            }

            foreach (Species species in CoL.instance.species)
            {
                if (!fiter.Match(species))
                {
                    continue;
                }

                ref FixedArray4<Vector2> xPref = ref species.preferences[(int)xAxis];
                ref FixedArray4<Vector2> yPref = ref species.preferences[(int)yAxis];

                float xCenter = (xPref[1].x + xPref[2].x) / 2;
                float yCenter = (yPref[1].x + yPref[2].x) / 2;

                LifeIcon icon = Instantiate(lifeIconPrefab, chartContainer.transform, false);

                icon.OnClick.AddListener((species) => OnClickLife(species));

                icon.SetTile(species);
                ScreenUtils.SetSize(icon.GetComponent<RectTransform>(), iconSize);

                icon.transform.localPosition = new Vector3(chartSize.x * (xCenter - xMin) / (xMax - xMin), chartSize.y * (yCenter - yMin) / (yMax - yMin), 0);

                icons.Add(species.id, icon);
            }

            UpdateContent();
        }

        public void OnTick()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            UpdateContent();
        }

        private void UpdateContent()
        {
            foreach (int spec in icons.Keys)
            {
                areas[spec].gameObject.SetActive(planet.Found(CoL.instance.species[spec]));
                icons[spec].gameObject.SetActive(planet.Found(CoL.instance.species[spec]));
            }
        }

        public void OnClickLife(in Species spec)
        {
            ClearSelection();
            areas[spec.id].SetColor(new Color(0x40 / 255.0f, 0xa5 / 255.0f, 0xbf / 255.0f, 1));
        }

        public void OnClickBackground()
        {
            ClearSelection();
        }

        private void ClearSelection()
        {
            foreach (NicheArea nicheArea in areas.Values)
            {
                if (nicheArea != null)
                {
                    nicheArea.SetColor(new Color(0xbf / 255.0f, 0x40 / 255.0f, 0x40 / 255.0f, 1));
                }
            }
        }

        private LifeFilter GetFilter()
        {
            switch (filterDropdown.value)
            {
                case 0:
                    return LifeFilter.Microbe;
                case 1:
                    return LifeFilter.Plant;
                case 2:
                    return LifeFilter.AquaticAnimal;
                case 3:
                    return LifeFilter.TerrestrialAnimal;
                default:
                    throw new NotImplementedException($"value {filterDropdown.value} is not implemented");
            }
        }

        private NicheAxis GetAxis(TMP_Dropdown dropdown)
        {
            switch (dropdown.value)
            {
                case 0:
                    return NicheAxis.Elevation;
                case 1:
                    return NicheAxis.Temperature;
                case 2:
                    return NicheAxis.Humidity;
                default:
                    throw new NotImplementedException($"value {dropdown.value} is not implemented");

            }
        }

        public void OnParamChanged()
        {
            CreateContent();
        }

        public void OnClickClose()
        {
            Hide();
        }
    }


}