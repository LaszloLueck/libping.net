using System.Net;

namespace LibPing.Net;

public record TracerouteResponse(int Hop, long RoundTripTime, IPEndPoint? Origin, string State, int Type);