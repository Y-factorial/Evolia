using UnityEngine;
using UnityEngine.Events;

namespace Evolia.Shared
{

    public class EventDispatcher : MonoBehaviour
    {
        [SerializeField]
        public UnityEvent Event;

        public void Dispatch()
        {
            Event?.Invoke();
        }
    }


}