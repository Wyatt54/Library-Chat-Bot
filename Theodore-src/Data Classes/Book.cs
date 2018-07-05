using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Microsoft.Bot
{
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
        public string CleanTitle(TextInfo Info)
        {
            title_long = title_long.Replace('_', ' ');
            title_long = title_long.Replace('-', ' ');
            title_long = Info.ToTitleCase(title_long);
            return title_long;
        }
    }

}