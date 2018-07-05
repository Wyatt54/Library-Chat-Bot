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
            var Message = await Argument;
            
            // Post the welcome message when somebody new joins the conversation
            if (Message.Type == ActivityTypes.ConversationUpdate)
            {
                await PostWelcomeMessage(Context, Message);
            }
            else if (Message.Type == ActivityTypes.Message)
            {
                // Here we have LUIS interpret the query
                LUISRequest Request = new LUISRequest();
                await Request.MakeRequest(Message.Text.ToString());

                // Now we parse the data to figure out what to search for.
                DataParser LUISData = new DataParser(Request.LUISResult);
                LUISData.LUISParse();

                // Check to see if LUIS understood the query
                if (LUISData.LUISParsed.entities.Count == 0)
                {
                    await Context.PostAsync("I'm sorry, I don't understand your message.");
                }
                else
                {
                    // It is time to search the ISBN database for the information.
                    SearchISBNdB Search = new SearchISBNdB();
                    String Results = Search.GetJSONData(LUISData.LUISParsed.topScoringIntent.intent, LUISData.LUISParsed.entities);

                    // Now parse it to make it readable.
                    DataParser ISBNData = new DataParser(Results);
                    ISBNData.DBParse(LUISData.LUISParsed.topScoringIntent.intent);

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
                foreach (var Member in iConversationUpdated.MembersAdded ?? Array.Empty<ChannelAccount>())
                {
                    // This means the bot is being added.
                    if (Member.Id == iConversationUpdated.Recipient.Id)
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
            string Author = GrabEntity(Entities, LUISConstants.Author, Info);
            string CardTitle = String.Format("Books by {0}", Author);

            // Grab all the titles the author has written
            List<CardAction> CardActions = new List<CardAction>();
            List<string> BookCheck = new List<string>();
            if (ISBNData.AuthorsReturned[0].books.Count > 0)
            {
                foreach (var Book in ISBNData.AuthorsReturned[0].books)
                {
                    // Have to clean up the title since all books in the database have '-' and '_' in them.
                    string Title = Book.CleanTitle(Info);
                    
                    // Check to make sure its not a repeat.
                    if (!BookCheck.Contains(Title))
                    {
                        // Add the book title to a list.
                        BookCheck.Add(Title);

                        // Now lets make a card action.
                        if (Book.image != null)
                        {
                            var Action = new CardAction(ActionTypes.ImBack, Title, Book.image, "details of " + Title, null, null);
                            CardActions.Add(Action);
                        }
                    }
                }

                // Now that we have the list of books by the author, lets make the response card.
                var Hero = new HeroCard
                {
                    Title = CardTitle,
                    Text = "Click on a button for more information.",
                    Buttons = CardActions
                };

                // Now we post the card.
                await PostCard(Context, Hero);
            }
            else
            {
                await Context.PostAsync("Sorry that author does not exist in my database.");
            }
        }
        public async Task PostBookCard(IDialogContext Context, List<Entity> Entities, DataParser ISBNData, TextInfo Info)
        {
            // First get the book name.
            string BookName = GrabEntity(Entities, LUISConstants.Book, Info);

            // Now look through the returned books for the searched book.
            if (ISBNData.BooksReturned != null)
            {
                Book FoundBook = new Book();
                foreach (var Book in ISBNData.BooksReturned.books)
                {
                    // Must make the cases identical to ensure accurate comparison
                    if (Info.ToTitleCase(Book.title_long).Contains(BookName))
                    {
                        // Here we found the book, so lets grab the necessary info.
                        FoundBook = Book;
                        break;
                    }
                }

                // Make sure we found the book. Don't have to clean the title here.
                if (FoundBook.title_long != null)
                {
                    // Now that we have the info, lets make a card to post.
                    var Hero = new HeroCard
                    {
                        Title = FoundBook.title_long,
                        Subtitle = String.Format("Written by {0}", FoundBook.authors[0]),
                        Images = new List<CardImage> { new CardImage(FoundBook.image) },
                        Buttons = new List<CardAction> {
                                        new CardAction(ActionTypes.ImBack, "Book details.", null, String.Format("Details of {0}", FoundBook.title_long), null, null),
                                        new CardAction(ActionTypes.ImBack, "More books by this author.", null, String.Format("Books by {0}", FoundBook.authors[0]), null, null)
                            }
                    };
                    // Now we post the card.
                    await PostCard(Context, Hero);
                }
                else
                {
                    await Context.PostAsync("Sorry I could not find that book in my database.");
                }
            }
            else
            {
                await Context.PostAsync("Sorry that book does not exist in my database.");
            }
        }
        public async Task PostSynopsisCard(IDialogContext Context, List<Entity> Entities, DataParser ISBNData, TextInfo Info)
        {
            // First get the book name.
            string BookSearch = GrabEntity(Entities, LUISConstants.Book, Info);

            // Search for the book in the Author data.
            string BookName = string.Empty;
            if (ISBNData.BooksReturned != null)
            {
                foreach (var Book in ISBNData.BooksReturned.books)
                {
                    // Clean up the title first since they always have '-' and '_' in them.
                    BookName = Book.CleanTitle(Info);

                    // Try to get the synopsis, but it isn't always available.
                    // Otherwise grab the overview.
                    if (BookName.Contains(BookSearch))
                    {
                        string PlotText = string.Empty;
                        if (Book.synopsys != null)
                        {
                            PlotText = ISBNData.StripHTML(Book.synopsys);
                        }
                        else if (Book.overview != null)
                        {
                            PlotText = ISBNData.StripHTML(Book.overview);
                        }
                        else
                        {
                            await Context.PostAsync("Sorry the database does not have info for that book.");
                            break;
                        }

                        // Now lets make a card to return the info.
                        var Hero = new HeroCard
                        {
                            Title = BookName,
                            Images = new List<CardImage> { new CardImage(Book.image) },
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
            var Attatchment = Hero.ToAttachment();
            CardMessage.Attachments.Add(Attatchment);

            await Context.PostAsync(CardMessage);
        }
        public String GrabEntity(List<Entity> EntityList, String Type, TextInfo Info)
        {
            foreach (Entity Item in EntityList)
            {
                if (Item.type == Type)
                {
                    return Info.ToTitleCase(Item.entity);
                }
            }
            return string.Empty;
        }
    }
}
