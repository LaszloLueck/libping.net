using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace LibPing.Net;

/// <summary>
/// class that do the socket connection and call a ping
/// </summary>
public static class IcmpConnector
{
    internal static async Task<IcmpResponse> Ping(IcmpRequest icmpRequest, CancellationToken cancellationToken)
    {
        if (icmpRequest is null)
            throw new ArgumentNullException(nameof(icmpRequest));

        var host = icmpRequest.AddressFamily switch
        {
            IpAddressFamily.IpV4 => new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp),
            IpAddressFamily.IpV6 => new Socket(AddressFamily.InterNetworkV6, SocketType.Raw, ProtocolType.IcmpV6),
            _ => throw new InvalidEnumArgumentException()
        };
        
        switch (icmpRequest.AddressFamily)
        {
            case IpAddressFamily.IpV4:
                host.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, icmpRequest.Ttl);
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    host.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DontFragment, icmpRequest.DontFragment);
                break;
            case IpAddressFamily.IpV6:
                host.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IpTimeToLive, icmpRequest.Ttl);
                if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) 
                    host.DualMode = false;
                break;
            default:
                throw new InvalidEnumArgumentException();
        }

        if (icmpRequest.IpEndPointTo is null) throw new ArgumentNullException(nameof(icmpRequest.IpEndPointTo));
        if (icmpRequest.IpEndPointFrom is null) throw new ArgumentNullException(nameof(icmpRequest.IpEndPointFrom));
        
        host.ReceiveTimeout = icmpRequest.ReceiveTimeout;

        var sendPayload = GetBytes(icmpRequest.Type, icmpRequest.Payload, icmpRequest.MessageSize,
            icmpRequest.Code, icmpRequest.Checksum);

        var returnedPayload = new byte[1024];

        var sw = Stopwatch.StartNew();
        await host.SendToAsync(sendPayload, SocketFlags.None, icmpRequest.IpEndPointTo, cancellationToken);
       
        var received =
            await host.ReceiveFromAsync(returnedPayload, SocketFlags.None, icmpRequest.IpEndPointFrom,
                cancellationToken);
        sw.Stop();
        host.Close();
        host.Dispose();
        var receivedEndpoint = (IPEndPoint) received.RemoteEndPoint;
        var payloadSize = received.ReceivedBytes;

        return icmpRequest.AddressFamily switch
        {
            IpAddressFamily.IpV4 => new IcmpResponseV4(receivedEndpoint, payloadSize,
                returnedPayload.Take(payloadSize).ToArray()
                , icmpRequest.IpEndPointFrom, icmpRequest.AddressFamily, sw.ElapsedMilliseconds),
            IpAddressFamily.IpV6 => new IcmpResponseV6(receivedEndpoint, payloadSize,
                returnedPayload.Take(payloadSize).ToArray()
                , icmpRequest.IpEndPointFrom, icmpRequest.AddressFamily, sw.ElapsedMilliseconds),
            _ => throw new InvalidEnumArgumentException()
        };
    }

    private static readonly Func<int, byte[], int, int, int, byte[]> GetBytes =
        (type, originPayload, messageSize, code, checksum) =>
        {
            byte[] data = new byte[messageSize + 4];
            Buffer.BlockCopy(BitConverter.GetBytes(type), 0, data, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(code), 0, data, 1, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(checksum), 0, data, 2, 2);
            Buffer.BlockCopy(originPayload, 0, data, 4, messageSize);
            return data;
        };
}