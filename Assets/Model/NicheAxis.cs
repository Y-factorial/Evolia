
namespace Evolia.Model
{
    public enum NicheAxis
    {
        O2,
        Elevation,
        Temperature,
        Humidity
    }

    public static class NicheAxisExt
    {
        public static string Label(this NicheAxis v)
        {
            switch (v)
            {
                case NicheAxis.O2:
                    return "酸素";
                case NicheAxis.Elevation:
                    return "標高";
                case NicheAxis.Temperature:
                    return "温度";
                case NicheAxis.Humidity:
                    return "湿度";
                default:
                    throw new System.NotImplementedException($"NicheAxis {v} is not implemented.");
            }
        }
    }
}