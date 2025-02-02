using Evolia.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Evolia.GameScene
{
    /// <summary>
    /// 年表のポップアップ
    /// </summary>
    public class HistoryLogPopup : Popup
    {
        [SerializeField]
        private GameObject logContainer;

        [SerializeField]
        private GameObject logEntryTemplate;

        [SerializeField]
        private ScrollRect scrollView;

        private GameObject currentLog;

        private GeologicalTimescale currentTimescale = (GeologicalTimescale)(-1);

        private int logCount;

        Planet planet => GameController.planet;

        public override void Show()
        {
            gameObject.SetActive(true);

            CreateContent();
        }
        public override void Hide()
        {
            gameObject.SetActive(false);
        }


        public void OnTick()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            UpdateContent(true);
        }

        private void CreateContent()
        {
            UpdateContent();
        }

        private void UpdateContent(bool scroll = false)
        {
            float pos = scrollView.verticalNormalizedPosition;
            bool added = false;
            while (logCount < planet.log.Count)
            {
                LogEntry log = planet.log[logCount];
                ++logCount;

                if (currentLog == null || log.timescale != currentTimescale)
                {
                    // 新しい時代の始まり
                    currentTimescale = log.timescale;
                    currentLog = Instantiate(logEntryTemplate, logContainer.transform);
                    currentLog.SetActive(true);

                    currentLog.transform.GetChild(0).GetComponentInChildren<TMP_Text>().text = currentTimescale.Name();
                    currentLog.transform.GetChild(0).GetComponentInChildren<TMP_Text>().color = currentTimescale.Color();
                    currentLog.transform.GetChild(0).GetComponentInChildren<Image>().color = currentTimescale.Color();
                }

                GameObject entry = Instantiate(currentLog.transform.GetChild(1).gameObject, currentLog.transform);
                entry.SetActive(true);

                entry.GetComponentsInChildren<TMP_Text>()[0].text = log.GetAgeText();
                entry.GetComponentsInChildren<TMP_Text>()[1].text = log.GetLogMessage(planet);

                LayoutRebuilder.ForceRebuildLayoutImmediate(entry.GetComponent<RectTransform>());
                LayoutRebuilder.ForceRebuildLayoutImmediate(currentLog.GetComponent<RectTransform>());
                LayoutRebuilder.ForceRebuildLayoutImmediate(logContainer.GetComponent<RectTransform>());

                added = true;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollView.GetComponent<RectTransform>());

            if (scroll && added)
            {
                // 新しい履歴が追加されていたら、一番下にスクロール
                scrollView.verticalNormalizedPosition = pos;
            }
        }

        public void OnClickClose()
        {
            Hide();
        }
    }

}