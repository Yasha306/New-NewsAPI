﻿using NewsAPI.Abstractions;
using NewsAPI.Entities.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using News.DAL;
using News.DAL.Entities;

namespace NewsAPI.Services
{
    public class NewsService : INewsService
    {
        private static IDatabase Redis => RedisService.GetDB;

        private readonly NewsDbContext _newsDbContext;

        public static Dictionary<int, string> _creators = new Dictionary<int, string>()
        {
           { 1,"Emily McGarvey"},
           { 2,"Lucy Williamson"},
           { 3,"Christiane Amanpour"},
           { 4,"Brian Williams"},
           { 5,"David Muir"},
           { 6,"Wolf Blitzer"},
           { 7,"Megyn Kelly"},
           { 8,"Lester Holt"},
           { 9,"Fareed Zakariar"},
           { 10,"Sean Hannity"},
        };

        public NewsService(NewsDbContext newsDbContext)
        {
            _newsDbContext = newsDbContext;
        }

        public async Task<List<NewsEntity>> GetAll()
        {
            return await _newsDbContext.News.ToListAsync();
        }

        public async Task<NewsEntity> Get(long newsId)
        {
            NewsEntity news = new NewsEntity();

            if (await Redis.KeyExistsAsync(Keys.KeyNews + newsId))
            {
                var newsHashEntres = await Redis.HashGetAllAsync(Keys.KeyNews + newsId);

                foreach (var hashEntrie in newsHashEntres)
                {
                    switch (hashEntrie.Name.ToString())
                    {
                        case "Id":
                            news.Id = (long)hashEntrie.Value;
                            break;
                        case "Title":
                            news.Title = hashEntrie.Value.ToString();
                            break;
                        case "Content":
                            news.Content = hashEntrie.Value.ToString();
                            break;
                        case "CreateDate":
                            news.CreateDate = hashEntrie.Value.ToString();
                            break;
                        case "CreatorName":
                            news.CreatorName = hashEntrie.Value.ToString();
                            break;
                        case "UpdateCreatorName":
                            news.UpdateCreatorName = hashEntrie.Value.ToString();
                            break;
                        case "UpdateDate":
                            news.UpdateDate = hashEntrie.Value.ToString();
                            break;
                        default:
                            break;
                    }
                }

                return news;
            }

            news = await _newsDbContext.News.FindAsync(newsId);

            if (news == null)
            {
                throw new KeyNotFoundException();
            }

            HashEntry[] hashEntries =
            {
                new HashEntry(Keys.FieldId, news.Id),
                new HashEntry(Keys.FieldTitle, news.Title),
                new HashEntry(Keys.FieldContent, news.Content),
                new HashEntry(Keys.FieldCreateDate, news.CreateDate??string.Empty),
                new HashEntry(Keys.FieldCreatorName, news.CreatorName??string.Empty)
            };

            await Redis.HashSetAsync(Keys.KeyNews + newsId, hashEntries);

            return news;
        }

        public async Task Add(AddInputNewsModel addNews)
        {
            NewsEntity news = new NewsEntity
            {
                Title = addNews.Title,
                Content = addNews.Content,
                CreatorName = _creators[addNews.CreatorId],
                CreateDate = DateTime.Now.ToString()
            };

            try
            {
                var addedNews = await _newsDbContext.News.AddAsync(news);
                await _newsDbContext.SaveChangesAsync();

                HashEntry[] hashEntries =
                {
                    new HashEntry(Keys.FieldId, addedNews.Entity.Id),
                    new HashEntry(Keys.FieldTitle, addedNews.Entity.Title),
                    new HashEntry(Keys.FieldContent, addedNews.Entity.Content),
                    new HashEntry(Keys.FieldCreateDate, addedNews.Entity.CreateDate),
                    new HashEntry(Keys.FieldCreatorName, addedNews.Entity.CreatorName)
                };

                await Redis.HashSetAsync(Keys.KeyNews + addedNews.Entity.Id, hashEntries);
            }
            catch (DbUpdateException)
            {
                throw new DbUpdateException();
            }
        }

        public async Task Update(long newsId, UpdateInputNewsModel updateNews)
        {
            var oldNews = await _newsDbContext.News.FindAsync(newsId);

            if (oldNews == null)
            {
                throw new KeyNotFoundException();
            }

            CheckAndUpdateNews(oldNews, updateNews);
            await _newsDbContext.SaveChangesAsync();

            HashEntry[] hashEntries =
            {
               new HashEntry(Keys.FieldTitle, updateNews.Title),
               new HashEntry(Keys.FieldContent, updateNews.Content),
               new HashEntry(Keys.FieldUpdateDate, DateTime.Now.ToString()),
               new HashEntry(Keys.FieldUpdaterCreatorName, _creators[updateNews.UpdateCreatorId])
            };

            await Redis.HashSetAsync(Keys.KeyNews + newsId, hashEntries);
        }

        public async Task Delete(long newsId)
        {
            var news = await _newsDbContext.News.FindAsync(newsId);

            if (news == null)
            {
                throw new KeyNotFoundException();
            }

            _newsDbContext.News.Remove(news);
            await _newsDbContext.SaveChangesAsync();

            await Redis.KeyDeleteAsync(Keys.KeyNews + newsId);
        }

        private void CheckAndUpdateNews(NewsEntity oldNews, UpdateInputNewsModel updateNews)
        {
            if (!oldNews.Title.Equals(updateNews))
            {
                oldNews.Title = updateNews.Title;
            }
            if (!oldNews.Content.Equals(updateNews))
            {
                oldNews.Content = updateNews.Title;
            }

            oldNews.UpdateCreatorName = _creators[updateNews.UpdateCreatorId];
            oldNews.UpdateDate = DateTime.Now.ToString();
        }
    }
}
