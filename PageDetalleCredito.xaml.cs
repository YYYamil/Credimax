namespace PrestamoApp;
using Microsoft.Maui.Controls;

public partial class PageDetalleCredito : ContentPage
{
	public PageDetalleCredito()
	{
		InitializeComponent();
	}

  /*  public PageDetalleCredito(CreditoMock credito)
    {
        InitializeComponent();
        BindingContext = credito;
    }*/


    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }


}



