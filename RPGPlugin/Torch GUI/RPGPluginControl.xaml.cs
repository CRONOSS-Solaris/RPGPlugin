using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using RPGPlugin.Utils;
using Timer = System.Timers.Timer;
using RPGPlugin.View;
using Torch.Collections;

namespace RPGPlugin
{
    public partial class RolesControl : UserControl
    {
        private Roles Plugin { get; }
        private static List<TabItem> RoleViews = new List<TabItem>();

        private RolesControl()
        {
            InitializeComponent();
        }

        public RolesControl(Roles plugin) : this()
        {
            Plugin = plugin;
            DataContext = this;
            BaseSaveLocation.DataContext = plugin.Config;
            SettingsTab.DataContext = plugin.Config;
            Roles.Log.Warn($"Main View Thread => {Thread.CurrentThread.ManagedThreadId}");
            
            RoleViews = RoleAgent.GetRoleViews();
            foreach (var roleView in RoleViews)
            {
                SettingsTab.Items.Add(roleView);
            }
        }

        public void Donate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.paypal.com/donate/?hosted_button_id=HCV9695KQDMFN");
        }

        private void SupportButton_Click(object sender, RoutedEventArgs e)
        {
            string discordInviteLink = "https://discord.com/invite/TqbCaHu7wr";
            Process.Start(new ProcessStartInfo
            {
                FileName = discordInviteLink,
                UseShellExecute = true
            });
        }

        private void BroadcastLevelUpToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Plugin.Config.BroadcastLevelUp = true;
        }

        private void BroadcastLevelUpToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Plugin.Config.BroadcastLevelUp = false;
        }
    }
}
