using System;
using System.Runtime.InteropServices;

namespace Evolia.Model
{

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct TileDetails
    {
        public Tile tile;
        public int visibleLayer;
        public FixedArray4<float> happiness;
        public float densityHappiness;
        public float nutrientHappiness;
        public float totalHappiness;
    }
}