using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Galleries;

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
