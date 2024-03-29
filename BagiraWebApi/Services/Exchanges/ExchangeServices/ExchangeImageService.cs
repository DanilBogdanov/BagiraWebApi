﻿using BagiraWebApi.Services.Exchanges.DataModels;
using Microsoft.EntityFrameworkCore;
using PhotoSauce.MagicScaler;
using System.Diagnostics;

namespace BagiraWebApi.Services.Exchanges.ExchangeServices
{
    public class ExchangeImageService
    {
        const string IMG_ROOT_DIRECTORY = "wwwroot/bagira/img";
        const string IMAGE_EXT = ".jpg";
        private readonly string IMG_400_DIRECTORY = Path.Combine(IMG_ROOT_DIRECTORY, "400");
        private readonly string IMG_800_DIRECTORY = Path.Combine(IMG_ROOT_DIRECTORY, "800");
        private readonly ApplicationContext _context;
        private readonly Soap1C _soap1C;
        private readonly ILogger<Exchange1C> _logger;

        public ExchangeImageService(ApplicationContext context, Soap1C soap1C, ILogger<Exchange1C> logger)
        {
            _context = context;
            _soap1C = soap1C;
            _logger = logger;

            CheckDirectoryExist(IMG_400_DIRECTORY);
            CheckDirectoryExist(IMG_800_DIRECTORY);
        }

        public async Task<ExchangeResult> UpdateAsync(ExchangeGoodResult exchangeGoodResult)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var updatedCount = await UpdateImagesAsync(exchangeGoodResult.IdsToUpdateImages);
            var deletedCount = DeleteImages(exchangeGoodResult.IdsToDeleteImages);

            stopwatch.Stop();

            return new ExchangeResult
            {
                UpdatedCount = updatedCount,
                DeletedCount = deletedCount,
                ElapsedSec = stopwatch.Elapsed.TotalSeconds
            };
        }

        public async Task<ExchangeResult> CheckImagesAsync()
        {

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var goodIdsWithImage = await _context.Goods
                .AsNoTracking()
                .Where(good => good.ImgDataVersion != null)
                .Select(good => good.Id)
                .ToListAsync();

            var imagesIds400 = GetImgIdsFromDirectory(IMG_400_DIRECTORY);
            var imagesIds800 = GetImgIdsFromDirectory(IMG_800_DIRECTORY);

            var missingImgIds400 = goodIdsWithImage.Except(imagesIds400);
            var missingImgIds800 = goodIdsWithImage.Except(imagesIds800);
            var imgIdsToUpdate = missingImgIds400.Union(missingImgIds800).ToList();

            var unusedImgIds400 = imagesIds400.Except(goodIdsWithImage);
            var unusedImgIds800 = imagesIds800.Except(goodIdsWithImage);
            var imgIdsToDelete = unusedImgIds400.Union(unusedImgIds800).ToList();

            var countAddedImg = await UpdateImagesAsync(imgIdsToUpdate);
            var countDeletedImg = DeleteImages(imgIdsToDelete);

            stopwatch.Stop();

            return new ExchangeResult
            {
                CreatedCount = countAddedImg,
                DeletedCount = countDeletedImg,
                ElapsedSec = stopwatch.Elapsed.TotalSeconds
            };
        }

        private List<int> GetImgIdsFromDirectory(string path)
        {
            return new DirectoryInfo(path)
                .GetFiles()
                .Select(path => int.Parse(path.Name[..^IMAGE_EXT.Length]))
                .ToList();
        }

        private async Task<int> UpdateImagesAsync(List<int> goodIds)
        {
            int countUpdatedImages = 0;

            foreach (int id in goodIds)
            {
                try
                {
                    var imgSourceBase64 = await _soap1C.GetImage(id);
                    var imgSource = Convert.FromBase64String(imgSourceBase64);
                    var filePaths = GetFilePaths(id);

                    filePaths.ForEach(imageInfo => ResizeImg(imgSource, imageInfo));

                    countUpdatedImages += filePaths.Count;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Cannot update image to goodId: {ImagePath}. Exception: {Ex}", id, ex);
                }
            }

            return countUpdatedImages;
        }

        private int DeleteImages(List<int> goodIds)
        {
            var deletedCount = 0;

            foreach (int id in goodIds)
            {
                var filePaths = GetFilePaths(id);

                foreach (var imageInfo in filePaths)
                {
                    var isSuccess = DeleteImg(imageInfo);

                    if (isSuccess)
                    {
                        deletedCount++;
                    }
                }
            }

            return deletedCount;
        }

        private static void ResizeImg(byte[] source, ImageInfo imageInfo)
        {
            MagicImageProcessor.ProcessImage(
                source,
                imageInfo.FilePath,
                new ProcessImageSettings
                {
                    Width = imageInfo.Size,
                    Height = imageInfo.Size,
                    ResizeMode = CropScaleMode.Max,
                    MatteColor = System.Drawing.Color.White,
                });
        }

        private bool DeleteImg(ImageInfo imgInfo)
        {
            try
            {
                File.Delete(imgInfo.FilePath);
                return true;
            }
            catch
            {
                _logger.LogError("Cannot delete image: {FilePath}", imgInfo.FilePath);
                return false;
            }
        }

        private List<ImageInfo> GetFilePaths(int goodId)
        {
            string fileName = $"{goodId}{IMAGE_EXT}";

            string path400 = Path.Combine(IMG_400_DIRECTORY, fileName);
            var imgInfo400 = new ImageInfo { FilePath = path400, Size = 400 };

            string path800 = Path.Combine(IMG_800_DIRECTORY, fileName);
            var imgInfo800 = new ImageInfo { FilePath = path800, Size = 800 };

            return new() { imgInfo400, imgInfo800 };
        }

        private static void CheckDirectoryExist(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private class ImageInfo
        {
            public required string FilePath { get; set; }
            public required int Size { get; set; }
        }
    }
}
