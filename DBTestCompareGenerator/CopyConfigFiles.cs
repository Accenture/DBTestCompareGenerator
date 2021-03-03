// <copyright file="CopyConfigFiles.cs" company="Objectivity Bespoke Software Specialists">
// Copyright (c) Objectivity Bespoke Software Specialists. All rights reserved.
// </copyright>
// <license>
//     The MIT License (MIT)
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//     The above copyright notice and this permission notice shall be included in all
//     copies or substantial portions of the Software.
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//     SOFTWARE.
// </license>

namespace DBTestCompareGenerator
{
    using System.IO;

    public static class CopyConfigFiles
    {
        public static readonly char PathSeparator = Path.DirectorySeparatorChar;
        public static readonly string PathToCurrentFolder = Directory.GetCurrentDirectory();

        /// <summary>
        /// NLog logger handle.
        /// </summary>
        private static readonly NLog.Logger Logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        public static void CopyConfigFile()
        {
            string filePath = $"{PathToCurrentFolder}{PathSeparator}test-definitions{PathSeparator}cmpSqlResults-config.xml";
            Logger.Debug($"File path {filePath}");
            if (Directory.Exists($"{PathToCurrentFolder}{PathSeparator}test-definitions"))
            {
                try
                {
                    Directory.Delete($"{PathToCurrentFolder}{PathSeparator}test-definitions", true);
                }
                catch (System.IO.IOException e)
                {
                    Logger.Error($"{e}");
                }
            }

            if (!Directory.Exists($"{PathToCurrentFolder}{PathSeparator}test-definitions"))
            {
                Directory.CreateDirectory($"{PathToCurrentFolder}{PathSeparator}test-definitions");
            }

            File.Copy($"{PathToCurrentFolder}{PathSeparator}Templates{PathSeparator}cmpSqlResults-config.xml", filePath, true);
        }

        public static void CreateTestDefinitions(string folderFetch, string query, string tableSchemaIteration, string tableNameIteration, string prefix, string actualExpected, string xmlFile = null)
        {
            File.WriteAllText(
                $"{CopyConfigFiles.PathToCurrentFolder}{CopyConfigFiles.PathSeparator}{folderFetch}{CopyConfigFiles.PathSeparator}{actualExpected}",
                query);
            if (!string.IsNullOrEmpty(xmlFile))
            {
                File.Copy(
                    $"{CopyConfigFiles.PathToCurrentFolder}{CopyConfigFiles.PathSeparator}Templates{CopyConfigFiles.PathSeparator}{xmlFile}",
                    $"{CopyConfigFiles.PathToCurrentFolder}{CopyConfigFiles.PathSeparator}{folderFetch}{CopyConfigFiles.PathSeparator}{prefix}{tableSchemaIteration}_{tableNameIteration}.xml",
                    true);
            }
        }

        public static string CreateFolderForTest(string tableSchemaIteration, string tableNameIteration, string testCategory, string columnName = null)
        {
            string folder = $"test-definitions{CopyConfigFiles.PathSeparator}{testCategory}{CopyConfigFiles.PathSeparator}{tableSchemaIteration}{CopyConfigFiles.PathSeparator}{tableNameIteration}";
            if (!string.IsNullOrEmpty(columnName))
            {
                folder += $"{CopyConfigFiles.PathSeparator}{columnName}";
            }

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            return folder;
        }
    }
}
