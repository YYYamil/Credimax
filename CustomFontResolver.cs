/*using PdfSharpCore.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PrestamoApp
{
    internal class CustomFontResolver
    {
    }
}*/
using System.Diagnostics;
using PdfSharpCore.Fonts;
using System.Reflection;

namespace PrestamoApp;

public class CustomFontResolver : IFontResolver
{
    public string DefaultFontName => "OpenSans-Regular";

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        if (familyName.Equals("OpenSans", StringComparison.OrdinalIgnoreCase))
        {
            if (isBold)
            {
                return new FontResolverInfo("OpenSans-Semibold");
            }
            return new FontResolverInfo("OpenSans-Regular");
        }

        return new FontResolverInfo("OpenSans-Regular");
    }

    public byte[] GetFont(string faceName)
    {

        



        var assembly = typeof(CustomFontResolver).GetTypeInfo().Assembly;
        string resourceName = faceName switch
        {
            "OpenSans-Regular" => "PrestamoApp.Resources.Fonts.OpenSans-Regular.ttf",
            "OpenSans-Semibold" => "PrestamoApp.Resources.Fonts.OpenSans-Semibold.ttf",
            _ => "PrestamoApp.Resources.Fonts.OpenSans-Regular.ttf"
        };


      




        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new Exception($"No se pudo encontrar el recurso de fuente: {resourceName}");
        }

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }






}