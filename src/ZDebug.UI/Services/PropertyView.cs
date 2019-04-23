using System;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;
using ZDebug.Core.Basics;
using ZDebug.UI.ViewModel;

namespace ZDebug.UI.Services
{
    
    public class PropertyView 
    {

        public PropertyView(string id, string displayName, Func<byte[], byte[], string> convertToString)
        {
            ID = id;
            DisplayName = displayName;
            ConvertToString = convertToString;
        }

        public Func<byte[], byte[], string> ConvertToString
        {
            get;
            private set;
        }

        public String ID
        {
            get;
            private set;
        }

        public String DisplayName
        {
            get;
            private set;
        } 
        
    }
}
