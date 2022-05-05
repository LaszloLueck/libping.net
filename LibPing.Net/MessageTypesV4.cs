namespace LibPing.Net;

/// <summary>
/// message types for ipv4 from documentation of rfc-792
/// </summary>
public enum MessageTypeV4
{
    /// <summary>
    /// 
    /// </summary>
    EchoReply = 0,
    /// <summary>
    /// 
    /// </summary>
    DestinationUnreachable = 3,
    /// <summary>
    /// 
    /// </summary>
    SourceQuench = 4,
    /// <summary>
    /// 
    /// </summary>
    Redirect = 5,
    /// <summary>
    /// 
    /// </summary>
    EchoRequest = 8,
    /// <summary>
    /// 
    /// </summary>
    TimeExceeded = 11,
    /// <summary>
    /// 
    /// </summary>
    ParameterProblem = 12,
    /// <summary>
    /// 
    /// </summary>
    Timestamp = 13,
    /// <summary>
    /// 
    /// </summary>
    TimestampReply = 14,
    /// <summary>
    /// 
    /// </summary>
    InformationRequest = 15,
    /// <summary>
    /// 
    /// </summary>
    InformationReply = 16
}

/// <summary>
/// message types for ipv6 from documentation of rfc-792
/// </summary>
public enum MessageTypeV6
{
    /// <summary>
    /// 
    /// </summary>
    DestinationUnreachable = 1,
    /// <summary>
    /// 
    /// </summary>
    PacketTooBig = 2,
    /// <summary>
    /// 
    /// </summary>
    TimeExceeded = 3,
    /// <summary>
    /// 
    /// </summary>
    PrivateExperimentation100 = 100,
    /// <summary>
    /// 
    /// </summary>
    PrivateExperimentation101 = 101,
    /// <summary>
    /// 
    /// </summary>
    EchoRequest = 128,
    /// <summary>
    /// 
    /// </summary>
    EchoReply = 129,
    /// <summary>
    /// 
    /// </summary>
    MulticastListenerQuery = 130,
    /// <summary>
    /// 
    /// </summary>
    Version1MulticastListenerQuery = 131,
    /// <summary>
    /// 
    /// </summary>
    MulticastListenerDone = 132,
    /// <summary>
    /// 
    /// </summary>
    RouterSolicitation = 133,
    /// <summary>
    /// 
    /// </summary>
    RouterAdvertisement = 134,
    /// <summary>
    /// 
    /// </summary>
    NeighborSolicitation = 135,
    /// <summary>
    /// 
    /// </summary>
    NeighborAdvertisement = 136,
    /// <summary>
    /// 
    /// </summary>
    Redirect = 137,
    /// <summary>
    /// 
    /// </summary>
    RouterRenumbering = 138,
    /// <summary>
    /// 
    /// </summary>
    IcmpNodeInformationQuery = 139,
    /// <summary>
    /// 
    /// </summary>
    IcmpNodeInformationResponse = 140,
    /// <summary>
    /// 
    /// </summary>
    InverseNeighborDiscoverySolicitationMessage = 141,
    /// <summary>
    /// 
    /// </summary>
    InverseNeighborDiscoveryAdvertisementMessage = 142,
    /// <summary>
    /// 
    /// </summary>
    Version2MulticastListenerReport = 143,
    /// <summary>
    /// 
    /// </summary>
    HomeAgentAddressDiscoveryRequestMessage = 144,
    /// <summary>
    /// 
    /// </summary>
    HomeAgentAddressDiscoveryReplyMessage = 145,
    /// <summary>
    /// 
    /// </summary>
    MobilePrefixSolicitation = 146,
    /// <summary>
    /// 
    /// </summary>
    MobilePrefixAdvertisement = 147,
    /// <summary>
    /// 
    /// </summary>
    CertificationPathSolicitationMessage = 148,
    /// <summary>
    /// 
    /// </summary>
    CertificationPathAdvertisementMessage = 149,
    /// <summary>
    /// 
    /// </summary>
    IcmpMessagesUtilizedByExperimentalMobilityProtocolsSuchAsSeamoby = 150,
    /// <summary>
    /// 
    /// </summary>
    MulticastRouterAdvertisement = 151,
    /// <summary>
    /// 
    /// </summary>
    MulticastRouterSolicitation = 152,
    /// <summary>
    /// 
    /// </summary>
    MulticastRouterTermination = 153,
    /// <summary>
    /// 
    /// </summary>
    RplControlMessage = 154,
    /// <summary>
    /// 
    /// </summary>
    PrivateExperimentation200 = 200,
    /// <summary>
    /// 
    /// </summary>
    PrivateExperimentation201 = 201,
    /// <summary>
    /// 
    /// </summary>
    ReservedForExpansionOfIcmpV6InformationalMessages = 255
}

/// <summary>
/// Internal enum for handling the ip address family (ipv4 / ipv6)
/// </summary>
public enum IpAddressFamily
{
    /// <summary>
    /// 
    /// </summary>
    IpV4,
    /// <summary>
    /// 
    /// </summary>
    IpV6
}