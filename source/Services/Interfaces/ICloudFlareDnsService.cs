namespace CloundFlaraDynDNS.Services.Api;

internal interface ICloudFlareDnsService
{
    Task UpdateAllDomains(string publicIp);
}