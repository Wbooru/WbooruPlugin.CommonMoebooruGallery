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
    public class YandeGallery : BooruGalleryAdapterBase,
        IGalleryTagMetaSearch
    {
        public YandeGallery() : base("https://yande.re/", "choujin-steiner--your-password--")
        {

        }

        public override string GalleryName => "Yande";

        #region Tag PreCache

        public IEnumerable<Wbooru.Models.Tag> StartPreCacheTags()
        {
            var record = Setting<CommonSetting>.Current.GetPreCacheRecordData(this);

            if (!record.FinishCache)
            {
                while (true)
                {
                    IEnumerable<Wbooru.Models.Tag> tags = Enumerable.Empty<Wbooru.Models.Tag>();

                    try
                    {
                        var url = $"https://yande.re/tag.json?order=count&page={record.CachedPages}&limit=100";
                        var array = RequestHelper.GetJsonContainer<JArray>(RequestHelper.CreateDeafult(url));

                        tags = array.Select(x => BuildTag(x)).ToArray();

                        if (!tags.Any())
                        {
                            //search done.
                            record.FinishCache = true;
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionHelper.DebugThrow(e);
                        Thread.Sleep(1000);
                    }

                    foreach (var tag in tags)
                        yield return tag;

                    record.CachedPages++;
                    Setting.ForceSave();
                }
            }
            else
            {
                //todo 更新以及维护
            }
        }

        private Tag BuildTag(JToken tag_json)
        {
            return new Tag()
            {
                Name = tag_json["name"].ToString(),
                Type = (Wbooru.Models.TagType)tag_json["type"].ToObject<int>()
            };
        }

        private readonly static Regex TAG_REGEX = new Regex(@"<li\sclass=""tag-type-(\w+)"">.*?tags=-?(.+?)\+?"".*?</li>");

        public IEnumerable<Tag> SearchTagMetaById(string id)
        {
            //https://yande.re/post/show/606506
            var url = $"https://yande.re/post/show/{id}";
            var html = RequestHelper.GetString(RequestHelper.CreateDeafult(url));

            foreach (Match match in TAG_REGEX.Matches(html))
            {
                if (!Enum.TryParse<TagType>(match.Groups[1].Value,true, out var type))
                    continue;

                yield return new Tag()
                {
                    Name = WebUtility.UrlDecode(match.Groups[2].Value),
                    Type = type
                };
            }
        }

        public IEnumerable<Tag> SearchTagMeta(params string[] tags)
        {
            var tasks = tags.Select(x => GetTagMeta(x)).ToArray();

            Task.WaitAll(tasks);

            return tasks.Select(x => x.Result).OfType<Tag>();
        }

        private async Task<Tag> GetTagMeta(string tag_name)
        {
            try
            {
                var url = $"https://yande.re/tag.json?name={tag_name}";
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
