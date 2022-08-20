using CloudFlare.Client;
using CloudFlare.Client.Api.Zones;
using CloudFlare.Client.Api.Zones.DnsRecord;
using CloudFlare.Client.Enumerators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloundFlaraDynDNS.Services.Api
{
    internal class CloudFlareDnsService : ICloudFlareDnsService
    {
        ILogger<CloudFlareDnsService> _logger;
        private string                _account;
        private string                _key;
        public CloudFlareDnsService(IConfiguration configuration, ILogger<CloudFlareDnsService> logger)
        {
            _logger = logger;
            _account = configuration["AppSettings:Account"];
            _key = configuration["AppSettings:ApiKey"];
        }
        public async Task UpdateAllDomains(string publicIp)
        {
            try
            {
                using (var client = new CloudFlareClient("contact@skalae.fr", "290e383539741cc4008b610e3e7ab6f515841"))
                {
                    var zoneFilter = new ZoneFilter();
                    zoneFilter.Status = ZoneStatus.Active;

                    var zonesQueryResult = await client.Zones.GetAsync(zoneFilter);
                    if (!zonesQueryResult.Success)
                    {
                        var messages = zonesQueryResult?.Errors?.Select(x => x?.Message);
                        _logger.LogError($"Can not read zone in CloudFlare > {string.Join(",", $"{messages}")}");
                        return;
                    }

                    _logger.LogInformation($"zones found: {zonesQueryResult.Result.Count()}");

                    foreach (var zone in zonesQueryResult.Result)
                    {
                        var dnsRecords = await client.Zones.DnsRecords.GetAsync(zone.Id);
                        if (!dnsRecords.Success)
                            continue;

                        foreach (var dnsRecord in dnsRecords.Result)
                        {
                            await UpdateDnsRecord(publicIp, dnsRecord, client);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exception trying to update dns > {ex}");
            }
        }

        private async Task UpdateDnsRecord(string publicIp, DnsRecord dnsRecord, CloudFlareClient client)
        {
            if (dnsRecord.Type != DnsRecordType.A)
                return;
            if (publicIp == dnsRecord.Content)
                return;

            var modifiedRecord = new ModifiedDnsRecord() { Content = publicIp, Name = dnsRecord.Name, Proxied = dnsRecord.Proxied, Type = dnsRecord.Type, Ttl = dnsRecord.Ttl };
            await client.Zones.DnsRecords.UpdateAsync(dnsRecord.ZoneId, dnsRecord.Id, modifiedRecord);
            _logger.LogInformation($"Updated to new IP > Type={dnsRecord.Type}: {dnsRecord.Name} => {dnsRecord.Content}");
        }
    }
}
