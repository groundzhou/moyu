using System;

namespace SalaryTracker
{
    public class SalaryConfig
    {
        public decimal MonthlySalary { get; set; }
        public TimeSpan WorkStartTime { get; set; }
        public TimeSpan WorkEndTime { get; set; }

        public SalaryConfig()
        {
            MonthlySalary = 5000;
            WorkStartTime = new TimeSpan(9, 0, 0);
            WorkEndTime = new TimeSpan(18, 0, 0);
        }
    }
}
