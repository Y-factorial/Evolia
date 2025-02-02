using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Evolia.Model
{
    /*
     * https://home.hiroshima-u.ac.jp/naosaka/echinoderm.html
     */
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Species
    {
        public int id;

        public LifeLayer layer;
        public GeologicalTimescale timescale;
        public int palette;
        public float scale;
        public int variantType;

        public float competitiveness;
        public float evolutionSpeed;

        public float growthSpeed;
        public float reproductAt;
        public float mobility;
        public float densityCapacity;

        // この生物が必要とする栄養素
        public Nutrients produce;
        public Nutrients consume;

        // 光合成
        public float photosynthesis;
        // 呼吸
        public float respiration;

        [SerializeField]
        public FixedArray4<FixedArray4<Vector2>> preferences;

        public FixedArray4<int> transforms;

        public int uniteWith;
        public int uniteTo;


        public float GetHappiness(NicheAxis axis, float value)
        {
            ref FixedArray4<Vector2> points = ref preferences[(int)axis];

            float segment1 = Mathf.Clamp((value - points[0].x) / (points[1].x - points[0].x), 0.0f, 1.0f) * (points[1].y - points[0].y) + points[0].y;
            float segment2 = Mathf.Clamp((value - points[1].x) / (points[2].x - points[1].x), 0.0f, 1.0f) * (points[2].y - points[1].y) + points[1].y;
            float segment3 = Mathf.Clamp((value - points[2].x) / (points[3].x - points[2].x), 0.0f, 1.0f) * (points[3].y - points[2].y) + points[2].y;

            return (value < points[1].x) ? segment1 : (value < points[2].x) ? segment2 : segment3;
        }

    }


}