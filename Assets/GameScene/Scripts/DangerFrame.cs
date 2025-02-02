using UnityEngine;
using UnityEngine.UI;

namespace Evolia.GameScene
{
    /// <summary>
    /// ユーザ操作が危険なモードの時に、それを知らせるための赤いフレーム
    /// </summary>
    public class DangerFrame : MonoBehaviour
    {
        [SerializeField]
        private Image image;

        private float deltaTime;

        public void Show(bool show)
        {
            gameObject.SetActive(show);
            deltaTime = 0;
        }

        // Update is called once per frame
        public void Update()
        {
            deltaTime += Time.deltaTime;

            image.color = new Color(1, 1, 1, (Mathf.Sin(deltaTime * Mathf.PI*2)+1)/2);
        }
    }

}