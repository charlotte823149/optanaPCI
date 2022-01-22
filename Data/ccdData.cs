using System.Collections.Generic;

namespace optanaPCI.Data
{
    public class ccdData
    {
        public string Name { get; set; }
        public double Longtitude { get; set; }
        public double Latitude { get; set; }
        public double Stake { get; set; }
        public string Address { get; set; }
        public List<damageData> Damages { get; set; } = new List<damageData>();
        public List<conditionData> Conditions { get; set; } = new List<conditionData>();
    }
}


