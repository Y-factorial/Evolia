using Evolia.Model;
using TMPro;
using UnityEngine;
using LogType = Evolia.Model.LogType;

namespace Evolia.GameScene
{
    /// <summary>
    /// 文明獲得のポップアップ
    /// </summary>
    public class CivilizationPopup : Popup
    {
        [SerializeField]
        private GameController controller;

        [SerializeField]
        private LifeIcon icon;

        [SerializeField]
        private TMP_Text yearLabel;

        [SerializeField]
        private TMP_Text messageLabel;

        private LogEntry log;

        private int processedLogCount;

        private Planet planet => GameController.planet;


        public override void Show()
        {
            gameObject.SetActive(true);

            CreateContent();

            controller.SetSpeed(GameSpeed.Pause);
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
            icon.SetTile(CoL.instance[log.param1]);

            yearLabel.text = log.GetAgeText();

            messageLabel.text = $"{CoL.instance.names[log.param1]}が文明を獲得しました。";
        }

        public void OnClickClose()
        {
            Hide();
        }

        public void OnTick()
        {
            if (gameObject.activeSelf)
            {
                // 今表示中なので無視
                return;
            }

            while(processedLogCount < planet.log.Count)
            {
                LogEntry log = planet.log[processedLogCount++];
                if (log.type == LogType.Civilization)
                {
                    this.log = log;
                    Show();
                    return;
                }
            }
        }

    }

}