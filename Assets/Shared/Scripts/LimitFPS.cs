using UnityEngine;
using UnityEngine.Rendering;

namespace Evolia.Shared
{

    public class LimitFPS : MonoBehaviour
    {
        public static LimitFPS instance;

        public int targetFps = 60;

        public int renderFrameInterval = 1;

        public void Awake()
        {
            instance = this;
        }

        public void Start()
        {
            if (Application.isPlaying)
            {
                Application.targetFrameRate = targetFps;
            }
        }

        public void StartOndemandRendering()
        {
            if (Application.isPlaying)
            {
                OnDemandRendering.renderFrameInterval = renderFrameInterval;
            }
        }

        public void StopOndemandRendering()
        {
            if (Application.isPlaying)
            {
                OnDemandRendering.renderFrameInterval = 1;
            }
        }
    }

}