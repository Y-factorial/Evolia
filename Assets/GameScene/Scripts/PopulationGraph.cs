using Evolia.Model;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Evolia.GameScene
{
    /// <summary>
    /// 人口グラフ
    /// </summary>
    public class PopulationGraph : MonoBehaviour
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
        private bool landscape;

        [SerializeField]
        private float minValue = 1000;

        [SerializeField]
        private float marginFactor = 1.2f;

        [SerializeField]
        private float smoothingFactor = 0.1f;

        public LifeFilter filter;

        private float prevMax;

        private Planet planet => GameController.planet;

        public void Awake()
        {
            foreach (Species spec in CoL.instance.species)
            {
                {
                    BarAndIcon bar = Instantiate(barTemplate, container.transform);
                    bar.gameObject.SetActive(true);

                    bar.Icon.SetTile(spec);

                    bar.Icon.OnClick.AddListener((spec) => OnClickLife?.Invoke(spec));
                }

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

            Vector2 iconSpace = new Vector2(landscape ? 0 : barTemplate.IconSpace, landscape ? barTemplate.IconSpace : 0);

            backgroundTransform.offsetMin = scrollView.viewport.offsetMin + iconSpace;
            backgroundTransform.offsetMax = scrollView.viewport.offsetMax;
        }

        public void UpdateContent(bool smoothing = false)
        {
            foreach (BarAndIcon bar in container.transform.GetComponentsInChildren<BarAndIcon>(true))
            {
                ref Species spec = ref bar.Icon.species;

                bool show = filter.Match(spec) && planet.Found(spec);
                bar.gameObject.SetActive(show);
            }

            // チャートを更新した結果、スクロールバーが出たかもしれないので、背景を更新する
            FitBackgroundToViewport();

            UpdateGraphMax(smoothing);
        }

        private void UpdateGraphMax(bool smoothing)
        {
            // グラフの最大値を計算
            float max = Mathf.Max(minValue, container.transform.GetComponentsInChildren<BarAndIcon>()
                .Select(bar=> (float)planet.populations[bar.Icon.species.id]).DefaultIfEmpty(0).Max())*marginFactor;

            // スムージング
            if (smoothing)
            {
                max = Mathf.Lerp(prevMax, max, smoothingFactor);
            }
            prevMax = max;

            // 各バーの長さを更新
            foreach (BarAndIcon bar in container.transform.GetComponentsInChildren<BarAndIcon>())
            {
                bar.Length = planet.populations[bar.Icon.species.id] / max;
            }

            background.SetMax(max);
        }
    }

}