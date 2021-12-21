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

        public async ValueTask<HttpWebResponse> CreateRequestAsync(string url, Func<HttpWebRequest, Task> custom = null) => (await RequestHelper.CreateDeafultAsync(url, custom)) as HttpWebResponse;

        public ValueTask<HttpWebResponse> CreateRequestAsync(string url) => CreateRequestAsync(url, default);

        public ValueTask<HttpWebResponse> CreateRequestAsync(string url, Action<HttpWebRequest> custom = null) => CreateRequestAsync(url, req =>
        {
            custom?.Invoke(req);
            return Task.CompletedTask;
        });
    }
}
