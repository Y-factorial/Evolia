using System.Runtime.InteropServices;
using System;

namespace Evolia.Model
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Nutrients
    {
        public float soil;
        public float leaves;
        public float honey;
        public float fruits;
        public float meats;
    }
}