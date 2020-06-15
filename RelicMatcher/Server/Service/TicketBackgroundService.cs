using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using RelicMatcher.Server.Hubs;

namespace RelicMatcher.Server.Service
{
    public class TicketBackgroundService
    {
        private readonly RelicHubResponseService _relicHubResponseService;

        public TicketBackgroundService(RelicHubResponseService relicHubResponseService)
        {
            _relicHubResponseService = relicHubResponseService;
        }

        public async Task BackgroundChecks()
        {
            await _relicHubResponseService.CheckForGroups();
            await _relicHubResponseService.ExpireGroups();
        }
    }
}
