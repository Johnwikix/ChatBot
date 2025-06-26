using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace wpfChat.Helpers
{
    /// <summary>
    /// 布尔值到对齐方式转换器
    /// </summary>
    [ValueConversion(typeof(bool), typeof(HorizontalAlignment))]
    public class BoolToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isFromMe = (bool)value;
            return isFromMe ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
