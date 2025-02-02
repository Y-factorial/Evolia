using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Evolia.Model
{

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct PlanetData
    {
        [SerializeField]
        public Orbit orbit;

        [SerializeField]
        public Atmosphere atmosphere;

        [SerializeField]
        public Ocean ocean;

        [SerializeField]
        public float solarLongitude;

        [SerializeField]
        public float solarConstant;

        [SerializeField]
        public float solarDeclination;

        [SerializeField]
        public float radiationForcing;

        [SerializeField]
        public Statistics statistics;
    }
}