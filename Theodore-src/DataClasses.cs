using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Bot
{
    public class BookData
    {
        public string total { get; set; }
        public List<Book> books { get; set; }
    }
    public class Book
    {
        public string title { get; set; }
        public string image { get; set; }
        public string title_long { get; set; }
        public string isbn { get; set; }
        public string isbn13 { get; set; }
        public string dewey_decimal { get; set; }
        public string format { get; set; }
        public string publisher { get; set; }
        public string language { get; set; }
        public string date_published { get; set; }
        public string edition { get; set; }
        public string pages { get; set; }
        public string dimensions { get; set; }
        public string overview { get; set; }
        public string excerpt { get; set; }
        public string synopsys { get; set; }
        public List<string> authors { get; set; }
        public List<string> subjects { get; set; }
        public List<string> reviews { get; set; }
    }
    public class Author
    {
        public string name { get; set; }
        public List<Book> books { get; set; }
    }
    public class LUISData
    {
        public string query { get; set; }
        public TopScoringIntent topScoringIntent { get; set; }
        public List<Entity> entities { get; set; }
    }
    public class TopScoringIntent
    {
        public string intent { get; set; }
        public float score { get; set; }
    }
    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public float score { get; set; }
    }
}