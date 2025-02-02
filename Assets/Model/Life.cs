using System;
using System.Runtime.InteropServices;

namespace Evolia.Model
{

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Life
    {
        public int species;

        public Variant variant;

        public float maturity;

        public float health;

        public float blessedTick;

        public bool Exists => species!=0;
    }

}