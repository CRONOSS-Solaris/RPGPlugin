using System.Threading;
using System.Windows.Controls;

namespace RPGPlugin.View
{
    public partial class Hunter : UserControl, ViewBase
    {
        public Hunter()
        {
            InitializeComponent();
        }


        public void LogThread()
        {
            Roles.Log.Warn($"HunterView Thread => {Thread.CurrentThread.ManagedThreadId}");
        }
    }
}