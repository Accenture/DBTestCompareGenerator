# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A .NET 8 console application (single project, `DBTestCompareGenerator/`) that connects to a SQL Server database, reads table/column metadata from system tables, and **generates SQL test-definition files** consumed by the external Java tool [DBTestCompare](https://github.com/Accenture/DBTestCompare). It also has a secondary mode that extracts and unpacks a DACPAC (`.dacpac`) into per-object `.sql` scripts.

The program produces files; it does not itself run the comparisons. There is no unit-test project — "tests" in this repo's vocabulary means the generated DBTestCompare definitions, which are executed downstream by the Java jar.

## Build & run

```powershell
# Restore + build (Release matches CI)
dotnet build DBTestCompareGenerator.sln --configuration Release

# Run (working directory must contain appsettings.json + Templates/, i.e. the build output dir)
cd .\DBTestCompareGenerator\bin\Release\net8.0
.\DBTestCompareGenerator.exe        # Windows
./DBTestCompareGenerator            # Linux
```

There is no lint command — StyleCop runs as an analyzer during `dotnet build`. Note `EnforceCodeStyleInBuild` is **false** in the csproj, so style violations surface as warnings rather than failing the build. Style is governed by `DBTestCompareGenerator/Project.ruleset`, `stylecop.json`, and `.editorconfig` (the company copyright header on every file is required by StyleCop).

A local SQL Server for testing is available via `DBTestCompareGenerator/docker-compose.yml`.

## Configuration is the control surface

Everything the program does is driven by `DBTestCompareGenerator/appsettings.json` under the `appSettings` key — there are no CLI arguments. `Configuration.cs` exposes each setting as a static property (most bool getters parse the string `"true"`/`"false"` manually). The two top-level modes:

- **Test generation** (default): toggled by `GenerateCountSmokeTests`, `GenerateCompareFetchTests`, `GenerateCompareMinusTests`. Fetch and Minus test the same thing two different ways — normally enable only one.
- **DACPAC unpack**: when `UnpackDacpac=true`, runs `UnpackDacpac` first, controlled by `ExtractAllTableData`, `VerifyExtraction`, `Folder`, `DacpacFolder`, `Database`, `SaveAsBaseline`, etc.

`ReadExcelFile=false` generates tests for **all** tables in the database. `ReadExcelFile=true` restricts to the table list (and per-table overrides like `WhereClause`, `OrderByClause`, `AggregateByClause`, `Comment`, `CreateTest=Y/N`) in `Templates/Table_Config.xlsx`, sheet `ListOfTables`.

In CI, settings are mutated at runtime by `set-appsettings.ps1`, and connection tokens in the generated config XML are replaced by `set-tokens-for-tests.ps1` before the Java tool runs.

## Pipeline (Program.cs)

`Main` runs a fixed sequence:
1. If `UnpackDacpac` → `UnpackDacpac.ExtractDacpacFile()` then `UnpackDacpacFile()`.
2. `CopyConfigFiles.CopyConfigFile()` — **deletes and recreates** the `test-definitions/` output dir, then seeds it from `Templates/cmpSqlResults-config.xml`.
3. `ReadConfigurationFromXlsx.ReadExcelFile()` — returns the per-table config list (null when `ReadExcelFile=false`).
4. `CountQuerySqlServer.CreateCountQuery(config)` — smoke/row-count tests.
5. `CompareQuerySqlServer.CreateCompareQuery(config)` — Fetch and Minus tests.

### How table metadata drives generation

Both query generators call `TablesDefinitions.GetTablesDefinitions()`, which runs one `INFORMATION_SCHEMA.COLUMNS` query **ordered by catalog/schema/table**. The generators iterate this flat row set and detect a table boundary by comparing the current row's schema+table to the next row's — columns are accumulated into a list until the boundary, then a test is emitted for that table. This ordering assumption is load-bearing: changing the `order by` would break grouping.

`ReadConfigurationFromXlsx.CheckIfTableInExcel(...)` is the per-table gate. It returns a `createTest` bool (note: `true` means **skip**) plus the Excel override columns, which both generators consult before emitting.

### Generated layout & query variants

`CopyConfigFiles.CreateFolderForTest` builds numbered category folders under `test-definitions/`: `1.Count`, `2.RowByRow` (Fetch), `3.MinusCompare`, `4.FetchAggregateGroupBy`, `5.MinusAggregateGroupBy`, each then nested by `schema/table[/column]`. Each test is a pair of `.sql` files (`ExpectedTable.sql` = "Live", `ActualTable.sql` = "Branch") plus a copied XML template (`from_file_sql.xml` for Fetch, `minus_from_file_sql.xml` for Minus).

`QueryDefinition` carries the four query strings per table (`QueryFetchLive/Branch`, `QueryMinusLive/Branch`). Minus queries are fully qualified with `DBNameLiveMinusTests` / `DBNameBranchMinusTests` (two databases on one server); Fetch queries are not (two datasources compared in the Java tool's memory). When a table has an `AggregateByClause`, an aggregated query is emitted plus per-column `GROUP BY count_big(*)` queries for columns whose type is in `ColumnTypesToGroupBy`.

## Conventions when editing

- SQL is built as interpolated strings with `Environment.NewLine`; column names are wrapped in `"` (double quotes) for cross-engine compatibility in the Java tool.
- Logging is NLog throughout — every class instantiates its own `Logger` via `NLog.Web.NLogBuilder.ConfigureNLog("nlog.config")`. Config is `nlog.config` (copied to output).
- DB access is `Microsoft.Data.SqlClient` via the single helper `ConnectSql.ExecuteSqlCommand` (returns a `DataTable`).
- The new copyright header block at the top of each `.cs` file must be preserved.

## CI

`.github/workflows/github-actions.yml` builds on Linux and Windows. The Linux job is the integration test: it spins up SQL Server in Docker, restores a backup, runs the generator, downloads the DBTestCompare jar, and executes the generated tests (JUnit reports published). Releases (artifacts uploaded) only fire on tag pushes. `azure-pipelines.yml` also exists.
