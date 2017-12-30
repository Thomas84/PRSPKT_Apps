// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

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
        private const string DefaultUserCoef1 = "0,5";
        private const string DefaultUserCoef2 = "0,3";
        private const string DefaultUserCoef3 = "0,3";
        private const string DefaultType = "П_Тип помещения";
        private const string DefaultAreaLiving = "Площадь квартиры Жилая";
        private const string DefaultArea = "Площадь квартиры";
        private const string DefaultRoomsName = "П_Имя помещения";
        private const string DefaultAreaCommon = "Площадь квартиры Общая";
        private const string DefaultAreaWithCoef = "Площадь с коэффициентом";
        private const string DefaultRoomsCount = "Число комнат";
        private const string DefaultRoomsNumber = "П_Номер квартиры";
        private const string DefaultApartmentRooms = "Кухня, Гостиная, Спальня, Коридор, Терраса, С/у, Ванная, Балкон, Лоджия, Кладовая, Веранда";
        private const string DefaultApartmentLivingRooms = "Гостиная, Спальня, Спальная, Общая комната, Жилая комната";
        private const string DefaultRoundNumber = "2";

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
                    if (string.IsNullOrEmpty(value) && key == "userCoef1")
                        return DefaultUserCoef1;
                    if (string.IsNullOrEmpty(value) && key == "userCoef2")
                        return DefaultUserCoef2;
                    if (string.IsNullOrEmpty(value) && key == "userCoef3")
                        return DefaultUserCoef3;
                    if (string.IsNullOrEmpty(value) && key == "type")
                        return DefaultType;
                    if (string.IsNullOrEmpty(value) && key == "area_living")
                        return DefaultAreaLiving;
                    if (string.IsNullOrEmpty(value) && key == "area")
                        return DefaultArea;
                    if (string.IsNullOrEmpty(value) && key == "area_common")
                        return DefaultAreaCommon;
                    if (string.IsNullOrEmpty(value) && key == "area_w_coef")
                        return DefaultAreaWithCoef;
                    if (string.IsNullOrEmpty(value) && key == "rooms_count")
                        return DefaultRoomsCount;
                    if (string.IsNullOrEmpty(value) && key == "rooms_number")
                        return DefaultRoomsNumber;
                    if (string.IsNullOrEmpty(value) && key == "apartment_rooms")
                        return DefaultApartmentRooms;
                    if (string.IsNullOrEmpty(value) && key == "apartment_living_rooms")
                        return DefaultApartmentLivingRooms;
                    if (string.IsNullOrEmpty(value) && key == "roundNumber")
                        return DefaultRoundNumber;
                    if (string.IsNullOrEmpty(value) && key == "rooms_name")
                        return DefaultRoomsName;
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
