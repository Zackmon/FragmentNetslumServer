using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FragmentServerWV.Enumerations;
using FragmentServerWV.Models;
using FragmentServerWV.Services.Interfaces;
using ImageMagick;
using Serilog;

namespace FragmentServerWV.Services
{
    public class NewsService : INewsService
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);
        private readonly ILogger _logger;
        private readonly Encoding _encoding;

        
        private static List<NewsSectionModel> _articleList;
        
        public string ServiceName => "News Section Service";
        public ServiceStatusEnum ServiceStatus => ServiceStatusEnum.Active;

       
        public NewsService(ILogger logger)
        {
            this._logger = logger;
            this._encoding = Encoding.GetEncoding("Shift-JIS");
        }
        
        /// <summary>
        /// Refresh the List of cached Articles (To be used only through web apis and on start up)
        /// </summary>
        public async Task RefreshNewsList()
        {
            List<NewsSectionModel> listOfArticles =  await Task.Run(() =>DBAcess.getInstance().GetNewsArticles());

            _articleList = new List<NewsSectionModel>();
            foreach (var article in listOfArticles)
            {
                article.ArticleByteArray = await ConvertNewsArticle(article);
                List<byte[]> imageInfo = await GetArticleImageData(article);
                if (imageInfo != null)
                {
                    article.ImageSizeInfo = imageInfo[0];
                    article.ImageDetails = imageInfo[1];    
                }
                else
                {
                    article.ImageSizeInfo = null;
;                   article.ImageDetails = null;
                }
                
                _articleList.Add(article);
            }
        }

        /// <summary>
        /// Get List of cached News Articles , if the cache is null then get from DB and cache it  
        /// </summary>
        /// <returns>List of News Section Model (Articles)</returns>
        public async Task<List<NewsSectionModel>> GetNewsArticles()
        {
            if (_articleList == null)
            {
                _logger.Warning("Article List is Empty although it shouldn't be , Retrieving again from DB");
                await RefreshNewsList();
            }

            return _articleList;
        }


        public async Task<List<NewsSectionModel>> GetNewsArticles(string saveId)
        {
            if (_articleList == null)
            {
                _logger.Warning("Article List is Empty although it shouldn't be , Retrieving again from DB");
                await RefreshNewsList();
            }

            List<NewsSectionModel> listOfArticles = new List<NewsSectionModel>();
            
            List<ushort> listOfReadArticles =  await Task.Run(() =>DBAcess.getInstance().GetNewsLog(saveId));

            foreach (var article in _articleList)
            {
                var clone = article.Clone();
                using MemoryStream memoryStream = new MemoryStream(); 
                
                await memoryStream.WriteAsync(clone.ArticleByteArray);
                if (listOfReadArticles.Contains(clone.ArticleID))
                {
                    // Article Already Read
                    await memoryStream.WriteAsync(BitConverter.GetBytes(UInt32.MinValue.Swap()), 0, 4);
                }
                else
                {
                    // The article is new and was never read
                    UInt32 isNew = 1;
                    await memoryStream.WriteAsync(BitConverter.GetBytes(isNew.Swap()), 0, 4);
                }

                clone.ArticleByteArray = memoryStream.ToArray();
                listOfArticles.Add(clone);
            }

            return listOfArticles;
        }

        public async Task<bool> CheckIfNewNewsForSaveId(string saveId)
        {
            List<ushort> listOfReadArticles =  await Task.Run(() =>DBAcess.getInstance().GetNewsLog(saveId));

            foreach (var article in _articleList)
            {
                if (!listOfReadArticles.Contains(article.ArticleID))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task UpdateNewsLog(string saveId, ushort articleId)
        {
            await Task.Run(() =>DBAcess.getInstance().UpdateNewsLog(saveId,articleId));
        }

        /// <summary>
        /// Convert Article model to byte array for PS2 Client
        /// </summary>
        /// <param name="article"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<byte[]> ConvertNewsArticle(NewsSectionModel article)
        {
            TimeSpan t = article.ArticleDate - UnixEpoch;
            ulong secondsSinceEpoch = Convert.ToUInt64(t.TotalSeconds);
            
            
            using MemoryStream memoryStream = new MemoryStream();
            await memoryStream.WriteAsync(BitConverter.GetBytes(article.ArticleID.Swap()));
            await memoryStream.WriteAsync(_encoding.GetBytes(article.ArticleTitle.PadRight(34, '\0')));
            await memoryStream.WriteAsync(_encoding.GetBytes(article.ArticleBody.PadRight(0x25a, '\0')));
            await memoryStream.WriteAsync(BitConverter.GetBytes(secondsSinceEpoch.Swap()),0,8);
            //await memoryStream.WriteAsync(BitConverter.GetBytes(isNew.Swap()),0,4); // this will be set based on the save ID
            

            return memoryStream.ToArray();
        }

        
        /// <summary>
        /// Get the bytearray of the image in the correct order and format 
        /// </summary>
        /// <param name="article"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<List<byte[]>> GetArticleImageData(NewsSectionModel article)
        {
            if (article.ArticleImage != null)
            {
                using MagickImage image = new MagickImage(article.ArticleImage);
                image.Format = MagickFormat.Tga;
                image.ColorType = ColorType.Palette;
                byte[] convertedImage = image.ToByteArray();
                
                const int colorMapLength = 768; //file[6] << 8 | file[5];
                byte[] colorData = new byte[0x300];
                Array.Copy(convertedImage,18,colorData,0,colorMapLength);
                byte[] imageData = new byte[convertedImage.Length - 19 - colorMapLength];
                Array.Copy(convertedImage,18 + colorMapLength,imageData,0,(imageData.Length));
                
                uint imageSize = (uint)(colorData.Length + imageData.Length);
                if (imageSize > 17152)
                {
                    _logger.Warning("Image size is bigger than 17152 the size is {0}",imageSize);
                }
                else
                {
                    _logger.Debug("Image size is equals to {0}",imageSize);
                }
                
                const ushort chunkCount = 1;
                
                using MemoryStream sizeInfoStream = new MemoryStream();
                await sizeInfoStream.WriteAsync(BitConverter.GetBytes(imageSize.Swap()));
                await sizeInfoStream.WriteAsync(BitConverter.GetBytes(chunkCount.Swap()));
                byte[] sizeInfo = sizeInfoStream.ToArray();
                
                using MemoryStream imageStream = new MemoryStream();
                
                await imageStream.WriteAsync(BitConverter.GetBytes(ushort.MinValue.Swap()));
                await imageStream.WriteAsync(BitConverter.GetBytes(imageSize.Swap()));
                await imageStream.WriteAsync(colorData);
                await imageStream.WriteAsync(imageData);

                byte[] imageDetails = imageStream.ToArray();
                
                return new List<byte[]>{sizeInfo,imageDetails};

            }

            return null;
        }
    }
}