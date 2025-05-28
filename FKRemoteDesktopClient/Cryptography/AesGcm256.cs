using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Cryptography
{
    public interface ICipherParameters
    {
    }

    public class ParametersWithIV : ICipherParameters
    {
        private readonly ICipherParameters parameters;
        private readonly byte[] iv;
        public ICipherParameters Parameters => parameters;

        public ParametersWithIV(ICipherParameters parameters, byte[] iv)
            : this(parameters, iv, 0, iv.Length)
        {
        }

        public ParametersWithIV(ICipherParameters parameters, byte[] iv, int ivOff, int ivLen)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            if (iv == null)
            {
                throw new ArgumentNullException("iv");
            }
            this.parameters = parameters;
            this.iv = new byte[ivLen];
            Array.Copy(iv, ivOff, this.iv, 0, ivLen);
        }

        public byte[] GetIV()
        {
            return (byte[])iv.Clone();
        }
    }

    public class AeadParameters : ICipherParameters
    {
        private readonly byte[] associatedText;
        private readonly byte[] nonce;
        private readonly KeyParameter key;
        private readonly int macSize;
        public virtual KeyParameter Key => key;
        public virtual int MacSize => macSize;

        public AeadParameters(KeyParameter key, int macSize, byte[] nonce, byte[] associatedText)
        {
            this.key = key;
            this.nonce = nonce;
            this.macSize = macSize;
            this.associatedText = associatedText;
        }

        public virtual byte[] GetAssociatedText()
        {
            return associatedText;
        }

        public virtual byte[] GetNonce()
        {
            return nonce;
        }
    }

    public class KeyParameter : ICipherParameters
    {
        private readonly byte[] key;

        public KeyParameter(byte[] key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            this.key = (byte[])key.Clone();
        }

        public KeyParameter(byte[] key, int keyOff, int keyLen)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (keyOff < 0 || keyOff > key.Length)
            {
                throw new ArgumentOutOfRangeException("keyOff");
            }
            if (keyLen < 0 || keyOff + keyLen > key.Length)
            {
                throw new ArgumentOutOfRangeException("keyLen");
            }
            this.key = new byte[keyLen];
            Array.Copy(key, keyOff, this.key, 0, keyLen);
        }

        public byte[] GetKey()
        {
            return (byte[])key.Clone();
        }

        public bool FixedTimeEquals(byte[] data) => Arrays.FixedTimeEquals(key, data);
    }

    internal sealed class Pack
    {
        private Pack()
        {
        }

        internal static void UInt32_To_BE(uint n, byte[] bs)
        {
            bs[0] = (byte)(n >> 24);
            bs[1] = (byte)(n >> 16);
            bs[2] = (byte)(n >> 8);
            bs[3] = (byte)n;
        }

        internal static void UInt32_To_BE(uint n, byte[] bs, int off)
        {
            bs[off] = (byte)(n >> 24);
            bs[++off] = (byte)(n >> 16);
            bs[++off] = (byte)(n >> 8);
            bs[++off] = (byte)n;
        }

        internal static uint BE_To_UInt32(byte[] bs)
        {
            return (uint)((bs[0] << 24) | (bs[1] << 16) | (bs[2] << 8) | bs[3]);
        }

        internal static uint BE_To_UInt32(byte[] bs, int off)
        {
            return (uint)((bs[off] << 24) | (bs[++off] << 16) | (bs[++off] << 8) | bs[++off]);
        }

        internal static ulong BE_To_UInt64(byte[] bs)
        {
            uint num = BE_To_UInt32(bs);
            uint num2 = BE_To_UInt32(bs, 4);
            return ((ulong)num << 32) | num2;
        }

        internal static ulong BE_To_UInt64(byte[] bs, int off)
        {
            uint num = BE_To_UInt32(bs, off);
            uint num2 = BE_To_UInt32(bs, off + 4);
            return ((ulong)num << 32) | num2;
        }

        internal static void UInt64_To_BE(ulong n, byte[] bs)
        {
            UInt32_To_BE((uint)(n >> 32), bs);
            UInt32_To_BE((uint)n, bs, 4);
        }

        internal static void UInt64_To_BE(ulong n, byte[] bs, int off)
        {
            UInt32_To_BE((uint)(n >> 32), bs, off);
            UInt32_To_BE((uint)n, bs, off + 4);
        }

        internal static void UInt32_To_LE(uint n, byte[] bs)
        {
            bs[0] = (byte)n;
            bs[1] = (byte)(n >> 8);
            bs[2] = (byte)(n >> 16);
            bs[3] = (byte)(n >> 24);
        }

        internal static void UInt32_To_LE(uint n, byte[] bs, int off)
        {
            bs[off] = (byte)n;
            bs[++off] = (byte)(n >> 8);
            bs[++off] = (byte)(n >> 16);
            bs[++off] = (byte)(n >> 24);
        }

        internal static uint LE_To_UInt32(byte[] bs)
        {
            return (uint)(bs[0] | (bs[1] << 8) | (bs[2] << 16) | (bs[3] << 24));
        }

        internal static uint LE_To_UInt32(byte[] bs, int off)
        {
            return (uint)(bs[off] | (bs[++off] << 8) | (bs[++off] << 16) | (bs[++off] << 24));
        }

        internal static ulong LE_To_UInt64(byte[] bs)
        {
            uint num = LE_To_UInt32(bs);
            return ((ulong)LE_To_UInt32(bs, 4) << 32) | num;
        }

        internal static ulong LE_To_UInt64(byte[] bs, int off)
        {
            uint num = LE_To_UInt32(bs, off);
            return ((ulong)LE_To_UInt32(bs, off + 4) << 32) | num;
        }

        internal static void UInt64_To_LE(ulong n, byte[] bs)
        {
            UInt32_To_LE((uint)n, bs);
            UInt32_To_LE((uint)(n >> 32), bs, 4);
        }

        internal static void UInt64_To_LE(ulong n, byte[] bs, int off)
        {
            UInt32_To_LE((uint)n, bs, off);
            UInt32_To_LE((uint)(n >> 32), bs, off + 4);
        }
    }

    internal sealed class Arrays
    {
        private Arrays()
        {
        }

        public static bool AreEqual(bool[] a, bool[] b)
        {
            if (a == b)
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            return HaveSameContents(a, b);
        }

        public static bool AreEqual(char[] a, char[] b)
        {
            if (a == b)
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            return HaveSameContents(a, b);
        }

        public static bool AreEqual(byte[] a, byte[] b)
        {
            if (a == b)
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            return HaveSameContents(a, b);
        }

        [Obsolete("Use 'AreEqual' method instead")]
        public static bool AreSame(byte[] a, byte[] b)
        {
            return AreEqual(a, b);
        }

        public static bool ConstantTimeAreEqual(byte[] a, byte[] b)
        {
            int num = a.Length;
            if (num != b.Length)
            {
                return false;
            }
            int num2 = 0;
            while (num != 0)
            {
                num--;
                num2 |= (a[num] ^ b[num]);
            }
            return num2 == 0;
        }

        public static bool AreEqual(int[] a, int[] b)
        {
            if (a == b)
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }
            return HaveSameContents(a, b);
        }

        private static bool HaveSameContents(bool[] a, bool[] b)
        {
            int num = a.Length;
            if (num != b.Length)
            {
                return false;
            }
            while (num != 0)
            {
                num--;
                if (a[num] != b[num])
                {
                    return false;
                }
            }
            return true;
        }

        private static bool HaveSameContents(char[] a, char[] b)
        {
            int num = a.Length;
            if (num != b.Length)
            {
                return false;
            }
            while (num != 0)
            {
                num--;
                if (a[num] != b[num])
                {
                    return false;
                }
            }
            return true;
        }

        private static bool HaveSameContents(byte[] a, byte[] b)
        {
            int num = a.Length;
            if (num != b.Length)
            {
                return false;
            }
            while (num != 0)
            {
                num--;
                if (a[num] != b[num])
                {
                    return false;
                }
            }
            return true;
        }

        private static bool HaveSameContents(int[] a, int[] b)
        {
            int num = a.Length;
            if (num != b.Length)
            {
                return false;
            }
            while (num != 0)
            {
                num--;
                if (a[num] != b[num])
                {
                    return false;
                }
            }
            return true;
        }

        public static string ToString(object[] a)
        {
            StringBuilder stringBuilder = new StringBuilder(91);
            if (a.Length != 0)
            {
                stringBuilder.Append(a[0]);
                for (int i = 1; i < a.Length; i++)
                {
                    stringBuilder.Append(", ").Append(a[i]);
                }
            }
            stringBuilder.Append(']');
            return stringBuilder.ToString();
        }

        public static int GetHashCode(byte[] data)
        {
            if (data == null)
            {
                return 0;
            }
            int num = data.Length;
            int num2 = num + 1;
            while (--num >= 0)
            {
                num2 *= 257;
                num2 ^= data[num];
            }
            return num2;
        }

        public static byte[] Clone(byte[] data)
        {
            if (data != null)
            {
                return (byte[])data.Clone();
            }
            return null;
        }

        public static int[] Clone(int[] data)
        {
            if (data != null)
            {
                return (int[])data.Clone();
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (null == a || null == b)
                return false;

            var len = a.Length;
            if (len != b.Length)
                return false;

            var d = 0;
            for (var i = 0; i < len; ++i)
            {
                d |= a[i] ^ b[i];
            }
            return 0 == d;
        }

        public static void Fill(byte[] buf, byte b)
        {
            var i = buf.Length;
            while (i > 0)
            {
                buf[--i] = b;
            }
        }
    }

    internal static class Bits
    {
#if NETSTANDARD1_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static uint BitPermuteStep(uint x, uint m, int s)
        {
            Debug.Assert((m & m << s) == 0U);
            Debug.Assert(m << s >> s == m);

            var t = (x ^ x >> s) & m;
            return t ^ t << s ^ x;
        }

#if NETSTANDARD1_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ulong BitPermuteStep(ulong x, ulong m, int s)
        {
            Debug.Assert((m & m << s) == 0UL);
            Debug.Assert(m << s >> s == m);

            var t = (x ^ x >> s) & m;
            return t ^ t << s ^ x;
        }

#if NETSTANDARD1_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static uint BitPermuteStepSimple(uint x, uint m, int s)
        {
            Debug.Assert(m << s == ~m);
            Debug.Assert((m & ~m) == 0U);

            return (x & m) << s | x >> s & m;
        }

#if NETSTANDARD1_0_OR_GREATER || NETCOREAPP1_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ulong BitPermuteStepSimple(ulong x, ulong m, int s)
        {
            Debug.Assert(m << s == ~m);
            Debug.Assert((m & ~m) == 0UL);

            return (x & m) << s | x >> s & m;
        }
    }

    internal static class Longs
    {
        public const int NumBits = 64;
        public const int NumBytes = 8;

        public static long LowestOneBit(long i) => i & -i;

        public static ulong Reverse(ulong i)
        {
            i = Bits.BitPermuteStepSimple(i, 0x5555555555555555UL, 1);
            i = Bits.BitPermuteStepSimple(i, 0x3333333333333333UL, 2);
            i = Bits.BitPermuteStepSimple(i, 0x0F0F0F0F0F0F0F0FUL, 4);
            return ReverseBytes(i);
        }

        public static ulong ReverseBytes(ulong i)
        {
            return RotateLeft(i & 0xFF000000FF000000UL, 8) |
                   RotateLeft(i & 0x00FF000000FF0000UL, 24) |
                   RotateLeft(i & 0x0000FF000000FF00UL, 40) |
                   RotateLeft(i & 0x000000FF000000FFUL, 56);
        }

        public static ulong RotateLeft(ulong i, int distance) =>
            i << distance | i >> -distance;

    }

    internal static class Interleave
    {
        private const ulong M64R = 0xAAAAAAAAAAAAAAAAUL;

        public static ulong Expand64To128Rev(ulong x, out ulong low)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Bmi2.X64.IsSupported)
            {
                low  = Bmi2.X64.ParallelBitDeposit(x >> 32, 0xAAAAAAAAAAAAAAAAUL);
                return Bmi2.X64.ParallelBitDeposit(x      , 0xAAAAAAAAAAAAAAAAUL);
            }
