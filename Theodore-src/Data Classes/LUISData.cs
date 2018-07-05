using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Bot
{
    public class LUISData
    {
        public string query { get; set; }
        public TopScoringIntent topScoringIntent { get; set; }
        public List<Entity> entities { get; set; }
    }
}