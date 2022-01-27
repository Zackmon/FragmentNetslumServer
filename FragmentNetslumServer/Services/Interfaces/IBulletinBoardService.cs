using System.Collections.Generic;
using System.Threading.Tasks;
using FragmentNetslumServer.Models;

namespace FragmentNetslumServer.Services.Interfaces
{
    public interface IBulletinBoardService: IBaseService
    {

        Task<IList<BbsCategoryModel>> GetCategoriesAsync();

        Task<IList<BbsThreadModel>> GetThreadsAsync(int categoryId);

        Task<IList<BbsPostMetaModel>> GetThreadDetailsAsync(int threadId);

        Task<BbsPostBody> GetThreadPostContentAsync(int postId);



        Task<byte[]> ConvertCategoryToBytesAsync(BbsCategoryModel categoryModel);

        Task<byte[]> ConvertThreadToBytesAsync(BbsThreadModel threadModel);

        Task<byte[]> ConvertThreadDetailsToBytesAsync(BbsPostMetaModel postMetaModel);

        Task<byte[]> ConvertThreadPostToBytesAsync(BbsPostBody postBody);

    }
}
