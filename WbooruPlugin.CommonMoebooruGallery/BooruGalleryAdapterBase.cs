using GeneralizableMoebooruAPI;
using GeneralizableMoebooruAPI.Bases;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wbooru;
using Wbooru.Galleries;
using Wbooru.Galleries.SupportFeatures;
using Wbooru.Models;
using Wbooru.Models.Gallery;
using Wbooru.Settings;
using Wbooru.UI.Pages;
using WbooruPlugin.CommonMoebooruGallery.InterfaceImpls;

namespace WbooruPlugin.CommonMoebooruGallery
{
    public abstract class BooruGalleryAdapterBase : Gallery,
        IGalleryTagSearch,
        IGallerySearchImage,
        IGalleryItemIteratorFastSkipable,
        IGalleryAccount,
        IGalleryNSFWFilter,
        IGalleryVote
    {
        public APIWrapperOption APIWrapperOption { get; }
        public APIWrapper APIWrapper { get; }

        public bool IsLoggined => current_info != null;

        private AccountInfo current_info;

        public CustomLoginPage CustomLoginPage => null;

        public BooruGalleryAdapterBase(string base_api_url, string password_salt)
        {
            APIWrapperOption = new APIWrapperOption()
            {
                ApiBaseUrl = base_api_url,
                PasswordSalts = password_salt,
                VoteValue = 3,
                HttpRequest = HttpRequestAdapter.Default,
                Log = LogAdapter.Default,
                TryGetValidFileSize = false,
                PicturesCountPerRequest = 20
            };

            APIWrapper = new APIWrapper(APIWrapperOption);
        }

        public IEnumerable<GalleryItem> GetImagesInternal(IEnumerable<string> tags = null, int page = 1)
        {
            var limit = Setting<GlobalSetting>.Current.GetPictureCountPerLoad;

            foreach (var item in APIWrapper.ImageFetcher.GetImages(tags,page).Select(x=>new WboorumageInfo(x) { GalleryName = GalleryName }))
            {
                //自我审查(
                if (!NSFWFilter(item))
                    continue;

                yield return item;
            }

            Log<BooruGalleryAdapterBase>.Info("there is no pic that gallery could provide.");
        }

        public bool NSFWFilter(GalleryItem item)
        {
            if (!SettingManager.LoadSetting<GlobalSetting>().EnableNSFWFileterMode)
                return true;

            if (item is WboorumageInfo pi)
            {
                if (pi.Rating == Rating.Safe)
                    return true;

                if (pi.Rating == Rating.Questionable && !Setting<CommonSetting>.Current.QuestionableIsNSFW)
                    return true;

                return false;
            }

            return false;
        }

        public override GalleryImageDetail GetImageDetial(GalleryItem item)
        {
            if (!((item as WboorumageInfo)?.GalleryDetail is GalleryImageDetail detail))
            {
                if (item.GalleryName != GalleryName)
                    throw new Exception($"This item doesn't belong with gallery {GalleryName}.");

                detail = (GetImage(item.GalleryItemID) as WboorumageInfo)?.GalleryDetail;
            }

            return detail;
        }

        public override GalleryItem GetImage(string id) => (int.TryParse(id, out var i) && APIWrapper.ImageFetcher.GetImageInfo(i) is ImageInfo info) ? new WboorumageInfo(info)
        {
            GalleryName = GalleryName
        } : default;

        public override IEnumerable<GalleryItem> GetMainPostedImages() => GetImagesInternal();

        public IEnumerable<Wbooru.Models.Tag> SearchTag(string keywords)
        {
            var tags = APIWrapper.TagSearcher.SearchTags(keywords);

            if (tags?.Count() == 0)
                return Enumerable.Empty<Wbooru.Models.Tag>();

            return tags.Select(x => new Wbooru.Models.Tag()
            {
                Name = x.Name,
                Type = (Wbooru.Models.TagType)(int)x.Type
            });
        }

        public IEnumerable<GalleryItem> SearchImages(IEnumerable<string> keywords) => GetImagesInternal(keywords);

        public IEnumerable<GalleryItem> IteratorSkip(int skip_count)
        {
            var limit_count = Setting<GlobalSetting>.Current.GetPictureCountPerLoad;

            var page = skip_count / limit_count + 1;
            skip_count = skip_count % Setting<GlobalSetting>.Current.GetPictureCountPerLoad;

            return GetImagesInternal(null, page).Skip(skip_count);
        }

        public void AccountLogin(AccountInfo info)
        {
            if (APIWrapper.AccountManager.Login(info.Name, info.Password))
            {
                current_info = info;
            }
        }

        public void AccountLogout()
        {
            APIWrapper.AccountManager.Logout();
            current_info = null;
        }

        public IEnumerable<GalleryItem> NSFWFilter(IEnumerable<GalleryItem> items) => items.Where(x => NSFWFilter(x));

        public void SetVote(GalleryItem item, bool is_mark) => APIWrapper.ImageVoter.SetVoteValue(int.Parse(item.GalleryItemID), is_mark);

        public bool IsVoted(GalleryItem item) => APIWrapper.ImageVoter.IsVoted(int.Parse(item.GalleryItemID));

        public IEnumerable<GalleryItem> GetVotedGalleryItem()
        {
            throw new NotImplementedException();
        }
    }
}
