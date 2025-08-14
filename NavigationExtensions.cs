using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrestamoApp
{
    public static class NavigationExtensions
    {


        public static Task<bool> PushAsyncWithResult(this INavigation navigation, IngresarPago page)
        {
            var tcs = new TaskCompletionSource<bool>();

            page.PagoFinalizado += (sender, exito) =>
            {
                tcs.SetResult(exito);
            };

            navigation.PushAsync(page);
            return tcs.Task;
        }


    }
}
