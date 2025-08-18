
using Microsoft.Maui.Controls;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using Microsoft.Maui.ApplicationModel;
//using SafariServices;

namespace PrestamoApp;

public partial class ResumenCtaPage : ContentPage
{
    private long idCredito;
    private string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True";

    private ResumenCta resumen;

    //public Command<long> BorrarCuotaCommand { get; set; }


    //Borrar credito
    //public Command EliminarCreditoCommand { get; set; }

    public List<CuotaDetalle> Cuotas { get; set; } = new();


    
    public ResumenCtaPage(long idCredito)
    {
        InitializeComponent();

        //BorrarCuotaCommand = new Command<long>(async (idCuota) => await BorrarCuotaAsync(idCuota));

        // Inicializar el comando sin verificación inmediata
        //EliminarCreditoCommand = new Command(
           // async () => await EliminarCreditoAsync(),
           // () => false // Inicialmente deshabilitado
        //);

        BindingContext = this;
        this.idCredito = idCredito;
        LoadResumen();
    }

    private async void LoadResumen()
    {
        try
        {
            IsBusy = true; // Activar indicador de carga
            resumen = await GetResumenCtaAsync();
            if (resumen != null)
            {
                lblApellido.Text = resumen.Apellido;
                lblNombre.Text = resumen.Nombre;
                lblTipoCredito.Text = resumen.TipoCredito;
                lblObservacion.Text = resumen.Observacion;
                lblMontoCredito.Text = resumen.MontoCredito.ToString("C", CultureInfo.CreateSpecificCulture("es-AR"));
                lblSumaPago.Text = resumen.SumaPago.ToString("C", CultureInfo.CreateSpecificCulture("es-AR"));
                lblSaldoTotal.Text = resumen.SaldoTotal.ToString("C", CultureInfo.CreateSpecificCulture("es-AR"));
                lblMontoFinanciado.Text = resumen.MontoFinanciado.ToString("C", CultureInfo.CreateSpecificCulture("es-AR"));
                lblFechaOtorgamiento.Text = resumen.FechaOtorgamiento.ToString("dd/MM/yyyy");


                cuotasListView.ItemsSource = resumen.Cuotas;


                // Actualizar estado del botón Eliminar
                // Actualizar estado del comando después de cargar los datos
                var tieneCuotasPagadas = await TieneCuotasPagadasAsync();
                //EliminarCreditoCommand = new Command(
                   // async () => await EliminarCreditoAsync(),
                   // () => !tieneCuotasPagadas
                //);
                //OnPropertyChanged(nameof(EliminarCreditoCommand)); // Notificar cambio
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    //    private async Task<ResumenCta> GetResumenCtaAsync()
    //    {
    //        using SqlConnection conn = new(connectionString);
    //        await conn.OpenAsync();

    //        SqlCommand cmd = new(@"SELECT
    //    p.Apellido,
    //    p.Nombre,
    //    c.TipoCredito,
    //    c.observacion,
    //    c.MontoCredito AS MontoOtorgado,
    //    ISNULL(SUM(pa.MontoPagado), 0) AS SumaPago,
    //    (MAX(cu.ImporteCuota) * c.NumCuotas - ISNULL(SUM(pa.MontoPagado), 0)) AS SaldoTotal,
    //    (MAX(cu.ImporteCuota) * c.NumCuotas) AS MontoFinanciado,
    //    MAX(cu.ImporteCuota) AS ValorCuota,
    //    c.NumCuotas,
    //    c.FechaOtorgamiento
    //FROM 
    //    Personas p
    //    INNER JOIN Creditos c ON p.Id_Personas = c.Id_Personas
    //    INNER JOIN Cuotas cu ON c.Id_Credito = cu.Id_Credito
    //    LEFT JOIN Pagos pa ON cu.Id_Cuota = pa.Id_Cuota
    //WHERE 
    //    c.EstadoCredito = 'ACTIVO'
    //    AND c.Id_Credito = @IdCredito
    //GROUP BY
    //    p.Apellido,
    //    p.Nombre,
    //    c.TipoCredito,
    //    c.observacion,
    //    c.MontoCredito,
    //    c.NumCuotas,
    //    c.Id_Credito,
    //    c.FechaOtorgamiento;
    //"
    //       , conn);

    //        cmd.Parameters.AddWithValue("@IdCredito", idCredito);

    //        using var reader = await cmd.ExecuteReaderAsync();
    //        if (await reader.ReadAsync())
    //        {
    //            var resumen = new ResumenCta
    //            {
    //                Apellido = reader.GetString(0),
    //                Nombre = reader.GetString(1),
    //                TipoCredito = reader.GetString(2),
    //                Observacion = reader.GetString(3),
    //                MontoCredito = reader.GetDecimal(4),
    //                SumaPago = reader.GetDecimal(5),
    //                SaldoTotal = reader.GetDecimal(6),
    //                MontoFinanciado = reader.GetDecimal(7),
    //                Cuotas = await GetCuotasAsync(),
    //                FechaOtorgamiento = reader.GetDateTime(10) // índice según el orden de columnas

    //            };
    //            return resumen;
    //        }
    //        return null;
    //    }

    //



    //

    private async Task<ResumenCta> GetResumenCtaAsync()
    {
        using SqlConnection conn = new(connectionString);
        await conn.OpenAsync();

        SqlCommand cmd = new(@"
SELECT
    p.Apellido,
    p.Nombre,
    p.Cel,                      -- Celular
    c.TipoCredito,
    c.FormaPago,               -- <<< agregado
    c.observacion,
    c.MontoCredito AS MontoOtorgado,
    ISNULL(SUM(pa.MontoPagado), 0) AS SumaPago,
    (MAX(cu.ImporteCuota) * c.NumCuotas - ISNULL(SUM(pa.MontoPagado), 0)) AS SaldoTotal,
    (MAX(cu.ImporteCuota) * c.NumCuotas) AS MontoFinanciado,
    MAX(cu.ImporteCuota) AS ValorCuota,   -- <<< el valor de cada cuota
    c.NumCuotas,
    c.FechaOtorgamiento
FROM 
    Personas p
    INNER JOIN Creditos c ON p.Id_Personas = c.Id_Personas
    INNER JOIN Cuotas cu ON c.Id_Credito = cu.Id_Credito
    LEFT JOIN Pagos pa ON cu.Id_Cuota = pa.Id_Cuota
WHERE 
    c.EstadoCredito = 'ACTIVO'
    AND c.Id_Credito = @IdCredito
GROUP BY
    p.Apellido,
    p.Nombre,
    p.Cel,
    c.TipoCredito,
    c.FormaPago,               -- <<< agregado al GROUP BY
    c.observacion,
    c.MontoCredito,
    c.NumCuotas,
    c.Id_Credito,
    c.FechaOtorgamiento;
", conn);

        cmd.Parameters.AddWithValue("@IdCredito", idCredito);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var resumen = new ResumenCta
            {
                Apellido = reader.GetString(0),
                Nombre = reader.GetString(1),
                Celular = reader.IsDBNull(2) ? "" : reader.GetString(2),
                TipoCredito = reader.GetString(3),
                FormaPago = reader.IsDBNull(4) ? "" : reader.GetString(4), // <<< nuevo
                Observacion = reader.GetString(5),
                MontoCredito = reader.GetDecimal(6),
                SumaPago = reader.GetDecimal(7),
                SaldoTotal = reader.GetDecimal(8),
                MontoFinanciado = reader.GetDecimal(9),
                ValorCuota = reader.GetDecimal(10),                         // <<< nuevo
                Cuotas = await GetCuotasAsync(),
                FechaOtorgamiento = reader.GetDateTime(12)
            };
            return resumen;
        }
        return null;
    }


    private async Task<List<CuotaDetalle>> GetCuotasAsync()
    {
        var cuotas = new List<CuotaDetalle>();
        using SqlConnection conn = new(connectionString);
        await conn.OpenAsync();

        //SqlCommand cmd = new(@"
        //    WITH CuotasOrdenadas AS (
        //    SELECT Id_Cuota, FechaVto, Estado,
        //           ROW_NUMBER() OVER (PARTITION BY Id_Credito ORDER BY FechaVto ASC) AS NumeroCuota
        //    FROM Cuotas
        //    WHERE Id_Credito = @idCredito
        //)
        //SELECT NumeroCuota, FechaVto, ISNULL(SUM(p.MontoPagado), 0), Estado
        //FROM CuotasOrdenadas c
        //LEFT JOIN Pagos p ON c.Id_Cuota = p.Id_Cuota
        //GROUP BY NumeroCuota, FechaVto, Estado
        //ORDER BY NumeroCuota
        //", conn);
        SqlCommand cmd = new(@"WITH CuotasOrdenadas AS (
    SELECT Id_Cuota, FechaVto, Estado,
           ROW_NUMBER() OVER (PARTITION BY Id_Credito ORDER BY FechaVto ASC) AS NumeroCuota
    FROM Cuotas
    WHERE Id_Credito = @IdCredito
)
SELECT 
    c.Id_Cuota, 
    c.NumeroCuota, 
    c.FechaVto, 
    ISNULL(SUM(p.MontoPagado), 0) AS MontoPagado, 
    c.Estado, 
    MAX(p.FechaPago) AS FechaPago
FROM CuotasOrdenadas c
LEFT JOIN Pagos p ON c.Id_Cuota = p.Id_Cuota
GROUP BY c.Id_Cuota, c.NumeroCuota, c.FechaVto, c.Estado
ORDER BY c.NumeroCuota;
                            ", conn);


        cmd.Parameters.AddWithValue("@idCredito", idCredito); // Cambiar parámetro

        using var reader = await cmd.ExecuteReaderAsync();
        //while (await reader.ReadAsync())
        //{
        //    cuotas.Add(new CuotaDetalle
        //    {
        //        //NumeroCuota = reader.GetInt32(0),
        //        NumeroCuota = Convert.ToInt32(reader.GetInt64(0)),

        //        FechaVto = reader.GetDateTime(1),
        //        MontoPagado = reader.GetDecimal(2),
        //        Estado = reader.GetString(3)
        //    });
        //}

        while (await reader.ReadAsync())
        {
            cuotas.Add(new CuotaDetalle
            {
                IdCuota = reader.GetInt64(0),
                NumeroCuota = Convert.ToInt32(reader.GetInt64(1)),
                FechaVto = reader.GetDateTime(2),
                MontoPagado = reader.GetDecimal(3),
                Estado = reader.GetString(4),
                FechaPago = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5)
            });
        }

        return cuotas;
    }

    //private async void OnShareClicked(object sender, EventArgs e)
    //{
    //    if (resumen == null) return;

    //    var culture = CultureInfo.CreateSpecificCulture("es-AR");
    //    var msg = new StringBuilder();

    //    msg.AppendLine($"📋 RESUMEN DE CUENTA");
    //    msg.AppendLine($"👤 {resumen.Apellido}, {resumen.Nombre}");
    //    msg.AppendLine($"📝 {resumen.TipoCredito} - {resumen.Observacion}");
    //    msg.AppendLine($"💰 Monto Crédito: {resumen.MontoCredito.ToString("C", culture)}");
    //    msg.AppendLine($"💵 Suma Pago: {resumen.SumaPago.ToString("C", culture)}");
    //    msg.AppendLine($"📉 Saldo Total: {resumen.SaldoTotal.ToString("C", culture)}");
    //    msg.AppendLine("\n📅 Cuotas:");
    //    foreach (var c in resumen.Cuotas)
    //    {
    //        msg.AppendLine($"- Cuota {c.NumeroCuota}: {c.FechaVto:dd/MM/yyyy} | {c.MontoPagado.ToString("C", culture)} | {c.Estado}");
    //    }

    //    await Launcher.OpenAsync($"whatsapp://send?text={Uri.EscapeDataString(msg.ToString())}");
    //}

    private async void OnShareClicked(object sender, EventArgs e)
    {
        if (resumen == null) return;

        var celular = resumen.Celular?.Trim();

        if (!string.IsNullOrWhiteSpace(celular) && celular.All(char.IsDigit) && celular.Length >= 8)
        {
            if (!celular.StartsWith("54") && !celular.StartsWith("1"))
                celular = "54" + celular;

            var culture = CultureInfo.CreateSpecificCulture("es-AR");

            //string encabezado =
            //    $"*📋 RESUMEN DE CUENTA*\n\n" +
            //    $"• *Cliente:* {resumen.Apellido}, {resumen.Nombre}\n" +
            //    $"• *Tipo Crédito:* {resumen.TipoCredito}\n" +
            //    (string.IsNullOrWhiteSpace(resumen.FormaPago) ? "" : $"• *Forma de pago:* {resumen.FormaPago}\n") +
            //    (string.IsNullOrWhiteSpace(resumen.Observacion) ? "" : $"• *Observación:* {resumen.Observacion}\n") +
            //    $"• *Valor cuota:* {resumen.ValorCuota.ToString("C", culture)}\n" +
            //    //$"• *Monto financiado:* {resumen.MontoFinanciado.ToString("C", culture)}\n" +
            //    //$"• *Suma pagos:* {resumen.SumaPago.ToString("C", culture)}\n" +
            //    $"• *Saldo total:* {resumen.SaldoTotal.ToString("C", culture)}\n\n" +
            //    $"*📅 Cuotas:*\n";

            //string detalleCuotas = string.Join("\n", (resumen.Cuotas ?? new List<CuotaDetalle>()).Select(c =>
            //    $"- {c.NumeroCuota} | {(c.FechaPago.HasValue ? c.FechaPago.Value.ToString("dd/MM/yyyy") : "-")} | {c.MontoPagado.ToString("C", culture)} | {c.Estado}"));

            //string mensaje = encabezado + detalleCuotas;

            string encabezado =
    $"🌟 *CREDIMAX - Resumen de Cuenta* 🌟\n" +
    $"────────────────────────────\n" +
    $"Estimado/a *{resumen.Nombre} {resumen.Apellido}*,\n" +
    $"A continuación le compartimos el detalle actualizado de su crédito personal:\n\n" +
    $"• *Tipo de Crédito:* {resumen.TipoCredito}\n" +
    (string.IsNullOrWhiteSpace(resumen.FormaPago) ? "" : $"• *Forma de Pago:* {resumen.FormaPago}\n") +
    (string.IsNullOrWhiteSpace(resumen.Observacion) ? "" : $"• *Observaciones:* {resumen.Observacion}\n") +
    $"• *Valor de cada cuota:* {resumen.ValorCuota.ToString("C", culture)}\n" +
    $"• *Saldo total pendiente:* {resumen.SaldoTotal.ToString("C", culture)}\n\n" +
    $"*📅 Detalle de cuotas:*\n" +
    "```" + // inicio bloque monoespaciado
    $"{"N°",-4} {"F. Pago",-12} {"Importe",-12} {"Estado",-10}\n" +
    new string('-', 42) + "\n";

            string detalleCuotas = string.Join("\n", (resumen.Cuotas ?? new List<CuotaDetalle>()).Select(c =>
                $"{c.NumeroCuota,-4} " +
                $"{(c.FechaPago.HasValue ? c.FechaPago.Value.ToString("dd/MM/yyyy") : "-"), -12} " +
                $"{c.MontoPagado.ToString("C", culture),-12} " +
                $"{c.Estado,-10}"
            ));

            string pie =
                "```\n" + // cierre bloque monoespaciado
                $"────────────────────────────\n" +
                $"💬 Ante cualquier consulta, recuerde que puede comunicarse con nosotros.\n" +
                $"Gracias por confiar en *CREDIMAX*.\n" +
                $"¡Le deseamos un excelente día! 🌞";

            string mensaje = encabezado + detalleCuotas + pie;


            try
            {
                // 1️⃣ Intentar abrir directamente la app de WhatsApp
                await Launcher.OpenAsync($"whatsapp://send?phone={celular}&text={Uri.EscapeDataString(mensaje)}");
            }
            catch
            {
                try
                {
                    // 2️⃣ Fallback: abrir en navegador
                    string fallbackUrl = $"https://wa.me/{celular}?text={Uri.EscapeDataString(mensaje)}";
                    await Launcher.Default.OpenAsync(fallbackUrl);
                }
                catch (Exception ex)
                {
                    await DisplayAlert(
                        "Error crítico",
                        $"No se pudo abrir WhatsApp.\nDetalles:\n{ex.Message}\n\n" +
                        "Asegúrese de tener WhatsApp instalado y un número válido.",
                        "OK");
                }
            }
        }
        else
        {
            await DisplayAlert("Número inválido",
                "Formato requerido: código de país + número (Ej: 5491123456789)",
                "OK");
        }
    }



    private async Task BorrarCuotaAsync(long idCuota)
    {
        bool confirm = await DisplayAlert("Confirmar", $"¿Desea borrar el pago de la Cuota N° {idCuota}?", "Sí", "No");
        if (!confirm)
            return;

        try
        {
            using SqlConnection conn = new(connectionString);
            await conn.OpenAsync();

            SqlCommand cmd = new(@"
            UPDATE Pagos 
            SET MontoPagado = 0 
            WHERE Id_Cuota = @idCuota;

            UPDATE Cuotas 
            SET Estado = 'Impaga' 
            WHERE Id_Cuota = @idCuota;", conn);

            cmd.Parameters.AddWithValue("@idCuota", idCuota);
            await cmd.ExecuteNonQueryAsync();

            await DisplayAlert("Éxito", "Cuota modificada correctamente.", "OK");

            LoadResumen(); // recarga datos
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    //Para el boton borrar credito
    private async Task<bool> TieneCuotasPagadasAsync()
    {
        using SqlConnection conn = new(connectionString);
        await conn.OpenAsync();

        SqlCommand cmd = new(@"
        SELECT COUNT(*) 
        FROM Cuotas 
        WHERE Id_Credito = @idCredito AND Estado = 'Pagada'", conn);

        cmd.Parameters.AddWithValue("@idCredito", idCredito);
        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());

        return count > 0;
    }

    private async Task EliminarCreditoAsync()
    {
        bool confirm = await DisplayAlert("Confirmar",
            "¿Está seguro que desea eliminar este crédito y todas sus cuotas?",
            "Sí", "No");
        if (!confirm) return;

        try
        {
            using SqlConnection conn = new(connectionString);
            await conn.OpenAsync();

            // Iniciar transacción
            using SqlTransaction transaction = conn.BeginTransaction();

            try
            {
                // 1. Eliminar pagos asociados a las cuotas de este crédito
                SqlCommand cmdPagos = new(@"
                DELETE FROM Pagos 
                WHERE Id_Cuota IN (
                    SELECT Id_Cuota FROM Cuotas WHERE Id_Credito = @idCredito
                )", conn, transaction);
                cmdPagos.Parameters.AddWithValue("@idCredito", idCredito);
                await cmdPagos.ExecuteNonQueryAsync();

                // 2. Eliminar cuotas
                SqlCommand cmdCuotas = new(@"
                DELETE FROM Cuotas 
                WHERE Id_Credito = @idCredito", conn, transaction);
                cmdCuotas.Parameters.AddWithValue("@idCredito", idCredito);
                await cmdCuotas.ExecuteNonQueryAsync();

                // 3. Eliminar crédito
                SqlCommand cmdCredito = new(@"
                DELETE FROM Creditos 
                WHERE Id_Credito = @idCredito", conn, transaction);
                cmdCredito.Parameters.AddWithValue("@idCredito", idCredito);
                await cmdCredito.ExecuteNonQueryAsync();

                // Confirmar transacción
                await transaction.CommitAsync();

                await DisplayAlert("Éxito", "Crédito eliminado correctamente.", "OK");

                // Regresar a la página anterior
                await Navigation.PopAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo eliminar el crédito: {ex.Message}", "OK");
        }
    }

    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime && dateTime != default(DateTime))
                return dateTime.ToString("dd/MM/yyyy");
            return "-"; // Or any other placeholder for NULL/invalid dates
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}

public class ResumenCta
{
    public string Apellido { get; set; }
    public string Nombre { get; set; }
    public string TipoCredito { get; set; }
    public string Observacion { get; set; }
    public decimal MontoCredito { get; set; }
    public decimal SumaPago { get; set; }
    public decimal SaldoTotal { get; set; }
    public decimal MontoFinanciado { get; set; }
    public List<CuotaDetalle> Cuotas { get; set; }
    public DateTime FechaOtorgamiento { get; set; }
    public string Celular { get; set; }

    public string FormaPago { get; set; }      // de Creditos.FormaPago
    public decimal ValorCuota { get; set; }    // de Cuotas.ImporteCuota (MAX)



}

public class CuotaDetalle
{
    public int NumeroCuota { get; set; }
    public DateTime FechaVto { get; set; }
    public decimal MontoPagado { get; set; }
    public string Estado { get; set; }

    public long IdCuota { get; set; }
    public DateTime? FechaPago { get; set; }
}
