using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace Mobile
{
    public static class ConfigReader
    {
        private static XDocument _xDocument;

        public static string GetWebServiceUri(Stream fileStream)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "app.config");

            if (!File.Exists(path))
            {
                _xDocument = XDocument.Load(fileStream);
                _xDocument.Save(path);
            }
            else
            {
                _xDocument = XDocument.Load(path);
            }

            return _xDocument.Root.Element("webservice").Attribute("uri").Value;
        }
    }
}