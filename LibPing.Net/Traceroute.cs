using System.Net;
using System.Runtime.CompilerServices;

namespace LibPing.Net;

public static class Traceroute
{
    public static async IAsyncEnumerable<TracerouteResponse> DoTraceroute(string ipAddressOrHost, int maxHops,
        int receiveTimeout)
    {

        var hop = 1;
        var tp = 3;
        while (hop <= maxHops && tp is not 0 and not 129)
        {
            var token = new CancellationTokenSource(receiveTimeout).Token;
            TracerouteResponse tracerouteResponse;
            try
            {
                var bt = await Icmp.Ping(ipAddressOrHost, hop, receiveTimeout, token);
                tracerouteResponse = new TracerouteResponse(hop, bt.RoundTripTime, true, bt.Origin, bt.TypeString);
                tp = bt.Type;
            }
            catch (Exception exception)
            {
                tracerouteResponse = new TracerouteResponse(hop, 0, false, null, exception.Message);
            }

            hop++;
            yield return tracerouteResponse;
        }
    }
}