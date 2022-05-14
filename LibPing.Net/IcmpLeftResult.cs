namespace LibPing.Net;

/// <summary>
/// If the safe ping method was called and an exception was thrown, this is the return record.
/// </summary>
/// <param name="exception">the thrown exception</param>
/// <param name="ipOrHostName">the origin given host name or ip address</param>
public record IcmpLeftResult(Exception exception, string ipOrHostName);