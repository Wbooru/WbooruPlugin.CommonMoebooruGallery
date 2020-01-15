using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.PluginExt;

namespace WbooruPlugin.CommonMoebooruGallery
{
    [Export(typeof(PluginInfo))]
    public class CommonMoebooruGalleryPluginInfo : PluginInfo
    {
        public override string PluginName => "Common Moebooru Gallery";

        public override string PluginProjectWebsite => "https://github.com/MikiraSora/GeneralizableMoebooruAPI";

        public override string PluginAuthor => "MikiraSora";

        public override string PluginDescription => "Easy to implement features for different *booru gallery providers";
    }
}
