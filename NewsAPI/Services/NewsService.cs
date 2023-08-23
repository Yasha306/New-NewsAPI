using NewsAPI.Abstractions;
using NewsAPI.Entities;
using NewsAPI.Entities.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using NewsAPI.RedisKeys;

namespace NewsAPI.Services
{
    public class NewsService : INews
    {
        IConfiguration configuration;

        private ConnectionMultiplexer _db;

        private IDatabase Redis => _db.GetDatabase();

        public static string connectionString;

        public static Dictionary<int, string> creators = new Dictionary<int, string>()
        {
           { 1,"Anderson Cooper"},
           { 2,"Rachel Maddow"},
           { 3,"Christiane Amanpour"},
           { 4,"Brian Williams"},
           { 5,"David Muir"},
           { 6,"Wolf Blitzer"},
           { 7,"Megyn Kelly"},
           { 8,"Lester Holt"},
           { 9,"Fareed Zakariar"},
           { 10,"Sean Hannity"},
        };


        public NewsService(IConfiguration _configuration)
        {
            configuration = _configuration;

            connectionString = configuration["ConnectionStrings:Redis"];
        }

        public async Task<News> Get(long newsId)
        {
            await CheckAndConnectRedisAsync();

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
            await CheckAndConnectRedisAsync();

            var id = await Redis.StringIncrementAsync("NewsId");

            HashEntry[] hashEntries =
            {
                new HashEntry(Keys.FieldTitle, news.Title),
                new HashEntry(Keys.FieldContent, news.Content),
                new HashEntry(Keys.FieldCreateDate, DateTime.Now.ToString()),
                new HashEntry(Keys.FieldCreatorName, creators[news.CreatorId])
            };

            await Redis.HashSetAsync(Keys.KeyNews + id, hashEntries);
        }

        public async Task Update(long newsId, UpdateInputNewsModel news)
        {
            await CheckAndConnectRedisAsync();

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
            await CheckAndConnectRedisAsync();

            if (!await Redis.KeyDeleteAsync(Keys.KeyNews + newsId))
            {
                throw new KeyNotFoundException();
            }
        }

        private async Task CheckAndConnectRedisAsync()
        {
            if (_db == null || !_db.IsConnected)
            {
                _db = await ConnectionMultiplexer.ConnectAsync(connectionString);
            }
        }
    }
}
