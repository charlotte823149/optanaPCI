using Microsoft.Office.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace optanaPCI.Function
{
    public class export
    {
        public void ToImage(Canvas canvas, string path, string fileName, double hor_offset, double ver_offset)
        {            
            canvas.GetType().GetProperty("VisualOffset", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(canvas, new Vector());
            path = path + @"\temp\" + fileName;
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.DesiredSize.Width, (int)canvas.DesiredSize.Height, 96d, 96d, PixelFormats.Default);
            rtb.Render(canvas);
            DrawingVisual dvInk = new DrawingVisual();
            DrawingContext dcInk = dvInk.RenderOpen();
            dcInk.DrawRectangle(canvas.Background, null, new Rect(0d, 0d, canvas.Width, canvas.Height));
            dcInk.Close();

            FileStream fs = File.Open(path, FileMode.OpenOrCreate);//save bitmap to file
            JpegBitmapEncoder encoder1 = new JpegBitmapEncoder();
            encoder1.Frames.Add(BitmapFrame.Create(rtb));
            encoder1.Save(fs);
            fs.Close();
            Vector old_offset = new Vector(hor_offset, ver_offset);
            canvas.GetType().GetProperty("VisualOffset", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(canvas, old_offset);
        }

        public void DrawImage(Canvas canvas1, string path, string fileName, int iii)
        {
            canvas1.Children.RemoveAt(iii);
            
            canvas1.GetType().GetProperty("VisualOffset", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(canvas1, new Vector());            
            path = path + @"\temp\" + fileName;
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas1.Width, (int)canvas1.Height, 96d, 96d, PixelFormats.Default);
			rtb.Render(canvas1);
			DrawingVisual dvInk = new DrawingVisual();
			DrawingContext dcInk = dvInk.RenderOpen();
			dcInk.DrawRectangle(canvas1.Background, null, new Rect(0d, 0d, canvas1.Width, canvas1.Height));
			dcInk.Close();

			FileStream fs = File.Open(path, FileMode.OpenOrCreate);//save bitmap to file
			PngBitmapEncoder encoder1 = new PngBitmapEncoder();
			encoder1.Frames.Add(BitmapFrame.Create(rtb));
			encoder1.Save(fs);
			fs.Close();
		}
	}
}
