# Before we begin
If you use windows (i have win 10 and win 11) it could be possible that you setup 2 firewall rules.
It could be possible because i visit the following behavior (all tests done with admin / sudo):

1. Windows 11 Machine A - Native - does not work
2. Windows 11 Machine A - WSL2 - does work
3. Windows 11 Machine B - Native - does work
4. Windows 11 Machine B - WSL 2 - does work
5. Windows 10 Machine C - Native - does work
6. Windows 10 Machine C - WSL 2 - does work

Solution (on Machine A)?

After hours of struggling, i found a solution.
Obviously WSL does not use the Windows Firewall, so icmp packets going out in normal manner.

I would setup 2 new INCOMING rules with relatively wide open set rules.
Protocol for rule 1 is icmp4
Protocol for rule 2 is icmp6
All apps, all networks, all interfaces and then it works.

When i wrote this text, i think it would clear why this behavior exists...
Let's look the non working sequence:
````shell
> dotnet run 1.1.1.1
01 * :: The operation was canceled. ERROR possible drop icmp
02 * :: The operation was canceled. ERROR possible drop icmp
03 * :: The operation was canceled. ERROR possible drop icmp
04 * :: The operation was canceled. ERROR possible drop icmp
05 * :: The operation was canceled. ERROR possible drop icmp
06 * :: The operation was canceled. ERROR possible drop icmp
07 1.1.1.1 (one.one.one.one) :: TTL: 58 :: TYPE: EchoReply (0) in 8 ms
````
Look at the last icmp entry! That works.

If we look at the chain, we do the following

Request 1: TTL 1 -> 127.0.1.1 send to 1.1.1.1

Response 1: TTL 64 --> 192.168.0.1 answered (instead of 1.1.1.1) to 127.0.1.1

It could be possible, that only the EchoReply on end is valid for the firewall, because
a request to 1.1.1.1 would be answered by 1.1.1.1

All other cases answered an other endpoint than the requested one.
Obviously that drops the windows firewall.

After i setup the 2 rules for icmpv4 and icmpv6 all things working well.

````shell
> dotnet run 1.1.1.1
01 192.168.0.1 (192.168.0.1) :: TTL: 64 :: TYPE: TimeExceeded (11) in 5 ms
02 195.14.226.125 (bras-vc2.netcologne.de) :: TTL: 254 :: TYPE: TimeExceeded (11) in 8 ms
03 89.1.16.161 (ip-core-eup2-ae12.netcologne.de) :: TTL: 253 :: TYPE: TimeExceeded (11) in 6 ms
04 89.1.86.37 (ip-core-net1-et9-2-2.netcologne.de) :: TTL: 252 :: TYPE: TimeExceeded (11) in 7 ms
05 81.173.192.2 (bdr-net1-ae1.netcologne.de) :: TTL: 251 :: TYPE: TimeExceeded (11) in 7 ms
06 194.146.118.139 (as13335.dusseldorf.megaport.com) :: TTL: 249 :: TYPE: TimeExceeded (11) in 8 ms
07 1.1.1.1 (one.one.one.one) :: TTL: 58 :: TYPE: EchoReply (0) in 7 ms
````

# Examples
All examples uses the libping.net library.

**Beware!**

**As of operating systems restrictions (windows, linux and mac) you should run the app included this library with administrator privileges.**

E.g. https://docs.microsoft.com/de-de/windows/win32/winsock/tcp-ip-raw-sockets-2?redirectedfrom=MSDN



## PING

To fetch some results i've written a small app, that use lib and show the results on console.

