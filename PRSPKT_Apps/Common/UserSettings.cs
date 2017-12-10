using System;
using System.Configuration;
using System.IO;
using System.Windows;

namespace PRSPKT_Apps.Common
{
    /// <summary>
    /// Store user settings to file and retrieve them
    /// </summary>
    public static class UserSettings
    {
        private static Configuration GetConfig()
        {
            string prspkt = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PRSPKT", "PRSPKT_Apps", "PRSPKT_Apps.config");
            ExeConfigurationFileMap configMap = new ExeConfigurationFileMap()
            {
                ExeConfigFilename = prspkt
            };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            return config;
        }


        public static string Get(string key)
        {
            try
            {
                Configuration config = GetConfig();
                if (config == null)
                {
                    return string.Empty;
                }

                KeyValueConfigurationElement element = config.AppSettings.Settings[key];
                if (element != null)
                {
                    string value = element.Value;
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                }
                else
                {
                    config.AppSettings.Settings.Add(key, "");
                    config.Save(ConfigurationSaveMode.Modified);
                }
            }
            catch (Exception ex1)
            {
                MessageBox.Show("exception: " + ex1);
            }
            return string.Empty;
        }

        public static string GetConfigPath()
        {
            return GetConfig().FilePath;
        }

        public static void Set(string key, string value)
        {
            try
            {
                Configuration config = GetConfig();
                if (config == null)
                {
                    return;
                }

                KeyValueConfigurationElement element = config.AppSettings.Settings[key];
                if (element != null)
                {
                    element.Value = value;
                }
                else
                {
                    config.AppSettings.Settings.Add(key, value);
                }
                config.Save(ConfigurationSaveMode.Modified);
            }
            catch (Exception ex1)
            {

                MessageBox.Show("exception: " + ex1);
            }
        }
    }
}
