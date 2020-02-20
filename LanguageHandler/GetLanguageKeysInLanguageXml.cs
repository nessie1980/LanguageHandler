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
using System.Xml;

namespace LanguageHandler
{
    internal class GetLanguageKeysInLanguageXml
    {
        #region Fields

        /// <summary>
        /// Stores the XPath
        /// </summary>
        private string _xPath = @"";

        #endregion Fields

        #region Properties

        /// <summary>
        /// List of all language keys as XPath in the given language XML file
        /// </summary>
        public List<string> ListOfLanguageKeys { get; internal set; } = new List<string>();

        #endregion Properties

        #region Methods

        /// <summary>
        /// Constructor with the root node of the language XML file.
        /// </summary>
        /// <param name="rootNode"></param>
        public GetLanguageKeysInLanguageXml(XmlNode rootNode)
        {
            var rootNode1 = rootNode;

            if (rootNode1 != null)
                GetAllLanguageKeysInXml(rootNode1);
        }

        /// <summary>
        /// This function gets all language key as XPath.
        /// The function is a recursive function which loops
        /// through the nodes of the language XML file.
        /// </summary>
        /// <param name="currentNode">Node which should be checked</param>
        private void GetAllLanguageKeysInXml(XmlNode currentNode)
        {
            // Check if the current node is an element node
            if (currentNode.NodeType == XmlNodeType.Element)
                _xPath += "/" + currentNode.Name;

            // Check the current node has child nodes
            if (!currentNode.HasChildNodes) return;

            // Loop through the child nodes of the current node
            foreach (XmlNode childNode in currentNode.ChildNodes)
            {
                // Recursive call with the new child node
                GetAllLanguageKeysInXml(childNode);

                // Check is the child node is an element node
                if (childNode.NodeType != XmlNodeType.Element) continue;

                // If the child node has also child nodes
                // if not the last node in the XPath has been reached
                if (!childNode.HasChildNodes)
                {
                    // Split XPath
                    ListOfLanguageKeys.Add(_xPath);
                }

                // Remove the last node of the current XPath
                var iLastIndex = _xPath.LastIndexOf('/');
                if (iLastIndex > 0)
                    _xPath = _xPath.Remove(iLastIndex);
            }
        }

        #endregion Methodes
    }
}
