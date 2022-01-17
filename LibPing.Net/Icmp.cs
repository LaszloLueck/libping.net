namespace LibPing.Net;

public class Icmp
{
    public static async Task<IcmpResponse> Ping(string ipOrHostName, int ttl, int receiveTimeout, CancellationToken cancellationToken = default)
    {
        var request = new IcmpRequest();
        await request.Init(ttl, ipOrHostName, receiveTimeout, cancellationToken);
        return await IcmpConnector.Ping(request, cancellationToken);
    }
}