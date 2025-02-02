using Evolia.Model;
using Evolia.Shared;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using LogType = Evolia.Model.LogType;

namespace Evolia.GameScene
{
    /// <summary>
    /// ニュースメッセージを表示するコンポーネント
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrder.AFTER_CONTROLLER)]
    public class NewsMessage : MonoBehaviour
    {
        [SerializeField]
        private UIDocument ui;

        [SerializeField]
        private GameController controller;


        [SerializeField]
        private float initialWaitTime = 5.0f;

        [SerializeField]
        private float slideInTime = 0.5f;

        [SerializeField]
        private float readWaitTime = 4.5f;

        [SerializeField]
        private float readWaitAfterScrollTime = 2.0f;

        [SerializeField]
        private float overScroll = 20;

        [SerializeField]
        private float height = 60;

        private int newsedLogCount = 0;

        private LogEntry? currentLog = null;

        private Planet planet => GameController.planet;

        private Label messageLabel;
        private VisualElement container;
        private VisualElement messageBox;

        public void Awake()
        {
            messageLabel = ui.rootVisualElement.Q<Label>("news-message");
            container = ui.rootVisualElement.Q("news-container");
            messageBox = ui.rootVisualElement.Q("news-message-box");

            newsedLogCount = planet.log.Count;

            messageLabel.text = "";
            container.style.height = 0;
            container.RegisterCallback<ClickEvent>(evt => OnClick());

        }

        public void Start()
        {
            StartCoroutine(UpdateNews());
        }

        private void OnClick()
        {
            if (currentLog.HasValue)
            {
                if (currentLog.Value.type == LogType.Speciation)
                {
                    controller.FindLife(CoL.instance.species[currentLog.Value.param1]);
                }
            }
        }

        private IEnumerator<object> UpdateNews()
        {
            yield return new WaitForSeconds(initialWaitTime);

            while ((this!=null))
            {
                if (planet.log.Count > newsedLogCount)
                {
                    yield return ShowAllMessages();
                }

                yield return null;
            }

        }

        private IEnumerator<object> ShowAllMessages()
        {
            // 最初のメッセージは枠ごとスライドインするのに対して、2つ目以降は枠はそのままでメッセージだけ差し替えるので
            // どちらのパターンかを判定するフラグが必要
            bool secondMessage = false;

            while (planet.log.Count > newsedLogCount)
            {
                yield return ShowSingleMessage(secondMessage);

                secondMessage = true;
            }

            // 表示すべきメッセージがなくなったので、枠を閉じる
            yield return SlideDownContainer();

            currentLog = null;

            // 左にスクロールしたままの可能性があるので戻す
            messageLabel.style.left = 0;
        }


        private IEnumerator<object> ShowSingleMessage(bool secondMessage)
        {
            if (secondMessage)
            {
                yield return SlideDownMessage();

                // 左にスクロールしていた可能性があるので、戻す
                messageLabel.style.left = 0;
            }

            // 現在表示中のメッセージを更新
            currentLog = planet.log[newsedLogCount++];

            // メッセージを差し替え
            messageLabel.text = currentLog.Value.GetNewsMessage(planet);

            if (secondMessage)
            {
                // 欄外に逃がしたので、復活させる
                yield return SlideUpMessage();
            }
            else
            {
                // 枠ごとスライドイン
                yield return SlideUpContainer();
            }

            // メインの表示時間
            yield return new WaitForSeconds(readWaitTime);

            // メッセージがはみ出ていたらスクロール
            yield return ScrollMessageIfOverflow();
        }

        private IEnumerator<object> ScrollMessageIfOverflow()
        {

            // はみ出る幅を計算
            float textWidth = messageLabel.MeasureTextSize(messageLabel.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined).x + overScroll;
            float boxWidth = messageBox.resolvedStyle.width;
            float overflowSize = textWidth - boxWidth;

            if (overflowSize > 0)
            {
                // はみ出る分だけ左にスクロール
                yield return ScrollMessage(overflowSize);

                // スクロール完了後はすぐ読み終わらないので、少し待つ。
                yield return new WaitForSeconds(readWaitAfterScrollTime);
            }
        }

        private IEnumerator<object> SlideUpContainer()
        {
            container.style.height = height;
            yield return new WaitForSeconds(slideInTime);
        }

        private IEnumerator<object> SlideDownContainer()
        {
            container.style.height = 0;
            yield return new WaitForSeconds(slideInTime);
        }

        private IEnumerator<object> SlideDownMessage()
        {
            yield return AnimateUtils.Animate(0, height, slideInTime, t => Mathf.SmoothStep(0, 1, t), (v) => {
                messageLabel.style.top = v;
            });
        }

        private IEnumerator<object> SlideUpMessage()
        {
            yield return AnimateUtils.Animate(height, 0, slideInTime, t => Mathf.SmoothStep(0, 1, t), (v) => {
                messageLabel.style.top = v;
            });
        }

        private IEnumerator<object> ScrollMessage(float overflowSize)
        {
            yield return AnimateUtils.Animate(0, -overflowSize, overflowSize / 100, t => t, (v) => {
                messageLabel.style.left = v;
            });
        }

    }
}