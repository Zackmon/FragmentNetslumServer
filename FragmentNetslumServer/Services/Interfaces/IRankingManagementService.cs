using System.Collections.Generic;

namespace FragmentNetslumServer.Services.Interfaces
{
    public interface IRankingManagementService : IBaseService
    {
        List<byte[]> GetPlayerRanking(ushort categoryID, ushort classID);
        List<byte[]> GetRankingCategory();
        byte[] GetRankingPlayerInfo(uint playerID);
    }
}