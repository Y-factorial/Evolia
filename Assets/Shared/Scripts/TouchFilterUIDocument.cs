using Evolia;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evolia.Shared
{
    public class TouchFilterUIDocument : TouchHandler.TouchFilter
    {
        [SerializeField]
        UIDocument ui;

        public override bool IsTouchAllowed(Vector2 position)
        {
            if (!ui.gameObject.activeInHierarchy)
            {
                return true;
            }

            Vector2 uiPos = ScreenUtils.ScreenToUIPoint(ui.rootVisualElement, position);

            VisualElement e = ui.runtimePanel.Pick(uiPos);

            return e == null;
        }
    }

}