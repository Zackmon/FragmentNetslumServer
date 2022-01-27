using FragmentNetslumServer.Entities.Attributes;
using FragmentNetslumServer.Services;
using FragmentNetslumServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FragmentNetslumServer.Services.Extensions;

namespace FragmentNetslumServer.Entities.OpCodeHandlers.Data
{
    [OpCodeData(OpCodes.OPCODE_DATA_REGISTER_CHAR)]
    public sealed class OPCODE_DATA_REGISTER_CHAR : IOpCodeHandler
    {
        private readonly IGuildManagementService guildManagementService;

        public OPCODE_DATA_REGISTER_CHAR(IGuildManagementService guildManagementService)
        {
            this.guildManagementService = guildManagementService;
        }

        public Task<IEnumerable<ResponseContent>> HandleIncomingRequestAsync(RequestContent request)
        {
            request.Client._characterPlayerID = ExtractCharacterData(request.Client, request.Data);
            byte[] guildStatus = guildManagementService.GetPlayerGuild(request.Client._characterPlayerID);
            if (guildStatus[0] == 0x01)
            {
                request.Client.isGuildMaster = true;
            }
            request.Client._guildID = swap16(BitConverter.ToUInt16(guildStatus, 1));
            // The first byte is membership status: 0=none 1= master 2= member
            return Task.FromResult<IEnumerable<ResponseContent>>(new[] { request.CreateResponse(OpCodes.OPCODE_DATA_REGISTER_CHAROK, guildStatus) });
        }

        uint ExtractCharacterData(GameClientAsync client, byte[] data)
        {
            client.save_slot = data[0];
            client.char_id = ReadByteString(data, 1);
            int pos = 1 + client.char_id.Length;
            client.char_name = ReadByteString(data, pos);
            pos += client.char_name.Length;
            client.char_class = data[pos++];
            client.char_level = swap16(BitConverter.ToUInt16(data, pos));
            pos += 2;
            client.greeting = ReadByteString(data, pos);
            pos += client.greeting.Length;
            client.char_model = swap32(BitConverter.ToUInt32(data, pos));
            pos += 5;
            client.char_HP = swap16(BitConverter.ToUInt16(data, pos));
            pos += 2;
            client.char_SP = swap16(BitConverter.ToUInt16(data, pos));
            pos += 2;
            client.char_GP = swap32(BitConverter.ToUInt32(data, pos));
            pos += 4;
            client.online_god_counter = swap16(BitConverter.ToUInt16(data, pos));
            pos += 2;
            client.offline_godcounter = swap16(BitConverter.ToUInt16(data, pos));
            pos += 2;
            client.goldCoinCount = swap16(BitConverter.ToUInt16(data, pos));
            pos += 2;
            client.silverCoinCount = swap16(BitConverter.ToUInt16(data, pos));
            pos += 2;
            client.bronzeCoinCount = swap16(BitConverter.ToUInt16(data, pos));

            client.classLetter = GetCharacterModelClass(client.char_model);
            client.modelNumber = GetCharacterModelNumber(client.char_model);
            client.modelType = GetCharacterModelType(client.char_model);
            client.colorCode = GetCharacterModelColorCode(client.char_model);

            client.charModelFile = "xf" + client.classLetter + client.modelNumber + client.modelType + "_" + client.colorCode;
            return DBAccess.getInstance().PlayerLogin(client);
        }

        static char GetCharacterModelClass(uint modelNumber)
        {
            char[] classLetters = { 't', 'b', 'h', 'a', 'l', 'w' };
            int index = (int)(modelNumber & 0x0F);
            return classLetters[index];
        }

        static int GetCharacterModelNumber(uint modelNumber)
        {
            return (int)(modelNumber >> 4 & 0x0F) + 1;
        }

        static char GetCharacterModelType(uint modelNumber)
        {
            char[] typeLetters = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i' };
            int index = (int)(modelNumber >> 12) & 0x0F;
            return typeLetters[index];
        }

        static string GetCharacterModelColorCode(uint modelNumber)
        {
            string[] colorCodes = { "rd", "bl", "yl", "gr", "br", "pp" };
            int index = (int)(modelNumber >> 8) & 0x0F;
            return colorCodes[index];
        }

    }
}
