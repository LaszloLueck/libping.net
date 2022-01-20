using System.Collections.Immutable;
using System.Net;

namespace LibPing.Net;
/// <summary>
/// return object from ping. It delivers a lot more information as the standard operating system ping command. 
/// </summary>
public class IcmpResponse
{
    /// <summary>
    /// the whole received datagram as byte array 
    /// </summary>
    public readonly ImmutableArray<byte> Data;
    /// <summary>
    /// the returned code from echo reply
    /// </summary>
    public readonly int Code;
    /// <summary>
    /// the returned ttl (only ipv4)
    /// </summary>
    public readonly int Ttl;
    /// <summary>
    /// the checksum of the return
    /// </summary>
    public readonly int Checksum;
    /// <summary>
    /// the return type in as byte value. Attention! IPv4 and IPv6 uses different return values
    /// </summary>
    public readonly byte Type;
    /// <summary>
    /// the payload size
    /// </summary>
    public readonly int PayloadSize;
    /// <summary>
    /// if the ttl is too slow for the amount of hops to the endpoint, the hop on the ttl position answered here. 
    /// </summary>
    public readonly IPEndPoint Origin;
    /// <summary>
    /// the local ip from which the ping is send
    /// </summary>
    public readonly IPEndPoint? FromIp;
    /// <summary>
    /// the identifier from reply
    /// </summary>
    public readonly int Identifier;
    /// <summary>
    /// the sequence number from response
    /// </summary>
    public readonly int Sequence;
    /// <summary>
    /// The addressfamily with which the ping was executed. Either IPv4 or IPv6
    /// </summary>
    public readonly IpAddressFamily AddressFamily;
    /// <summary>
    /// the roundtriptime from send the ping to receive the result in milliseconds
    /// </summary>
    public readonly long RoundTripTime;
    /// <summary>
    /// the response type as readable string
    /// </summary>
    public readonly string TypeString;

    internal IcmpResponse(IPEndPoint origin, int payloadSize, byte[] data, IPEndPoint? fromIp,
        IpAddressFamily addressFamily, long roundTripTime)
    {
        Origin = origin;
        PayloadSize = payloadSize;
        Data = data.ToImmutableArray();
        Type = addressFamily == IpAddressFamily.IpV4 ? data[20] : data[0];
        TypeString = CalculateTypeString(Type, addressFamily);
        Code = addressFamily == IpAddressFamily.IpV4 ? data[21] : data[1];
        FromIp = fromIp;
        AddressFamily = addressFamily;
        RoundTripTime = roundTripTime;
        if (addressFamily != IpAddressFamily.IpV4) return;
        Checksum = BitConverter.ToUInt16(data, 22);
        Identifier = BitConverter.ToInt16(data, 0);
        Sequence = BitConverter.ToInt16(data, 2);
        Ttl = data[8];
    }

    private static readonly Func<byte, IpAddressFamily, string> CalculateTypeString = (byteValue, addressFamily) =>
    {
        return addressFamily switch
        {
            IpAddressFamily.IpV4 => ((MessageTypeV4) byteValue).ToString(),
            IpAddressFamily.IpV6 => ((MessageTypeV6) byteValue).ToString(),
            _ => "IPAddressFamily not supported!"
        };
    };

}