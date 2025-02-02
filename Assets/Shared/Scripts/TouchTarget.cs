using UnityEngine;
using UnityEngine.UIElements;

namespace Evolia.Shared
{
    public interface ITouchTarget
    {
        void OnPress(Vector3 position) { }

        void OnTap(Vector3 position) { }
        void OnDoubleTap(Vector3 position) { }
        void OnNthTap(Vector3 position, int count) { }
        void OnLongPress(Vector3 position) { }
        void OnDrag(Vector3 startPosition, Vector3 prevPosition, Vector3 endPosition) { }
        void OnDragEnd(Vector3 position) { }

        void OnZoom(Vector3 position, float scale) { }
    }


}