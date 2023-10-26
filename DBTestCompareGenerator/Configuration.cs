// <copyright file="Configuration.cs" company="Objectivity Bespoke Software Specialists">
// Copyright (c) Objectivity Bespoke Software Specialists. All rights reserved.
// </copyright>

namespace DBTestCompareGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Configuration that consume settings file file.
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// Getting appsettings.json file.
        /// </summary>
        public static readonly IConfigurationRoot Builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .Build();

        /// <summary>
        /// NLog logger handle.
        /// </summary>
        private static readonly NLog.Logger Logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        /// <summary>
        /// Gets Connection String.
        /// </summary>
        public static string ConnectionString
        {
            get { return Configuration.Builder["appSettings:ConnectionStrings:DB"]; }
        }

        public static string DacpacFolder
        {
            get
            {
                string setting = null;
                setting = Configuration.Builder["appSettings:DacpacFolder"];
                Logger.Trace(CultureInfo.CurrentCulture, "DacpacFolder Folder value from settings file '{0}'", setting);
                return setting;
            }
        }

        public static string Database
        {
            get { return Configuration.Builder["appSettings:Database"]; }
        }

        public static string FolderPath
        {
            get
            {
                string setting = null;
                setting = Configuration.Builder["appSettings:Folder"];
                Logger.Trace(CultureInfo.CurrentCulture, "Folder value from settings file '{0}'", setting);
                return setting;
            }
        }

        public static bool ExtractAllTableData
        {
            get
            {
                var setting = Configuration.Builder["appSettings:ExtractAllTableData"];
                Logger.Trace(CultureInfo.CurrentCulture, "Read ExtractAllTableData '{0}'", setting);
                if (string.IsNullOrEmpty(setting))
                {
                    return false;
                }

                if (setting.ToLower(CultureInfo.CurrentCulture).Equals("true"))
                {
                    return true;
                }

                return false;
            }
        }

        public static bool ExtractApplicationScopedObjectsOnly
        {
            get
            {
                var setting = Configuration.Builder["appSettings:ExtractApplicationScopedObjectsOnly"];
                Logger.Trace(CultureInfo.CurrentCulture, "Read ExtractApplicationScopedObjectsOnly '{0}'", setting);
                if (string.IsNullOrEmpty(setting))
                {
                    return false;
                }

                if (setting.ToLower(CultureInfo.CurrentCulture).Equals("true"))
                {
                    return true;
                }

                return false;
            }
        }

        public static bool VerifyExtraction
        {
            get
            {
                var setting = Configuration.Builder["appSettings:VerifyExtraction"];
                Logger.Trace(CultureInfo.CurrentCulture, "Read VerifyExtraction'{0}'", setting);
                if (string.IsNullOrEmpty(setting))
                {
                    return false;
                }

                if (setting.ToLower(CultureInfo.CurrentCulture).Equals("true"))
                {
                    return true;
                }

                return false;
            }
        }

        public static bool IgnoreExtendedProperties
        {
            get
            {
                var setting = Configuration.Builder["appSettings:IgnoreExtendedProperties"];
                Logger.Trace(CultureInfo.CurrentCulture, "Read IgnoreExtendedProperties '{0}'", setting);
                if (string.IsNullOrEmpty(setting))
                {
                    return false;
                }

                if (setting.ToLower(CultureInfo.CurrentCulture).Equals("true"))
                {
                    return true;
                }

                return false;
            }
        }

        public static bool IgnorePermissions
        {
            get
            {
                var setting = Configuration.Builder["appSettings:IgnorePermissions"];
                Logger.Trace(CultureInfo.CurrentCulture, "Read IgnorePermissions '{0}'", setting);
                if (string.IsNullOrEmpty(setting))
                {
                    return false;
                }

                if (setting.ToLower(CultureInfo.CurrentCulture).Equals("true"))
                {
                    return true;
                }

                return false;
            }
        }

        public static bool SaveAsBaseline
        {
            get
            {
                var setting = Configuration.Builder["appSettings:SaveAsBaseline"];
                Logger.Trace(CultureInfo.CurrentCulture, "Read IgnorePermissions '{0}'", setting);
                if (string.IsNullOrEmpty(setting))
                {
                    return false;
                }

                if (setting.ToLower(CultureInfo.CurrentCulture).Equals("true"))
                {
                    return true;
                }

                return false;
            }
        }

        public static List<string> ColumnTypesToGroupBy
        {
            get
            {
                var columns = Configuration.Builder["appSettings:ColumnTypesToGroupBy"];
                List<string> columnTypes = new List<string>();
                foreach (var column in columns.Split(","))
                {
                    columnTypes.Add(column);
                }

                return columnTypes;
            }
        }

        public static string DBNameLiveMinusTests
        {
            get { return Configuration.Builder["appSettings:DBNameLiveMinusTests"]; }
        }

        public static string DBNameBranchMinusTests
        {
            get { return Configuration.Builder["appSettings:DBNameBranchMinusTests"]; }
        }

        public static bool ReadExcelFile
        {
            get
            {
                var setting = Configuration.Builder["appSettings:ReadExcelFile"];
                Logger.Trace(CultureInfo.CurrentCulture, "Read Excel File '{0}'", setting);
                if (string.IsNullOrEmpty(setting))
                {
                    return false;
                }

                if (setting.ToLower(CultureInfo.CurrentCulture).Equals("true"))
                {
                    return true;
                }

                return false;
            }
        }

        public static bool GenerateCountSmokeTests
        {
            get
            {
                var setting = Configuration.Builder["appSettings:GenerateCountSmokeTests"];
                Logger.Trace(CultureInfo.CurrentCulture, "Generate count smoke tests '{0}'", setting);
                if (string.IsNullOrEmpty(setting))
                {
                    return false;
                }

                if (setting.ToLower(CultureInfo.CurrentCulture).Equals("true"))
                {
                    return true;
                }

                return false;
            }
        }

        public static bool GenerateCompareFetchTests
        {
            get
            {
                var setting = Configuration.Builder["appSettings:GenerateCompareFetchTests"];
                Logger.Trace(CultureInfo.CurrentCulture, "Generate compare fetch tests '{0}'", setting);
                if (string.IsNullOrEmpty(setting))
                {
                    return false;
                }

                if (setting.ToLower(CultureInfo.CurrentCulture).Equals("true"))
                {
                    return true;
                }

                return false;
            }
        }

        public static bool GenerateCompareMinusTests
        {
            get
            {
                var setting = Configuration.Builder["appSettings:GenerateCompareMinusTests"];
                Logger.Trace(CultureInfo.CurrentCulture, "Generate compare minusT tests '{0}'", setting);
                if (string.IsNullOrEmpty(setting))
                {
                    return false;
                }

                if (setting.ToLower(CultureInfo.CurrentCulture).Equals("true"))
                {
                    return true;
                }

                return false;
            }
        }
    }
}
