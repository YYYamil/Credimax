namespace PrestamoApp;

public partial class RutaPage : ContentPage
{
	public RutaPage()
	{
		InitializeComponent();
	}


    private async void OnZonaClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            string zona = button.Text;
            await Navigation.PushAsync(new ZonaPage(zona));
        }
    }


}