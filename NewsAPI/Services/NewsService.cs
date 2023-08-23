using NewsAPI.Abstractions;
using NewsAPI.Entities;
using NewsAPI.Entities.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
using NewsAPI.RedisKeys;

namespace NewsAPI.Services
{
    public class NewsService : INews
    {
        private static IDatabase Redis => RedisService.GetDB;

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

        public async Task<News> Get(long newsId)
        {
            if (!await Redis.KeyExistsAsync(Keys.KeyNews + newsId))
            {
                throw new KeyNotFoundException();
            }

            var newsHashEntres = await Redis.HashGetAllAsync(Keys.KeyNews + newsId);

            News news = new News();

            foreach (var hashEntrie in newsHashEntres)
            {
                switch (hashEntrie.Name.ToString())
                {
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
                    default:
                        break;
                }
            }

            return news;
        }

        public async Task Add(AddInputNewsModel news)
        {
            var newsId = await Redis.StringIncrementAsync("NewsId");

            HashEntry[] hashEntries =
            {
                new HashEntry(Keys.FieldTitle, news.Title),
                new HashEntry(Keys.FieldContent, news.Content),
                new HashEntry(Keys.FieldCreateDate, DateTime.Now.ToString()),
                new HashEntry(Keys.FieldCreatorName, _creators[news.CreatorId])
            };

            await Redis.HashSetAsync(Keys.KeyNews + newsId, hashEntries);
        }

        public async Task Update(long newsId, UpdateInputNewsModel news)
        {
            if (!await Redis.KeyExistsAsync(Keys.KeyNews + newsId))
            {
                throw new KeyNotFoundException();
            }

            HashEntry[] hashEntries =
            {
               new HashEntry(Keys.FieldTitle, news.Title),
               new HashEntry(Keys.FieldContent, news.Content),
               new HashEntry(Keys.FieldUpdateCreatorId, news.UpdateCreatorId),
               new HashEntry(Keys.FieldUpdateDate, DateTime.Now.ToString())
            };

            await Redis.HashSetAsync(Keys.KeyNews + newsId, hashEntries);
        }

        public async Task Delete(long newsId)
        {
            if (!await Redis.KeyDeleteAsync(Keys.KeyNews + newsId))
            {
                throw new KeyNotFoundException();
            }

            await Redis.SetAddAsync("DeleteIds", newsId);
        }
    }
}
