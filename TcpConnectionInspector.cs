using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace NetWatcher.App;

internal static class TcpConnectionInspector
{
    public static IReadOnlyList<TcpProcessConnection> GetAllTcpConnections()
    {
        var size = 0;
        var family = (int)AddressFamily.InterNetwork;
        var result = NativeMethods.GetExtendedTcpTable(IntPtr.Zero, ref size, true, family, TcpTableClass.TcpTableOwnerPidAll, 0);

        if (result != NativeMethods.ErrorInsufficientBuffer || size <= 0)
        {
            return [];
        }

        var buffer = Marshal.AllocHGlobal(size);
        try
        {
            result = NativeMethods.GetExtendedTcpTable(buffer, ref size, true, family, TcpTableClass.TcpTableOwnerPidAll, 0);
            if (result != 0)
            {
                return [];
            }

            var count = Marshal.ReadInt32(buffer);
            var rowPtr = IntPtr.Add(buffer, sizeof(int));
            var rowSize = Marshal.SizeOf<MibTcpRowOwnerPid>();
            var connections = new List<TcpProcessConnection>(count);

            for (var i = 0; i < count; i++)
            {
                var row = Marshal.PtrToStructure<MibTcpRowOwnerPid>(rowPtr);
                connections.Add(TcpProcessConnection.FromRow(row));
                rowPtr = IntPtr.Add(rowPtr, rowSize);
            }

            return connections;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static class NativeMethods
    {
        public const int ErrorInsufficientBuffer = 122;

        [DllImport("iphlpapi.dll", SetLastError = true)]
        public static extern int GetExtendedTcpTable(
            IntPtr pTcpTable,
            ref int dwOutBufLen,
            bool sort,
            int ipVersion,
            TcpTableClass tableClass,
            int reserved);

        [DllImport("iphlpapi.dll", SetLastError = true)]
        public static extern int GetPerTcpConnectionEStats(
            ref MibTcpRow row,
            TcpConnectionEstatsType estatsType,
            IntPtr rw,
            uint rwVersion,
            uint rwSize,
            IntPtr ros,
            uint rosVersion,
            uint rosSize,
            IntPtr rod,
            uint rodVersion,
            uint rodSize);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibTcpRowOwnerPid
    {
        public uint state;
        public uint localAddr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] localPort;

        public uint remoteAddr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] remotePort;

        public uint owningPid;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MibTcpRow
    {
        public TcpState state;
        public uint localAddr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] localPort;

        public uint remoteAddr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] remotePort;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TcpEstatsDataRodV0
    {
        public ulong DataBytesOut;
        public ulong DataSegsOut;
        public ulong DataBytesIn;
        public ulong DataSegsIn;
        public ulong SegsOut;
        public ulong SegsIn;
        public ulong SoftErrors;
        public ulong SoftErrorReason;
        public ulong SndUna;
        public ulong SndNxt;
        public ulong SndMax;
        public ulong ThruBytesAcked;
        public ulong RcvNxt;
        public ulong ThruBytesReceived;
    }

    internal sealed class TcpProcessConnection
    {
        private readonly MibTcpRow _row;

        private TcpProcessConnection(MibTcpRow row, int processId, IPEndPoint? local, IPEndPoint? remote)
        {
            _row = row;
            ProcessId = processId;
            LocalEndPoint = local;
            RemoteEndPoint = remote;
        }

        public int ProcessId { get; }
        public IPEndPoint? LocalEndPoint { get; }
        public IPEndPoint? RemoteEndPoint { get; }

        public bool HasEstablishedAddresses => LocalEndPoint is not null && RemoteEndPoint is not null;

        public string Key => $"{ProcessId}:{LocalEndPoint}-{RemoteEndPoint}";

        internal static TcpProcessConnection FromRow(MibTcpRowOwnerPid row)
        {
            var baseRow = new MibTcpRow
            {
                state = (TcpState)row.state,
                localAddr = row.localAddr,
                localPort = row.localPort,
                remoteAddr = row.remoteAddr,
                remotePort = row.remotePort
            };

            return new TcpProcessConnection(
                baseRow,
                unchecked((int)row.owningPid),
                TryCreateEndpoint(row.localAddr, row.localPort),
                TryCreateEndpoint(row.remoteAddr, row.remotePort));
        }

        public TcpReadResult TryReadBytes(out TcpConnectionBytesSnapshot bytes)
        {
            bytes = default;
            var buffer = Marshal.AllocHGlobal(Marshal.SizeOf<TcpEstatsDataRodV0>());
            var row = _row;

            try
            {
                var result = NativeMethods.GetPerTcpConnectionEStats(
                    ref row,
                    TcpConnectionEstatsType.Data,
                    IntPtr.Zero,
                    0,
                    0,
                    IntPtr.Zero,
                    0,
                    0,
                    buffer,
                    0,
                    (uint)Marshal.SizeOf<TcpEstatsDataRodV0>());

                if (result == 5)
                {
                    return TcpReadResult.AccessDenied;
                }

                if (result != 0)
                {
                    return TcpReadResult.Unavailable;
                }

                var data = Marshal.PtrToStructure<TcpEstatsDataRodV0>(buffer);
                bytes = new TcpConnectionBytesSnapshot(data.DataBytesOut, data.DataBytesIn);
                return TcpReadResult.Success;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static IPEndPoint? TryCreateEndpoint(uint address, byte[] rawPort)
        {
            var port = ((rawPort[0] << 8) | rawPort[1]) & 0xffff;
            if (port == 0)
            {
                return null;
            }

            var bytes = BitConverter.GetBytes(address);
            return new IPEndPoint(new IPAddress(bytes), port);
        }
    }

    private enum TcpTableClass
    {
        TcpTableOwnerPidAll = 5
    }

    private enum TcpConnectionEstatsType
    {
        Data = 2
    }
}

internal readonly record struct TcpConnectionBytesSnapshot(ulong DataBytesOut, ulong DataBytesIn);

internal enum TcpReadResult
{
    Success,
    AccessDenied,
    Unavailable
}
