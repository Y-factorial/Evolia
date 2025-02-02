using System;
using System.Runtime.InteropServices;

namespace Evolia.Model
{
    public enum Variant
    {
        Original,
        Variant1,
        Variant2,
        Variant3,
        Variant4,
        Variant5,
        Variant6,
        Variant7,
        Variant8,
        Variant9,
        Variant10,
        Variant11
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct VariantData
    {
        public readonly FixedArray4<float> preferences;
        public readonly float competitiveness;

        public VariantData(float dElevation, float dTemperature, float dHumidity, float competitiveness)
        {
            this.preferences = new FixedArray4<float>();
            this.preferences[(int)NicheAxis.Elevation] = dElevation;
            this.preferences[(int)NicheAxis.Temperature] = dTemperature;
            this.preferences[(int)NicheAxis.Humidity] = dHumidity;
            this.competitiveness = competitiveness;
        }

        public static readonly VariantData[] AllData = {
            new ( 0, 0, 0, 1 ),
            new ( 500, 0, 0, 0.9f ),
            new ( -500, 0, 0, 0.9f ),
            new (0, 2.5f, 0, 0.9f ),
            new (0, -2.5f, 0, 0.9f ),
            new (0, 0, 0.1f, 0.9f ),
            new (0, 0, -0.1f, 0.9f ),
            new (500, 2.5f, 0, 0.8f ),
            new (-500, -2.5f, 0, 0.8f ),
            new (500, -2.5f, 0, 0.8f ),
            new (-500, 2.5f, 0, 0.8f ),
            new (500, 0, -0.1f, 0.8f )
        };

    }

    public static class VariantExt
    {
        public static float DPref(this Variant variant, NicheAxis axis)
        {
            return VariantData.AllData[(int)variant].preferences[(int)axis];
        }

        public static string Name(this Variant variant)
        {
            if (variant == Variant.Original)
            {
                return "";
            }
            else
            {
                return $"亜種{(int)variant}";
            }
        }

    }
}