﻿using Tizen.Applications;
using Uno.UI.Runtime.Skia;

namespace AutyzmTest_Uno.Skia.Tizen
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = new TizenHost(() => new AutyzmTest_Uno.App(), args);
            host.Run();
        }
    }
}
