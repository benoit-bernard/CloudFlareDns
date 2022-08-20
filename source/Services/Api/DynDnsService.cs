using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloundFlaraDynDNS.Services.Api
{
    internal class DynDnsService : IDynDnsService
    {
        private ICloudFlareDnsService _cfDns;
        private ILogger               _logger;
        private IPublicIpService      _publicIpService;

        public DynDnsService(ICloudFlareDnsService cfDns, IPublicIpService publicIpService, ILogger<DynDnsService> logger)
        {
            _cfDns = cfDns;
            _publicIpService = publicIpService;
            _logger = logger;
        }
        public async Task UpdateDns()
        {
            var ip = await _publicIpService.GetPublicIp();
            if (string.IsNullOrEmpty(ip))
                return;
            await _cfDns.UpdateAllDomains(ip);
        }
    }
}
