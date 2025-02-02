using UnityEngine;
using UnityEngine.Events;

namespace Evolia.Shared
{
    public class WatchScreenOrientation : MonoBehaviour
    {
        [SerializeField]
        UnityEvent OnOrientationChanged;

        private Vector2Int screenSize;

        public void Start()
        {
            screenSize = new Vector2Int(Screen.width, Screen.height);
        }

        // Update is called once per frame
        public void Update()
        {

            Vector2Int newScreenSize = new Vector2Int(Screen.width, Screen.height);
            if (newScreenSize != screenSize)
            {
                screenSize = newScreenSize;

                OnOrientationChanged?.Invoke();
            }

        }
    }


}