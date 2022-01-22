using System.Collections.Generic;
using System.Windows;

namespace optanaPCI.Data
{
    public class damageData
    {
        public string damageType { get; set; }
        public string damageLevel { get; set; }
        public int drawType { get; set; } 
        public double length { get; set; }
        public double area { get; set; }

        public List<Point> points = new List<Point>();
    }
}

