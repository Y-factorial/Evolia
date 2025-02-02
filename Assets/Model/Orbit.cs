using System;
using System.Runtime.InteropServices;

namespace Evolia.Model
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Orbit
    {
        public float solarLuminosity;

        public float solarDistance;

        public float radius;

        public float axisTilt;

        public float gravity;
    }
}