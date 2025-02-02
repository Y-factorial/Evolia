
using System;
using System.Runtime.InteropServices;

namespace Evolia.Model
{

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Ocean
    {
        public float seaLevel;

        public float oceanMass;

        public float mineral;

        public float oxydizedMineral;
    }
}