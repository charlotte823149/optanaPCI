using System.Collections.Generic;

namespace optanaPCI.Data
{
    public class IRIData
    {
        public string Name { get; set; }
        public double IRI_left { get; set; }
        public double IRI_right { get; set; }
        public double IRI_average { get; set; }
        public double Stake_start { get; set; }
        public double Stake_end { get; set; }
        public List<ccdData> Ccd { get; set; } = new List<ccdData>();
    }
}


