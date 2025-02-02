using UnityEngine;

namespace Evolia.Shared
{
    public class BillBoard : MonoBehaviour
    {
        public void Update()
        {
            transform.forward = -Camera.main.transform.forward;
        }
    }

}