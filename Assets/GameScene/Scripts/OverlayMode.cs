namespace Evolia.GameScene
{
    /// <summary>
    /// オーバーレイの表示モード
    /// </summary>
    public enum OverlayMode
    {
        None,
        Wind,
        UpDown,
        Temperature,
        Humidity,
        Happiness
    }

    public static class OverlayModeExt
    {
        public static string Message(this OverlayMode mode)
        {
            switch (mode)
            {
                case OverlayMode.None:
                    return "オーバーレイを非表示にします";
                case OverlayMode.Wind:
                    return "風向と風速を表示します";
                case OverlayMode.UpDown:
                    return "地形の変化を表示します";
                case OverlayMode.Temperature:
                    return "気温を表示します";
                case OverlayMode.Humidity:
                    return "湿度を表示します";
                case OverlayMode.Happiness:
                    return "幸福度を表示します";
                default:
                    throw new System.NotImplementedException($"OverlayMode {mode} is not implemented.");
            }
        }
    }
}