
using System;

namespace Evolia.Model.Algorithm
{

    /// <summary>
    /// 中点変位法でランダムな地形データを生成する。
    /// </summary>
    public class GeographicalRandom
    {
        /// <summary>
        ///  生成された地形データが格納される配列。
        /// </summary>
        readonly float[,] values;

        /// <summary>
        /// 地形データを生成するための乱数オブジェクト。
        /// </summary>
        readonly Random random;

        /// <summary>
        /// 生成する地形の単位。
        /// おおむねこの大きさで一つ起伏ができる。
        /// 
        /// このサイズの矩形の四隅は、周囲の影響を受けずに高さが設定される。
        /// 例えば (0,unitSize) の高さは (0,0) からも (0,unitSize*2) からも影響を受けない。
        /// </summary>
        readonly int unitSize;

        public GeographicalRandom(float[,] values, Random random, int unitSize)
        {
            this.values = values;
            this.random = random;
            this.unitSize = unitSize;
        }

        /// <summary>
        /// ランダムな地形を生成する。
        /// </summary>
        public void Next(float min, float max)
        {
            Next();

            Normalize(min, max);
        }

        /// <summary>
        /// ランダムな地形を生成する。
        /// </summary>
        public void Next()
        {
            // 全体を NaN で埋めておく
            UnsetAll();

            // 各地形単位ごとに、地形データを生成する
            GenrateAllTerrains();
        }

        /// <summary>
        /// 「同じマスを上書きしない」判定のために NaN を使用するため、全てのマスを NaN で埋めておく。
        /// </summary>
        void UnsetAll()
        {
            Each((x, y) =>
            {
                this.values[x, y] = float.NaN;
            });
        }

        /// <summary>
        /// 各地形単位ごとに、地形データを生成する
        /// </summary>
        void GenrateAllTerrains()
        {
            // マップの縦横に unitSize が何個入るか調べて、その個数に分割しながら地形生成する
            // 分割後最初の四隅は「変位」ではないので、周囲の地形の影響を受けない。
            int xCount = (int)Math.Ceiling(this.Width / (float)this.unitSize);
            int yCount = (int)Math.Ceiling(this.Height / (float)this.unitSize);
            for (int x = 0; x < xCount; ++x)
            {
                for (int y = 0; y < yCount; ++y)
                {
                    int xMin = this.XMax * x / xCount;
                    int yMin = this.YMax * y / yCount;
                    int xMax = this.XMax * (x + 1) / xCount;
                    int yMax = this.YMax * (y + 1) / yCount;

                    // 分割された範囲の地形データを生成する
                    GenerateTerrain(xMin, yMin, xMax, yMax);
                }
            }
        }

        /// <summary>
        /// 四隅と中点を設定していく
        /// </summary>
        void GenerateTerrain(int xMin, int yMin, int xMax, int yMax)
        {
            // 乱数の最大値
            float randomMax = 1;

            // 四隅を設定する
            SetCorners(xMin, yMin, xMax, yMax, randomMax);

            // 地形を精細化する
            DetailTerrain(xMin, yMin, xMax, yMax, randomMax);
        }

        /// <summary>
        /// 四隅を設定する
        /// </summary>
        void SetCorners(int xMin, int yMin, int xMax, int yMax, float randomMax)
        {
            // 左上、右上、左下、右下
            SetValue(xMin, yMin, Random(randomMax));
            SetValue(xMax, yMin, Random(randomMax));
            SetValue(xMin, yMax, Random(randomMax));
            SetValue(xMax, yMax, Random(randomMax));
        }

        /// <summary>
        /// 地形を精細化する
        /// </summary>
        void DetailTerrain(int xMin, int yMin, int xMax, int yMax, float moveMax)
        {
            // 中点が存在しないくらい小さな四角形になったら終了
            if (Math.Abs(xMax - xMin) <= 1 && Math.Abs(yMax - yMin) <= 1)
            {
                return;
            }

            // 中点の座標
            int xMid = (xMin + xMax) / 2;
            int yMid = (yMin + yMax) / 2;

            // 中点を動かす
            MoveMidpoints(xMin, yMin, xMid, yMid, xMax, yMax, moveMax);

            // 4つに分割した小さな四角形について、同じことを繰り返す
            // 範囲が半分になるので、動かす量も半分
            DetailDividedTerrains(xMin, yMin, xMid, yMid, xMax, yMax, moveMax / 2);
        }