```c#
    public static async Task Main(string[] args)
    {

        var hostNameOrAddress = "one.one.one.one";
       
        try
        {
            var cts = new CancellationTokenSource(4000);
            var response = await Icmp.Ping(hostNameOrAddress, 64, 3000, cts.Token);
            Console.WriteLine($"Code: {response.Code}");
            Console.WriteLine($"Address family: {response.AddressFamily}");
            Console.WriteLine($"TTL: {response.Ttl}");
            Console.WriteLine($"Ping-Endpoint: {hostNameOrAddress}");
            Console.WriteLine($"Sender-Endpoint: {response.FromIp?.Address}");
            Console.WriteLine($"Response-Endpoint: {response.Origin.Address}");
            Console.WriteLine($"Type: {response.Type}");
            Console.WriteLine($"TypeString: {response.TypeString}");
            Console.WriteLine($"Identifier: {response.Identifier}");
            Console.WriteLine($"Sequence: {response.Sequence}");
            Console.WriteLine($"Checksum: {response.Checksum}");
            Console.WriteLine($"Payload size: {response.PayloadSize}");
            Console.WriteLine($"DATA: {BitConverter.ToString(response.Data.ToArray())}");
            Console.WriteLine($"RoundTripTime: {response.RoundTripTime} ms");
            Console.WriteLine("Finished");
        }
        catch (Exception exception)
        {
            Console.WriteLine("an error occured!");
            Console.WriteLine($"Message: {exception.Message}");
            Console.WriteLine($"ST: {exception.StackTrace}");
        }
    }
```
Here are some results. All results are fetched from a linux os (kubuntu 21.10).
```shell
➜  PingTest git:(main) ✗ sudo dotnet run
Code: 0
Address family: IpV6
TTL: 0
Ping-Endpoint: one.one.one.one
Sender-Endpoint: 127.0.1.1
Response-Endpoint: 2606:4700:4700::1111
Type: 129
TypeString: EchoReply
Identifier: 0
Sequence: 0
Checksum: 0
Payload size: 40
DATA: 81-00-09-99-01-00-01-00-DB-48-18-74-3B-46-75-B0-FD-94-81-9C-66-1C-FF-DB-7B-52-F7-FD-1E-7D-A3-DF-C6-6D-AE-80-6C-39-F8-8C
RoundTripTime: 8 ms
Finished

```

So, let's interpret the result:
- Code -> is the response code value from the ping socket result byte[1] in returned datagram
- AddressFamily -> internal calculation of with which protocolfamily the ping is sended (ipv4 or ipv6). It depends on your network settings and with which hostOrIpAddress you ask.
- TTL -> response time to live. It is not a time, mor the amount of hops from returned host to your client. The value you give to se ping has the same behavior but it is not the same result from the response. It is very usefull, if you would build up a traceroute. For more informations about ttl see here:
  https://de.wikipedia.org/wiki/Time_to_Live#:~:text=In%20IPv6%20wurde%20die%20TTL,der%20maximal%20m%C3%B6gliche%20Wert%20255.
- FromIp -> Is the Ip-Address from which the client sends the ping command. It must not be a public address
- Origin -> That is the IP-address from the answering host. It could be a different one, if the sended ttl is lower then the hops to host.
- Type -> The response type as 8bit field. it is different from ipv4 to ipv6
- TypeString -> The response type as written text (from rfc-792 page). EchoReply means, that the answered host ist the asked host.
- Identifier -> only ipv4
- Sequence -> only ipv4
- Checksum -> only ipv4
- PayloadSize -> the size of the datagram which comes from host.
- Data -> The byte array of the datagram. It is that, what the Socket read from the host. It is more a debugging feature. All the values from that array are encapsulated in the appropriate fields.
- RoundTripTime -> the time from sending the packet to the host and receive the answer.

Let's do a little different ping:
`var response = await Icmp.Ping("1.1.1.1", 64, 3000, cts.Token);`

We change the ip address to an ipv4 one. And let's see what happened:
```shell
➜  PingTest git:(main) ✗ sudo dotnet run
Code: 0
Address family: IpV4
TTL: 58
Ping-Endpoint: 1.1.1.1
Sender-Endpoint: 127.0.1.1
Response-Endpoint: 1.1.1.1
Type: 0
TypeString: EchoReply
Identifier: 69
Sequence: 15360
Checksum: 53873
Payload size: 60
DATA: 45-00-00-3C-B9-FE-00-00-3A-01-02-3C-01-01-01-01-C0-A8-01-DD-00-00-71-D2-01-00-01-00-51-99-DB-C3-F5-65-8D-61-4C-17-78-11-6A-6F-64-A1-5D-7F-68-A1-1E-A3-6C-2F-E8-26-77-74-5A-96-3D-AA
RoundTripTime: 11 ms
Finished
```

Aha, we receive some more results.
In the following, there are only the changed results described:
- AddressFamily -> ok, we ping in ipv4 manner
- TTL -> The pinged host send us a value how many hops he is away from our client. This one must interpreted as 64 - 58 = 6 + 1 Why + 1? In this case the pinged host does'nt know about our internal network. He knows only our public router. If you have a very complicated company net with a lot of internal routing, the value could be much higher.
  Probably I think, this could be the reason, why the response ttl does not work with ipv6. With an ipv6-address, the client is able to be the endpoint, there is no network router. Same goes for intermediate hops.
