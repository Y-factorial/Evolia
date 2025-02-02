using UnityEngine;

namespace Evolia.Shared
{
    public class MoveTo : MonoBehaviour
    {
        [SerializeField]
        public AnimationCurve curve;

        public Vector3 from;

        [SerializeField]
        public GameObject target;

        public float duration = 1.5f;

        public float timeElapsed;

        // Update is called once per frame
        public void Update()
        {
            if (target == null)
            {
                return;
            }

            timeElapsed += Time.deltaTime;

            transform.position = Vector3.Lerp(from, target.transform.position, curve.Evaluate(timeElapsed / duration));
        }
    }

}