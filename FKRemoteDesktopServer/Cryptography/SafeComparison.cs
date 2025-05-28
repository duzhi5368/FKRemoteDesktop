using System.Runtime.CompilerServices;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Cryptography
{
    public class SafeComparison
    {
        // 比较两个数组是否相等，使用禁止内联优化是防止 timing attack
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static bool AreEqual(byte[] a1, byte[] a2)
        {
            bool result = true;
            for (int i = 0; i < a1.Length; ++i)
            {
                if (a1[i] != a2[i])
                    result = false;
            }
            return result;
        }
    }
}
