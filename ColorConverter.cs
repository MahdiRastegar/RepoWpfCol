using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;
using System.Globalization;
using System.Windows.Controls;
using ControlPaint = System.Windows.Forms.ControlPaint;
using ColorF = System.Drawing.Color;

namespace WpfCol
{
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var btn = FindParent<Button>(value as DependencyObject);
            var gr = btn.Background as LinearGradientBrush;
            if (gr != null)
            {
                if (parameter.ToString() == "change")
                {
                    if (btn.Tag == null)
                        btn.Tag = true;
                    else
                        btn.Tag = null;
                }
                var y = parameter.ToString().Contains("change");
                if (btn.IsMouseOver)
                {
                    if ((btn.Tag as bool?) == true)
                    {
                        return GetLightOrDarkGradient("0", GetSelectedGradient());
                    }
                    else if (y)
                        parameter = "0";
                }
                else if (btn.Tag == null)
                    return gr;
                else if (y)
                    return GetSelectedGradient();

                return GetLightOrDarkGradient(parameter, gr);
            }
            else
            {
                if (parameter.ToString() == "0")
                {
                    return new SolidColorBrush(GetLightOfColor((btn.Background as SolidColorBrush).Color, .15f));
                }
                else
                    return new SolidColorBrush(GetDarkOfColor(
                        GetLightOfColor((btn.Background as SolidColorBrush).Color, .15f), .15f));

            }
            //throw new NotImplementedException();
        }
        private static LinearGradientBrush GetSelectedGradient()
        {
            var gr2 = new LinearGradientBrush();
            gr2.GradientStops.Add(new GradientStop(Colors.Orange, 0));
            gr2.GradientStops.Add(new GradientStop(Colors.Gold, .5));
            gr2.EndPoint = new Point(.5, 1);
            gr2.StartPoint = new Point(.5, 0);
            return gr2;
        }
        private static LinearGradientBrush GetLightOrDarkGradient(object parameter, LinearGradientBrush gr)
        {
            var gr2 = new LinearGradientBrush();
            foreach (var item in gr.GradientStops)
            {
                gr2.GradientStops.Add(new GradientStop(item.Color, item.Offset));
            }
            for (var i = 1; i < gr2.GradientStops.Count; i++)
            {
                gr2.GradientStops[i].Color = parameter.ToString() == "0" ? GetLightOfColor(gr.GradientStops[i].Color, .15f) :
                 GetDarkOfColor(GetLightOfColor(gr.GradientStops[i].Color, .15f), .15f);
            }
            gr2.EndPoint = gr.EndPoint;
            gr2.StartPoint = gr.StartPoint;
            return gr2;
        }

        public static Color GetLightOfColor(Color color, float percOfLightLight)
        {
            var changedcolor=ColorF.FromArgb(color.A,color.R, color.G, color.B);
            var cf=ControlPaint.LightLight(changedcolor);
            return Color.FromArgb(cf.A, cf.R, cf.G, cf.B);
        }
        public static Color GetDarkOfColor(Color color, float percOfLightLight)
        {
            var changedcolor = ColorF.FromArgb(color.A, color.R, color.G, color.B);
            var cf = ControlPaint.Dark(changedcolor);
            return Color.FromArgb(cf.A, cf.R, cf.G, cf.B);
        }
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
