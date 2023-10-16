﻿using System;
using Windows.UI.Xaml.Data;
using CodeHub.Helpers;

namespace CodeHub.Converters
{
    class ColorStringToColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return GlobalHelper.GetSolidColorBrush((value as string)+"FF");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
