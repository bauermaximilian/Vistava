// SPDX-License-Identifier: GPL-3.0-or-later

using System.Reflection;

namespace Vistava.Service.Utils;

public static class AppPathsHelper
{
   public const string HttpsCertificateFileName = "https.pfx";
   public const string MediaCacheFileName = "cache.temp";
   public const string IncludeDirectoryName = "include";
   public const string ConfigurationsDirectoryName = "Configurations";

   public static string GenerateIncludePath()
   {
      return Path.Combine(GenerateAppDataPath(), IncludeDirectoryName);
   }

   public static string GenerateHttpsCertificatePath()
   {
      return Path.Combine(GenerateAppDataPath(), HttpsCertificateFileName);
   }

   public static string GenerateMediaCachePath()
   {
      return Path.Combine(GenerateAppDataPath(), MediaCacheFileName);
   }

   public static string GenerateAppDataPath()
   {
      var applicationVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(2) ?? "0.0";
      return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
         "vistava", "Service", $"v{applicationVersion}");
   }
}