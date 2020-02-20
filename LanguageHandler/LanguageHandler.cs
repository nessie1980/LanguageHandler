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

using System;
using System.Collections.Generic;
using System.Xml;

namespace LanguageHandler
{
    public class Language
    {
        //        ValidationEventHandler eventHandler = new ValidationEventHandler(Language.);

        #region Fields

        private readonly XmlReader _xmlReader;
        private readonly XmlDocument _xmlDocument;
        private XmlReaderSettings _xmlReaderSettings;

        private GetLanguageKeysInProjectFiles _checkLanguageKeysInProjectFiles;

        private GetLanguageKeysInLanguageXml _checkLanguageKeysInXmlFile;

        private const string InvalidLanguageKeyReturnValue = "invalid";

        #endregion Fields

        #region Properties

        public string LanguageFilePath { get; set; }

        public bool InitFlag { get; }

        public Exception LastException { get; private set; }

        public List<string> LanguageKeyListOfProject { get; } = new List<string>();

        public List<string> InvalidLanguageKeysOfProject { get; } = new List<string>();

        public List<string> LanguageKeyListOfXml { get; } = new List<string>();

        public List<string> InvalidLanguageKeysOfXml { get; } = new List<string>();

        public string InvalidLanguageKeyValue => InvalidLanguageKeyReturnValue;

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="languageFile">File name of the language XML file</param>
        public Language(string languageFile)
        {
            InitFlag = false;
            LanguageFilePath = languageFile;

            // Init language
            try
            {
                //// Create the validating reader and specify DTD validation.
                //_xmlReaderSettings = new XmlReaderSettings();
                //_xmlReaderSettings.DtdProcessing = DtdProcessing.Parse;
                //_xmlReaderSettings.ValidationType = ValidationType.DTD;
                //_xmlReaderSettings.ValidationEventHandler += ValidationCallBack;

                _xmlReader = XmlReader.Create(LanguageFilePath, _xmlReaderSettings);

                // Pass the validating reader to the XML document.
                // Validation fails due to an undefined attribute, but the 
                // data is still loaded into the document.
                _xmlDocument = new XmlDocument();
                _xmlDocument.Load(_xmlReader);

                InitFlag = true;
            }
            catch (Exception ex)
            {
                LastException = ex;

                InitFlag = false;
                _xmlReader?.Close();
            }
        }

        /// <summary>
        /// Get the language text for a given language key
        /// </summary>
        /// <param name="givenXpath">XPath without "/Language" and language specification (e.g. "/English")</param>
        /// <param name="language">Name of the selected language (e.g. "English")</param>
        /// <returns>String with the language text</returns>
        public string GetLanguageTextByXPath(string givenXpath, string language)
        {
            if (!InitFlag) return InvalidLanguageKeyReturnValue;

            try
            {
                var xPath = "/Language/" + language + givenXpath;
                var xmlNode = _xmlDocument.SelectSingleNode(xPath);
                if (xmlNode?.Attributes != null)
                    return xmlNode.Attributes["text"].InnerText;
                            
                return InvalidLanguageKeyReturnValue;
            }
            catch (Exception ex)
            {
                LastException = ex;
                return InvalidLanguageKeyReturnValue;
            }
        }

        /// <summary>
        /// Get the language texts for a given language key
        /// </summary>
        /// <param name="givenXpath">XPath without "/Language" and language specification (e.g. "/English")</param>
        /// <param name="language">Name of the selected language (e.g. "English")</param>
        /// <returns>String list with the language texts</returns>
        public List<string> GetLanguageTextListByXPath(string givenXpath, string language)
        {
            if (!InitFlag) return null;

            try
            {
                var textList = new List<string>();
                var xPath = "/Language/" + language + givenXpath;
                var xmlNodes = _xmlDocument.SelectNodes(xPath);
                if (xmlNodes == null || xmlNodes.Count <= 0)
                {
                    textList.Add(@"invalid");
                    return textList;
                }

                foreach (var xmlNode in xmlNodes)
                {
                    var xmlAttributeCollection = ((XmlNode) xmlNode).Attributes;
                    if (xmlAttributeCollection != null)
                        textList.Add(xmlAttributeCollection["text"].InnerText);
                }

                return textList;
            }
            catch (Exception ex)
            {
                LastException = ex;
                var textList = new List<string> {@"invalid"};
                return textList;
            }
        }

        /// <summary>
        /// This functions returns a list with the available languages in the language XML.
        /// </summary>
        /// <returns>List with the available languages. If no languages are available the function returns null.</returns>
        public List<string> GetAvailableLanguages()
        {
            if (!InitFlag) return null;

            try
            {
                var xmlNodeList = _xmlDocument.SelectNodes("/Language/*");
                if (xmlNodeList == null || xmlNodeList.Count == 0) return null;

                var lstLanguages = new List<string>();
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    if (xmlNode != null)
                        lstLanguages.Add(xmlNode.Name);
                }
                return lstLanguages;
            }
            catch (Exception ex)
            {
                LastException = ex;
                return null;
            }
        }

