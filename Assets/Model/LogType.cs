namespace Evolia.Model
{
    public enum LogType
    {
        // 惑星の形成
        PlanetFormation,

        // 地表の形成
        SurfaceFormation,

        // 海洋の形成
        OceanFormation,

        // 種の誕生
        Speciation,

        // 縞状鉄鉱層の形成
        BandedIronFormation,

        // 酸素濃度が徐々に増加（GOEの前段階）
        PreGreatOxidationEvent,

        // 大酸化イベント
        GreatOxidationEvent,
        
        // 植物の上陸
        EmergenceOfTerrestrialPlants,

        // 動物の上陸
        EmergenceOfTerrestrialAnimals,

        // 文明化
        Civilization,

        // 太陽の膨張
        SolarInflation,

        // 海洋の蒸発
        OceanEvaporation,

        // 生命の終わり
        EndOfLife,

        // 地表の融解
        SurfaceMeltdown,
    }
}