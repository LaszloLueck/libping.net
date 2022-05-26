using System.Net;

namespace LibPing.Net;

/// <summary>
/// 
/// </summary>
/// <param name="Hop"></param>
/// <param name="RoundTripTime"></param>
/// <param name="Origin"></param>
/// <param name="State"></param>
/// <param name="Type"></param>
/// <param name="AddressFamily"></param>
public record TracerouteResponse(int Hop, long RoundTripTime, IPEndPoint? Origin, string? State, int Type, IpAddressFamily AddressFamily);