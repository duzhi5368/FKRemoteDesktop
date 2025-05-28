using FKRemoteDesktop.DllHook;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Message.SubMessages;
using System;
using System.Runtime.InteropServices;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class TcpConnectionsHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is GetConnections ||
                                            message is DoCloseConnection;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetConnections msg:
                    Execute(sender, msg);
                    break;

                case DoCloseConnection msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, GetConnections message)
        {
            var table = GetTable();
            var connections = new TcpConnection[table.Length];
            for (int i = 0; i < table.Length; i++)
            {
                string processName;
                try
                {
                    var p = System.Diagnostics.Process.GetProcessById((int)table[i].owningPid);
                    processName = p.ProcessName;
                }
                catch
                {
                    processName = $"PID: {table[i].owningPid}";
                }
                connections[i] = new TcpConnection
                {
                    ProcessName = processName,
                    LocalAddress = table[i].LocalAddress.ToString(),
                    LocalPort = table[i].LocalPort,
                    RemoteAddress = table[i].RemoteAddress.ToString(),
                    RemotePort = table[i].RemotePort,
                    State = (EConnectionState)table[i].state
                };
            }
            client.Send(new GetConnectionsResponse { Connections = connections });
        }

        private void Execute(ISender client, DoCloseConnection message)
        {
            var table = GetTable();
            for (var i = 0; i < table.Length; i++)
            {
                if (message.LocalAddress == table[i].LocalAddress.ToString() &&
                    message.LocalPort == table[i].LocalPort &&
                    message.RemoteAddress == table[i].RemoteAddress.ToString() &&
                    message.RemotePort == table[i].RemotePort)
                {
                    table[i].state = (byte)EConnectionState.eConnectionState_Delete_TCB;
                    var ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(table[i]));
                    Marshal.StructureToPtr(table[i], ptr, false);
                    NativeMethods.SetTcpEntry(ptr);
                    Execute(client, new GetConnections());
                    return;
                }
            }
        }

        private NativeMethods.MibTcprowOwnerPid[] GetTable()
        {
            NativeMethods.MibTcprowOwnerPid[] tTable;
            var afInet = 2;
            var buffSize = 0;
            NativeMethods.GetExtendedTcpTable(IntPtr.Zero, ref buffSize, true, afInet, NativeMethods.TcpTableClass.TcpTableOwnerPidAll);
            var buffTable = Marshal.AllocHGlobal(buffSize);
            try
            {
                var ret = NativeMethods.GetExtendedTcpTable(buffTable, ref buffSize, true, afInet, NativeMethods.TcpTableClass.TcpTableOwnerPidAll);
                if (ret != 0)
                    return null;
                var tab = (NativeMethods.MibTcptableOwnerPid)Marshal.PtrToStructure(buffTable, typeof(NativeMethods.MibTcptableOwnerPid));
                var rowPtr = (IntPtr)((long)buffTable + Marshal.SizeOf(tab.dwNumEntries));
                tTable = new NativeMethods.MibTcprowOwnerPid[tab.dwNumEntries];
                for (var i = 0; i < tab.dwNumEntries; i++)
                {
                    var tcpRow = (NativeMethods.MibTcprowOwnerPid)Marshal.PtrToStructure(rowPtr, typeof(NativeMethods.MibTcprowOwnerPid));
                    tTable[i] = tcpRow;
                    rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(tcpRow));
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffTable);
            }
            return tTable;
        }
    }
}