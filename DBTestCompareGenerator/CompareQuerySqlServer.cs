// <copyright file="CompareQuerySqlServer.cs" company="Objectivity Bespoke Software Specialists">
// Copyright (c) Objectivity Bespoke Software Specialists. All rights reserved.
// </copyright>

namespace DBTestCompareGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    internal static class CompareQuerySqlServer
    {
        private static readonly NLog.Logger Logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        public static void CreateCompareQuery(List<Dictionary<string, string>> configList)
        {
            DataTable data = new DataTable();
            if (Configuration.GenerateCompareFetchTests || Configuration.GenerateCompareMinusTests)
            {
                 data = TablesDefinitions.GetTablesDefinitions();
            }

            var columnsDictionary = new List<Tuple<string, string>>();

            for (int i = 0; i < data.Rows.Count; i++)
            {
                var tableSchemaIteration = data.Rows[i]["TABLE_SCHEMA"].ToString();
                var tableNameIteration = data.Rows[i]["TABLE_NAME"].ToString();
                var (createTest, domain, comment, whereClause, orderByCluse, aggregateByClause) = ReadConfigurationFromXlsx.CheckIfTableInExcel(configList, tableSchemaIteration, tableNameIteration);
                if (createTest)
                {
                    continue;
                }

                columnsDictionary.Add(Tuple.Create(data.Rows[i]["COLUMN_NAME"].ToString(), data.Rows[i]["DATA_TYPE"].ToString()));
                string nextTable = null;
                string nextSchema = null;
                if (i + 1 < data.Rows.Count)
                {
                    nextTable = data.Rows[i + 1]["TABLE_NAME"].ToString();
                    nextSchema = data.Rows[i + 1]["TABLE_SCHEMA"].ToString();
                }

                if (tableNameIteration != nextTable || tableSchemaIteration != nextSchema)
                {
                    var queryDefinition = CreateQuery(tableSchemaIteration, tableNameIteration, columnsDictionary, comment, whereClause, orderByCluse, aggregateByClause);

                    for (int j = 0; j < queryDefinition.Count; j++)
                    {
                        if (!queryDefinition[j].QueryAggregate)
                        {
                            if (Configuration.GenerateCompareFetchTests)
                            {
                                var folderFetch = CopyConfigFiles.CreateFolderForTest(tableSchemaIteration, tableNameIteration, "2.RowByRow");
                                CopyConfigFiles.CreateTestDefinitions(folderFetch, queryDefinition[j].QueryFetchBranch, tableSchemaIteration, tableNameIteration, "Fetch_", "ActualTable.sql", "from_file_sql.xml");
                                CopyConfigFiles.CreateTestDefinitions(folderFetch, queryDefinition[j].QueryFetchLive, tableSchemaIteration, tableNameIteration, "Fetch_", "ExpectedTable.sql");
                            }

                            if (Configuration.GenerateCompareMinusTests)
                            {
                                var folderMinus = CopyConfigFiles.CreateFolderForTest(tableSchemaIteration, tableNameIteration, "3.MinusCompare");
                                CopyConfigFiles.CreateTestDefinitions(folderMinus, queryDefinition[j].QueryMinusBranch, tableSchemaIteration, tableNameIteration, "Minus_", "ActualTable.sql", "minus_from_file_sql.xml");
                                CopyConfigFiles.CreateTestDefinitions(folderMinus, queryDefinition[j].QueryMinusLive, tableSchemaIteration, tableNameIteration, "Minus_", "ExpectedTable.sql");
                            }
                        }
                        else
                        {
                            if (Configuration.GenerateCompareFetchTests)
                            {
                                var folderFetch = CopyConfigFiles.CreateFolderForTest(tableSchemaIteration, tableNameIteration, "4.FetchAggregateGroupBy", queryDefinition[j].ColumnName);
                                CopyConfigFiles.CreateTestDefinitions(folderFetch, queryDefinition[j].QueryFetchBranch, tableSchemaIteration, tableNameIteration, "Fetch_", "ActualTable.sql", "from_file_sql.xml");
                                CopyConfigFiles.CreateTestDefinitions(folderFetch, queryDefinition[j].QueryFetchLive, tableSchemaIteration, tableNameIteration, "Fetch_", "ExpectedTable.sql");
                            }

                            if (Configuration.GenerateCompareMinusTests)
                            {
                                var folderMinus = CopyConfigFiles.CreateFolderForTest(tableSchemaIteration, tableNameIteration, "5.MinusAggregateGroupBy", queryDefinition[j].ColumnName);
                                CopyConfigFiles.CreateTestDefinitions(folderMinus, queryDefinition[j].QueryMinusBranch, tableSchemaIteration, tableNameIteration, "Minus_", "ActualTable.sql", "minus_from_file_sql.xml");
                                CopyConfigFiles.CreateTestDefinitions(folderMinus, queryDefinition[j].QueryMinusLive, tableSchemaIteration, tableNameIteration, "Minus_", "ExpectedTable.sql");
                            }
                        }
                    }

                    columnsDictionary.Clear();
                }
            }
        }

        private static List<QueryDefinition> CreateQuery(string schema, string table, List<Tuple<string, string>> columns, string comment, string whereClause, string orderByCluse, string aggregateByClause)
        {
            string columnList = null;

            var commands = new List<QueryDefinition>();

            if (!string.IsNullOrEmpty(aggregateByClause))
            {
                var query = QueryAggregated(schema, table, aggregateByClause);
                commands.Add(query);
                query.PrintQueries();
                for (int i = 0; i < columns.Count; i++)
                {
                    if (Configuration.ColumnTypesToGroupBy.Contains(columns[i].Item2))
                    {
                        string where = null;
                        columnList = $"{columns[i].Item1}";

                        if (!string.IsNullOrEmpty(whereClause))
                        {
                            where = $"{Environment.NewLine}where {Environment.NewLine} {whereClause}{Environment.NewLine}";
                        }

                        query = QueryDefinitionGroupBy(schema, table, columnList, where);
                        commands.Add(query);
                        query.PrintQueries();
                    }
                }
            }
            else
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    columnList += $"\"{columns[i].Item1}\"";
                    if (i < columns.Count - 1)
                    {
                        columnList += " ,";
                    }

                    columnList += $" {Environment.NewLine}";
                }

                string addComment = null;
                if (!string.IsNullOrEmpty(comment))
                {
                    addComment = $"{Environment.NewLine} --{comment}";
                }

                string addWhereClause = null;
                if (!string.IsNullOrEmpty(whereClause))
                {
                    addWhereClause = $"{Environment.NewLine}where {Environment.NewLine} {whereClause}";
                }

                string addOrderByCluse = null;
                if (!string.IsNullOrEmpty(orderByCluse))
                {
                    addOrderByCluse = $"{Environment.NewLine}order by {orderByCluse}";
                }
                else
                {
                    addOrderByCluse = $"{Environment.NewLine}order by {columns[0].Item1}";
                }

                var query = QueryDefinition(schema, table, columnList, addWhereClause, addOrderByCluse, addComment);
                commands.Add(query);
                query.PrintQueries();
            }

            return commands;
        }

        private static QueryDefinition QueryAggregated(string schema, string table, string aggregateByClause)
        {
            var query = new QueryDefinition
            {
                QueryFetchLive = $"SELECT {Environment.NewLine}" +
                                 $" {aggregateByClause}{Environment.NewLine}" +
                                 $"FROM {Environment.NewLine}" +
                                 $" {schema}.{table}",
                QueryFetchBranch = $"SELECT {Environment.NewLine}" +
                                   $" {aggregateByClause}{Environment.NewLine}" +
                                   $"FROM {Environment.NewLine}" +
                                   $" {schema}.{table}",
                QueryMinusLive = $"SELECT {Environment.NewLine}" +
                                 $" {aggregateByClause}{Environment.NewLine}" +
                                 $"FROM {Configuration.DBNameLiveMinusTests}.{schema}.{table}",
                QueryMinusBranch = $"SELECT {Environment.NewLine}" +
                                 $" {aggregateByClause}{Environment.NewLine}" +
                                 $"FROM {Configuration.DBNameBranchMinusTests}.{schema}.{table}",
            };

            query.QueryMinusBranch = query.QueryMinusLive.Replace(Configuration.DBNameLiveMinusTests, Configuration.DBNameBranchMinusTests);
            query.QueryAggregate = true;
            query.ColumnName = "Aggregated";
            return query;
        }

        private static QueryDefinition QueryDefinitionGroupBy(string schema, string table, string columnList, string where)
        {
            var query = new QueryDefinition
            {
                QueryFetchLive = $"SELECT {Environment.NewLine}" +
                                 $" {columnList},{Environment.NewLine}" +
                                 $"count_big(*) AS CountNo {Environment.NewLine}" +
                                 $"FROM {Environment.NewLine}" +
                                 $" {schema}.{table}{Environment.NewLine}" +
                                 $" {where}" +
                                 $"{Environment.NewLine}group by {columnList}{Environment.NewLine}" +
                                 $"{Environment.NewLine}order by {columnList};",
                QueryFetchBranch = $"SELECT {Environment.NewLine}" +
                                   $" {columnList},{Environment.NewLine}" +
                                   $"count_big(*) AS CountNo {Environment.NewLine}" +
                                   $"FROM {Environment.NewLine}" +
                                   $" {schema}.{table}{Environment.NewLine}" +
                                   $" {where}" +
                                   $"{Environment.NewLine}group by {columnList}{Environment.NewLine}" +
                                   $"{Environment.NewLine}order by {columnList};",
                QueryMinusLive = $"SELECT {Environment.NewLine}" +
                                 $" {columnList},{Environment.NewLine}" +
                                 $"count_big(*) AS CountNo {Environment.NewLine}" +
                                 $"FROM {Environment.NewLine}" +
                                 $" {Configuration.DBNameLiveMinusTests}.{schema}.{table}{Environment.NewLine}" +
                                 $" {where}" +
                                 $"{Environment.NewLine}group by {columnList}{Environment.NewLine}",
                QueryMinusBranch = $"SELECT {Environment.NewLine}" +
                                   $" {columnList},{Environment.NewLine}" +
                                   $"count_big(*) AS CountNo {Environment.NewLine}" +
                                   $"FROM {Environment.NewLine}" +
                                   $" {Configuration.DBNameBranchMinusTests}.{schema}.{table}{Environment.NewLine}" +
                                   $" {where}" +
                                   $"{Environment.NewLine}group by {columnList}{Environment.NewLine}",
                QueryAggregate = true,
                ColumnName = columnList,
            };

            return query;
        }

        private static QueryDefinition QueryDefinition(string schema, string table, string columnList, string addWhereClause, string addOrderByClause, string addComment)
        {
            var query = new QueryDefinition
            {
                QueryFetchLive = $"SELECT {Environment.NewLine}" +
                                 $"{columnList}" +
                                 $"FROM {Environment.NewLine}" +
                                 $" {schema}.{table}" +
                                 $"{addWhereClause} " +
                                 $"{addOrderByClause} ;" +
                                 $"{addComment} ",
                QueryFetchBranch = $"SELECT {Environment.NewLine}" +
                                   $"{columnList}" +
                                   $"FROM {Environment.NewLine}" +
                                   $" {schema}.{table}" +
                                   $"{addWhereClause} " +
                                   $"{addOrderByClause} ;" +
                                   $"{addComment} ",
                QueryMinusLive = $"SELECT {Environment.NewLine}" +
                                 $"{columnList}" +
                                 $"FROM {Environment.NewLine}" +
                                 $" {Configuration.DBNameLiveMinusTests}.{schema}.{table}" +
                                 $"{Environment.NewLine}{addWhereClause} " +
                                 $"{Environment.NewLine}{addComment} ",
                QueryMinusBranch = $"SELECT {Environment.NewLine}" +
                                   $"{columnList}" +
                                   $"FROM {Environment.NewLine}" +
                                   $" {Configuration.DBNameBranchMinusTests}.{schema}.{table}" +
                                   $"{Environment.NewLine}{addWhereClause} " +
                                   $"{Environment.NewLine}{addComment} ",
                QueryAggregate = false,
            };

            return query;
        }
    }
}
