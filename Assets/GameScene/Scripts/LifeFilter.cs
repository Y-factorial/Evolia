using Evolia.Model;

namespace Evolia.GameScene
{
    /// <summary>
    /// 生物のフィルタ
    /// </summary>
    public enum LifeFilter
    {
        Microbe,
        Plant,
        AquaticAnimal,
        TerrestrialAnimal
    }

    public static class LifeFilterExt
    {
        public static bool Match(this LifeFilter filter, in Species spec)
        {
            if (spec.id == 0)
            {
                return false;
            }

            switch (filter)
            {
                case LifeFilter.Microbe:
                    return spec.layer == LifeLayer.Microbe;
                case LifeFilter.Plant:
                    return spec.layer == LifeLayer.Plant;
                case LifeFilter.AquaticAnimal:
                    return spec.layer == LifeLayer.Animal && ((spec.preferences[(int)NicheAxis.Elevation][1].x + spec.preferences[(int)NicheAxis.Elevation][2].x) / 2 < 0);
                case LifeFilter.TerrestrialAnimal:
                    return spec.layer == LifeLayer.Animal && ((spec.preferences[(int)NicheAxis.Elevation][1].x + spec.preferences[(int)NicheAxis.Elevation][2].x) / 2 >= 0);
                default:
                    throw new System.NotImplementedException($"filter {filter} is not implemented");
            }
        }

        public static bool Found(this LifeFilter filter, Planet planet)
        {
            foreach (Species spec in CoL.instance.species)
            {
                if (filter.Match(spec) && planet.Found(spec))
                {
                    return true;
                }
            }
            return false;
        }
        public static string Label(this LifeFilter filter)
        {
            switch (filter)
            {
                case LifeFilter.Microbe:
                    return "微生物";
                case LifeFilter.Plant:
                    return "植物";
                case LifeFilter.AquaticAnimal:
                    return "水生動物";
                case LifeFilter.TerrestrialAnimal:
                    return "陸生動物";
                default:
                    throw new System.NotImplementedException($"filter {filter} is not implemented");
            }
        }
    }
}