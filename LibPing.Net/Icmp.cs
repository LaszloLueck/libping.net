namespace LibPing.Net;

public class Icmp
{
    public static async Task<IcmpResponse> Ping(string ipOrHostName, int ttl, int receiveTimeout,
        CancellationToken cancellationToken) => await Ping(ipOrHostName, ttl, receiveTimeout, true, cancellationToken);
    
    public static async Task<IcmpResponse> Ping(string ipOrHostName, int ttl, int receiveTimeout, bool dontFragment, CancellationToken cancellationToken = default)
    {
        var request = new IcmpRequest();
        await request.Init(ttl, ipOrHostName, receiveTimeout, dontFragment, cancellationToken);
        return await IcmpConnector.Ping(request, cancellationToken);
    }
}