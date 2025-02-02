using System.Collections.Generic;
using System;
using UnityEngine;

namespace Evolia.Model
{
    [Serializable]
    public class DataSeries
    {
        [Serializable]
        public struct TimedValue
        {
            [SerializeField]
            public long age;

            [SerializeField]
            public long tick;

            [SerializeField]
            public float value;

            public TimedValue(long age, long tick, float value)
            {
                this.age = age;
                this.tick = tick;
                this.value = value;
            }
        }

        [SerializeField]
        public List<TimedValue> data = new List<TimedValue>();

        [SerializeField]
        public long count;

        public float GetCurrentValue(float defaultValue)
        {
            return data.Count == 0 ? defaultValue : data[data.Count - 1].value;
        }

        public void AddData(long age, long tick, float value)
        {
            // データをリストに追加
            data.Add(new TimedValue(age, tick, value));

            ++count;

            // 古いデータを間引く
            CompressData();

        }

        // 圧縮処理
        private void CompressData()
        {
            // レベル1削除はdeltaがN以上で、2の倍数の時に起きる
            // レベル2削除はレベル1削除がN回以上で、4の倍数の時に起きる
            // つまり delta >= N*2+N = N*3 で、4の倍数の時に起きる
            // レベル3削除はレベル2削除がN回以上で、8の倍数の時に起きる
            // つまり delta >= N*4+N*2+N = N*7 で、8の倍数の時に起きる
            // レベル4削除はレベル3削除がN回以上で、16の倍数の時に起きる
            // つまり delta >= N*8+N*4+N*2+N = N*15 で、16の倍数の時に起きる
            // 削除インデックスは後ろから数えて (N-1)*レベルである
            // 本来なら N*レベル-1 だが、順次削除していくので、N*レベル-1 -(レベル-1) となり、N*レベル-レベル となる

            // レベルから、削除数を一発で求めることもできるが、
            // どうせ削除時にループするのでそれほど無駄ではない

            int N = 60;

            for (int level = 1; level < 1000; ++level)
            {
                int pow = (int)Mathf.Pow(2, level);
                int minCount = N * (pow-1);
                if (count <= minCount)
                {
                    break;
                }

                if (count % pow == 0)
                {
                    int removeIndex = data.Count - 1 - (N - 1) * level;

                    // 削除する分は、次のデータに平均化する
                    TimedValue prev = this.data[removeIndex - 1];
                    prev.value = (prev.value + this.data[removeIndex].value) / 2;
                    this.data[removeIndex - 1] = prev;

                    this.data.RemoveAt(removeIndex);
                }
            }
        }


    }
}