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
    public class KonachanGallery : BooruGalleryAdapterBase
    {
        public KonachanGallery() : base("http://konachan.net/", "So-I-Heard-You-Like-Mupkids-?--your-password--")
        {

        }

        public override string GalleryName => "Konachan";
    }
}
