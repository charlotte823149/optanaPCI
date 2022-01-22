using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace optanaPCI.Function
{
    class cal_fuction
    {
        public double[] coe = new double[8];
        public List<Point> origin_points = new List<Point>();
        public List<Point> after_points = new List<Point>();
        public cal_fuction()
        {
            origin_points.Clear();
            origin_points.Add(new Point() { X = 360, Y = 700 });
            origin_points.Add(new Point() { X = 880, Y = 700 });
            origin_points.Add(new Point() { X = 1610, Y = 1225 });
            origin_points.Add(new Point() { X = 5, Y = 1225 });

            after_points.Clear();
            after_points.Add(new Point() { X = 0, Y = 0 });
            after_points.Add(new Point() { X = 4, Y = 0 });
            after_points.Add(new Point() { X = 4, Y = 5 });
            after_points.Add(new Point() { X = 0, Y = 5 });
            transform_matrix();
        }
        
        public double PCI(List<drawingData> drawingDataList)
        {
            //statistic all type and level length/area
            List<Statistic> statistics = new List<Statistic>();
            foreach (drawingData item in drawingDataList)
            {
                string[] type = item.damageType.Split('.');
                string[] level = item.damageLevel.Split('.');
                if (Convert.ToInt32(type[0]) != 0)
                {
                    int index = statistics.FindIndex(x => x.damageType == Convert.ToInt32(type[0]) && x.damageLevel == Convert.ToInt32(level[0]));
                    if (index != -1)
                    {
                        if (Convert.ToInt32(type[0]) ==  13)
                        {
                            statistics[index].value++;
                        }
                        else
                        {
                            statistics[index].value += item.length + item.area;
                        }                        
                    }
                    else
                    {
                        statistics.Add(new Statistic()
                        {
                            damageType = Convert.ToInt32(type[0]),
                            damageLevel = Convert.ToInt32(level[0])
                        });
                        if (Convert.ToInt32(type[0]) == 13)
                        {
                            statistics[statistics.Count - 1].value++;
                        }
                        else
                        {
                            statistics[statistics.Count - 1].value = item.length + item.area;
                        }
                    }
                }                
            }

            if (statistics.Count != 0)
            {
                //calculate density, DV
                double road_area = 3.60 * 100;
                List<double> DV_list = new List<double>();
                foreach (Statistic item in statistics)
                {
                    item.density = item.value / road_area * 100;
                    double dn = DV(item.damageType, item.damageLevel, item.density);
                    if (dn > 0)
                    {
                        DV_list.Add(dn);
                    }
                    
                }
                DV_list.Sort();

                if (DV_list.Count > 0)
                {
                    int q = 0;
                    if (DV_list.FindIndex(x => x > 2) > 0)
                    {
                        q = DV_list.Count - DV_list.FindIndex(x => x > 2);
                    }
                    else
                    {
                        q = 1;
                    }
                    double max_CDV = 0;
                    if (q <= 1)
                    {
                        max_CDV = CDV(DV_list.Sum(), q);
                    }
                    else
                    {
                        List<double> cdv = new List<double>();
                        double co = 100 - DV_list[DV_list.Count - 1];
                        double m = (0.0918 * co) + 1;
                        double dec = m - Math.Floor(m);
                        DV_list[0] = DV_list[0] * dec;
                        q = DV_list.Count - DV_list.FindIndex(x => x > 2);
                        cdv.Add(CDV(DV_list.Sum(), q));

                        while (q >= 2)
                        {
                            DV_list[DV_list.FindIndex(x => x > 2)] = 2;
                            q = DV_list.Count - DV_list.FindIndex(x => x > 2);
                            cdv.Add(CDV(DV_list.Sum(), q));                            
                        }
                        max_CDV = cdv.Max();
                    }
                    return 100 - max_CDV;
                }                
            }
            return 100;
        }
        
        private double DV(int type, int level, double value)
        {
            double DV = 0;            
            double[] coef = { };
            switch (type)
            {
                //Alligator cracking
                case 1:
                    switch (level)
                    {
                        case 1:
                            coef = new double[9]
                            {
                                11.57282, 14.47294, 5.323757,
                                1.59328, -0.8304, 0,
                                0, 0.1, 100
                            };
                            break;
                        case 2:
                            coef = new double[9]
                            {
                                21.06287, 21.87252, 5.430085,
                                -2.25521, 0.524358, 0,
                                0, 0.1, 100
                            };
                            break;
                        case 3:
                            coef = new double[9]
                            {
                                30.35494, 29.47183, 5.773053,
                                -5.02079, 1.123057, 0,
                                0, 0.1, 100
                            };
                            break;
                    }
                    break;
                //Bleeding
                case 2:
                    switch (level)
                    {
                        case 1:
                            coef = new double[9]
                            {
                                0.226389, 0.526314, 0.808991,
                                0.98469, 0.47562, 0,
                                0, 0.1, 100
                            };
                            break;
                        case 2:
                            coef = new double[9]
                            {
                                3.103022, 5.033304, 3.347506,
                                0.957058, 0.339835, 0,
                                0, 0.1, 100
                            };
                            break;
                        case 3:
                            coef = new double[9]
                            {
                                5.174671, 6.973435, 7.552022,
                                3.26362, -0.08964, 0,
                                0, 0.1, 100
                            };
                            break;
                    }
                    break;
                //Block cracking
                case 3:
                    switch (level)
                    {
                        case 1:
                            coef = new double[9]
                            {
                                0.653826, 2.442088, 3.661845,
                                1.55629, -0.3173, 0,
                                0, 0.1, 100
                            };
                            break;
                        case 2:
                            coef = new double[9]
                            {
                                2.505961, 6.738529, 5.642363,
                                1.13416, -0.27564, 0,
                                0, 0.1, 100
                            };
                            break;
                        case 3:
                            coef = new double[9]
                            {
                                5.698064, 11.97259, 10.52476,
                                2.565825, -1.28441, 0,
                                0, 0.1, 100
                            };
                            break;
                    }
                    break;
                //Bumps and sags
                case 4:
                    switch (level)
                    {
                        case 1:
                            coef = new double[9]
                            {
                                6.842161, 13.21294, 10.81703,
                                6.6296, 2.78335, 0,
                                0, 0.1, 100
                            };
                            break;
                        case 2:
                            coef = new double[9]
                            {
                                23.6662, 24.87604, 13.0282,
                                11.5001, 6.281742, 0,
                                0, 0.1, 100
                            };
                            break;
                        case 3:
                            coef = new double[9]
                            {
                                52.43768, 36.51803, 5.190109,
                                3.443652, 2.333901, 0,
                                0, 0.1, 100
                            };
                            break;
                    }
                    break;
                //Corrugation
                case 5:
                    switch (level)
                    {
                        case 1:
                            coef = new double[9]
                            {
                                1.719801, 4.148407, 5.883605,
                                2.15801, -0.697, 0,
                                0, 0.1, 100
                            };
                            break;
                        case 2:
                            coef = new double[9]
                            {
                                15.52694, 18.69575, 6.45422,
                                -1.36052, 0.354079, 0,
                                0, 0.1, 100
                            };
                            break;
                        case 3:
                            coef = new double[9]
                            {
                                33.73598, 22.8334, 2.978519,
                                2.716514, -1.16458, 0,
                                0, 0.1, 100
                            };
                            break;
                    }
                    break;
                //Depression
                case 6:
                    switch (level)
                    {
                        case 1:
                            coef = new double[9]
                            {
                                4.576458, 1.56377, 5.720728,
                                7.57356, 0.9556, -1.8667,
                                0.01785, 0.1, 100
                            };
                            break;
                        case 2:
                            coef = new double[9]
                            {
                                9.18211, 3.496649, 11.12484,
                                11.6605, -1.95889, -3.85509,
                                0.954007, 0.1, 100
                            };
                            break;
                        case 3:
                            coef = new double[9]
                            {
                                16.2489, 6.837703, 13.47965,
                                15.15847, -3.98759, -6.20127,
                                2.053938, 0.1, 100
                            };
                            break;
                    }
                    break;
                //Edge cracking
                case 7:
                    switch (level)
                    {
                        case 1:
                            coef = new double[9]
                            {
                                2.678353, 3.009038, 4.088985,
                                2.31193, -1.2464, 0,
                                0, 0.1, 20
                            };
                            break;
                        case 2:
                            coef = new double[9]
                            {
                                8.118318, 9.360312, 7.534917,
                                0.340772, -1.95215, 0,
                                0, 0.1, 20
                            };
                            break;
                        case 3:
                            coef = new double[9]
                            {
                                13.03806, 15.51621, 14.72085,
                                0.336104, -4.50659, 0,
                                0, 0.1, 20
                            };
                            break;
                    }
                    break;
                //Joint reflection cracking
                case 8:
                    switch (level)
                    {
                        case 1:
                            coef = new double[9]
                            {
                                2.368495, 5.585601, 4.304673,
                                1.40666, 0.31909, 0,
                                0, 0.1, 30
                            };
                            break;
                        case 2:
                            coef = new double[9]
                            {
                                6.622707, 14.02556, 14.48692,
                                2.16426, -4.81981, 0,
                                0, 0.1, 30
                            };
                            break;
                        case 3:
                            coef = new double[9]
                            {
                                14.01349, 18.83563, 25.97381,
                                22.76282, -14.5529, -12.5832,
                                5.580756, 0.1, 25
                            };
                            break;
                    }
                    break;
                //Lane/shoudler drop-off
                case 9:
                    switch (level)
                    {
                        case 1:
                            coef = new double[9]
                            {
                                2.603195, 2.571387, 4.029737,
                                2.46853, 0.40699, 0,
                                0, 0.1, 15
                            };
                            break;
                        case 2:
                            coef = new double[9]
                            {
                                4.506029, 1.917193, 4.716884,
                                5.791711, 2.48599, 0,
                                0, 0.1, 15
                            };
                            break;
                        case 3:
                            coef = new double[9]
                            {
                                7.040119, 5.204559, 9.724312,
                                8.009115, 2.449223, 0,
                                0, 0.1, 15
                            };
                            break;
                    }
                    break;
                //Long and trans cracking
                case 10:
                    switch (level)
                    {
                        case 1:
                            coef = new double[9]
                            {
                                2.026879, 6.764295, 7.027913,
                                1.52647, -0.764, 0,
                                0, 0.1, 30
                            };
                            break;
                        case 2:
                            coef = new double[9]
                            {
                                8.428558, 15.6896, 6.70787,
                                -0.447, 0.106175, 0,
                                0, 0.1, 30
                            };
                            break;
                        case 3:
                            coef = new double[9]
                            {
                                18.19322, 22.18564, 14.63774,
                                12.49489, -0.00014, -5.2497,
                                0, 0.1, 30
                            };
                            break;
                    }
                    break;
                //Patching
                case 11:
                    switch (level)
                    {
                        case 1:
                            coef = new double[9]
                            {
                                2.318707, 5.967067, 6.850561,
                                2.08319, -1.119, 0,
                                0, 0.1, 50
                            };
                            break;
                        case 2:
                            coef = new double[9]
                            {
                                9.573035, 12.04862, 7.786538,
                                1.894723, -0.41622, 0,
                                0, 0.1, 50
                            };
                            break;
                        case 3:
                            coef = new double[9]
                            {
                                18.65748, 14.89525, 9.107156,
                                15.73892, -1.06098, -7.76801,
                                2.162507, 0.1, 50
                            };
                            break;
                    }
                    break;
                //Polished aggregate
                case 12:
                    switch (level)
                    {
                        case 1:
                            coef = new double[9]
                            {
                                0, -0.1415721, 3.588791,
                                -1.68193, 1.210908, 0,
                                0, 1, 100
                            };
                            break;
                        case 2:
                            coef = new double[9]
                            {
                                0, -0.1415721, 3.588791,
                                -1.68193, 1.210908, 0,
                                0, 1, 100
                            };
                            break;
                        case 3:
                            coef = new double[9]
                            {
                                0, -0.1415721, 3.588791,
                                -1.68193, 1.210908, 0,
                                0, 1, 100
                            };
                            break;
                    }
                    break;
                //Potholes
                case 13:
                    switch (level)
                    {
                        case 1:
                            coef = new double[9]
                            {
                                58.0456, 41.97506, 2.918861,
                                -2.7756, -0.3735, 0,
                                0, 0.01, 10
                            };
                            break;
                        case 2:
                            coef = new double[9]
                            {
                                89.72291, 61.35048, -0.8974963,
                                -7.80229, -1.24064, 0,
                                0, 0.01, 1.5
                            };
                            break;
                        case 3:
                            coef = new double[9]
                            {
                                108.9686, 58.37936, 0.97282088,
                                -3.59034, -0.26399, 0,
                                0, 0.01, 0.69999999
                            };
                            break;
                    }
                    break;
                //Railroad crossing
                case 14:
                    switch (level)
                    {
                        case 1:
                            coef = new double[9]
                            {
                                2, 5.51293, -29.36848,
                                76.942, -55.301, 12.4976,
                                0, 1, 50
                            };
                            break;
                        case 2:
                            coef = new double[9]
                            {
                                6, 0.0940975, 74.13531,
                                -52.6759, 10.54689, 0,
                                0, 1, 50
                            };
                            break;
                        case 3:
                            coef = new double[9]
                            {
                                20, 32.91058, -33.17019,
                                138.5578, -122.88, 31.40176,
                                0, 1, 50
                            };
                            break;
                    }
                    break;
                //Rutting
                case 15:
                    switch (level)
                    {
                        case 1:
                            coef = new double[9]
                            {
                                7.864241, 13.94023, 7.431795,
                                -0.3771, -0.733, 0,
                                0, 0.1, 100
                            };
                            break;
                        case 2:
                            coef = new double[9]
                            {
                                17.9108, 20.09714, 6.764661,
                                0.335158, -0.60516, -0.362,
                                0, 0.1, 100
                            };
                            break;
                        case 3:
                            coef = new double[9]
                            {
                                27.35017, 24.50075, 5.838376,
                                3.139074, -0.15555, -1.00682,
                                0, 0.1, 100
                            };
                            break;
                    }
                    break;
                //Shoving
                case 16:
                    switch (level)
                    {
                        case 1:
                            coef = new double[9]
                            {
                                3.968421, 9.926723, 7.064671,
                                0.31435, -0.792, 0,
                                0, 0.1, 50
                            };
                            break;
                        case 2:
                            coef = new double[9]
                            {
                                9.312978, 14.62188, 11.48379,
                                1.394249, -1.18064, 0,
                                0, 0.1, 50
                            };
                            break;
                        case 3:
                            coef = new double[9]
                            {
                                18.97428, 15.49013, 12.02632,
                                13.36067, -3.0542, -6.69051,
                                2.213889, 0.1, 50
                            };
                            break;
                    }
                    break;
                //Slippage cracking
                case 17:
                    switch (level)
                    {
                        case 1:
                            coef = new double[9]
                            {
                                4.348589, 11.43505, 14.15354,
                                0.39453, -4.8667, 1.40584,
                                0, 0.1, 100
                            };
                            break;
                        case 2:
                            coef = new double[9]
                            {
                                10.77733, 20.24104, 18.4704,
                                -1.7614, -6.43406, 1.934033,
                                0, 0.1, 100
                            };
                            break;
                        case 3:
                            coef = new double[9]
                            {
                                18.90548, 30.13452, 25.77373,
                                2.893255, -11.3648, -1.77798,
                                1.935406, 0.1, 100
                            };
                            break;
                    }
                    break;
                //Swell
                case 18:
                    switch (level)
                    {
                        case 1:
                            coef = new double[9]
                            {
                                2, 5.846334, 4.610276,
                                -0.974, 0.52452, 0,
                                0, 0.1, 30
                            };
                            break;
                        case 2:
                            coef = new double[9]
                            {
                                12, 21.45781, -9.318042,
                                14.77898, -4.38961, 0,
                                0, 0.1, 30
                            };
                            break;
                        case 3:
                            coef = new double[9]
                            {
                                34, 6.308562, 9.44694,
                                4.614884, -1.85515, 0,
                                1.935406, 0.1, 30
                            };
                            break;
                    }
                    break;
                //Weather and raveling
                case 19:
                    switch (level)
                    {
                        case 1:
                            coef = new double[9]
                            {
                                1.518308, 1.463035, 1.225113,
                                1.18395, -0.0964, 0,
                                0, 0.1, 100
                            };
                            break;
                        case 2:
                            coef = new double[9]
                            {
                                8.216442, 4.187497, 3.774271,
                                3.050996, -0.75222, 0,
                                0, 0.1, 100
                            };
                            break;
                        case 3:
                            coef = new double[9]
                            {
                                15.03442, 13.0601, 12.93693,
                                4.599652, -3.3116, 0,
                                0, 0.1, 100
                            };
                            break;
                    }
                    break;
            }
            if (value == 0)
            {
                return -1;
            }
            else if (value < coef[7])
            {
                return -1;
            }
            else if (value > coef[8])
            {
                return -1;
            }

            double t1 = coef[0] * Math.Pow(Math.Log10(value), 0);
            double t2 = coef[1] * Math.Pow(Math.Log10(value), 1);
            double t3 = coef[2] * Math.Pow(Math.Log10(value), 2);
            double t4 = coef[3] * Math.Pow(Math.Log10(value), 3);
            double t5 = coef[4] * Math.Pow(Math.Log10(value), 4);
            double t6 = coef[5] * Math.Pow(Math.Log10(value), 5);
            double t7 = coef[6] * Math.Pow(Math.Log10(value), 6);
            DV = t1 + t2 + t3 + t4 + t5 + t6 + t7;
            return DV;
        }
        
        private double CDV(double value, int q)
        {
            double CDV = 0;
            double[] x = {
                1, value, Math.Pow(value, 2),
                Math.Pow(value, 3), Math.Pow(value, 4)
            };
            double[] coef = { };
            switch (q)
            {
                case 1:
                    coef = new double[5]
                    {
                        0, 1, 0,
                        0, 0
                    };
                    break;
                case 2:
                    coef = new double[5]
                    {
                        -0.2154, 0.7615, -0.000245,
                        -0.000002528, -0.00000001374
                    };
                    break;
                case 3:
                    coef = new double[5]
                    {
                        -0.9875, 0.5942, 0.001899,
                        -0.00001706, 0.00000002721
                    };
                    break;
                case 4:
                    coef = new double[5]
                    {
                        -1.124, 0.3481, 0.005446,
                        -0.00003763, 0.00000007052
                    };
                    break;
                case 5:
                    coef = new double[5]
                    {
                        -1.156, 0.3167, 0.00457,
                        -0.0000291, 0.00000005093
                    };
                    break;
                case 6:
                    coef = new double[5]
                    {
                        -1.7, 0.2627, 0.004811,
                        -0.00002828, 0.00000004436
                    };
                    break;
                default : //for q >= 7
                    coef = new double[5]
                    {
                        -1.448, 0.2112, 0.006489,
                        -0.00004419, 0.00000008446
                    };
                    break;
            }
            for (int i = 0; i < 5; i++)
            {
                CDV += coef[i] * x[i];
            }
            if (CDV > 100)
            {
                CDV = 100;
            }
            return CDV;
        }

        public class Statistic
        {
            public int damageLevel { get; set; }
            public int damageType { get; set; }
            public double value { get; set; }
            public double density { get; set; }
        }

        //function source https://jason-chen-1992.weebly.com/home/-perspective-distortion-correction               
        public Point transform(Point point)
        {
            //transform_matrix(origin_points);
            double new_X = coe[0] * point.X + coe[1] * point.Y + coe[2] * point.X * point.Y + coe[3];
            double new_Y = coe[4] * point.X + coe[5] * point.Y + coe[6] * point.X * point.Y + coe[7];
            return new Point() { X = new_X, Y = new_Y };
        }


        #region calculate matrix
        public void transform_matrix()
        {
            double[,] A = new double[8, 8];
            for (int i = 0; i < 4; i++)
            {
                A[i, 0] = origin_points[i].X;
                A[i, 1] = origin_points[i].Y;
                A[i, 2] = origin_points[i].X * origin_points[i].Y;
                A[i, 3] = 1;
                A[i, 4] = 0;
                A[i, 5] = 0;
                A[i, 6] = 0;
                A[i, 7] = 0;
            }
            for (int i = 4; i < 8; i++)
            {
                A[i, 0] = 0;
                A[i, 1] = 0;
                A[i, 2] = 0;
                A[i, 3] = 0;
                A[i, 4] = origin_points[i - 4].X;
                A[i, 5] = origin_points[i - 4].Y;
                A[i, 6] = origin_points[i - 4].X * origin_points[i - 4].Y;
                A[i, 7] = 1;
            }
            double[,] a = Inverse(A);
            for (int i = 0; i < 8; i++)
            {
                coe[i] = a[i, 0] * after_points[0].X +
                    a[i, 1] * after_points[1].X +
                    a[i, 2] * after_points[2].X +
                    a[i, 3] * after_points[3].X +
                    a[i, 4] * after_points[0].Y +
                    a[i, 5] * after_points[1].Y +
                    a[i, 6] * after_points[2].Y +
                    a[i, 7] * after_points[3].Y;
            }
        }

        public static double[,] Inverse(double[,] Array)
        {
            int m = 0;
            int n = 0;
            m = Array.GetLength(0);
            n = Array.GetLength(1);
            double[,] array = new double[2 * m + 1, 2 * n + 1];
            for (int k = 0; k < 2 * m + 1; k++)  //初始化數組
            {
                for (int t = 0; t < 2 * n + 1; t++)
                {
                    array[k, t] = 0.00000000;
                }
            }
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    array[i, j] = Array[i, j];
                }
            }

            for (int k = 0; k < m; k++)
            {
                for (int t = n; t <= 2 * n; t++)
                {
                    if ((t - k) == m)
                    {
                        array[k, t] = 1.0;
                    }
                    else
                    {
                        array[k, t] = 0;
                    }
                }
            }
            //得到逆矩陣
            for (int k = 0; k < m; k++)
            {
                if (array[k, k] != 1)
                {
                    double bs = array[k, k];
                    array[k, k] = 1;
                    for (int p = k + 1; p < 2 * n; p++)
                    {
                        array[k, p] /= bs;
                    }
                }
                for (int q = 0; q < m; q++)
                {
                    if (q != k)
                    {
                        double bs = array[q, k];
                        for (int p = 0; p < 2 * n; p++)
                        {
                            array[q, p] -= bs * array[k, p];
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            double[,] NI = new double[m, n];
            for (int x = 0; x < m; x++)
            {
                for (int y = n; y < 2 * n; y++)
                {
                    NI[x, y - n] = array[x, y];
                }
            }
            return NI;
        }
        #endregion
    }
}
