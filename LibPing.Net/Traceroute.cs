using System.Net;
using System.Runtime.CompilerServices;

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
    /// <returns></returns>
    public static async IAsyncEnumerable<TracerouteResponse> DoTraceroute(string ipAddressOrHost, int maxHops,
        int receiveTimeout)
    {
        var hop = 1;
        var tp = 3;
        while (hop <= maxHops && tp is not 0 and not 129)
        {
            var tR = await GetTracerouteResponse(ipAddressOrHost, receiveTimeout, hop);
            tp = tR.Type;
            hop++;
            yield return tR;
        }

    }

    private static async Task<TracerouteResponse> GetTracerouteResponse(string ipOrAddress, int receiveTimeout, int hop)
    {
        var token = new CancellationTokenSource(receiveTimeout).Token;

        try
        {
            var bt = await Icmp.Ping(ipOrAddress, hop, receiveTimeout, token);
            return new TracerouteResponse(hop, bt.RoundTripTime, bt.Origin, bt.TypeString, bt.Type);
        }
        catch (Exception exception)
        {
            return new TracerouteResponse(hop, 0, null, exception.Message, -1);
        }
    }
}