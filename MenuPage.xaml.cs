namespace PrestamoApp;

public partial class MenuPage : ContentPage
{
	public MenuPage()
	{
		InitializeComponent();
	}



    private async void OnAltaClienteClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AltaClientePage());
    }

    private async void OnAltaCreditoClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AltaCreditoPage());
    }


    private async void OnPagoClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new IngresaPagoPage());
    }


    private async void OnRutaClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RutaPage());
    }

    private async void OnVerRutaClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new VerHojaRuta());
    }

    private async void MenuConsultaClick(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MenuConsulta());
    }

    private async void OnListadoClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ListadoClientesPages());

    }

    private async void OnModificacionesClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MenuModificaciones());
    }

}