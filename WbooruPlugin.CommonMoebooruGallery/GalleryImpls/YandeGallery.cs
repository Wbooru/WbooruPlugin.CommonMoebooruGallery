using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Wbooru.Galleries;
using Wbooru.Galleries.SupportFeatures;
using Wbooru.Models;
using Wbooru.Network;
using Wbooru.Settings;
using Wbooru.Utils;

namespace WbooruPlugin.CommonMoebooruGallery.GalleryImpls
{
    [Export(typeof(Gallery))]
    public class YandeGallery : BooruGalleryAdapterBase
    {
        public YandeGallery() : base("https://yande.re/", "choujin-steiner--your-password--")
        {

        }

        public override string GalleryName => "Yande";
    }
}
