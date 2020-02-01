using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Settings;
using Wbooru.Settings.UIAttributes;

namespace WbooruPlugin.CommonMoebooruGallery
{
    [Export(typeof(SettingBase))]
    public class CommonSetting : SettingBase
    {
        [NeedRestart]
        [Group("View Options")]
        [Description("钦定Questionable评价的图片也算是NSFW内容")]
        public bool QuestionableIsNSFW { get; set; } = true;

        [NeedRestart]
        [Group("Network Options")]
        [Description("能提前缓存图片详细信息的数据,前提GlobalSetting中的EnableMemoryCache为开启")]
        public bool CacheImagePostData { get; set; } = true;

        public Dictionary<string, PreCacheRecord> PreCacheRecordData { get; set; } = new Dictionary<string, PreCacheRecord>();

        public PreCacheRecord GetPreCacheRecordData(BooruGalleryAdapterBase gallery) => PreCacheRecordData.TryGetValue(gallery.GalleryName, out var record) ? record : PreCacheRecordData[gallery.GalleryName] = new PreCacheRecord();
    }
}
