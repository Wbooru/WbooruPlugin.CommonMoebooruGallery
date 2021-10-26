using GeneralizableMoebooruAPI;
using GeneralizableMoebooruAPI.Bases;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Wbooru;
using Wbooru.Galleries;
using Wbooru.Galleries.SupportFeatures;
using Wbooru.Models;
using Wbooru.Models.Gallery;
using Wbooru.Network;
using Wbooru.Settings;
using Wbooru.UI.Pages;
using Wbooru.Utils;
using WbooruPlugin.CommonMoebooruGallery.InterfaceImpls;

namespace WbooruPlugin.CommonMoebooruGallery
{
    public abstract class BooruGalleryAdapterBase : Gallery,
        IGalleryTagSearch,
        IGallerySearchImage,
        IGalleryItemIteratorFastSkipable,
        IGalleryAccount,
        IGalleryNSFWFilter,
        IGalleryVote,
        IGalleryTagMetaSearch
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
                PicturesCountPerRequest = (ulong)Setting<GlobalSetting>.Current.GetPictureCountPerLoad
            };

            APIWrapper = new APIWrapper(APIWrapperOption);
        }

        public IEnumerable<GalleryItem> GetImagesInternal(IEnumerable<string> tags = null, int page = 1)
        {
            var limit = Setting<GlobalSetting>.Current.GetPictureCountPerLoad;

            foreach (var item in APIWrapper.ImageFetcher.GetImages(tags, page)
                .Where(x =>
                {
                    CacheImageDetailData(x);
                    return true;
                })
                .Select(x => new WbooruImageInfo(x, GalleryName)))
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

            if (item is WbooruImageInfo pi)
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
            if (!((item as WbooruImageInfo)?.GalleryDetail is GalleryImageDetail detail))
            {
                if (item.GalleryName != GalleryName)
                    throw new Exception($"This item doesn't belong with gallery {GalleryName}.");

                detail = (GetImage(item.GalleryItemID) as WbooruImageInfo)?.GalleryDetail;
            }

            return detail;
        }

        public override GalleryItem GetImage(string id) 
        {
            if (int.TryParse(id, out var i))
            {
                if (TryGetCacheImageDetailData(i) is ImageInfo info1)
                    return new WbooruImageInfo(info1, GalleryName);


                if (APIWrapper.ImageFetcher.GetImageInfo(i) is ImageInfo info2)
                    return new WbooruImageInfo(info2, GalleryName);
            }
            return default;
        }

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

        #region Image Detail Data Cache

        private void CacheImageDetailData(ImageInfo image_detail)
        {
            if (!(Setting<CommonSetting>.Current.CacheImagePostData&& Setting<GlobalSetting>.Current.EnableFileCache))
                return;

            var path = Path.Combine(CacheFolderHelper.CacheFolderPath,$"{image_detail.Id}_{nameof(GalleryName)}_{nameof(ImageInfo)}.cache");
            var data = JsonConvert.SerializeObject(image_detail);

            File.WriteAllText(path, data);
        }

        private ImageInfo TryGetCacheImageDetailData(int id)
        {
            if (!(Setting<CommonSetting>.Current.CacheImagePostData && Setting<GlobalSetting>.Current.EnableFileCache))
                return null;

            var path = Path.Combine(CacheFolderHelper.CacheFolderPath, $"{id}_{nameof(GalleryName)}_{nameof(ImageInfo)}.cache");

            if (!File.Exists(path))
                return null;

            var data = File.ReadAllText(path);

            var image_detail = JsonConvert.DeserializeObject<ImageInfo>(data);

            return image_detail;
        }

        #endregion


        #region Tag PreCache

        public IEnumerable<Wbooru.Models.Tag> StartPreCacheTags()
        {
            var record = Setting<CommonSetting>.Current.GetPreCacheRecordData(this);

            const int check_point = 100;
            int current_added = 0;

            if (!record.FinishCache)
            {
                IEnumerable<Wbooru.Models.Tag> tags = Enumerable.Empty<Wbooru.Models.Tag>();

                try
                {
                    //page param not work for konachan
                    //var url = $"{APIWrapperOption.ApiBaseUrl}tag.json?order=count&page={record.CachedPages}&limit=100"

                    var url = $"{APIWrapperOption.ApiBaseUrl}tag.json?limit=0";
                    var array = RequestHelper.GetJsonContainer<JArray>(RequestHelper.CreateDeafult(url));

                    tags = array.Where(x => x["count"].ToObject<int>() > 0).Select(x => BuildTag(x)).Skip(record.CachedCount).ToArray();

                    if (!tags.Any())
                    {
                        //search done.
                        record.FinishCache = true;
                        yield break;
                    }
                }
                catch (Exception e)
                {
                    tags = Enumerable.Empty<Wbooru.Models.Tag>();
                    ExceptionHelper.DebugThrow(e);
                    Thread.Sleep(1000);
                }

                foreach (var tag in tags)
                {
                    yield return tag;

                    record.CachedCount++;
                    current_added++;

                    if (current_added>check_point)
                    {
                        current_added = 0;
                        Setting.ForceSave();
                    }
                }
            }
            else
            {
                //todo 更新以及维护
            }
        }

        private Wbooru.Models.Tag BuildTag(JToken tag_json)
        {
            return new Wbooru.Models.Tag()
            {
                Name = tag_json["name"].ToString(),
                Type = (Wbooru.Models.TagType)tag_json["type"].ToObject<int>()
            };
        }

        private readonly static Regex TAG_REGEX = new Regex(@"<li\sclass="".*?tag-type-(\w+).*?"">.*?tags=-?(.+?)\+?"".*?</li>");

        public IEnumerable<Wbooru.Models.Tag> SearchTagMetaById(string id)
        {
            var url = $"{APIWrapperOption.ApiBaseUrl}post/show/{id}";
            var html = RequestHelper.GetString(RequestHelper.CreateDeafult(url));

            foreach (Match match in TAG_REGEX.Matches(html))
            {
                if (!Enum.TryParse<Wbooru.Models.TagType>(match.Groups[1].Value, true, out var type))
                    continue;

                yield return new Wbooru.Models.Tag()
                {
                    Name = WebUtility.UrlDecode(match.Groups[2].Value),
                    Type = type
                };
            }
        }

        public IEnumerable<Wbooru.Models.Tag> SearchTagMeta(params string[] tags)
        {
            var tasks = tags.Select(x => GetTagMeta(x)).ToArray();

            Task.WaitAll(tasks);

            return tasks.Select(x => x.Result).OfType<Wbooru.Models.Tag>();
        }

        private async Task<Wbooru.Models.Tag> GetTagMeta(string tag_name)
        {
            try
            {
                var url = $"{APIWrapperOption.ApiBaseUrl}tag.json?name={tag_name}";
                var array = RequestHelper.GetJsonContainer<JArray>(await RequestHelper.CreateDeafultAsync(url));

                var result = array.Select(x => BuildTag(x)).OrderBy(x => Math.Abs(x.Name.Length - tag_name.Length)).ToArray();

                return result.FirstOrDefault();
            }
            catch (Exception e)
            {
                ExceptionHelper.DebugThrow(e);
                return null;
            }
        }

        #endregion
    }
}
