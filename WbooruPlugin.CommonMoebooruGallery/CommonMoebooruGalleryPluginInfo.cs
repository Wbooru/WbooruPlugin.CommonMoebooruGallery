using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Kernel.Updater;
using Wbooru.PluginExt;
using Wbooru.Utils;

namespace WbooruPlugin.CommonMoebooruGallery
{
    [Export(typeof(PluginInfo))]
    public class CommonMoebooruGalleryPluginInfo : PluginInfo,IPluginUpdatable
    {
        public override string PluginName => "Common Moebooru Gallery";

        public override string PluginProjectWebsite => "https://github.com/Wbooru/WbooruPlugin.CommonMoebooruGallery";

        public override string PluginAuthor => "MikiraSora";

        public override string PluginDescription => "Easy to implement features for different *booru gallery providers";

        public Version CurrentPluginVersion => GetType().Assembly.GetName().Version;

        public IEnumerable<ReleaseInfo> GetReleaseInfoList()
        {
            return UpdaterHelper.GetGithubAllReleaseInfoList("Wbooru", "WbooruPlugin.CommonMoebooruGallery");
        }
    }
}
