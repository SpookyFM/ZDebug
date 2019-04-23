using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using ZDebug.UI.ViewModel;

namespace ZDebug.UI.Services
{

    internal class PropertyViewMultiValueConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var pvm = values[0] as PropertyViewModel;
            var pv = values[1] as PropertyView;
            if (pvm == null || pv == null)
            {
                return null;
            }
            var result = new KeyValuePair<PropertyViewModel, PropertyView>((PropertyViewModel)values[0], (PropertyView)values[1]);
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[0];
        }

    }

}