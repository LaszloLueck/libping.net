using System.Net;

namespace LibPing.Net;

/// <summary>
/// class which contains methods for icmp (rfc 792) also called ping
/// </summary>
public class Icmp
{
    /// <summary>
    /// process an ICMP echo (RFC-792) command, also called ping in an blocking manner. It does the job with both IPv4 and IPv6.
    /// </summary>
    /// <param name="ipOrHostName">the IP-address or the Hostname to ping to. If you pass an IPv4 address, IPv4 echo is also used. If you pass an IPv6 address, then according to IPv6 rules. If you enter a hostname, it will be checked if the hostname could be resolved as an IPv6 address, if not, IPv4 will be used. </param>
    /// <param name="ttl">The meaning of ttl (time to live) is not so much a time but how many hops (endpoints) may be spent on the ping. E.g. the endpoint to be pinged is 8 hops away. If you enter a 3 as ttl, a result is returned from the 3rd hop in the query series with a ResultType for IPv4 = 11 / IPv6 = 3 (means Time exceeded). ttl is especially useful for traceroute queries. </param>
    /// <param name="receiveTimeout">time in milliseconds. After this time, the connection would be unsuccessful closed with an exception.</param>
    /// <param name="dontFragment">split the datagram to multiple frames when the payload to send is bigger than the the framesize (mtu). It does not work on apple macs.</param>
    /// <returns>returns a IcmpResponse object as encapsulated in a task, or in case of an error, the exception</returns>
    public static IcmpResponse PingSync(string ipOrHostName, int ttl, int receiveTimeout, bool dontFragment = true)
    {
        return Ping(ipOrHostName, ttl, receiveTimeout, dontFragment)
            .GetAwaiter()
            .GetResult();
    }

    /// <summary>
    /// process an ICMP echo (RFC-792) command, also called ping in an async manner. It does the job with both IPv4 and IPv6.
    /// </summary>
    /// <param name="ipOrHostName">the IP-address or the Hostname to ping to. If you pass an IPv4 address, IPv4 echo is also used. If you pass an IPv6 address, then according to IPv6 rules. If you enter a hostname, it will be checked if the hostname could be resolved as an IPv6 address, if not, IPv4 will be used. </param>
    /// <param name="ttl">The meaning of ttl (time to live) is not so much a time but how many hops (endpoints) may be spent on the ping. E.g. the endpoint to be pinged is 8 hops away. If you enter a 3 as ttl, a result is returned from the 3rd hop in the query series with a ResultType for IPv4 = 11 / IPv6 = 3 (means Time exceeded). ttl is especially useful for traceroute queries. </param>
    /// <param name="receiveTimeout">time in milliseconds. After this time, the connection would be unsuccessful closed with an exception.</param>
    /// <param name="cancellationToken">cancellation token for async requests we made.</param>
    /// <returns>returns a IcmpResponse object as encapsulated in a task, or in case of an error, the exception</returns>
    public static async Task<IcmpResponse> Ping(string ipOrHostName, int ttl, int receiveTimeout,
        CancellationToken cancellationToken) => await Ping(ipOrHostName, ttl, receiveTimeout, true, cancellationToken);
    
    /// <summary>
    /// process an ICMP echo (RFC-792) command, also called ping in an async manner. It does the job with both IPv4 and IPv6.
    /// </summary>
    /// <param name="ipOrHostName">the IP-address or the Hostname to ping to. If you pass an IPv4 address, IPv4 echo is also used. If you pass an IPv6 address, then according to IPv6 rules. If you enter a hostname, it will be checked if the hostname could be resolved as an IPv6 address, if not, IPv4 will be used. </param>
    /// <param name="ttl">The meaning of ttl (time to live) is not so much a time but how many hops (endpoints) may be spent on the ping. E.g. the endpoint to be pinged is 8 hops away. If you enter a 3 as ttl, a result is returned from the 3rd hop in the query series with a ResultType for IPv4 = 11 / IPv6 = 3 (means Time exceeded). ttl is especially useful for traceroute queries. </param>
    /// <param name="receiveTimeout">time in milliseconds. After this time, the connection would be unsuccessful closed with an exception.</param>
    /// <param name="dontFragment">split the datagram to multiple frames when the payload to send is bigger than the the framesize (mtu). It does not work on apple macs.</param>
    /// <param name="cancellationToken">cancellation token for async requests we made.</param>
    /// <returns>returns a IcmpResponse object as encapsulated in a task, or in case of an error, the exception</returns>
    public static async Task<IcmpResponse> Ping(string ipOrHostName, int ttl, int receiveTimeout, bool dontFragment, CancellationToken cancellationToken = default)
    {
        var request = new IcmpRequest();
        await request.Init(ttl, ipOrHostName, receiveTimeout, dontFragment, cancellationToken);
        return await IcmpConnector.Ping(request, cancellationToken);
    }
    
    /// <summary>
    /// As of standard Dns.GetHostEntryAsync is not cancelable (yes you can give a CancellationToken, but it does not work), here is a replacement for it.
    /// </summary>
    public static readonly Func<string, Task<string>> AsyncResolveHostName =
        ipAddress =>
        {
            return Task.Run(() =>
            {
                try
                {
                    return Dns.GetHostEntry(ipAddress).HostName;
                }
                catch (Exception)
                {
                    return ipAddress;
                }
            });
        };
    
}