- Type -> Aha, in ipv4 the Type for a valid echo reply is 0 (in ipv6 it is 129).
  The other changed things are more debugging things and i did'nt explain that. See in the rfc-792 if it is importand for you.

And now, let's do a funny thing.
`var response = await Icmp.Ping("1.1.1.1", 3, 3000, cts.Token);`

We change the ttl from 64 to 3. Let's look to the result:
```shell
➜  PingTest git:(main) ✗ sudo dotnet run
Code: 0
Address family: IpV4
TTL: 253
Ping-Endpoint: 1.1.1.1
Sender-Endpoint: 127.0.1.1
Response-Endpoint: 89.1.16.161
Type: 11
TypeString: TimeExceeded
Identifier: 69
Sequence: 14336
Checksum: 41026
Payload size: 56
DATA: 45-00-00-38-00-00-00-00-FD-01-91-9D-59-01-10-A1-C0-A8-01-DD-0B-00-42-A0-00-00-00-00-45-00-00-3C-00-00-40-00-01-01-B5-3A-C0-A8-01-DD-01-01-01-01-08-00-A8-5F-01-00-01-00
RoundTripTime: 10 ms
Finished
```

Now, you could see how traceroute work. We limit the ttl to the host to 3. The max amount of hops to the pinged endpoint 1.1.1.1 is 3. As we know from the foreign result that the amount of hops to 1.1.1.1 is in minumum 7 from our client, we never reached the asked endpoint!
But we reached the 3rd hop (endpoint) of this route.
In my case it is: 89.1.16.161

And what is the context with traceroute?

Ok, lets look.

If we wrote a small app, that chain (e.g. from 1 to 30) and ping so long, until the type is EchoReply?

It could be a simple for-each-loop.

With the response endpoint of every intermediate step and the roundtriptime, you receive every hop from your client to the host.

Nothing more does traceroute do.

Very nice, indeed?

And now, let's check if this could be a real scenario and use the ping from the operating system and check this:
```shell
➜  PingTest git:(main) ✗ ping 1.1.1.1 -4 -c 1 -t 3
PING 1.1.1.1 (1.1.1.1) 56(84) bytes of data.
From 89.1.16.161 icmp_seq=1 Time to live exceeded

--- 1.1.1.1 ping statistics ---
1 packets transmitted, 0 received, +1 errors, 100% packet loss, time 0ms
```

The lib does exactly the same things as the os ping do. But now, we have all the funny results in our app.

The returned type (and TypeString) comes also back correctly. In this case it is 11 (TimeExceeded)

## 2022-02-05 Built in TRACEROUTE
In release 0.1.14 of this lib, i deliver a built in function of traceroute.
It´s easy to use as you can see in the following example.

````c#
    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("missing at least one parameter ip-address or hostname");
            Environment.Exit(-127);
        }


        var hostNameOrAddress = args[0];

        var l = Traceroute.DoTraceroute(hostNameOrAddress, 30, 1000);

        await l.ForEachAsync(response =>
        {
            response.Match(leftResult => Console.WriteLine($"{leftResult.Hop} : {leftResult.Exception.Message}"),
                rightResult =>
                    Console.WriteLine(
                        $"{rightResult.Hop} : {rightResult.Origin?.Address.ToString()} : {rightResult.RoundTripTime} ms : {rightResult.State}"));
        });


        Console.WriteLine("ready");
    }

````

The method returns an `IAsyncEnumerable`. That´s because of the inner handling of the traceroute method.
It yields every result, so it is very asynchronous.

If you wish to select / foreach every result, you must import `System.Linq.Async`.

The result type of the `IAsyncEnumerable` is an Either<LEFT,RIGHT> type.

Some times away, i code in Scala and within Scala it is a very useful type when you return on the right hand the "good" result and on the left hand the "bad" one.
In the processing you can match the left and the right and handle it separately.

As for an example, if the request timed out / or the cancellation token throws an exception because of timeout, the result returned a left hand with the exception and the current hop.
If the result returned a valid response, the right hand where filled.
So, the execution result looks as follows (tracerouting a domain spiegel.de, from my location i know there is at least one hop that drops the ping) :

