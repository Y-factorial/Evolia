using UnityEngine;

namespace Evolia.GameScene
{
    /// <summary>
    /// ポップアップの基底クラス
    /// </summary>
    public abstract class Popup : MonoBehaviour
    {
        public abstract void Show();

        public abstract void Hide();
    }
}