using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Web.Script.Serialization;

namespace Microsoft.Bot
{
    public class DataParser
    {
        // This class parses the JSON data returned from LUIS and ISBNdb to make it
        // easier to analyze.  Also handles some other string cleaning.

        public String ToParse;
        public List<String> Parsed;
        public List<Author> AuthorsReturned;
        public BookData BooksReturned;
        public LUISData LUISParsed;

        public DataParser(String Data)
        {
            ToParse = Data;
            Parsed = new List<string>();
            AuthorsReturned = new List<Author>();
            LUISParsed = new LUISData();
        }
        public void DBParse(string Intent)
        {
            // Simple parser which deserializes strings into classes based on  the intent of the query.

            if (Intent == LUISConstants.SearchByAuthorForBooks)
            {
                // Deserialize the string into Book and Author objects.
                try
                {
                    Author Deserialized = new JavaScriptSerializer().Deserialize<Author>(@ToParse);
                    AuthorsReturned.Add(Deserialized);
                }
                catch (Exception e) { string exception = e.Message; }
            }
            else if (Intent == LUISConstants.SearchByBookForAuthor || Intent == LUISConstants.SearchByBookForSynopsis)
            {
                // Deserialize the string into a list of books.
                try
                {
                    BooksReturned = new JavaScriptSerializer().Deserialize<BookData>(@ToParse);
                    ToParse = string.Empty;
                }
                catch (Exception e) { string exception = e.Message; }
            }
        }
        public void LUISParse()
        {
            // Deserialize the string into a LUISData structure
            try
            {
                LUISParsed = new JavaScriptSerializer().Deserialize<LUISData>(@ToParse);
            }
            catch (Exception e) { string exception = e.Message; }
        }
        public string StripHTML(string InpString)
        {
            // Strips the HTML tag information to make the return message easy to read
            // Only used on book sysnopsis and overviews.
            HtmlDocument Plot = new HtmlDocument();
            Plot.LoadHtml(InpString);

            return Plot.DocumentNode.InnerText;
        }
    }
}