````shell
➜  PingTest git:(main) ✗ sudo dotnet run spiegel.de
1 : 172.31.112.1 : 4 ms : TimeExceeded
2 : 192.168.0.1 : 0 ms : TimeExceeded
3 : 195.14.226.125 : 5 ms : TimeExceeded
4 : 89.1.16.169 : 5 ms : TimeExceeded
5 : 81.173.192.118 : 25 ms : TimeExceeded
6 : 80.81.192.218 : 9 ms : TimeExceeded
7 : The operation was canceled.
8 : 80.95.144.117 : 9 ms : TimeExceeded
9 : 128.65.210.8 : 9 ms : EchoReply
ready
````
As you can see on hop 7, the cancellation token was thrown an exception because of timeout and the left hand returned the exception and the hop.

For that case, i write the exception message to the Console.
The right hand do some other stuff (writes other text). Here you can do a resolving dns names for the ip-address or other useful things.

## TRACEROUTE

Traceroute, in simple words, is a chain of pings where the ttl-value is increased by 1 for every hop, until the origin endpoint is reached.
I changed the above described ping method and enhance this thing a little bit.
```c#
public static async Task Main(string[] args)
    {
        var hostNameOrAddress = "heise.de";

        var cnt = 1;
            var state = 3;
            while (cnt <= 30 && state is not 0 and not 129)
            {
                try
                {
                    var cts = new CancellationTokenSource(20000);
                    var response = await Icmp.Ping(hostNameOrAddress, cnt, 500, true, cts.Token);

                    var hostName = "";
                    try
                    {
                        hostName = (await Dns.GetHostEntryAsync(response.Origin.Address)).HostName;
                    }
                    catch (Exception)
                    {
                        hostName = response.Origin.Address.ToString();
                    }
                    var writeLine = response.AddressFamily switch
                    {
                        IpAddressFamily.IpV4 =>
                            $"{cnt.ToString("D2")} {response.Origin.Address} ({hostName}) :: TTL: {response.Ttl} :: TYPE: {response.TypeString} ({response.Type}) in {response.RoundTripTime} ms",
                        IpAddressFamily.IpV6 =>
                            $"{cnt.ToString("D2")} {response.Origin.Address} ({hostName}) :: TYPE: {response.TypeString} ({response.Type}) in {response.RoundTripTime} ms;",
                        _ => "Unknown address family received, received an updated library?"
                    };
                    Console.WriteLine(writeLine);
                    state = response.Type;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{cnt:D2} :: {ex.Message} :: ERROR possible drop icmp");
                }
                cnt++;
            }
    }
```

Lets explain some things.
We build up a while-loop. The loop ends, when
- the max hop count (30 per default for the most traceroute implementations) is received
- the state of the result is 0 for ipv4 or 129 for ipv6 which means EchoReply (or endpoint reached).
- the try-catch block inside the loop, brings up a possible exception (mostly token cancellation, if the pinged host drops icmp packages)
- the inner try-catch block (surround the dns resolve) will catch exceptions, if an ip address is not resolvable to a hostname. Instead, we give back the ip-address (like other traceroute tools this do).

I set the cancellation token inside the loop, so in every loop step the token time where reset.
I set the cancellation token time to a very high amount (you can make that configurable for your own purposes), to receive long running neighbor things.
I put away some unused debug information and format the return a little bit (one liner and so on)
Let's do some tests.

First with an ipv4 ip address 1.1.1.1

```shell
➜  PingTest git:(main) ✗ sudo dotnet run
01 192.168.0.1 (_gateway) :: TTL: 64 :: TYPE: TimeExceeded (11) in 5 ms
02 195.14.226.125 (bras-vc2.netcologne.de) :: TTL: 254 :: TYPE: TimeExceeded (11) in 6 ms
03 89.1.16.161 (ip-core-eup2-ae12.netcologne.de) :: TTL: 253 :: TYPE: TimeExceeded (11) in 6 ms
04 89.1.86.37 (ip-core-net1-et9-2-2.netcologne.de) :: TTL: 252 :: TYPE: TimeExceeded (11) in 6 ms
05 81.173.192.2 (bdr-net1-ae1.netcologne.de) :: TTL: 251 :: TYPE: TimeExceeded (11) in 7 ms
06 194.146.118.139 (as13335.dusseldorf.megaport.com) :: TTL: 249 :: TYPE: TimeExceeded (11) in 7 ms
07 1.1.1.1 (one.one.one.one) :: TTL: 58 :: TYPE: EchoReply (0) in 7 ms
```

