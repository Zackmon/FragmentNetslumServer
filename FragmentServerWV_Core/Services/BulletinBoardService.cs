using FragmentServerWV.Enumerations;
using FragmentServerWV.Models;
using FragmentServerWV.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FragmentServerWV.Services
{
    public sealed class BulletinBoardService : IBulletinBoardService
    {

        private readonly Encoding encoding = Encoding.GetEncoding("Shift-JIS");


        public string ServiceName => "Bulletin Board Service";

        public ServiceStatusEnum ServiceStatus => ServiceStatusEnum.Active;




        public async Task<byte[]> ConvertCategoryToBytesAsync(BbsCategoryModel categoryModel)
        {
            var m = new MemoryStream();
            await m.WriteAsync(BitConverter.GetBytes(swap16((ushort)categoryModel.categoryID)), 0, 2);
            var buff2 = encoding.GetBytes(categoryModel.categoryName);
            await m.WriteAsync(buff2, 0, buff2.Length);
            while (m.Length < 0x24) m.WriteByte(0);
            return m.ToArray();
        }

        public async Task<byte[]> ConvertThreadDetailsToBytesAsync(BbsPostMetaModel postMetaModel)
        {
            var m = new MemoryStream();
            await m.WriteAsync(BitConverter.GetBytes(swap32((uint)postMetaModel.unk0)), 0, 4); //unk
            await m.WriteAsync(BitConverter.GetBytes(swap32((uint)postMetaModel.postID)), 0, 4); //postid
            await m.WriteAsync(BitConverter.GetBytes(swap32((uint)postMetaModel.unk2)), 0, 4); //unk2

            TimeSpan t = postMetaModel.date - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;
            await m.WriteAsync(BitConverter.GetBytes(swap32((uint)secondsSinceEpoch)), 0, 4); //date

            // Setting the username
            if (postMetaModel.username.Length > 16) //if the lenghth is more than 15 char then truncate 
            {
                byte[] temp = new byte[17];
                Buffer.BlockCopy(postMetaModel.username, 0, temp, 0, 16);
                postMetaModel.username = temp;
            }

            byte[] usernameBytes = postMetaModel.username;
            await m.WriteAsync(usernameBytes, 0, usernameBytes.Length); //username
            while (m.Length < 0x20) m.WriteByte(0);

            //setting the Subtitle
            if (postMetaModel.subtitle.Length > 17) //if the lengh is more than 17 then truncate
            {
                byte[] temp = new byte[17];
                Buffer.BlockCopy(postMetaModel.subtitle, 0, temp, 0, 17);
                postMetaModel.subtitle = temp;
            }

            byte[] subtitleBytes = postMetaModel.subtitle;
            await m.WriteAsync(subtitleBytes, 0, subtitleBytes.Length); // subtitles
            while (m.Length < 0x32) m.WriteByte(0);


            //setting unk3
            if (postMetaModel.unk3.Length > 45)
            {
                postMetaModel.unk3 = postMetaModel.unk3.Substring(0, 45);
            }

            byte[] unk3Bytes = encoding.GetBytes(postMetaModel.unk3);
            await m.WriteAsync(unk3Bytes, 0, unk3Bytes.Length);

            while (m.Length < 0x60) m.WriteByte(0);


            //setting the title

            if (postMetaModel.title.Length > 32) //if the length is more than 17 then truncate
            {
                byte[] temp = new byte[32];
                Buffer.BlockCopy(postMetaModel.title, 0, temp, 0, 32);
                postMetaModel.title = temp;
            }

            byte[] titleBytes = postMetaModel.title;
            await m.WriteAsync(titleBytes, 0, titleBytes.Length); // title
            while (m.Length < 0x80) m.WriteByte(0);
            return m.ToArray();
        }

        public async Task<byte[]> ConvertThreadPostToBytesAsync(BbsPostBody postBody)
        {
            var m = new MemoryStream();
            await m.WriteAsync(BitConverter.GetBytes(0), 0, 4);
            var bodyBytes = postBody.postBody;
            await m.WriteAsync(bodyBytes, 0, bodyBytes.Length);
            return m.ToArray();
        }

        public async Task<byte[]> ConvertThreadToBytesAsync(BbsThreadModel threadModel)
        {
            var m = new MemoryStream();
            await m.WriteAsync(BitConverter.GetBytes(swap32((ushort)threadModel.threadID)), 0, 4);
            byte[] threadTitleBytes = threadModel.threadTitle;
            await m.WriteAsync(threadTitleBytes, 0, threadTitleBytes.Length);
            while (m.Length < 0x26) m.WriteByte(0);
            return m.ToArray();
        }



        public async Task<IList<BbsCategoryModel>> GetCategoriesAsync()
        {
            return await Task.Run(() => DBAcess.getInstance().GetListOfBbsCategory());
        }

        public async Task<IList<BbsPostMetaModel>> GetThreadDetailsAsync(int threadId)
        {
            return await Task.Run(() => DBAcess.getInstance().GetPostsMetaByThreadId(threadId));
        }

        public async Task<BbsPostBody> GetThreadPostContentAsync(int postId)
        {
            return await Task.Run(() => DBAcess.getInstance().GetPostBodyByPostId(postId));
        }

        public async Task<IList<BbsThreadModel>> GetThreadsAsync(int categoryId)
        {
            return await Task.Run(() => DBAcess.getInstance().getThreadsByCategoryID(categoryId));
        }

        static ushort swap16(ushort data) => data.Swap();


        static uint swap32(uint data) => data.Swap();

    }
}