        /// <summary>
        /// This function checks if all language keys in the project files
        /// are available in the language XML file.
        /// </summary>
        /// <param name="strProjectPath">Path to the project files with the language keys</param>
        public void CheckLanguageKeysOfProject(string strProjectPath)
        {
            if (!InitFlag) return;

            try
            {
                // Get all language keys in the project files
                if (_checkLanguageKeysInProjectFiles == null)
                {
                    _checkLanguageKeysInProjectFiles = new GetLanguageKeysInProjectFiles(strProjectPath);
                }

                // Get all available languages in the language XML file
                var availableLanguages = GetAvailableLanguages();

                // Loop through the languages and search for the language keys in the language XML file.
                foreach (var languageName in availableLanguages)
                {
                    foreach (var keyName in _checkLanguageKeysInProjectFiles.ListOfLanguageKeysAndProjectFileName.Keys)
                    {
                        // Try to get the language key value from the language file and if the return value is invalid
                        // add it to the invalid language key list.
                        if (GetLanguageTextByXPath(keyName, languageName) == InvalidLanguageKeyReturnValue)
                        {
                            InvalidLanguageKeysOfProject.Add(string.Format(@"{0,-15}: {1}", languageName, keyName + " (File: " + _checkLanguageKeysInProjectFiles.ListOfLanguageKeysAndProjectFileName[keyName]) + ")");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LastException = ex;
            }
        }

        /// <summary>
        /// This function checks if all language keys in the XML language file
        /// are used in the project files.
        /// </summary>
        /// <param name="strProjectPath">Path to the project files with the used language keys</param>
        public void CheckLanguageKeysOfXml(string strProjectPath)
        {
            if (!InitFlag) return;

            try
            {
                // Flag if a XML language key is used or not
                var bXmlKeyUsed = false;

                // Get all language keys in the project files
                if (_checkLanguageKeysInProjectFiles == null)
                {
                    _checkLanguageKeysInProjectFiles = new GetLanguageKeysInProjectFiles(strProjectPath);
                }

                // Get all language keys in the project files
                if (_checkLanguageKeysInXmlFile == null)
                {
                    _checkLanguageKeysInXmlFile = new GetLanguageKeysInLanguageXml(_xmlDocument);
                }

                // Get all available languages in the language XML file
                var availableLanguages = GetAvailableLanguages();

                foreach (var keyNameXml in _checkLanguageKeysInXmlFile.ListOfLanguageKeys)
                {
                    foreach (var keyNameProject in _checkLanguageKeysInProjectFiles.ListOfLanguageKeysAndProjectFileName.Keys)
                    {
                        // Loop through the languages and search for the language keys in the language XML file.
                        foreach (var languageName in availableLanguages)
                        {
                            // Check for single line keys
                            if (keyNameXml == "/Language/" + languageName + keyNameProject)
                                bXmlKeyUsed = true;
                            else
                            {
                                // Check for multiline keys
                                // Remove last XML part
                                var xmlSplitParts = keyNameXml.Split('/');
                                var xmlKeyNameLines = @"";

                                for (var i = 0; i < xmlSplitParts.Length - 1; i++)
                                {
                                    if (xmlSplitParts[i] == @"") continue;

                                    xmlKeyNameLines += "/";
                                    xmlKeyNameLines += xmlSplitParts[i];
                                }

                                xmlKeyNameLines += "/*";

                                if (xmlKeyNameLines == "/Language/" + languageName + keyNameProject)
                                    bXmlKeyUsed = true;
                            }
                        }
                    }
                    if (bXmlKeyUsed == false)
                    {
                        // Split XPath in the language and language key
                        // Remove /Language/ part
                        var keyName = keyNameXml.Remove(0, 1);
                        keyName = keyName.Remove(0, keyName.IndexOf('/') + 1);
                        var languageName = keyName.Substring(0, keyName.IndexOf('/') + 1);
                        languageName = languageName.Remove(keyName.IndexOf('/'), 1);
                        keyName = keyName.Substring(keyName.IndexOf('/'));
                        InvalidLanguageKeysOfXml.Add($"{languageName,-15}: {keyName}");
                    }

                    bXmlKeyUsed = false;
                }
            }
            catch (Exception ex)
            {
                LastException = ex;
            }
        }

        #endregion Methods

        // Display any warnings or errors.
        //private static void ValidationCallBack(object sender, ValidationEventArgs args)
        //{
        //    if (args.Severity == XmlSeverityType.Warning)
        //        Console.WriteLine("\tWarning: Matching schema not found.  No validation occurred." + args.Message);
        //    else
        //        Console.WriteLine("\tValidation error: " + args.Message);

        //}
    }
}