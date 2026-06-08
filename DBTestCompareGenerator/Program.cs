// <copyright file="Program.cs" company="Objectivity Bespoke Software Specialists">
// Copyright (c) Objectivity Bespoke Software Specialists. All rights reserved.
// </copyright>

namespace DBTestCompareGenerator
{
    using NLog;

    internal static class Program
    {
        private static readonly Logger Logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();

        private static int Main(string[] args)
        {
            try
            {
                if (Configuration.UnpackDacpac)
                {
                    UnpackDacpac.ExtractDacpacFile();
                    UnpackDacpac.UnpackDacpacFile();
                }

                CopyConfigFiles.CopyConfigFile();
                var configFromExcel = ReadConfigurationFromXlsx.ReadExcelFile();
                CountQuerySqlServer.CreateCountQuery(configFromExcel);
                CompareQuerySqlServer.CreateCompareQuery(configFromExcel);
                return 0;
            }
            catch (System.Exception ex)
            {
                Logger.Fatal(ex, "Test generation failed.");
                return 1;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }
    }
}
