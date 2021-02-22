using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DruIDSdk
{
    public static class AuxStrings
    {
        //JFT facilitar la comprobación de nulos para checkear la respuesta en json
        public static string ToSafeString(this object obj)
        {
            return (obj ?? string.Empty).ToString();
        }
    }
}
