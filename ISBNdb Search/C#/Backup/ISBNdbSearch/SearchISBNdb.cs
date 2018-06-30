using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace ISBNdbSearch
{
    public class SearchISBNdB
    {
        public String GetXMLData(String accessKey, String isbnStr)
        {
            String results = null;
            XmlTextReader reader = null;

            try
            {
                String ISBNUriStr = "http://isbndb.com//api/books.xml?access_key=" +
                    accessKey + "&results=details&index1=isbn&value1=";
                
                Uri uri = null;

                if (isbnStr != null)
                    uri = new Uri(ISBNUriStr + isbnStr);

                WebRequest http = HttpWebRequest.Create(uri);
                HttpWebResponse response = (HttpWebResponse)http.GetResponse();
                Stream stream = response.GetResponseStream();

                reader = new XmlTextReader(stream);

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        String name = reader.Name;

                        if (name == "BookData")
                        {
                            if (reader.MoveToFirstAttribute())
                            {
                                results += reader.Name + ": ";
                                results += reader.Value + "\r\n";
                            }

                            while (reader.MoveToNextAttribute())
                            {
                                results += reader.Name + ": ";
                                results += reader.Value + "\r\n";
                            }
                        }

                        else if (name == "Title")
                            results += "Title: " + reader.ReadString() + "\r\n";

                        else if (name == "TitleLongText")
                            results += "Title Long: " + reader.ReadString() + "\r\n";

                        else if (name == "AuthorsText")
                            results += "Authors: " + reader.ReadString() + "\r\n";

                        else if (name == "PublisherText")
                            results += "Publisher: " + reader.ReadString() + "\r\n";

                        else if (name == "Details")
                        {
                            if (reader.MoveToFirstAttribute())
                            {
                                results += reader.Name + ": ";
                                results += reader.Value + "\r\n";
                            }

                            while (reader.MoveToNextAttribute())
                            {
                                results += reader.Name + ": ";
                                results += reader.Value + "\r\n";
                            }
                        }

                        reader.Read();
                    }
                    
                }

                return results;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}