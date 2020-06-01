using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.ComponentModel.DataAnnotations;


namespace RelicMatcher.Shared
{
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

}
