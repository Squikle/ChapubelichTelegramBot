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

        public static int MessagesCheckPeriod => 1; // how old message to check (in minutes)
        public static int StopGameDelay => 300000; // default: 300000 = 5min
        public static int RouletteAnimationDuration => 3000; // default: 3000 = 3sec
    }
}