#endif

            // "shuffle" low half to even bits and high half to odd bits
            x = Bits.BitPermuteStep(x, 0x00000000FFFF0000UL, 16);
            x = Bits.BitPermuteStep(x, 0x0000FF000000FF00UL, 8);
            x = Bits.BitPermuteStep(x, 0x00F000F000F000F0UL, 4);
            x = Bits.BitPermuteStep(x, 0x0C0C0C0C0C0C0C0CUL, 2);
            x = Bits.BitPermuteStep(x, 0x2222222222222222UL, 1);

            low = x & M64R;
            return x << 1 & M64R;
        }
    }

    internal abstract class GcmUtilities
    {
        internal struct FieldElement
        {
            public ulong n0, n1;
        }

        private const uint E1 = 0xe1000000;
        private const ulong E1UL = (ulong)E1 << 32;

        internal static void One(out FieldElement x)
        {
            x.n0 = 1UL << 63;
            x.n1 = 0UL;
        }

        internal static void Square(ref FieldElement x)
        {
            var t1 = Interleave.Expand64To128Rev(x.n0, out var t0);
            var t3 = Interleave.Expand64To128Rev(x.n1, out var t2);

            Debug.Assert((t0 | t1 | t2 | t3) << 63 == 0UL);

            var z1 = t1 ^ t3 ^ t3 >> 1 ^ t3 >> 2 ^ t3 >> 7;
            var z2 = t2 ^ t3 << 62 ^ t3 << 57;

            x.n0 = t0 ^ z2 ^ z2 >> 1 ^ z2 >> 2 ^ z2 >> 7;
            x.n1 = z1 ^ t2 << 62 ^ t2 << 57;
        }

        internal static void AsFieldElement(byte[] x, out FieldElement z)
        {
            z.n0 = Pack.BE_To_UInt64(x, 0);
            z.n1 = Pack.BE_To_UInt64(x, 8);
        }

        internal static void AsBytes(ulong x0, ulong x1, byte[] z)
        {
            Pack.UInt64_To_BE(x0, z, 0);
            Pack.UInt64_To_BE(x1, z, 8);
        }

        internal static void MultiplyP7(ref FieldElement x)
        {
            ulong x0 = x.n0, x1 = x.n1;
            var c = x1 << 57;
            x.n0 = x0 >> 7 ^ c ^ c >> 1 ^ c >> 2 ^ c >> 7;
            x.n1 = x1 >> 7 | x0 << 57;
        }

        internal static void Multiply(ref FieldElement x, ref FieldElement y)
        {
            ulong z0, z1, z2;

            /*
             * "Three-way recursion" as described in "Batch binary Edwards", Daniel J. Bernstein.
             *
             * Without access to the high part of a 64x64 product x * y, we use a bit reversal to calculate it:
             *     rev(x) * rev(y) == rev((x * y) << 1) 
             */

            ulong x0 = x.n0, x1 = x.n1;
            ulong y0 = y.n0, y1 = y.n1;
            ulong x0r = Longs.Reverse(x0), x1r = Longs.Reverse(x1);
            ulong y0r = Longs.Reverse(y0), y1r = Longs.Reverse(y1);
            ulong z3;

            var h0 = Longs.Reverse(ImplMul64(x0r, y0r));
            var h1 = ImplMul64(x0, y0) << 1;
            var h2 = Longs.Reverse(ImplMul64(x1r, y1r));
            var h3 = ImplMul64(x1, y1) << 1;
            var h4 = Longs.Reverse(ImplMul64(x0r ^ x1r, y0r ^ y1r));
            var h5 = ImplMul64(x0 ^ x1, y0 ^ y1) << 1;

            z0 = h0;
            z1 = h1 ^ h0 ^ h2 ^ h4;
            z2 = h2 ^ h1 ^ h3 ^ h5;
            z3 = h3;

            Debug.Assert(z3 << 63 == 0);

            z1 ^= z3 ^ z3 >> 1 ^ z3 >> 2 ^ z3 >> 7;
            //              z2 ^=      (z3 << 63) ^ (z3 << 62) ^ (z3 << 57);
            z2 ^= z3 << 62 ^ z3 << 57;

            z0 ^= z2 ^ z2 >> 1 ^ z2 >> 2 ^ z2 >> 7;
            z1 ^= z2 << 63 ^ z2 << 62 ^ z2 << 57;

            x.n0 = z0;
            x.n1 = z1;
        }

        internal static ulong ImplMul64(ulong x, ulong y)
        {
            var x0 = x & 0x1111111111111111UL;
            var x1 = x & 0x2222222222222222UL;
            var x2 = x & 0x4444444444444444UL;
            var x3 = x & 0x8888888888888888UL;

            var y0 = y & 0x1111111111111111UL;
            var y1 = y & 0x2222222222222222UL;
            var y2 = y & 0x4444444444444444UL;
            var y3 = y & 0x8888888888888888UL;

            var z0 = x0 * y0 ^ x1 * y3 ^ x2 * y2 ^ x3 * y1;
            var z1 = x0 * y1 ^ x1 * y0 ^ x2 * y3 ^ x3 * y2;
            var z2 = x0 * y2 ^ x1 * y1 ^ x2 * y0 ^ x3 * y3;
            var z3 = x0 * y3 ^ x1 * y2 ^ x2 * y1 ^ x3 * y0;

            z0 &= 0x1111111111111111UL;
            z1 &= 0x2222222222222222UL;
            z2 &= 0x4444444444444444UL;
            z3 &= 0x8888888888888888UL;

            return z0 | z1 | z2 | z3;
        }

        internal static void DivideP(ref FieldElement x, out FieldElement z)
        {
            ulong x0 = x.n0, x1 = x.n1;
            var m = (ulong)((long)x0 >> 63);
            x0 ^= m & E1UL;
            z.n0 = x0 << 1 | x1 >> 63;
            z.n1 = x1 << 1 | (ulong)-(long)m;
        }

        internal static void Xor(ref FieldElement x, ref FieldElement y, out FieldElement z)
        {
            z.n0 = x.n0 ^ y.n0;
            z.n1 = x.n1 ^ y.n1;
        }

        internal static void AsBytes(ref FieldElement x, byte[] z) => AsBytes(x.n0, x.n1, z);

        internal static byte[] OneAsBytes()
        {
            return new byte[16]
            {
                128,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0
            };
        }

        internal static uint[] OneAsUints()
        {
            return new uint[4]
            {
                2147483648u,
                0u,
                0u,
                0u
            };
        }

        internal static uint[] AsUints(byte[] bs)
        {
            return new uint[4]
            {
                Pack.BE_To_UInt32(bs, 0),
                Pack.BE_To_UInt32(bs, 4),
                Pack.BE_To_UInt32(bs, 8),
                Pack.BE_To_UInt32(bs, 12)
            };
        }

        internal static void Multiply(byte[] block, byte[] val)
        {
            byte[] array = Arrays.Clone(block);
            byte[] array2 = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                byte b = val[i];
                for (int num = 7; num >= 0; num--)
                {
                    if ((b & (1 << num)) != 0)
                    {
                        Xor(array2, array);
                    }
                    bool num2 = (array[15] & 1) != 0;
                    ShiftRight(array);
                    if (num2)
                    {
                        array[0] ^= 225;
                    }
                }
            }
            Array.Copy(array2, 0, block, 0, 16);
        }

        internal static void MultiplyP(uint[] x)
        {
            bool num = (x[3] & 1) != 0;
            ShiftRight(x);
            if (num)
            {
                x[0] ^= 3774873600u;
            }
        }

        internal static void MultiplyP8(uint[] x)
        {
            uint num = x[3];
            ShiftRightN(x, 8);
            for (int num2 = 7; num2 >= 0; num2--)
            {
                if ((num & (1 << num2)) != 0L)
                {
                    x[0] ^= 3774873600u >> 7 - num2;
                }
            }
        }

        internal static void ShiftRight(byte[] block)
        {
            int num = 0;
            byte b = 0;
            while (true)
            {
                byte b2 = block[num];
                block[num] = (byte)((b2 >> 1) | b);
                if (++num != 16)
                {
                    b = (byte)(b2 << 7);
                    continue;
                }
                break;
            }
        }

        internal static void ShiftRight(uint[] block)
        {
            int num = 0;
            uint num2 = 0u;
            while (true)
            {
                uint num3 = block[num];
                block[num] = ((num3 >> 1) | num2);
                if (++num != 4)
                {
                    num2 = num3 << 31;
                    continue;
                }
                break;
            }
        }

        internal static void ShiftRightN(uint[] block, int n)
        {
            int num = 0;
            uint num2 = 0u;
            while (true)
            {
                uint num3 = block[num];
                block[num] = ((num3 >> n) | num2);
                if (++num != 4)
                {
                    num2 = num3 << 32 - n;
                    continue;
                }
                break;
            }
        }

        internal static void Xor(byte[] block, byte[] val)
        {
            for (int num = 15; num >= 0; num--)
            {
                block[num] ^= val[num];
            }
        }

        internal static void Xor(uint[] block, uint[] val)
        {
            for (int num = 3; num >= 0; num--)
            {
                block[num] ^= val[num];
            }
        }

        internal static void Xor(byte[] x, byte[] y, int yOff, int yLen)
        {
            while (--yLen >= 0)
            {
                x[yLen] ^= y[yOff + yLen];
            }
        }

        internal static void Xor(byte[] x, int xOff, byte[] y, int yOff, int len)
        {
            while (--len >= 0)
            {
                x[xOff + len] ^= y[yOff + len];
            }
        }

        internal static void Xor(byte[] x, byte[] y, int yOff)
        {
            var i = 0;
            do
            {
                x[i] ^= y[yOff + i];
                ++i;
                x[i] ^= y[yOff + i];
                ++i;
                x[i] ^= y[yOff + i];
                ++i;
                x[i] ^= y[yOff + i];
                ++i;
            }
            while (i < 16);
        }
    }

    public interface IGcmMultiplier
    {
        void Init(byte[] H);

        void MultiplyH(byte[] x);
    }

    public class Tables8kGcmMultiplier : IGcmMultiplier
    {
        private readonly uint[][][] M = new uint[32][][];

        public void Init(byte[] H)
        {
            M[0] = new uint[16][];
            M[1] = new uint[16][];
            M[0][0] = new uint[4];
            M[1][0] = new uint[4];
            M[1][8] = GcmUtilities.AsUints(H);
            for (int num = 4; num >= 1; num >>= 1)
            {
                uint[] array = (uint[])M[1][num + num].Clone();
                GcmUtilities.MultiplyP(array);
                M[1][num] = array;
            }
            uint[] array2 = (uint[])M[1][1].Clone();
            GcmUtilities.MultiplyP(array2);
            M[0][8] = array2;
            for (int num2 = 4; num2 >= 1; num2 >>= 1)
            {
                uint[] array3 = (uint[])M[0][num2 + num2].Clone();
                GcmUtilities.MultiplyP(array3);
                M[0][num2] = array3;
            }
            int num3 = 0;
            while (true)
            {
                for (int i = 2; i < 16; i += i)
                {
                    for (int j = 1; j < i; j++)
                    {
                        uint[] array4 = (uint[])M[num3][i].Clone();
                        GcmUtilities.Xor(array4, M[num3][j]);
                        M[num3][i + j] = array4;
                    }
                }
                if (++num3 == 32)
                {
                    break;
                }
                if (num3 > 1)
                {
                    M[num3] = new uint[16][];
                    M[num3][0] = new uint[4];
                    for (int num4 = 8; num4 > 0; num4 >>= 1)
                    {
                        uint[] array5 = (uint[])M[num3 - 2][num4].Clone();
                        GcmUtilities.MultiplyP8(array5);
                        M[num3][num4] = array5;
                    }
                }
            }
        }

        public void MultiplyH(byte[] x)
        {
            uint[] array = new uint[4];
            for (int num = 15; num >= 0; num--)
            {
                uint[] array2 = M[num + num][x[num] & 0xF];
                array[0] ^= array2[0];
                array[1] ^= array2[1];
                array[2] ^= array2[2];
                array[3] ^= array2[3];
                array2 = M[num + num + 1][(x[num] & 0xF0) >> 4];
                array[0] ^= array2[0];
                array[1] ^= array2[1];
                array[2] ^= array2[2];
                array[3] ^= array2[3];
            }
            Pack.UInt32_To_BE(array[0], x, 0);
            Pack.UInt32_To_BE(array[1], x, 4);
            Pack.UInt32_To_BE(array[2], x, 8);
            Pack.UInt32_To_BE(array[3], x, 12);
        }
    }

    public interface IBlockCipher
    {
        string AlgorithmName { get; }

        void Init(bool forEncryption, ICipherParameters parameters);

        int GetBlockSize();

        int ProcessBlock(byte[] inBuf, int inOff, byte[] outBuf, int outOff);
    }

    public class AesFastEngine : IBlockCipher
    {
        private static readonly byte[] S = new byte[256]
        {
            99,
            124,
            119,
            123,
            242,
            107,
            111,
            197,
            48,
            1,
            103,
            43,
            254,
            215,
            171,
            118,
            202,
            130,
            201,
            125,
            250,
            89,
            71,
            240,
            173,
            212,
            162,
            175,
            156,
            164,
            114,
            192,
            183,
            253,
            147,
            38,
            54,
            63,
            247,
            204,
            52,
            165,
            229,
            241,
            113,
            216,
            49,
            21,
            4,
            199,
            35,
            195,
            24,
            150,
            5,
            154,
            7,
            18,
            128,
            226,
            235,
            39,
            178,
            117,
            9,
            131,
            44,
            26,
            27,
            110,
            90,
            160,
            82,
            59,
            214,
            179,
            41,
            227,
            47,
            132,
            83,
            209,
            0,
            237,
            32,
            252,
            177,
            91,
            106,
            203,
            190,
            57,
            74,
            76,
            88,
            207,
            208,
            239,
            170,
            251,
            67,
            77,
            51,
            133,
            69,
            249,
            2,
            127,
            80,
            60,
            159,
            168,
            81,
            163,
            64,
            143,
            146,
            157,
            56,
            245,
            188,
            182,
            218,
            33,
            16,
            255,
            243,
            210,
            205,
            12,
            19,
            236,
            95,
            151,
            68,
            23,
            196,
            167,
            126,
            61,
            100,
            93,
            25,
            115,
            96,
            129,
            79,
            220,
            34,
            42,
            144,
            136,
            70,
            238,
            184,
            20,
            222,
            94,
            11,
            219,
            224,
            50,
            58,
            10,
            73,
            6,
            36,
            92,
            194,
            211,
            172,
            98,
            145,
            149,
            228,
            121,
            231,
            200,
            55,
            109,
            141,
            213,
            78,
            169,
            108,
            86,
            244,
            234,
            101,
            122,
            174,
            8,
            186,
            120,
            37,
            46,
            28,
            166,
            180,
            198,
            232,
            221,
            116,
            31,
            75,
            189,
            139,
            138,
            112,
            62,
            181,
            102,
            72,
            3,
            246,
            14,
            97,
            53,
            87,
            185,
            134,
            193,
            29,
            158,
            225,
            248,
            152,
            17,
            105,
            217,
            142,
            148,
            155,
            30,
            135,
            233,
            206,
            85,
            40,
            223,
            140,
            161,
            137,
            13,
            191,
            230,
            66,
            104,
            65,
            153,
            45,
            15,
            176,
            84,
            187,
            22
        };

        private static readonly byte[] Si = new byte[256]
        {
            82,
            9,
            106,
            213,
            48,
            54,
            165,
            56,
            191,
            64,
            163,
            158,
            129,
            243,
            215,
            251,
            124,
            227,
            57,
            130,
            155,
            47,
            255,
            135,
            52,
            142,
            67,
            68,
            196,
            222,
            233,
            203,
            84,
            123,
            148,
            50,
            166,
            194,
            35,
            61,
            238,
            76,
            149,
            11,
            66,
            250,
            195,
            78,
            8,
            46,
            161,
            102,
            40,
            217,
            36,
            178,
            118,
            91,
            162,
            73,
            109,
            139,
            209,
            37,
            114,
            248,
            246,
            100,
            134,
            104,
            152,
            22,
            212,
            164,
            92,
            204,
            93,
            101,
            182,
            146,
            108,
            112,
            72,
            80,
            253,
            237,
            185,
            218,
            94,
            21,
            70,
            87,
            167,
            141,
            157,
            132,
            144,
            216,
            171,
            0,
            140,
            188,
            211,
            10,
            247,
            228,
            88,
            5,
            184,
            179,
            69,
            6,
            208,
            44,
            30,
            143,
            202,
            63,
            15,
            2,
            193,
            175,
            189,
            3,
            1,
            19,
            138,
            107,
            58,
            145,
            17,
            65,
            79,
            103,
            220,
            234,
            151,
            242,
            207,
            206,
            240,
            180,
            230,
            115,
            150,
            172,
            116,
            34,
            231,
            173,
            53,
            133,
            226,
            249,
            55,
            232,
            28,
            117,
            223,
            110,
            71,
            241,
            26,
            113,
            29,
            41,
            197,
            137,
            111,
            183,
            98,
            14,
            170,
            24,
            190,
            27,
            252,
            86,
            62,
            75,
            198,
            210,
            121,
            32,
            154,
            219,
            192,
            254,
            120,
            205,
            90,
            244,
            31,
            221,
            168,
            51,
            136,
            7,
            199,
            49,
            177,
            18,
            16,
            89,
            39,
            128,
            236,
            95,
            96,
            81,
            127,
            169,
            25,
            181,
            74,
            13,
            45,
            229,
            122,
            159,
            147,
            201,
            156,
            239,
            160,
            224,
            59,
            77,
            174,
            42,
            245,
            176,
            200,
            235,
            187,
            60,
            131,
            83,
            153,
            97,
            23,
            43,
            4,
            126,
            186,
            119,
            214,
            38,
            225,
            105,
            20,
            99,
            85,
            33,
            12,
            125
        };

        private static readonly byte[] rcon = new byte[30]
        {
            1,
            2,
            4,
            8,
            16,
            32,
            64,
            128,
            27,
            54,
            108,
            216,
            171,
            77,
            154,
            47,
            94,
            188,
            99,
            198,
            151,
            53,
            106,
            212,
            179,
            125,
            250,
            239,
            197,
            145
        };

        private static readonly uint[] T0 = new uint[256]
        {
            2774754246u,
            2222750968u,
            2574743534u,
            2373680118u,
            234025727u,
            3177933782u,
            2976870366u,
            1422247313u,
            1345335392u,
            50397442u,
            2842126286u,
            2099981142u,
            436141799u,
            1658312629u,
            3870010189u,
            2591454956u,
            1170918031u,
            2642575903u,
            1086966153u,
            2273148410u,
            368769775u,
            3948501426u,
            3376891790u,
            200339707u,
            3970805057u,
            1742001331u,
            4255294047u,
            3937382213u,
            3214711843u,
            4154762323u,
            2524082916u,
            1539358875u,
            3266819957u,
            486407649u,
            2928907069u,
            1780885068u,
            1513502316u,
            1094664062u,
            49805301u,
            1338821763u,
            1546925160u,
            4104496465u,
            887481809u,
            150073849u,
            2473685474u,
            1943591083u,
            1395732834u,
            1058346282u,
            201589768u,
            1388824469u,
            1696801606u,
            1589887901u,
            672667696u,
            2711000631u,
            251987210u,
            3046808111u,
            151455502u,
            907153956u,
            2608889883u,
            1038279391u,
            652995533u,
            1764173646u,
            3451040383u,
            2675275242u,
            453576978u,
            2659418909u,
            1949051992u,
            773462580u,
            756751158u,
            2993581788u,
            3998898868u,
            4221608027u,
            4132590244u,
            1295727478u,
            1641469623u,
            3467883389u,
            2066295122u,
            1055122397u,
            1898917726u,
            2542044179u,
            4115878822u,
            1758581177u,
            0u,
            753790401u,
            1612718144u,
            536673507u,
            3367088505u,
            3982187446u,
            3194645204u,
            1187761037u,
            3653156455u,
            1262041458u,
            3729410708u,
            3561770136u,
            3898103984u,
            1255133061u,
            1808847035u,
            720367557u,
            3853167183u,
            385612781u,
            3309519750u,
            3612167578u,
            1429418854u,
            2491778321u,
            3477423498u,
            284817897u,
            100794884u,
            2172616702u,
            4031795360u,
            1144798328u,
            3131023141u,
            3819481163u,
            4082192802u,
            4272137053u,
            3225436288u,
            2324664069u,
            2912064063u,
            3164445985u,
            1211644016u,
            83228145u,
            3753688163u,
            3249976951u,
            1977277103u,
            1663115586u,
            806359072u,
            452984805u,
            250868733u,
            1842533055u,
            1288555905u,
            336333848u,
            890442534u,
            804056259u,
            3781124030u,
            2727843637u,
            3427026056u,
            957814574u,
            1472513171u,
            4071073621u,
            2189328124u,
            1195195770u,
            2892260552u,
            3881655738u,
            723065138u,
            2507371494u,
            2690670784u,
            2558624025u,
            3511635870u,
            2145180835u,
            1713513028u,
            2116692564u,
            2878378043u,
            2206763019u,
            3393603212u,
            703524551u,
            3552098411u,
            1007948840u,
            2044649127u,
            3797835452u,
            487262998u,
            1994120109u,
            1004593371u,
            1446130276u,
            1312438900u,
            503974420u,
            3679013266u,
            168166924u,
            1814307912u,
            3831258296u,
            1573044895u,
            1859376061u,
            4021070915u,
            2791465668u,
            2828112185u,
            2761266481u,
            937747667u,
            2339994098u,
            854058965u,
            1137232011u,
            1496790894u,
            3077402074u,
            2358086913u,
            1691735473u,
            3528347292u,
            3769215305u,
            3027004632u,
            4199962284u,
            133494003u,
            636152527u,
            2942657994u,
            2390391540u,
            3920539207u,
            403179536u,
            3585784431u,
            2289596656u,
            1864705354u,
            1915629148u,
            605822008u,
            4054230615u,
            3350508659u,
            1371981463u,
            602466507u,
            2094914977u,
            2624877800u,
            555687742u,
            3712699286u,
            3703422305u,
            2257292045u,
            2240449039u,
            2423288032u,
            1111375484u,
            3300242801u,
            2858837708u,
            3628615824u,
            84083462u,
            32962295u,
            302911004u,
            2741068226u,
            1597322602u,
            4183250862u,
            3501832553u,
            2441512471u,
            1489093017u,
            656219450u,
            3114180135u,
            954327513u,
            335083755u,
            3013122091u,
            856756514u,
            3144247762u,
            1893325225u,
            2307821063u,
            2811532339u,
            3063651117u,
            572399164u,
            2458355477u,
            552200649u,
            1238290055u,
            4283782570u,
            2015897680u,
            2061492133u,
            2408352771u,
            4171342169u,
            2156497161u,
            386731290u,
            3669999461u,
            837215959u,
            3326231172u,
            3093850320u,
            3275833730u,
            2962856233u,
            1999449434u,
            286199582u,
            3417354363u,
            4233385128u,
            3602627437u,
            974525996u
        };

        private static readonly uint[] T1 = new uint[256]
        {
            1667483301u,
            2088564868u,
            2004348569u,
            2071721613u,
            4076011277u,
            1802229437u,
            1869602481u,
            3318059348u,
            808476752u,
            16843267u,
            1734856361u,
            724260477u,
            4278118169u,
            3621238114u,
            2880130534u,
            1987505306u,
            3402272581u,
            2189565853u,
            3385428288u,
            2105408135u,
            4210749205u,
            1499050731u,
            1195871945u,
            4042324747u,
            2913812972u,
            3570709351u,
            2728550397u,
            2947499498u,
            2627478463u,
            2762232823u,
            1920132246u,
            3233848155u,
            3082253762u,
            4261273884u,
            2475900334u,
            640044138u,
            909536346u,
            1061125697u,
            4160222466u,
            3435955023u,
            875849820u,
            2779075060u,
            3857043764u,
            4059166984u,
            1903288979u,
            3638078323u,
            825320019u,
            353708607u,
            67373068u,
            3351745874u,
            589514341u,
            3284376926u,
            404238376u,
            2526427041u,
            84216335u,
            2593796021u,
            117902857u,
            303178806u,
            2155879323u,
            3806519101u,
            3958099238u,
            656887401u,
            2998042573u,
            1970662047u,
            151589403u,
            2206408094u,
            741103732u,
            437924910u,
            454768173u,
            1852759218u,
            1515893998u,
            2694863867u,
            1381147894u,
            993752653u,
            3604395873u,
            3014884814u,
            690573947u,
            3823361342u,
            791633521u,
            2223248279u,
            1397991157u,
            3520182632u,
            0u,
            3991781676u,
            538984544u,
            4244431647u,
            2981198280u,
            1532737261u,
            1785386174u,
            3419114822u,
            3200149465u,
            960066123u,
            1246401758u,
            1280088276u,
            1482207464u,
            3486483786u,
            3503340395u,
            4025468202u,
            2863288293u,
            4227591446u,
            1128498885u,
            1296931543u,
            859006549u,
            2240090516u,
            1162185423u,
            4193904912u,
            33686534u,
            2139094657u,
            1347461360u,
            1010595908u,
            2678007226u,
            2829601763u,
            1364304627u,
            2745392638u,
            1077969088u,
            2408514954u,
            2459058093u,
            2644320700u,
            943222856u,
            4126535940u,
            3166462943u,
            3065411521u,
            3671764853u,
            555827811u,
            269492272u,
            4294960410u,
            4092853518u,
            3537026925u,
            3452797260u,
            202119188u,
            320022069u,
            3974939439u,
            1600110305u,
            2543269282u,
            1145342156u,
            387395129u,
            3301217111u,
            2812761586u,
            2122251394u,
            1027439175u,
            1684326572u,
            1566423783u,
            421081643u,
            1936975509u,
            1616953504u,
            2172721560u,
            1330618065u,
            3705447295u,
            572671078u,
            707417214u,
            2425371563u,
            2290617219u,
            1179028682u,
            4008625961u,
            3099093971u,
            336865340u,
            3739133817u,
            1583267042u,
            185275933u,
            3688607094u,
            3772832571u,
            842163286u,
            976909390u,
            168432670u,
            1229558491u,
            101059594u,
            606357612u,
            1549580516u,
            3267534685u,
            3553869166u,
            2896970735u,
            1650640038u,
            2442213800u,
            2509582756u,
            3840201527u,
            2038035083u,
            3890730290u,
            3368586051u,
            926379609u,
            1835915959u,
            2374828428u,
            3587551588u,
            1313774802u,
            2846444000u,
            1819072692u,
            1448520954u,
            4109693703u,
            3941256997u,
            1701169839u,
            2054878350u,
            2930657257u,
            134746136u,
            3132780501u,
            2021191816u,
            623200879u,
            774790258u,
            471611428u,
            2795919345u,
            3031724999u,
            3334903633u,
            3907570467u,
            3722289532u,
            1953818780u,
            522141217u,
            1263245021u,
            3183305180u,
            2341145990u,
            2324303749u,
            1886445712u,
            1044282434u,
            3048567236u,
            1718013098u,
            1212715224u,
            50529797u,
            4143380225u,
            235805714u,
            1633796771u,
            892693087u,
            1465364217u,
            3115936208u,
            2256934801u,
            3250690392u,
            488454695u,
            2661164985u,
            3789674808u,
            4177062675u,
            2560109491u,
            286335539u,
            1768542907u,
            3654920560u,
            2391672713u,
            2492740519u,
            2610638262u,
            505297954u,
            2273777042u,
            3924412704u,
            3469641545u,
            1431677695u,
            673730680u,
            3755976058u,
            2357986191u,
            2711706104u,
            2307459456u,
            218962455u,
            3216991706u,
            3873888049u,
            1111655622u,
            1751699640u,
            1094812355u,
            2576951728u,
            757946999u,
            252648977u,
            2964356043u,
            1414834428u,
            3149622742u,
            370551866u
        };

        private static readonly uint[] T2 = new uint[256]
        {
            1673962851u,
            2096661628u,
            2012125559u,
            2079755643u,
            4076801522u,
            1809235307u,
            1876865391u,
            3314635973u,
            811618352u,
            16909057u,
            1741597031u,
            727088427u,
            4276558334u,
            3618988759u,
            2874009259u,
            1995217526u,
            3398387146u,
            2183110018u,
            3381215433u,
            2113570685u,
            4209972730u,
            1504897881u,
            1200539975u,
            4042984432u,
            2906778797u,
            3568527316u,
            2724199842u,
            2940594863u,
            2619588508u,
            2756966308u,
            1927583346u,
            3231407040u,
            3077948087u,
            4259388669u,
            2470293139u,
            642542118u,
            913070646u,
            1065238847u,
            4160029431u,
            3431157708u,
            879254580u,
            2773611685u,
            3855693029u,
            4059629809u,
            1910674289u,
            3635114968u,
            828527409u,
            355090197u,
            67636228u,
            3348452039u,
            591815971u,
            3281870531u,
            405809176u,
            2520228246u,
            84545285u,
            2586817946u,
            118360327u,
            304363026u,
            2149292928u,
            3806281186u,
            3956090603u,
            659450151u,
            2994720178u,
            1978310517u,
            152181513u,
            2199756419u,
            743994412u,
            439627290u,
            456535323u,
            1859957358u,
            1521806938u,
            2690382752u,
            1386542674u,
            997608763u,
            3602342358u,
            3011366579u,
            693271337u,
            3822927587u,
            794718511u,
            2215876484u,
            1403450707u,
            3518589137u,
            0u,
            3988860141u,
            541089824u,
            4242743292u,
            2977548465u,
            1538714971u,
            1792327274u,
            3415033547u,
            3194476990u,
            963791673u,
            1251270218u,
            1285084236u,
            1487988824u,
            3481619151u,
            3501943760u,
            4022676207u,
            2857362858u,
            4226619131u,
            1132905795u,
            1301993293u,
            862344499u,
            2232521861u,
            1166724933u,
            4192801017u,
            33818114u,
            2147385727u,
            1352724560u,
            1014514748u,
            2670049951u,
            2823545768u,
            1369633617u,
            2740846243u,
            1082179648u,
            2399505039u,
            2453646738u,
            2636233885u,
            946882616u,
            4126213365u,
            3160661948u,
            3061301686u,
            3668932058u,
            557998881u,
            270544912u,
            4293204735u,
            4093447923u,
            3535760850u,
            3447803085u,
            202904588u,
            321271059u,
            3972214764u,
            1606345055u,
            2536874647u,
            1149815876u,
            388905239u,
            3297990596u,
            2807427751u,
            2130477694u,
            1031423805u,
            1690872932u,
            1572530013u,
            422718233u,
            1944491379u,
            1623236704u,
            2165938305u,
            1335808335u,
            3701702620u,
            574907938u,
            710180394u,
            2419829648u,
            2282455944u,
            1183631942u,
            4006029806u,
            3094074296u,
            338181140u,
            3735517662u,
            1589437022u,
            185998603u,
            3685578459u,
            3772464096u,
            845436466u,
            980700730u,
            169090570u,
            1234361161u,
            101452294u,
            608726052u,
            1555620956u,
            3265224130u,
            3552407251u,
            2890133420u,
            1657054818u,
            2436475025u,
            2503058581u,
            3839047652u,
            2045938553u,
            3889509095u,
            3364570056u,
            929978679u,
            1843050349u,
            2365688973u,
            3585172693u,
            1318900302u,
            2840191145u,
            1826141292u,
            1454176854u,
            4109567988u,
            3939444202u,
            1707781989u,
            2062847610u,
            2923948462u,
            135272456u,
            3127891386u,
            2029029496u,
            625635109u,
            777810478u,
            473441308u,
            2790781350u,
            3027486644u,
            3331805638u,
            3905627112u,
            3718347997u,
            1961401460u,
            524165407u,
            1268178251u,
            3177307325u,
            2332919435u,
            2316273034u,
            1893765232u,
            1048330814u,
            3044132021u,
            1724688998u,
            1217452104u,
            50726147u,
            4143383030u,
            236720654u,
            1640145761u,
            896163637u,
            1471084887u,
            3110719673u,
            2249691526u,
            3248052417u,
            490350365u,
            2653403550u,
            3789109473u,
            4176155640u,
            2553000856u,
            287453969u,
            1775418217u,
            3651760345u,
            2382858638u,
            2486413204u,
            2603464347u,
            507257374u,
            2266337927u,
            3922272489u,
            3464972750u,
            1437269845u,
            676362280u,
            3752164063u,
            2349043596u,
            2707028129u,
            2299101321u,
            219813645u,
            3211123391u,
            3872862694u,
            1115997762u,
            1758509160u,
            1099088705u,
            2569646233u,
            760903469u,
            253628687u,
            2960903088u,
            1420360788u,
            3144537787u,
            371997206u
        };

        private static readonly uint[] T3 = new uint[256]
        {
            3332727651u,
            4169432188u,
            4003034999u,
            4136467323u,
            4279104242u,
            3602738027u,
            3736170351u,
            2438251973u,
            1615867952u,
            33751297u,
            3467208551u,
            1451043627u,
            3877240574u,
            3043153879u,
            1306962859u,
            3969545846u,
            2403715786u,
            530416258u,
            2302724553u,
            4203183485u,
            4011195130u,
            3001768281u,
            2395555655u,
            4211863792u,
            1106029997u,
            3009926356u,
            1610457762u,
            1173008303u,
            599760028u,
            1408738468u,
            3835064946u,
            2606481600u,
            1975695287u,
            3776773629u,
            1034851219u,
            1282024998u,
            1817851446u,
            2118205247u,
            4110612471u,
            2203045068u,
            1750873140u,
            1374987685u,
            3509904869u,
            4178113009u,
            3801313649u,
            2876496088u,
            1649619249u,
            708777237u,
            135005188u,
            2505230279u,
            1181033251u,
            2640233411u,
            807933976u,
            933336726u,
            168756485u,
            800430746u,
            235472647u,
            607523346u,
            463175808u,
            3745374946u,
            3441880043u,
            1315514151u,
            2144187058u,
            3936318837u,
            303761673u,
            496927619u,
            1484008492u,
            875436570u,
            908925723u,
            3702681198u,
            3035519578u,
            1543217312u,
            2767606354u,
            1984772923u,
            3076642518u,
            2110698419u,
            1383803177u,
            3711886307u,
            1584475951u,
            328696964u,
            2801095507u,
            3110654417u,
            0u,
            3240947181u,
            1080041504u,
            3810524412u,
            2043195825u,
            3069008731u,
            3569248874u,
            2370227147u,
            1742323390u,
            1917532473u,
            2497595978u,
            2564049996u,
            2968016984u,
            2236272591u,
            3144405200u,
            3307925487u,
            1340451498u,
            3977706491u,
            2261074755u,
            2597801293u,
            1716859699u,
            294946181u,
            2328839493u,
            3910203897u,
            67502594u,
            4269899647u,
            2700103760u,
            2017737788u,
            632987551u,
            1273211048u,
            2733855057u,
            1576969123u,
            2160083008u,
            92966799u,
            1068339858u,
            566009245u,
            1883781176u,
            4043634165u,
            1675607228u,
            2009183926u,
            2943736538u,
            1113792801u,
            540020752u,
            3843751935u,
            4245615603u,
            3211645650u,
            2169294285u,
            403966988u,
            641012499u,
            3274697964u,
            3202441055u,
            899848087u,
            2295088196u,
            775493399u,
            2472002756u,
            1441965991u,
            4236410494u,
            2051489085u,
            3366741092u,
            3135724893u,
            841685273u,
            3868554099u,
            3231735904u,
            429425025u,
            2664517455u,
            2743065820u,
            1147544098u,
            1417554474u,
            1001099408u,
            193169544u,
            2362066502u,
            3341414126u,
            1809037496u,
            675025940u,
            2809781982u,
            3168951902u,
            371002123u,
            2910247899u,
            3678134496u,
            1683370546u,
            1951283770u,
            337512970u,
            2463844681u,
            201983494u,
            1215046692u,
            3101973596u,
            2673722050u,
            3178157011u,
            1139780780u,
            3299238498u,
            967348625u,
            832869781u,
            3543655652u,
            4069226873u,
            3576883175u,
            2336475336u,
            1851340599u,
            3669454189u,
            25988493u,
            2976175573u,
            2631028302u,
            1239460265u,
            3635702892u,
            2902087254u,
            4077384948u,
            3475368682u,
            3400492389u,
            4102978170u,
            1206496942u,
            270010376u,
            1876277946u,
            4035475576u,
            1248797989u,
            1550986798u,
            941890588u,
            1475454630u,
            1942467764u,
            2538718918u,
            3408128232u,
            2709315037u,
            3902567540u,
            1042358047u,
            2531085131u,
            1641856445u,
            226921355u,
            260409994u,
            3767562352u,
            2084716094u,
            1908716981u,
            3433719398u,
            2430093384u,
            100991747u,
            4144101110u,
            470945294u,
            3265487201u,
            1784624437u,
            2935576407u,
            1775286713u,
            395413126u,
            2572730817u,
            975641885u,
            666476190u,
            3644383713u,
            3943954680u,
            733190296u,
            573772049u,
            3535497577u,
            2842745305u,
            126455438u,
            866620564u,
            766942107u,
            1008868894u,
            361924487u,
            3374377449u,
            2269761230u,
            2868860245u,
            1350051880u,
            2776293343u,
            59739276u,
            1509466529u,
            159418761u,
            437718285u,
            1708834751u,
            3610371814u,
            2227585602u,
            3501746280u,
            2193834305u,
            699439513u,
            1517759789u,
            504434447u,
            2076946608u,
            2835108948u,
            1842789307u,
            742004246u
        };

        private static readonly uint[] Tinv0 = new uint[256]
        {
            1353184337u,
            1399144830u,
            3282310938u,
            2522752826u,
            3412831035u,
            4047871263u,
            2874735276u,
            2466505547u,
            1442459680u,
            4134368941u,
            2440481928u,
            625738485u,
            4242007375u,
            3620416197u,
            2151953702u,
            2409849525u,
            1230680542u,
            1729870373u,
            2551114309u,
            3787521629u,
            41234371u,
            317738113u,
            2744600205u,
            3338261355u,
            3881799427u,
            2510066197u,
            3950669247u,
            3663286933u,
            763608788u,
            3542185048u,
            694804553u,
            1154009486u,
            1787413109u,
            2021232372u,
            1799248025u,
            3715217703u,
            3058688446u,
            397248752u,
            1722556617u,
            3023752829u,
            407560035u,
            2184256229u,
            1613975959u,
            1165972322u,
            3765920945u,
            2226023355u,
            480281086u,
            2485848313u,
            1483229296u,
            436028815u,
            2272059028u,
            3086515026u,
            601060267u,
            3791801202u,
            1468997603u,
            715871590u,
            120122290u,
            63092015u,
            2591802758u,
            2768779219u,
            4068943920u,
            2997206819u,
            3127509762u,
            1552029421u,
            723308426u,
            2461301159u,
            4042393587u,
            2715969870u,
            3455375973u,
            3586000134u,
            526529745u,
            2331944644u,
            2639474228u,
            2689987490u,
            853641733u,
            1978398372u,
            971801355u,
            2867814464u,
            111112542u,
            1360031421u,
            4186579262u,
            1023860118u,
            2919579357u,
            1186850381u,
            3045938321u,
            90031217u,
            1876166148u,
            4279586912u,
            620468249u,
            2548678102u,
            3426959497u,
            2006899047u,
            3175278768u,
            2290845959u,
            945494503u,
            3689859193u,
            1191869601u,
            3910091388u,
            3374220536u,
            0u,
            2206629897u,
            1223502642u,
            2893025566u,
            1316117100u,
            4227796733u,
            1446544655u,
            517320253u,
            658058550u,
            1691946762u,
            564550760u,
            3511966619u,
            976107044u,
            2976320012u,
            266819475u,
            3533106868u,
            2660342555u,
            1338359936u,
            2720062561u,
            1766553434u,
            370807324u,
            179999714u,
            3844776128u,
            1138762300u,
            488053522u,
            185403662u,
            2915535858u,
            3114841645u,
            3366526484u,
            2233069911u,
            1275557295u,
            3151862254u,
            4250959779u,
            2670068215u,
            3170202204u,
            3309004356u,
            880737115u,
            1982415755u,
            3703972811u,
            1761406390u,
            1676797112u,
            3403428311u,
            277177154u,
            1076008723u,
            538035844u,
            2099530373u,
            4164795346u,
            288553390u,
            1839278535u,
            1261411869u,
            4080055004u,
            3964831245u,
            3504587127u,
            1813426987u,
            2579067049u,
            4199060497u,
            577038663u,
            3297574056u,
            440397984u,
            3626794326u,
            4019204898u,
            3343796615u,
            3251714265u,
            4272081548u,
            906744984u,
            3481400742u,
            685669029u,
            646887386u,
            2764025151u,
            3835509292u,
            227702864u,
            2613862250u,
            1648787028u,
            3256061430u,
            3904428176u,
            1593260334u,
            4121936770u,
            3196083615u,
            2090061929u,
            2838353263u,
            3004310991u,
            999926984u,
            2809993232u,
            1852021992u,
            2075868123u,
            158869197u,
            4095236462u,
            28809964u,
            2828685187u,
            1701746150u,
            2129067946u,
            147831841u,
            3873969647u,
            3650873274u,
            3459673930u,
            3557400554u,
            3598495785u,
            2947720241u,
            824393514u,
            815048134u,
            3227951669u,
            935087732u,
            2798289660u,
            2966458592u,
            366520115u,
            1251476721u,
            4158319681u,
            240176511u,
            804688151u,
            2379631990u,
            1303441219u,
            1414376140u,
            3741619940u,
            3820343710u,
            461924940u,
            3089050817u,
            2136040774u,
            82468509u,
            1563790337u,
            1937016826u,
            776014843u,
            1511876531u,
            1389550482u,
            861278441u,
            323475053u,
            2355222426u,
            2047648055u,
            2383738969u,
            2302415851u,
            3995576782u,
            902390199u,
            3991215329u,
            1018251130u,
            1507840668u,
            1064563285u,
            2043548696u,
            3208103795u,
            3939366739u,
            1537932639u,
            342834655u,
            2262516856u,
            2180231114u,
            1053059257u,
            741614648u,
            1598071746u,
            1925389590u,
            203809468u,
            2336832552u,
            1100287487u,
            1895934009u,
            3736275976u,
            2632234200u,
            2428589668u,
            1636092795u,
            1890988757u,
            1952214088u,
            1113045200u
        };

        private static readonly uint[] Tinv1 = new uint[256]
        {
            2817806672u,
            1698790995u,
            2752977603u,
            1579629206u,
            1806384075u,
            1167925233u,
            1492823211u,
            65227667u,
            4197458005u,
            1836494326u,
            1993115793u,
            1275262245u,
            3622129660u,
            3408578007u,
            1144333952u,
            2741155215u,
            1521606217u,
            465184103u,
            250234264u,
            3237895649u,
            1966064386u,
            4031545618u,
            2537983395u,
            4191382470u,
            1603208167u,
            2626819477u,
            2054012907u,
            1498584538u,
            2210321453u,
            561273043u,
            1776306473u,
            3368652356u,
            2311222634u,
            2039411832u,
            1045993835u,
            1907959773u,
            1340194486u,
            2911432727u,
            2887829862u,
            986611124u,
            1256153880u,
            823846274u,
            860985184u,
            2136171077u,
            2003087840u,
            2926295940u,
            2692873756u,
            722008468u,
            1749577816u,
            4249194265u,
            1826526343u,
            4168831671u,
            3547573027u,
            38499042u,
            2401231703u,
            2874500650u,
            686535175u,
            3266653955u,
            2076542618u,
            137876389u,
            2267558130u,
            2780767154u,
            1778582202u,
            2182540636u,
            483363371u,
            3027871634u,
            4060607472u,
            3798552225u,
            4107953613u,
            3188000469u,
            1647628575u,
            4272342154u,
            1395537053u,
            1442030240u,
            3783918898u,
            3958809717u,
            3968011065u,
            4016062634u,
            2675006982u,
            275692881u,
            2317434617u,
            115185213u,
            88006062u,
            3185986886u,
            2371129781u,
            1573155077u,
            3557164143u,
            357589247u,
            4221049124u,
            3921532567u,
            1128303052u,
            2665047927u,
            1122545853u,
            2341013384u,
            1528424248u,
            4006115803u,
            175939911u,
            256015593u,
            512030921u,
            0u,
            2256537987u,
            3979031112u,
            1880170156u,
            1918528590u,
            4279172603u,
            948244310u,
            3584965918u,
            959264295u,
            3641641572u,
            2791073825u,
            1415289809u,
            775300154u,
            1728711857u,
            3881276175u,
            2532226258u,
            2442861470u,
            3317727311u,
            551313826u,
            1266113129u,
            437394454u,
            3130253834u,
            715178213u,
            3760340035u,
            387650077u,
            218697227u,
            3347837613u,
            2830511545u,
            2837320904u,
            435246981u,
            125153100u,
            3717852859u,
            1618977789u,
            637663135u,
            4117912764u,
            996558021u,
            2130402100u,
            692292470u,
            3324234716u,
            4243437160u,
            4058298467u,
            3694254026u,
            2237874704u,
            580326208u,
            298222624u,
            608863613u,
            1035719416u,
            855223825u,
            2703869805u,
            798891339u,
            817028339u,
            1384517100u,
            3821107152u,
            380840812u,
            3111168409u,
            1217663482u,
            1693009698u,
            2365368516u,
            1072734234u,
            746411736u,
            2419270383u,
            1313441735u,
            3510163905u,
            2731183358u,
            198481974u,
            2180359887u,
            3732579624u,
            2394413606u,
            3215802276u,
            2637835492u,
            2457358349u,
            3428805275u,
            1182684258u,
            328070850u,
            3101200616u,
            4147719774u,
            2948825845u,
            2153619390u,
            2479909244u,
            768962473u,
            304467891u,
            2578237499u,
            2098729127u,
            1671227502u,
            3141262203u,
            2015808777u,
            408514292u,
            3080383489u,
            2588902312u,
            1855317605u,
            3875515006u,
            3485212936u,
            3893751782u,
            2615655129u,
            913263310u,
            161475284u,
            2091919830u,
            2997105071u,
            591342129u,
            2493892144u,
            1721906624u,
            3159258167u,
            3397581990u,
            3499155632u,
            3634836245u,
            2550460746u,
            3672916471u,
            1355644686u,
            4136703791u,
            3595400845u,
            2968470349u,
            1303039060u,
            76997855u,
            3050413795u,
            2288667675u,
            523026872u,
            1365591679u,
            3932069124u,
            898367837u,
            1955068531u,
            1091304238u,
            493335386u,
            3537605202u,
            1443948851u,
            1205234963u,
            1641519756u,
            211892090u,
            351820174u,
            1007938441u,
            665439982u,
            3378624309u,
            3843875309u,
            2974251580u,
            3755121753u,
            1945261375u,
            3457423481u,
            935818175u,
            3455538154u,
            2868731739u,
            1866325780u,
            3678697606u,
            4088384129u,
            3295197502u,
            874788908u,
            1084473951u,
            3273463410u,
            635616268u,
            1228679307u,
            2500722497u,
            27801969u,
            3003910366u,
            3837057180u,
            3243664528u,
            2227927905u,
            3056784752u,
            1550600308u,
            1471729730u
        };

        private static readonly uint[] Tinv2 = new uint[256]
        {
            4098969767u,
            1098797925u,
            387629988u,
            658151006u,
            2872822635u,
            2636116293u,
            4205620056u,
            3813380867u,
            807425530u,
            1991112301u,
            3431502198u,
            49620300u,
            3847224535u,
            717608907u,
            891715652u,
            1656065955u,
            2984135002u,
            3123013403u,
            3930429454u,
            4267565504u,
            801309301u,
            1283527408u,
            1183687575u,
            3547055865u,
            2399397727u,
            2450888092u,
            1841294202u,
            1385552473u,
            3201576323u,
            1951978273u,
            3762891113u,
            3381544136u,
            3262474889u,
            2398386297u,
            1486449470u,
            3106397553u,
            3787372111u,
            2297436077u,
            550069932u,
            3464344634u,
            3747813450u,
            451248689u,
            1368875059u,
            1398949247u,
            1689378935u,
            1807451310u,
            2180914336u,
            150574123u,
            1215322216u,
            1167006205u,
            3734275948u,
            2069018616u,
            1940595667u,
            1265820162u,
            534992783u,
            1432758955u,
            3954313000u,
            3039757250u,
            3313932923u,
            936617224u,
            674296455u,
            3206787749u,
            50510442u,
            384654466u,
            3481938716u,
            2041025204u,
            133427442u,
            1766760930u,
            3664104948u,
            84334014u,
            886120290u,
            2797898494u,
            775200083u,
            4087521365u,
            2315596513u,
            4137973227u,
            2198551020u,
            1614850799u,
            1901987487u,
            1857900816u,
            557775242u,
            3717610758u,
            1054715397u,
            3863824061u,
            1418835341u,
            3295741277u,
            100954068u,
            1348534037u,
            2551784699u,
            3184957417u,
            1082772547u,
            3647436702u,
            3903896898u,
            2298972299u,
            434583643u,
            3363429358u,
            2090944266u,
            1115482383u,
            2230896926u,
            0u,
            2148107142u,
            724715757u,
            287222896u,
            1517047410u,
            251526143u,
            2232374840u,
            2923241173u,
            758523705u,
            252339417u,
            1550328230u,
            1536938324u,
            908343854u,
            168604007u,
            1469255655u,
            4004827798u,
            2602278545u,
            3229634501u,
            3697386016u,
            2002413899u,
            303830554u,
            2481064634u,
            2696996138u,
            574374880u,
            454171927u,
            151915277u,
            2347937223u,
            3056449960u,
            504678569u,
            4049044761u,
            1974422535u,
            2582559709u,
            2141453664u,
            33005350u,
            1918680309u,
            1715782971u,
            4217058430u,
            1133213225u,
            600562886u,
            3988154620u,
            3837289457u,
            836225756u,
            1665273989u,
            2534621218u,
            3330547729u,
            1250262308u,
            3151165501u,
            4188934450u,
            700935585u,
            2652719919u,
            3000824624u,
            2249059410u,
            3245854947u,
            3005967382u,
            1890163129u,
            2484206152u,
            3913753188u,
            4238918796u,
            4037024319u,
            2102843436u,
            857927568u,
            1233635150u,
            953795025u,
            3398237858u,
            3566745099u,
            4121350017u,
            2057644254u,
            3084527246u,
            2906629311u,
            976020637u,
            2018512274u,
            1600822220u,
            2119459398u,
            2381758995u,
            3633375416u,
            959340279u,
            3280139695u,
            1570750080u,
            3496574099u,
            3580864813u,
            634368786u,
            2898803609u,
            403744637u,
            2632478307u,
            1004239803u,
            650971512u,
            1500443672u,
            2599158199u,
            1334028442u,
            2514904430u,
            4289363686u,
            3156281551u,
            368043752u,
            3887782299u,
            1867173430u,
            2682967049u,
            2955531900u,
            2754719666u,
            1059729699u,
            2781229204u,
            2721431654u,
            1316239292u,
            2197595850u,
            2430644432u,
            2805143000u,
            82922136u,
            3963746266u,
            3447656016u,
            2434215926u,
            1299615190u,
            4014165424u,
            2865517645u,
            2531581700u,
            3516851125u,
            1783372680u,
            750893087u,
            1699118929u,
            1587348714u,
            2348899637u,
            2281337716u,
            201010753u,
            1739807261u,
            3683799762u,
            283718486u,
            3597472583u,
            3617229921u,
            2704767500u,
            4166618644u,
            334203196u,
            2848910887u,
            1639396809u,
            484568549u,
            1199193265u,
            3533461983u,
            4065673075u,
            337148366u,
            3346251575u,
            4149471949u,
            4250885034u,
            1038029935u,
            1148749531u,
            2949284339u,
            1756970692u,
            607661108u,
            2747424576u,
            488010435u,
            3803974693u,
            1009290057u,
            234832277u,
            2822336769u,
            201907891u,
            3034094820u,
            1449431233u,
            3413860740u,
            852848822u,
            1816687708u,
            3100656215u
        };

        private static readonly uint[] Tinv3 = new uint[256]
        {
            1364240372u,
            2119394625u,
            449029143u,
            982933031u,
            1003187115u,
            535905693u,
            2896910586u,
            1267925987u,
            542505520u,
            2918608246u,
            2291234508u,
            4112862210u,
            1341970405u,
            3319253802u,
            645940277u,
            3046089570u,
            3729349297u,
            627514298u,
            1167593194u,
            1575076094u,
            3271718191u,
            2165502028u,
            2376308550u,
            1808202195u,
            65494927u,
            362126482u,
            3219880557u,
            2514114898u,
            3559752638u,
            1490231668u,
            1227450848u,
            2386872521u,
            1969916354u,
            4101536142u,
            2573942360u,
            668823993u,
            3199619041u,
            4028083592u,
            3378949152u,
            2108963534u,
            1662536415u,
            3850514714u,
            2539664209u,
            1648721747u,
            2984277860u,
            3146034795u,
            4263288961u,
            4187237128u,
            1884842056u,
            2400845125u,
            2491903198u,
            1387788411u,
            2871251827u,
            1927414347u,
            3814166303u,
            1714072405u,
            2986813675u,
            788775605u,
            2258271173u,
            3550808119u,
            821200680u,
            598910399u,
            45771267u,
            3982262806u,
            2318081231u,
            2811409529u,
            4092654087u,
            1319232105u,
            1707996378u,
            114671109u,
            3508494900u,
            3297443494u,
            882725678u,
            2728416755u,
            87220618u,
            2759191542u,
            188345475u,
            1084944224u,
            1577492337u,
            3176206446u,
            1056541217u,
            2520581853u,
            3719169342u,
            1296481766u,
            2444594516u,
            1896177092u,
            74437638u,
            1627329872u,
            421854104u,
            3600279997u,
            2311865152u,
            1735892697u,
            2965193448u,
            126389129u,
            3879230233u,
            2044456648u,
            2705787516u,
            2095648578u,
            4173930116u,
            0u,
            159614592u,
            843640107u,
            514617361u,
            1817080410u,
            4261150478u,
            257308805u,
            1025430958u,
            908540205u,
            174381327u,
            1747035740u,
            2614187099u,
            607792694u,
            212952842u,
            2467293015u,
            3033700078u,
            463376795u,
            2152711616u,
            1638015196u,
            1516850039u,
            471210514u,
            3792353939u,
            3236244128u,
            1011081250u,
            303896347u,
            235605257u,
            4071475083u,
            767142070u,
            348694814u,
            1468340721u,
            2940995445u,
            4005289369u,
            2751291519u,
            4154402305u,
            1555887474u,
            1153776486u,
            1530167035u,
            2339776835u,
            3420243491u,
            3060333805u,
            3093557732u,
            3620396081u,
            1108378979u,
            322970263u,
            2216694214u,
            2239571018u,
            3539484091u,
            2920362745u,
            3345850665u,
            491466654u,
            3706925234u,
            233591430u,
            2010178497u,
            728503987u,
            2845423984u,
            301615252u,
            1193436393u,
            2831453436u,
            2686074864u,
            1457007741u,
            586125363u,
            2277985865u,
            3653357880u,
            2365498058u,
            2553678804u,
            2798617077u,
            2770919034u,
            3659959991u,
            1067761581u,
            753179962u,
            1343066744u,
            1788595295u,
            1415726718u,
            4139914125u,
            2431170776u,
            777975609u,
            2197139395u,
            2680062045u,
            1769771984u,
            1873358293u,
            3484619301u,
            3359349164u,
            279411992u,
            3899548572u,
            3682319163u,
            3439949862u,
            1861490777u,
            3959535514u,
            2208864847u,
            3865407125u,
            2860443391u,
            554225596u,
            4024887317u,
            3134823399u,
            1255028335u,
            3939764639u,
            701922480u,
            833598116u,
            707863359u,
            3325072549u,
            901801634u,
            1949809742u,
            4238789250u,
            3769684112u,
            857069735u,
            4048197636u,
            1106762476u,
            2131644621u,
            389019281u,
            1989006925u,
            1129165039u,
            3428076970u,
            3839820950u,
            2665723345u,
            1276872810u,
            3250069292u,
            1182749029u,
            2634345054u,
            22885772u,
            4201870471u,
            4214112523u,
            3009027431u,
            2454901467u,
            3912455696u,
            1829980118u,
            2592891351u,
            930745505u,
            1502483704u,
            3951639571u,
            3471714217u,
            3073755489u,
            3790464284u,
            2050797895u,
            2623135698u,
            1430221810u,
            410635796u,
            1941911495u,
            1407897079u,
            1599843069u,
            3742658365u,
            2022103876u,
            3397514159u,
            3107898472u,
            942421028u,
            3261022371u,
            376619805u,
            3154912738u,
            680216892u,
            4282488077u,
            963707304u,
            148812556u,
            3634160820u,
            1687208278u,
            2069988555u,
            3580933682u,
            1215585388u,
            3494008760u
        };

        private const uint m1 = 2155905152u;

        private const uint m2 = 2139062143u;

        private const uint m3 = 27u;

        private int ROUNDS;

        private uint[,] WorkingKey;

        private uint C0;

        private uint C1;

        private uint C2;

        private uint C3;

        private bool forEncryption;

        private const int BLOCK_SIZE = 16;

        public string AlgorithmName => "AES";

        private uint Shift(uint r, int shift)
        {
            return (r >> shift) | (r << 32 - shift);
        }

        private uint FFmulX(uint x)
        {
            return ((x & 0x7F7F7F7F) << 1) ^ (((uint)((int)x & -2139062144) >> 7) * 27);
        }

        private uint Inv_Mcol(uint x)
        {
            uint num = FFmulX(x);
            uint num2 = FFmulX(num);
            uint num3 = FFmulX(num2);
            uint num4 = x ^ num3;
            return num ^ num2 ^ num3 ^ Shift(num ^ num4, 8) ^ Shift(num2 ^ num4, 16) ^ Shift(num4, 24);
        }

        private uint SubWord(uint x)
        {
            return (uint)(S[x & 0xFF] | (S[(x >> 8) & 0xFF] << 8) | (S[(x >> 16) & 0xFF] << 16) | (S[(x >> 24) & 0xFF] << 24));
        }

        private uint[,] GenerateWorkingKey(byte[] key, bool forEncryption)
        {
            int num = key.Length / 4;
            if ((num != 4 && num != 6 && num != 8) || num * 4 != key.Length)
            {
                throw new ArgumentException("Key length not 128/192/256 bits.");
            }
            ROUNDS = num + 6;
            uint[,] array = new uint[ROUNDS + 1, 4];
            int num2 = 0;
            int num3 = 0;
            while (num3 < key.Length)
            {
                array[num2 >> 2, num2 & 3] = Pack.LE_To_UInt32(key, num3);
                num3 += 4;
                num2++;
            }
            int num4 = ROUNDS + 1 << 2;
            for (int i = num; i < num4; i++)
            {
                uint num5 = array[i - 1 >> 2, (i - 1) & 3];
                if (i % num == 0)
                {
                    num5 = (SubWord(Shift(num5, 8)) ^ rcon[i / num - 1]);
                }
                else if (num > 6 && i % num == 4)
                {
                    num5 = SubWord(num5);
                }
                array[i >> 2, i & 3] = (array[i - num >> 2, (i - num) & 3] ^ num5);
            }
            if (!forEncryption)
            {
                for (int j = 1; j < ROUNDS; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        array[j, k] = Inv_Mcol(array[j, k]);
                    }
                }
            }
            return array;
        }

        public void Init(bool forEncryption, ICipherParameters parameters)
        {
            if (!(parameters is KeyParameter))
            {
                throw new ArgumentException("invalid parameter passed to AES init - " + parameters.GetType().ToString());
            }
            WorkingKey = GenerateWorkingKey(((KeyParameter)parameters).GetKey(), forEncryption);
            this.forEncryption = forEncryption;
        }

        public int GetBlockSize()
        {
            return 16;
        }

        public int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
        {
            if (WorkingKey == null)
            {
                throw new InvalidOperationException("AES engine not initialised");
            }
            if (inOff + 16 > input.Length)
            {
                throw new DataLengthException("input buffer too short");
            }
            if (outOff + 16 > output.Length)
            {
                throw new DataLengthException("output buffer too short");
            }
            UnPackBlock(input, inOff);
            if (forEncryption)
            {
                EncryptBlock(WorkingKey);
            }
            else
            {
                DecryptBlock(WorkingKey);
            }
            PackBlock(output, outOff);
            return 16;
        }

        private void UnPackBlock(byte[] bytes, int off)
        {
            C0 = Pack.LE_To_UInt32(bytes, off);
            C1 = Pack.LE_To_UInt32(bytes, off + 4);
            C2 = Pack.LE_To_UInt32(bytes, off + 8);
            C3 = Pack.LE_To_UInt32(bytes, off + 12);
        }

        private void PackBlock(byte[] bytes, int off)
        {
            Pack.UInt32_To_LE(C0, bytes, off);
            Pack.UInt32_To_LE(C1, bytes, off + 4);
            Pack.UInt32_To_LE(C2, bytes, off + 8);
            Pack.UInt32_To_LE(C3, bytes, off + 12);
        }

        private void EncryptBlock(uint[,] KW)
        {
            C0 ^= KW[0, 0];
            C1 ^= KW[0, 1];
            C2 ^= KW[0, 2];
            C3 ^= KW[0, 3];
            int num = 1;
            uint num2;
            uint num3;
            uint num4;
            uint num5;
            while (num < ROUNDS - 1)
            {
                num2 = (T0[C0 & 0xFF] ^ T1[(C1 >> 8) & 0xFF] ^ T2[(C2 >> 16) & 0xFF] ^ T3[C3 >> 24] ^ KW[num, 0]);
                num3 = (T0[C1 & 0xFF] ^ T1[(C2 >> 8) & 0xFF] ^ T2[(C3 >> 16) & 0xFF] ^ T3[C0 >> 24] ^ KW[num, 1]);
                num4 = (T0[C2 & 0xFF] ^ T1[(C3 >> 8) & 0xFF] ^ T2[(C0 >> 16) & 0xFF] ^ T3[C1 >> 24] ^ KW[num, 2]);
                num5 = (T0[C3 & 0xFF] ^ T1[(C0 >> 8) & 0xFF] ^ T2[(C1 >> 16) & 0xFF] ^ T3[C2 >> 24] ^ KW[num++, 3]);
                C0 = (T0[num2 & 0xFF] ^ T1[(num3 >> 8) & 0xFF] ^ T2[(num4 >> 16) & 0xFF] ^ T3[num5 >> 24] ^ KW[num, 0]);
                C1 = (T0[num3 & 0xFF] ^ T1[(num4 >> 8) & 0xFF] ^ T2[(num5 >> 16) & 0xFF] ^ T3[num2 >> 24] ^ KW[num, 1]);
                C2 = (T0[num4 & 0xFF] ^ T1[(num5 >> 8) & 0xFF] ^ T2[(num2 >> 16) & 0xFF] ^ T3[num3 >> 24] ^ KW[num, 2]);
                C3 = (T0[num5 & 0xFF] ^ T1[(num2 >> 8) & 0xFF] ^ T2[(num3 >> 16) & 0xFF] ^ T3[num4 >> 24] ^ KW[num++, 3]);
            }
            num2 = (T0[C0 & 0xFF] ^ T1[(C1 >> 8) & 0xFF] ^ T2[(C2 >> 16) & 0xFF] ^ T3[C3 >> 24] ^ KW[num, 0]);
            num3 = (T0[C1 & 0xFF] ^ T1[(C2 >> 8) & 0xFF] ^ T2[(C3 >> 16) & 0xFF] ^ T3[C0 >> 24] ^ KW[num, 1]);
            num4 = (T0[C2 & 0xFF] ^ T1[(C3 >> 8) & 0xFF] ^ T2[(C0 >> 16) & 0xFF] ^ T3[C1 >> 24] ^ KW[num, 2]);
            num5 = (T0[C3 & 0xFF] ^ T1[(C0 >> 8) & 0xFF] ^ T2[(C1 >> 16) & 0xFF] ^ T3[C2 >> 24] ^ KW[num++, 3]);
            C0 = (uint)(S[num2 & 0xFF] ^ (S[(num3 >> 8) & 0xFF] << 8) ^ (S[(num4 >> 16) & 0xFF] << 16) ^ (S[num5 >> 24] << 24) ^ (int)KW[num, 0]);
            C1 = (uint)(S[num3 & 0xFF] ^ (S[(num4 >> 8) & 0xFF] << 8) ^ (S[(num5 >> 16) & 0xFF] << 16) ^ (S[num2 >> 24] << 24) ^ (int)KW[num, 1]);
            C2 = (uint)(S[num4 & 0xFF] ^ (S[(num5 >> 8) & 0xFF] << 8) ^ (S[(num2 >> 16) & 0xFF] << 16) ^ (S[num3 >> 24] << 24) ^ (int)KW[num, 2]);
            C3 = (uint)(S[num5 & 0xFF] ^ (S[(num2 >> 8) & 0xFF] << 8) ^ (S[(num3 >> 16) & 0xFF] << 16) ^ (S[num4 >> 24] << 24) ^ (int)KW[num, 3]);
        }

        private void DecryptBlock(uint[,] KW)
        {
            C0 ^= KW[ROUNDS, 0];
            C1 ^= KW[ROUNDS, 1];
            C2 ^= KW[ROUNDS, 2];
            C3 ^= KW[ROUNDS, 3];
            int num = ROUNDS - 1;
            uint num2;
            uint num3;
            uint num4;
            uint num5;
            while (num > 1)
            {
                num2 = (Tinv0[C0 & 0xFF] ^ Tinv1[(C3 >> 8) & 0xFF] ^ Tinv2[(C2 >> 16) & 0xFF] ^ Tinv3[C1 >> 24] ^ KW[num, 0]);
                num3 = (Tinv0[C1 & 0xFF] ^ Tinv1[(C0 >> 8) & 0xFF] ^ Tinv2[(C3 >> 16) & 0xFF] ^ Tinv3[C2 >> 24] ^ KW[num, 1]);
                num4 = (Tinv0[C2 & 0xFF] ^ Tinv1[(C1 >> 8) & 0xFF] ^ Tinv2[(C0 >> 16) & 0xFF] ^ Tinv3[C3 >> 24] ^ KW[num, 2]);
                num5 = (Tinv0[C3 & 0xFF] ^ Tinv1[(C2 >> 8) & 0xFF] ^ Tinv2[(C1 >> 16) & 0xFF] ^ Tinv3[C0 >> 24] ^ KW[num--, 3]);
                C0 = (Tinv0[num2 & 0xFF] ^ Tinv1[(num5 >> 8) & 0xFF] ^ Tinv2[(num4 >> 16) & 0xFF] ^ Tinv3[num3 >> 24] ^ KW[num, 0]);
                C1 = (Tinv0[num3 & 0xFF] ^ Tinv1[(num2 >> 8) & 0xFF] ^ Tinv2[(num5 >> 16) & 0xFF] ^ Tinv3[num4 >> 24] ^ KW[num, 1]);
                C2 = (Tinv0[num4 & 0xFF] ^ Tinv1[(num3 >> 8) & 0xFF] ^ Tinv2[(num2 >> 16) & 0xFF] ^ Tinv3[num5 >> 24] ^ KW[num, 2]);
                C3 = (Tinv0[num5 & 0xFF] ^ Tinv1[(num4 >> 8) & 0xFF] ^ Tinv2[(num3 >> 16) & 0xFF] ^ Tinv3[num2 >> 24] ^ KW[num--, 3]);
            }
            num2 = (Tinv0[C0 & 0xFF] ^ Tinv1[(C3 >> 8) & 0xFF] ^ Tinv2[(C2 >> 16) & 0xFF] ^ Tinv3[C1 >> 24] ^ KW[num, 0]);
            num3 = (Tinv0[C1 & 0xFF] ^ Tinv1[(C0 >> 8) & 0xFF] ^ Tinv2[(C3 >> 16) & 0xFF] ^ Tinv3[C2 >> 24] ^ KW[num, 1]);
            num4 = (Tinv0[C2 & 0xFF] ^ Tinv1[(C1 >> 8) & 0xFF] ^ Tinv2[(C0 >> 16) & 0xFF] ^ Tinv3[C3 >> 24] ^ KW[num, 2]);
            num5 = (Tinv0[C3 & 0xFF] ^ Tinv1[(C2 >> 8) & 0xFF] ^ Tinv2[(C1 >> 16) & 0xFF] ^ Tinv3[C0 >> 24] ^ KW[num, 3]);
            C0 = (uint)(Si[num2 & 0xFF] ^ (Si[(num5 >> 8) & 0xFF] << 8) ^ (Si[(num4 >> 16) & 0xFF] << 16) ^ (Si[num3 >> 24] << 24) ^ (int)KW[0, 0]);
            C1 = (uint)(Si[num3 & 0xFF] ^ (Si[(num2 >> 8) & 0xFF] << 8) ^ (Si[(num5 >> 16) & 0xFF] << 16) ^ (Si[num4 >> 24] << 24) ^ (int)KW[0, 1]);
            C2 = (uint)(Si[num4 & 0xFF] ^ (Si[(num3 >> 8) & 0xFF] << 8) ^ (Si[(num2 >> 16) & 0xFF] << 16) ^ (Si[num5 >> 24] << 24) ^ (int)KW[0, 2]);
            C3 = (uint)(Si[num5 & 0xFF] ^ (Si[(num4 >> 8) & 0xFF] << 8) ^ (Si[(num3 >> 16) & 0xFF] << 16) ^ (Si[num2 >> 24] << 24) ^ (int)KW[0, 3]);
        }
    }

    public static class Check
    {
        public static void DataLength(byte[] buf, int off, int len, string message)
        {
            if (off > buf.Length - len)
                ThrowDataLengthException(message);
        }

        public static void OutputLength(byte[] buf, int off, int len, string message)
        {
            if (off > buf.Length - len)
                ThrowOutputLengthException(message);
        }

        public static void ThrowDataLengthException(string message) => throw new DataLengthException(message);

        public static void ThrowOutputLengthException(string message) => throw new OutputLengthException(message);
    }

    public sealed class AesEngine : IBlockCipher
    {
        // The S box
        private static readonly byte[] S =
        {
            99, 124, 119, 123, 242, 107, 111, 197,
            48, 1, 103, 43, 254, 215, 171, 118,
            202, 130, 201, 125, 250, 89, 71, 240,
            173, 212, 162, 175, 156, 164, 114, 192,
            183, 253, 147, 38, 54, 63, 247, 204,
            52, 165, 229, 241, 113, 216, 49, 21,
            4, 199, 35, 195, 24, 150, 5, 154,
            7, 18, 128, 226, 235, 39, 178, 117,
            9, 131, 44, 26, 27, 110, 90, 160,
            82, 59, 214, 179, 41, 227, 47, 132,
            83, 209, 0, 237, 32, 252, 177, 91,
            106, 203, 190, 57, 74, 76, 88, 207,
            208, 239, 170, 251, 67, 77, 51, 133,
            69, 249, 2, 127, 80, 60, 159, 168,
            81, 163, 64, 143, 146, 157, 56, 245,
            188, 182, 218, 33, 16, 255, 243, 210,
            205, 12, 19, 236, 95, 151, 68, 23,
            196, 167, 126, 61, 100, 93, 25, 115,
            96, 129, 79, 220, 34, 42, 144, 136,
            70, 238, 184, 20, 222, 94, 11, 219,
            224, 50, 58, 10, 73, 6, 36, 92,
            194, 211, 172, 98, 145, 149, 228, 121,
            231, 200, 55, 109, 141, 213, 78, 169,
            108, 86, 244, 234, 101, 122, 174, 8,
            186, 120, 37, 46, 28, 166, 180, 198,
            232, 221, 116, 31, 75, 189, 139, 138,
            112, 62, 181, 102, 72, 3, 246, 14,
            97, 53, 87, 185, 134, 193, 29, 158,
            225, 248, 152, 17, 105, 217, 142, 148,
            155, 30, 135, 233, 206, 85, 40, 223,
            140, 161, 137, 13, 191, 230, 66, 104,
            65, 153, 45, 15, 176, 84, 187, 22,
        };

        // The inverse S-box
        private static readonly byte[] Si =
        {
            82, 9, 106, 213, 48, 54, 165, 56,
            191, 64, 163, 158, 129, 243, 215, 251,
            124, 227, 57, 130, 155, 47, 255, 135,
            52, 142, 67, 68, 196, 222, 233, 203,
            84, 123, 148, 50, 166, 194, 35, 61,
            238, 76, 149, 11, 66, 250, 195, 78,
            8, 46, 161, 102, 40, 217, 36, 178,
            118, 91, 162, 73, 109, 139, 209, 37,
            114, 248, 246, 100, 134, 104, 152, 22,
            212, 164, 92, 204, 93, 101, 182, 146,
            108, 112, 72, 80, 253, 237, 185, 218,
            94, 21, 70, 87, 167, 141, 157, 132,
            144, 216, 171, 0, 140, 188, 211, 10,
            247, 228, 88, 5, 184, 179, 69, 6,
            208, 44, 30, 143, 202, 63, 15, 2,
            193, 175, 189, 3, 1, 19, 138, 107,
            58, 145, 17, 65, 79, 103, 220, 234,
            151, 242, 207, 206, 240, 180, 230, 115,
            150, 172, 116, 34, 231, 173, 53, 133,
            226, 249, 55, 232, 28, 117, 223, 110,
            71, 241, 26, 113, 29, 41, 197, 137,
            111, 183, 98, 14, 170, 24, 190, 27,
            252, 86, 62, 75, 198, 210, 121, 32,
            154, 219, 192, 254, 120, 205, 90, 244,
            31, 221, 168, 51, 136, 7, 199, 49,
            177, 18, 16, 89, 39, 128, 236, 95,
            96, 81, 127, 169, 25, 181, 74, 13,
            45, 229, 122, 159, 147, 201, 156, 239,
            160, 224, 59, 77, 174, 42, 245, 176,
            200, 235, 187, 60, 131, 83, 153, 97,
            23, 43, 4, 126, 186, 119, 214, 38,
            225, 105, 20, 99, 85, 33, 12, 125,
        };

        // vector used in calculating key schedule (powers of x in GF(256))
        private static readonly byte[] rcon =
        {
            0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1b, 0x36, 0x6c, 0xd8, 0xab, 0x4d, 0x9a,
            0x2f, 0x5e, 0xbc, 0x63, 0xc6, 0x97, 0x35, 0x6a, 0xd4, 0xb3, 0x7d, 0xfa, 0xef, 0xc5, 0x91
        };

        // precomputation tables of calculations for rounds
        private static readonly uint[] T0 =
        {
            0xa56363c6, 0x847c7cf8, 0x997777ee, 0x8d7b7bf6, 0x0df2f2ff,
            0xbd6b6bd6, 0xb16f6fde, 0x54c5c591, 0x50303060, 0x03010102,
            0xa96767ce, 0x7d2b2b56, 0x19fefee7, 0x62d7d7b5, 0xe6abab4d,
            0x9a7676ec, 0x45caca8f, 0x9d82821f, 0x40c9c989, 0x877d7dfa,
            0x15fafaef, 0xeb5959b2, 0xc947478e, 0x0bf0f0fb, 0xecadad41,
            0x67d4d4b3, 0xfda2a25f, 0xeaafaf45, 0xbf9c9c23, 0xf7a4a453,
            0x967272e4, 0x5bc0c09b, 0xc2b7b775, 0x1cfdfde1, 0xae93933d,
            0x6a26264c, 0x5a36366c, 0x413f3f7e, 0x02f7f7f5, 0x4fcccc83,
            0x5c343468, 0xf4a5a551, 0x34e5e5d1, 0x08f1f1f9, 0x937171e2,
            0x73d8d8ab, 0x53313162, 0x3f15152a, 0x0c040408, 0x52c7c795,
            0x65232346, 0x5ec3c39d, 0x28181830, 0xa1969637, 0x0f05050a,
            0xb59a9a2f, 0x0907070e, 0x36121224, 0x9b80801b, 0x3de2e2df,
            0x26ebebcd, 0x6927274e, 0xcdb2b27f, 0x9f7575ea, 0x1b090912,
            0x9e83831d, 0x742c2c58, 0x2e1a1a34, 0x2d1b1b36, 0xb26e6edc,
            0xee5a5ab4, 0xfba0a05b, 0xf65252a4, 0x4d3b3b76, 0x61d6d6b7,
            0xceb3b37d, 0x7b292952, 0x3ee3e3dd, 0x712f2f5e, 0x97848413,
            0xf55353a6, 0x68d1d1b9, 0x00000000, 0x2cededc1, 0x60202040,
            0x1ffcfce3, 0xc8b1b179, 0xed5b5bb6, 0xbe6a6ad4, 0x46cbcb8d,
            0xd9bebe67, 0x4b393972, 0xde4a4a94, 0xd44c4c98, 0xe85858b0,
            0x4acfcf85, 0x6bd0d0bb, 0x2aefefc5, 0xe5aaaa4f, 0x16fbfbed,
            0xc5434386, 0xd74d4d9a, 0x55333366, 0x94858511, 0xcf45458a,
            0x10f9f9e9, 0x06020204, 0x817f7ffe, 0xf05050a0, 0x443c3c78,
            0xba9f9f25, 0xe3a8a84b, 0xf35151a2, 0xfea3a35d, 0xc0404080,
            0x8a8f8f05, 0xad92923f, 0xbc9d9d21, 0x48383870, 0x04f5f5f1,
            0xdfbcbc63, 0xc1b6b677, 0x75dadaaf, 0x63212142, 0x30101020,
            0x1affffe5, 0x0ef3f3fd, 0x6dd2d2bf, 0x4ccdcd81, 0x140c0c18,
            0x35131326, 0x2fececc3, 0xe15f5fbe, 0xa2979735, 0xcc444488,
            0x3917172e, 0x57c4c493, 0xf2a7a755, 0x827e7efc, 0x473d3d7a,
            0xac6464c8, 0xe75d5dba, 0x2b191932, 0x957373e6, 0xa06060c0,
            0x98818119, 0xd14f4f9e, 0x7fdcdca3, 0x66222244, 0x7e2a2a54,
            0xab90903b, 0x8388880b, 0xca46468c, 0x29eeeec7, 0xd3b8b86b,
            0x3c141428, 0x79dedea7, 0xe25e5ebc, 0x1d0b0b16, 0x76dbdbad,
            0x3be0e0db, 0x56323264, 0x4e3a3a74, 0x1e0a0a14, 0xdb494992,
            0x0a06060c, 0x6c242448, 0xe45c5cb8, 0x5dc2c29f, 0x6ed3d3bd,
            0xefacac43, 0xa66262c4, 0xa8919139, 0xa4959531, 0x37e4e4d3,
            0x8b7979f2, 0x32e7e7d5, 0x43c8c88b, 0x5937376e, 0xb76d6dda,
            0x8c8d8d01, 0x64d5d5b1, 0xd24e4e9c, 0xe0a9a949, 0xb46c6cd8,
            0xfa5656ac, 0x07f4f4f3, 0x25eaeacf, 0xaf6565ca, 0x8e7a7af4,
            0xe9aeae47, 0x18080810, 0xd5baba6f, 0x887878f0, 0x6f25254a,
            0x722e2e5c, 0x241c1c38, 0xf1a6a657, 0xc7b4b473, 0x51c6c697,
            0x23e8e8cb, 0x7cdddda1, 0x9c7474e8, 0x211f1f3e, 0xdd4b4b96,
            0xdcbdbd61, 0x868b8b0d, 0x858a8a0f, 0x907070e0, 0x423e3e7c,
            0xc4b5b571, 0xaa6666cc, 0xd8484890, 0x05030306, 0x01f6f6f7,
            0x120e0e1c, 0xa36161c2, 0x5f35356a, 0xf95757ae, 0xd0b9b969,
            0x91868617, 0x58c1c199, 0x271d1d3a, 0xb99e9e27, 0x38e1e1d9,
            0x13f8f8eb, 0xb398982b, 0x33111122, 0xbb6969d2, 0x70d9d9a9,
            0x898e8e07, 0xa7949433, 0xb69b9b2d, 0x221e1e3c, 0x92878715,
            0x20e9e9c9, 0x49cece87, 0xff5555aa, 0x78282850, 0x7adfdfa5,
            0x8f8c8c03, 0xf8a1a159, 0x80898909, 0x170d0d1a, 0xdabfbf65,
            0x31e6e6d7, 0xc6424284, 0xb86868d0, 0xc3414182, 0xb0999929,
            0x772d2d5a, 0x110f0f1e, 0xcbb0b07b, 0xfc5454a8, 0xd6bbbb6d,
            0x3a16162c
        };

        private static readonly uint[] Tinv0 =
        {
            0x50a7f451, 0x5365417e, 0xc3a4171a, 0x965e273a, 0xcb6bab3b,
            0xf1459d1f, 0xab58faac, 0x9303e34b, 0x55fa3020, 0xf66d76ad,
            0x9176cc88, 0x254c02f5, 0xfcd7e54f, 0xd7cb2ac5, 0x80443526,
            0x8fa362b5, 0x495ab1de, 0x671bba25, 0x980eea45, 0xe1c0fe5d,
            0x02752fc3, 0x12f04c81, 0xa397468d, 0xc6f9d36b, 0xe75f8f03,
            0x959c9215, 0xeb7a6dbf, 0xda595295, 0x2d83bed4, 0xd3217458,
            0x2969e049, 0x44c8c98e, 0x6a89c275, 0x78798ef4, 0x6b3e5899,
            0xdd71b927, 0xb64fe1be, 0x17ad88f0, 0x66ac20c9, 0xb43ace7d,
            0x184adf63, 0x82311ae5, 0x60335197, 0x457f5362, 0xe07764b1,
            0x84ae6bbb, 0x1ca081fe, 0x942b08f9, 0x58684870, 0x19fd458f,
            0x876cde94, 0xb7f87b52, 0x23d373ab, 0xe2024b72, 0x578f1fe3,
            0x2aab5566, 0x0728ebb2, 0x03c2b52f, 0x9a7bc586, 0xa50837d3,
            0xf2872830, 0xb2a5bf23, 0xba6a0302, 0x5c8216ed, 0x2b1ccf8a,
            0x92b479a7, 0xf0f207f3, 0xa1e2694e, 0xcdf4da65, 0xd5be0506,
            0x1f6234d1, 0x8afea6c4, 0x9d532e34, 0xa055f3a2, 0x32e18a05,
            0x75ebf6a4, 0x39ec830b, 0xaaef6040, 0x069f715e, 0x51106ebd,
            0xf98a213e, 0x3d06dd96, 0xae053edd, 0x46bde64d, 0xb58d5491,
            0x055dc471, 0x6fd40604, 0xff155060, 0x24fb9819, 0x97e9bdd6,
            0xcc434089, 0x779ed967, 0xbd42e8b0, 0x888b8907, 0x385b19e7,
            0xdbeec879, 0x470a7ca1, 0xe90f427c, 0xc91e84f8, 0x00000000,
            0x83868009, 0x48ed2b32, 0xac70111e, 0x4e725a6c, 0xfbff0efd,
            0x5638850f, 0x1ed5ae3d, 0x27392d36, 0x64d90f0a, 0x21a65c68,
            0xd1545b9b, 0x3a2e3624, 0xb1670a0c, 0x0fe75793, 0xd296eeb4,
            0x9e919b1b, 0x4fc5c080, 0xa220dc61, 0x694b775a, 0x161a121c,
            0x0aba93e2, 0xe52aa0c0, 0x43e0223c, 0x1d171b12, 0x0b0d090e,
            0xadc78bf2, 0xb9a8b62d, 0xc8a91e14, 0x8519f157, 0x4c0775af,
            0xbbdd99ee, 0xfd607fa3, 0x9f2601f7, 0xbcf5725c, 0xc53b6644,
            0x347efb5b, 0x7629438b, 0xdcc623cb, 0x68fcedb6, 0x63f1e4b8,
            0xcadc31d7, 0x10856342, 0x40229713, 0x2011c684, 0x7d244a85,
            0xf83dbbd2, 0x1132f9ae, 0x6da129c7, 0x4b2f9e1d, 0xf330b2dc,
            0xec52860d, 0xd0e3c177, 0x6c16b32b, 0x99b970a9, 0xfa489411,
            0x2264e947, 0xc48cfca8, 0x1a3ff0a0, 0xd82c7d56, 0xef903322,
            0xc74e4987, 0xc1d138d9, 0xfea2ca8c, 0x360bd498, 0xcf81f5a6,
            0x28de7aa5, 0x268eb7da, 0xa4bfad3f, 0xe49d3a2c, 0x0d927850,
            0x9bcc5f6a, 0x62467e54, 0xc2138df6, 0xe8b8d890, 0x5ef7392e,
            0xf5afc382, 0xbe805d9f, 0x7c93d069, 0xa92dd56f, 0xb31225cf,
            0x3b99acc8, 0xa77d1810, 0x6e639ce8, 0x7bbb3bdb, 0x097826cd,
            0xf418596e, 0x01b79aec, 0xa89a4f83, 0x656e95e6, 0x7ee6ffaa,
            0x08cfbc21, 0xe6e815ef, 0xd99be7ba, 0xce366f4a, 0xd4099fea,
            0xd67cb029, 0xafb2a431, 0x31233f2a, 0x3094a5c6, 0xc066a235,
            0x37bc4e74, 0xa6ca82fc, 0xb0d090e0, 0x15d8a733, 0x4a9804f1,
            0xf7daec41, 0x0e50cd7f, 0x2ff69117, 0x8dd64d76, 0x4db0ef43,
            0x544daacc, 0xdf0496e4, 0xe3b5d19e, 0x1b886a4c, 0xb81f2cc1,
            0x7f516546, 0x04ea5e9d, 0x5d358c01, 0x737487fa, 0x2e410bfb,
            0x5a1d67b3, 0x52d2db92, 0x335610e9, 0x1347d66d, 0x8c61d79a,
            0x7a0ca137, 0x8e14f859, 0x893c13eb, 0xee27a9ce, 0x35c961b7,
            0xede51ce1, 0x3cb1477a, 0x59dfd29c, 0x3f73f255, 0x79ce1418,
            0xbf37c773, 0xeacdf753, 0x5baafd5f, 0x146f3ddf, 0x86db4478,
            0x81f3afca, 0x3ec468b9, 0x2c342438, 0x5f40a3c2, 0x72c31d16,
            0x0c25e2bc, 0x8b493c28, 0x41950dff, 0x7101a839, 0xdeb30c08,
            0x9ce4b4d8, 0x90c15664, 0x6184cb7b, 0x70b632d5, 0x745c6c48,
            0x4257b8d0
        };

        private static uint Shift(uint r, int shift) => r >> shift | r << 32 - shift;

        /* multiply four bytes in GF(2^8) by 'x' {02} in parallel */

        private const uint m1 = 0x80808080;
        private const uint m2 = 0x7f7f7f7f;
        private const uint m3 = 0x0000001b;
        private const uint m4 = 0xC0C0C0C0;
        private const uint m5 = 0x3f3f3f3f;

        private static uint FFmulX(uint x) => (x & m2) << 1 ^ ((x & m1) >> 7) * m3;

        private static uint FFmulX2(uint x)
        {
            var t0 = (x & m5) << 2;
            var t1 = x & m4;
            t1 ^= t1 >> 1;
            return t0 ^ t1 >> 2 ^ t1 >> 5;
        }

        /*
        The following defines provide alternative definitions of FFmulX that might
        give improved performance if a fast 32-bit multiply is not available.

        private int FFmulX(int x) { int u = x & m1; u |= (u >> 1); return ((x & m2) << 1) ^ ((u >>> 3) | (u >>> 6)); }
        private static final int  m4 = 0x1b1b1b1b;
        private int FFmulX(int x) { int u = x & m1; return ((x & m2) << 1) ^ ((u - (u >>> 7)) & m4); }

        */

        private static uint Inv_Mcol(uint x)
        {
            uint t0, t1;
            t0 = x;
            t1 = t0 ^ Shift(t0, 8);
            t0 ^= FFmulX(t1);
            t1 ^= FFmulX2(t0);
            t0 ^= t1 ^ Shift(t1, 16);
            return t0;
        }

        private static uint SubWord(uint x)
        {
            return S[x & 255]
                | (uint)S[x >> 8 & 255] << 8
                | (uint)S[x >> 16 & 255] << 16
                | (uint)S[x >> 24 & 255] << 24;
        }

        /**
        * Calculate the necessary round keys
        * The number of calculations depends on key size and block size
        * AES specified a fixed block size of 128 bits and key sizes 128/192/256 bits
        * This code is written assuming those are the only possible values
        */
        private uint[][] GenerateWorkingKey(KeyParameter keyParameter, bool forEncryption)
        {
            var key = keyParameter.GetKey();

            var keyLen = key.Length;
            if (keyLen < 16 || keyLen > 32 || (keyLen & 7) != 0)
                throw new ArgumentException("Key length not 128/192/256 bits.");

            var KC = keyLen >> 2;
            ROUNDS = KC + 6;  // This is not always true for the generalized Rijndael that allows larger block sizes

            var W = new uint[ROUNDS + 1][]; // 4 words in a block
            for (var i = 0; i <= ROUNDS; ++i)
            {
                W[i] = new uint[4];
            }

            switch (KC)
            {
                case 4:
                    {
                        var t0 = Pack.LE_To_UInt32(key, 0);
                        W[0][0] = t0;
                        var t1 = Pack.LE_To_UInt32(key, 4);
                        W[0][1] = t1;
                        var t2 = Pack.LE_To_UInt32(key, 8);
                        W[0][2] = t2;
                        var t3 = Pack.LE_To_UInt32(key, 12);
                        W[0][3] = t3;

                        for (var i = 1; i <= 10; ++i)
                        {
                            var u = SubWord(Shift(t3, 8)) ^ rcon[i - 1];
                            t0 ^= u;
                            W[i][0] = t0;
                            t1 ^= t0;
                            W[i][1] = t1;
                            t2 ^= t1;
                            W[i][2] = t2;
                            t3 ^= t2;
                            W[i][3] = t3;
                        }

                        break;
                    }
                case 6:
                    {
                        var t0 = Pack.LE_To_UInt32(key, 0);
                        W[0][0] = t0;
                        var t1 = Pack.LE_To_UInt32(key, 4);
                        W[0][1] = t1;
                        var t2 = Pack.LE_To_UInt32(key, 8);
                        W[0][2] = t2;
                        var t3 = Pack.LE_To_UInt32(key, 12);
                        W[0][3] = t3;
                        var t4 = Pack.LE_To_UInt32(key, 16);
                        W[1][0] = t4;
                        var t5 = Pack.LE_To_UInt32(key, 20);
                        W[1][1] = t5;

                        uint rcon = 1;
                        var u = SubWord(Shift(t5, 8)) ^ rcon;
                        rcon <<= 1;
                        t0 ^= u;
                        W[1][2] = t0;
                        t1 ^= t0;
                        W[1][3] = t1;
                        t2 ^= t1;
                        W[2][0] = t2;
                        t3 ^= t2;
                        W[2][1] = t3;
                        t4 ^= t3;
                        W[2][2] = t4;
                        t5 ^= t4;
                        W[2][3] = t5;

                        for (var i = 3; i < 12; i += 3)
                        {
                            u = SubWord(Shift(t5, 8)) ^ rcon;
                            rcon <<= 1;
                            t0 ^= u;
                            W[i][0] = t0;
                            t1 ^= t0;
                            W[i][1] = t1;
                            t2 ^= t1;
                            W[i][2] = t2;
                            t3 ^= t2;
                            W[i][3] = t3;
                            t4 ^= t3;
                            W[i + 1][0] = t4;
                            t5 ^= t4;
                            W[i + 1][1] = t5;
                            u = SubWord(Shift(t5, 8)) ^ rcon;
                            rcon <<= 1;
                            t0 ^= u;
                            W[i + 1][2] = t0;
                            t1 ^= t0;
                            W[i + 1][3] = t1;
                            t2 ^= t1;
                            W[i + 2][0] = t2;
                            t3 ^= t2;
                            W[i + 2][1] = t3;
                            t4 ^= t3;
                            W[i + 2][2] = t4;
                            t5 ^= t4;
                            W[i + 2][3] = t5;
                        }

                        u = SubWord(Shift(t5, 8)) ^ rcon;
                        t0 ^= u;
                        W[12][0] = t0;
                        t1 ^= t0;
                        W[12][1] = t1;
                        t2 ^= t1;
                        W[12][2] = t2;
                        t3 ^= t2;
                        W[12][3] = t3;

                        break;
                    }
                case 8:
                    {
                        var t0 = Pack.LE_To_UInt32(key, 0);
                        W[0][0] = t0;
                        var t1 = Pack.LE_To_UInt32(key, 4);
                        W[0][1] = t1;
                        var t2 = Pack.LE_To_UInt32(key, 8);
                        W[0][2] = t2;
                        var t3 = Pack.LE_To_UInt32(key, 12);
                        W[0][3] = t3;
                        var t4 = Pack.LE_To_UInt32(key, 16);
                        W[1][0] = t4;
                        var t5 = Pack.LE_To_UInt32(key, 20);
                        W[1][1] = t5;
                        var t6 = Pack.LE_To_UInt32(key, 24);
                        W[1][2] = t6;
                        var t7 = Pack.LE_To_UInt32(key, 28);
                        W[1][3] = t7;

                        uint u, rcon = 1;

                        for (var i = 2; i < 14; i += 2)
                        {
                            u = SubWord(Shift(t7, 8)) ^ rcon;
                            rcon <<= 1;
                            t0 ^= u;
                            W[i][0] = t0;
                            t1 ^= t0;
                            W[i][1] = t1;
                            t2 ^= t1;
                            W[i][2] = t2;
                            t3 ^= t2;
                            W[i][3] = t3;
                            u = SubWord(t3);
                            t4 ^= u;
                            W[i + 1][0] = t4;
                            t5 ^= t4;
                            W[i + 1][1] = t5;
                            t6 ^= t5;
                            W[i + 1][2] = t6;
                            t7 ^= t6;
                            W[i + 1][3] = t7;
                        }

                        u = SubWord(Shift(t7, 8)) ^ rcon;
                        t0 ^= u;
                        W[14][0] = t0;
                        t1 ^= t0;
                        W[14][1] = t1;
                        t2 ^= t1;
                        W[14][2] = t2;
                        t3 ^= t2;
                        W[14][3] = t3;

                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException("Should never get here");
                    }
            }

            if (!forEncryption)
            {
                for (var j = 1; j < ROUNDS; j++)
                {
                    var w = W[j];
                    for (var i = 0; i < 4; i++)
                    {
                        w[i] = Inv_Mcol(w[i]);
                    }
                }
            }

            return W;
        }

        private int ROUNDS;
        private uint[][] WorkingKey;
        private bool forEncryption;

        private byte[] s;

        private const int BLOCK_SIZE = 16;

        /**
        * default constructor - 128 bit block size.
        */
        public AesEngine()
        {
        }

        /**
        * initialise an AES cipher.
        *
        * @param forEncryption whether or not we are for encryption.
        * @param parameters the parameters required to set up the cipher.
        * @exception ArgumentException if the parameters argument is
        * inappropriate.
        */
        public void Init(bool forEncryption, ICipherParameters parameters)
        {
            if (!(parameters is KeyParameter keyParameter))
                throw new ArgumentException("invalid parameter passed to AES init");

            WorkingKey = GenerateWorkingKey(keyParameter, forEncryption);

            this.forEncryption = forEncryption;
            s = Arrays.Clone(forEncryption ? S : Si);
        }

        public string AlgorithmName
        {
            get { return "AES"; }
        }

        public int GetBlockSize() => BLOCK_SIZE;

        public int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
        {
            if (WorkingKey == null)
                throw new InvalidOperationException("AES engine not initialised");

            Check.DataLength(input, inOff, 16, "input buffer too short");
            Check.OutputLength(output, outOff, 16, "output buffer too short");

            if (forEncryption)
                EncryptBlock(input, inOff, output, outOff, WorkingKey);
            else
            {
                DecryptBlock(input, inOff, output, outOff, WorkingKey);
            }

            return BLOCK_SIZE;
        }

        private void EncryptBlock(byte[] input, int inOff, byte[] output, int outOff, uint[][] KW)
        {
            var C0 = Pack.LE_To_UInt32(input, inOff + 0);
            var C1 = Pack.LE_To_UInt32(input, inOff + 4);
            var C2 = Pack.LE_To_UInt32(input, inOff + 8);
            var C3 = Pack.LE_To_UInt32(input, inOff + 12);

            var kw = KW[0];
            var t0 = C0 ^ kw[0];
            var t1 = C1 ^ kw[1];
            var t2 = C2 ^ kw[2];

            uint r0, r1, r2, r3 = C3 ^ kw[3];
            var r = 1;
            while (r < ROUNDS - 1)
            {
                kw = KW[r++];
                r0 = T0[t0 & 255] ^ Shift(T0[t1 >> 8 & 255], 24) ^ Shift(T0[t2 >> 16 & 255], 16) ^ Shift(T0[r3 >> 24 & 255], 8) ^ kw[0];
                r1 = T0[t1 & 255] ^ Shift(T0[t2 >> 8 & 255], 24) ^ Shift(T0[r3 >> 16 & 255], 16) ^ Shift(T0[t0 >> 24 & 255], 8) ^ kw[1];
                r2 = T0[t2 & 255] ^ Shift(T0[r3 >> 8 & 255], 24) ^ Shift(T0[t0 >> 16 & 255], 16) ^ Shift(T0[t1 >> 24 & 255], 8) ^ kw[2];
                r3 = T0[r3 & 255] ^ Shift(T0[t0 >> 8 & 255], 24) ^ Shift(T0[t1 >> 16 & 255], 16) ^ Shift(T0[t2 >> 24 & 255], 8) ^ kw[3];
                kw = KW[r++];
                t0 = T0[r0 & 255] ^ Shift(T0[r1 >> 8 & 255], 24) ^ Shift(T0[r2 >> 16 & 255], 16) ^ Shift(T0[r3 >> 24 & 255], 8) ^ kw[0];
                t1 = T0[r1 & 255] ^ Shift(T0[r2 >> 8 & 255], 24) ^ Shift(T0[r3 >> 16 & 255], 16) ^ Shift(T0[r0 >> 24 & 255], 8) ^ kw[1];
                t2 = T0[r2 & 255] ^ Shift(T0[r3 >> 8 & 255], 24) ^ Shift(T0[r0 >> 16 & 255], 16) ^ Shift(T0[r1 >> 24 & 255], 8) ^ kw[2];
                r3 = T0[r3 & 255] ^ Shift(T0[r0 >> 8 & 255], 24) ^ Shift(T0[r1 >> 16 & 255], 16) ^ Shift(T0[r2 >> 24 & 255], 8) ^ kw[3];
            }

            kw = KW[r++];
            r0 = T0[t0 & 255] ^ Shift(T0[t1 >> 8 & 255], 24) ^ Shift(T0[t2 >> 16 & 255], 16) ^ Shift(T0[r3 >> 24 & 255], 8) ^ kw[0];
            r1 = T0[t1 & 255] ^ Shift(T0[t2 >> 8 & 255], 24) ^ Shift(T0[r3 >> 16 & 255], 16) ^ Shift(T0[t0 >> 24 & 255], 8) ^ kw[1];
            r2 = T0[t2 & 255] ^ Shift(T0[r3 >> 8 & 255], 24) ^ Shift(T0[t0 >> 16 & 255], 16) ^ Shift(T0[t1 >> 24 & 255], 8) ^ kw[2];
            r3 = T0[r3 & 255] ^ Shift(T0[t0 >> 8 & 255], 24) ^ Shift(T0[t1 >> 16 & 255], 16) ^ Shift(T0[t2 >> 24 & 255], 8) ^ kw[3];

            // the final round's table is a simple function of S so we don't use a whole other four tables for it

            kw = KW[r];
            C0 = S[r0 & 255] ^ (uint)S[r1 >> 8 & 255] << 8 ^ (uint)s[r2 >> 16 & 255] << 16 ^ (uint)s[r3 >> 24 & 255] << 24 ^ kw[0];
            C1 = s[r1 & 255] ^ (uint)S[r2 >> 8 & 255] << 8 ^ (uint)S[r3 >> 16 & 255] << 16 ^ (uint)s[r0 >> 24 & 255] << 24 ^ kw[1];
            C2 = s[r2 & 255] ^ (uint)S[r3 >> 8 & 255] << 8 ^ (uint)S[r0 >> 16 & 255] << 16 ^ (uint)S[r1 >> 24 & 255] << 24 ^ kw[2];
            C3 = s[r3 & 255] ^ (uint)s[r0 >> 8 & 255] << 8 ^ (uint)s[r1 >> 16 & 255] << 16 ^ (uint)S[r2 >> 24 & 255] << 24 ^ kw[3];

            Pack.UInt32_To_LE(C0, output, outOff + 0);
            Pack.UInt32_To_LE(C1, output, outOff + 4);
            Pack.UInt32_To_LE(C2, output, outOff + 8);
            Pack.UInt32_To_LE(C3, output, outOff + 12);
        }

        private void DecryptBlock(byte[] input, int inOff, byte[] output, int outOff, uint[][] KW)
        {
            var C0 = Pack.LE_To_UInt32(input, inOff + 0);
            var C1 = Pack.LE_To_UInt32(input, inOff + 4);
            var C2 = Pack.LE_To_UInt32(input, inOff + 8);
            var C3 = Pack.LE_To_UInt32(input, inOff + 12);

            var kw = KW[ROUNDS];
            var t0 = C0 ^ kw[0];
            var t1 = C1 ^ kw[1];
            var t2 = C2 ^ kw[2];

            uint r0, r1, r2, r3 = C3 ^ kw[3];
            var r = ROUNDS - 1;
            while (r > 1)
            {
                kw = KW[r--];
                r0 = Tinv0[t0 & 255] ^ Shift(Tinv0[r3 >> 8 & 255], 24) ^ Shift(Tinv0[t2 >> 16 & 255], 16) ^ Shift(Tinv0[t1 >> 24 & 255], 8) ^ kw[0];
                r1 = Tinv0[t1 & 255] ^ Shift(Tinv0[t0 >> 8 & 255], 24) ^ Shift(Tinv0[r3 >> 16 & 255], 16) ^ Shift(Tinv0[t2 >> 24 & 255], 8) ^ kw[1];
                r2 = Tinv0[t2 & 255] ^ Shift(Tinv0[t1 >> 8 & 255], 24) ^ Shift(Tinv0[t0 >> 16 & 255], 16) ^ Shift(Tinv0[r3 >> 24 & 255], 8) ^ kw[2];
                r3 = Tinv0[r3 & 255] ^ Shift(Tinv0[t2 >> 8 & 255], 24) ^ Shift(Tinv0[t1 >> 16 & 255], 16) ^ Shift(Tinv0[t0 >> 24 & 255], 8) ^ kw[3];
                kw = KW[r--];
                t0 = Tinv0[r0 & 255] ^ Shift(Tinv0[r3 >> 8 & 255], 24) ^ Shift(Tinv0[r2 >> 16 & 255], 16) ^ Shift(Tinv0[r1 >> 24 & 255], 8) ^ kw[0];
                t1 = Tinv0[r1 & 255] ^ Shift(Tinv0[r0 >> 8 & 255], 24) ^ Shift(Tinv0[r3 >> 16 & 255], 16) ^ Shift(Tinv0[r2 >> 24 & 255], 8) ^ kw[1];
                t2 = Tinv0[r2 & 255] ^ Shift(Tinv0[r1 >> 8 & 255], 24) ^ Shift(Tinv0[r0 >> 16 & 255], 16) ^ Shift(Tinv0[r3 >> 24 & 255], 8) ^ kw[2];
                r3 = Tinv0[r3 & 255] ^ Shift(Tinv0[r2 >> 8 & 255], 24) ^ Shift(Tinv0[r1 >> 16 & 255], 16) ^ Shift(Tinv0[r0 >> 24 & 255], 8) ^ kw[3];
            }

            kw = KW[1];
            r0 = Tinv0[t0 & 255] ^ Shift(Tinv0[r3 >> 8 & 255], 24) ^ Shift(Tinv0[t2 >> 16 & 255], 16) ^ Shift(Tinv0[t1 >> 24 & 255], 8) ^ kw[0];
            r1 = Tinv0[t1 & 255] ^ Shift(Tinv0[t0 >> 8 & 255], 24) ^ Shift(Tinv0[r3 >> 16 & 255], 16) ^ Shift(Tinv0[t2 >> 24 & 255], 8) ^ kw[1];
            r2 = Tinv0[t2 & 255] ^ Shift(Tinv0[t1 >> 8 & 255], 24) ^ Shift(Tinv0[t0 >> 16 & 255], 16) ^ Shift(Tinv0[r3 >> 24 & 255], 8) ^ kw[2];
            r3 = Tinv0[r3 & 255] ^ Shift(Tinv0[t2 >> 8 & 255], 24) ^ Shift(Tinv0[t1 >> 16 & 255], 16) ^ Shift(Tinv0[t0 >> 24 & 255], 8) ^ kw[3];

            // the final round's table is a simple function of Si so we don't use a whole other four tables for it

            kw = KW[0];
            C0 = Si[r0 & 255] ^ (uint)s[r3 >> 8 & 255] << 8 ^ (uint)s[r2 >> 16 & 255] << 16 ^ (uint)Si[r1 >> 24 & 255] << 24 ^ kw[0];
            C1 = s[r1 & 255] ^ (uint)s[r0 >> 8 & 255] << 8 ^ (uint)Si[r3 >> 16 & 255] << 16 ^ (uint)s[r2 >> 24 & 255] << 24 ^ kw[1];
            C2 = s[r2 & 255] ^ (uint)Si[r1 >> 8 & 255] << 8 ^ (uint)Si[r0 >> 16 & 255] << 16 ^ (uint)s[r3 >> 24 & 255] << 24 ^ kw[2];
            C3 = Si[r3 & 255] ^ (uint)s[r2 >> 8 & 255] << 8 ^ (uint)s[r1 >> 16 & 255] << 16 ^ (uint)s[r0 >> 24 & 255] << 24 ^ kw[3];

            Pack.UInt32_To_LE(C0, output, outOff + 0);
            Pack.UInt32_To_LE(C1, output, outOff + 4);
            Pack.UInt32_To_LE(C2, output, outOff + 8);
            Pack.UInt32_To_LE(C3, output, outOff + 12);
        }
    }

    public interface IAeadBlockCipher
    {
        string AlgorithmName { get; }

        void Init(bool forEncryption, ICipherParameters parameters);

        int GetBlockSize();

        int ProcessByte(byte input, byte[] outBytes, int outOff);

        int ProcessBytes(byte[] inBytes, int inOff, int len, byte[] outBytes, int outOff);

        int DoFinal(byte[] outBytes, int outOff);

        byte[] GetMac();

        int GetUpdateOutputSize(int len);

        int GetOutputSize(int len);

        void Reset();
    }

    public class CryptoException : Exception
    {
        public CryptoException()
        {
        }

        public CryptoException(string message)
            : base(message)
        {
        }

        public CryptoException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }

    public class DataLengthException : CryptoException
    {
        public DataLengthException()
        {
        }

        public DataLengthException(string message)
            : base(message)
        {
        }

        public DataLengthException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }

    public class OutputLengthException
    : DataLengthException
    {
        public OutputLengthException()
        {
        }

        public OutputLengthException(string message)
            : base(message)
        {
        }

        public OutputLengthException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class InvalidCipherTextException : CryptoException
    {
        public InvalidCipherTextException()
        {
        }

        public InvalidCipherTextException(string message)
            : base(message)
        {
        }

        public InvalidCipherTextException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }

    public class GcmBlockCipher : IAeadBlockCipher
    {
        private const int BlockSize = 16;
        private static readonly byte[] Zeroes = new byte[16];
        private readonly IBlockCipher cipher;
        private readonly IGcmMultiplier multiplier;
        private bool forEncryption;
        private int macSize;
        private byte[] nonce;
        private byte[] A;
        private KeyParameter keyParam;
        private byte[] H;
        private byte[] initS;
        private byte[] J0;
        private byte[] bufBlock;
        private byte[] macBlock;
        private byte[] S;
        private byte[] counter;
        private int bufOff;
        private ulong totalLength;
        public virtual string AlgorithmName => cipher.AlgorithmName + "/GCM";

        public GcmBlockCipher(IBlockCipher c)
            : this(c, null)
        {
        }

        public GcmBlockCipher(IBlockCipher c, IGcmMultiplier m)
        {
            if (c.GetBlockSize() != 16)
            {
                throw new ArgumentException("所需密码块大小应当为 " + 16 + ".");
            }
            if (m == null)
            {
                m = new Tables8kGcmMultiplier();
            }
            cipher = c;
            multiplier = m;
        }

        public virtual int GetBlockSize()
        {
            return 16;
        }

        public virtual void Init(bool forEncryption, ICipherParameters parameters)
        {
            this.forEncryption = forEncryption;
            macBlock = null;
            if (parameters is AeadParameters)
            {
                AeadParameters aeadParameters = (AeadParameters)parameters;
                nonce = aeadParameters.GetNonce();
                A = aeadParameters.GetAssociatedText();
                int num = aeadParameters.MacSize;
                if (num < 96 || num > 128 || num % 8 != 0)
                {
                    throw new ArgumentException("MAC 的 Size 值 无效: " + num);
                }
                macSize = num / 8;
                keyParam = aeadParameters.Key;
            }
            else
            {
                if (!(parameters is ParametersWithIV))
                {
                    throw new ArgumentException("向 GCM 传递了无效参数");
                }
                ParametersWithIV parametersWithIV = (ParametersWithIV)parameters;
                nonce = parametersWithIV.GetIV();
                A = null;
                macSize = 16;
                keyParam = (KeyParameter)parametersWithIV.Parameters;
            }
            int num2 = forEncryption ? 16 : (16 + macSize);
            bufBlock = new byte[num2];
            if (nonce == null || nonce.Length < 1)
            {
                throw new ArgumentException("IV must be at least 1 byte");
            }
            if (A == null)
            {
                A = new byte[0];
            }
            cipher.Init(true, keyParam);
            H = new byte[16];
            cipher.ProcessBlock(H, 0, H, 0);
            multiplier.Init(H);
            initS = gHASH(A);
            if (nonce.Length == 12)
            {
                J0 = new byte[16];
                Array.Copy(nonce, 0, J0, 0, nonce.Length);
                J0[15] = 1;
            }
            else
            {
                J0 = gHASH(nonce);
                byte[] array = new byte[16];
                packLength((ulong)((long)nonce.Length * 8L), array, 8);
                GcmUtilities.Xor(J0, array);
                multiplier.MultiplyH(J0);
            }
            S = Arrays.Clone(initS);
            counter = Arrays.Clone(J0);
            bufOff = 0;
            totalLength = 0uL;
        }

        public virtual byte[] GetMac()
        {
            return Arrays.Clone(macBlock);
        }

        public virtual int GetOutputSize(int len)
        {
            if (forEncryption)
            {
                return len + bufOff + macSize;
            }
            return len + bufOff - macSize;
        }

        public virtual int GetUpdateOutputSize(int len)
        {
            return (len + bufOff) / 16 * 16;
        }

        public virtual int ProcessByte(byte input, byte[] output, int outOff)
        {
            return Process(input, output, outOff);
        }

        public virtual int ProcessBytes(byte[] input, int inOff, int len, byte[] output, int outOff)
        {
            int num = 0;
            for (int i = 0; i != len; i++)
            {
                bufBlock[bufOff++] = input[inOff + i];
                if (bufOff == bufBlock.Length)
                {
                    gCTRBlock(bufBlock, 16, output, outOff + num);
                    if (!forEncryption)
                    {
                        Array.Copy(bufBlock, 16, bufBlock, 0, macSize);
                    }
                    bufOff = bufBlock.Length - 16;
                    num += 16;
                }
            }
            return num;
        }

        private int Process(byte input, byte[] output, int outOff)
        {
            bufBlock[bufOff++] = input;
            if (bufOff == bufBlock.Length)
            {
                gCTRBlock(bufBlock, 16, output, outOff);
                if (!forEncryption)
                {
                    Array.Copy(bufBlock, 16, bufBlock, 0, macSize);
                }
                bufOff = bufBlock.Length - 16;
                return 16;
            }
            return 0;
        }

        public int DoFinal(byte[] output, int outOff)
        {
            int num = bufOff;
            if (!forEncryption)
            {
                if (num < macSize)
                {
                    throw new InvalidCipherTextException("数据过短");
                }
                num -= macSize;
            }
            if (num > 0)
            {
                byte[] array = new byte[16];
                Array.Copy(bufBlock, 0, array, 0, num);
                gCTRBlock(array, num, output, outOff);
            }
            byte[] array2 = new byte[16];
            packLength((ulong)((long)A.Length * 8L), array2, 0);
            packLength(totalLength * 8, array2, 8);
            GcmUtilities.Xor(S, array2);
            multiplier.MultiplyH(S);
            byte[] array3 = new byte[16];
            cipher.ProcessBlock(J0, 0, array3, 0);
            GcmUtilities.Xor(array3, S);
            int num2 = num;
            macBlock = new byte[macSize];
            Array.Copy(array3, 0, macBlock, 0, macSize);
            if (forEncryption)
            {
                Array.Copy(macBlock, 0, output, outOff + bufOff, macSize);
                num2 += macSize;
            }
            else
            {
                byte[] array4 = new byte[macSize];
                Array.Copy(bufBlock, num, array4, 0, macSize);
                if (!Arrays.ConstantTimeAreEqual(macBlock, array4))
                {
                    throw new InvalidCipherTextException("GCM 中 MAC 检查失败");
                }
            }
            Reset(clearMac: false);
            return num2;
        }

        public virtual void Reset()
        {
            Reset(clearMac: true);
        }

        private void Reset(bool clearMac)
        {
            S = Arrays.Clone(initS);
            counter = Arrays.Clone(J0);
            bufOff = 0;
            totalLength = 0uL;
            if (bufBlock != null)
            {
                Array.Clear(bufBlock, 0, bufBlock.Length);
            }
            if (clearMac)
            {
                macBlock = null;
            }
        }

        private void gCTRBlock(byte[] buf, int bufCount, byte[] output, int outOff)
        {
            int num = 15;
            while (num >= 12 && ++counter[num] == 0)
            {
                num--;
            }
            byte[] array = new byte[16];
            cipher.ProcessBlock(counter, 0, array, 0);
            byte[] val;
            if (forEncryption)
            {
                Array.Copy(Zeroes, bufCount, array, bufCount, 16 - bufCount);
                val = array;
            }
            else
            {
                val = buf;
            }
            for (int num2 = bufCount - 1; num2 >= 0; num2--)
            {
                array[num2] ^= buf[num2];
                output[outOff + num2] = array[num2];
            }
            GcmUtilities.Xor(S, val);
            multiplier.MultiplyH(S);
            totalLength += (ulong)bufCount;
        }

        private byte[] gHASH(byte[] b)
        {
            byte[] array = new byte[16];
            for (int i = 0; i < b.Length; i += 16)
            {
                byte[] array2 = new byte[16];
                int length = Math.Min(b.Length - i, 16);
                Array.Copy(b, i, array2, 0, length);
                GcmUtilities.Xor(array, array2);
                multiplier.MultiplyH(array);
            }
            return array;
        }

        private static void packLength(ulong len, byte[] bs, int off)
        {
            Pack.UInt32_To_BE((uint)(len >> 32), bs, off);
            Pack.UInt32_To_BE((uint)len, bs, off + 4);
        }
    }

    public static class AesGcm256
    {
        public static string Decrypt(byte[] encryptedBytes, byte[] key, byte[] iv)
        {
            string result = string.Empty;
            try
            {
                GcmBlockCipher gcmBlockCipher = new GcmBlockCipher(new AesEngine());
                AeadParameters parameters = new AeadParameters(new KeyParameter(key), 128, iv, null);
                gcmBlockCipher.Init(false, parameters);
                byte[] array = new byte[gcmBlockCipher.GetOutputSize(encryptedBytes.Length)];
                int outOff = gcmBlockCipher.ProcessBytes(encryptedBytes, 0, encryptedBytes.Length, array, 0);
                gcmBlockCipher.DoFinal(array, outOff);
                result = Encoding.UTF8.GetString(array).TrimEnd("\r\n\0".ToCharArray());
                return result;
            }
            catch
            {
                return result;
            }
        }
    }

    public interface IGcmExponentiator
    {
        void Init(byte[] x);
        void ExponentiateX(long pow, byte[] output);
    }

    public class BasicGcmMultiplier : IGcmMultiplier
    {
#if NETCOREAPP3_0_OR_GREATER
        public static bool IsHardwareAccelerated => Pclmulqdq.IsSupported;
#else
        public static bool IsHardwareAccelerated => false;
#endif

        private GcmUtilities.FieldElement H;

        public void Init(byte[] H) => GcmUtilities.AsFieldElement(H, out this.H);

        public void MultiplyH(byte[] x)
        {
            GcmUtilities.AsFieldElement(x, out var T);
            GcmUtilities.Multiply(ref T, ref H);
            GcmUtilities.AsBytes(ref T, x);
        }
    }

    public class Tables4kGcmMultiplier : IGcmMultiplier
    {
        private byte[] H;
        private GcmUtilities.FieldElement[] T;

        public void Init(byte[] H)
        {
            if (T == null)
                T = new GcmUtilities.FieldElement[256];
            else if (Arrays.AreEqual(this.H, H))
            {
                return;
            }

            this.H = Arrays.Clone(H);

            // T[0] = 0
            // T[1] = H.p^7
            GcmUtilities.AsFieldElement(this.H, out T[1]);
            GcmUtilities.MultiplyP7(ref T[1]);

            for (var n = 1; n < 128; ++n)
            {
                // T[2.n] = T[n].p^-1
                GcmUtilities.DivideP(ref T[n], out T[n << 1]);

                // T[2.n + 1] = T[2.n] + T[1]
                GcmUtilities.Xor(ref T[n << 1], ref T[1], out T[(n << 1) + 1]);
            }
        }

        public void MultiplyH(byte[] x)
        {
            int pos = x[15];
            ulong z0 = T[pos].n0, z1 = T[pos].n1;

            for (var i = 14; i >= 0; --i)
            {
                pos = x[i];

                var c = z1 << 56;
                z1 = T[pos].n1 ^ (z1 >> 8 | z0 << 56);
                z0 = T[pos].n0 ^ z0 >> 8 ^ c ^ c >> 1 ^ c >> 2 ^ c >> 7;
            }

            GcmUtilities.AsBytes(z0, z1, x);
        }
    }

    public class BasicGcmExponentiator : IGcmExponentiator
    {
        private GcmUtilities.FieldElement x;

        public void Init(byte[] x) => GcmUtilities.AsFieldElement(x, out this.x);

        public void ExponentiateX(long pow, byte[] output)
        {
            GcmUtilities.FieldElement y;
            GcmUtilities.One(out y);

            if (pow > 0)
            {
                var powX = x;
                do
                {
                    if ((pow & 1L) != 0)
                        GcmUtilities.Multiply(ref y, ref powX);
                    GcmUtilities.Square(ref powX);
                    pow >>= 1;
                }
                while (pow > 0);
            }

            GcmUtilities.AsBytes(ref y, output);
        }
    }

    public sealed class GcmBlockCipherNew
    {
        public static IGcmMultiplier CreateGcmMultiplier()
        {
            if (BasicGcmMultiplier.IsHardwareAccelerated)
                return new BasicGcmMultiplier();

            return new Tables4kGcmMultiplier();
        }

        private const int BlockSize = 16;

        private readonly IBlockCipher cipher;
        private readonly IGcmMultiplier multiplier;
        private IGcmExponentiator exp;

        // These fields are set by Init and not modified by processing
        private bool forEncryption;
        private bool initialised;
        private int macSize;
        private byte[] lastKey;
        private byte[] nonce;
        private byte[] initialAssociatedText;
        private byte[] H;
        private byte[] J0;

        // These fields are modified during processing
        private byte[] bufBlock;
        private byte[] macBlock;
        private byte[] S, S_at, S_atPre;
        private byte[] counter;
        private uint counter32;
        private uint blocksRemaining;
        private int bufOff;
        private ulong totalLength;
        private byte[] atBlock;
        private int atBlockPos;
        private ulong atLength;
        private ulong atLengthPre;

        public GcmBlockCipherNew(
            IBlockCipher c)
            : this(c, null)
        {
        }

        public GcmBlockCipherNew(
            IBlockCipher c,
            IGcmMultiplier m)
        {
            if (c.GetBlockSize() != BlockSize)
                throw new ArgumentException("cipher required with a block size of " + BlockSize + ".");

            if (m == null)
                m = CreateGcmMultiplier();

            cipher = c;
            multiplier = m;
        }

        public string AlgorithmName => cipher.AlgorithmName + "/GCM";

        public IBlockCipher UnderlyingCipher => cipher;

        public int GetBlockSize() => BlockSize;

        /// <remarks>
        /// MAC sizes from 32 bits to 128 bits (must be a multiple of 8) are supported. The default is 128 bits.
        /// Sizes less than 96 are not recommended, but are supported for specialized applications.
        /// </remarks>
        public void Init(bool forEncryption, ICipherParameters parameters)
        {
            this.forEncryption = forEncryption;
            macBlock = null;
            initialised = true;

            KeyParameter keyParam;
            byte[] newNonce;

            if (parameters is AeadParameters aeadParameters)
            {
                newNonce = aeadParameters.GetNonce();
                initialAssociatedText = aeadParameters.GetAssociatedText();

                var macSizeBits = aeadParameters.MacSize;
                if (macSizeBits < 32 || macSizeBits > 128 || macSizeBits % 8 != 0)
                    throw new ArgumentException("Invalid value for MAC size: " + macSizeBits);

                macSize = macSizeBits / 8;
                keyParam = aeadParameters.Key;
            }
            else if (parameters is ParametersWithIV withIV)
            {
                newNonce = withIV.GetIV();
                initialAssociatedText = null;
                macSize = 16;
                keyParam = (KeyParameter)withIV.Parameters;
            }
            else
            {
                throw new ArgumentException("invalid parameters passed to GCM");
            }

            var bufLength = forEncryption ? BlockSize : BlockSize + macSize;
            bufBlock = new byte[bufLength];

            if (newNonce.Length < 1)
                throw new ArgumentException("IV must be at least 1 byte");

            if (forEncryption)
            {
                if (nonce != null && Arrays.AreEqual(nonce, newNonce))
                {
                    if (keyParam == null)
                        throw new ArgumentException("cannot reuse nonce for GCM encryption");

                    if (lastKey != null && keyParam.FixedTimeEquals(lastKey))
                        throw new ArgumentException("cannot reuse nonce for GCM encryption");
                }
            }

            nonce = newNonce;
            if (keyParam != null)
                lastKey = keyParam.GetKey();

            // TODO Restrict macSize to 16 if nonce length not 12?

            // Cipher always used in forward mode
            // if keyParam is null we're reusing the last key.
            if (keyParam != null)
            {
                cipher.Init(true, keyParam);

                H = new byte[BlockSize];
                cipher.ProcessBlock(H, 0, H, 0);

                // if keyParam is null we're reusing the last key and the multiplier doesn't need re-init
                multiplier.Init(H);
                exp = null;
            }
            else if (H == null)
            {
                throw new ArgumentException("Key must be specified in initial Init");
            }

            J0 = new byte[BlockSize];

            if (nonce.Length == 12)
            {
                Array.Copy(nonce, 0, J0, 0, nonce.Length);
                J0[BlockSize - 1] = 0x01;
            }
            else
            {
                gHASH(J0, nonce, nonce.Length);
                var X = new byte[BlockSize];
                Pack.UInt64_To_BE((ulong)nonce.Length * 8UL, X, 8);
                gHASHBlock(J0, X);
            }

            S = new byte[BlockSize];
            S_at = new byte[BlockSize];
            S_atPre = new byte[BlockSize];
            atBlock = new byte[BlockSize];
            atBlockPos = 0;
            atLength = 0;
            atLengthPre = 0;
            counter = Arrays.Clone(J0);
            counter32 = Pack.BE_To_UInt32(counter, 12);
            blocksRemaining = uint.MaxValue - 1; // page 8, len(P) <= 2^39 - 256, 1 block used by tag
            bufOff = 0;
            totalLength = 0;

            if (initialAssociatedText != null)
                ProcessAadBytes(initialAssociatedText, 0, initialAssociatedText.Length);
        }

        public byte[] GetMac() => macBlock == null ? new byte[macSize] : (byte[])macBlock.Clone();

        public int GetOutputSize(int len)
        {
            var totalData = len + bufOff;

            if (forEncryption)
                return totalData + macSize;

            return totalData < macSize ? 0 : totalData - macSize;
        }

        public int GetUpdateOutputSize(int len)
        {
            var totalData = len + bufOff;
            if (!forEncryption)
            {
                if (totalData < macSize)
                    return 0;

                totalData -= macSize;
            }
            return totalData - totalData % BlockSize;
        }

        public void ProcessAadByte(byte input)
        {
            CheckStatus();

            atBlock[atBlockPos] = input;
            if (++atBlockPos == BlockSize)
            {
                // Hash each block as it fills
                gHASHBlock(S_at, atBlock);
                atBlockPos = 0;
                atLength += BlockSize;
            }
        }

        public void ProcessAadBytes(byte[] inBytes, int inOff, int len)
        {
            CheckStatus();

            if (atBlockPos > 0)
            {
                var available = BlockSize - atBlockPos;
                if (len < available)
                {
                    Array.Copy(inBytes, inOff, atBlock, atBlockPos, len);
                    atBlockPos += len;
                    return;
                }

                Array.Copy(inBytes, inOff, atBlock, atBlockPos, available);
                gHASHBlock(S_at, atBlock);
                atLength += BlockSize;
                inOff += available;
                len -= available;
                //atBlockPos = 0;
            }

            var inLimit = inOff + len - BlockSize;

            while (inOff <= inLimit)
            {
                gHASHBlock(S_at, inBytes, inOff);
                atLength += BlockSize;
                inOff += BlockSize;
            }

            atBlockPos = BlockSize + inLimit - inOff;
            Array.Copy(inBytes, inOff, atBlock, 0, atBlockPos);
        }

        private void InitCipher()
        {
            if (atLength > 0)
            {
                Array.Copy(S_at, 0, S_atPre, 0, BlockSize);
                atLengthPre = atLength;
            }

            // Finish hash for partial AAD block
            if (atBlockPos > 0)
            {
                gHASHPartial(S_atPre, atBlock, 0, atBlockPos);
                atLengthPre += (uint)atBlockPos;
            }

            if (atLengthPre > 0)
                Array.Copy(S_atPre, 0, S, 0, BlockSize);
        }

        public int ProcessByte(byte input, byte[] output, int outOff)
        {
            CheckStatus();

            bufBlock[bufOff] = input;
            if (++bufOff == bufBlock.Length)
            {
                Check.OutputLength(output, outOff, BlockSize, "output buffer too short");

                if (blocksRemaining == 0)
                    throw new InvalidOperationException("Attempt to process too many blocks");

                --blocksRemaining;

                if (totalLength == 0)
                    InitCipher();

                if (forEncryption)
                {
                    EncryptBlock(bufBlock, 0, output, outOff);
                    bufOff = 0;
                }
                else
                {
                    DecryptBlock(bufBlock, 0, output, outOff);
                    Array.Copy(bufBlock, BlockSize, bufBlock, 0, macSize);
                    bufOff = macSize;
                }

                totalLength += BlockSize;
                return BlockSize;
            }
            return 0;
        }

        public int ProcessBytes(byte[] input, int inOff, int len, byte[] output, int outOff)
        {
            CheckStatus();

            Check.DataLength(input, inOff, len, "input buffer too short");

            var resultLen = bufOff + len;

            if (forEncryption)
            {
                resultLen &= -BlockSize;
                if (resultLen > 0)
                {
                    Check.OutputLength(output, outOff, resultLen, "output buffer too short");

                    var blocksNeeded = (uint)resultLen >> 4;
                    if (blocksRemaining < blocksNeeded)
                        throw new InvalidOperationException("Attempt to process too many blocks");

                    blocksRemaining -= blocksNeeded;

                    if (totalLength == 0)
                        InitCipher();
                }

                if (bufOff > 0)
                {
                    var available = BlockSize - bufOff;
                    if (len < available)
                    {
                        Array.Copy(input, inOff, bufBlock, bufOff, len);
                        bufOff += len;
                        return 0;
                    }

                    Array.Copy(input, inOff, bufBlock, bufOff, available);
                    inOff += available;
                    len -= available;

                    EncryptBlock(bufBlock, 0, output, outOff);
                    outOff += BlockSize;

                    //bufOff = 0;
                }

                var inLimit1 = inOff + len - BlockSize;
                var inLimit2 = inLimit1 - BlockSize;

                while (inOff <= inLimit2)
                {
                    EncryptBlocks2(input, inOff, output, outOff);
                    inOff += BlockSize * 2;
                    outOff += BlockSize * 2;
                }

                if (inOff <= inLimit1)
                {
                    EncryptBlock(input, inOff, output, outOff);
                    inOff += BlockSize;
                    //outOff += BlockSize;
                }

                bufOff = BlockSize + inLimit1 - inOff;
                Array.Copy(input, inOff, bufBlock, 0, bufOff);
            }
            else
            {
                resultLen -= macSize;
                resultLen &= -BlockSize;
                if (resultLen > 0)
                {
                    Check.OutputLength(output, outOff, resultLen, "output buffer too short");

                    var blocksNeeded = (uint)resultLen >> 4;
                    if (blocksRemaining < blocksNeeded)
                        throw new InvalidOperationException("Attempt to process too many blocks");

                    blocksRemaining -= blocksNeeded;

                    if (totalLength == 0)
                        InitCipher();
                }

                var available = bufBlock.Length - bufOff;
                if (len < available)
                {
                    Array.Copy(input, inOff, bufBlock, bufOff, len);
                    bufOff += len;
                    return 0;
                }

                if (bufOff >= BlockSize)
                {
                    DecryptBlock(bufBlock, 0, output, outOff);
                    outOff += BlockSize;

                    bufOff -= BlockSize;
                    Array.Copy(bufBlock, BlockSize, bufBlock, 0, bufOff);

                    available += BlockSize;
                    if (len < available)
                    {
                        Array.Copy(input, inOff, bufBlock, bufOff, len);
                        bufOff += len;

                        totalLength += BlockSize;
                        return BlockSize;
                    }
                }

                var inLimit1 = inOff + len - bufBlock.Length;
                var inLimit2 = inLimit1 - BlockSize;

                available = BlockSize - bufOff;
                Array.Copy(input, inOff, bufBlock, bufOff, available);
                inOff += available;

                DecryptBlock(bufBlock, 0, output, outOff);
                outOff += BlockSize;
                //bufOff = 0;

                while (inOff <= inLimit2)
                {
                    DecryptBlocks2(input, inOff, output, outOff);
                    inOff += BlockSize * 2;
                    outOff += BlockSize * 2;
                }

                if (inOff <= inLimit1)
                {
                    DecryptBlock(input, inOff, output, outOff);
                    inOff += BlockSize;
                    //outOff += BlockSize;
                }

                bufOff = bufBlock.Length + inLimit1 - inOff;
                Array.Copy(input, inOff, bufBlock, 0, bufOff);
            }

            totalLength += (uint)resultLen;
            return resultLen;
        }

        public int DoFinal(byte[] output, int outOff)
        {
            CheckStatus();

            var extra = bufOff;

            if (forEncryption)
                Check.OutputLength(output, outOff, extra + macSize, "output buffer too short");
            else
            {
                if (extra < macSize)
                    throw new InvalidCipherTextException("data too short");

                extra -= macSize;

                Check.OutputLength(output, outOff, extra, "output buffer too short");
            }

            if (totalLength == 0)
                InitCipher();

            if (extra > 0)
            {
                if (blocksRemaining == 0)
                    throw new InvalidOperationException("Attempt to process too many blocks");

                --blocksRemaining;

                ProcessPartial(bufBlock, 0, extra, output, outOff);
            }

            atLength += (uint)atBlockPos;

            if (atLength > atLengthPre)
            {
                /*
                 *  Some AAD was sent after the cipher started. We determine the difference b/w the hash value
                 *  we actually used when the cipher started (S_atPre) and the final hash value calculated (S_at).
                 *  Then we carry this difference forward by multiplying by H^c, where c is the number of (full or
                 *  partial) cipher-text blocks produced, and adjust the current hash.
                 */

                // Finish hash for partial AAD block
                if (atBlockPos > 0)
                    gHASHPartial(S_at, atBlock, 0, atBlockPos);

                // Find the difference between the AAD hashes
                if (atLengthPre > 0)
                    GcmUtilities.Xor(S_at, S_atPre);

                // Number of cipher-text blocks produced
                var c = (long)(totalLength * 8 + 127 >> 7);

                // Calculate the adjustment factor
                var H_c = new byte[16];
                if (exp == null)
                {
                    exp = new BasicGcmExponentiator();
                    exp.Init(H);
                }
                exp.ExponentiateX(c, H_c);

                // Carry the difference forward
                GcmUtilities.Multiply(S_at, H_c);

                // Adjust the current hash
                GcmUtilities.Xor(S, S_at);
            }

            // Final gHASH
            var X = new byte[BlockSize];
            Pack.UInt64_To_BE(atLength * 8UL, X, 0);
            Pack.UInt64_To_BE(totalLength * 8UL, X, 8);

            gHASHBlock(S, X);

            // T = MSBt(GCTRk(J0,S))
            var tag = new byte[BlockSize];
            cipher.ProcessBlock(J0, 0, tag, 0);
            GcmUtilities.Xor(tag, S);

            var resultLen = extra;

            // We place into macBlock our calculated value for T
            macBlock = new byte[macSize];
            Array.Copy(tag, 0, macBlock, 0, macSize);

            if (forEncryption)
            {
                // Append T to the message
                Array.Copy(macBlock, 0, output, outOff + bufOff, macSize);
                resultLen += macSize;
            }
            else
            {
                // Retrieve the T value from the message and compare to calculated one
                var msgMac = new byte[macSize];
                Array.Copy(bufBlock, extra, msgMac, 0, macSize);
                if (!Arrays.FixedTimeEquals(macBlock, msgMac))
                    throw new InvalidCipherTextException("mac check in GCM failed");
            }

            Reset(false);

            return resultLen;
        }

        public void Reset() => Reset(true);

        private void Reset(bool clearMac)
        {
            // note: we do not reset the nonce.

            S = new byte[BlockSize];
            S_at = new byte[BlockSize];
            S_atPre = new byte[BlockSize];
            atBlock = new byte[BlockSize];
            atBlockPos = 0;
            atLength = 0;
            atLengthPre = 0;
            counter = Arrays.Clone(J0);
            counter32 = Pack.BE_To_UInt32(counter, 12);
            blocksRemaining = uint.MaxValue - 1;
            bufOff = 0;
            totalLength = 0;

            if (bufBlock != null)
                Arrays.Fill(bufBlock, 0);

            if (clearMac)
                macBlock = null;

            if (forEncryption)
                initialised = false;
            else if (initialAssociatedText != null)
            {
                ProcessAadBytes(initialAssociatedText, 0, initialAssociatedText.Length);
            }
        }

        private void DecryptBlock(byte[] inBuf, int inOff, byte[] outBuf, int outOff)
        {
            var ctrBlock = new byte[BlockSize];

            GetNextCtrBlock(ctrBlock);
            {
                for (var i = 0; i < BlockSize; i += 4)
                {
                    var c0 = inBuf[inOff + i + 0];
                    var c1 = inBuf[inOff + i + 1];
                    var c2 = inBuf[inOff + i + 2];
                    var c3 = inBuf[inOff + i + 3];

                    S[i + 0] ^= c0;
                    S[i + 1] ^= c1;
                    S[i + 2] ^= c2;
                    S[i + 3] ^= c3;

                    outBuf[outOff + i + 0] = (byte)(c0 ^ ctrBlock[i + 0]);
                    outBuf[outOff + i + 1] = (byte)(c1 ^ ctrBlock[i + 1]);
                    outBuf[outOff + i + 2] = (byte)(c2 ^ ctrBlock[i + 2]);
                    outBuf[outOff + i + 3] = (byte)(c3 ^ ctrBlock[i + 3]);
                }
            }
            multiplier.MultiplyH(S);
        }

        private void DecryptBlocks2(byte[] inBuf, int inOff, byte[] outBuf, int outOff)
        {
            var ctrBlock = new byte[BlockSize];

            GetNextCtrBlock(ctrBlock);
            {
                for (var i = 0; i < BlockSize; i += 4)
                {
                    var c0 = inBuf[inOff + i + 0];
                    var c1 = inBuf[inOff + i + 1];
                    var c2 = inBuf[inOff + i + 2];
                    var c3 = inBuf[inOff + i + 3];

                    S[i + 0] ^= c0;
                    S[i + 1] ^= c1;
                    S[i + 2] ^= c2;
                    S[i + 3] ^= c3;

                    outBuf[outOff + i + 0] = (byte)(c0 ^ ctrBlock[i + 0]);
                    outBuf[outOff + i + 1] = (byte)(c1 ^ ctrBlock[i + 1]);
                    outBuf[outOff + i + 2] = (byte)(c2 ^ ctrBlock[i + 2]);
                    outBuf[outOff + i + 3] = (byte)(c3 ^ ctrBlock[i + 3]);
                }
            }
            multiplier.MultiplyH(S);

            inOff += BlockSize;
            outOff += BlockSize;

            GetNextCtrBlock(ctrBlock);
            {
                for (var i = 0; i < BlockSize; i += 4)
                {
                    var c0 = inBuf[inOff + i + 0];
                    var c1 = inBuf[inOff + i + 1];
                    var c2 = inBuf[inOff + i + 2];
                    var c3 = inBuf[inOff + i + 3];

                    S[i + 0] ^= c0;
                    S[i + 1] ^= c1;
                    S[i + 2] ^= c2;
                    S[i + 3] ^= c3;

                    outBuf[outOff + i + 0] = (byte)(c0 ^ ctrBlock[i + 0]);
                    outBuf[outOff + i + 1] = (byte)(c1 ^ ctrBlock[i + 1]);
                    outBuf[outOff + i + 2] = (byte)(c2 ^ ctrBlock[i + 2]);
                    outBuf[outOff + i + 3] = (byte)(c3 ^ ctrBlock[i + 3]);
                }
            }
            multiplier.MultiplyH(S);
        }

        private void EncryptBlock(byte[] inBuf, int inOff, byte[] outBuf, int outOff)
        {
            var ctrBlock = new byte[BlockSize];

            GetNextCtrBlock(ctrBlock);
            {
                for (var i = 0; i < BlockSize; i += 4)
                {
                    var c0 = (byte)(ctrBlock[i + 0] ^ inBuf[inOff + i + 0]);
                    var c1 = (byte)(ctrBlock[i + 1] ^ inBuf[inOff + i + 1]);
                    var c2 = (byte)(ctrBlock[i + 2] ^ inBuf[inOff + i + 2]);
                    var c3 = (byte)(ctrBlock[i + 3] ^ inBuf[inOff + i + 3]);

                    S[i + 0] ^= c0;
                    S[i + 1] ^= c1;
                    S[i + 2] ^= c2;
                    S[i + 3] ^= c3;

                    outBuf[outOff + i + 0] = c0;
                    outBuf[outOff + i + 1] = c1;
                    outBuf[outOff + i + 2] = c2;
                    outBuf[outOff + i + 3] = c3;
                }
            }
            multiplier.MultiplyH(S);
        }

        private void EncryptBlocks2(byte[] inBuf, int inOff, byte[] outBuf, int outOff)
        {
            var ctrBlock = new byte[BlockSize];

            GetNextCtrBlock(ctrBlock);
            {
                for (var i = 0; i < BlockSize; i += 4)
                {
                    var c0 = (byte)(ctrBlock[i + 0] ^ inBuf[inOff + i + 0]);
                    var c1 = (byte)(ctrBlock[i + 1] ^ inBuf[inOff + i + 1]);
                    var c2 = (byte)(ctrBlock[i + 2] ^ inBuf[inOff + i + 2]);
                    var c3 = (byte)(ctrBlock[i + 3] ^ inBuf[inOff + i + 3]);

                    S[i + 0] ^= c0;
                    S[i + 1] ^= c1;
                    S[i + 2] ^= c2;
                    S[i + 3] ^= c3;

                    outBuf[outOff + i + 0] = c0;
                    outBuf[outOff + i + 1] = c1;
                    outBuf[outOff + i + 2] = c2;
                    outBuf[outOff + i + 3] = c3;
                }
            }
            multiplier.MultiplyH(S);

            inOff += BlockSize;
            outOff += BlockSize;

            GetNextCtrBlock(ctrBlock);
            {
                for (var i = 0; i < BlockSize; i += 4)
                {
                    var c0 = (byte)(ctrBlock[i + 0] ^ inBuf[inOff + i + 0]);
                    var c1 = (byte)(ctrBlock[i + 1] ^ inBuf[inOff + i + 1]);
                    var c2 = (byte)(ctrBlock[i + 2] ^ inBuf[inOff + i + 2]);
                    var c3 = (byte)(ctrBlock[i + 3] ^ inBuf[inOff + i + 3]);

                    S[i + 0] ^= c0;
                    S[i + 1] ^= c1;
                    S[i + 2] ^= c2;
                    S[i + 3] ^= c3;

                    outBuf[outOff + i + 0] = c0;
                    outBuf[outOff + i + 1] = c1;
                    outBuf[outOff + i + 2] = c2;
                    outBuf[outOff + i + 3] = c3;
                }
            }
            multiplier.MultiplyH(S);
        }

        private void GetNextCtrBlock(byte[] block)
        {
            Pack.UInt32_To_BE(++counter32, counter, 12);

            cipher.ProcessBlock(counter, 0, block, 0);
        }

        private void ProcessPartial(byte[] buf, int off, int len, byte[] output, int outOff)
        {
            var ctrBlock = new byte[BlockSize];
            GetNextCtrBlock(ctrBlock);

            if (forEncryption)
            {
                GcmUtilities.Xor(buf, off, ctrBlock, 0, len);
                gHASHPartial(S, buf, off, len);
            }
            else
            {
                gHASHPartial(S, buf, off, len);
                GcmUtilities.Xor(buf, off, ctrBlock, 0, len);
            }

            Array.Copy(buf, off, output, outOff, len);
            totalLength += (uint)len;
        }

        private void gHASH(byte[] Y, byte[] b, int len)
        {
            for (var pos = 0; pos < len; pos += BlockSize)
            {
                var num = Math.Min(len - pos, BlockSize);
                gHASHPartial(Y, b, pos, num);
            }
        }

        private void gHASHBlock(byte[] Y, byte[] b)
        {
            GcmUtilities.Xor(Y, b);
            multiplier.MultiplyH(Y);
        }

        private void gHASHBlock(byte[] Y, byte[] b, int off)
        {
            GcmUtilities.Xor(Y, b, off);
            multiplier.MultiplyH(Y);
        }

        private void gHASHPartial(byte[] Y, byte[] b, int off, int len)
        {
            GcmUtilities.Xor(Y, b, off, len);
            multiplier.MultiplyH(Y);
        }

        private void CheckStatus()
        {
            if (!initialised)
            {
                if (forEncryption)
                    throw new InvalidOperationException("GCM cipher cannot be reused for encryption");

                throw new InvalidOperationException("GCM cipher needs to be initialized");
            }
        }
    }

}