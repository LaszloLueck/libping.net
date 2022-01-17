using System.Collections.Immutable;
using System.Net;

namespace LibPing.Net;

public class IcmpResponse
{
    public readonly ImmutableArray<byte> Data;
    public readonly int Code;
    public readonly int Ttl;
    public readonly int Checksum;
    public readonly byte Type;
    public readonly int PayloadSize;
    public readonly IPEndPoint Origin;
    public readonly IPEndPoint? FromIp;
    public readonly int Identifier;
    public readonly int Sequence;
    public readonly IpAddressFamily AddressFamily;
    public readonly long RoundTripTime;
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
        if (addressFamily != IpAddressFamily.IpV4) return;
        Checksum = BitConverter.ToUInt16(data, 22);
        Identifier = BitConverter.ToInt16(data, 0);
        Sequence = BitConverter.ToInt16(data, 2);
        Ttl = data[8];
        RoundTripTime = roundTripTime;
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