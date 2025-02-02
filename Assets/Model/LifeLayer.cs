
using Evolia.Model;

namespace Evolia.Model
{
    public enum LifeLayer
    {
        // 微生物
        Microbe,
        // 植物
        Plant,
        // 動物
        Animal
    }

    public static class LifeLayerExt
    {
        public static string Label(this LifeLayer layer)
        {
            switch (layer)
            {
                case LifeLayer.Microbe:
                    return "微生物";
                case LifeLayer.Plant:
                    return "植物";
                case LifeLayer.Animal:
                    return "動物";
                default:
                    throw new System.NotImplementedException($"LifeLayer {layer} is not implemented.");
            }
        }
    }
}