using GeneralizableMoebooruAPI.Bases;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Wbooru.Kernel.Updater;
using Wbooru.Models;
using Wbooru.Models.Gallery;
using Wbooru.PluginExt;
using Wbooru.Utils;

namespace WbooruPlugin.CommonMoebooruGallery
{
    public class WbooruImageInfo : GalleryItem
    {
        public WbooruImageInfo(ImageInfo raw_info,string gallery_name)
        {
            Rating = raw_info.Rating;
            GalleryName = gallery_name;

            GalleryItemID = raw_info.Id.ToString();
            PreviewImageSize = new ImageSize(raw_info.ThumbnailImageUrl.ImageWidth, raw_info.ThumbnailImageUrl.ImageHeight);
            PreviewImageDownloadLink = raw_info.ThumbnailImageUrl.Url;
            DownloadFileName = $"{raw_info.Id} {string.Join(" ", raw_info.Tags)}";

            GalleryDetail = new GalleryImageDetail()
            {
                Author = raw_info.Author,
                CreateDate = raw_info.CreateDateTime,
                ID = raw_info.Id.ToString(),
                Rate = raw_info.Rating.ToString(),
                Resolution = raw_info.ImageUrls.OrderByDescending(x => x.FileLength).Select(x => new Size(x.ImageWidth, x.ImageHeight)).FirstOrDefault(),
                Score = raw_info.Score.ToString(),
                Source = raw_info.Source,
                Tags = raw_info.Tags,
                DownloadableImageLinks = raw_info.ImageUrls.Select(x=>new DownloadableImageLink() 
                {
                    Description = x.UrlDescription,
                    DownloadLink=x.Url,
                    FileLength = x.FileLength,
                    FullFileName = WebUtility.UrlDecode(Path.GetFileName(x.Url)),
                    Size = new Size(x.ImageWidth,x.ImageHeight)
                }).ToArray()
            };
        }

        public GalleryImageDetail GalleryDetail { get; set; }

        public Rating Rating { get; set; }
    }
}
