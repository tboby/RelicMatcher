using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using RelicMatcher.Shared;

namespace RelicMatcher.Client.ViewModels
{
    public class UserSettings
    {
        [Required]
        public string DisplayName { get; set; }
        [Required]
        public Platform? Platform { get; set; }
        [Required]
        public Region? PreferredRegion { get; set; }
        [Required]
        public HostPreference? HostPreference { get; set; }
    }
}
