using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Evolia.Model
{

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Tile
    {
        // 一片の長さ
        // 地球の赤道が4万キロなので、横400x縦200タイルで地球と同じ大きさとする（トーラスと見做す）
        public static readonly float SideLength = 100000;

        public static readonly float Area = SideLength * SideLength;

        public float temperature;
        public float humidity;
        public float elevation;

        public Nutrients nutrients;

        public float dTemperature;

        public float dHumidity;

        public float dElevation;

        [SerializeField]
        public FixedArray3<Life> lives;

        public ref Life microbe => ref lives[0];

        public ref Life plant => ref lives[1];

        public ref Life animal => ref lives[2];

    }
}