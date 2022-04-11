using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using vb14 = VBlib.pkarlibmodule14;

// z wersji VB, 2022.03.24
// aktualizacja z VB (po pelniejszym VBlib), 2022.04.06

namespace AutyzmTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async System.Threading.Tasks.Task SendEmailResult(string sResult)
        {
            Windows.ApplicationModel.Email.EmailMessage oMsg = new Windows.ApplicationModel.Email.EmailMessage();
            oMsg.Subject = vb14.GetLangString("msgEmailSubject");
            oMsg.Body = sResult;
            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(oMsg);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private async void uiWynikuj_Click(object sender, RoutedEventArgs e)
        {
            // sprawdz czy są jakies "0", jesli tak, to stop
            if (VBlib.MainPage.CzyBrakujeOdpowiedzi())
            {
                await vb14.DialogBoxResAsync("msgBrakAnswer");
                uiList.ItemsSource = null;
                uiList.ItemsSource = (from c in VBlib.MainPage.moListPytania select c).ToArray();
                return;
            }

            int iWynik = VBlib.MainPage.PoliczWynik();
            string sSummary = VBlib.MainPage.GetResultSummaryText(iWynik);
            await vb14.DialogBoxAsync(sSummary);
            string sResult = sSummary + VBlib.MainPage.GetResultText();

            // wynik do pliku
            VBlib.MainPage.SaveResult(sResult);
            if (await vb14.DialogBoxResYNAsync("msgAskEmail"))
            {
                await SendEmailResult(sResult);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await vb14.DialogBoxResAsync("msgInfo");
            VBlib.MainPage.StworzPytania();
            uiList.ItemsSource = (from c in VBlib.MainPage.moListPytania select c).ToArray();
        }

    }


    public partial class KonwersjaVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool bTemp = System.Convert.ToBoolean(value);

            if (bTemp)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        // ConvertBack is not implemented for a OneWay binding.
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    //public partial class KonwersjaKolor : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, string language)
    //    {
    //        double bTemp = (double)value;
    //        if (bTemp == 0d)
    //            return new SolidColorBrush(Windows.UI.Colors.Red);
    //        return new SolidColorBrush(Windows.UI.Colors.Blue);
    //    }

    //    // ConvertBack is not implemented for a OneWay binding.
    //    public object ConvertBack(object value, Type targetType, object parameter, string language)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

}
