using UnityEngine;

namespace Evolia.Shared
{
    [RequireComponent(typeof(Camera))]
    public class LookAt : MonoBehaviour
    {

        [SerializeField]
        public GameObject target;

        private void LateUpdate()
        {
            Camera camera = GetComponent<Camera>();
            if (target != null)
            {
                camera.transform.LookAt(target.transform.position);
            }
        }
    }

}