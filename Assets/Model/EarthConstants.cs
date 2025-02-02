

using UnityEngine;

namespace Evolia.Model
{
    public struct EarthConstants
    {
        // 太陽の光度 (ワット単位)
        public static readonly float SolarLuminosity = 3.828e26f;

        // 太陽からの距離 (メートル単位)
        public static readonly float SolarDistance = 149597870700;

        // 地球が単位面積単位時間当たりに受け取るエネルギー (ワット毎平方メートル単位)
        public static readonly float SolarConstant = SolarLuminosity / (4 * Mathf.PI * SolarDistance * SolarDistance);

        // 地球の半径 (メートル単位)
        public static readonly float Radius = 6371000;

        // 地球の表面積
        public static readonly float SurfaceArea = 4 * Mathf.PI * Radius * Radius;

        // 地球の平均アルベド
        public static readonly float Albedo = 0.3f;

        // 赤道付近の吸収エネルギー密度 (ワット毎平方メートル単位)
        public static readonly float EquatorialAbsorption = (1 - Albedo) * SolarConstant / 2; // 夜は光が当たらないので、平均すると半分

        // 赤道付近の気温 (ケルビン単位)
        public static readonly float EquatorialTemperature = Mathf.Pow(EquatorialAbsorption / PhysicalConstants.BoltzmannConstant, 0.25f);

        // 極付近のアルベド
        public static readonly float PolarAlbedo = 0.6f;

        // 極付近の入射角 (ラジアン単位)
        public static readonly float PolarIncidence = Mathf.PI / 2 * 0.7f;

        // 極付近の吸収エネルギー密度 (ワット毎平方メートル単位)
        public static readonly float PolarAbsorption = (1 - PolarAlbedo) * SolarConstant * Mathf.Cos(PolarIncidence);

        // 極付近の気温 (ケルビン単位)
        public static readonly float PolarTemperature = Mathf.Pow(PolarAbsorption / PhysicalConstants.BoltzmannConstant, 0.25f);

        // 地球に存在する水の総量 (キログラム単位)
        public static readonly float WaterMass = 1.4e21f;

        // 大気中の窒素の総質量 (キログラム単位)
        public static readonly float N2Mass = 3.89e18f;

        // 大気中の酸素の総質量 (キログラム単位)
        public static readonly float O2Mass = 1.19e18f;

        // 重力加速度 (メートル毎秒毎秒単位)
        public static readonly float Gravity = 9.81f;

        public static readonly float AtmosphereMass = N2Mass + O2Mass;

        // 地球の気圧 (パスカル単位)
        public static readonly float Pressure = AtmosphereMass * Gravity / SurfaceArea;


        // エベレストの海抜高度 (メートル単位)
        public static readonly float EverestAltitude = 8848;

        // マリアナ海溝の海抜高度 (メートル単位)
        public static readonly float MarianaTrenchAltitude = -10994;

        // 海洋の平均深度 (メートル単位)
        public static readonly float OceanDepth = -3688;

        // 土壌の熱容量 (ジュール毎平方メートル毎ケルビン単位)
        public static readonly float SoilHeatCapacity = 300000.0f;
        // 海洋の熱容量 (ジュール毎平方メートル毎ケルビン単位)
        public static readonly float OceanHeatCapacity = 41860000.0f;

        public static float SolarAbsorption(float latitude, float albedo, float solarConstant, float axisTiltRad, float solarLongitude)
        {
            float solarDeclination = Mathf.Asin(Mathf.Sin(axisTiltRad) * Mathf.Sin(solarLongitude));

            float sunriseAngle = Mathf.Acos(Mathf.Clamp(-Mathf.Tan(solarDeclination) * Mathf.Tan(latitude), -1, 1));

            return (1 - albedo) * solarConstant * (Mathf.Sin(solarDeclination) * Mathf.Sin(latitude) * sunriseAngle + Mathf.Cos(solarDeclination) * Mathf.Cos(latitude) * Mathf.Sin(sunriseAngle));
        }

        public static readonly int Width = 400;

        public static readonly int Height = 200;

    }
}