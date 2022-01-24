using System.Collections.Immutable;
using System.Net;

namespace LibPing.Net;

public class IcmpResponse
{
    /// <summary>
    /// The addressfamily with which the ping was executed. Either IPv4 or IPv6
    /// </summary>
    public IpAddressFamily AddressFamily;

    /// <summary>
    /// the roundtriptime from send the ping to receive the result in milliseconds
    /// </summary>
    public long RoundTripTime;

    /// <summary>
    /// the response type as readable string
    /// </summary>
    public string TypeString;

    /// <summary>
    /// the return type in as byte value. Attention! IPv4 and IPv6 uses different return values
    /// </summary>
    public byte Type;

    /// <summary>
    /// if the ttl is too slow for the amount of hops to the endpoint, the hop on the ttl position answered here. 
    /// </summary>
    public IPEndPoint Origin;

    /// <summary>
    /// the local ip from which the ping is send
    /// </summary>
    public IPEndPoint? FromIp;

    /// <summary>
    /// the returned code from echo reply
    /// </summary>
    public int Code;

    /// <summary>
    /// the payload size
    /// </summary>
    public int PayloadSize;

    /// <summary>
    /// the whole received datagram as byte array 
    /// </summary>
    public ImmutableArray<byte> Data;
}

/// <summary>
/// return object from ping if result is address family ipv4. It delivers a lot more information as the standard operating system ping command. 
/// </summary>
public class IcmpResponseV4 : IcmpResponse
{
    /// <summary>
    /// the returned ttl (only ipv4)
    /// </summary>
    public readonly int Ttl;

    /// <summary>
    /// the checksum of the return
    /// </summary>
    public readonly int Checksum;

    /// <summary>
    /// the identifier from reply
    /// </summary>
    public readonly int Identifier;

    /// <summary>
    /// the sequence number from response
    /// </summary>
    public readonly int Sequence;

    internal IcmpResponseV4(IPEndPoint origin, int payloadSize, byte[] data, IPEndPoint? fromIp,
        IpAddressFamily addressFamily, long roundTripTime)
    {
        Origin = origin;
        PayloadSize = payloadSize;
        Data = data.ToImmutableArray();
        Type = addressFamily == IpAddressFamily.IpV4 ? data[20] : data[0];
        TypeString = FunctionalExtensions.CalculateTypeString(Type, addressFamily);
        Code = addressFamily == IpAddressFamily.IpV4 ? data[21] : data[1];
        FromIp = fromIp;
        AddressFamily = addressFamily;
        RoundTripTime = roundTripTime;
        Checksum = BitConverter.ToUInt16(data, 22);
        Identifier = BitConverter.ToInt16(data, 0);
        Sequence = BitConverter.ToInt16(data, 2);
        Ttl = data[8];
    }


}

/// <summary>
/// return object from ping if result is address family ipv6. It delivers a lot more information as the standard operating system ping command. 
/// </summary>
public class IcmpResponseV6 : IcmpResponse
{
    internal IcmpResponseV6(IPEndPoint origin, int payloadSize, byte[] data, IPEndPoint? fromIp,
        IpAddressFamily addressFamily, long roundTripTime)
    {
        Origin = origin;
        PayloadSize = payloadSize;
        Data = data.ToImmutableArray();
        Type = addressFamily == IpAddressFamily.IpV4 ? data[20] : data[0];
        TypeString = FunctionalExtensions.CalculateTypeString(Type, addressFamily);
        Code = addressFamily == IpAddressFamily.IpV4 ? data[21] : data[1];
        FromIp = fromIp;
        AddressFamily = addressFamily;
        RoundTripTime = roundTripTime;
    }
    
}