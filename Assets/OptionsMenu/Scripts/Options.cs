
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Evolia.Shared
{
    public enum SmoothingMode
    {
        None,
        Tile,
        Vertex
    }

    public static class SmoothingModeExt
    {
        public static List<string> Keywords(this SmoothingMode mode)
        {
            switch (mode)
            {
                case SmoothingMode.None:
                    return new List<string> { "_" };
                case SmoothingMode.Tile:
                    return new List<string> { "_USE_INTERPORATION_TILE" };
                case SmoothingMode.Vertex:
                    return new List<string> { "_USE_INTERPORATION_TILE", "_USE_INTERPORATION_VERTEX" };
                default:
                    throw new NotImplementedException($"SmoothingMode {mode} is not implemented.");
            }
        }
    }

    public static class Options
    {
        public static ScreenOrientation screenOrienttion;
        public static SmoothingMode smoothSurface;
        public static bool tierScaling;
        public static bool godMode;
        public static bool limitFps;
    }
}