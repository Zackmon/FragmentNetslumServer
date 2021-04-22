﻿using FragmentServerWV.Entities.Attributes;
using FragmentServerWV.Services.Interfaces;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace FragmentServerWV.Entities.OpCodeHandlers.Regular
{
    [OpCode(OpCodes.OPCODE_KEY_EXCHANGE_REQUEST)]
    public sealed class OPCODE_KEY_EXCHANGE_REQUEST : IOpCodeHandler
    {
        public async Task<ResponseContent> HandleIncomingRequestAsync(RequestContent request)
        {
            var responseStream = new MemoryStream();
            responseStream.Write(request.Data, 4, 16);
            var from_key = responseStream.ToArray();
            var to_key = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(to_key);
            }
            await responseStream.DisposeAsync();
            request.Client.InitializeDecryptionKey(from_key);
            request.Client.InitializeEncryptionKey(to_key);

            responseStream = new MemoryStream();
            responseStream.WriteByte(0);
            responseStream.WriteByte(0x10);
            responseStream.Write(from_key, 0, 16);
            responseStream.WriteByte(0);
            responseStream.WriteByte(0x10);
            responseStream.Write(to_key, 0, 16);
            responseStream.Write(new byte[] { 0, 0, 0, 0xe, 0, 0, 0, 0, 0, 0 }, 0, 10);
            var responseArray = responseStream.ToArray();
            var checksum = Crypto.Checksum(responseArray);
            await responseStream.DisposeAsync();
            return new ResponseContent(request, OpCodes.OPCODE_KEY_EXCHANGE_RESPONSE, responseArray, checksum);
        }
    }
}
