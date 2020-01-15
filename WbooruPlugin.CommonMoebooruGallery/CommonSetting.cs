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
        public bool QuestionableIsNSFW { get; set; }
    }
}
