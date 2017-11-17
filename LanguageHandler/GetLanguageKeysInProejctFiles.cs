/*
MIT License

Copyright (c) 2017 nessie1980 (nessie1980@gmx.de)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace LanguageHandler
{
    public class GetLanguageKeysInProejctFiles
    {
        #region Fields

        /// <summary>
        /// Path to the project
        /// </summary>
        private string _projectPath;

        /// <summary>
        /// List with all .cs files of the project
        /// </summary>
        private readonly List<string> _listProjectFiles = new List<string>();

        /// <summary>
        /// List of all language keys in the .cs files as key and as value the .cs file name
        /// </summary>
        private readonly Dictionary<string, string> _listOfLangaugeKeysAndProjectFileName = new Dictionary<string, string>();

        #endregion Fields

        #region Properties

        public string ProjectPath
        {
            get { return _projectPath; }

            internal set
            {
                // Check if the path is valid
                _projectPath = Directory.Exists(value) ? value : null;
            }
        }

        public List<string> ListProjectFiles
        {
            get { return _listProjectFiles; }
        }

        public Dictionary<string, string> ListOfLangaugeKeysAndProjectFileName
        {
            get { return _listOfLangaugeKeysAndProjectFileName; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// This function search for the language keys in
        /// all c# files in the given path.
        /// The function also search in the subdirectories of the
        /// given path.
        /// </summary>
        /// <param name="strProjectPath"></param>
        public GetLanguageKeysInProejctFiles(string strProjectPath)
        {
            ProjectPath = strProjectPath;

            if (ProjectPath == null) return;

            ProcessDirectory(ProjectPath);
            GetAllLanguageKeysInProjectFiles();
        }

        /// <summary>
        /// This function search in the files of the project
        /// for the function calls of the localization of the project.
        /// </summary>
        private void GetAllLanguageKeysInProjectFiles()
        {
            // Loop through the project files
            foreach (var filename in ListProjectFiles)
            {
                // Read the file content and search for the localization function calls.
                var strFileContent = File.ReadAllText(ProjectPath + filename);

                // Remove line feeds
                strFileContent = strFileContent.Replace('\n', ' ');
                strFileContent = strFileContent.Replace('\r', ' ');
                strFileContent = strFileContent.Replace(" ", string.Empty);

                var regEx = new Regex(@"GetLanguageTextByXPath\([@][""]([0-9a-zA-Z_/]*)");
                var matchCollection = regEx.Matches(strFileContent);

                // Process the found matches and add the language keys to the language key list.
                foreach (Match match in matchCollection)
                {
                    for (var gIdx = 0; gIdx < match.Groups.Count; gIdx++)
                    {
                        if (gIdx != match.Groups.Count - 1) continue;

                        if (!ListOfLangaugeKeysAndProjectFileName.ContainsKey(match.Groups[gIdx].Value))
                            ListOfLangaugeKeysAndProjectFileName.Add(match.Groups[gIdx].Value, filename);
                    }
                }
            }
        }

        /// <summary>
        /// This function processes through all files, directories and
        /// subdirectories in the given directory.
        /// Files with the extension ".cs" will be added to a file list.
        /// </summary>
        /// <param name="targetDirectory">Start directory for the file and directory search</param>
        private void ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            var fileEntries = Directory.GetFiles(targetDirectory, "*.cs");
            foreach (var fileName in fileEntries)
                ProcessFile(fileName.Replace(ProjectPath, ""));

            // Recurse into subdirectories of this directory.
            var subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (var subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }

        /// <summary>
        /// This function adds the given file to the file list with the project files
        /// </summary>
        /// <param name="file">File which should be added to the project file list</param>
        private void ProcessFile(string file)
        {
            ListProjectFiles.Add(file);
        }

        #endregion Methodes
    }
}
