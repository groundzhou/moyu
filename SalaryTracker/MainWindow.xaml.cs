using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SalaryTracker
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer uiTimer;
        private DispatcherTimer salaryTimer;
        private SalaryConfig config;
        private ConfigHelper configHelper;

        public MainWindow()
        {
            InitializeComponent();
            configHelper = new ConfigHelper();
            config = configHelper.LoadConfig();

            InitializeTimers();
        }

        private void InitializeTimers()
        {
            // UI更新定时器（每秒更新）
            uiTimer = new DispatcherTimer();
            uiTimer.Interval = TimeSpan.FromSeconds(1);
            uiTimer.Tick += UiTimer_Tick;
            uiTimer.Start();

            // 工资计算定时器（每3秒更新）
            salaryTimer = new DispatcherTimer();
            salaryTimer.Interval = TimeSpan.FromSeconds(3);
            salaryTimer.Tick += SalaryTimer_Tick;
            salaryTimer.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            UpdateCurrentTime();
            UpdateWorkStatus();
        }

        private void SalaryTimer_Tick(object sender, EventArgs e)
        {
            UpdateSalaryData();
        }

        private void UpdateCurrentTime()
        {
            txtCurrentTime.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        private void UpdateWorkStatus()
        {
            var now = DateTime.Now.TimeOfDay;
            var status = GetWorkStatus(now);

            txtWorkStatus.Text = status.Item1;
            txtWorkStatus.Foreground = new SolidColorBrush(status.Item2);
        }

        private Tuple<string, Color> GetWorkStatus(TimeSpan currentTime)
        {
            if (currentTime >= config.WorkStartTime && currentTime <= config.WorkEndTime)
            {
                return new Tuple<string, Color>("正在上班中", Colors.Green);
            }
            else if (currentTime > config.WorkEndTime)
            {
                return new Tuple<string, Color>("已下班", Colors.Blue);
            }
            else
            {
                return new Tuple<string, Color>("等待上班", Colors.Orange);
            }
        }

        private void UpdateSalaryData()
        {
            var now = DateTime.Now;
            var today = now.Date;

            // 计算今日收入和工作时长
            var todayIncome = CalculateTodayIncome(now);
            var todayWorkHours = CalculateTodayWorkHours(now);
            var workProgress = CalculateWorkProgress(now);

            // 计算时薪
            var dailyWorkHours = (config.WorkEndTime - config.WorkStartTime).TotalHours;
            var dailySalary = config.MonthlySalary / GetWorkDaysInMonth(now.Year, now.Month);
            var hourlyRate = dailySalary / (decimal)dailyWorkHours;

            // 计算本月数据
            var monthlyData = CalculateMonthlyData(now);

            // 更新UI
            txtTodayIncome.Text = string.Format("今日已赚取 ￥{0:F2}", todayIncome);
            txtWorkHours.Text = string.Format("{0:F1} 小时", todayWorkHours);
            txtHourlyRate.Text = string.Format("￥{0:F0}", hourlyRate);
            txtWorkDays.Text = string.Format("{0}/{1}", monthlyData.Item1, monthlyData.Item2);
            txtMonthlyIncome.Text = string.Format("￥{0:F2}", monthlyData.Item3);
            progressBar.Value = workProgress * 100;
        }

        private decimal CalculateTodayIncome(DateTime now)
        {
            var currentTime = now.TimeOfDay;
            var dailyWorkHours = (config.WorkEndTime - config.WorkStartTime).TotalHours;
            var dailySalary = config.MonthlySalary / GetWorkDaysInMonth(now.Year, now.Month);

            if (currentTime < config.WorkStartTime)
            {
                return 0;
            }

            var workEndTime = currentTime > config.WorkEndTime ? config.WorkEndTime : currentTime;
            var workedHours = (workEndTime - config.WorkStartTime).TotalHours;

            return dailySalary * (decimal)(workedHours / dailyWorkHours);
        }

        private double CalculateTodayWorkHours(DateTime now)
        {
            var currentTime = now.TimeOfDay;

            if (currentTime < config.WorkStartTime)
            {
                return 0;
            }

            var workEndTime = currentTime > config.WorkEndTime ? config.WorkEndTime : currentTime;
            return (workEndTime - config.WorkStartTime).TotalHours;
        }

        private double CalculateWorkProgress(DateTime now)
        {
            var currentTime = now.TimeOfDay;
            var totalWorkTime = (config.WorkEndTime - config.WorkStartTime).TotalHours;

            if (currentTime < config.WorkStartTime)
            {
                return 0;
            }

            if (currentTime > config.WorkEndTime)
            {
                return 1;
            }

            var workedTime = (currentTime - config.WorkStartTime).TotalHours;
            return workedTime / totalWorkTime;
        }

        private Tuple<int, int, decimal> CalculateMonthlyData(DateTime now)
        {
            var totalWorkDays = GetWorkDaysInMonth(now.Year, now.Month);
            var workedDays = GetWorkedDaysThisMonth(now);
            var dailySalary = config.MonthlySalary / totalWorkDays;
            var monthlyIncome = dailySalary * workedDays;

            return new Tuple<int, int, decimal>(workedDays, totalWorkDays, monthlyIncome);
        }

        private int GetWorkDaysInMonth(int year, int month)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var workDays = 0;

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    workDays++;
                }
            }

            return workDays;
        }

        private int GetWorkedDaysThisMonth(DateTime now)
        {
            var workedDays = 0;

            for (int day = 1; day <= now.Day; day++)
            {
                var date = new DateTime(now.Year, now.Month, day);
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    if (date.Date < now.Date || (date.Date == now.Date && now.TimeOfDay > config.WorkStartTime))
                    {
                        workedDays++;
                    }
                }
            }

            return workedDays;
        }

        private void UpdateUI()
        {
            UpdateCurrentTime();
            UpdateWorkStatus();
            UpdateSalaryData();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void MenuResetSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            if (settingsWindow.ShowDialog() == true)
            {
                config = configHelper.LoadConfig();
                UpdateUI();
            }
        }

        private void MenuTopmost_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
            menuTopmost.Header = this.Topmost ? "取消置顶" : "置顶";
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (uiTimer != null)
            {
                uiTimer.Stop();
                uiTimer = null;
            }

            if (salaryTimer != null)
            {
                salaryTimer.Stop();
                salaryTimer = null;
            }

            base.OnClosed(e);
        }
    }

    public class Tuple<T1>
    {
        public Tuple(T1 item1)
        {
            Item1 = item1;
        }

        public T1 Item1 { get; set; }
    }

    public class Tuple<T1, T2> : Tuple<T1>
    {
        public Tuple(T1 item1, T2 item2) : base(item1)
        {
            Item2 = item2;
        }

        public T2 Item2 { get; set; }
    }

    public class Tuple<T1, T2, T3> : Tuple<T1, T2>
    {
        public Tuple(T1 item1, T2 item2, T3 item3) : base(item1, item2)
        {
            Item3 = item3;
        }

        public T3 Item3 { get; set; }
    }

    public static class Tuple
    {
        public static Tuple<T1> Create<T1>(T1 item1)
        {
            return new Tuple<T1>(item1);
        }

        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Tuple<T1, T2>(item1, item2);
        }

        public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
        {
            return new Tuple<T1, T2, T3>(item1, item2, item3);
        }
    }
}
