using Newtonsoft.Json.Linq;
using System;
using Windows.UI.Xaml.Data;

namespace JSEEN.Converters
{
    public class JsonValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is JProperty property)
            {
                return property.Value.Value<string>();
            }
            return "Conversion error";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
