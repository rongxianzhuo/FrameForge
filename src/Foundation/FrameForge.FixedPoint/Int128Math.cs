using System.Numerics;

namespace FrameForge.Foundation.FixedPoint
{
    /// <summary>
    /// Internal helper for 128-bit integer arithmetic. Replaces
    /// <c>System.Int128</c> which is not available in Unity (Unity uses
    /// .NET Standard 2.1 / Mono and does not ship .NET 7+ types).
    /// Backed by <see cref="System.Numerics.BigInteger"/> which is part of
    /// the .NET BCL and available in Unity 2021.2+.
    /// All operations are deterministic for small inputs.
    /// </summary>
    internal static class Int128Math
    {
        // ====================================================================
        // Public API
        // ====================================================================

        /// <summary>
        /// Computes <c>(a * b) &gt;&gt; shift</c> using 128-bit intermediate precision.
        /// </summary>
        public static long MultiplyShiftRight(long a, long b, int shift)
        {
            BigInteger product = (BigInteger)a * b;
            return (long)(product >> shift);
        }

        /// <summary>
        /// Computes <c>(a &lt;&lt; shift) / b</c> using 128-bit intermediate precision.
        /// Result is truncated toward zero. Throws if <paramref name="b"/> is 0.
        /// </summary>
        public static long ShiftLeftDivide(long a, long b, int shift)
        {
            if (b == 0)
                throw new DivideByZeroException();
            BigInteger dividend = (BigInteger)a << shift;
            return (long)(dividend / b);
        }

        /// <summary>
        /// Computes <c>(a * b + c) &gt;&gt; shift</c> using 128-bit intermediate precision.
        /// </summary>
        public static long MultiplyAddShiftRight(long a, long b, long c, int shift)
        {
            BigInteger product = (BigInteger)a * b + c;
            return (long)(product >> shift);
        }
    }
}
