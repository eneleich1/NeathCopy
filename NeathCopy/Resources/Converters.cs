using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static NeathCopy.VisualCopy;

namespace NeathCopy.Resources
{
    public class TargetDeviceNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var parts = text.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length <= 2) return text.Trim();

            return string.Join(" ", parts.Take(parts.Length - 2));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class TargetDeviceSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var parts = text.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) return string.Empty;

            return string.Join(" ", parts.Skip(parts.Length - 2));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
    //public class ImagePauseResumeConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        try
    //        {
    //            // <ImageBrush x:Key="pauseBrush" ImageSource="Images/Blue/pause.png" Stretch="Uniform"/>
    //            //< ImageBrush x: Key = "resumeStartBrush" ImageSource = "Images/Blue/resume.png" Stretch = "Uniform" />
    //            var state = (VisualCopyState)value;

    //            //if(state==VisualCopyState.Paused)
    //            //   return (Brush)this.FindResource("resumeStartBrush");
    //            //else return (Brush)this.FindResource("pauseBrush");
    //            return null;
    //        }
    //        catch (Exception)
    //        {
    //            return value;
    //        }
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        return value;
    //    }
    //}
}
