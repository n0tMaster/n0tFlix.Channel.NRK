﻿using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace n0tFlix.Channel.NRK
{
    public class Channel : IChannel, IRequiresMediaInfoCallback, ISupportsLatestMedia
    {
        public string Name => Plugin.Instance.Name;

        public string Description => Plugin.Instance.Description;

        public string DataVersion => Plugin.Instance.Version.Build.ToString();

        public string HomePageUrl => "https://tv.nrk.no/";

        public ChannelParentalRating ParentalRating => ChannelParentalRating.GeneralAudience;

        private readonly ILogger<Channel> logger;
        private readonly IMemoryCache memoryCache;

        public Channel(ILogger<Channel> logger, IMemoryCache memoryCache)
        {
            this.logger = logger;
            this.memoryCache = memoryCache;
            this.logger.LogInformation("Initialized n0tFlix NRK web tv Channel");
        }

        public InternalChannelFeatures GetChannelFeatures()
        {
            //Todo get somebody to explain the meaning of all the choises here
            return new InternalChannelFeatures()
            {
                MediaTypes = new List<MediaBrowser.Model.Channels.ChannelMediaType>()
                {
                    MediaBrowser.Model.Channels.ChannelMediaType.Video
                },
                ContentTypes = new List<MediaBrowser.Model.Channels.ChannelMediaContentType>()
                 {
                    MediaBrowser.Model.Channels.ChannelMediaContentType.Clip,
                     MediaBrowser.Model.Channels.ChannelMediaContentType.Episode,
                      MediaBrowser.Model.Channels.ChannelMediaContentType.Movie,
                },
                DefaultSortFields = new List<MediaBrowser.Model.Channels.ChannelItemSortField>()
                 {
                      MediaBrowser.Model.Channels.ChannelItemSortField.Name,
                       MediaBrowser.Model.Channels.ChannelItemSortField.DateCreated,
                        MediaBrowser.Model.Channels.ChannelItemSortField.Runtime
                 },
                MaxPageSize = 100,
                SupportsContentDownloading = true,
                SupportsSortOrderToggle = true,
                AutoRefreshLevels = 4,
            };
        }

        public async Task<DynamicImageResponse> GetChannelImage(ImageType type, CancellationToken cancellationToken)
        {
            logger.LogInformation(nameof(GetChannelImage));
            if (type == ImageType.Thumb || type == ImageType.Primary || type == ImageType.Backdrop || type == ImageType.Menu)
            {
                var name = "n0tFlix.Channel.NRK.Images.logo.png";
                var response = new DynamicImageResponse
                {
                    Format = ImageFormat.Png,
                    HasImage = true,
                    Stream = GetType().Assembly.GetManifestResourceStream(name),
                };

                return response;
            }
            return await Task.FromResult<DynamicImageResponse>(null);
        }

        //Todo check with a developer how this should be setup in the best possible way because this is only guessing atm
        public IEnumerable<ImageType> GetSupportedChannelImages()
        {
            yield return ImageType.Primary;
            yield return ImageType.Thumb;
            yield return ImageType.Logo;
        }

        public async Task<ChannelItemResult> GetChannelItems(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(query.FolderId))
                return await Worker.GetChannelCategoriesAsync(logger, memoryCache);
            else if (query.FolderId.StartsWith("https://psapi.nrk.no/tv/pages/"))
                return await Worker.GetCategoryItemsAsync(query, logger, memoryCache);
            else if (query.FolderId.StartsWith("https://psapi.nrk.no/tv/catalog/series/") && !query.FolderId.Contains("seasons"))
                return await Worker.GetSeasonInfoAsync(query, logger, memoryCache);
            else if (query.FolderId.StartsWith("https://psapi.nrk.no/tv/catalog/series/") && query.FolderId.Contains("seasons"))
                return await Worker.GetEpisodeInfoAsync(query, logger, memoryCache);

            logger.LogInformation("ERROR ERROR ERROR INGEN FOLDERID DETECTA ");
            logger.LogInformation("ERROR ERROR ERROR INGEN FOLDERID DETECTA ");
            logger.LogInformation("ERROR ERROR ERROR INGEN FOLDERID DETECTA ");
            logger.LogInformation("ERROR ERROR ERROR INGEN FOLDERID DETECTA ");
            logger.LogInformation("ERROR ERROR ERROR INGEN FOLDERID DETECTA ");
            logger.LogInformation("ERROR ERROR ERROR INGEN FOLDERID DETECTA ");

            return null;
        }

        public bool IsEnabledFor(string userId)
        {
            //todo figue out a way to get a autentication popup for each user so we can implement custom user acounts for spotify and netflix integrations
            return true;
        }

        public async Task<IEnumerable<MediaSourceInfo>> GetChannelItemMediaInfo(string id, CancellationToken cancellationToken)
            => await Worker.GetChannelItemMediaInfo(id, logger, cancellationToken);

        public async Task<IEnumerable<ChannelItemInfo>> GetLatestMedia(ChannelLatestMediaSearch request, CancellationToken cancellationToken)
         => await Worker.GetHeadlinersInfoAsync(logger, memoryCache);
    }
}