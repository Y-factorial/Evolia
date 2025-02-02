namespace Evolia.GameScene
{
    /// <summary>
    /// ユーザが画面をタップした時に行う操作
    /// </summary>
    public enum ClickMode
    {
        Focus,

        Meteor,
        Volcano,
        Collapse,

        Abduction,
        Introduction,

        XRay,
    }

    public static class ClickModeExt
    {

        public static string Message(this ClickMode mode)
        {
            switch (mode)
            {
                case ClickMode.Focus:
                    return "クリック位置のタイルを選択します";
                case ClickMode.Meteor:
                    return "クリック位置に隕石を落とします";
                case ClickMode.Volcano:
                    return "クリック位置に火山を噴火させます";
                case ClickMode.Collapse:
                    return "クリック位置に大規模な陥没を起こします";
                case ClickMode.Abduction:
                    return "クリック位置の生物を捕獲します";
                case ClickMode.Introduction:
                    return "クリック位置に生物を放します";
                case ClickMode.XRay:
                    return "クリック位置の生物の突然変異を促します";
                default:
                    throw new System.NotImplementedException($"ClickMode {mode} is not implemented.");
            }
        }

        public static bool IsDanger(this ClickMode mode)
        {
            switch (mode)
            {
                case ClickMode.Focus:
                    return false;
                case ClickMode.Meteor:
                    return true;
                case ClickMode.Volcano:
                    return true;
                case ClickMode.Collapse:
                    return true;
                case ClickMode.Abduction:
                    return false;
                case ClickMode.Introduction:
                    return false;
                case ClickMode.XRay:
                    return false;
                default:
                    throw new System.NotImplementedException($"ClickMode {mode} is not implemented.");
            }
        }
    }
}