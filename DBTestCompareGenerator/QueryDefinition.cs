// <copyright file="QueryDefinition.cs" company="Objectivity Bespoke Software Specialists">
// Copyright (c) Objectivity Bespoke Software Specialists. All rights reserved.
// </copyright>

namespace DBTestCompareGenerator
{
    public class QueryDefinition
    {
        private static readonly NLog.Logger Logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        public bool QueryAggregate { get; set; }

        public string QueryFetchLive { get; set; }

        public string QueryFetchBranch { get; set; }

        public string QueryMinusLive { get; set; }

        public string QueryMinusBranch { get; set; }

        public string ColumnName { get; set; }

        public void PrintQueries()
        {
            if (!string.IsNullOrEmpty(this.QueryAggregate.ToString()))
            {
                Logger.Info($"SQL query QueryAggregate: {this.QueryAggregate}");
            }

            if (!string.IsNullOrEmpty(this.QueryFetchLive))
            {
                Logger.Info($"SQL query QueryFetchLive: {this.QueryFetchLive}");
            }

            if (!string.IsNullOrEmpty(this.QueryFetchBranch))
            {
                Logger.Info($"SQL query QueryFetchBranch: {this.QueryFetchBranch}");
            }

            if (!string.IsNullOrEmpty(this.QueryMinusLive))
            {
                Logger.Info($"SQL query QueryMinusLive: {this.QueryMinusLive}");
            }

            if (!string.IsNullOrEmpty(this.QueryMinusBranch))
            {
                Logger.Info($"SQL query QueryMinusBranch: {this.QueryMinusBranch}");
            }

            if (!string.IsNullOrEmpty(this.ColumnName))
            {
                Logger.Info($"SQL query ColumnName: {this.ColumnName}");
            }
        }
    }
}
