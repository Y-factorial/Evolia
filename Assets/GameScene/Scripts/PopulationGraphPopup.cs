using Evolia.Model;
using System;
using TMPro;
using UnityEngine;

namespace Evolia.GameScene
{
    /// <summary>
    /// 人口グラフのポップアップ
    /// </summary>
    public class PopulationGraphPopup : Popup
    {
        [SerializeField]
        private TMP_Text subTitleLabel;

        [SerializeField]
        private GameController controller;

        [SerializeField]
        private TMP_Dropdown filterDropdown;

        [SerializeField]
        private PopulationGraph landscapeGraph;

        [SerializeField]
        private PopulationGraph portraitGraph;
        
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
            subTitleLabel.text = $" ― {GetFilter().Label()}";

            LifeFilter filter = GetFilter();
            landscapeGraph.filter = filter;
            portraitGraph.filter = filter;

            UpdateContent();
        }

        private void UpdateContent()
        {
            landscapeGraph.UpdateContent();
            portraitGraph.UpdateContent();

            bool landscape = Screen.width > Screen.height;
            this.landscapeGraph.gameObject.SetActive(landscape);
            this.portraitGraph.gameObject.SetActive(!landscape);
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

        public void OnClickLife(Species spec)
        {
            controller.FindLife(spec, ()=>OnClickClose());
        }

        public void OnParamChanged()
        {
            CreateContent();
        }

        public void OnClickClose()
        {
            Hide();
        }

        public void OnScreenOrientationChanged()
        {
            UpdateContent();
        }

    }

}