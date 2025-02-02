
using System.Runtime.InteropServices;
using UnityEngine;

namespace Evolia.Model
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Row
    {
        float latitude;
        float solarEnergy;

        Vector2 wind;
        Vector2Int upstream1;
        Vector2Int upstream2;
        float up1Weight;
        float windPower;
    }
}