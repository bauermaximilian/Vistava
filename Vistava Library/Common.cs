using System;
using System.IO;
using System.Reflection;

namespace Vistava.Library
{
    public static class Common
    {
        public const string ApplicationDataFolder = "Vistava";

        public const string ConfigurationFileName = "config.xml";

        public static string ConfigurationFilePath
        {
            get
            {
                string configurationFolder = Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.ApplicationData),
                    ApplicationDataFolder);

                return Path.Combine(configurationFolder, 
                    ConfigurationFileName);
            }
        }

        public static string LibraryFullName { get; } =
            Assembly.GetExecutingAssembly().GetName().Name + " " +
            Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}
