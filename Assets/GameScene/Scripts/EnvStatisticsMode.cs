using Evolia.Model;
using System;
using UnityEngine;

namespace Evolia.GameScene
{
    /// <summary>
    /// 環境統計のモード
    /// </summary>
    public enum EnvStatisticsMode
    {
        Elevation,
        Temperature,
        Humidity
    }

    public static class EnvStatisticsModeExt
    {
        public static EnvStatisticsMode ToStatistics(this NicheAxis v)
        {
            switch (v)
            {
                case NicheAxis.Elevation:
                    return EnvStatisticsMode.Elevation;
                case NicheAxis.Temperature:
                    return EnvStatisticsMode.Temperature;
                case NicheAxis.Humidity:
                    return EnvStatisticsMode.Humidity;
                default:
                    throw new NotImplementedException($"NicheAxis {v} is not implemented");
            }
        }

        public static float MinValue(this EnvStatisticsMode v)
        {
                switch (v)
            {
                case EnvStatisticsMode.Elevation:
                    return -10000;
                case EnvStatisticsMode.Temperature:
                    return PhysicalConstants.CelsiusZero - 10;
                case EnvStatisticsMode.Humidity:
                    return 0;
                default:
                    throw new NotImplementedException($"EnvStatisticsMode {v} is not implemented");
            }
        }

        public static float MaxValue(this EnvStatisticsMode v)
        {
            switch (v)
            {
                case EnvStatisticsMode.Elevation:
                    return 10000;
                case EnvStatisticsMode.Temperature:
                    return PhysicalConstants.CelsiusZero + 30;
                case EnvStatisticsMode.Humidity:
                    return 1;
                default:
                    throw new NotImplementedException($"EnvStatisticsMode {v} is not implemented");
            }
        }

        public static float GetTile(this EnvStatisticsMode v, float value)
        {
            switch (v)
            {
                case EnvStatisticsMode.Elevation:
                    return Mathf.Clamp((1 + value / 10000) * 8, 0.0f, 15.0f);
                case EnvStatisticsMode.Temperature:
                    return Mathf.Floor(Mathf.Clamp((value - (PhysicalConstants.CelsiusZero - 10) ) / 2.5f, 0, 15)) + 16;
                case EnvStatisticsMode.Humidity:
                    return Mathf.Floor(Mathf.Clamp((1 - value) * 16, 0, 15)) + 16;
                default:
                    throw new NotImplementedException($"EnvStatisticsMode {v} is not implemented");
            }
        }

        public static Color Color(this EnvStatisticsMode v)
        {
            switch (v)
            {
                case EnvStatisticsMode.Elevation:
                    return new Color(96 / 255.0f, 56 / 255.0f, 29 / 255.0f);
                case EnvStatisticsMode.Temperature:
                    return new Color(191 / 255.0f, 64 / 255.0f, 64 / 255.0f);
                case EnvStatisticsMode.Humidity:
                    return new Color(64 / 255.0f, 165 / 255.0f, 191 / 255.0f);
                default:
                    throw new NotImplementedException($"EnvStatisticsMode {v} is not implemented");
            }
        }

        public static string Label(this EnvStatisticsMode v)
        {
            switch (v)
            {
                case EnvStatisticsMode.Elevation:
                    return "標高";
                case EnvStatisticsMode.Temperature:
                    return "温度";
                case EnvStatisticsMode.Humidity:
                    return "湿度";
                default:
                    throw new NotImplementedException($"NicheAxis {v} is not implemented");
            }
        }
    }
}