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

namespace LanguageFileHandler
{
    public class Language
    {
        //        ValidationEventHandler eventHandler = new ValidationEventHandler(Language.);

        #region Variables

        private XmlReader _xmlReader;
        private XmlDocument _xmlDocument;
        private XmlReaderSettings _xmlReaderSettings;

        private bool _bInitFlag;
        private string _strLanguageFile;
        private Exception _lastException;

        private GetLanguageKeysInProejctFiles _checkLanguageKeysInProejctFiles;
        private List<string> _languageKeyListOfProject = new List<string>();
        private List<string> _invalidLanguageKeysOfProject = new List<string>();

        private GetLanguageKeysInLanguageXml _checkLanguageKeysInXmlFile;
        private List<string> _languageKeyListOfXml = new List<string>();
        private List<string> _invalidLanguageKeysOfXml = new List<string>();

        private const string _invalidLanguageKeyReturnValue = "invalid";

        #endregion Variables

        #region Properties

        public String LanguageFilePath
        {
            get { return _strLanguageFile; }
            set { _strLanguageFile = value; }
        }

        public bool InitFlag
        {
            get { return _bInitFlag; }
        }

        public Exception LastException
        {
            get { return _lastException; }
        }

        public List<string> LanguageKeyListOfProejct
        {
            get { return _languageKeyListOfProject; }
        }

        public List<string> InvalidLanguageKeysOfProject
        {
            get { return _invalidLanguageKeysOfProject; }
        }

        public List<string> LanguageKeyListOfXml
        {
            get { return _languageKeyListOfXml; }
        }

        public List<string> InvalidLanguageKeysOfXml
        {
            get { return _invalidLanguageKeysOfXml; }
        }

        public string InvalidLanguageKeyValue
        {
            get { return _invalidLanguageKeyReturnValue; }    
        }

        #endregion

        #region Methodes

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="strLanguageFile">File name of the language xml file</param>
        public Language(String strLanguageFile)
        {
            _bInitFlag = false;
            _strLanguageFile = strLanguageFile;

            // Init language
            try
            {
                //// Create the validating reader and specify DTD validation.
                //_xmlReaderSettings = new XmlReaderSettings();
                //_xmlReaderSettings.DtdProcessing = DtdProcessing.Parse;
                //_xmlReaderSettings.ValidationType = ValidationType.DTD;
                //_xmlReaderSettings.ValidationEventHandler += ValidationCallBack;

                _xmlReader = XmlReader.Create(_strLanguageFile, _xmlReaderSettings);

                // Pass the validating reader to the XML document.
                // Validation fails due to an undefined attribute, but the 
                // data is still loaded into the document.
                _xmlDocument = new XmlDocument();
                _xmlDocument.Load(_xmlReader);

                _bInitFlag = true;
            }
            catch (Exception ex)
            {
                _lastException = ex;

                _bInitFlag = false;
                if (_xmlReader != null)
                    _xmlReader.Close();
            }
        }

        /// <summary>
        /// Get the language text for a given language key
        /// </summary>
        /// <param name="strXPath">XPath without "/Language" and language specification (e.g. "/English")</param>
        /// <param name="strLanguage">Name of the selected language (e.g. "English")</param>
        /// <returns>String with the language text</returns>
        public string GetLanguageTextByXPath(String strXPath, String strLanguage)
        {
            if (_bInitFlag)
            {
                try
                {
                    string XPath = "/Language/" + strLanguage + strXPath;
                    XmlNode xmlNode = _xmlDocument.SelectSingleNode(XPath);
                    if (xmlNode != null)
                        return xmlNode.Attributes["text"].InnerText;
                    else
                        return _invalidLanguageKeyReturnValue;
                }
                catch (Exception ex)
                {
                    _lastException = ex;
                    return _invalidLanguageKeyReturnValue;
                }
            }
            else
                return _invalidLanguageKeyReturnValue;
        }

