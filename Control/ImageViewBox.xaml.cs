using optanaPCI.Function;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;
using optanaPCI.Data;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace optanaPCI.Control
{

	public partial class ImageViewBox : UserControl
	{
		#region parameter setting
		public string path = "", fileName = "";
		bool newData_flag = true;
		double ratio = 1;
		string category = "1.鱷魚狀裂縫", level = "1.輕微";
		int drawType = (int)DrawType.area, baseType = (int)BaseType.left_short;
		List<drawingData> drawingDataList = new List<drawingData>();
		List<conditionData> conditionDataList = new List<conditionData>();
		cal_fuction cal_function = new cal_fuction();
		export exports = new export();
		public ImageViewBox()
		{
			InitializeComponent();
		}
		#endregion

		#region functions for called
		public void setImage(string path, string fileName)
		{
			reset();
			BitmapImage image = new BitmapImage(new Uri(path + "/CCD/" + fileName, UriKind.Absolute));
			ImageBrush imageBrush = new ImageBrush(image);
			canvas.Background = imageBrush;
			canvas.Width = image.Width;
			canvas.Height = image.Height;
			redraw(path, fileName);
			newData_flag = true;
		}

		public void categoryChange(string input)
		{
			category = input;
			string[] str = input.Split('.');
			int type = Convert.ToInt32(str[0]);
			//if(type == 7 || type == 17)
			//         {
			//	drawType = (int)DrawType.line;
			//         }
			if (type == 7 || type == 8 || type == 10 || type == 17)
			{
				drawType = (int)DrawType.curve;
			}
			else
			{
				drawType = (int)DrawType.area;
			}
			newData_flag = true;
		}

		public void levelChange(string le)
		{
			level = le;
			newData_flag = true;
		}

		public void newData()
		{
			newData_flag = true;
		}

		public void ZoomIn()
		{
			ScaleTransform st = canvas.LayoutTransform as ScaleTransform;
			st = new ScaleTransform(ratio, ratio);
			if (ratio < 2)
			{
				ratio = ratio + 0.1;
			}
			st = new ScaleTransform(ratio, ratio);
			canvas.LayoutTransform = st;
			base1.Width = Math.Abs(cal_function.origin_points[0].X - cal_function.origin_points[1].X) * ratio;
			base2.Width = Point.Subtract(cal_function.origin_points[1], cal_function.origin_points[2]).Length * ratio;
			base3.Width = Math.Abs(cal_function.origin_points[2].X - cal_function.origin_points[3].X) * ratio;
			base4.Width = Point.Subtract(cal_function.origin_points[3], cal_function.origin_points[0]).Length * ratio;
		}

		public void ZoomOut()
		{
			ScaleTransform st = canvas.LayoutTransform as ScaleTransform;
			st = new ScaleTransform(ratio, ratio);
			if (ratio > 0.2)
			{
				ratio = ratio - 0.1;
			}
			st = new ScaleTransform(ratio, ratio);
			canvas.LayoutTransform = st;
			base1.Width = Math.Abs(cal_function.origin_points[0].X - cal_function.origin_points[1].X) * ratio;
			base2.Width = Point.Subtract(cal_function.origin_points[1], cal_function.origin_points[2]).Length * ratio;
			base3.Width = Math.Abs(cal_function.origin_points[2].X - cal_function.origin_points[3].X) * ratio;
			base4.Width = Point.Subtract(cal_function.origin_points[3], cal_function.origin_points[0]).Length * ratio;
		}
		#endregion

		#region drawing
		private void drawPoint(double x, double y)
		{
			SolidColorBrush brush = new SolidColorBrush();
			brush.Color = Colors.Red;
			Ellipse ellipse = new Ellipse();
			ellipse.Margin = new Thickness(x - 5, y - 5, 0, 0);
			ellipse.Width = 10;
			ellipse.Height = 10;
			ellipse.Stroke = brush;
			ellipse.StrokeThickness = 5;

			canvas.Children.Add(ellipse);
		}

		private void drawLine(List<Point> points)
		{
			SolidColorBrush brush = new SolidColorBrush();
			brush.Color = Colors.Blue;
			Line line = new Line();
			line.X1 = points[0].X;
			line.Y1 = points[0].Y;
			line.X2 = points[1].X;
			line.Y2 = points[1].Y;
			line.Stroke = brush;
			line.StrokeThickness = 2;
			canvas.Children.Add(line);
		}

		private void drawPolyline(List<Point> points)
		{
			SolidColorBrush brush = new SolidColorBrush();
			brush.Color = Colors.Yellow;
			Polyline polyline = new Polyline();
			PointCollection pointCollection = new PointCollection();
			foreach (Point point in points)
			{
				pointCollection.Add(point);
			}
			polyline.Points = pointCollection;
			polyline.Stroke = brush;
			polyline.StrokeThickness = 2;
			canvas.Children.Add(polyline);
		}

		private void drawPolygon(List<Point> points)
		{
			SolidColorBrush brush = new SolidColorBrush();
			brush.Color = Colors.White;
			Polygon polygon = new Polygon();
			PointCollection pointCollection = new PointCollection();
			foreach (Point point in points)
			{
				pointCollection.Add(point);
			}
			polygon.Points = pointCollection;
			polygon.Stroke = brush;
			polygon.StrokeThickness = 2;
			canvas.Children.Add(polygon);
		}

		private void drawStandard(List<Point> points)
		{
			//SolidColorBrush brush = new SolidColorBrush();
			//brush.Color = Colors.Red;
			//Line line1 = new Line() { X1 = points[0].X, Y1 = points[0].Y, X2 = points[1].X, Y2 = points[1].Y };
			//Line line2 = new Line() { X1 = points[1].X, Y1 = points[1].Y, X2 = points[2].X, Y2 = points[2].Y };
			//Line line3 = new Line() { X1 = points[2].X, Y1 = points[2].Y, X2 = points[3].X, Y2 = points[3].Y };
			//Line line4 = new Line() { X1 = points[3].X, Y1 = points[3].Y, X2 = points[0].X, Y2 = points[0].Y };
			//line1.Stroke = line2.Stroke = line3.Stroke = line4.Stroke = brush;
			//line1.StrokeThickness = line2.StrokeThickness = line3.StrokeThickness = line4.StrokeThickness = 5;
			//canvas.Children.Add(line1);
			//canvas.Children.Add(line2);
			//canvas.Children.Add(line3);
			//canvas.Children.Add(line4);

			double top1 = -scrollViewer.VerticalOffset + cal_function.origin_points[0].Y * ratio + 271;
			double top3 = -scrollViewer.VerticalOffset + cal_function.origin_points[3].Y * ratio + 271;
			double left1 = -scrollViewer.HorizontalOffset + cal_function.origin_points[0].X * ratio;
			double left3 = -scrollViewer.HorizontalOffset + cal_function.origin_points[3].X * ratio;

			base1.Width = Math.Abs(points[0].X - points[1].X) * ratio;
			base1.Margin = new Thickness(left1, top1, 0, 0);

			base2.Width = Point.Subtract(points[1], points[2]).Length * ratio;
			double angle = Math.Atan2(points[1].Y - points[2].Y, points[2].X - points[1].X);
			angle = -angle * 180 / Math.PI;
			base2.RenderTransform = new RotateTransform(angle);
			base2.Margin = new Thickness(left1 + base1.Width, top1, 0, 0);

			base3.Width = Math.Abs(points[2].X - points[3].X) * ratio;
			base3.Margin = new Thickness(left3, top3, 0, 0);

			base4.Width = Point.Subtract(points[3], points[0]).Length * ratio;
			angle = Math.Atan2(points[0].Y - points[3].Y, points[0].X - points[3].X);
			angle = angle * 180 / Math.PI + 180;
			base4.RenderTransform = new RotateTransform(angle);
			base4.Margin = new Thickness(left1 + 5, top1 + 5, 0, 0);
		}
		#endregion

		#region data add, save, read, delete
		private void drawingDataAdd(Point point, int id)
		{
			if (id == -1) //first dataline *special case*
			{
				id = 0;
			}
			else if (drawingDataList[id].points.Count == 1 && drawingDataList[id].drawType == (int)DrawType.line)
			{
				newData_flag = false;
			}
			else if (newData_flag == true)
			{
				id = listView.Items.Count;
			}
			if (newData_flag == true) //new data line
			{
				drawingDataList.Add(new drawingData()
				{
					damageType = category,
					damageLevel = level,
					drawType = this.drawType
				});
				drawingDataList[id].points.Add(point);
				newData_flag = false;
			}
			else
			{
				if (drawType == (int)DrawType.line)
				{
					drawingDataList[id].points.Add(point);
					drawLine(drawingDataList[id].points);
					listView.Items.Add(new listviewData()
					{
						damageType = category,
						damageLevel = level,
					});
					listView.SelectedIndex = id + 1;
					newData_flag = true;
				}
				else //add point to curve or area
				{
					drawingDataList[id].points.Add(point);
					if (drawType == (int)DrawType.curve)
					{
						drawPolyline(drawingDataList[id].points);
					}
					else if (drawType == (int)DrawType.area && drawingDataList[id].points.Count >= 3)
					{
						drawPolygon(drawingDataList[id].points);
					}
				}
			}
			listView.Items.Add(new listviewData()
			{
				damageType = category,
				damageLevel = level,
			});
			listView.SelectedIndex = id;
		}

		public void saveTemp(string path, string fileName)
		{
			try
			{
				if (drawingDataList.Count != 0 || conditionDataList.Count != 0)
				{
					using (StreamWriter output = new StreamWriter(path + "/temp/" + fileName + ".txt"))
					{
						output.WriteLine("( " + baseType.ToString() + " )");
						foreach (drawingData data in drawingDataList)
						{
							if (data.points.Count > 0)
							{
								string pp = "";
								foreach (Point point in data.points)
								{
									pp += " " + point.X.ToString() + " " + point.Y.ToString();
								}
								//id damageType damageLevel drawType length area {points}
								output.WriteLine(
									data.damageType + " " +
									data.damageLevel + " " +
									data.drawType + " " +
									data.length.ToString() + " " +
									data.area.ToString() + " {" + pp + " }"
									);
							}
						}
						foreach (conditionData data in conditionDataList)
						{
							output.WriteLine("* " + data.Category + " " + data.Number.ToString() + " *");
						}
						string[] ss = pci_TextBox.Text.Split();
						output.WriteLine("[ " + ss[2] + " ]");
					}
					exports.ToImage(canvas, path, fileName, -scrollViewer.HorizontalOffset, -scrollViewer.VerticalOffset);
				}
				else
				{
					File.Delete(path + "/temp/" + fileName + ".txt");
					File.Delete(path + "/temp/" + fileName);
				}
			}
			catch
			{

			}
		}

		private void redraw(string path, string fileName)
		{
			try
			{
				using (StreamReader input = new StreamReader(path + "/temp/" + fileName + ".txt"))
				{
					reset();
					string str;
					int id = 0;
					while ((str = input.ReadLine()) != null)
					{
						string[] ss = str.Split(' ');
						if (ss[0] == "(")
						{
							baseType = Convert.ToInt32(ss[1]);
							base_combo.SelectedIndex = baseType;
						}
						else if (ss[0] == "*")
						{
							conditionDataList.Add(new conditionData()
							{
								Category = ss[1],
								Number = Convert.ToInt32(ss[2])
							});
							conditionView.Items.Add(new conditionviewData()
							{
								cate = ss[1],
								num = Convert.ToInt32(ss[2])
							});
						}
						else if (ss[0] != "[")
						{
							drawingDataList.Add(new drawingData()
							{
								damageType = ss[0],
								damageLevel = ss[1],
								drawType = Convert.ToInt32(ss[2]),
							});
							for (int i = 6; i < ss.Length - 1; i += 2)
							{
								drawingDataList[id].points.Add(new Point()
								{
									X = Convert.ToDouble(ss[i]),
									Y = Convert.ToDouble(ss[i + 1])
								});
								int c = drawingDataList[id].points.Count;
								drawPoint(drawingDataList[id].points[c - 1].X, drawingDataList[id].points[c - 1].Y);
							}
							if (drawingDataList[id].drawType == (int)DrawType.line && drawingDataList[id].points.Count == 2)
							{
								drawLine(drawingDataList[id].points);
								drawingDataList[id].length = calculate(id);
							}
							else if (drawingDataList[id].drawType == (int)DrawType.curve && drawingDataList[id].points.Count >= 2)
							{
								drawPolyline(drawingDataList[id].points);
								drawingDataList[id].length = calculate(id);
							}
							else if (drawingDataList[id].drawType == (int)DrawType.area && drawingDataList[id].points.Count >= 3)
							{
								drawPolygon(drawingDataList[id].points);
								drawingDataList[id].area = calculate(id);
							}

							listView.Items.Add(new listviewData()
							{
								damageType = drawingDataList[id].damageType,
								damageLevel = drawingDataList[id].damageLevel,
								length = drawingDataList[id].length,
								area = drawingDataList[id].area
							});
							listView.SelectedIndex = id;
							id++;
						}
					}
					drawStandard(cal_function.origin_points);
					double pci = cal_function.PCI(drawingDataList);
					pci_TextBox.Text = "PCI : " + pci.ToString();
				}
			}
			catch
			{
				reset();
				drawStandard(cal_function.origin_points);
			}
		}

		public void deletePoint(int index)
		{
			if (index > -1)
			{
				if (drawingDataList[index].points.Count > 1) //remove the last point in selected data
				{
					drawingDataList[index].points.RemoveAt(drawingDataList[index].points.Count - 1);

					if (drawingDataList[index].drawType == (int)DrawType.curve && drawingDataList[index].points.Count >= 2)
					{
						drawingDataList[index].length = calculate(index);
					}
					else if (drawingDataList[index].drawType == (int)DrawType.area && drawingDataList[index].points.Count >= 3)
					{
						drawingDataList[index].area = calculate(index);
					}
					else
					{
						drawingDataList[index].length = 0;
						drawingDataList[index].area = 0;
					}
					listView.SelectedIndex = index;
				}
				else if (drawingDataList[index].points.Count == 1)
				{
					drawingDataList.RemoveAt(index);
					newData_flag = true;
				}
				saveTemp(path, fileName);
				redraw(path, fileName);
			}
		}
		#endregion

		#region Event
		private void scrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Point current_p = e.GetPosition(canvas);
			if (current_p.X < canvas.Width && current_p.Y < canvas.Height)
			{
				drawPoint(current_p.X, current_p.Y);
				drawingDataAdd(current_p, listView.SelectedIndex);
				saveTemp(path, fileName);
				redraw(path, fileName);
			}
		}

		//canvas position to grid position: (0, 0) => (0, 271)
		private void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			//vertical
			double top1 = -scrollViewer.VerticalOffset + cal_function.origin_points[0].Y * ratio + 271;
			double top3 = -scrollViewer.VerticalOffset + cal_function.origin_points[3].Y * ratio + 271;
			if (top1 > 965)
			{
				base1.Visibility = Visibility.Hidden;
				base2.Visibility = Visibility.Hidden;
				base3.Visibility = Visibility.Hidden;
				base4.Visibility = Visibility.Hidden;
			}
			else
			{
				base1.Visibility = Visibility.Visible;
				base2.Visibility = Visibility.Visible;
				base3.Visibility = Visibility.Visible;
				base4.Visibility = Visibility.Visible;
			}
			base1.Margin = new Thickness(base1.Margin.Left, top1, 0, 0);
			base2.Margin = new Thickness(base2.Margin.Left, top1, 0, 0);
			base3.Margin = new Thickness(base3.Margin.Left, top3, 0, 0);
			base4.Margin = new Thickness(base4.Margin.Left, top1 + 5, 0, 0);

			//horizontal
			double left1 = -scrollViewer.HorizontalOffset + cal_function.origin_points[0].X * ratio;
			double left3 = -scrollViewer.HorizontalOffset + cal_function.origin_points[3].X * ratio;

			base1.Margin = new Thickness(left1, base1.Margin.Top, 0, 0);
			base2.Margin = new Thickness(left1 + base1.Width, base2.Margin.Top, 0, 0);
			base3.Margin = new Thickness(left3, base3.Margin.Top, 0, 0);
			base4.Margin = new Thickness(left1 + 5, base4.Margin.Top, 0, 0);
		}

		private void usertype_num_KeyDown(object sender, KeyEventArgs e)
		{
			TextBox txt = sender as TextBox;
			if ((e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
			{
				e.Handled = false;
			}
			else if (((e.Key >= Key.D0 && e.Key <= Key.D9)) && e.KeyboardDevice.Modifiers != ModifierKeys.Shift)
			{
				e.Handled = false;
			}
			else
			{
				e.Handled = true;
			}
		}
		#endregion

		#region base setting
		private void base_combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			baseChange(base_combo.SelectedIndex, path, fileName, Convert.ToDouble(Length_TextBox.Text), Convert.ToDouble(Width_TextBox.Text));
		}

		private void Length_TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
            {
				baseChange(base_combo.SelectedIndex, path, fileName, Convert.ToDouble(Length_TextBox.Text), Convert.ToDouble(Width_TextBox.Text));
			}
		}

		private void Width_TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				baseChange(base_combo.SelectedIndex, path, fileName, Convert.ToDouble(Length_TextBox.Text), Convert.ToDouble(Width_TextBox.Text));
			}
		}
		private void baseChange(int index, string path, string fileName, double l, double w)
		{
			l = l / 1624;
			w = w / 1234;
			switch (index)
			{
				case 0: //941.jpg
					cal_function.origin_points.Clear();
					cal_function.origin_points.Add(new Point { X = 360 * l, Y = 700 * w });
					cal_function.origin_points.Add(new Point { X = 880 * l, Y = 700 * w });
					cal_function.origin_points.Add(new Point { X = 1610 * l, Y = 1225 * w });
					cal_function.origin_points.Add(new Point { X = 5 * l, Y = 1225 * w });
					cal_function.after_points.Clear();
					cal_function.after_points.Add(new Point { X = 0 * l, Y = 0 * w });
					cal_function.after_points.Add(new Point { X = 4 * l, Y = 0 * w });
					cal_function.after_points.Add(new Point { X = 4 * l, Y = 5 * w });
					cal_function.after_points.Add(new Point { X = 0 * l, Y = 5 * w });
					break;
				case 1: //71.jpg
					cal_function.origin_points.Clear();
					cal_function.origin_points.Add(new Point { X = 460 * l, Y = 670 * w });
					cal_function.origin_points.Add(new Point { X = 640 * l, Y = 670 * w });
					cal_function.origin_points.Add(new Point { X = 1620 * l, Y = 1225 * w });
					cal_function.origin_points.Add(new Point { X = 0 * l, Y = 1225 * w });
					cal_function.after_points.Clear();
					cal_function.after_points.Add(new Point { X = 0 * l, Y = 0 * w });
					cal_function.after_points.Add(new Point { X = 4 * l, Y = 0 * w });
					cal_function.after_points.Add(new Point { X = 4 * l, Y = 5 * w });
					cal_function.after_points.Add(new Point { X = 0 * l, Y = 5 * w });
					break;
				case 2: //2.jpg
					cal_function.origin_points.Clear();
					cal_function.origin_points.Add(new Point { X = 910 * l, Y = 700 * w });
					cal_function.origin_points.Add(new Point { X = 1165 * l, Y = 700 * w });
					cal_function.origin_points.Add(new Point { X = 1625 * l, Y = 1220 * w });
					cal_function.origin_points.Add(new Point { X = 60 * l, Y = 1220 * w });
					cal_function.after_points.Clear();
					cal_function.after_points.Add(new Point { X = 0 * l, Y = 0 * w });
					cal_function.after_points.Add(new Point { X = 4 * l, Y = 0 * w });
					cal_function.after_points.Add(new Point { X = 4 * l, Y = 15 * w });
					cal_function.after_points.Add(new Point { X = 0 * l, Y = 15 * w });
					break;
				case 3: //128.jpg
					cal_function.origin_points.Clear();
					cal_function.origin_points.Add(new Point { X = 925 * l, Y = 580 * w });
					cal_function.origin_points.Add(new Point { X = 1000 * l, Y = 580 * w });
					cal_function.origin_points.Add(new Point { X = 1615 * l, Y = 1220 * w });
					cal_function.origin_points.Add(new Point { X = 308 * l, Y = 1220 * w });
					cal_function.after_points.Clear();
					cal_function.after_points.Add(new Point { X = 0 * l, Y = 0 * w });
					cal_function.after_points.Add(new Point { X = 4 * l, Y = 0 * w });
					cal_function.after_points.Add(new Point { X = 4 * l, Y = 15 * w });
					cal_function.after_points.Add(new Point { X = 0 * l, Y = 15 * w });
					break;
				case 4: //209.jpg
					cal_function.origin_points.Clear();
					cal_function.origin_points.Add(new Point { X = 555 * l, Y = 580 * w });
					cal_function.origin_points.Add(new Point { X = 630 * l, Y = 580 * w });
					cal_function.origin_points.Add(new Point { X = 1285 * l, Y = 1220 * w });
					cal_function.origin_points.Add(new Point { X = 2 * l, Y = 1220 * w });
					cal_function.after_points.Clear();
					cal_function.after_points.Add(new Point { X = 0 * l, Y = 0 * w });
					cal_function.after_points.Add(new Point { X = 4 * l, Y = 0 * w });
					cal_function.after_points.Add(new Point { X = 4 * l, Y = 15 * w });
					cal_function.after_points.Add(new Point { X = 0 * l, Y = 15 * w });
					break;
			}
			canvas.Children.Clear();
			cal_function.transform_matrix();
			baseType = index;
			if (drawingDataList.Count > 0)
			{
				saveTemp(path, fileName);
				redraw(path, fileName);
			}
			else
			{
				drawStandard(cal_function.origin_points);
			}
		}

		#endregion

		#region condition record
		public void conditionRecord()
		{
			string cate = "";
			int num = 0;
			if (ditch.IsChecked == true && ditch_Combo.SelectedIndex != -1)
			{
				cate = "水溝";
				num = ditch_Combo.SelectedIndex + 1;
				ditch.IsChecked = false;
			}
			else if (coil.IsChecked == true && coil_Combo.SelectedIndex != -1)
			{
				cate = "感應線圈";
				num = coil_Combo.SelectedIndex + 1;
				coil.IsChecked = false;
			}
			else if (manhole.IsChecked == true && manhole_Combo.SelectedIndex != -1)
			{
				cate = "人手孔";
				num = manhole_Combo.SelectedIndex + 1;
				manhole.IsChecked = false;
			}
			else if (junction.IsChecked == true && junction_Combo.SelectedIndex != -1)
			{
				cate = "新舊面交界";
				num = junction_Combo.SelectedIndex + 1;
				junction.IsChecked = false;
			}
			else if (expansion.IsChecked == true && expansion_Combo.SelectedIndex != -1)
			{
				cate = "伸縮縫";
				num = expansion_Combo.SelectedIndex + 1;
				expansion.IsChecked = false;
			}
			else if (intersection.IsChecked == true && intersection_Combo.SelectedIndex != -1)
			{
				cate = "路口交界";
				num = intersection_Combo.SelectedIndex + 1;
				intersection.IsChecked = false;
			}
			else if (railway.IsChecked == true && railway_Combo.SelectedIndex != -1)
			{
				cate = "鐵路平交道";
				num = railway_Combo.SelectedIndex + 1;
				railway.IsChecked = false;
			}
			else if (slowhump.IsChecked == true && slowhump_Combo.SelectedIndex != -1)
			{
				cate = "減速坡";
				num = slowhump_Combo.SelectedIndex + 1;
				slowhump.IsChecked = false;
			}
			else if (nonasphalt.IsChecked == true && nonasphalt_Combo.SelectedIndex != -1)
			{
				cate = "非瀝青路面";
				num = nonasphalt_Combo.SelectedIndex + 1;
				nonasphalt.IsChecked = false;
			}
			else if (slowline.IsChecked == true && slowline_Combo.SelectedIndex != -1)
			{
				cate = "減速線";
				num = slowline_Combo.SelectedIndex + 1;
				slowline.IsChecked = false;
			}
			else if (usertype.IsChecked == true)
			{
				if (cate == "")
				{
					MessageBox.Show("種類不可為空");
					return;
				}
				cate = usertype_cate.Text;
				num = Convert.ToInt32(usertype_num.Text);
				usertype.IsChecked = false;
			}

			if (num != 0)
			{
				int id = conditionDataList.FindIndex(x => x.Category.Equals(cate));
				if (id != -1)
				{
					conditionDataList[id].Number += num;
					saveTemp(path, fileName);
					redraw(path, fileName);
				}
				else
				{
					conditionDataList.Add(new conditionData()
					{
						Category = cate,
						Number = num
					});
					conditionView.Items.Add(new conditionviewData()
					{
						cate = cate,
						num = num
					});
				}
			}
			conditionReset();
		}

		private void conditionReset()
		{
			//reset condition case index
			ditch_Combo.SelectedIndex = coil_Combo.SelectedIndex = manhole_Combo.SelectedIndex = -1;
			junction_Combo.SelectedIndex = expansion_Combo.SelectedIndex = intersection_Combo.SelectedIndex = -1;
			railway_Combo.SelectedIndex = slowhump_Combo.SelectedIndex = nonasphalt_Combo.SelectedIndex = -1;
			slowline_Combo.SelectedIndex = -1;
			usertype_cate.Text = "";
			usertype_num.Text = "";
		}

		public void conditionDelete(int index)
		{
			if (index > -1)
			{
				conditionDataList.RemoveAt(index);
				conditionView.Items.RemoveAt(index);
				conditionView.SelectedIndex = conditionView.Items.Count;
			}
		}
		#endregion

		#region resetting, cal length and area
		private void reset()
		{
			//point list clear
			drawingDataList.Clear();
			listView.Items.Clear();

			//condition list clear
			conditionDataList.Clear();
			conditionView.Items.Clear();
			conditionReset();
			//canvas clear
			canvas.Children.Clear();
			pci_TextBox.Text = "PCI : 100";
		}

		private double calculate(int num)
		{
			if (drawingDataList[num].drawType == (int)DrawType.line) //calculate line length
			{
				return Point.Subtract(cal_function.transform(drawingDataList[num].points[0]), cal_function.transform(drawingDataList[num].points[1])).Length;
			}
			else if (drawingDataList[num].drawType == (int)DrawType.curve)
			{
				double length = 0;
				for (int i = 0; i < drawingDataList[num].points.Count - 1; i++)
				{
					length += Point.Subtract(cal_function.transform(drawingDataList[num].points[i]), cal_function.transform(drawingDataList[num].points[i + 1])).Length;
				}
				return length;
			}
			else //for area
			{
				int point_num = drawingDataList[num].points.Count;
				double area = 0;
				for (int i = 0; i < point_num; i++)
				{
					var x = cal_function.transform(drawingDataList[num].points[i]).X;
					var y = i + 1 < point_num ? cal_function.transform(drawingDataList[num].points[i + 1]).Y : cal_function.transform(drawingDataList[num].points[0]).Y;
					//+
					area += x * y;
				}
				for (int i = 0; i < point_num; i++)
				{
					var y = cal_function.transform(drawingDataList[num].points[i]).Y;
					var x = i + 1 < point_num ? cal_function.transform(drawingDataList[num].points[i + 1]).X : cal_function.transform(drawingDataList[num].points[0]).X;
					//+
					area -= x * y;
				}
				return Math.Abs(area / 2);
			}
		}
		#endregion

		#region list data class & enum
		public class listviewData
		{
			public string damageType { get; set; }
			public string damageLevel { get; set; }
			public double length { get; set; }
			public double area { get; set; }
		}

        public class conditionviewData
		{
			public string cate { get; set; }
			public int num { get; set; }
		}

		enum DrawType : int
		{
			line = 0,
			curve = 1,
			area = 2
		}

		enum BaseType : int
		{
			left_short = 0,
			left_long = 1,
			right_long = 2,
			center_left = 3,
			crnter_right = 4
		}
		#endregion
	}
}