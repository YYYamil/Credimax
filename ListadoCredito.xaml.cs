using Microsoft.Maui.Controls;
using System.Data.SqlClient;
using System.Collections.ObjectModel;

namespace PrestamoApp;

public partial class ListadoCredito : ContentPage
{
    private long idPersona;
    private string connectionString = "workstation id=Credimax.mssql.somee.com;packet size=4096;user id=mgonzy2_SQLLogin_1;pwd=s48wgh6m39;data source=Credimax.mssql.somee.com;persist security info=False;initial catalog=Credimax;TrustServerCertificate=True";
    public ObservableCollection<CreditoResumen> Creditos { get; set; } = new();

    public ListadoCredito(long idPersona)
    {
        InitializeComponent();
        this.idPersona = idPersona;
        BindingContext = this;
        // No llamamos a LoadCreditosAsync aquí, lo manejaremos en OnAppearing
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadClienteNombreAsync(); // Carga el nombre y apellido del cliente
        await LoadCreditosAsync(); // Carga los créditos
    }

    private async Task LoadClienteNombreAsync()
    {
        using SqlConnection conn = new(connectionString);
        await conn.OpenAsync();

        string query = "SELECT Nombre, Apellido FROM Personas WHERE Id_Personas = @idPersona";
        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@idPersona", idPersona);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            string nombre = reader.GetString(0);
            string apellido = reader.GetString(1);
            ClienteNombreLabel.Text = $"Cliente: {nombre} {apellido}";
        }
    }

    private async Task LoadCreditosAsync(string formaPagoFilter = null)
    {
        using SqlConnection conn = new(connectionString);
        await conn.OpenAsync();

        string query = @"
            SELECT
                p.Apellido,
                p.Nombre,
                c.TipoCredito,
                c.observacion,
                c.MontoCredito,
                ISNULL(pagos.SumaPago, 0) AS SumaPago,
                (ISNULL(MAX(cu.ImporteCuota), 0) * c.NumCuotas) - ISNULL(pagos.SumaPago, 0) AS SaldoTotal,
                c.Id_Credito
            FROM Personas p
            INNER JOIN Creditos c ON p.Id_Personas = c.Id_Personas
            INNER JOIN Cuotas cu ON c.Id_Credito = cu.Id_Credito
            LEFT JOIN (
                SELECT cu.Id_Credito, SUM(pa.MontoPagado) AS SumaPago
                FROM Cuotas cu
                LEFT JOIN Pagos pa ON cu.Id_Cuota = pa.Id_Cuota
                GROUP BY cu.Id_Credito
            ) pagos ON c.Id_Credito = pagos.Id_Credito
            WHERE c.EstadoCredito = 'ACTIVO' 
            AND p.Id_Personas = @idPersona";

        if (!string.IsNullOrEmpty(formaPagoFilter) && formaPagoFilter != "Todos")
        {
            query += " AND c.FormaPago = @formaPago";
        }

        query += " GROUP BY p.Apellido, p.Nombre, c.TipoCredito, c.observacion, c.MontoCredito, c.NumCuotas, pagos.SumaPago, c.Id_Credito";

        using SqlCommand cmd = new(query, conn);
        cmd.Parameters.AddWithValue("@idPersona", idPersona);
        if (!string.IsNullOrEmpty(formaPagoFilter) && formaPagoFilter != "Todos")
        {
            cmd.Parameters.AddWithValue("@formaPago", formaPagoFilter);
        }

        using var reader = await cmd.ExecuteReaderAsync();
        Creditos.Clear();
        while (await reader.ReadAsync())
        {
            Creditos.Add(new CreditoResumen
            {
                Apellido = reader.GetString(0),
                Nombre = reader.GetString(1),
                TipoCredito = reader.GetString(2),
                Observacion = reader.GetString(3),
                MontoCredito = reader.GetDecimal(4),
                SumaPago = reader.GetDecimal(5),
                SaldoTotal = reader.GetDecimal(6),
                IdCredito = reader.GetInt64(7)
            });
        }
    }

    private void OnFormaPagoChanged(object sender, EventArgs e)
    {
        string selectedFormaPago = FormaPagoPicker.SelectedItem?.ToString();
        // No recargamos aquí, solo actualizamos al presionar el botón
    }

    private async void OnFilterClicked(object sender, EventArgs e)
    {
        string selectedFormaPago = FormaPagoPicker.SelectedItem?.ToString();
        await LoadCreditosAsync(selectedFormaPago);
    }

    private async void OnVerResumenClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is long idCredito)
        {
            await Navigation.PushAsync(new ResumenCtaPage(idCredito));
        }
    }

}

public class CreditoResumen
{
    public string Apellido { get; set; }
    public string Nombre { get; set; }
    public string TipoCredito { get; set; }
    public string Observacion { get; set; }
    public decimal MontoCredito { get; set; }
    public decimal SumaPago { get; set; }
    public decimal SaldoTotal { get; set; }
    public long IdCredito { get; set; }
}