using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Microsoft.Bot
{
    public class SearchISBNdB
    {
        public String GetJSONData(String Intent, List<Entity> Entities)
        {
            String Results = null;
            try
            {
                // To determine what kind of search we need to do, look at the Intent
                String ISBNUriStr = string.Empty;
                TextInfo Info = new CultureInfo("en-US", false).TextInfo;
                if (Intent == LUISConstants.SearchByAuthorForBooks)
                {
                    foreach (var Item in Entities)
                    {
                        if (Item.type == LUISConstants.Author)
                        {
                            ISBNUriStr = String.Format("https://api.isbndb.com/author/{0}", Info.ToTitleCase(Item.entity));
                            break;
                        }
                    }
                }
                else if (Intent == LUISConstants.SearchByBookForAuthor || Intent == LUISConstants.SearchByBookForSynopsis)
                {
                    foreach (var Item in Entities)
                    {
                        if (Item.type == LUISConstants.Book)
                        {
                            ISBNUriStr = String.Format("https://api.isbndb.com/books/{0}", Info.ToTitleCase(Item.entity));
                            break;
                        }
                    }
                }

                // Format the webrequest for the ISBN database.
                Uri Uri = new Uri(ISBNUriStr);

                WebRequest Http = WebRequest.Create(Uri);
                Http.Method = "GET";
                Http.ContentType = "application/json";
                Http.Headers["X-API-KEY"] = Keys.ISBNdBAccessKey;
                WebResponse Response = Http.GetResponse();
                Stream Stream = Response.GetResponseStream();

                StreamReader Reader = new StreamReader(Stream);
                Results = Reader.ReadToEnd();

                // For some unknown reason, when searching for books written by an author, some of the books will have synopsis or overview
                // details available, but when a book is searched these details are always absent.  So if the intent is SearchByBookForSynopsis
                // there must be another database search.
                // This seems to be an issue with the database itself, and the way it chooses to return data.
                if (Intent == LUISConstants.SearchByBookForSynopsis)
                {
                    // Grab the author of the book.
                    DataParser Data = new DataParser(Results);
                    Data.DBParse(Intent);

                    // Have to do this because the author data can be incomplete on some books.
                    foreach (var Book in Data.BooksReturned.books)
                    {
                        if (Book.authors.Count != 0)
                        {
                            ISBNUriStr = String.Format("https://api.isbndb.com/author/{0}", Book.authors[0]);
                            break;
                        }
                    }

                    // Now do the search for the author.
                    Uri = new Uri(ISBNUriStr);

                    Http = WebRequest.Create(Uri);
                    Http.Method = "GET";
                    Http.ContentType = "application/json";
                    Http.Headers["X-API-KEY"] = Keys.ISBNdBAccessKey;
                    Response = Http.GetResponse();
                    Stream = Response.GetResponseStream();

                    Reader = new StreamReader(Stream);
                    Results = Reader.ReadToEnd();
                }
                return Results;
            }
            catch (Exception e) { string exception = e.Message; return exception; }
        }
    }

}