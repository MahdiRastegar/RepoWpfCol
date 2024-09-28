using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WpfCol
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MTMxM0AzMjM1MkUzMTJFMzlKK2QyalU0d1EyVUgxN0FFdUVENGdDYmY4UWEyZ2poeEhoSWlUcmFSd2JjPQ==");

            CultureInfo persianCulture = new CultureInfo("fa-IR");
            persianCulture.NumberFormat.DigitSubstitution = DigitShapes.NativeNational;

            Thread.CurrentThread.CurrentCulture = persianCulture;
            Thread.CurrentThread.CurrentUICulture= persianCulture;

            base.OnStartup(e);
        }
    }
}
