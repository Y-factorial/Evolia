
using System;
using System.Runtime.InteropServices;

namespace Evolia.Model
{

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Atmosphere
    {
        public float co2Mass;

        public float o2Mass;

        public float n2Mass;

        public float h2oMass;

        public float temperature;

        public float h2oCapacity;

        public float o2Ratio;

        public float Mass => co2Mass + o2Mass + n2Mass + h2oMass;
    }

}