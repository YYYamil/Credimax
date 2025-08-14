﻿/*using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrestamoApp
{
    internal class StringNotNullOrEmptyConverter
    {
    }
}
*/

using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace PrestamoApp;

public class StringNotNullOrEmptyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !string.IsNullOrWhiteSpace(value as string);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}