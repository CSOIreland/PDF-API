using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PDFapi.Data
{
    /// <summary>
    /// Contains methods for mapping across Convert requests, i.e. RESTful to Json-rpc
    /// </summary>
    internal class Convert_MAP
    {
        /// <summary>
        /// Map RESTful parameters to JsonRpc parameters for create
        /// </summary>
        /// <param name="restfulParameters"></param>
        /// <returns></returns>
        internal dynamic Create_MapParameters(dynamic restfulParameters, string encodedUrls)
        {
            List<string> urls = encodedUrls.Split(',').ToList();
            urls.ForEach(url => HttpUtility.UrlDecode(url));
            var prm = JObject.FromObject(new
            {

                urls,
                printOptions = JObject.FromObject(new PrintOptions())
            });
            return prm;
        }

    }
}
