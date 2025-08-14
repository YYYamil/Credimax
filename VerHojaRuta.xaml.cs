using Microsoft.Data.SqlClient;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using QColors = QuestPDF.Helpers.Colors;


namespace PrestamoApp;

public partial class VerHojaRuta : ContentPage
{
    public ObservableCollection<HojaRutaModel> HojaRutaDatos { get; set; } = new();

    public VerHojaRuta()
    {
        InitializeComponent();
        BindingContext = this;
        CargarDatos();
    }

    private void CargarDatos()
    {
        string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True";

        string query = @"SELECT Id_Personas, Id_Credito, (Apellido + ' ' + Nombre) AS Nombre,
                        TipoCredito, ValorCredito, ImporteValorCuota AS ImporteCuota,
                        FechaUltimoPago, MontoUltimoPago, SaldoALaFecha
                        FROM HojaRuta
                        --WHERE CAST(FechaRegistro AS DATE) = CAST(GETDATE() AS DATE)";

        using SqlConnection conn = new(connectionString);
        conn.Open();
        using SqlCommand cmd = new(query, conn);
        using SqlDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            HojaRutaDatos.Add(new HojaRutaModel
            {
                Id_Personas = reader["Id_Personas"].ToString(),
                Id_Credito = reader["Id_Credito"].ToString(),
                Nombre = reader["Nombre"].ToString(),
                TipoCredito = reader["TipoCredito"].ToString(),
                ValorCredito = reader.GetDecimal(reader.GetOrdinal("ValorCredito")),
                ImporteCuota = reader.GetDecimal(reader.GetOrdinal("ImporteCuota")),
                FechaUltimoPago = reader["FechaUltimoPago"] as DateTime?,
                MontoUltimoPago = reader["MontoUltimoPago"] as decimal?,
                SaldoALaFecha = reader.GetDecimal(reader.GetOrdinal("SaldoALaFecha"))
            });
        }
    }

    private async void OnPagarClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is HojaRutaModel model)
        {
            string idCredito = model.Id_Credito;

            // 1. Eliminar fila de la tabla HojaRuta en la BD
            string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True";
            string deleteQuery = "DELETE FROM HojaRuta WHERE Id_Credito = @IdCredito";

            using (SqlConnection conn = new(connectionString))
            {
                conn.Open();
                using SqlCommand cmd = new(deleteQuery, conn);
                cmd.Parameters.AddWithValue("@IdCredito", idCredito);
                cmd.ExecuteNonQuery();
            }

            // 2. Eliminar el ítem de la colección local para que desaparezca de la interfaz
            var itemToRemove = HojaRutaDatos.FirstOrDefault(x => x.Id_Credito == idCredito);
            if (itemToRemove != null)
            {
                HojaRutaDatos.Remove(itemToRemove);
            }

            // 3. (Opcional) Navegar a la pantalla de pago
            //await Navigation.PushAsync(new PagarHojaRutaPage(idCredito));
            await Navigation.PushAsync(new PagarHojaRutaPage(long.Parse(idCredito)));

        }
    }


    /*    private async void OnExportarPDFClicked(object sender, EventArgs e)
        {
            try
            {
                await DisplayAlert("Info", "Exportando PDF, por favor espere...", "OK");

                var datosHojaRuta = new List<Dictionary<string, object>>();

                // 1. Obtener datos y generar PDF en un hilo aparte
                await Task.Run(async () =>
                {
                    string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True";
                    string query = @"SELECT (HojaRuta.Apellido + ' ' + HojaRuta.Nombre) AS Nombre,
                                Observacion, ValorCredito,
                                ImporteValorCuota AS ImporteCuota,
                                SaldoALaFecha, FechaUltimoPago, MontoUltimoPago,
                                Direccion, Cel, Zona
                                FROM HojaRuta, Personas
                                WHERE Personas.Id_Personas = HojaRuta.Id_Personas";

                    using SqlConnection conn = new(connectionString);
                    await conn.OpenAsync();
                    using SqlCommand cmd = new(query, conn);
                    using SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var fila = new Dictionary<string, object>
                    {
                        { "Nombre", reader["Nombre"].ToString() },
                        { "Observacion", reader.IsDBNull(reader.GetOrdinal("Observacion")) ? "" : reader.GetString(reader.GetOrdinal("Observacion")) },
                        { "ValorCredito", reader.GetDecimal(reader.GetOrdinal("ValorCredito")) },
                        { "ImporteCuota", reader.GetDecimal(reader.GetOrdinal("ImporteCuota")) },
                        { "SaldoALaFecha", reader.GetDecimal(reader.GetOrdinal("SaldoALaFecha")) },
                        { "FechaUltimoPago", reader["FechaUltimoPago"] as DateTime? ?? DateTime.MinValue },
                        { "MontoUltimoPago", reader["MontoUltimoPago"] as decimal? ?? 0m },
                        { "Direccion", reader.IsDBNull(reader.GetOrdinal("Direccion")) ? "" : reader.GetString(reader.GetOrdinal("Direccion")) },
                        { "Cel", reader.IsDBNull(reader.GetOrdinal("Cel")) ? "" : reader.GetString(reader.GetOrdinal("Cel")) },
                        { "Zona", reader.IsDBNull(reader.GetOrdinal("Zona")) ? "" : reader.GetString(reader.GetOrdinal("Zona")) }
                    };
                        datosHojaRuta.Add(fila);
                    }
                });

                if (datosHojaRuta.Count == 0)
                {
                    await DisplayAlert("Aviso", "La consulta no devolvió datos.", "OK");
                    return;
                }

                // 2. Generar el PDF en otro hilo también
                string pdfFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, "ReporteHojaRuta.pdf");

                await Task.Run(() =>
                {
                    var document = new PdfSharpCore.Pdf.PdfDocument();
                    var page = document.AddPage();
                    var gfx = XGraphics.FromPdfPage(page);
                    var font = new XFont("Arial", 12, XFontStyle.Regular);
                    var boldFont = new XFont("Arial", 12, XFontStyle.Bold);

                    double yPoint = 40;

                    gfx.DrawString("Reporte de Hoja de Ruta", boldFont, XBrushes.Black,
                        new XRect(0, yPoint, page.Width, page.Height),
                        XStringFormats.TopCenter);

                    yPoint += 40;

                    foreach (var fila in datosHojaRuta)
                    {
                        if (yPoint > page.Height - 100)
                        {
                            page = document.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            yPoint = 40;
                        }

                        string nombre = fila["Nombre"].ToString();
                        string observacion = fila["Observacion"].ToString();
                        string direccion = fila["Direccion"].ToString();
                        string cel = fila["Cel"].ToString();
                        string zona = fila["Zona"].ToString();
                        string fechaUltimoPago = ((DateTime)fila["FechaUltimoPago"]).ToString("dd/MM/yyyy");

                        gfx.DrawString($"Nombre: {nombre}", boldFont, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft); yPoint += 20;
                        gfx.DrawString($"Observación: {observacion}", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft); yPoint += 20;
                        gfx.DrawString($"Valor Crédito: ${fila["ValorCredito"]:N2}", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft); yPoint += 20;
                        gfx.DrawString($"Importe Cuota: ${fila["ImporteCuota"]:N2}", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft); yPoint += 20;
                        gfx.DrawString($"Saldo a la Fecha: ${fila["SaldoALaFecha"]:N2}", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft); yPoint += 20;
                        gfx.DrawString($"Fecha Último Pago: {fechaUltimoPago}", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft); yPoint += 20;
                        gfx.DrawString($"Monto Último Pago: ${fila["MontoUltimoPago"]:N2}", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft); yPoint += 20;
                        gfx.DrawString($"Dirección: {direccion}", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft); yPoint += 20;
                        gfx.DrawString($"Celular: {cel}", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft); yPoint += 20;
                        gfx.DrawString($"Zona: {zona}", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft); yPoint += 30;
                    }

                    using var stream = File.Create(pdfFilePath);
                    document.Save(stream);
                });

                // 3. Mostrar opciones al usuario
                string action = await DisplayActionSheet("¿Qué desea hacer con el PDF?", "Cancelar", null, "Guardar", "Compartir");
                if (action == "Guardar")
                {
                    await DisplayAlert("Guardado", $"El PDF se guardó en: {pdfFilePath}", "OK");
                }
                else if (action == "Compartir")
                {
                    await Share.RequestAsync(new ShareFileRequest
                    {
                        Title = "Compartir Reporte de Hoja de Ruta",
                        File = new ShareFile(pdfFilePath)
                    });
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error:\n{ex.Message}", "OK");
            }
        }



        */

    /*
    private async void OnExportarPDFClicked(object sender, EventArgs e)
    {
        try
        {
            await DisplayAlert("Info", "Exportando PDF, por favor espere...", "OK");

            var datosHojaRuta = new List<Dictionary<string, object>>();

            // 1. Configurar el FontResolver (antes de cualquier operación con PdfSharpCore)
            GlobalFontSettings.FontResolver = new CustomFontResolver();

            // 2. Obtener datos y generar PDF en un hilo aparte
            await Task.Run(async () =>
            {
                string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True;Connect Timeout=10";
                string query = @"SELECT (HojaRuta.Apellido + ' ' + HojaRuta.Nombre) AS Nombre,
                            Observacion, ValorCredito,
                            ImporteValorCuota AS ImporteCuota,
                            SaldoALaFecha, FechaUltimoPago, MontoUltimoPago,
                            Direccion, Cel, Zona
                            FROM HojaRuta, Personas
                            WHERE Personas.Id_Personas = HojaRuta.Id_Personas";

                using SqlConnection conn = new(connectionString);
                await conn.OpenAsync();
                using SqlCommand cmd = new(query, conn);
                cmd.CommandTimeout = 15;
                using SqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var fila = new Dictionary<string, object>
                {
                    { "Nombre", reader["Nombre"].ToString() },
                    { "Observacion", reader.IsDBNull(reader.GetOrdinal("Observacion")) ? "" : reader.GetString(reader.GetOrdinal("Observacion")) },
                    { "ValorCredito", reader.GetDecimal(reader.GetOrdinal("ValorCredito")) },
                    { "ImporteCuota", reader.GetDecimal(reader.GetOrdinal("ImporteCuota")) },
                    { "SaldoALaFecha", reader.GetDecimal(reader.GetOrdinal("SaldoALaFecha")) },
                    { "FechaUltimoPago", reader["FechaUltimoPago"] as DateTime? ?? DateTime.MinValue },
                    { "MontoUltimoPago", reader["MontoUltimoPago"] as decimal? ?? 0m },
                    { "Direccion", reader.IsDBNull(reader.GetOrdinal("Direccion")) ? "" : reader.GetString(reader.GetOrdinal("Direccion")) },
                    { "Cel", reader.IsDBNull(reader.GetOrdinal("Cel")) ? "" : reader.GetString(reader.GetOrdinal("Cel")) },
                    { "Zona", reader.IsDBNull(reader.GetOrdinal("Zona")) ? "" : reader.GetString(reader.GetOrdinal("Zona")) }
                };
                    datosHojaRuta.Add(fila);
                }
            });

            if (datosHojaRuta.Count == 0)
            {
                await DisplayAlert("Aviso", "La consulta no devolvió datos.", "OK");
                return;
            }

            // 3. Generar el PDF en otro hilo también
            string pdfFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, "ReporteHojaRuta.pdf");

            await Task.Run(() =>
            {
                var document = new PdfSharpCore.Pdf.PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var font = new XFont("OpenSans", 12, XFontStyle.Regular); // Cambiado de "Arial" a "OpenSans"
                var boldFont = new XFont("OpenSans", 12, XFontStyle.Bold);

                double yPoint = 40;

                gfx.DrawString("Reporte de Hoja de Ruta", boldFont, XBrushes.Black,
                    new XRect(0, yPoint, page.Width, page.Height),
                    XStringFormats.TopCenter);

                yPoint += 40;

                foreach (var fila in datosHojaRuta)
                {
                    if (yPoint > page.Height - 100)
                    {
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        yPoint = 40;
                    }

                    string nombre = fila["Nombre"].ToString();
                    string observacion = fila["Observacion"].ToString();
                    string direccion = fila["Direccion"].ToString();
                    string cel = fila["Cel"].ToString();
                    string zona = fila["Zona"].ToString();
                    string fechaUltimoPago = ((DateTime)fila["FechaUltimoPago"]) == DateTime.MinValue
                        ? "Sin pagos"
                        : ((DateTime)fila["FechaUltimoPago"]).ToString("dd/MM/yyyy");

                    gfx.DrawString($"Nombre: {nombre}", boldFont, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft); yPoint += 20;
                    gfx.DrawString($"Observación: {observacion}", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft); yPoint += 20;
                    gfx.DrawString($"Valor Crédito: ${fila["ValorCredito"]:N2}", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft); yPoint += 20;
                    gfx.DrawString($"Importe Cuota: ${fila["ImporteCuota"]:N2}", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft); yPoint += 20;
                    gfx.DrawString($"Saldo a la Fecha: ${fila["SaldoALaFecha"]:N2}", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft); yPoint += 20;
                    gfx.DrawString($"Fecha Último Pago: {fechaUltimoPago}", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft); yPoint += 20;
                    gfx.DrawString($"Monto Último Pago: ${fila["MontoUltimoPago"]:N2}", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft); yPoint += 20;
                    gfx.DrawString($"Dirección: {direccion}", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft); yPoint += 20;
                    gfx.DrawString($"Celular: {cel}", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft); yPoint += 20;
                    gfx.DrawString($"Zona: {zona}", font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft); yPoint += 30;
                }

                using var stream = File.Create(pdfFilePath);
                document.Save(stream);
            });

            // 4. Mostrar opciones al usuario
            string action = await DisplayActionSheet("¿Qué desea hacer con el PDF?", "Cancelar", null, "Guardar", "Compartir");
            if (action == "Guardar")
            {
                await DisplayAlert("Guardado", $"El PDF se guardó en: {pdfFilePath}", "OK");
            }
            else if (action == "Compartir")
            {
                await Share.RequestAsync(new ShareFileRequest
                {
                    Title = "Compartir Reporte de Hoja de Ruta",
                    File = new ShareFile(pdfFilePath)
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error:\n{ex.Message}", "OK");
        }
    }

    */

    //private async void OnExportarPDFClicked(object sender, EventArgs e)
    //{
    //    try
    //    {
    //        await DisplayAlert("Info", "Exportando PDF, por favor espere...", "OK");

    //        var datosHojaRuta = new List<Dictionary<string, object>>();

    //        // Obtener datos de SQL Server
    //        await Task.Run(async () =>
    //        {
    //            string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True";
    //            string query = @"SELECT (HojaRuta.Apellido + ' ' + HojaRuta.Nombre) AS Nombre,
    //                        Observacion, ValorCredito,
    //                        ImporteValorCuota AS ImporteCuota,
    //                        SaldoALaFecha, FechaUltimoPago, MontoUltimoPago,
    //                        Direccion, Cel, Zona
    //                        FROM HojaRuta, Personas
    //                        WHERE Personas.Id_Personas = HojaRuta.Id_Personas";

    //            using SqlConnection conn = new(connectionString);
    //            await conn.OpenAsync();
    //            using SqlCommand cmd = new(query, conn);
    //            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

    //            while (await reader.ReadAsync())
    //            {
    //                var fila = new Dictionary<string, object>
    //            {
    //                { "Nombre", reader["Nombre"].ToString() },
    //                { "Observacion", reader.IsDBNull(reader.GetOrdinal("Observacion")) ? "" : reader.GetString(reader.GetOrdinal("Observacion")) },
    //                { "ValorCredito", reader.GetDecimal(reader.GetOrdinal("ValorCredito")) },
    //                { "ImporteCuota", reader.GetDecimal(reader.GetOrdinal("ImporteCuota")) },
    //                { "SaldoALaFecha", reader.GetDecimal(reader.GetOrdinal("SaldoALaFecha")) },
    //                { "FechaUltimoPago", reader["FechaUltimoPago"] as DateTime? ?? DateTime.MinValue },
    //                { "MontoUltimoPago", reader["MontoUltimoPago"] as decimal? ?? 0m },
    //                { "Direccion", reader.IsDBNull(reader.GetOrdinal("Direccion")) ? "" : reader.GetString(reader.GetOrdinal("Direccion")) },
    //                { "Cel", reader.IsDBNull(reader.GetOrdinal("Cel")) ? "" : reader.GetString(reader.GetOrdinal("Cel")) },
    //                { "Zona", reader.IsDBNull(reader.GetOrdinal("Zona")) ? "" : reader.GetString(reader.GetOrdinal("Zona")) }
    //            };
    //                datosHojaRuta.Add(fila);
    //            }
    //        });

    //        if (datosHojaRuta.Count == 0)
    //        {
    //            await DisplayAlert("Aviso", "La consulta no devolvió datos.", "OK");
    //            return;
    //        }

    //        // Definir ruta de guardado según plataforma
    //        string pdfFilePath;
    //        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
    //        {
    //            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    //            pdfFilePath = Path.Combine(documents, "ReporteHojaRuta.pdf");
    //        }
    //        else
    //        {
    //            pdfFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, "ReporteHojaRuta.pdf");
    //        }

    //        // Generar PDF
    //        await Task.Run(() =>
    //        {
    //            var document = new PdfSharpCore.Pdf.PdfDocument();
    //            var page = document.AddPage();
    //            var gfx = XGraphics.FromPdfPage(page);
    //            var font = new XFont("Arial", 10, XFontStyle.Regular);
    //            var boldFont = new XFont("Arial", 10, XFontStyle.Bold);

    //            double y = 40;

    //            gfx.DrawString("Reporte de Hoja de Ruta", boldFont, XBrushes.Black,
    //                new XRect(0, y, page.Width, page.Height), XStringFormats.TopCenter);
    //            y += 30;

    //            string header = "Nombre | Crédito | Cuota | Saldo | Últ. Pago | Monto | Dirección | Cel | Zona";
    //            gfx.DrawString(header, boldFont, XBrushes.Black, new XRect(40, y, page.Width - 80, page.Height), XStringFormats.TopLeft);
    //            y += 20;

    //            foreach (var fila in datosHojaRuta)
    //            {
    //                if (y > page.Height - 60)
    //                {
    //                    page = document.AddPage();
    //                    gfx = XGraphics.FromPdfPage(page);
    //                    y = 40;
    //                }

    //                string nombre = fila["Nombre"].ToString();
    //                string credito = $"{fila["ValorCredito"]:N2}";
    //                string cuota = $"{fila["ImporteCuota"]:N2}";
    //                string saldo = $"{fila["SaldoALaFecha"]:N2}";
    //                string fechaUltimoPago = ((DateTime)fila["FechaUltimoPago"]) == DateTime.MinValue
    //                    ? "Sin pagos"
    //                    : ((DateTime)fila["FechaUltimoPago"]).ToString("dd/MM/yyyy");
    //                string montoPago = $"{fila["MontoUltimoPago"]:N2}";
    //                string direccion = fila["Direccion"].ToString();
    //                string cel = fila["Cel"].ToString();
    //                string zona = fila["Zona"].ToString();

    //                string linea = $"{nombre} | {credito} | {cuota} | {saldo} | {fechaUltimoPago} | {montoPago} | {direccion} | {cel} | {zona}";
    //                gfx.DrawString(linea, font, XBrushes.Black, new XRect(40, y, page.Width - 80, page.Height), XStringFormats.TopLeft);
    //                y += 15;
    //            }

    //            using var stream = File.Create(pdfFilePath);
    //            document.Save(stream);
    //        });

    //        // Mostrar opciones
    //        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
    //        {
    //            await DisplayAlert("PDF generado", $"El archivo fue guardado en:\n{pdfFilePath}", "OK");
    //        }
    //        else
    //        {
    //            string action = await DisplayActionSheet("¿Qué desea hacer con el PDF?", "Cancelar", null, "Compartir");
    //            if (action == "Compartir")
    //            {
    //                await Share.RequestAsync(new ShareFileRequest
    //                {
    //                    Title = "Compartir Reporte de Hoja de Ruta",
    //                    File = new ShareFile(pdfFilePath)
    //                });
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        await DisplayAlert("Error", $"Ocurrió un error:\n{ex.Message}", "OK");
    //    }
    //}


    private async void OnExportarPDFClicked(object sender, EventArgs e)
    {
        try
        {
            await DisplayAlert("Info", "Exportando PDF, por favor espere...", "OK");

            var datosHojaRuta = new List<HojaRutaPdfModel>();

            string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;" +
                "user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;" +
                "data source=Credimax.mssql.somee.com;persist security info=False;" +
                "initial catalog=Credimax;TrustServerCertificate=True";

            string query = @"
    SELECT 
        (HojaRuta.Apellido + ' ' + HojaRuta.Nombre) AS Nombre,
        ValorCredito,
        Observacion AS Descripcion, 
        ImporteValorCuota AS ImporteCuota,
        Direccion,
        Cel,
        (
            SELECT COUNT(*) 
            FROM Pagos, Cuotas, Creditos 
            WHERE Creditos.Id_Credito = Cuotas.Id_Credito 
              AND Cuotas.Id_Cuota = Pagos.Id_Cuota 
              AND Creditos.Id_Credito = HojaRuta.Id_Credito
              AND Cuotas.Estado = 'PAGADA'
        ) AS CuotasPagadas,
        (
            SELECT NumCuotas 
            FROM Creditos 
            WHERE Creditos.Id_Credito = HojaRuta.Id_Credito
        ) AS NroCuotas,
        (
            SELECT SUM(MontoPagado)
            FROM Pagos
            WHERE Id_Cuota IN (
                SELECT Id_Cuota FROM Cuotas
                WHERE Id_Credito = HojaRuta.Id_Credito
            )
        ) AS SumaPagos,

        (ValorCredito - 
            ISNULL((
                SELECT SUM(MontoPagado)
                FROM Pagos
                WHERE Id_Cuota IN (
                    SELECT Id_Cuota FROM Cuotas
                    WHERE Id_Credito = HojaRuta.Id_Credito
                )
            ), 0)
        ) AS SaldoTotal

    FROM 
        HojaRuta
    JOIN 
        Personas ON Personas.Id_Personas = HojaRuta.Id_Personas";

            using SqlConnection conn = new(connectionString);
            await conn.OpenAsync();
            using SqlCommand cmd = new(query, conn);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                int cuotasPagadas = reader.IsDBNull(reader.GetOrdinal("CuotasPagadas")) ? 0 : reader.GetInt32(reader.GetOrdinal("CuotasPagadas"));
                int nroCuotas = reader.IsDBNull(reader.GetOrdinal("NroCuotas")) ? 0 : reader.GetInt32(reader.GetOrdinal("NroCuotas"));
                string cuota = $"{cuotasPagadas} / {nroCuotas}";

                datosHojaRuta.Add(new HojaRutaPdfModel
                {
                    Nombre = reader["Nombre"].ToString(),
                    Descripcion = reader["Descripcion"].ToString(),
                    ImporteCuota = reader.GetDecimal(reader.GetOrdinal("ImporteCuota")),
                    Direccion = reader["Direccion"].ToString(),
                    Cel = reader["Cel"].ToString(),
                    Cuota = cuota,
                    SumaPagos = reader.IsDBNull(reader.GetOrdinal("SumaPagos")) ? 0 : reader.GetDecimal(reader.GetOrdinal("SumaPagos")),
                    ValorCredito = reader.GetDecimal(reader.GetOrdinal("ValorCredito")),
                    SaldoTotal = reader.GetDecimal(reader.GetOrdinal("SaldoTotal"))
                });
            }

            if (datosHojaRuta.Count == 0)
            {
                await DisplayAlert("Aviso", "La consulta no devolvió datos.", "OK");
                return;
            }

            string filePath = Path.Combine(FileSystem.Current.AppDataDirectory, "HojaRuta.pdf");

            // Generar PDF
            QuestPDF.Settings.License = LicenseType.Community;
            var doc = new HojaRutaDocument(datosHojaRuta);
            doc.GeneratePdf(filePath);

            // Abrir PDF automáticamente
            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(filePath)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error:\n{ex.Message}", "OK");
        }
    }


















}

