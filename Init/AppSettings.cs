using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChapubelichBot.Init
{
    class AppSettings
    {
        public static string Url => "";

#if (DEBUG)
        public static string Key => "1365901559:AAFHWZlzG6SBHz9NLaOlEvx-Robel1d6V1Q";
#else
        public static string Key => "1144819840:AAGyGnkSkzQrovOQTaeLTm4Jrt5cbsUEdFM";
#endif

        public static string Name => "Chapubelich";

        public static int MessagesPeriod => 1;
    }
}
