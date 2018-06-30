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

            try
            {
                //String ISBNUriStr = "http://isbndb.com//api/books.xml?access_key=" +
                //accessKey + "&results=details&index1=isbn&value1=";
                //String ISBNUriStr = "https://api.isbndb.com/book/9781934759486";
                String ISBNUriStr = "https://api.isbndb.com/authors/alan";
                Uri uri = null;

                if (isbnStr != null)
                    //uri = new Uri(ISBNUriStr + isbnStr);
                    uri = new Uri(ISBNUriStr);

                WebRequest http = WebRequest.Create(uri);
                http.Method = "GET";
                http.ContentType = "application/json";
                http.Headers["X-API-KEY"] = accessKey;
                WebResponse response = http.GetResponse();
                Stream stream = response.GetResponseStream();

                StreamReader reader = new StreamReader(stream);
                results = reader.ReadToEnd();
                

                return results;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}