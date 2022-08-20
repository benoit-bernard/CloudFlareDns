namespace CloundFlaraDynDNS.Services.Api;

internal interface IPublicIpService
{
    Task<string> GetPublicIp();
}