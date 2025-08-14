/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrestamoApp
{
    internal class EstadoColorConverter
    {
    }
}
*/

using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace PrestamoApp;

public class EstadoColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string estado)
        {
            return estado switch
            {
                "Activo" => "#1A73E8", // Azul
                "Cancelado" => "#4CAF50", // Verde
                _ => "#666666" // Gris por defecto
            };
        }
        return "#666666";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}