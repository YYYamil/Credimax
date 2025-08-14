using Microsoft.Maui.Controls;

namespace PrestamoApp;

public partial class ContabilidadPage : ContentPage
{
    public ContabilidadPage()
    {
        InitializeComponent();
        BindingContext = new MainViewModel();
    }
}