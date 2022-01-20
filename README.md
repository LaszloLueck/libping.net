# LibPing.Net

Regards and thanks for usage.

## Installation
As easy as is, use nuget to include the lib (dll) to your project and use it.

Or you download the sources (clone the repo) and compile it for yourself.

## Motivation
I found, that the current implementation of ping (https://docs.microsoft.com/de-de/dotnet/api/system.net.networkinformation.ping?view=net-6.0) in dotnet core (current 6.0.1)
- is very buggy if you don´t use windows
- use the ping command from operating system

The buggy implementation prevent me to use this functionality inside a linux docker container.

A ping from macos or linux returned in every case a timeout, if the ttl is set to lower than the pinged host is away.

And for the other point, it feels for me very clumsy, when i know, that there is not a native implementation of things although there are all the stuff here to implement it correctly.

And it is a lot of fun to code with low level network sockets, i've learned al lot of things.

Some tips and tricks and how the things could work, i learned from here

http://www.java2s.com/Code/CSharp/Network/SimplePing.htm

Very usefull code example


I take the newest (from 1981) documentation from here about icmp

https://datatracker.ietf.org/doc/html/rfc792

and read / try to understand how it works.

For debugging purposes, i use wireshark, to see directly what goes to the network and what comes back.

As i say, very forensic and funny.

## Usage
**Beware!**

**As of operating systems restrictions (windows, linux and mac) you should run the app with the library with administrator privileges.**

E.g. https://docs.microsoft.com/de-de/windows/win32/winsock/tcp-ip-raw-sockets-2?redirectedfrom=MSDN


Thats sad, but not my fault.


Anyway.


If you use the lib in your project, simply do things as this (c# syntax):


`var response = await Icmp.Ping("1.1.1.1", 64, 3000, true, cts.Token);`


OR


`var response = await Icmp.Ping("one.one.one.one", 64, 3000, true, cts.Token);`


OR


`var response = await Icmp.Ping("2606:4700:4700::1111", 64, 3000, true, cts.Token);`



The method Ping(...) runs asynchron, so it´s in your app-code to await the things.
Later i will implement a synchronous variant (internal async)

The method parameters are:
- ipOrHostName - string - as described, the ip or hostname of the host to ping
- ttl - int - it´s not a time but more a number of max hops to the host to ping. Its very usefull for traceroutes.
- receiveTimeout - int - timespan in ms when the packet is send and no pong returned back
- dontFragment (optional) - bool - if the frame too big, split it in multiple packets. Attention! This thing does not run with MacOS
- token - CancellationToken - The token, when the async process is cancelled (async can theoretically runs for ever and that ist not the goal). Attention! The token value should be higher than the receiveTimeout value because it cancel the async process before the receiveTimeout is reached!


The result is much more comprehensive than the std OS-Ping or the dotnet core ping, which use the std-OS-Ping.
If you don´t believe this, rename your ping command and try to run the dotnet core ping. It says: No ping command found.
Lets look at the result.

To fetch some results i've written a small app, that use lib and show the results on console.

```csharp
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
```bash
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
```bash
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
- Type -> Aha, in ipv4 the Type for a valid echo reply is 0 (in ipv6 it is 129).
The other changed things are more debugging things and i did'nt explain that. See in the rfc-792 if it is importand for you.

And now, let's do a funny thing.
`var response = await Icmp.Ping("1.1.1.1", 3, 3000, cts.Token);`

We change the ttl from 64 to 3. Let's look to the result:
```bash
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

Very nice.

And now, let's check if this could be a real scenario and use the ping from the operating system and check this:
```bash
➜  PingTest git:(main) ✗ ping 1.1.1.1 -4 -c 1 -t 3
PING 1.1.1.1 (1.1.1.1) 56(84) bytes of data.
From 89.1.16.161 icmp_seq=1 Time to live exceeded

--- 1.1.1.1 ping statistics ---
1 packets transmitted, 0 received, +1 errors, 100% packet loss, time 0ms
```

The lib does exactly the same things as the os ping do. But now, we have all the funny results in our app.

The returned type (and TypeString) comes also back correctly. In this case it is 11 (TimeExceeded)

Wonderful.
