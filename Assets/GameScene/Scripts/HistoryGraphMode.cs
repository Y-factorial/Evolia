
namespace Evolia.GameScene
{
    /// <summary>
    /// 履歴グラフの表示モード
    /// </summary>
    public enum HistoryGraphMode
    {
        Atmosphere,
        Temperature,
        Metabolism,
        Population
    }

    public static class HistoryGraphModeExt
    {
        public static string Label(this HistoryGraphMode mode)
        {
            switch (mode)
            {
                case HistoryGraphMode.Atmosphere:
                    return "大気";
                case HistoryGraphMode.Temperature:
                    return "平均気温";
                case HistoryGraphMode.Metabolism:
                    return "代謝";
                case HistoryGraphMode.Population:
                    return "生物数";
                default:
                    throw new System.NotImplementedException($"HistoryGraphMode {mode} is not implemented.");
            }
        }
    }
}