using System;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using System.Web;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Drawing;
using System.Globalization;
using System.Collections.Specialized;
using System.Xml;
using HtmlAgilityPack;
using System.Windows;
using Newtonsoft.Json;




namespace Microsoft.Bot
{
    [Serializable]
    public class BookDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext Context)
        {
            Context.Wait(MessageReceivedAsync);
        }
        public async Task MessageReceivedAsync(IDialogContext Context, IAwaitable<IMessageActivity> Argument)
        {
            // This task handles all of the message returning.  Can be considered the master function for the bot.
            var message = await Argument;
            
            // Post the welcome message when somebody new joins the conversation
            if (message.Type == ActivityTypes.ConversationUpdate)
            {
                await PostWelcomeMessage(Context, message);
            }
            else if (message.Type == ActivityTypes.Message)
            {
                // Here we have LUIS interpret the query
                LUISRequest Request = new LUISRequest();
                await Request.MakeRequest(message.Text.ToString());

                // Now we parse the data to figure out what to search for.
                DataParser LUISData = new DataParser(Request.LUISResult);
                LUISData.LUISParse();

                // Check to see if LUIS understood the query
                if (LUISData.LUISParsed.entities.Count == 0)
                {
                    string response = "I'm sorry, I don't understand your message.";
                    await Context.PostAsync(response);
                }
                else
                {
                    // It is time to search the ISBN database for the information.
                    SearchISBNdB Search = new SearchISBNdB();
                    String Results = Search.GetJSONData(LUISData.LUISParsed.topScoringIntent.intent, LUISData.LUISParsed.entities);

                    // Now parse it to make it readable.
                    DataParser ISBNData = new DataParser(Results);
                    ISBNData.DBParse(LUISData.LUISParsed.topScoringIntent.intent);


                    // All that is left is to format a response message.
                    // Check the intent and post the appropriate card.
                    TextInfo Info = new CultureInfo("en-US", false).TextInfo;
                    if (LUISData.LUISParsed.topScoringIntent.intent == LUISConstants.SearchByAuthorForBooks)
                    {
                        await PostAuthorCard(Context, LUISData.LUISParsed.entities, ISBNData, Info);
                    }
                    else if (LUISData.LUISParsed.topScoringIntent.intent == LUISConstants.SearchByBookForAuthor)
                    {
                        await PostBookCard(Context, LUISData.LUISParsed.entities, ISBNData, Info);
                    }
                    else if (LUISData.LUISParsed.topScoringIntent.intent == LUISConstants.SearchByBookForSynopsis)
                    {
                        await PostSynopsisCard(Context, LUISData.LUISParsed.entities, ISBNData, Info);
                    }
                }
            }
            Context.Wait(MessageReceivedAsync);
        }
        public async Task PostWelcomeMessage(IDialogContext Context, IMessageActivity Message)
        {
            IConversationUpdateActivity iConversationUpdated = Message as IConversationUpdateActivity;
            if (iConversationUpdated != null)
            {
                ConnectorClient connector = new ConnectorClient(new System.Uri(Message.ServiceUrl));

                foreach (var member in iConversationUpdated.MembersAdded ?? System.Array.Empty<ChannelAccount>())
                {
                    // This means the bot is being added.
                    if (member.Id == iConversationUpdated.Recipient.Id)
                    {
                        // Tell users what the bot does.
                        var Hero = new HeroCard
                        {
                            Title = "Library Chat Bot",
                            Subtitle = "",
                            Text = "Hello! I am a bot designed to give you information on authors and books.  Wyatt Fraley made me.  Ask me anything you like!"
                        };
                        await PostCard(Context, Hero);
                    }
                }
            }
        }
        public async Task PostAuthorCard(IDialogContext Context, List<Entity> Entities, DataParser ISBNData, TextInfo Info)
        {
            // Grab the Author name.
            string author = string.Empty;
            foreach (Entity entity in Entities)
            {
                if (entity.type == LUISConstants.Author)
                {
                    author = entity.entity;
                    break;
                }
            }
            author = Info.ToTitleCase(author);
            string CardTitle = "Books by " + author;

            // Grab all the titles the author has written
            List<CardAction> CardActions = new List<CardAction>();
            List<string> BookCheck = new List<string>();
            if (ISBNData.AuthorsReturned[0].books.Count > 0)
            {
                foreach (Book book in ISBNData.AuthorsReturned[0].books)
                {
                    string title = book.title_long;
                    title = title.Replace('_', ' ');
                    title = title.Replace('-', ' ');


                    title = Info.ToTitleCase(title);

                    // Check to make sure its not a repeat.
                    if (!BookCheck.Contains(title))
                    {
                        // Add the book title to a list.
                        BookCheck.Add(title);

                        // Now lets make a card action.
                        if (book.image != null)
                        {
                            var Action = new CardAction(ActionTypes.ImBack, title, book.image, "details of " + title, null, null);
                            CardActions.Add(Action);
                        }
                    }
                }

                // Now that we have the list of books by the author, lets make the response card.
                var Hero = new HeroCard
                {
                    Title = CardTitle,
                    Subtitle = "",
                    Text = "Click on a button for more information.",
                    Images = null,
                    Buttons = CardActions
                };

                // Now we post the card.
                await PostCard(Context, Hero);
            }
            else
            {
                string response = "Sorry that author does not exist in my database.";
                await Context.PostAsync(response);
            }
        }
        public async Task PostBookCard(IDialogContext Context, List<Entity> Entities, DataParser ISBNData, TextInfo Info)
        {
            // First get the book name.
            string BookName = string.Empty;
            foreach (Entity entity in Entities)
            {
                if (entity.type == LUISConstants.Book)
                {
                    BookName = entity.entity;
                    break;
                }
            }
            BookName = Info.ToTitleCase(BookName);

            // Now look through the returned books for the searched book.
            if (ISBNData.BooksReturned != null)
            {
                Book FoundBook = new Book();
                foreach (var BookReturned in ISBNData.BooksReturned.books)
                {
                    // Must make the cases identical to ensure accurate comparison
                    if (Info.ToTitleCase(BookReturned.title_long).Contains(BookName))
                    {
                        // Here we found the book, so lets grab the necessary info.
                        FoundBook = BookReturned;
                        break;
                    }
                }

                // Make sure we found the book
                if (FoundBook.title_long != null)
                {
                    // Now that we have the info, lets make a card to post.
                    var Hero = new HeroCard
                    {
                        Title = FoundBook.title_long,
                        Subtitle = "Written by " + FoundBook.authors[0],
                        Text = "",
                        Images = new List<CardImage> { new CardImage(FoundBook.image) },
                        Buttons = new List<CardAction> {
                                        new CardAction(ActionTypes.ImBack, "Book details.", null, "Details of " + FoundBook.title_long, null, null),
                                        new CardAction(ActionTypes.ImBack, "More books by this author.", null, "Books by " + FoundBook.authors[0], null, null)
                            }
                    };
                    // Now we post the card.
                    await PostCard(Context, Hero);
                }
                else
                {
                    string Response = "Sorry I could not find that book in my database.";
                    await Context.PostAsync(Response);
                }
            }
            else
            {
                string Response = "Sorry that book does not exist in my database.";
                await Context.PostAsync(Response);
            }
        }
        public async Task PostSynopsisCard(IDialogContext Context, List<Entity> Entities, DataParser ISBNData, TextInfo Info)
        {
            // First get the book name.
            string BookSearch = string.Empty;
            foreach (Entity entity in Entities)
            {
                if (entity.type == LUISConstants.Book)
                {
                    BookSearch = Info.ToTitleCase(entity.entity);
                    break;
                }
            }
            // Search for the book in the Author data.
            string BookName = string.Empty;
            if (ISBNData.BooksReturned != null)
            {
                foreach (var book in ISBNData.BooksReturned.books)
                {
                    // All books returned from ISBN have '_' or '-' in place of spaces, so must replace those first.
                    BookName = book.title_long;
                    BookName = BookName.Replace('_', ' ');
                    BookName = BookName.Replace('-', ' ');
                    BookName = Info.ToTitleCase(BookName);

                    // Try to get the synopsis, but it isn't always available.
                    // Otherwise grab the overview.
                    if (BookName.Contains(BookSearch))
                    {
                        string PlotText = string.Empty;
                        if (book.synopsys != null)
                        {
                            PlotText = ISBNData.StripXML(book.synopsys);
                        }
                        else if (book.overview != null)
                        {
                            PlotText = ISBNData.StripXML(book.overview);
                        }
                        else
                        {
                            string response = "Sorry the database does not have info for that book.";
                            await Context.PostAsync(response);
                            break;
                        }

                        // Now lets make a card to return the info.
                        var Hero = new HeroCard
                        {
                            Title = BookName,
                            Images = new List<CardImage> { new CardImage(book.image) },
                            Text = PlotText
                        };

                        // Post the card.
                        await PostCard(Context, Hero);
                        break;
                    }
                }
            }
        }
        public async Task PostCard(IDialogContext Context, HeroCard Hero)
        {
            // Posts hero cards.
            var CardMessage = Context.MakeMessage();
            var attatchment = Hero.ToAttachment();
            CardMessage.Attachments.Add(attatchment);

            await Context.PostAsync(CardMessage);
        }
    }
    public class DataParser
    {
        // This class parses the JSON data returned from LUIS and ISBNdb to make it
        // easier to analyze.  Also handles some other string cleaning.

        public String ToParse;
        public List<String> Parsed;
        public List<Author> AuthorsReturned;
        public BookData BooksReturned;
        public LUISData LUISParsed;

        public DataParser(String data)
        {
            ToParse = data;
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
        public string StripXML(string InpString)
        {
            // Strips the xml tag information to make the return message easy to read
            // Only used on book sysnopsis and overviews.
            HtmlDocument Plot = new HtmlDocument();
            Plot.LoadHtml(InpString);
            

            return Plot.DocumentNode.InnerText;
        }
    }
    public class LUISRequest
    {
        public string LUISResult;
        public HttpClient Client;
        public NameValueCollection QueryString;


        public LUISRequest()
        {
            LUISResult = string.Empty;
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Keys.LUISSubscriptionKey);

            QueryString = HttpUtility.ParseQueryString(string.Empty);
            QueryString["timezoneOffset"] = "0";
            QueryString["verbose"] = "false";
            QueryString["spellCheck"] = "false";
            QueryString["staging"] = "false";
        }
        public async Task MakeRequest(String query)
        {
            // Add the query
            QueryString["q"] = query;
            
            var uri = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/" + Keys.LUISAppID + "?" + QueryString;
            var response = await Client.GetAsync(uri);
            var strResponseContent = await response.Content.ReadAsStringAsync();
            LUISResult = strResponseContent.ToString();
        }
    }
    public class SearchISBNdB
    {
        public String GetJSONData(String Intent, List<Entity> Entities)
        {
            String results = null;
            try
            {
                // To determine what kind of search we need to do, look at the Intent
                String ISBNUriStr = "https://api.isbndb.com/";
                if (Intent == LUISConstants.SearchByAuthorForBooks)
                {
                    ISBNUriStr += "author/";
                    foreach (var entity in Entities)
                    {
                        if (entity.type == LUISConstants.Author)
                        {
                            TextInfo info = new CultureInfo("en-US", false).TextInfo;
                            ISBNUriStr += info.ToTitleCase(entity.entity);
                            break;
                        }
                    }
                }
                else if (Intent == LUISConstants.SearchByBookForAuthor || Intent == LUISConstants.SearchByBookForSynopsis)
                {
                    ISBNUriStr += "books/";
                    foreach (var entity in Entities)
                    {
                        if (entity.type == LUISConstants.Book)
                        {
                            TextInfo info = new CultureInfo("en-US", false).TextInfo;
                            ISBNUriStr += info.ToTitleCase(entity.entity);
                            break;
                        }
                    }
                }



                Uri uri = null;
                uri = new Uri(ISBNUriStr);

                WebRequest http = WebRequest.Create(uri);
                http.Method = "GET";
                http.ContentType = "application/json";
                http.Headers["X-API-KEY"] = Keys.ISBNdBAccessKey;
                WebResponse response = http.GetResponse();
                Stream stream = response.GetResponseStream();

                StreamReader reader = new StreamReader(stream);
                results = reader.ReadToEnd();

                // For some unknown reason, when searching for books written by an author, some of the books will have synopsis or overview
                // details available, but when a book is searched these details are always absent.  So if the intent is SearchByBookForSynopsis
                // there must be another database search.
                // This seems to be an issue with the database itself, and the way it chooses to return data.

                if (Intent == LUISConstants.SearchByBookForSynopsis)
                {
                    ISBNUriStr = "https://api.isbndb.com/author/";
                    // Grab the author of the book.
                    DataParser data = new DataParser(results);
                    data.DBParse(Intent);

                    // Have to do this because the author data can be incomplete on some books.
                    foreach (var book in data.BooksReturned.books)
                    {
                        if (book.authors.Count != 0)
                        {
                            ISBNUriStr += book.authors[0];
                            break;
                        }
                    }

                    // Now do the search for the author.
                    uri = null;
                    uri = new Uri(ISBNUriStr);

                    http = WebRequest.Create(uri);
                    http.Method = "GET";
                    http.ContentType = "application/json";
                    http.Headers["X-API-KEY"] = Keys.ISBNdBAccessKey;
                    response = http.GetResponse();
                    stream = response.GetResponseStream();

                    reader = new StreamReader(stream);
                    results = reader.ReadToEnd();

                }


                return results;
            }
            catch (Exception e) { string exception = e.Message; return exception; }
        }
    }
}
