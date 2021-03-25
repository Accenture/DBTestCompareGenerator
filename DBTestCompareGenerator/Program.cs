// <copyright file="Program.cs" company="Objectivity Bespoke Software Specialists">
// Copyright (c) Objectivity Bespoke Software Specialists. All rights reserved.
// </copyright>

namespace DBTestCompareGenerator
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            CopyConfigFiles.CopyConfigFile();
            var configFromExcel = ReadConfigurationFromXlsx.ReadExcelFile();
            CountQuerySqlServer.CreateCountQuery(configFromExcel);
            CompareQuerySqlServer.CreateCompareQuery(configFromExcel);
        }
    }
}
