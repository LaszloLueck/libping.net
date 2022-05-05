using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace LibPing.Net;

internal class IcmpRequest
{
    public int Ttl { get; private set; }
    public byte[] Payload { get; } = new byte[1024];
    public byte Type { get; private set; }
    public int Code { get; private set; }
    public int Checksum { get; private set; }
    public int MessageSize { get; private set; }
    public int ReceiveTimeout { get; private set; }
    public bool DontFragment { get; private set; }
    public IpAddressFamily AddressFamily { get; private set; }
    public IPEndPoint IpEndPointFrom { get; private set; } = null!;
    public IPEndPoint IpEndPointTo { get; private set; } = null!;

    internal async Task Init(int ttl, string endPoint, int receiveTimeout, bool dontFragment, CancellationToken cancellationToken)
    {
        IpEndPointTo = await GetEndpointFromString(endPoint, cancellationToken);
        IpEndPointFrom = new IPEndPoint(await GetFromHostOrIp(AddressFamily, cancellationToken), 0);

        if (IpEndPointTo is null)
            throw new ArgumentNullException(nameof(IpEndPointTo));

        if (IpEndPointFrom is null)
            throw new ArgumentNullException(nameof(IpEndPointFrom));

        AddressFamily = GetIpAddressFamily(IpEndPointTo.AddressFamily);
        Type = CalculateType(AddressFamily);

        DontFragment = dontFragment;
        Ttl = ttl;
        ReceiveTimeout = receiveTimeout;
        Code = 0x00;
        Buffer.BlockCopy(BitConverter.GetBytes((short)1), 0, Payload, 0, 2);
        Buffer.BlockCopy(BitConverter.GetBytes((short)1), 0, Payload, 2, 2);
        var data = GenerateRandomData(32);
        Buffer.BlockCopy(data, 0, Payload, 4, data.Length);
        MessageSize = data.Length + 4;
        Checksum = GetChecksum(MessageSize, GetBytes());
    }

    private static readonly Func<AddressFamily, IpAddressFamily> GetIpAddressFamily = (addressFamily) =>
        addressFamily switch
        {
            System.Net.Sockets.AddressFamily.InterNetwork => IpAddressFamily.IpV4,
            System.Net.Sockets.AddressFamily.InterNetworkV6 => IpAddressFamily.IpV6,
            _ => throw new InvalidEnumArgumentException()
        };

    private static readonly Func<IpAddressFamily, CancellationToken, Task<IPAddress>> GetFromHostOrIp =
        async (addressFamily, cancellationToken) =>
        {
            return addressFamily == IpAddressFamily.IpV4
                ? (await Dns.GetHostEntryAsync(Dns.GetHostName(), cancellationToken)).AddressList.First(i =>
                    i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                : (await Dns.GetHostEntryAsync(Dns.GetHostName(), cancellationToken)).AddressList.First(i =>
                    i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);
        };

    private static readonly Func<IpAddressFamily, byte> CalculateType = addressFamily =>
        addressFamily switch
        {
            IpAddressFamily.IpV4 => 0x08,
            IpAddressFamily.IpV6 => 0x80,
            _ => throw new InvalidEnumArgumentException()
        };
#pragma warning disable
    private static readonly Func<string, CancellationToken, Task<IPEndPoint>> GetEndpointFromString =
        async (endpointAsString, cancellationToken) => !IPAddress.TryParse(endpointAsString, out var ipAddress)
            ? await ResolveIpEndPoint(endpointAsString, cancellationToken)
            : ipAddress is not null
                ? new IPEndPoint(ipAddress, 0)
                : throw new ArgumentNullException(nameof(ipAddress));
#pragma warning restore
    
    private static readonly Func<int, byte[]> GenerateRandomData = length =>
    {
        var data = new byte[length];
        RandomNumberGenerator
            .Create()
            .GetBytes(data);
        return data;
    };

    private static readonly Func<string, CancellationToken, Task<IPEndPoint>> ResolveIpEndPoint =
        async (stringIpOrHost, cancellationToken) =>
        {
            var hostIpAddresses = await Dns.GetHostAddressesAsync(stringIpOrHost, cancellationToken);
            if (hostIpAddresses.Length is 0)
                throw new InvalidOperationException(nameof(hostIpAddresses));
            var hostAddress = hostIpAddresses.FirstOrDefault();
            return (hostAddress is not null) 
                ? new IPEndPoint(hostAddress, 0) 
                : throw new ArgumentNullException(nameof(hostAddress));
        };

    private byte[] GetBytes()
    {
        byte[] data = new byte[MessageSize + 9];
        Buffer.BlockCopy(BitConverter.GetBytes(Type), 0, data, 0, 1);
        Buffer.BlockCopy(BitConverter.GetBytes(Code), 0, data, 1, 1);
        Buffer.BlockCopy(BitConverter.GetBytes(Checksum), 0, data, 2, 2);
        Buffer.BlockCopy(Payload, 0, data, 4, MessageSize);
        return data;
    }

    private static readonly Func<int, byte[], ushort> GetChecksum = (messageSize, payload) =>
    {
        var bytes = payload;
        var checkSum = Enumerable
            .Range(0, messageSize + 8)
            .ToArray()
            .GetNth(2)
            .Sum(value => Convert.ToUInt32(BitConverter.ToUInt16(bytes, value)));
        checkSum = (checkSum >> 16) + (checkSum & 0xffff);
        checkSum += checkSum >> 16;
        return (ushort)~checkSum;
    };
}