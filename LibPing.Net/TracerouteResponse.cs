using System.Net;

namespace LibPing.Net;

public record TracerouteResponse(int Hop, long RoundTripTime, bool HasResult, IPEndPoint? Origin, string State);