public class HojaRutaModel
{
    public string Id_Personas { get; set; }
    public string Id_Credito { get; set; }
    public string Nombre { get; set; }
    public string TipoCredito { get; set; }
    public string Observacion { get; set; }
    public decimal ImporteCuota { get; set; }
    public decimal ValorCredito { get; set; }
    public DateTime? FechaUltimoPago { get; set; }
    public decimal? MontoUltimoPago { get; set; }
    public decimal SaldoALaFecha { get; set; }
    public string Direccion { get; set; }
    public string Cel { get; set; }
    public string Zona { get; set; }
}


public class HojaRutaPdfModel
{
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal ImporteCuota { get; set; }
    public string Direccion { get; set; }
    public string Cel { get; set; }
    public string Cuota { get; set; }  // Formato: "x / y"
    public decimal SumaPagos { get; set; }

    public decimal ValorCredito { get; set; }
    public decimal SaldoTotal { get; set; }

}

public class HojaRutaDocument : IDocument
{
    public List<HojaRutaPdfModel> Datos { get; }

    public HojaRutaDocument(List<HojaRutaPdfModel> datos)
    {
        Datos = datos;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(30);
            page.PageColor(QColors.White);
            page.DefaultTextStyle(x => x.FontSize(10));

            page.Header().Text("Reporte de Hoja de Ruta").SemiBold().FontSize(16).AlignCenter();

            page.Content().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(100); // Nombre
                    columns.RelativeColumn();    // Descripción
                    columns.ConstantColumn(60);  // Cuota
                    columns.ConstantColumn(70);  // Importe
                    columns.ConstantColumn(70);  // Pagos
                    columns.ConstantColumn(70);  // Cel
                    columns.RelativeColumn();    // Dirección
                    
                    columns.ConstantColumn(70);  // Saldo

                });

                table.Header(header =>
                {
                    header.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Text("Nombre").Bold();
                    header.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Text("Descripción").Bold();
                    header.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Text("Cuota").Bold();
                    header.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Text("Importe").Bold();
                    header.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Text("Pagado").Bold();
                    header.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Text("Celular").Bold();
                    header.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Text("Dirección").Bold();
                    
                    header.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Text("Saldo").Bold();



                });

                foreach (var item in Datos)
                {
                    table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Text(item.Nombre);
                    table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Text(item.Descripcion);
                    table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Text(item.Cuota);
                    table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Text($"${item.ImporteCuota:N2}");
                    table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Text($"${item.SumaPagos:N2}");
                    table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Text(item.Cel);
                    table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Text(item.Direccion);

                    table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Text($"${item.SaldoTotal:N2}");


                }
            });
        });
    }

    //static IContainer CellStyle(IContainer container)
    //{
    //    return container
    //        .PaddingVertical(5)
    //        .BorderBottom(1)
    //        .BorderColor(QColors.Grey.Lighten2);
    //}


}
