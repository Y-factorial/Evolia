
using System;

namespace Evolia.GameScene
{
    /// <summary>
    /// ゲームスピード
    /// </summary>
    public enum GameSpeed
    {
        Pause,
        Normal,
        Fast,
        Faster,
        SuperFast
    }

    public static class GameSpeedExt
    {
        public static int LoopCount(this GameSpeed v)
        {
            switch (v)
            {
                case GameSpeed.Pause: return 0;
                case GameSpeed.Normal: return 1;
                case GameSpeed.Fast: return 2;
                case GameSpeed.Faster: return 4;
                case GameSpeed.SuperFast: return 32;
                default:
                    throw new NotImplementedException($"GameSpeed {v} is not implemented");
            }
        }

        public static float WaitTime(this GameSpeed v)
        {
            switch (v)
            {
                case GameSpeed.Pause: return 1.0f;
                case GameSpeed.Normal: return 1.0f;
                case GameSpeed.Fast: return 0.5f;
                case GameSpeed.Faster: return 0.25f;
                case GameSpeed.SuperFast: return 0.0f;
                default:
                    throw new NotImplementedException($"GameSpeed {v} is not implemented");
            }
        }

        public static string Message(this GameSpeed v)
        {
            switch (v)
            {
                case GameSpeed.Pause: return "停止";
                case GameSpeed.Normal: return "通常";
                case GameSpeed.Fast: return "速い";
                case GameSpeed.Faster: return "高速";
                case GameSpeed.SuperFast: return "超高速";
                default:
                    throw new NotImplementedException($"GameSpeed {v} is not implemented");
            }
        }
    }
}