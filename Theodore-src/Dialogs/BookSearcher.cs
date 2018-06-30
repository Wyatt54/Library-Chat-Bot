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
using Newtonsoft.Json;

namespace Microsoft.Bot
{
    [Serializable]
    public class BookDialog : IDialog<object>
    {
        public Image UrlImage;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }
        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            // This task handles all of the message returning.  Can be considered the master function for the bot.

            var message = await argument;
            LUISRequest request = new LUISRequest();
            if (message.Type == ActivityTypes.ConversationUpdate)
            {
                IConversationUpdateActivity iConversationUpdated = message as IConversationUpdateActivity;
                if (iConversationUpdated != null)
                {
                    ConnectorClient connector = new ConnectorClient(new System.Uri(message.ServiceUrl));

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
                            await PostCard(context, Hero);
                        }

                    }

                }
            }
            else if (message.Type == ActivityTypes.Message)
            {
                // Here we have LUIS interpret the query
                await request.MakeRequest(message.Text.ToString());

                // Now we parse the data to figure out what to search for.
                DataParser LUISData = new DataParser(request.LUISResult);
                LUISData.LUISParse();

                // Check to see if LUIS understood the query
                var response = string.Empty;
                if (LUISData.LUISParsed.entities.Count == 0)
                {
                    response = "I'm sorry, I don't understand your message.";
                    await context.PostAsync(response);
                }
                else
                {
                    // It is time to search the ISBN database for the information.
                    SearchISBNdB search = new SearchISBNdB();
                    String accessKey = "";
                    String results = search.GetJSONData(accessKey, LUISData.LUISParsed.topScoringIntent.intent, LUISData.LUISParsed.entities);

                    // Now parse it to make it readable.
                    DataParser data = new DataParser(results);
                    data.DBParse(LUISData.LUISParsed.topScoringIntent.intent);


                    // All that is left is to format a response message.
                    TextInfo info = new CultureInfo("en-US", false).TextInfo;
                    if (LUISData.LUISParsed.topScoringIntent.intent == "SearchByAuthorForBooks")
                    {
                        // Grab the Author name.
                        string author = string.Empty;
                        foreach (Entity entity in LUISData.LUISParsed.entities)
                        {
                            if (entity.type == "Author")
                            {
                                author = entity.entity;
                                break;
                            }
                        }
                        author = info.ToTitleCase(author);
                        string CardTitle = "Books by " + author;

                        // Grab all the titles the author has written
                        List<CardAction> CardActions = new List<CardAction>();
                        List<string> BookCheck = new List<string>();
                        if (data.AuthorsReturned[0].books.Count > 0)
                        {
                            foreach (Book book in data.AuthorsReturned[0].books)
                            {
                                string title = book.title_long;
                                title = title.Replace('_', ' ');
                                title = title.Replace('-', ' ');


                                title = info.ToTitleCase(title);

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
                            await PostCard(context, Hero);
                        }
                        else
                        {
                            response = "Sorry that author does not exist in my database.";
                            await context.PostAsync(response);
                        }
                        
                    }
                    else if (LUISData.LUISParsed.topScoringIntent.intent == "SearchByBookForAuthor")
                    {
                        // First get the book name.
                        string book = string.Empty;
                        foreach (Entity entity in LUISData.LUISParsed.entities)
                        {
                            if (entity.type == "Book")
                            {
                                book = entity.entity;
                                break;
                            }
                        }
                        book = info.ToTitleCase(book);

                        string BookTitle = null;
                        string BookAuthor = null;
                        string BookImage = null;
                        // Now look through the returned books for the searched book.
                        if (data.BooksReturned != null)
                        {
                            foreach (var BookReturned in data.BooksReturned.books)
                            {
                                // Must make the cases identical to ensure accurate comparison
                                if (info.ToTitleCase(BookReturned.title_long).Contains(book))
                                {
                                    // Here we found the book, so lets grab the necessary info.
                                    BookTitle = BookReturned.title_long;
                                    BookAuthor = BookReturned.authors[0];
                                    BookImage = BookReturned.image;

                                    break;
                                }
                            }

                            // Make sure we found the book
                            if (BookTitle != null)
                            {
                                HeroCard Hero = null;
                                // Here we make a Hero Card if an image exists for us to use.
                                if (BookImage != null)
                                {
                                    // Now that we have the info, lets make a card to post.
                                    Hero = new HeroCard
                                    {
                                        Title = BookTitle,
                                        Subtitle = "Written by " + BookAuthor,
                                        Text = "",
                                        Images = new List<CardImage> { new CardImage(BookImage) },
                                        Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "Book details.", null, "Details on " + BookTitle, null, null), new CardAction(ActionTypes.ImBack, "More books by this author.", null, "Books by " + BookAuthor, null, null)}
                                    };
                                }
                                else
                                {
                                    // Make the card without the image
                                    Hero = new HeroCard
                                    {
                                        Title = BookTitle,
                                        Subtitle = "Written by " + BookAuthor,
                                        Text = "",
                                        Images = null,
                                        Buttons = null
                                    };
                                }

                                // Now we post the card.
                                await PostCard(context, Hero);
                            }
                            else
                            {
                                response = "Sorry I could not find that book in my database.";
                                await context.PostAsync(response);
                            }                            
                        }
                        else
                        {
                            response = "Sorry that book does not exist in my database.";
                            await context.PostAsync(response);
                        }
                    }
                    else if (LUISData.LUISParsed.topScoringIntent.intent == "SearchByBookForSynopsis")
                    {
                        // First get the book name.
                        string BookSearch = string.Empty;
                        foreach (Entity entity in LUISData.LUISParsed.entities)
                        {
                            if (entity.type == "Book")
                            {
                                BookSearch = info.ToTitleCase(entity.entity);
                                break;
                            }
                        }
                        // Search for the book in the Author data.
                        string BookName = string.Empty;
                        response = "Sorry, my database does not have that book.";
                        if (data.BooksReturned != null)
                        {
                            foreach (var book in data.BooksReturned.books)
                            {
                                BookName = book.title_long;
                                BookName = BookName.Replace('_', ' ');
                                BookName = BookName.Replace('-', ' ');
                                BookName = info.ToTitleCase(BookName);

                                // Try to get the synopsis, but it isn't always available.
                                // Otherwise grab the overview.
                                if (BookName.Contains(BookSearch))
                                {
                                    string PlotText = string.Empty;
                                    if (book.synopsys != null)
                                    {
                                        PlotText = data.StripXML(book.synopsys);
                                    }
                                    else if (book.overview != null)
                                    {
                                        PlotText = data.StripXML(book.overview);
                                    }
                                    else
                                    {
                                        response = "Sorry the database does not have info for that book.";
                                        await context.PostAsync(response);
                                        break;
                                    }

                                    // Now lets make a card to return the info.
                                    if (book.image != null)
                                    {
                                        var Hero = new HeroCard
                                        {
                                            Title = BookName,
                                            Subtitle = "",
                                            Images = new List<CardImage> { new CardImage(book.image) },
                                            Text = PlotText
                                        };

                                        // Post the card.
                                        await PostCard(context, Hero);
                                    }
                                    else
                                    {
                                        // If we don't have an image to use, just post the text.
                                        // Seems like the majority of books have images.
                                        await context.PostAsync(PlotText);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                
            }
            context.Wait(MessageReceivedAsync);
        }
        public async Task PostCard(IDialogContext context, HeroCard Hero)
        {
            // Posts hero cards.
            var CardMessage = context.MakeMessage();
            var attatchment = Hero.ToAttachment();
            CardMessage.Attachments.Add(attatchment);

            await context.PostAsync(CardMessage);
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

            if (Intent == "SearchByAuthorForBooks")
            {
                // Deserialize the string into Book and Author objects.
                try
                {
                    Author deserialized = new JavaScriptSerializer().Deserialize<Author>(@ToParse);
                    AuthorsReturned.Add(deserialized);
                }
                catch (Exception e) { string exception = e.Message; }
            }
            else if (Intent == "SearchByBookForAuthor" || Intent == "SearchByBookForSynopsis")
            {
                // Deserialize the string into a list of books.
                try
                {
                    BooksReturned = new JavaScriptSerializer().Deserialize<BookData>(@ToParse);
                    ToParse = string.Empty;
                }
                catch (Exception e) { string exception = e.Message; }
            }
            
            
            ToParse = string.Empty;
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
            while (InpString.Contains("<"))
            {
                int first = InpString.IndexOf('<');
                int second = InpString.IndexOf('>');

                InpString = InpString.Remove(first, second - first + 1);
            }
            return InpString;
        }
    }
    public class LUISRequest
    {
        public string LUISResult;
        public LUISRequest()
        {
            LUISResult = string.Empty;
        }
        public async Task MakeRequest(String query)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // app ID for the book recommender
            var luisAppId = "";
            var subscriptionKey = "";

            // The request header contains your subscription key
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // The "q" parameter contains the utterance to send to LUIS
            queryString["q"] = query;

            // These optional request parameters are set to their default values
            queryString["timezoneOffset"] = "0";
            queryString["verbose"] = "false";
            queryString["spellCheck"] = "false";
            queryString["staging"] = "false";

            var uri = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/" + luisAppId + "?" + queryString;
            var response = await client.GetAsync(uri);
            var strResponseContent = await response.Content.ReadAsStringAsync();
            LUISResult = strResponseContent.ToString();
        }
    }
    public class SearchISBNdB
    {
        public String GetJSONData(String AccessKey, String Intent, List<Entity> Entities)
        {
            String results = null;
            try
            {
                // To determine what kind of search we need to do, look at the Intent
                String ISBNUriStr = "https://api.isbndb.com/";
                if (Intent == "SearchByAuthorForBooks")
                {
                    ISBNUriStr += "author/";
                    foreach (var entity in Entities)
                    {
                        if (entity.type == "Author")
                        {
                            TextInfo info = new CultureInfo("en-US", false).TextInfo;
                            ISBNUriStr += info.ToTitleCase(entity.entity);
                            break;
                        }
                    }
                }
                else if (Intent == "SearchByBookForAuthor" || Intent == "SearchByBookForSynopsis")
                {
                    ISBNUriStr += "books/";
                    foreach (var entity in Entities)
                    {
                        if (entity.type == "Book")
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
                http.Headers["X-API-KEY"] = AccessKey;
                WebResponse response = http.GetResponse();
                Stream stream = response.GetResponseStream();

                StreamReader reader = new StreamReader(stream);
                results = reader.ReadToEnd();

                // For some unknown reason, when searching for books written by an author, some of the books will have synopsis or overview
                // details available, but when a book is searched these details are always absent.  So if the intent is SearchByBookForSynopsis
                // there must be another database search.
                // This seems to be an issue with the database itself, and the way it chooses to return data.

                if (Intent == "SearchByBookForSynopsis")
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
                    http.Headers["X-API-KEY"] = AccessKey;
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
    public class CardMaker
    {
        private static Attachment GetHeroCard()
        {
            var heroCard = new HeroCard
            {
                Title = "BotFramework Hero Card",
                Subtitle = "Your bots — wherever your users are talking",
                Text = "Build and connect intelligent bots to interact with your users naturally wherever they are, from text/sms to Skype, Slack, Office 365 mail and other popular services.",
                Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Get Started", value: "https://docs.microsoft.com/bot-framework") }
            };

            return heroCard.ToAttachment();
        }
    }

}
