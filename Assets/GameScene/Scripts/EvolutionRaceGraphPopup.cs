using Evolia.Model;
using UnityEngine;

namespace Evolia.GameScene
{

    /// <summary>
    /// 進化レースのグラフを表示するポップアップ
    /// </summary>
    public class EvolutionRaceGraphPopup : Popup
    {
        [SerializeField]
        private GameController controller;

        [SerializeField]
        private EvolutionRaceGraph graph;

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
            UpdateContent();
        }

        private void UpdateContent()
        {
            graph.UpdateContent();
        }

        public void OnClickLife(Species spec)
        {
            controller.FindLife(spec, () => OnClickClose());
        }

        public void OnClickClose()
        {
            Hide();
        }

    }

}