Wow, that looks pretty cool!
And yes, i have a DSL-Connection, but this thing ist very fast.
Let's use the ordinary traceroute from linux and make a check:

Parameters are:

-I --> use ICMP (instead of tcp 80)

-q 1 --> make 1 request (instead of 3)

```shell
➜  PingTest git:(main) ✗ traceroute -I -n -q 1 1.1.1.1
traceroute to 1.1.1.1 (1.1.1.1), 30 hops max, 60 byte packets
 1  192.168.0.1  3.281 ms
 2  195.14.226.125  8.389 ms
 3  89.1.16.161  8.431 ms
 4  89.1.86.37  8.728 ms
 5  81.173.192.2  9.387 ms
 6  194.146.118.139  23.367 ms
 7  1.1.1.1  10.224 ms
```

OK, very impressive.

Let's try another one:

Traceroute to heise.de (a german it-magazine)

```shell
➜  PingTest git:(main) ✗ sudo dotnet run               
01 2001:4dd6:5411:0:201:2eff:fe95:fd24 (2001-4dd6-5411-0-201-2eff-fe95-fd24.ipv6dyn.netcologne.de) :: TYPE: TimeExceeded (3) in 7 ms;
02 2001:4dd0:a2a:62::4acc (bras-vc2-lo0.netcologne.de) :: TYPE: TimeExceeded (3) in 6 ms;
03 2001:4dd0:a2b:e0:dc30::c (ip-core-sto1-ae12.netcologne.de) :: TYPE: TimeExceeded (3) in 7 ms;
04 2001:4dd0:a2b:e9:dc31::b (bdr-sto1-ae1.netcologne.de) :: TYPE: TimeExceeded (3) in 7 ms;
05 fe80::201:2eff:fe95:fd24%3 (_gateway) :: TYPE: NeighborAdvertisement (136) in 4821 ms;
06 2001:4dd6:5411:0:201:2eff:fe95:fd24 (2001-4dd6-5411-0-201-2eff-fe95-fd24.ipv6dyn.netcologne.de) :: TYPE: NeighborSolicitation (135) in 308 ms;
07 fe80::201:2eff:fe95:fd24%3 (_gateway) :: TYPE: NeighborSolicitation (135) in 4490 ms;
08 2a02:2e0:12:32::2 (2a02:2e0:12:32::2) :: TYPE: TimeExceeded (3) in 11 ms;
09 2a02:2e0:3fe:0:c::1 (2a02:2e0:3fe:0:c::1) :: TYPE: TimeExceeded (3) in 11 ms;
10 2a02:2e0:3fe:1001:302:: (redirector.heise.de) :: TYPE: EchoReply (129) in 10 ms;
```

Aha, nice types with a spooky fe80 local address from an internet hop. At now, i can't say what neighbor Advertisement say, but, it looks impressive.

And now the check with the ordinary traceroute:

The new parameter -6 is, because without this traceroute does a ipv4 call.
```shell
➜  PingTest git:(main) ✗ traceroute -6 -I -q 1 heise.de 
traceroute to heise.de (2a02:2e0:3fe:1001:302::), 30 hops max, 80 byte packets
 1  2001-4dd6-5411-0-201-2eff-fe95-fd24.ipv6dyn.netcologne.de (2001:4dd6:5411:0:201:2eff:fe95:fd24)  4.185 ms
 2  bras-vc2-lo0.netcologne.de (2001:4dd0:a2a:62::4acc)  9.130 ms
 3  ip-core-sto1-ae12.netcologne.de (2001:4dd0:a2b:e0:dc30::c)  12.282 ms
 4  bdr-sto1-ae1.netcologne.de (2001:4dd0:a2b:e9:dc31::b)  12.274 ms
 5  be100.c350.f.de.plusline.net (2001:7f8::3012:0:2)  12.268 ms
 6  c301.f.de.plusline.net (2a02:2e0:11::301)  12.261 ms
 7  *
 8  2a02:2e0:12:32::2 (2a02:2e0:12:32::2)  13.541 ms
 9  2a02:2e0:3fe:0:c::1 (2a02:2e0:3fe:0:c::1)  12.225 ms
10  redirector.heise.de (2a02:2e0:3fe:1001:302::)  13.526 ms
```

