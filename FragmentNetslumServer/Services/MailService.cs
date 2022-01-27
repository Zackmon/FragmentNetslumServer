using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FragmentNetslumServer.Enumerations;
using FragmentNetslumServer.Models;
using FragmentNetslumServer.Services.Interfaces;
using Serilog;

namespace FragmentNetslumServer.Services
{
    public sealed class MailService : IMailService
    {
        private const int MAXIMUM_NAME_SIZE = 18;
        private const int MAXIMUM_FACE_BYTES = 130;
        private const int MAXIMUM_SUBJECT_SIZE = 128;
        private const int MAXIMUM_BODY_SIZE = 1200;

        private readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1);
        private readonly ILogger logger;
        private readonly Encoding encoding;

        public string ServiceName => "Mail Delivery Service";

        public ServiceStatusEnum ServiceStatus => ServiceStatusEnum.Active;



        public MailService(ILogger logger)
        {
            this.logger = logger;
            this.encoding = Encoding.GetEncoding("Shift-JIS");
        }



        public async Task<IList<MailMetaModel>> GetMailAsync(int accountId)
        {
            logger.Information($"Account {accountId} has requested their mail", accountId);
            return await Task.Run(() => DBAccess.getInstance().GetAccountMail(accountId));
        }

        public async Task<MailBodyModel> GetMailContent(int mailId)
        {
            logger.Debug($"Fetching Mail Content for {mailId}", mailId);
            return await Task.Run(() => DBAccess.getInstance().GetMailBodyByMailId(mailId));
        }

        public async Task SaveMailAsync(byte[] data)
        {
            var receiver_accountID = new byte[4];
            var receiver = new byte[16];
            var sender_accountID = new byte[4];
            var sender = new byte[16];
            var subject = new byte[32];
            var body = new byte[1200];
            var face = new byte[25];

            logger.Debug("Taking data block and breaking it out...");
            Buffer.BlockCopy(data, 4, receiver_accountID, 0, 4);
            Buffer.BlockCopy(data, 8, receiver, 0, 16);
            Buffer.BlockCopy(data, 26, sender_accountID, 0, 4);
            Buffer.BlockCopy(data, 30, sender, 0, 16);
            Buffer.BlockCopy(data, 48, subject, 0, 32);
            Buffer.BlockCopy(data, 176, body, 0, 1200);
            Buffer.BlockCopy(data, 1378, face, 0, 25);
            logger.Debug("...completed; dumping primative information");

            logger.Debug("Receiving Account ID: " + swap32(BitConverter.ToUInt32(receiver_accountID)));
            logger.Debug("Receiver Name: " + encoding.GetString(receiver));
            logger.Debug("Sender Account ID: " + swap32(BitConverter.ToUInt32(sender_accountID)));
            logger.Debug("Sender Name: " + encoding.GetString(sender));
            logger.Debug("Subject Line: " + encoding.GetString(subject));
            logger.Debug("Message Body: " + encoding.GetString(body));
            logger.Debug("Face ID: " + encoding.GetString(face));

            var metaModel = new MailMetaModel
            {
                Receiver_Account_ID = (int)swap32(BitConverter.ToUInt32(receiver_accountID)),
                Receiver_Name = receiver,
                Sender_Account_ID = (int)swap32(BitConverter.ToUInt32(sender_accountID)),
                Sender_Name = sender,
                Mail_Subject = subject,
                date = DateTime.UtcNow,
                Mail_Delivered = false
            };

            var bodyModel = new MailBodyModel
            {
                Mail_Body = body,
                Mail_Face_ID = encoding.GetString(face)
            };

            logger.Information($"An email has been sent by {sender} to {receiver}. Saving...", sender, receiver);
            await Task.Run(() => DBAccess.getInstance().CreateNewMail(metaModel, bodyModel));
            logger.Information("The email has been saved to the database");
        }
        
        
        public async Task SaveGuildMailAsync(byte[] data)
        {
            var dateNow = DateTime.UtcNow;
            
            var guildIdByteArray = new byte[2]; // 0x00 0x01
            
            var senderAccountIdByteArray = new byte[4]; // 0x02 0x03 0x04 0x05
            var sender = new byte[16]; // 0x06 for 16 count
            var subject = new byte[32];// 0x18 = 24 for 32 count
            var body = new byte[1200]; // 0x98 = 152 for 1200 count
            var face = new byte[25]; // 0x54A = 1354 for 25 count

            logger.Debug("Taking data block and breaking it out...");
            Buffer.BlockCopy(data, 0, guildIdByteArray, 0, 2);
            Buffer.BlockCopy(data, 2, senderAccountIdByteArray, 0, 4);
            Buffer.BlockCopy(data, 6, sender, 0, 16);
            Buffer.BlockCopy(data, 24, subject, 0, 32);
            Buffer.BlockCopy(data, 152, body, 0, 1200);
            Buffer.BlockCopy(data, 1354, face, 0, 25);


            ushort guildId = BitConverter.ToUInt16(guildIdByteArray).Swap();
            GuildRepositoryModel guildRepositoryModel =  await Task.Run(() =>DBAccess.getInstance().GetGuildInfo(guildId));
            
            int senderAccountId = (int)BitConverter.ToUInt32(senderAccountIdByteArray).Swap();
            
            logger.Debug("...completed; dumping primative information");

            logger.Debug("Receiving Guild ID: {0}" ,guildId);
            logger.Debug("Receiver Guild Name: {0} " , encoding.GetString(guildRepositoryModel.GuildName)); // Get guild name from DB
            logger.Debug("Sender Account ID: {0}" , senderAccountId);
            logger.Debug("Sender Name: {0}" , encoding.GetString(sender));
            logger.Debug("Subject Line: {0}" , encoding.GetString(subject));
            logger.Debug("Message Body: {0}" , encoding.GetString(body));
            logger.Debug("Face ID: {0}" , encoding.GetString(face));

            
            List<CharacterRepositoryModel> guildMembers =  await Task.Run(() =>DBAccess.getInstance().GetAllGuildMembers(guildId));
            HashSet<int> receiverAccountIdSet = guildMembers.Select(m => m.accountID).ToHashSet(); // get unique accountIDs only

            if (receiverAccountIdSet.Contains(senderAccountId))
            {
                receiverAccountIdSet.Remove(senderAccountId);  // just so that we don't send the mail to the sender
            }

            MailBodyModel bodyModel = new MailBodyModel // this only needed to be created once
            {
                Mail_Body = body,
                Mail_Face_ID = encoding.GetString(face)
            };
            
            foreach (int receiverAccountId in receiverAccountIdSet)
            {
                
                MailMetaModel metaModel = new MailMetaModel
                {
                    Receiver_Account_ID = receiverAccountId,
                    Receiver_Name = guildRepositoryModel.GuildName,
                    Sender_Account_ID = senderAccountId,
                    Sender_Name = sender,
                    Mail_Subject = subject,
                    date = dateNow,
                    Mail_Delivered = false
                };
                await Task.Run(() => DBAccess.getInstance().CreateNewMail(metaModel, bodyModel));
            }
            
            logger.Information("An email has been sent by {0} to Guild Members of {1}. Saving...", encoding.GetString(sender), encoding.GetString(guildRepositoryModel.GuildName));
            
            logger.Information("The emails has been saved to the database");
        }

