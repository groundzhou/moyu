using System;
using System.Globalization;
using System.Windows;

namespace SalaryTracker
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var config = new SalaryConfig();

                // 验证月工资
                if (!decimal.TryParse(txtMonthlySalary.Text, out decimal salary) || salary <= 0)
                {
                    MessageBox.Show("请输入有效的月工资金额", "输入错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                config.MonthlySalary = salary;

                // 验证上班时间
                if (!TimeSpan.TryParse(txtWorkStart.Text, out TimeSpan startTime))
                {
                    MessageBox.Show("请输入有效的上班时间格式（如：09:00）", "输入错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                config.WorkStartTime = startTime;

                // 验证下班时间
                if (!TimeSpan.TryParse(txtWorkEnd.Text, out TimeSpan endTime))
                {
                    MessageBox.Show("请输入有效的下班时间格式（如：18:00）", "输入错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                config.WorkEndTime = endTime;

                // 验证时间逻辑
                if (startTime >= endTime)
                {
                    MessageBox.Show("下班时间必须晚于上班时间", "时间设置错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 保存配置
                var configHelper = new ConfigHelper();
                configHelper.SaveConfig(config);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存配置时发生错误：" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}