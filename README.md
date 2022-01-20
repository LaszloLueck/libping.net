# LibPing.Net

Regards and thanks for usage.

## Installation
As easy as is, use nuget to include the lib (dll) to your project and use it.

Or you download the sources (clone the repo) and compile it for yourself.

## Usage
Beware!

As of operating systems restrictions (windows, linux and mac) you should run the app with the library with administrator privileges.

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
