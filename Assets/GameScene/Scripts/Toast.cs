using Evolia.Shared;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Evolia.GameScene
{
    /// <summary>
    /// ユーザにメッセージを表示するためのトースト
    /// </summary>
    public class Toast : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup container;

        [SerializeField]
        private TMP_Text text;

        [SerializeField]
        private float fadeTime = 0.25f;

        [SerializeField]
        private float readWaitTime = 3.0f;

        private Coroutine coroutine;

        public void Show(string message)
        {
            gameObject.SetActive(true);

            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = StartCoroutine(MessageCoroutine(message));
        }

        private IEnumerator<object> MessageCoroutine(string message)
        {
            // 表示メッセージを差し替え
            text.text = message;

            // テキストの幅が変わったはずなのでレイアウトを更新
            LayoutRebuilder.ForceRebuildLayoutImmediate(text.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(container.GetComponent<RectTransform>());

            // フェードイン
            yield return FadeInMessage();

            // 一定時間待つ
            yield return new WaitForSeconds(readWaitTime);

            // フェードアウト
            yield return FadeOutMessage();

            gameObject.SetActive(false);
        }

        private IEnumerator<object> FadeInMessage()
        {
            return AnimateUtils.Animate(0, 1, fadeTime, t => t, v => container.alpha = v);
        }

        private IEnumerator<object> FadeOutMessage()
        {
            return AnimateUtils.Animate(1, 0, fadeTime, t => t, v => container.alpha = v);
        }
    }
}