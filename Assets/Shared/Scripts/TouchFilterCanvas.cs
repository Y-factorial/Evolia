using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Evolia.Shared
{
    public class TouchFilterCanvas : TouchHandler.TouchFilter
    {
        [SerializeField]
        GraphicRaycaster canvas;

        public override bool IsTouchAllowed(Vector2 position)
        {
            if ( !canvas.gameObject.activeInHierarchy)
            {
                return true;
            }

            List<RaycastResult> result = new List<RaycastResult>();
            canvas.Raycast(new PointerEventData(EventSystem.current) { position = position }, result);

            return result.Count == 0;
        }
    }

}