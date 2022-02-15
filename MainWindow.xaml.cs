using System;
using System.Windows;
using optanaPCI.Function;
using System.Windows.Input;
using System.Windows.Controls;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using optanaPCI.Data;

namespace optanaPCI
{
    public partial class MainWindow : Window
    {
        private static void MyHandler(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.ToString());
        }

        #region parameter setting        
        string path = "";
        string folder_name = "";
        bool IsCity = true;
        int oldfilenum = -1;
        List<IRIData> iRiDataList = new List<IRIData>();
        List<ccdData> ccdDataList = new List<ccdData>();
        basicData basicdata = new basicData();
        
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region file menu event       
        private void readFolder_Click(object sender, RoutedEventArgs e)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);

            #region resetting
            ccdDataList.Clear();
            iRiDataList.Clear();
            basicdata = new basicData();
            fileName_Combo.Items.Clear();
            fileName_Combo.SelectedIndex = -1;
            path = "";
            oldfilenum = -1;
            #endregion

            //opne choose file window
            getData getdata = new getData();
            path = getdata.SelectedPath();
            
            string line;
            string[] str = path.Split('\\');
            folder_name = str[str.Length - 1];
            
            int count = 0;

            #region check files and folders exist            
            if (!Directory.Exists(path + "/CCD/"))
            {
                MessageBox.Show("沒有CCD資料夾");
                return;
            }
            if (!File.Exists(path + "/CCD/ccd.txt"))
            {
                MessageBox.Show("沒有ccd.txt檔");
                return;
            }            
            if (!File.Exists(path + "/IRI.txt"))
            {
                MessageBox.Show("沒有IRI.txt檔");
                return;
            }
            #endregion

            //IRI.txt
            try
            {
                using (StreamReader IRI = new StreamReader((path + "/IRI.txt"), System.Text.Encoding.Default))
                {
                    line = IRI.ReadLine(); //[日期]
                    line = IRI.ReadLine();
                    basicdata.Date = line;
                    line = IRI.ReadLine(); //[單位] or [縣市]
                    if (line == "[縣市]")
                    {
                        IsCity = true;
                        roadType_Combo.SelectedIndex = 0;
                    }
                    else
                    {
                        IsCity = false;
                        roadType_Combo.SelectedIndex = 1;
                    }
                    count = 3;
                    while ((line = IRI.ReadLine()) != null && !String.Equals(line, "里程\tIRI左\tIRI右\tIRI平均\t經度\t緯度\t車速\t"))
                    {
                        if (IsCity)
                        {
                            direction_box.SelectedIndex = 0;
                            switch (count)
                            {
                                case 3:
                                    city_TextBox.Text = line;
                                    break;
                                case 5:
                                    cityRoad_TextBox.Text = line;
                                    basicdata.Name = line;
                                    break;
                                case 7:
                                    cityStart_TextBox.Text = line;
                                    basicdata.start_end = line;
                                    break;
                                case 9:
                                    cityEnd_TextBox.Text = line;
                                    basicdata.start_end += "、" + line;
                                    break;
                                case 11:
                                    lane_box.SelectedIndex = Convert.ToInt32(line) - 1;
                                    basicdata.Lane = Convert.ToInt32(line);
                                    break;
                            }
                        }
                        else
                        {
                            switch (count)
                            {
                                case 5:
                                    country_TextBox.Text = line;
                                    basicdata.Name = line;
                                    break;
                                case 7:
                                    basicdata.Direction = line;
                                    if (line == "順樁")
                                    {
                                        direction_box.SelectedIndex = 1;
                                    }
                                    else
                                    {
                                        direction_box.SelectedIndex = 2;
                                    }
                                    break;
                                case 9:
                                    countryStart_TextBox.Text = line;
                                    basicdata.start_end = line;
                                    break;
                                case 11:
                                    countryEnd_TextBox.Text = line;
                                    basicdata.start_end += "、" + line;
                                    break;
                                case 13:
                                    lane_box.SelectedIndex = Convert.ToInt32(line) - 1;
                                    basicdata.Lane = Convert.ToInt32(line);
                                    break;
                            }
                        }
                        count++;
                    }
                    while ((line = IRI.ReadLine()) != null && line != "")
                    {
                        str = line.Split();
                        string[] stake_str = str[0].Split(new char[3] { 'k', '_', '-' });
                        double stake_start = 0, stake_end = 0;
                        if (direction_box.SelectedIndex != 2)
                        {
                            stake_start = Convert.ToDouble(stake_str[0]) + (Convert.ToDouble(stake_str[2]) * 0.001);
                            stake_end = Convert.ToDouble(stake_str[3]) + (Convert.ToDouble(stake_str[5]) * 0.001);
                        }
                        else
                        {
                            stake_start = Convert.ToDouble(stake_str[3]) + (Convert.ToDouble(stake_str[5]) * 0.001);
                            stake_end = Convert.ToDouble(stake_str[0]) + (Convert.ToDouble(stake_str[2]) * 0.001);
                        }
                        iRiDataList.Add(new IRIData
                        {
                            Name = str[0],
                            IRI_left = Convert.ToDouble(str[1]),
                            IRI_right = Convert.ToDouble(str[2]),
                            IRI_average = Convert.ToDouble(str[3]),
                            Stake_start = stake_start,
                            Stake_end = stake_end
                        });

                    }
                }
            }
            catch
            {
                MessageBox.Show("fail to open IRI file.");
            }

            //ccd.txt
            try
            {
                //0:file name, 1:date, 2:time, 3:country, 5:lane, 8:longitude, 11:latitude, 14:stake
                using (StreamReader ccd = new StreamReader((path + "/CCD/ccd.txt"), System.Text.Encoding.Default))
                {
                    while (true)
                    {                        
                        line = ccd.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        if (line.Length == 0){
                            break;
                        }
                        str = line.Split();
                        fileName_Combo.Items.Add(str[0]);
                        string[] stake_str = str[3].Split(new char[2] { 'k', '_' });
                        double stake = Convert.ToDouble(stake_str[0]) + (Convert.ToDouble(stake_str[2]) * 0.001);
                        int index = iRiDataList.FindIndex(x => x.Stake_start <= stake && x.Stake_end >= stake);
                        ccdDataList.Add(new ccdData
                        {
                            Name = str[0],
                            Latitude = Convert.ToDouble(str[4]),
                            Longtitude = Convert.ToDouble(str[5]),
                            Stake = stake
                        });                     
                    }
                    fileName_Combo.SelectedIndex = 0;
                }
            }
            catch
            {
                MessageBox.Show("fail to open ccd file.");
            }            

            if(iRiDataList.Count > 0)
            {
                EnabledItem();
            }

            Directory.CreateDirectory(path + "/temp/");
        }

        private void readFolder_old_Click(object sender, RoutedEventArgs e)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);

            #region resetting
            ccdDataList.Clear();
            iRiDataList.Clear();
            basicdata = new basicData();
            fileName_Combo.Items.Clear();
            fileName_Combo.SelectedIndex = -1;
            path = "";
            oldfilenum = -1;
            #endregion

            //opne choose file window
            getData getdata = new getData();
            path = getdata.SelectedPath();

            string line;
            string[] str = path.Split('\\');
            folder_name = str[str.Length - 1];

            int count = 0;

            #region check files and folders exist            
            if (!Directory.Exists(path + "/CCD/"))
            {
                MessageBox.Show("沒有CCD資料夾");
                return;
            }
            if (!File.Exists(path + "/CCD/ccd.txt"))
            {
                MessageBox.Show("CCD資料夾中沒有ccd.txt檔");
                return;
            }
            if (!Directory.Exists(path + "/Profile/"))
            {
                MessageBox.Show("沒有Profile資料夾");
                return;
            }
            if (!File.Exists(path + "/Profile/IRI.txt"))
            {
                MessageBox.Show("Profile資料夾中沒有IRI.txt檔");
                return;
            }
            #endregion

            //IRI.txt
            try
            {
                using (StreamReader IRI = new StreamReader((path + "/Profile/IRI.txt"), System.Text.Encoding.Default))
                {
                    line = IRI.ReadLine(); //[日期]
                    line = IRI.ReadLine();
                    basicdata.Date = line;
                    line = IRI.ReadLine(); //[單位] or [縣市]
                    if (line == "[縣市]")
                    {
                        IsCity = true;
                        roadType_Combo.SelectedIndex = 0;
                    }
                    else
                    {
                        IsCity = false;
                        roadType_Combo.SelectedIndex = 1;
                    }
                    count = 3;
                    while ((line = IRI.ReadLine()) != null && !String.Equals(line, "里程\tIRI左\tIRI右\tIRI平均\t經度\t緯度\t車速"))
                    {
                        if (IsCity)
                        {
                            direction_box.SelectedIndex = 0;
                            switch (count)
                            {
                                case 3:
                                    city_TextBox.Text = line;
                                    break;
                                case 5:
                                    cityRoad_TextBox.Text = line;
                                    basicdata.Name = line;
                                    break;
                                case 7:
                                    cityStart_TextBox.Text = line;
                                    basicdata.start_end = line;
                                    break;
                                case 9:
                                    cityEnd_TextBox.Text = line;
                                    basicdata.start_end += "、" + line;
                                    break;
                                case 11:
                                    lane_box.SelectedIndex = Convert.ToInt32(line.Remove(0, 2)) - 1;
                                    basicdata.Lane = Convert.ToInt32(line.Remove(0, 2));
                                    break;
                            }
                        }
                        else
                        {
                            switch (count)
                            {
                                case 5:
                                    country_TextBox.Text = line;
                                    basicdata.Name = line;
                                    break;
                                case 7:
                                    basicdata.Direction = line;
                                    if (line == "順樁")
                                    {
                                        direction_box.SelectedIndex = 1;
                                    }
                                    else
                                    {
                                        direction_box.SelectedIndex = 2;
                                    }
                                    break;
                                case 9:
                                    countryStart_TextBox.Text = line;
                                    basicdata.start_end = line;
                                    break;
                                case 11:
                                    countryEnd_TextBox.Text = line;
                                    basicdata.start_end += "、" + line;
                                    break;
                                case 13:
                                    lane_box.SelectedIndex = Convert.ToInt32(line.Remove(0, 2)) - 1;
                                    basicdata.Lane = Convert.ToInt32(line.Remove(0, 2));
                                    break;
                            }
                        }
                        count++;
                    }
                    while ((line = IRI.ReadLine()) != null && line != "")
                    {
                        str = line.Split();
                        string[] stake_str = str[0].Split(new char[3] { 'k', '_', '-' });
                        double stake_start = 0, stake_end = 0;
                        if (direction_box.SelectedIndex != 2)
                        {
                            stake_start = Convert.ToDouble(stake_str[0]) + (Convert.ToDouble(stake_str[2]) * 0.001);
                            stake_end = Convert.ToDouble(stake_str[3]) + (Convert.ToDouble(stake_str[5]) * 0.001);
                        }
                        else
                        {
                            stake_start = Convert.ToDouble(stake_str[3]) + (Convert.ToDouble(stake_str[5]) * 0.001);
                            stake_end = Convert.ToDouble(stake_str[0]) + (Convert.ToDouble(stake_str[2]) * 0.001);
                        }
                        iRiDataList.Add(new IRIData
                        {
                            Name = str[0],
                            IRI_left = Convert.ToDouble(str[1]),
                            IRI_right = Convert.ToDouble(str[2]),
                            IRI_average = Convert.ToDouble(str[3]),
                            Stake_start = stake_start,
                            Stake_end = stake_end
                        });

                    }
                }
            }
            catch
            {
                MessageBox.Show("fail to open IRI file.");
            }

            //ccd.txt
            try
            {
                //0:file name, 1:date, 2:time, 3:country, 5:lane, 8:longitude, 11:latitude, 14:stake
                using (StreamReader ccd = new StreamReader((path + "/CCD/ccd.txt"), System.Text.Encoding.Default))
                {
                    line = ccd.ReadLine();
                    while (true)
                    {
                        line = ccd.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        if (line.Length == 0)
                        {
                            break;
                        }
                        str = line.Split();
                        fileName_Combo.Items.Add(str[0]);
                        string[] stake_str = str[14].Split(new char[2] { 'k', '+' });
                        double stake = Convert.ToDouble(stake_str[0]) + (Convert.ToDouble(stake_str[2]) * 0.001);
                        ccdDataList.Add(new ccdData
                        {
                            Name = str[0],
                            Latitude = Convert.ToDouble(str[11]),
                            Longtitude = Convert.ToDouble(str[8]),
                            Stake = stake
                        });
                    }
                    fileName_Combo.SelectedIndex = 0;
                }
            }
            catch
            {
                MessageBox.Show("fail to open ccd file.");
            }            

            if (iRiDataList.Count > 0)
            {
                EnabledItem();
            }

            Directory.CreateDirectory(path + "/temp/");
        }
        
        public void saveProject_Click(object sender, RoutedEventArgs e)
        {
            if (fileName_Combo.SelectedIndex != -1)
            {
                image_show.saveTemp(path, fileName_Combo.SelectedItem.ToString());
            }
        }

        private void explanation_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("資料夾配置(新版)\n" +
                "1.CCD - 包含照片與ccd.txt的資料夾\n" +
                "2.IRI.txt - IRI資訊的txt檔案\n\n" +
                "資料夾配置(舊版)\n" +
                "1.CCD - 包含照片與ccd.txt的資料夾\n" +
                "2.Profile - 包含IRI.txt的資料夾\n\n" +
                "使用說明:\n" +
                "1.先點選[檔案]->[讀取資料夾]選擇需要的資料夾\n" +
                "2.點選[圖名]選擇需要顯示的照片檔\n" +
                "3.選擇破壞類型及等級並在圖片上畫出直線或多邊形\n" +
                "4.按下ctrl + S儲存圖片\n" +
                "5.重複1.~4.直到資料夾內檔案都修改完成\n" +
                "6.按下ctrl+S儲存最後一張照片資訊\n" +
                "7.按下[匯出]製作excel報表\n\n" +
                "圖片大小與基底切換:\n" +
                "輸入[長]與[寬]後按下Enter鍵即可設定圖片大小並且重新計算基底\n" +
                "有五種基底可供選擇\n" +
                "!!! 更改圖片大小後必須在重新看過所有照片後才會重新計算PCI !!!\n\n" + 
                "快捷鍵(必須切換為英文輸入法才可使用):\n" +
                "ctrl + S: 儲存當前圖片的破壞資訊\n" +
                "ctrl + C: 按下後即可使用方向鍵[左]、[右]來切換上一張或下一張圖片\n" +
                "ctrl + Z: 返回上一步(畫圖的部分)\n" +
                "Enter: 按下Enter鍵即可在同樣破壞、同樣等級的情況下開始繪製新的破壞(其餘時候只需切換破壞種類及等級就會自動新增新的破壞)\n" +
                "[ + ]: 放大圖片\n" +
                "[ - ]: 縮小圖片\n" +
                "insert: 選取[特殊道路現況]中的類別及數量再按下insert鍵即可加入到最右欄的表格中\n" +
                "delete: 選取最右欄表格中欲刪除的項目並按下delete鍵即可刪除\n" +
                "F1~F12: 切換破壞種類");
        }

        #endregion        

        #region shortcut setting
        public void CanExecute(object sender, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = true;
        }
        
        //choosing file shortcut
        public void choose_file(object sender, RoutedEventArgs e)
        {
            fileName_Combo.Focus();
        }
        
        public void delete_point(object sender, RoutedEventArgs e)
        {
            if (path != "" && fileName_Combo.SelectedIndex != -1 && image_show.listView.SelectedIndex != -1)
            {
                image_show.deletePoint(image_show.listView.SelectedIndex);
            }
        }

        private void newData(object sender, RoutedEventArgs e)
        {
            image_show.newData();
        }

        private void ZoomIn(object sender, RoutedEventArgs e)
        {
            image_show.ZoomIn();
        }

        private void ZoomOut(object sender, RoutedEventArgs e)
        {
            image_show.ZoomOut();
        }

        private void delete_Button_Click(object sender, RoutedEventArgs e)
        {
            image_show.conditionDelete(image_show.conditionView.SelectedIndex);            
        }       

        private void cate1_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)cate1.IsChecked == false)
            {
                cate1.IsChecked = true;
                string[] temp = cate1.Content.ToString().Split('(');
                category_Label.Content = "破壞類型 : " + temp[0];
                image_show.categoryChange(temp[0]);
            }
        }
        private void cate3_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)cate3.IsChecked == false)
            {
                cate3.IsChecked = true;
                string[] temp = cate3.Content.ToString().Split('(');
                category_Label.Content = "破壞類型 : " + temp[0];
                image_show.categoryChange(temp[0]);
            }
        }
        private void cate5_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)cate5.IsChecked == false)
            {
                cate5.IsChecked = true;
                string[] temp = cate5.Content.ToString().Split('(');
                category_Label.Content = "破壞類型 : " + temp[0];
                image_show.categoryChange(temp[0]);
            }
        }
        private void cate7_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)cate7.IsChecked == false)
            {
                cate7.IsChecked = true;
                string[] temp = cate7.Content.ToString().Split('(');
                category_Label.Content = "破壞類型 : " + temp[0];
                image_show.categoryChange(temp[0]);
            }
        }
        private void cate8_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)cate8.IsChecked == false)
            {
                cate8.IsChecked = true;
                string[] temp = cate8.Content.ToString().Split('(');
                category_Label.Content = "破壞類型 : " + temp[0];
                image_show.categoryChange(temp[0]);
            }
        }
        private void cate10_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)cate10.IsChecked == false)
            {
                cate10.IsChecked = true;
                string[] temp = cate10.Content.ToString().Split('(');
                category_Label.Content = "破壞類型 : " + temp[0];
                image_show.categoryChange(temp[0]);
            }
        }
        private void cate11_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)cate11.IsChecked == false)
            {
                cate11.IsChecked = true;
                string[] temp = cate11.Content.ToString().Split('(');
                category_Label.Content = "破壞類型 : " + temp[0];
                image_show.categoryChange(temp[0]);
            }
        }
        private void cate12_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)cate12.IsChecked == false)
            {
                cate12.IsChecked = true;
                string[] temp = cate12.Content.ToString().Split('(');
                category_Label.Content = "破壞類型 : " + temp[0];
                image_show.categoryChange(temp[0]);
            }
        }
        private void cate13_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)cate13.IsChecked == false)
            {
                cate13.IsChecked = true;
                string[] temp = cate13.Content.ToString().Split('(');
                category_Label.Content = "破壞類型 : " + temp[0];
                image_show.categoryChange(temp[0]);
            }
        }
        private void cate16_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)cate16.IsChecked == false)
            {
                cate16.IsChecked = true;
                string[] temp = cate16.Content.ToString().Split('(');
                category_Label.Content = "破壞類型 : " + temp[0];
                image_show.categoryChange(temp[0]);
            }
        }
        private void cate17_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)cate17.IsChecked == false)
            {
                cate17.IsChecked = true;
                string[] temp = cate17.Content.ToString().Split('(');
                category_Label.Content = "破壞類型 : " + temp[0];
                image_show.categoryChange(temp[0]);
            }
        }
        private void cate19_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)cate19.IsChecked == false)
            {
                cate19.IsChecked = true;
                string[] temp = cate19.Content.ToString().Split('(');
                category_Label.Content = "破壞類型 : " + temp[0];
                image_show.categoryChange(temp[0]);
            }
        }

        private void insertConditionRecord(object sender, RoutedEventArgs e)
        {
            image_show.conditionRecord();
        }
        #endregion

        #region radio button changed
        private void category_CheckedChanged(object sender, RoutedEventArgs e)
        {            
            RadioButton ra = (RadioButton)sender;
            string[] str = ra.Content.ToString().Split('(');
            category_Label.Content = "破壞類型 : " + str[0];
            image_show.categoryChange(str[0]);
        }
        
        private void level_CheckedChanged(object sender, RoutedEventArgs e)
        {
            RadioButton ra = (RadioButton)sender;
            string str = ra.Content.ToString();
            level_Label.Content = "破壞等級 : " + str;
            image_show.levelChange(str);
        }
        #endregion

        private void fileName_Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(fileName_Combo.SelectedItem != null)
            {
                if (File.Exists(path + "/CCD/" + fileName_Combo.SelectedItem.ToString()))
                {
                    if (oldfilenum != -1)
                    {
                        image_show.saveTemp(path, ccdDataList[oldfilenum].Name);
                    }
                    image_show.path = path;
                    image_show.fileName = fileName_Combo.SelectedItem.ToString();
                    image_show.setImage(path, fileName_Combo.SelectedItem.ToString());
                    oldfilenum = Convert.ToInt32(fileName_Combo.SelectedIndex);
                    path_Textbox.Text = path + @"\" + fileName_Combo.SelectedItem.ToString();

                    if ((bool)cate1.IsChecked == false)
                    {
                        cate1.IsChecked = true;
                        string[] temp = cate1.Content.ToString().Split('(');
                        category_Label.Content = "破壞類型 : " + temp[0];
                        image_show.categoryChange(temp[0]);
                    }
                    if ((bool)level1.IsChecked == false)
                    {
                        level1.IsChecked = true;
                        level_Label.Content = "破壞等級 : " + level1.Content.ToString();
                        image_show.levelChange(level1.Content.ToString());
                    }
                }
                else
                {
                    MessageBox.Show("無圖片檔");
                    fileName_Combo.SelectedIndex = -1;
                }
            }             
        }

        private void export_Click(object sender, RoutedEventArgs e)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);

            string[] temp = path.Split(new[] { "\\" }, StringSplitOptions.None);
            string savepath = path + "\\" + temp[temp.Length - 1] + ".xlsx";
            bool IsExists = File.Exists(savepath);
            if (IsExists)
            {
                try
                {
                    Stream s = File.Open(savepath, FileMode.Open, FileAccess.Read, FileShare.None);
                    s.Close();
                    File.Delete(savepath);
                }
                catch (Exception)
                {
                    MessageBox.Show("檔案被佔用！請關閉檔案再試一次");
                    return;
                }
            }
            ProgressWindow window = new ProgressWindow(path, folder_name, ccdDataList, iRiDataList, basicdata);
            window.Show();
            window.Exporting();         
        }

        private void EnabledItem()
        {
            //menu
            saveProject.IsEnabled = true;
            export_menu.IsEnabled = true;
            //on grid1
            fileName_Combo.IsEnabled = true;
            category_Group.IsEnabled = true;
            destoryLevel.IsEnabled = true;
            //on grid2
            image_show.IsEnabled = true;
            delete_Button.IsEnabled = true;
        }

        private void DisabledItem()
        {
            //menu
            saveProject.IsEnabled = false;
            export_menu.IsEnabled = false;
            //on grid1
            fileName_Combo.IsEnabled = false;
            category_Group.IsEnabled = false;
            destoryLevel.IsEnabled = false;
            //on grid2
            image_show.IsEnabled = false;
            delete_Button.IsEnabled = false;
        }        
    }
}
