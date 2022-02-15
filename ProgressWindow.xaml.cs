using System.Collections.Generic;
using Window = System.Windows.Window;
using Color = System.Drawing.Color;
using System.IO;
using System;
using System.Linq;
using Action = System.Action;
using System.Threading.Tasks;
using System.Windows;
using optanaPCI.Data;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using OfficeOpenXml.Drawing.Chart;

namespace optanaPCI
{
    public partial class ProgressWindow : Window
    {
        string path = "", excelName = "";
        List<IRIData> iRIDatas = new List<IRIData>();
        List<ccdData> ccdDatas = new List<ccdData>();
        basicData basicdata = new basicData();
        double bar_step = 0;
        List<string> record = new List<string>();
        
        public ProgressWindow(string path, string excelName, List<ccdData> ccdDatas, List<IRIData> iRIDatas, basicData basicdata)
        {            
            InitializeComponent();
            this.path = path;
            this.iRIDatas = iRIDatas;
            this.ccdDatas = ccdDatas;
            this.basicdata = basicdata;
            this.excelName = excelName;
        }

        public async void Exporting()
        {
            bar_step = iRIDatas.Count / 100;
            int imgSize = 95; //pixel                
            int cellHeight = 84;
            Action<double> bindProgress = value => bar.Value = value;

            IProgress<double> progress = new Progress<double>(bindProgress);

            Action growProgress =
                () =>
                {
                    //fondation setting
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // 關閉新許可模式通知
                    //save and release resource
                    string savepath = path + "\\" + excelName + ".xlsx";
                    var file = new FileInfo(savepath);
                    using (var excel = new ExcelPackage())
                    {
                        #region setting
                        string fileName = "";
                        int start = 4; //cells row number
                        int iri_start = 0; //cells row number
                        int ccd_start = 0; //ccd data row number
                        int damage_start = 0; //damage row number
                        int damage_count = 0; //count damage number
                        List<double> pci = new List<double>();
                        List<conditionData> total_condition = new List<conditionData>();
                        List<conditionData> img_condition = new List<conditionData>();
                        #endregion

                        #region datasheet
                        #region basic setting
                        var data_ws = excel.Workbook.Worksheets.Add("資料");

                        data_ws.Cells[1, 1].Value = "日期";
                        data_ws.Cells[2, 1].Value = basicdata.Date;
                        data_ws.Cells[1, 2].Value = "省鄉道/市區道路名稱";
                        data_ws.Cells[2, 2].Value = basicdata.Name;
                        data_ws.Cells[1, 3].Value = "起始點、終止點";
                        data_ws.Cells[2, 3].Value = basicdata.start_end;
                        data_ws.Cells[1, 4].Value = "車道";
                        data_ws.Cells[2, 4].Value = basicdata.Lane;
                        data_ws.Cells[1, 5].Value = "車行方向";
                        data_ws.Cells[2, 5].Value = basicdata.Direction;
                        data_ws.Cells[1, 1, 1, 13].Style.Fill.PatternType = ExcelFillStyle.Solid; //一定要加這行..不然會報錯
                        data_ws.Cells[1, 1, 1, 13].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(221, 217, 196));

                        data_ws.Cells[3, 1].Value = "里程(公里)";
                        data_ws.Cells[3, 2].Value = "IRI平均";
                        data_ws.Cells[3, 3].Value = "PCI";
                        data_ws.Cells[3, 4].Value = "圖片名";
                        data_ws.Cells[3, 5].Value = "原始圖片";
                        data_ws.Cells[3, 6].Value = "點圖";
                        data_ws.Cells[3, 7].Value = "經緯度";
                        data_ws.Cells[3, 8].Value = "樁號";
                        data_ws.Cells[3, 9].Value = "現況紀錄";
                        data_ws.Cells[3, 10].Value = "破壞類型";
                        data_ws.Cells[3, 11].Value = "破壞程度";
                        data_ws.Cells[3, 12].Value = "破壞長度/面積";
                        data_ws.Cells[3, 13].Value = "門牌";
                        data_ws.Cells[3, 1, 3, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        data_ws.Cells[3, 1, 3, 13].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(238, 236, 225));
                        data_ws.Cells[1, 1, 3, 4].AutoFitColumns();
                        data_ws.Column(5).Width = 17;
                        data_ws.Column(6).Width = 17;
                        progress.Report(1);
                        #endregion

                        #region export data

                        for (int i = 0; i < iRIDatas.Count; i++)
                        {
                            iri_start = start;
                            //adding IRI
                            data_ws.Cells[iri_start, 1].Value = iRIDatas[i].Name;
                            data_ws.Cells[iri_start, 2].Value = iRIDatas[i].IRI_average;

                            List<ccdData> listOfMember = new List<ccdData>();
                            if (i == iRIDatas.Count - 1)
                            {
                                listOfMember = ccdDatas.FindAll(x => x.Stake >= iRIDatas[i].Stake_start && x.Stake <= iRIDatas[i].Stake_end);
                            }                                
                            else
                            {
                                listOfMember = ccdDatas.FindAll(x => x.Stake >= iRIDatas[i].Stake_start && x.Stake < iRIDatas[i].Stake_end);                            
                            }
                            foreach (ccdData ccd in listOfMember)
                            {
                                ccd_start = start;
                                fileName = ccd.Name;
                                //add IRI info
                                data_ws.Cells[ccd_start, 4].Value = fileName;
                                data_ws.Cells[ccd_start, 7].Value = ccd.Longtitude + "-" + ccd.Latitude;
                                data_ws.Cells[ccd_start, 8].Value = ccd.Stake;
                                damage_start = start;

                                //having damage data or not
                                if (File.Exists(path + "/temp/" + fileName + ".txt"))
                                {
                                    using (StreamReader input = new StreamReader(path + "/temp/" + fileName + ".txt"))
                                    {
                                        string str;
                                        while ((str = input.ReadLine()) != null)
                                        {
                                            string[] ss = str.Split(' ');
                                            if (ss[0] == "[") //pci
                                            {
                                                pci.Add(Convert.ToDouble(ss[1]));
                                            }
                                            else if (ss[0] == "*") //condition
                                            {
                                                int total_id = total_condition.FindIndex(x => x.Category.Contains(ss[1]));
                                                if (total_id != -1)
                                                {
                                                    total_condition[total_id].Number += Convert.ToInt32(ss[2]);
                                                }
                                                else
                                                {
                                                    total_condition.Add(new conditionData()
                                                    {
                                                        Category = ss[1],
                                                        Number = Convert.ToInt32(ss[2])
                                                    });
                                                }
                                                int img_id = img_condition.FindIndex(x => x.Category.Contains(ss[1]));
                                                if (img_id != -1)
                                                {
                                                    img_condition[img_id].Number += Convert.ToInt32(ss[2]);
                                                }
                                                else
                                                {
                                                    img_condition.Add(new conditionData()
                                                    {
                                                        Category = ss[1],
                                                        Number = Convert.ToInt32(ss[2])
                                                    });
                                                }
                                            }
                                            else if (ss[0] != "(") //damage
                                            {
                                                double lenOrArea = Convert.ToDouble(ss[3]) + Convert.ToDouble(ss[4]);
                                                data_ws.Cells[start, 10].Value = ss[0];
                                                data_ws.Cells[start, 11].Value = ss[1];
                                                data_ws.Cells[start, 12].Value = lenOrArea;
                                                start++;
                                            }
                                        }
                                    }

                                    //merging cells
                                    if (start > ccd_start)
                                    {
                                        for (int j = ccd_start; j < start; j++)
                                        {
                                            data_ws.Row(j).Height = cellHeight / (start - ccd_start);
                                        }
                                        data_ws.Cells[ccd_start, 4, (start - 1), 4].Merge = true;
                                        data_ws.Cells[ccd_start, 5, (start - 1), 5].Merge = true;
                                        data_ws.Cells[ccd_start, 6, (start - 1), 6].Merge = true;
                                        data_ws.Cells[ccd_start, 7, (start - 1), 7].Merge = true;
                                        data_ws.Cells[ccd_start, 8, (start - 1), 8].Merge = true;
                                        data_ws.Cells[ccd_start, 9, (start - 1), 9].Merge = true;
                                    }
                                    else
                                    {
                                        data_ws.Row(ccd_start).Height = cellHeight;
                                    }

                                    //add condition
                                    if (img_condition.Count != 0)
                                    {
                                        string condition = "";
                                        foreach (conditionData item in img_condition)
                                        {
                                            condition += item.Category + "*" + item.Number + "\n";
                                        }
                                        data_ws.Cells[damage_start, 9].Value = condition;
                                        img_condition.Clear();
                                    }

                                    if (File.Exists(path + @"\temp\" + fileName))
                                    {
                                        Image img = Image.FromFile(path + @"\temp\" + fileName);
                                        var after_img = data_ws.Drawings.AddPicture(path + @"\temp\" + fileName, img);
                                        after_img.SetSize(imgSize, imgSize);
                                        after_img.SetPosition(damage_start - 1, 0, 5, 0);
                                        damage_count = start - damage_start;
                                    }

                                    if (damage_count == 0)
                                    {
                                        start++;
                                    }
                                }
                                else
                                {
                                    data_ws.Row(start).Height = cellHeight;
                                    pci.Add(100);
                                    start++;
                                }

                                //add origin photos
                                if (File.Exists(path + @"\CCD\" + fileName))
                                {
                                    Image img = Image.FromFile(path + @"\CCD\" + fileName);
                                    var origin_img = data_ws.Drawings.AddPicture(path + @"\CCD\" + fileName, img);
                                    origin_img.SetSize(imgSize, imgSize);
                                    origin_img.SetPosition(damage_start - 1, 0, 4, 0);
                                }
                            }                           

                            if (pci.Count != 0)
                            {
                                data_ws.Cells[iri_start, 3].Value = pci.Sum() / pci.Count;
                                pci.Clear();
                            }

                            if (listOfMember.Count > 1)
                            {
                                data_ws.Cells[iri_start, 3, (start - 1), 3].Merge = true;
                                data_ws.Cells[iri_start, 2, (start - 1), 2].Merge = true;
                                data_ws.Cells[iri_start, 1, (start - 1), 1].Merge = true;
                            }

                            if (i > 1 && i < 98)
                            {
                                progress.Report(i);
                            }                            
                        }
                        #endregion
                        data_ws.Cells[1, 1, start, 13].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        data_ws.Cells[4, 7, start, 13].AutoFitColumns();
                        data_ws.Cells.Style.WrapText = true;
                        data_ws.Cells.Style.Font.Name = "微軟正黑體";
                        data_ws.Cells.Style.Font.Size = 12;
                        #endregion

                        #region IRIPCI worksheet
                        var IRIPCI_ws = excel.Workbook.Worksheets.Add("IRI、PCI統計");
                        IRIPCI_ws.Cells[1, 1].Value = "IRI統計";
                        IRIPCI_ws.Cells["A1:A2"].Merge = true;
                        IRIPCI_ws.Cells[1, 2].Value = "0~1";
                        IRIPCI_ws.Cells[2, 2].Formula = "COUNTIF(資料!B4:B" + start.ToString() + ",\"<=1\")-COUNTIF(資料!B4:B" + start.ToString() + ",\"<=0\")";
                        IRIPCI_ws.Cells[1, 3].Value = "1~2";
                        IRIPCI_ws.Cells[2, 3].Formula = "COUNTIF(資料!B4:B" + start.ToString() + ",\"<=2\")-COUNTIF(資料!B4:B" + start.ToString() + ",\"<=1\")";
                        IRIPCI_ws.Cells[1, 4].Value = "2~3";
                        IRIPCI_ws.Cells[2, 4].Formula = "COUNTIF(資料!B4:B" + start.ToString() + ",\"<=3\")-COUNTIF(資料!B4:B" + start.ToString() + ",\"<=2\")";
                        IRIPCI_ws.Cells[1, 5].Value = "3~4";
                        IRIPCI_ws.Cells[2, 5].Formula = "COUNTIF(資料!B4:B" + start.ToString() + ",\"<=4\")-COUNTIF(資料!B4:B" + start.ToString() + ",\"<=3\")";
                        IRIPCI_ws.Cells[1, 6].Value = "4~5";
                        IRIPCI_ws.Cells[2, 6].Formula = "COUNTIF(資料!B4:B" + start.ToString() + ",\"<=5\")-COUNTIF(資料!B4:B" + start.ToString() + ",\"<=4\")";
                        IRIPCI_ws.Cells[1, 7].Value = "5~6";
                        IRIPCI_ws.Cells[2, 7].Formula = "COUNTIF(資料!B4:B" + start.ToString() + ",\"<=6\")-COUNTIF(資料!B4:B" + start.ToString() + ",\"<=5\")";
                        IRIPCI_ws.Cells[1, 8].Value = "6~7";
                        IRIPCI_ws.Cells[2, 8].Formula = "COUNTIF(資料!B4:B" + start.ToString() + ",\"<=7\")-COUNTIF(資料!B4:B" + start.ToString() + ",\"<=6\")";
                        IRIPCI_ws.Cells[1, 9].Value = "7~8";
                        IRIPCI_ws.Cells[2, 9].Formula = "COUNTIF(資料!B4:B" + start.ToString() + ",\"<=8\")-COUNTIF(資料!B4:B" + start.ToString() + ",\"<=7\")";
                        IRIPCI_ws.Cells[1, 10].Value = "8以上";
                        IRIPCI_ws.Cells[2, 10].Formula = "COUNTIF(資料!B4:B" + start.ToString() + ",\">=8\")";
                        IRIPCI_ws.Cells[1, 11].Value = "平均";
                        IRIPCI_ws.Cells[2, 11].Formula = "AVERAGE(資料!B4:B" + start.ToString() + ")";
                        IRIPCI_ws.Cells["A1:K2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        IRIPCI_ws.Cells["A1:K2"].AutoFitColumns();

                        var bar = (ExcelBarChart)IRIPCI_ws.Drawings.AddChart("BarChart", eChartType.ColumnClustered);
                        bar.Title.Text = "IRI統計";
                        bar.Legend.Remove();
                        bar.Series.Add(ExcelRange.GetAddress(2, 2, 2, 10), ExcelRange.GetAddress(1, 2, 1, 10));
                        bar.SetSize(450, 300);
                        bar.SetPosition(110, 10);
                        bar = null;

                        IRIPCI_ws.Cells[4, 1].Value = "PCI統計";
                        IRIPCI_ws.Cells["A4:A5"].Merge = true;
                        IRIPCI_ws.Cells[4, 2].Value = "100~90";
                        IRIPCI_ws.Cells[5, 2].Formula = "COUNTIF(資料!C4:C" + start.ToString() + ",\"<=100\")-COUNTIF(資料!C4:C" + start.ToString() + ",\"<=90\")";
                        IRIPCI_ws.Cells[4, 3].Value = "90~80";
                        IRIPCI_ws.Cells[5, 3].Formula = "COUNTIF(資料!C4:C" + start.ToString() + ",\"<=90\")-COUNTIF(資料!C4:C" + start.ToString() + ",\"<=80\")";
                        IRIPCI_ws.Cells[4, 4].Value = "80~70";
                        IRIPCI_ws.Cells[5, 4].Formula = "COUNTIF(資料!C4:C" + start.ToString() + ",\"<=80\")-COUNTIF(資料!C4:C" + start.ToString() + ",\"<=70\")";
                        IRIPCI_ws.Cells[4, 5].Value = "70~60";
                        IRIPCI_ws.Cells[5, 5].Formula = "COUNTIF(資料!C4:C" + start.ToString() + ",\"<=70\")-COUNTIF(資料!C4:C" + start.ToString() + ",\"<=60\")";
                        IRIPCI_ws.Cells[4, 6].Value = "60~50";
                        IRIPCI_ws.Cells[5, 6].Formula = "COUNTIF(資料!C4:C" + start.ToString() + ",\"<=60\")-COUNTIF(資料!C4:C" + start.ToString() + ",\"<=50\")";
                        IRIPCI_ws.Cells[4, 7].Value = "50~40";
                        IRIPCI_ws.Cells[5, 7].Formula = "COUNTIF(資料!C4:C" + start.ToString() + ",\"<=50\")-COUNTIF(資料!C4:C" + start.ToString() + ",\"<=40\")";
                        IRIPCI_ws.Cells[4, 8].Value = "40~30";
                        IRIPCI_ws.Cells[5, 8].Formula = "COUNTIF(資料!C4:C" + start.ToString() + ",\"<=40\")-COUNTIF(資料!C4:C" + start.ToString() + ",\"<=30\")";
                        IRIPCI_ws.Cells[4, 9].Value = "30~20";
                        IRIPCI_ws.Cells[5, 9].Formula = "COUNTIF(資料!C4:C" + start.ToString() + ",\"<=30\")-COUNTIF(資料!C4:C" + start.ToString() + ",\"<=20\")";
                        IRIPCI_ws.Cells[4, 10].Value = "20以下";
                        IRIPCI_ws.Cells[5, 10].Formula = "COUNTIF(資料!C4:C" + start.ToString() + ",\"<=20\")";
                        IRIPCI_ws.Cells[4, 11].Value = "平均";
                        IRIPCI_ws.Cells[5, 11].Formula = "AVERAGE(資料!C4:C" + start.ToString() + ")";
                        IRIPCI_ws.Cells["A4:K5"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        IRIPCI_ws.Cells["A4:K5"].AutoFitColumns();

                        var bar2 = (ExcelBarChart)IRIPCI_ws.Drawings.AddChart("BarChart2", eChartType.ColumnClustered);
                        bar2.Title.Text = "PCI統計";
                        bar2.Legend.Remove();
                        bar2.Series.Add(ExcelRange.GetAddress(5, 2, 5, 10), ExcelRange.GetAddress(4, 2, 4, 10));
                        bar2.SetSize(450, 300);
                        bar2.SetPosition(110, 470);
                        bar2 = null;

                        IRIPCI_ws.Cells.Style.Font.Name = "微軟正黑體";
                        IRIPCI_ws.Cells.Style.Font.Size = 12;
                        progress.Report(98);
                        #endregion

                        #region type worksheet
                        var type_ws = excel.Workbook.Worksheets.Add("破壞、現況統計");
                        type_ws.Cells[1, 1].Value = "破壞統計";
                        type_ws.Column(1).Width = 10;
                        type_ws.Cells["A1:A2"].Merge = true;
                        type_ws.Cells[1, 2].Value = "1.鱷魚狀裂縫";
                        type_ws.Cells[2, 2].Formula = "COUNTIFS(資料!J4:J" + start.ToString() + ", B1)";
                        type_ws.Cells[1, 3].Value = "3.塊狀裂縫";
                        type_ws.Cells[2, 3].Formula = "COUNTIFS(資料!J4:J" + start.ToString() + ", C1)";
                        type_ws.Cells[1, 4].Value = "7.邊緣裂縫";
                        type_ws.Cells[2, 4].Formula = "COUNTIFS(資料!J4:J" + start.ToString() + ", D1)";
                        type_ws.Cells[1, 5].Value = "8.反射裂縫";
                        type_ws.Cells[2, 5].Formula = "COUNTIFS(資料!J4:J" + start.ToString() + ", E1)";
                        type_ws.Cells[1, 6].Value = "10.縱橫向裂縫";
                        type_ws.Cells[2, 6].Formula = "COUNTIFS(資料!J4:J" + start.ToString() + ", F1)";
                        type_ws.Cells[1, 7].Value = "11.補錠";
                        type_ws.Cells[2, 7].Formula = "COUNTIFS(資料!J4:J" + start.ToString() + ", G1)";
                        type_ws.Cells[1, 8].Value = "12.粒料光滑";
                        type_ws.Cells[2, 8].Formula = "COUNTIFS(資料!J4:J" + start.ToString() + ", H1)";
                        type_ws.Cells[1, 9].Value = "13.坑洞";
                        type_ws.Cells[2, 9].Formula = "COUNTIFS(資料!J4:J" + start.ToString() + ", I1)";
                        type_ws.Cells[1, 10].Value = "16.推擠";
                        type_ws.Cells[2, 10].Formula = "COUNTIFS(資料!J4:J" + start.ToString() + ", J1)";
                        type_ws.Cells[1, 11].Value = "17.滑動裂縫";
                        type_ws.Cells[2, 11].Formula = "COUNTIFS(資料!J4:J" + start.ToString() + ", K1)";
                        type_ws.Cells[1, 12].Value = "19.剝脫";
                        type_ws.Cells[2, 12].Formula = "COUNTIFS(資料!J4:J" + start.ToString() + ", L1)";
                        type_ws.Cells["A1:L2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;                     

                        if (total_condition.Count > 0)
                        {
                            type_ws.Cells[4, 1].Value = "現況統計";
                            type_ws.Cells["A4:A5"].Merge = true;
                            for (int i = 0; i < total_condition.Count; i++)
                            {
                                type_ws.Cells[4, i + 2].Value = total_condition[i].Category;
                                type_ws.Cells[5, i + 2].Value = total_condition[i].Number;
                            }
                            type_ws.Cells[4, 1, 5, total_condition.Count + 2 - 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            type_ws.Cells[4, 1, 5, total_condition.Count + 2 - 1].AutoFitColumns();

                            ExcelPieChart pie2 = type_ws.Drawings.AddChart("PieChart2", eChartType.Pie) as ExcelPieChart;
                            pie2.Title.Text = "現況統計";
                            pie2.Series.Add(ExcelRange.GetAddress(5, 2, 5, total_condition.Count + 2 - 1), ExcelRange.GetAddress(4, 2, 4, total_condition.Count + 2 - 1));
                            pie2.SetSize(450, 300);
                            pie2.SetPosition(110, 470);
                        }

                        ExcelPieChart pie = type_ws.Drawings.AddChart("PieChart", eChartType.Pie) as ExcelPieChart;
                        pie.Title.Text = "破壞統計";
                        pie.Series.Add(ExcelRange.GetAddress(2, 2, 2, 12), ExcelRange.GetAddress(1, 2, 1, 12));
                        pie.SetSize(450, 300);
                        pie.SetPosition(110, 10);
                        pie = null;

                        type_ws.Column(1).Width = 11;
                        type_ws.Cells["B1:L5"].AutoFitColumns();
                        type_ws.Cells.Style.Font.Name = "微軟正黑體";
                        type_ws.Cells.Style.Font.Size = 12;
                        progress.Report(99);
                        #endregion
                        
                        excel.SaveAs(file);
                        progress.Report(100);
                    }                    
                    MessageBox.Show("匯出成功");
                };
            await Task.Run(growProgress);
            this.Close();
        }
    }
}
