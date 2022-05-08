# LibPing.Net

Regards and thanks for usage.

If you only know about how you can build ping or traceroute things, please visit the <a href="SAMPLES.md">samples</a> file, where you can find examples for those routines.

## Which configuration works?
The following list shows some configurations with which the library was tested.

| System                    | Current state | Description  | Tested with version |
|---------------------------|---------------|--------------|---------------------|
| Microsoft Windows (x64)   | working       |              | 0.1.18              |
| Microsoft Windows (arm64) | not tested    |              |                     |
| Linux (x64)               | working       | ubuntu       | 0.1.19              |
| Linux (arm64)             | not tested    |              |                     |
| MacOSx (x64)              | not tested    |              |                     |
| MacOSx (arm64)            | working       |              | 0.1.18              |
| Docker (x64)              | working       | ubuntu image | 0.1.18              |
| Docker (arm64)            | not tested    |              |                     |

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

**As of operating systems restrictions (windows, linux and mac) you should run the app included this library with administrator privileges.**

E.g. https://docs.microsoft.com/de-de/windows/win32/winsock/tcp-ip-raw-sockets-2?redirectedfrom=MSDN


Thats sad, but not my fault.


Anyway.


If you use the lib in your project, simply do things as this (c# syntax):


`var response = await Icmp.Ping("1.1.1.1", 64, 3000, true, cts.Token);`


OR


`var response = await Icmp.Ping("one.one.one.one", 64, 3000, true, cts.Token);`


OR


`var response = await Icmp.Ping("2606:4700:4700::1111", 64, 3000, true, cts.Token);`

OR (in case that you need it in synchronous manner)

`var respnse = Icmp.PingSync("hostNameOrIp", ttl, timeout, dontFragment)`


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

Please look at the <a href="SAMPLES.md">SAMPLES.md</a> document to checkout how the things work and how you could use the lib to build ping and traceroute methods.

## 2022-02-05 Implementation of traceroute in this library
As a result of a request, i implement a traceroute function in this library. See the samples.md for an example of usage. 

Have a great day!