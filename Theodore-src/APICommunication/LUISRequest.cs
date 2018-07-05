using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot
{
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
        public async Task MakeRequest(String Query)
        {
            // Add the query
            QueryString["q"] = Query;

            var Uri = String.Format("https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/{0}?{1}", Keys.LUISAppID, QueryString);
            var Response = await Client.GetAsync(Uri);
            var strResponseContent = await Response.Content.ReadAsStringAsync();
            LUISResult = strResponseContent.ToString();
        }
    }

}