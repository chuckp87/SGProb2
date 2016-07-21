using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SGProb2
{
    public class SpinResult
    {
        public string PlayerName { get; set; }
        public int PlayerCredits { get; set; }
        public int LifetimeSpins { get; set; }
        public decimal LifetimeAvgReturn { get; set; }
    }
}