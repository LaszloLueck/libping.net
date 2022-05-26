using System.Net;
using System.Runtime.CompilerServices;
using LanguageExt;

namespace LibPing.Net;

/// <summary>
/// class for executing a traceroute
/// </summary>
public static class Traceroute
{
    /// <summary>
    /// executes a traceroute, which is a chain of pings with different (incrementing) hops.
    /// </summary>
    /// <param name="ipAddressOrHost">ip or hostname to traceroute</param>
    /// <param name="maxHops">in most cases 30 is a good value</param>
    /// <param name="receiveTimeout">after this time, the current ping gave up and the next hop is in the row</param>
    /// <returns>An Either type (simple implementation of scala like Either). returns a right value if all things went well and a left when there was an error in one of the steps.</returns>
    public static async IAsyncEnumerable<Either<TracerouteLeftResult, TracerouteResponse>> DoTraceroute(string ipAddressOrHost, int maxHops,
        int receiveTimeout)
    {
        var hop = 1;
        var tp = 3;
        while (hop <= maxHops && tp is not 0 and not 129)
        {
            var tR = await GetTracerouteResponse(ipAddressOrHost, receiveTimeout, hop);
            tR.IfRight(r => tp = r.Type);
            hop++;
            yield return tR;
        }
    }

    private static async Task<Either<TracerouteLeftResult, TracerouteResponse>> GetTracerouteResponse(string ipOrAddress, int receiveTimeout, int hop)
    {
        var token = new CancellationTokenSource(receiveTimeout).Token;

        try
        {
            var bt = await Icmp.Ping(ipOrAddress, hop, receiveTimeout, token);
            return new TracerouteResponse(hop, bt.RoundTripTime, bt.Origin, bt.TypeString, bt.Type, bt.AddressFamily);
        }
        catch (Exception exception)
        {
            return new TracerouteLeftResult(exception, hop);
        }
    }
}