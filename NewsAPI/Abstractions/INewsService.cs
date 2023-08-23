using NewsAPI.Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using News.DAL.Entities;

namespace NewsAPI.Abstractions
{
    interface INewsService
    {
        public Task<List<NewsEntity>> GetAll();

        public Task<NewsEntity> Get(long newsId);

        public Task Add(AddInputNewsModel news);

        public Task Update(long newsId, UpdateInputNewsModel news);

        public Task Delete(long newsId);
    }
}
