using Evolia.Model;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Evolia.GameScene
{
    /// <summary>
    /// 進化レースのグラフ
    /// </summary>
    public class EvolutionRaceGraph : MonoBehaviour
    {
        [SerializeField]
        private GameObject container;

        [SerializeField]
        private GraphBackground background;

        [SerializeField]
        private UnityEvent<Species> OnClickLife;

        [SerializeField]
        private ScrollRect scrollView;

        [SerializeField]
        private BarAndIcon barTemplate;

        [SerializeField]
        private float minValue = 100;

        [SerializeField]
        private float marginFactor = 1.1f;

        [SerializeField]
        private float smoothingFactor = 0.1f;

        private Planet planet => GameController.planet;

        private float prevMax;

        public void Awake()
        {
            foreach (Species spec in CoL.instance.species)
            {
                BarAndIcon bar = Instantiate(barTemplate, container.transform);
                bar.gameObject.SetActive(true);

                bar.Icon.SetTile(spec);

                bar.Icon.OnClick.AddListener((spec) => OnClickLife?.Invoke(spec));
            }
        }

        public void Start()
        {
            UpdateContent();
        }

        public void Update()
        {
            UpdateGraphMax(true);
        }

        public void OnTick()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            UpdateContent(true);
        }

        private void FitBackgroundToViewport()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollView.GetComponent<RectTransform>());

            RectTransform backgroundTransform = background.GetComponent<RectTransform>();

            Vector2 iconSpace = new Vector2(barTemplate.IconSpace, 0);
            backgroundTransform.offsetMin = scrollView.viewport.offsetMin + iconSpace;
            backgroundTransform.offsetMax = scrollView.viewport.offsetMax;

        }

        public void UpdateContent(bool smoothing = false)
        {
            // 棒グラフの可視性を更新
            foreach (BarAndIcon bar in container.transform.GetComponentsInChildren<BarAndIcon>(true))
            {
                ref Species spec = ref bar.Icon.species;

                bool show = planet.Found(spec) && planet.evolutionScores[spec.id]!=0;
                bar.gameObject.SetActive(show);
            }

            // スコアの高い順に並び替える
            List<BarAndIcon> bars = container.transform.GetComponentsInChildren<BarAndIcon>().OrderByDescending(
                child => planet.evolutionScores[child.Icon.species.id]
                ).ToList();

            for(int i = 0; i < bars.Count; ++i)
            {
                bars[i].transform.SetSiblingIndex(i);
            }

            // チャートを更新した結果、スクロールバーが出たかもしれないので、背景を更新する
            FitBackgroundToViewport();

            UpdateGraphMax(smoothing);
        }

        private void UpdateGraphMax(bool smoothing)
        {
            // スコアの最大値を取得
            float max = Mathf.Max(minValue, container.transform.GetComponentsInChildren<BarAndIcon>()
                .Select(bar => planet.evolutionScores[bar.Icon.species.id]).DefaultIfEmpty(0).Max())*marginFactor;

            if (smoothing)
            {
                max = Mathf.Lerp(prevMax, max, smoothingFactor);
            }
            prevMax = max;

            foreach (BarAndIcon bar in container.transform.GetComponentsInChildren<BarAndIcon>())
            {
                bar.Length = planet.evolutionScores[bar.Icon.species.id] / max;
            }

            background.SetMax(max);
        }
    }

}