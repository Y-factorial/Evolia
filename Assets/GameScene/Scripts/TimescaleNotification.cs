using Evolia.Model;
using System;
using UnityEngine;

namespace Evolia.GameScene
{
    /// <summary>
    /// 地質時代の変更を通知する
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrder.AFTER_SIMULATOR)]
    public class TimescaleNotification : MonoBehaviour
    {
        [SerializeField]
        private PlanetSimulator simulator;

        [SerializeField]
        private Toast toast;

        [NonSerialized]
        private GeologicalTimescale currentTimescale = (GeologicalTimescale)(-1);

        private Planet planet => simulator.planet;

        public void OnTick()
        {
            if (planet.timescale != currentTimescale)
            {
                // 時代が変わった

                currentTimescale = planet.timescale;
                toast.Show($"{planet.timescale.Name()}");
            }
        }
    }

}