using UnityEngine;
using UnityEngine.UIElements;

namespace Evolia.GameScene
{
    /// <summary>
    /// 自動保存中に表示するスピナー
    /// </summary>
    public class SavingThrobber : MonoBehaviour
    {
        [SerializeField]
        private UIDocument ui;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Start()
        {
            InvokeRepeating(nameof(RotateThrobber), 0.1f, 0.1f);
        }

        private void RotateThrobber()
        {
            int angle = (int)(Time.timeAsDouble % 1 * 8) * (360 / 8);

            ui.rootVisualElement.Q("saving-throbber").style.rotate = new StyleRotate(new Rotate(new Angle(angle, AngleUnit.Degree)));
        }

        public void OnSaveStateChanged(bool saving)
        {
            ui.rootVisualElement.Q("saving-throbber").style.opacity = saving ? 1 : 0;
        }

    }

}