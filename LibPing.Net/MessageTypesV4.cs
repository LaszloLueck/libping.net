namespace LibPing.Net;

/// <summary>
/// message types for ipv4 from documentation of rfc-792
/// </summary>
public enum MessageTypeV4
{
    EchoReply = 0,
    DestinationUnreachable = 3,
    SourceQuench = 4,
    Redirect = 5,
    EchoRequest = 8,
    TimeExceeded = 11,
    ParameterProblem = 12,
    Timestamp = 13,
    TimestampReply = 14,
    InformationRequest = 15,
    InformationReply = 16
}

/// <summary>
/// message types for ipv6 from documentation of rfc-792
/// </summary>
public enum MessageTypeV6
{
    DestinationUnreachable = 1,
    PacketTooBig = 2,
    TimeExceeded = 3,
    PrivateExperimentation100 = 100,
    PrivateExperimentation101 = 101,
    EchoRequest = 128,
    EchoReply = 129,
    MulticastListenerQuery = 130,
    Version1MulticastListenerQuery = 131,
    MulticastListenerDone = 132,
    RouterSolicitation = 133,
    RouterAdvertisement = 134,
    NeighborSolicitation = 135,
    NeighborAdvertisement = 136,
    Redirect = 137,
    RouterRenumbering = 138,
    IcmpNodeInformationQuery = 139,
    IcmpNodeInformationResponse = 140,
    InverseNeighborDiscoverySolicitationMessage = 141,
    InverseNeighborDiscoveryAdvertisementMessage = 142,
    Version2MulticastListenerReport = 143,
    HomeAgentAddressDiscoveryRequestMessage = 144,
    HomeAgentAddressDiscoveryReplyMessage = 145,
    MobilePrefixSolicitation = 146,
    MobilePrefixAdvertisement = 147,
    CertificationPathSolicitationMessage = 148,
    CertificationPathAdvertisementMessage = 149,
    IcmpMessagesUtilizedByExperimentalMobilityProtocolsSuchAsSeamoby = 150,
    MulticastRouterAdvertisement = 151,
    MulticastRouterSolicitation = 152,
    MulticastRouterTermination = 153,
    RplControlMessage = 154,
    PrivateExperimentation200 = 200,
    PrivateExperimentation201 = 201,
    ReservedForExpansionOfIcmpV6InformationalMessages = 255
}

/// <summary>
/// Internal enum for handling the ip address family (ipv4 / ipv6)
/// </summary>
public enum IpAddressFamily
{
    IpV4,
    IpV6
}