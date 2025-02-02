
using System;
using System.Runtime.InteropServices;

namespace Evolia.Model
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Statistics
    {
        public float temperatureSum;
        public float oceanDepthSum;
        public float photosynthesisSum;
        public float respirationSum;
        public int oceanTileCount;

        public float photosynthesisMass;
        public float respirationMass;
    }
}
