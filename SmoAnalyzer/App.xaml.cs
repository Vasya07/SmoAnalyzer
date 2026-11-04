using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace SmoAnalyzer
{
    public partial class App : Application
    {
        public App()
        {
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.InvariantCulture.IetfLanguageTag)));
        }
    }
}