        public async Task<byte[]> ConvertMailMetaIntoBytes(MailMetaModel mail)
        {
            var messageID = BitConverter.GetBytes(swap32((uint)mail.Mail_ID)).ToList();
            var sender = mail.Sender_Name.ToList();
            var receiver = mail.Receiver_Name.ToList();
            var sender_accountID = BitConverter.GetBytes(swap32((uint)mail.Sender_Account_ID)).ToList();
            var receiver_accountID = BitConverter.GetBytes(swap32((uint)mail.Receiver_Account_ID)).ToList();
            var mail_subject = mail.Mail_Subject.ToList();


            while (sender.Count < MAXIMUM_NAME_SIZE)
            {
                sender.Add(0x00);
            }

            while (receiver.Count < MAXIMUM_NAME_SIZE)
            {
                receiver.Add(0x00);
            }

            while (mail_subject.Count < MAXIMUM_SUBJECT_SIZE)
            {
                mail_subject.Add(0x00);
            }

            var t = mail.date - UNIX_EPOCH;
            var date = new List<byte>() { 0x00, 0x00, 0x00, 0x00 };
            date.AddRange(BitConverter.GetBytes(swap32((uint)t.TotalSeconds)).ToList());

            var m = new MemoryStream();
            await m.WriteAsync(messageID.ToArray(), 0, messageID.Count);
            await m.WriteAsync(receiver_accountID.ToArray(), 0, sender_accountID.Count);
            await m.WriteAsync(date.ToArray(), 0, date.Count);
            await m.WriteAsync(new byte[] { 0x07 });
            await m.WriteAsync(sender_accountID.ToArray(), 0, sender_accountID.Count);
            await m.WriteAsync(sender.ToArray(), 0, sender.Count);
            await m.WriteAsync(BitConverter.GetBytes(0), 0, 4);
            await m.WriteAsync(receiver.ToArray(), 0, receiver.Count);
            await m.WriteAsync(mail_subject.ToArray(), 0, mail_subject.Count);

            return m.ToArray();
        }


       

        public async Task<byte[]> ConvertMailBodyIntoBytes(MailBodyModel bodyModel)
        {
            var face = encoding.GetBytes(bodyModel.Mail_Face_ID).ToList();
            var body = bodyModel.Mail_Body.ToList();
            while (face.Count < MAXIMUM_FACE_BYTES)
            {
                face.Add(0x00);
            }
            while (body.Count < MAXIMUM_BODY_SIZE)
            {
                body.Add(0x00);
            }

            var m = new MemoryStream();


            await m.WriteAsync(BitConverter.GetBytes(5), 0, 4);
            await m.WriteAsync(BitConverter.GetBytes(0), 0, 2);
            await m.WriteAsync(body.ToArray(), 0, body.Count);
            await m.WriteAsync(BitConverter.GetBytes(0), 0, 2);
            await m.WriteAsync(face.ToArray(), 0, face.Count);

            return m.ToArray();
        }


        static ushort swap16(ushort data) => data.Swap();

        static uint swap32(uint data) => data.Swap();


    }
}
