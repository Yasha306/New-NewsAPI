using NewsAPI.Entities;
using NewsAPI.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsAPI.Abstractions
{
    interface INews
    {
        public Task<News> Get(long newsId);

        public Task Add(AddInputNewsModel news);

        public Task Update(long newsId, UpdateInputNewsModel news);

        public Task Delete(long newsId);
    }
}
