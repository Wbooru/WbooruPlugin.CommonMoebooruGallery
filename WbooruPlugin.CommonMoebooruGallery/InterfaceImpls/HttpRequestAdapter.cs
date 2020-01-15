using GeneralizableMoebooruAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Network;

namespace WbooruPlugin.CommonMoebooruGallery.InterfaceImpls
{
    public class HttpRequestAdapter : IHttpRequest
    {
        public static IHttpRequest Default { get; } = new HttpRequestAdapter();

        public HttpWebResponse CreateRequest(string url, Action<HttpWebRequest> custom = null) => RequestHelper.CreateDeafult(url, custom) as HttpWebResponse;
    }
}
