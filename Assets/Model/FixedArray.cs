using System.Runtime.InteropServices;
using System;

namespace Evolia.Model
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct FixedArray3<T>
    {
        public T a0, a1, a2;

        //public static implicit operator Span<T>(FixedArray3<T> x) => MemoryMarshal.CreateSpan(ref x.a0, 3);

        public ref T this[int index] => ref MemoryMarshal.CreateSpan(ref a0, Length)[index];

        public int Length => 3;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct FixedArray4<T>
    {
        public T a0, a1, a2, a3;

        //public static implicit operator Span<T>(FixedArray4<T> x) => MemoryMarshal.CreateSpan(ref x.a0, 4);

        public ref T this[int index] => ref MemoryMarshal.CreateSpan(ref a0, Length)[index];

        public int Length => 4;
    }


    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct FixedArray8<T>
    {
        public T a0, a1, a2, a3, a4, a5, a6, a7;

        //public static implicit operator Span<T>(FixedArray8<T> x) => MemoryMarshal.CreateSpan(ref x.a0, 8);

        public ref T this[int index] => ref MemoryMarshal.CreateSpan(ref a0, Length)[index];

        public int Length => 8;
    }
    
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct FixedArray16<T>
    {
        public T a0, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15;

        //public static implicit operator Span<T>(FixedArray16<T> x) => MemoryMarshal.CreateSpan(ref x.a0, 16);

        public ref T this[int index] => ref MemoryMarshal.CreateSpan(ref a0, Length)[index];


        public int Length => 16;

    }


}