using Evolia.GameScene;
using Evolia.Model;
using Evolia.Shared;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Evolia.GameScene
{
    /// <summary>
    /// 系統樹を表示するポップアップ
    /// </summary>
    public class PhylogeneticTreePopup : Popup
    {
        [SerializeField]
        private GameController controller;

        [SerializeField]
        private PhylogeneticTree chartContainer;

        [SerializeField]
        private LifeIcon lifeIconPrefab;

        [SerializeField]
        private Vector2Int iconSize = new Vector2Int(64, 64);

        [SerializeField]
        private Vector2Int gridSize = new Vector2Int(80,80);

        private Dictionary<Species, PhylogeneticLink> lifeLinks = new();

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
            foreach(PhylogeneticLink link in lifeLinks.Values)
            {
                Destroy(link.icon.gameObject);
            }

            lifeLinks.Clear();

            for (int i = 1; i < CoL.instance.species.Length; ++i)
            {
                RegisterLifeLink(i);
            }

            foreach (PhylogeneticLink link in lifeLinks.Values.OrderBy((link)=>link.icon.species.id))
            {
                link.Connect(lifeLinks);
            }

            foreach (PhylogeneticLink link in lifeLinks.Values.OrderBy((link) => link.icon.species.id))
            {
                link.SetTierX(gridSize.x);
            }

            lifeLinks[CoL.instance.species[SpecialSpecies.LUCA]].ArrangeY(lifeLinks[CoL.instance.species[SpecialSpecies.LUCA]].TotalChildCount() * gridSize.y);

            foreach(PhylogeneticLink link in lifeLinks.Values.OrderByDescending((link) => link.icon.species.id))
            {
                link.CenterizeY(gridSize.y);
            }

            chartContainer.SetLifeLinks(lifeLinks.Values);

            ResizeToFitChildren();
        }

        public void ResizeToFitChildren()
        {
            RectTransform parentRect = chartContainer.GetComponent<RectTransform>();

            // 初期化: 境界を計算するための変数
            Vector2 minBounds = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maxBounds = new Vector2(float.MinValue, float.MinValue);

            // 子オブジェクトをループして境界を計算
            foreach (RectTransform child in parentRect)
            {
                minBounds = Vector2.Min(minBounds, (Vector2)child.localPosition + child.rect.min);
                maxBounds = Vector2.Max(maxBounds, (Vector2)child.localPosition + child.rect.max);
            }

            Vector2 center = (minBounds + maxBounds) /2;

            foreach (RectTransform child in parentRect)
            {
                child.localPosition -= (Vector3)center;
            }

            Vector2 size = maxBounds - minBounds;

            // RectTransform のサイズを更新
            parentRect.sizeDelta = size + gridSize;
        }

        private void RegisterLifeLink(int spec)
        {
            Species species = CoL.instance.species[spec];

            LifeIcon icon = Instantiate(lifeIconPrefab, chartContainer.transform, false);

            icon.OnClick.AddListener(OnClickLife);

            icon.SetTile(species);
            ScreenUtils.SetSize(icon.GetComponent<RectTransform>(), iconSize);

            PhylogeneticLink lifeLink = new PhylogeneticLink(icon);
            this.lifeLinks.Add(species, lifeLink);

        }

        private void OnClickLife(Species spec)
        {
            controller.FindLife(spec, () => { Hide(); });
        }

        public void OnClickClose()
        {
            Hide();
        }
    }


}