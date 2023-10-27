namespace DBTestCompareGenerator
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.SqlServer.Dac;
    using Microsoft.SqlServer.Dac.Model;

    internal static class UnpackDacpac
    {
        private static readonly NLog.Logger Logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        public static void ExtractDacpacFile()
        {
            string dacpacFilePath = $"{Configuration.DacpacFolder}";

            if (!Directory.Exists(dacpacFilePath))
            {
                Directory.CreateDirectory(dacpacFilePath);
                Logger.Debug($"Folder created: {dacpacFilePath}");
            }

            string dacpackFile = $"{dacpacFilePath}{Path.DirectorySeparatorChar}{Configuration.Database}.dacpac";

            Logger.Info($"Extracting dacpac file {dacpackFile} from DB {Configuration.Database}");

            DacExtractOptions extractOptions = new DacExtractOptions
            {
                ExtractAllTableData = Configuration.ExtractAllTableData,
                ExtractApplicationScopedObjectsOnly = Configuration.ExtractApplicationScopedObjectsOnly,
                VerifyExtraction = Configuration.VerifyExtraction,
                IgnoreExtendedProperties = Configuration.IgnoreExtendedProperties,
                IgnorePermissions = Configuration.IgnorePermissions,
            };

            DacServices dacServices = new DacServices(Configuration.ConnectionString);

            dacServices.Message += (sender, e) =>
            {
                Logger.Debug($"Message: {e.Message.MessageType} - {e.Message.Message}");
            };

            dacServices.Extract(dacpackFile, Configuration.Database, "Mi9", new Version(), null, null, extractOptions);
        }

        public static void UnpackDacpacFile()
        {
            var dacpacPath = $"{Configuration.DacpacFolder}{Path.DirectorySeparatorChar}{Configuration.Database}.dacpac";

            Logger.Info($"Unpacking dacpac file {dacpacPath} from DB {Configuration.Database}");

            var model = TSqlModel.LoadFromDacpac(dacpacPath, new ModelLoadOptions(DacSchemaModelStorageType.Memory, false));

            foreach (var dbObject in model.GetObjects(DacQueryScopes.UserDefined).Reverse())
            {
                string folder = $"{Configuration.FolderPath}{Path.DirectorySeparatorChar}{Configuration.Database}";
                string file = string.Empty;

                if (dbObject.TryGetScript(out var script))
                {
                    Logger.Info($"{dbObject.ObjectType.Name} - {dbObject.Name} processing");
                    if (string.IsNullOrEmpty(dbObject.Name.ToString()))
                    {
                        file = $"{Configuration.Database}.Role";
                        if (Configuration.SaveAsBaseline)
                        {
                            file = $"Role";
                        }

                        SaveTextToFile(script, folder, file);
                    }
                    else
                    {
                        // Get Schema name and Object Name
                        var fullName = dbObject.Name.Parts;
                        string type = dbObject.ObjectType.Name.ToString();

                        if (fullName.Count >= 4 && !type.Equals("Permission"))
                        {
                            // Get Object Name
                            file = $"{fullName[2]}";
                            type = $"{fullName[0]}";
                            type = AlterToType(type, file, script, out file);

                            // Add DB Name and Schema  Name to file Name
                            if (!Configuration.SaveAsBaseline)
                            {
                                file = $"{Configuration.Database}.{fullName[1]}.{file}";
                            }

                            folder = $"{folder}{Path.DirectorySeparatorChar}{fullName[1]}{Path.DirectorySeparatorChar}{SetType(type)}";
                        }
                        else if ((fullName.Count == 3 && (type.Equals("Index") || type.Equals("ColumnStoreIndex"))) || fullName.Count == 2)
                        {
                            // Get Object Name
                            file = $"{fullName[1]}";

                            type = AlterToType(type, file, script, out file);

                            // Add DB Name and Schema  Name to file Name
                            if (!Configuration.SaveAsBaseline)
                            {
                                file = $"{Configuration.Database}.{fullName[0]}.{file}";
                            }

                            folder = $"{folder}{Path.DirectorySeparatorChar}{fullName[0]}{Path.DirectorySeparatorChar}{SetType(type)}";
                        }
                        else if (type.Equals("Permission"))
                        {
                            // Get Object Name
                            file = $"{type}";

                            type = AlterToType(type, file, script, out file);

                            // Add DB Name and Schema  Name to file Name
                            if (!Configuration.SaveAsBaseline)
                            {
                                file = $"{Configuration.Database}.{file}";
                            }

                            folder = $"{folder}{Path.DirectorySeparatorChar}{fullName[2]}{Path.DirectorySeparatorChar}{SetType(type)}";
                        }
                        else if (fullName.Count == 3)
                        {
                            // Get Object Name
                            file = $"{fullName[1]}";
                            type = $"{fullName[0]}";
                            type = AlterToType(type, file, script, out file);

                            // Add DB Name and Schema  Name to file Name
                            if (!Configuration.SaveAsBaseline)
                            {
                                file = $"{Configuration.Database}.{file}";
                            }

                            folder = $"{folder}{Path.DirectorySeparatorChar}{fullName[1]}{Path.DirectorySeparatorChar}{SetType(type)}";
                        }
                        else
                        {
                            // Add DB Name and Schema Name to file Name
                            if (Configuration.SaveAsBaseline)
                            {
                                file = $"{type}";
                            }
                            else
                            {
                                file = $"{Configuration.Database}.{type}";
                            }

                            // Add Schema Name and Object Type as Subfolders
                            folder = $"{folder}{Path.DirectorySeparatorChar}";
                        }

                        // Add Object type to file name
                        if (!Configuration.SaveAsBaseline)
                        {
                            file = $"{file}__{type}";
                        }

                        SaveTextToFile(script, folder, file);
                    }
                }
                else
                    Logger.Info($"{dbObject.ObjectType} - {dbObject.Name} could not be scripted");
            }
        }

        static void SaveTextToFile(string text, string folder, string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                file = "ToBeChecked";
            }

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                Logger.Debug($"Folder created: {folder}");
            }

            file = file.Replace("*", string.Empty).Replace($"{Path.DirectorySeparatorChar}", "_");
            file = $"{file}.sql";
            string fullPath = Path.Combine(folder, file);
            if (File.Exists(fullPath))
            {
                bool hasEmptyLineAtEnd = CheckForEmptyLineAtEnd(fullPath);
                if (!hasEmptyLineAtEnd)
                {
                    text = $"\n{text}";
                }
            }

            Logger.Trace($"Saving script {file}");
            File.AppendAllText(Path.Combine(folder, file), text);
        }

        static string SetType(string text)
        {
            if (text.Equals("TableValuedFunction"))
            {
                Logger.Trace($"Set type Functions");
                return "Functions";
            }

            if (text.Equals("Procedure"))
            {
                Logger.Trace($"Set type Procedure");
                return "Stored Procedures";
            }

            if (text.Equals("TableType"))
            {
                Logger.Trace($"Set type TableType");
                return "User Defined Types";
            }

            if (text.Equals("Table"))
            {
                Logger.Trace($"Set type Table");
                return "Tables";
            }

            if (text.Equals("View"))
            {
                Logger.Trace($"Set type View");
                return "Views";
            }

            if (text.Equals("ScalarFunction"))
            {
                Logger.Trace($"Set type Functions");
                return "Functions";
            }

            if (text.Equals("Sequence"))
            {
                Logger.Trace($"Set type Sequence");
                return "Sequences";
            }

            if (text.Equals("Synonym"))
            {
                Logger.Trace($"Set type Synonym");
                return "Synonyms";
            }

            return text;
        }

        static string AlterToType(string text, string fileName, string script, out string file)
        {
            file = fileName;
            if (text.Equals("PrimaryKeyConstraint"))
            {
                file = FindTableName(script, text);
                text = text.Replace("PrimaryKeyConstraint", "Table");
                return text;
            }

            if (text.Equals("DefaultConstraint"))
            {
                file = FindTableName(script, text);
                text = text.Replace("DefaultConstraint", "Table");
                return text;
            }

            if (text.Equals("CheckConstraint"))
            {
                file = FindTableName(script, text);
                text = text.Replace("CheckConstraint", "Table");
                return text;
            }

            if (text.Equals("ForeignKeyConstraint"))
            {
                file = FindTableName(script, text);
                text = text.Replace("ForeignKeyConstraint", "Table");
                return text;
            }

            if (text.Equals("UniqueConstraint"))
            {
                file = FindTableName(script, text);
                text = text.Replace("UniqueConstraint", "Table");
                return text;
            }

            if (text.Equals("ColumnStoreIndex"))
            {
                file = FindTableName(script, text);
                text = text.Replace("ColumnStoreIndex", "Table");
                return text;
            }

            if (text.Equals("DmlTrigger"))
            {
                file = FindTableName(script, text);
                text = text.Replace("DmlTrigger", "Table");
                return text;
            }

            if (text.Equals("Index"))
            {
                text = text.Replace("Index", "Table");
                return text;
            }

            if (text.Equals("SqlView"))
            {
                text = text.Replace("SqlView", "View");
                return text;
            }

            if (text.Equals("SqlTableBase"))
            {
                text = text.Replace("SqlTableBase", "Table");
                return text;
            }

            if (text.Equals("SqlSubroutineParameter"))
            {
                text = text.Replace("SqlSubroutineParameter", "Procedure");
                return text;
            }

            if (text.Equals("SqlProcedure"))
            {
                text = text.Replace("SqlProcedure", "Procedure");
                return text;
            }

            if (text.Equals("SqlFunction"))
            {
                text = text.Replace("SqlFunction", "Functions");
                return text;
            }

            if (text.Equals("SqlColumn"))
            {
                text = text.Replace("SqlColumn", "Table");
                return text;
            }

            return text;
        }

        static string FindTableName(string text, string type)
        {
            Regex r = new Regex(@"[\[]?[a-zA-Z_0-9]+[\]]?\.[\[]?([a-zA-Z_0-9]+)[\]]?");
            foreach (Match m in r.Matches(text))
            {
                if (type.Equals("DmlTrigger"))
                {
                    type = "Done";
                    continue;
                }

                return m.Groups[1].Value;
            }

            return "NameOfTableNotFound";
        }

        static bool CheckForEmptyLineAtEnd(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string lastNonEmptyLine = null;

                while (reader.ReadLine() is { } line)
                {
                    lastNonEmptyLine = line;
                }

                // Check if the last non-empty line is not null and if it is empty
                return string.IsNullOrWhiteSpace(lastNonEmptyLine);
            }
        }
    }
}