Oops, whats that? #7 is marked with a star (not resolvable)?
But there are results...btw. in my resultset but obviously from my local internet router.
I think the shorter receive timeout from the os traceroute prevent a result and give instead a drop back

And here is a check, that the exception handling works as expected.
```shell
➜  PingTest git:(main) ✗ sudo dotnet run
01 2001:4dd6:5411:0:201:2eff:fe95:fd24 :: TYPE: TimeExceeded (3) in 5 ms;
02 2001:4dd0:a2a:62::4acc :: TYPE: TimeExceeded (3) in 6 ms;
03 2001:4dd0:a2b:e0:dc30::c :: TYPE: TimeExceeded (3) in 6 ms;
04 2001:4dd0:a2b:e9:dc31::b :: TYPE: TimeExceeded (3) in 13 ms;
05 2001:7f8::3012:0:2 :: TYPE: TimeExceeded (3) in 10 ms;
06 :: The operation was canceled. :: ERROR possible drop icmp
07 2001:4dd6:5411:0:201:2eff:fe95:fd24 :: TYPE: NeighborSolicitation (135) in 3003 ms;
08 2a02:2e0:12:32::2 :: TYPE: TimeExceeded (3) in 9 ms;
09 2a02:2e0:3fe:0:c::1 :: TYPE: TimeExceeded (3) in 10 ms;
10 2a02:2e0:3fe:1001:302:: :: TYPE: EchoReply (129) in 9 ms;
```
## Traceroute 2
I found out, that the method 

`(await Dns.GetHostEntryAsync(response.Origin.Address)).HostName`

work not correctly, even when you give a CancellationToken to the async method.

Why? No clue!

I´ve implemented a "helper" method in the lib, that resolve the problem for you.

````c#
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
````

It trys to resolve the ip address in a synced / blocking manner. But surrounded with a Task.

That´s all.

In your code, you wait for the response for a configurable amount of time.

`hostNameTask.Wait(200) ? hostNameTask.Result : response.Origin.Address.ToString();`

If the wait (in this case after 200ms) gets no result, the false leg returned the original given value (ip address).

Ah and another one. The main method get the ip address / host name to traceroute from outside.

In Linux you can now call:

`sudo dotnet run one.one.one.one`

or

`sudo dotnet run 1.1.1.1`

With windows you can call:

`dotnet run one.one.one.one`

or

`dotnet run 1.1.1.1`

Here is the final code of the traceroute main method.

````c#
    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("missing at least one parameter ip-address or hostname");
            Environment.Exit(-127);
        }

        var hostNameOrAddress = args[0];

        var cnt = 1;
        var state = 3;
        while (cnt <= 30 && state is not 0 and not 129)
        {
            try
            {
                var cts = new CancellationTokenSource(3000);
                var response = await Icmp.Ping(hostNameOrAddress, cnt, 2000, true, cts.Token);

                var hostNameTask = Icmp.AsyncResolveHostName(response.Origin.Address.ToString());
                var hostName = hostNameTask.Wait(200) ? hostNameTask.Result : response.Origin.Address.ToString();
                var writeLine = response.AddressFamily switch
                {
                    IpAddressFamily.IpV4 =>
                        $"{cnt.ToString("D2")} {response.Origin.Address} ({hostName}) :: TTL: {response.Ttl} :: TYPE: {response.TypeString} ({response.Type}) in {response.RoundTripTime} ms",
                    IpAddressFamily.IpV6 =>
                        $"{cnt.ToString("D2")} {response.Origin.Address} ({hostName}) :: TYPE: {response.TypeString} ({response.Type}) in {response.RoundTripTime} ms;",
                    _ => "Unknown address family received, received an updated library?"
                };
                Console.WriteLine(writeLine);
                state = response.Type;
            }
            catch (OperationCanceledException operationCanceledException)
            {
                Console.WriteLine($"{cnt:D2} * :: {operationCanceledException.Message} ERROR possible drop icmp");
            }
            catch (SocketException socketException)
            {
                Console.WriteLine($"An error while tracerouting an address occured. {socketException.Message}");
                Console.WriteLine($"Stacktrace: {socketException.StackTrace}");
                Environment.Exit(-127);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"a default error occured, gave up :: {ex.Message}");
                Console.WriteLine($"Stacktrace: {ex.StackTrace}");
            }

            cnt++;
        }
    }
````