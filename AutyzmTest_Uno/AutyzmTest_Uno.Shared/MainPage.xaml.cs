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

        // public static Brush backgrnd;  // przekazywanie do konwertera

        private VBlib.MainPage inVb = new VBlib.MainPage();

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
            if (inVb.CzyBrakujeOdpowiedzi())
            {
                await vb14.DialogBoxResAsync("msgBrakAnswer");
                uiList.ItemsSource = null;
                vb14.SetSettingsBool("initialMode", false);
                uiList.ItemsSource = (from c in inVb.moListPytania select c).ToArray();
                return;
            }

            int iWynik = inVb.PoliczWynik();
            string sSummary = inVb.GetResultSummaryText(iWynik);
            await vb14.DialogBoxAsync(sSummary);
            string sResult = inVb.GetHeaderText() + sSummary + inVb.GetResultText();

            // wynik do pliku
            inVb.SaveResult(sResult);
            if (await vb14.DialogBoxResYNAsync("msgAskEmail"))
            {
                await SendEmailResult(sResult);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await vb14.DialogBoxResAsync("msgInfo");
            // backgrnd = uiTitle.ba
            vb14.SetSettingsBool("initialMode", true);
            inVb.StworzPytania();
            uiList.ItemsSource = (from c in inVb.moListPytania select c).ToArray();
        }

    }

    #region "XAML converters"

    public partial class KonwersjaWarningBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // wejście: value = wartość Slidera
            // SettingsBool("initialMode")
            // zwraca:  Background

            int iVal = System.Convert.ToInt32(value);

            if (iVal == 0 && !vb14.GetSettingsBool("initialMode"))
                return new SolidColorBrush(Windows.UI.Colors.OrangeRed);

            return new SolidColorBrush(Windows.UI.Colors.Gray);
        }

        // ConvertBack is not implemented for a OneWay binding.
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class KonwersjaWarningOpacity : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // wejście: value = wartość Slidera
            // SettingsBool("initialMode")
            // zwraca:  Opacity

            int iVal = System.Convert.ToInt32(value);

            if (iVal == 0 && vb14.GetSettingsBool("initialMode"))
                //if (iVal == 0)
                return 0.5;

            return 1;
        }

        // ConvertBack is not implemented for a OneWay binding.
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class KonwersjaVisibility : IValueConverter
    {
        // tylko dla Android, gdzie nie działa Background
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // wejście: value = wartość Slidera
            // SettingsBool("initialMode")
            // zwraca:  Opacity

            int iVal = System.Convert.ToInt32(value);

            //if (iVal == 0 && vb14.GetSettingsBool("initialMode"))
                return Visibility.Collapsed;

            //return Visibility.Visible;
        }

        // ConvertBack is not implemented for a OneWay binding.
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
