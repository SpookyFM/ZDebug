using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using ZDebug.UI.ViewModel;

namespace ZDebug.UI.Services
{

    internal class VariableViewMultiValueConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(values[0] is VariableViewModel))
            {
                return null;
            }
            if (!(values[1] is VariableView))
            {
                return null;
            }
            var result = new KeyValuePair<VariableViewModel, VariableView>((VariableViewModel)values[0], (VariableView)values[1]);
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[0];
        }

    }

}