        /// <summary>
        /// This functions returns a list with the available languages in the language XML.
        /// </summary>
        /// <returns>List with the available languages. If no languages are available the function returns null.</returns>
        public List<string> GetAvailableLanguages()
        {
            if (_bInitFlag)
            {
                try
                {
                    XmlNodeList xmlNodeList = _xmlDocument.SelectNodes("/Language/*");
                    if (xmlNodeList.Count != 0)
                    {
                        List<string> lstLanguages = new List<string>();
                        foreach (XmlNode xmlNode in xmlNodeList)
                        {
                            if (xmlNode != null)
                                lstLanguages.Add(xmlNode.Name);
                        }
                        return lstLanguages;
                    }
                    else
                        return null;
                }
                catch (Exception ex)
                {
                    _lastException = ex;
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// This function checks if all language keys in the project files
        /// are available in the language xml file.
        /// </summary>
        /// <param name="strProjectPath">Path to the project files with the language keys</param>
        public void CheckLanguageKeysOfProject(string strProjectPath)
        {
            if (_bInitFlag)
            {
                try
                {
                    // Get all language keys in the project files
                    if (_checkLanguageKeysInProejctFiles == null)
                    {
                        _checkLanguageKeysInProejctFiles = new GetLanguageKeysInProejctFiles(strProjectPath);
                    }

                    // Get all available languages in the language XML file
                    List<string> availableLanguages = GetAvailableLanguages();

                    // Loop through the languages and search for the language keys in the language XML file.
                    foreach (var languageName in availableLanguages)
                    {
                        foreach (var keyName in _checkLanguageKeysInProejctFiles.ListOfLangaugeKeysAndProjectFileName.Keys)
                        {
                            // Try to get the language key value from the language file and if the return value is invalid
                            // add it to the invalid language key list.
                            if (GetLanguageTextByXPath(keyName, languageName) == _invalidLanguageKeyReturnValue)
                            {
                                InvalidLanguageKeysOfProject.Add(String.Format("{0,-15}: {1}", languageName, keyName + " (File: " + _checkLanguageKeysInProejctFiles.ListOfLangaugeKeysAndProjectFileName[keyName]) + ")");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _lastException = ex;
                }
            }
        }

        /// <summary>
        /// This function checks if all language keys in the xml language file
        /// are used in the project files.
        /// </summary>
        /// <param name="strProjectPath">Path to the project files with the used language keys</param>
        public void CheckLanguageKeysOfXML(string strProjectPath)
        {
            if (_bInitFlag)
            {
                try
                {
                    // Flag if a xml language key is used or not
                    bool bXMLKeyUsed = false;

                    // Get all language keys in the project files
                    if (_checkLanguageKeysInProejctFiles == null)
                    {
                        _checkLanguageKeysInProejctFiles = new GetLanguageKeysInProejctFiles(strProjectPath);
                    }

                    // Get all language keys in the project files
                    if (_checkLanguageKeysInXmlFile == null)
                    {
                        _checkLanguageKeysInXmlFile = new GetLanguageKeysInLanguageXml(_xmlDocument);
                    }

                    // Get all available languages in the language XML file
                    List<string> availableLanguages = GetAvailableLanguages();

                    foreach (var keyNameXML in _checkLanguageKeysInXmlFile.ListOfLangaugeKeys)
                    {
                        foreach (var keyNameProject in _checkLanguageKeysInProejctFiles.ListOfLangaugeKeysAndProjectFileName.Keys)
                        {
                            // Loop through the languages and search for the language keys in the language XML file.
                            foreach (var languageName in availableLanguages)
                            {
                                if (keyNameXML == "/Language/" + languageName + keyNameProject)
                                    bXMLKeyUsed = true;
                            }
                        }
                        if (bXMLKeyUsed == false)
                        {
                            string keyName = @"";
                            // Split XPath in the language and language key
                            // Remove /Language/ part
                            keyName = keyNameXML.Remove(0, 1);
                            keyName = keyName.Remove(0, keyName.IndexOf('/') + 1);
                            string languageName = keyName.Substring(0, keyName.IndexOf('/') + 1);
                            languageName = languageName.Remove(keyName.IndexOf('/'), 1);
                            keyName = keyName.Substring(keyName.IndexOf('/'));
                            InvalidLanguageKeysOfXml.Add(String.Format("{0,-15}: {1}", languageName, keyName));
                        }

                        bXMLKeyUsed = false;
                    }
                }
                catch (Exception ex)
                {
                    _lastException = ex;
                }
            }
        }

        #endregion Methodes

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