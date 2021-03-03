// <copyright file="ReadConfigurationFromXlsx.cs" company="Objectivity Bespoke Software Specialists">
// Copyright (c) Objectivity Bespoke Software Specialists. All rights reserved.
// </copyright>

namespace DBTestCompareGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using NPOI.SS.UserModel;
    using NPOI.XSSF.UserModel;

    public static class ReadConfigurationFromXlsx
    {
        private static readonly NLog.Logger Logger =
            NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        public static List<Dictionary<string, string>> ReadExcelFile()
        {
            if (!Configuration.ReadExcelFile)
            {
                return null;
            }

            var sheetName = "ListOfTables";
            var path = $"{CopyConfigFiles.PathToCurrentFolder}{CopyConfigFiles.PathSeparator}Templates{CopyConfigFiles.PathSeparator}Table_Config.xlsx";
            Logger.Debug("Sheet {0} in file: {1}", sheetName, path);
            XSSFWorkbook wb;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                wb = new XSSFWorkbook(fs);
            }

            // get sheet
            var sh = (XSSFSheet)wb.GetSheet(sheetName);
            int startRow = 1;
            int startCol = 0;
            int totalRows = sh.LastRowNum;
            int totalCols = sh.GetRow(0).LastCellNum;
            List<Dictionary<string, string>> configList = new List<Dictionary<string, string>>();
            var row = 1;
            for (int i = startRow; i <= totalRows; i++, row++)
            {
                var configParams = new Dictionary<string, string>();
                var column = 0;
                for (int j = startCol; j < totalCols; j++, column++)
                {
                    if (sh.GetRow(0).GetCell(column).CellType != CellType.String)
                    {
                        throw new InvalidOperationException(string.Format("Cell with name of parameter must be string only, file {0} at sheet {1} row {2} column {3}", CopyConfigFiles.PathToCurrentFolder, sheetName, 0, column));
                    }

                    var cellType = sh.GetRow(row).GetCell(column).CellType;
                    switch (cellType)
                    {
                        case CellType.String: configParams.Add(sh.GetRow(0).GetCell(column).StringCellValue, sh.GetRow(row).GetCell(column).StringCellValue);
                            break;
                        case CellType.Numeric: configParams.Add(sh.GetRow(0).GetCell(column).StringCellValue, sh.GetRow(row).GetCell(column).NumericCellValue.ToString(CultureInfo.CurrentCulture));
                            break;
                        case CellType.Blank: configParams.Add(sh.GetRow(0).GetCell(column).StringCellValue, null);
                            break;
                        default:
                            throw new InvalidOperationException(string.Format("Not supported cell type {0} in file {1} at sheet {2} row {3} column {4}", cellType, path, sheetName, row, column));
                    }
                }

                configList.Add(configParams);
            }

            return configList;
        }

        public static (bool, string, string, string, string, string) CheckIfTableInExcel(List<Dictionary<string, string>> configList, string tableSchemaIteration, string tableNameIteration)
        {
            string domain = null;
            string schema = null;
            string tableName = null;
            string comment = null;
            string createTest = null;
            string whereClause = null;
            string orderByCluse = null;
            string aggregateByClause = null;
            if (configList != null)
            {
                for (int j = 0; j < configList.Count; j++)
                {
                    configList[j].TryGetValue("Domain", out domain);
                    configList[j].TryGetValue("Schema", out schema);
                    configList[j].TryGetValue("TableName", out tableName);
                    configList[j].TryGetValue("Comment", out comment);
                    configList[j].TryGetValue("CreateTest", out createTest);
                    configList[j].TryGetValue("WhereClause", out whereClause);
                    configList[j].TryGetValue("OrderByClause", out orderByCluse);
                    configList[j].TryGetValue("AggregateByClause", out aggregateByClause);

                    if (tableSchemaIteration == schema && tableNameIteration == tableName && createTest == "Y")
                    {
                        break;
                    }
                }

                if (tableSchemaIteration != schema || tableNameIteration != tableName || createTest == "N")
                {
                    return (true, domain, comment, whereClause, orderByCluse, aggregateByClause);
                }
            }

            return (false, domain, comment, whereClause, orderByCluse, aggregateByClause);
        }
    }
}