        /// <summary>
        /// 中点を移動する
        /// 中点は、隣接頂点の平均＋ランダムで移動する
        /// </summary>
        void MoveMidpoints(int xMin, int yMin, int xMid, int yMid, int xMax, int yMax, float moveMax)
        {
            // 左、右、上、下、真ん中
            SetValue(xMin, yMid, (this.values[xMin, yMin] + this.values[xMin, yMax]) / 2 + Random(moveMax));
            SetValue(xMax, yMid, (this.values[xMax, yMin] + this.values[xMax, yMax]) / 2 + Random(moveMax));
            SetValue(xMid, yMin, (this.values[xMin, yMin] + this.values[xMax, yMin]) / 2 + Random(moveMax));
            SetValue(xMid, yMax, (this.values[xMin, yMax] + this.values[xMax, yMax]) / 2 + Random(moveMax));
            SetValue(xMid, yMid, (this.values[xMin, yMin] + this.values[xMin, yMax] + this.values[xMax, yMin] + this.values[xMax, yMax]) / 4 + Random(moveMax));
        }

        /// <summary>
        /// 四つに分割した四角形を、さらに精細化していく
        /// </summary>
        void DetailDividedTerrains(int xMin, int yMin, int xMid, int yMid, int xMax, int yMax, float moveMax)
        {
            // 左上、右上、左下、右下
            DetailTerrain(xMin, yMin, xMid, yMid, moveMax);
            DetailTerrain(xMid, yMin, xMax, yMid, moveMax);
            DetailTerrain(xMin, yMid, xMid, yMax, moveMax);
            DetailTerrain(xMid, yMid, xMax, yMax, moveMax);
        }

        /// <summary>
        /// -max から max までの乱数を作る。
        /// </summary>
        float Random(float max)
        {
            return (float)(this.random.NextDouble() * max * 2 - max );
        }

        /// <summary>
        /// 地形の値を設定する。
        /// 
        /// その座標の値が設定済みなら上書きしない。
        /// 極はすべての地点で同じ値を共有し、左端、右端も同じ値を共有する。
        /// </summary>
        void SetValue(int x, int y, float value)
        {
            if (!float.IsNaN(this.values[x, y]))
            {
                // 値が設定済みなら上書きしない
                return;
            }

            this.values[x, y] = value;

            if (y == 0 || y == this.YMax)
            {
                // 極地方は上下でくっついている
                SetOtherPoleValue(x, y, value);
            }

            if (x == 0 || x == this.XMax)
            {
                // 一番左端と右端もくっついている
                SetOtherSideValue(x, y, value);
            }
        }

        void SetOtherPoleValue(int x, int y, float value)
        {
            int y2 = this.YMax - y;

            this.values[x, y2] = value;

            if (x == 0 || x == this.XMax)
            {
                // 一番左端と右端もくっついている
                SetOtherSideValue(x, y2, value);
            }
        }

        void SetOtherSideValue(int x, int y, float value)
        {
            int x2 = this.XMax - x;

            this.values[x2, y] = value;
        }

        /// <summary>
        /// 地形データの乱数を指定された範囲に収まるようにする。
        /// </summary>
        public void Normalize(float min, float max)
        {
            // 現在の値の範囲を取得して
            (float vMin, float vMax) = GetValueRange();

            // 変換の係数を求める
            (float scale, float offset) = CalcScaleAndOffset(min, max, vMin, vMax);

            // 値をスケーリングする
            ScaleAndOffsetAll(scale, offset);
        }

        static (float scale, float offset) CalcScaleAndOffset(float toMin, float toMax, float fromMin, float fromMax)
        {
            float scale = (toMax - toMin) / (fromMax - fromMin);
            float offset = toMin - fromMin * scale;

            return (scale, offset);
        }

        /// <summary>
        /// 現在の値の範囲を取得する。
        /// </summary>
        (float vMin, float vMax) GetValueRange()
        {
            float vMin = float.MaxValue;
            float vMax = float.MinValue;

            Each((x, y) =>
            {
                vMin = Math.Min(vMin, this.values[x, y]);
                vMax = Math.Max(vMax, this.values[x, y]);
            });

            return (vMin, vMax);
        }

        /// <summary>
        /// 値をスケーリングする。
        /// </summary>
        void ScaleAndOffsetAll(float scale, float offset)
        {
            Each((x, y) =>
            {
                this.values[x, y] = this.values[x, y] * scale + offset;
            });
        }

        void Each(Action<int, int> action)
        {
            for (int x = 0; x < this.Width; ++x)
            {
                for (int y = 0; y < this.Height; ++y)
                {
                    action(x, y);
                }
            }
        }

        int Width => this.values.GetLength(0);

        int Height => this.values.GetLength(1);

        int XMax => this.Width - 1;

        int YMax => this.Height - 1;

        public float this[int x, int y] => this.values[x, y];

    }
}