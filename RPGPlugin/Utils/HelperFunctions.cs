using System;
using System.Threading;

namespace RPGPlugin.Utils
{
    public static class StaticHelperFunctions
    {
        public static void StaThreadWrapper(Action action)
        {
            Thread t = new Thread(o =>
            {
                action();
                System.Windows.Threading.Dispatcher.Run();
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }
    }
}