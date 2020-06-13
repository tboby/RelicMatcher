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
    public enum RelicType {
        A1,
        A2,
        A3,
        A4
    }
    public class RelicQueueItem
    {
        [Required]
        public string User { get; set; }
        [Required]
        public RelicType RelicType { get; set; }
    }

    public class Party
    {
        public Guid PartyId { get; set; } = Guid.NewGuid();
        public RelicType RelicType { get; set; }
        public IEnumerable<UserWrapper> Members { get; set; }
        public bool Done { get; set; }
    }


}
