// <copyright file="TablesDefinitions.cs" company="Objectivity Bespoke Software Specialists">
// Copyright (c) Objectivity Bespoke Software Specialists. All rights reserved.
// </copyright>

namespace DBTestCompareGenerator
{
    using System;
    using System.Data;
    using NLog;

    /// <summary>
    /// Get Tables Definitions.
    /// </summary>
    public static class TablesDefinitions
    {
        private static readonly NLog.Logger Logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        /// <summary>
        /// Get Tables Definitions.
        /// </summary>
        /// <returns>DataTable.</returns>
        public static DataTable GetTablesDefinitions()
        {
            var command = $"SELECT TABLE_CATALOG, {Environment.NewLine}" +
                          $"TABLE_SCHEMA, TABLE_NAME,COLUMN_NAME, {Environment.NewLine}" +
                          $"IS_NULLABLE, DATA_TYPE {Environment.NewLine}" +
                          $"FROM INFORMATION_SCHEMA.COLUMNS  {Environment.NewLine}" +
                          "order by 1,2,3;";
            Logger.Info($"About to execute SQL query: {command}");

            return ConnectSql.ExecuteSqlCommand(command, Configuration.ConnectionString);
        }

        /// <summary>
        /// Get query for rows count.
        /// </summary>
        /// <returns>DataTable.</returns>
        public static string CreateSmokeCountQuery(string schema, string table)
        {
            var command = $"SELECT {Environment.NewLine}" +
                          $" sum(cast(SysP.row_count as bigint)) {Environment.NewLine}" +
                          $" FROM sys.dm_db_partition_stats as SysP {Environment.NewLine}" +
                          $" inner join sys.indexes as SysI {Environment.NewLine}" +
                          $" on SysP.object_id = SysI.object_id {Environment.NewLine}" +
                          $" and SysP.index_id = SysI.index_id {Environment.NewLine}" +
                          $" inner join sys.tables as SysT {Environment.NewLine}" +
                          $" on SysP.object_id = SysT.object_id {Environment.NewLine}" +
                          $" inner join sys.schemas as SysS {Environment.NewLine}" +
                          $" on SysT.schema_id = SysS.schema_id {Environment.NewLine}" +
                          $" WHERE {Environment.NewLine}" +
                          $" SysI.[type] in (0, 1)  and SysS.name = '{schema}' and SysT.name = '{table}'; ";
            Logger.Info($"About to execute SQL query: {command}");
            return command;
        }
    }
}
