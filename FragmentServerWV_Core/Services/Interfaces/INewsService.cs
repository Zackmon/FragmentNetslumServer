using System.Collections.Generic;
using System.Threading.Tasks;
using FragmentServerWV.Models;

namespace FragmentServerWV.Services.Interfaces
{
    public interface INewsService : IBaseService
    {
        
        Task RefreshNewsList();
        Task<List<NewsSectionModel>> GetNewsArticles();
        
        Task<List<NewsSectionModel>> GetNewsArticles(string saveID);

        Task<bool> CheckIfNewNewsForSaveId(string saveId);

        Task UpdateNewsLog(string saveId, ushort articleId);

    }
}