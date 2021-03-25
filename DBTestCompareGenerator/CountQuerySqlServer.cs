// <copyright file="CountQuerySqlServer.cs" company="Objectivity Bespoke Software Specialists">
// Copyright (c) Objectivity Bespoke Software Specialists. All rights reserved.
// </copyright>

namespace DBTestCompareGenerator
{
    using System.Collections.Generic;

    public static class CountQuerySqlServer
    {
        private static readonly NLog.Logger Logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        public static void CreateCountQuery(List<Dictionary<string, string>> configList)
        {
            if (Configuration.GenerateCountSmokeTests)
            {
                var data = TablesDefinitions.GetTablesDefinitions();

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    var tableSchemaIteration = CompareQuerySqlServer.TableSchemaIteration(data, i, out var tableNameIteration);
                    var (createTest, domain, comment, whereClause, orderByCluse, aggregateByClause) = ReadConfigurationFromXlsx.CheckIfTableInExcel(configList, tableSchemaIteration, tableNameIteration);
                    if (createTest)
                    {
                        Logger.Debug("Skip iteration");
                        continue;
                    }

                    if (i + 1 < data.Rows.Count && tableNameIteration == data.Rows[i + 1]["TABLE_NAME"].ToString() &&
                        tableSchemaIteration == data.Rows[i + 1]["TABLE_SCHEMA"].ToString())
                    {
                        Logger.Debug("Skip iteration");
                        continue;
                    }

                    var folder = CopyConfigFiles.CreateFolderForTest(tableSchemaIteration, tableNameIteration, "1.Count");
                    var countQuery = TablesDefinitions.CreateSmokeCountQuery(tableSchemaIteration, tableNameIteration);
                    CopyConfigFiles.CreateTestDefinitions(folder, countQuery, tableSchemaIteration, tableNameIteration, "Smoke_", "ActualTable.sql", "from_file_sql.xml");
                    CopyConfigFiles.CreateTestDefinitions(folder, countQuery, tableSchemaIteration, tableNameIteration, "Smoke_", "ExpectedTable.sql");
                }
            }
        }
    }
}
