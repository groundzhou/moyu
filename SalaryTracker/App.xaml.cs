using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace SalaryTracker
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 检查是否存在配置文件，如果不存在则显示设置窗口
            var configHelper = new ConfigHelper();
            if (!configHelper.ConfigExists())
            {
                var settingsWindow = new SettingsWindow();
                if (settingsWindow.ShowDialog() == true)
                {
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                }
                else
                {
                    this.Shutdown();
                }
            }
            else
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }
    }
}
