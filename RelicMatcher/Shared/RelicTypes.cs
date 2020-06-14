using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.ComponentModel.DataAnnotations;


namespace RelicMatcher.Shared
{
    public class UserWrapper
    {
        public Guid UserGuid { get; set; }
        public string DisplayName { get; set; }
        public bool Accepted { get; set; }
    }

    public class RelicType
    {
        public string UniqueName { get; set; }
        public string BaseName { get; set; }
        public bool Vaulted { get; set; }
        public string DisplayName => Vaulted ? $"{BaseName} (Vaulted)" : BaseName;
    }

    public class RelicQueueDisplay
    {
        [Required]
        public string User { get; set; }
        [Required]
        public string RelicDisplayName { get; set; }
    }

    public class RelicQueueInput
    {
        [Required]
        public string User { get; set; }
        [Required]
        public string RelicUniqueName { get; set; }
    }

    public class Party
    {
        public Guid PartyId { get; set; } = Guid.NewGuid();
        public RelicType RelicType { get; set; }
        public IEnumerable<UserWrapper> Members { get; set; }
        public bool Done { get; set; }
        public DateTime DeadLine { get; set; }
    }


}
