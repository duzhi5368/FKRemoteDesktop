using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Extensions
{
    // KeepAlive的套接字扩展
    public static class SocketExtensions
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct TcpKeepAlive
        {
            internal uint onoff;
            internal uint keepalivetime;
            internal uint keepaliveinterval;
        };

        /// <summary>
        /// 设置当前TCP连接的 Keep-Alive 值
        /// </summary>
        /// <param name="socket">目标Socket</param>
        /// <param name="keepAliveInterval">指定TCP在未收到响应时，重复Keep-Alive传输的频率。发送该消息可防止TCP意外断开</param>
        /// <param name="keepAliveTime">指定TCP在发送Keep-Alive传输的频率。该条目是验证空闲链接是否依然活跃。</param>
        public static void SetKeepAliveEx(this Socket socket, uint keepAliveInterval, uint keepAliveTime)
        {
            var keepAlive = new TcpKeepAlive
            {
                onoff = 1,
                keepaliveinterval = keepAliveInterval,
                keepalivetime = keepAliveTime
            };
            int size = Marshal.SizeOf(keepAlive);
            IntPtr keepAlivePtr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(keepAlive, keepAlivePtr, true);
            var buffer = new byte[size];
            Marshal.Copy(keepAlivePtr, buffer, 0, size);
            Marshal.FreeHGlobal(keepAlivePtr);
            socket.IOControl(IOControlCode.KeepAliveValues, buffer, null);
        }
    }
}
