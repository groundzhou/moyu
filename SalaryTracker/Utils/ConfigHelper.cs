using System;
using System.IO;
using System.Text;
using System.Xml;

namespace SalaryTracker
{
    public class ConfigHelper
    {
        private readonly string configPath;

        public ConfigHelper()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "SalaryTracker");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            configPath = Path.Combine(appFolder, "config.xml");
        }

        public bool ConfigExists()
        {
            return File.Exists(configPath);
        }

        public void SaveConfig(SalaryConfig config)
        {
            try
            {
                using (var writer = new XmlTextWriter(configPath, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    writer.WriteStartDocument();
                    writer.WriteStartElement("SalaryConfig");

                    writer.WriteElementString("MonthlySalary", config.MonthlySalary.ToString());
                    writer.WriteElementString("WorkStartTime", config.WorkStartTime.ToString());
                    writer.WriteElementString("WorkEndTime", config.WorkEndTime.ToString());

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("保存配置失败: " + ex.Message);
            }
        }

        public SalaryConfig LoadConfig()
        {
            var config = new SalaryConfig();

            if (!File.Exists(configPath))
            {
                return config;
            }

            try
            {
                var doc = new XmlDocument();
                doc.Load(configPath);

                var salaryNode = doc.SelectSingleNode("//MonthlySalary");
                if (salaryNode != null)
                {
                    config.MonthlySalary = decimal.Parse(salaryNode.InnerText);
                }

                var startTimeNode = doc.SelectSingleNode("//WorkStartTime");
                if (startTimeNode != null)
                {
                    config.WorkStartTime = TimeSpan.Parse(startTimeNode.InnerText);
                }

                var endTimeNode = doc.SelectSingleNode("//WorkEndTime");
                if (endTimeNode != null)
                {
                    config.WorkEndTime = TimeSpan.Parse(endTimeNode.InnerText);
                }
            }
            catch (Exception)
            {
                return new SalaryConfig();
            }

            return config;
        }
    }
}