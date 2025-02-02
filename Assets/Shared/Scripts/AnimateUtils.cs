using System.Collections.Generic;
using System;
using UnityEngine;

namespace Evolia.Shared
{
    public static class AnimateUtils
    {

        public static IEnumerator<object> Animate(float from, float to, float duration, Func<float, float> ease, Action<float> callback)
        {
            double startTime = Time.timeAsDouble;

            while (Time.timeAsDouble < startTime + duration)
            {
                float t = (float)((Time.timeAsDouble - startTime) / duration);
                float e = ease(t);
                callback(Mathf.Lerp(from, to, e));
                yield return null;
            }
            callback(to);
        }
    }
}