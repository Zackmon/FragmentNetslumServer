using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FragmentServerWV
{
    public class Crypto
    {
        public byte[] secretKey;
        public uint secretKeyLength;
        public uint[] pArray;
        public List<uint[]> sBoxes;
        public Crypto()
        {
            PrepareStructure(Encoding.UTF8.GetBytes("hackOnline"));
        }

        public Crypto(byte[] key)
        {
            PrepareStructure(key);
        }

        public Crypto(string key, uint keyLen)
        {
            PrepareStructure(Encoding.UTF8.GetBytes(key));
        }

        public void PrepareStructure(byte[] key)
        {
            secretKeyLength = (uint)key.Length;
            secretKey = key;
            // Initialize Fields
            pArray = new uint[18];
            for (int i = 0; i < 18; i++)
                pArray[i] = default_parray[i];
            sBoxes = new List<uint[]>();
            for (int i = 0; i < 4; i++)
            {
                uint[] sBox = new uint[256];
                for (int j = 0; j < 256; j++)
                    sBox[j] = default_sboxes[i * 256 + j];
                sBoxes.Add(sBox);
            }
            // Mix Key into P-Array
            for (int i = 0; i < 18; i++)
            {
                // Roll Key on Edge
                byte[] rolledKeyBytes = new byte[4];
                for (int j = 0; j < 4; j++)
                    rolledKeyBytes[3 - j] = secretKey[(int)((i * 4 + j) % secretKeyLength)];

                // XOR P-Array Entry with Rolled Key
                pArray[i] ^= BitConverter.ToUInt32(rolledKeyBytes, 0);
            }

            // Encrypt P-Array
            uint[] tempChunks = new uint[] { 0, 0 };
            for (int i = 0; i < 9; i++)
            {
                // Encrypt 1st Half
                tempChunks[0] ^= pArray[0];
                tempChunks[1] ^= pArray[1] ^ rotateDword(tempChunks[0]);
                tempChunks[0] ^= pArray[2] ^ rotateDword(tempChunks[1]);
                tempChunks[1] ^= pArray[3] ^ rotateDword(tempChunks[0]);
                tempChunks[0] ^= pArray[4] ^ rotateDword(tempChunks[1]);
                tempChunks[1] ^= pArray[5] ^ rotateDword(tempChunks[0]);
                tempChunks[0] ^= pArray[6] ^ rotateDword(tempChunks[1]);
                tempChunks[1] ^= pArray[7] ^ rotateDword(tempChunks[0]);
                tempChunks[0] ^= pArray[8] ^ rotateDword(tempChunks[1]);

                // Encrypt 2nd Half
                tempChunks[1] = swap(tempChunks[1], tempChunks[0], 9);
                tempChunks[0] = swap(tempChunks[0], tempChunks[1], 10);
                tempChunks[1] = swap(tempChunks[1], tempChunks[0], 11);
                tempChunks[0] = swap(tempChunks[0], tempChunks[1], 12);
                tempChunks[1] = swap(tempChunks[1], tempChunks[0], 13);
                tempChunks[0] = swap(tempChunks[0], tempChunks[1], 14);
                tempChunks[1] = swap(tempChunks[1], tempChunks[0], 15);
                tempChunks[0] = swap(tempChunks[0], tempChunks[1], 16);

                // Write Data to P-Array
                pArray[i * 2] = pArray[17] ^ tempChunks[1];
                pArray[i * 2 + 1] = tempChunks[0];

                // Read Data for next Cycle
                tempChunks[0] = pArray[i * 2];
                tempChunks[1] = pArray[i * 2 + 1];
            }

            // Encrypt S-Boxes
            for (int i = 0; i < 4; i++)
            {
                // Encrypt S-Box
                for (int j = 0; j < 256; j += 2)
                {
                    // Encrypt Data
                    tempChunks[0] ^= pArray[0];
                    tempChunks[1] ^= pArray[1] ^ rotateDword(tempChunks[0]);
                    tempChunks[0] = swap(tempChunks[0], tempChunks[1], 2);
                    tempChunks[1] = swap(tempChunks[1], tempChunks[0], 3);
                    tempChunks[0] = swap(tempChunks[0], tempChunks[1], 4);
                    tempChunks[1] = swap(tempChunks[1], tempChunks[0], 5);
                    tempChunks[0] = swap(tempChunks[0], tempChunks[1], 6);
                    tempChunks[1] = swap(tempChunks[1], tempChunks[0], 7);
                    tempChunks[0] = swap(tempChunks[0], tempChunks[1], 8);
                    tempChunks[1] = swap(tempChunks[1], tempChunks[0], 9);
                    tempChunks[0] = swap(tempChunks[0], tempChunks[1], 10);
                    tempChunks[1] = swap(tempChunks[1], tempChunks[0], 11);
                    tempChunks[0] = swap(tempChunks[0], tempChunks[1], 12);
                    tempChunks[1] = swap(tempChunks[1], tempChunks[0], 13);
                    tempChunks[0] = swap(tempChunks[0], tempChunks[1], 14);
                    tempChunks[1] = swap(tempChunks[1], tempChunks[0], 15);
                    tempChunks[0] = swap(tempChunks[0], tempChunks[1], 16);

                    // Write Data to S-Box
                    sBoxes[i][j] = pArray[17] ^ tempChunks[1];
                    sBoxes[i][j + 1] = tempChunks[0];

                    // Read Data for next Cycle
                    tempChunks[0] = sBoxes[i][j];
                    tempChunks[1] = sBoxes[i][j + 1];
                }
            }
        }

        public byte[] Decrypt(byte[] payload)
        {

            byte[] result = new byte[payload.Length];
            // Invalid Payload Alignment / Size (has to be a multiple of 8)
            if ((payload.Length & 7) != 0) return result;
            for (int i = 0; i < payload.Length; i++)
                result[i] = payload[i];
            // Decrypt Payload
            for (int i = 0; i < payload.Length / 8; i++)
            {
                // Cast Buffer for use as a Chunk Pointer
                uint[] chunkBuffer = new uint[2];
                chunkBuffer[0] = BitConverter.ToUInt32(result, i * 8);
                chunkBuffer[1] = BitConverter.ToUInt32(result, i * 8 + 4);
                // Processing Variables
                uint runningChunk = 0;
                uint[] tempChunks = new uint[] { 0, 0 };

                // Decrypt Chunk
                runningChunk = chunkBuffer[0] ^ pArray[17];
                tempChunks[0] = runningChunk;
                runningChunk = chunkBuffer[1] ^ pArray[16] ^ rotateDword(runningChunk);
                tempChunks[1] = runningChunk;
                runningChunk = pArray[15] ^ rotateDword(runningChunk) ^ tempChunks[0];
                tempChunks[0] = runningChunk;
                runningChunk = pArray[14] ^ rotateDword(runningChunk) ^ tempChunks[1];
                tempChunks[1] = runningChunk;
                runningChunk = pArray[13] ^ rotateDword(runningChunk) ^ tempChunks[0];
                tempChunks[0] = runningChunk;
                runningChunk = pArray[12] ^ rotateDword(runningChunk) ^ tempChunks[1];
                tempChunks[1] = runningChunk;
                runningChunk = pArray[11] ^ rotateDword(runningChunk) ^ tempChunks[0];
                tempChunks[0] = runningChunk;
                runningChunk = pArray[10] ^ rotateDword(runningChunk) ^ tempChunks[1];
                tempChunks[1] = runningChunk;
                runningChunk = pArray[9] ^ rotateDword(runningChunk) ^ tempChunks[0];
                tempChunks[0] = runningChunk;
                runningChunk = pArray[8] ^ rotateDword(runningChunk) ^ tempChunks[1];
                tempChunks[1] = runningChunk;
                runningChunk = pArray[7] ^ rotateDword(runningChunk) ^ tempChunks[0];
                tempChunks[0] = runningChunk;
                runningChunk = pArray[6] ^ rotateDword(runningChunk) ^ tempChunks[1];
                tempChunks[1] = runningChunk;
                runningChunk = pArray[5] ^ rotateDword(runningChunk) ^ tempChunks[0];
                tempChunks[0] = runningChunk;
                runningChunk = pArray[4] ^ rotateDword(runningChunk) ^ tempChunks[1];
                tempChunks[1] = runningChunk;
                runningChunk = pArray[3] ^ rotateDword(runningChunk) ^ tempChunks[0];
                tempChunks[0] = runningChunk;
                runningChunk = pArray[2] ^ rotateDword(runningChunk) ^ tempChunks[1];
                tempChunks[1] = runningChunk;
                runningChunk = pArray[1] ^ rotateDword(runningChunk) ^ tempChunks[0];

                // Save Plaintext Chunk
                chunkBuffer[0] = pArray[0] ^ tempChunks[1];
                chunkBuffer[1] = runningChunk;

                byte[] buff = BitConverter.GetBytes(chunkBuffer[0]);
                for (int j = 0; j < 4; j++)
                    result[i * 8 + j] = buff[j];
                buff = BitConverter.GetBytes(chunkBuffer[1]);
                for (int j = 0; j < 4; j++)
                    result[i * 8 + j + 4] = buff[j];
            }
            return result;
        }

        public byte[] Encrypt(byte[] payload)
        {

            byte[] result = new byte[payload.Length];
            // Invalid Payload Alignment / Size (has to be a multiple of 8)
            if ((payload.Length & 7) != 0) return result;
            for (int i = 0; i < payload.Length; i++)
                result[i] = payload[i];
            // Encrypt Payload
            for (int i = 0; i < payload.Length / 8; i++)
            {
                // Cast Buffer for use as a Chunk Pointer
                uint[] chunkBuffer = new uint[2];
                chunkBuffer[0] = BitConverter.ToUInt32(result, i * 8);
                chunkBuffer[1] = BitConverter.ToUInt32(result, i * 8 + 4);
                // Processing Variables
                uint runningChunk = 0;
                uint[] tempChunks = new uint[] { 0, 0 };

                // Encrypt Chunk
                runningChunk = chunkBuffer[0] ^ pArray[0];
                tempChunks[0] = runningChunk;
                runningChunk = chunkBuffer[1] ^ pArray[1] ^ rotateDword(runningChunk);
                tempChunks[1] = runningChunk;
                runningChunk = pArray[2] ^ rotateDword(runningChunk) ^ tempChunks[0];
                tempChunks[0] = runningChunk;
                runningChunk = pArray[3] ^ rotateDword(runningChunk) ^ tempChunks[1];
                tempChunks[1] = runningChunk;
                runningChunk = pArray[4] ^ rotateDword(runningChunk) ^ tempChunks[0];
                tempChunks[0] = runningChunk;
                runningChunk = pArray[5] ^ rotateDword(runningChunk) ^ tempChunks[1];
                tempChunks[1] = runningChunk;
                runningChunk = pArray[6] ^ rotateDword(runningChunk) ^ tempChunks[0];
                tempChunks[0] = runningChunk;
                runningChunk = pArray[7] ^ rotateDword(runningChunk) ^ tempChunks[1];
                tempChunks[1] = runningChunk;
                runningChunk = pArray[8] ^ rotateDword(runningChunk) ^ tempChunks[0];
                tempChunks[0] = runningChunk;
                runningChunk = pArray[9] ^ rotateDword(runningChunk) ^ tempChunks[1];
                tempChunks[1] = runningChunk;
                runningChunk = pArray[10] ^ rotateDword(runningChunk) ^ tempChunks[0];
                tempChunks[0] = runningChunk;
                runningChunk = pArray[11] ^ rotateDword(runningChunk) ^ tempChunks[1];
                tempChunks[1] = runningChunk;
                runningChunk = pArray[12] ^ rotateDword(runningChunk) ^ tempChunks[0];
                tempChunks[0] = runningChunk;
                runningChunk = pArray[13] ^ rotateDword(runningChunk) ^ tempChunks[1];
                tempChunks[1] = runningChunk;
                runningChunk = pArray[14] ^ rotateDword(runningChunk) ^ tempChunks[0];
                tempChunks[0] = runningChunk;
                runningChunk = pArray[15] ^ rotateDword(runningChunk) ^ tempChunks[1];
                tempChunks[1] = runningChunk;
                runningChunk = pArray[16] ^ rotateDword(runningChunk) ^ tempChunks[0];

                // Save Encrypted Chunk
                chunkBuffer[0] = pArray[17] ^ tempChunks[1];
                chunkBuffer[1] = runningChunk;

                byte[] buff = BitConverter.GetBytes(chunkBuffer[0]);
                for (int j = 0; j < 4; j++)
                    result[i * 8 + j] = buff[j];
                buff = BitConverter.GetBytes(chunkBuffer[1]);
                for (int j = 0; j < 4; j++)
                    result[i * 8 + j + 4] = buff[j];
            }
            return result;
        }

        public uint swap(uint L, uint R, uint P)
        {
            uint result = L;
            result ^= pArray[P] ^ rotateDword(R);
            return result;
        }

        public uint rotateDword(uint value)
        {
            byte[] buff = BitConverter.GetBytes(value);
            return sBoxes[3][buff[0]] + (sBoxes[2][buff[1]] ^ (sBoxes[0][buff[3]] + sBoxes[1][buff[2]]));
        }

        public static ushort Checksum(byte[] data)
        {
            int dwordChunks = (int)(data.Length >> 2);
            int byteChunks = (int)(data.Length & 3);
            uint checksum = 0;
            for (int i = 0; i < dwordChunks; i++)
                checksum ^= BitConverter.ToUInt32(data, i * 4);
            byte b = BitConverter.GetBytes(checksum)[0];
            for (int i = 0; i < byteChunks; i++)
                b ^= data[dwordChunks * 4 + i];
            checksum = (uint)(checksum & 0xFFFFFF00) + b;
            checksum = (checksum >> 16) ^ checksum;
            return (ushort)checksum;
        }

        #region defaults
        static uint[] default_parray = new uint[]
        {
	        0x25406B89, 0x86A409D4, 0x141A8B2F, 0x04717445,
	        0xA50A3923, 0x2AA032D1, 0x092FFB99, 0xED4F6D8A,
	        0x462922E7, 0x39D11478, 0xBF5567D0, 0x35EA0D6D,
	        0xC1AD2AB8, 0xCA7D51DE, 0x4085D6B6, 0xB6480A18,
	        0x9317D6DA, 0x8A7AFC1C
        };

        static uint[] default_sboxes = new uint[]
        {
	        0xD2320CA7, 0x99E0B6AD, 0x30FE73DC, 0xD11BE0B8,
	        0xB9E2B0EE, 0x6B277F97, 0xBB7D9146, 0xF22D809A,
	        0x25A29A48, 0xB4926DF8, 0x0902F3E3, 0x868FFD17,
	        0x646A21D9, 0x72584F6A, 0xA559FFA4, 0xF5943E7F,
	        0x0E967590, 0x738FB759, 0x728CCE59, 0x83164BEF,
	        0x7C55A51E, 0xC35B5AB6, 0x9D31D63A, 0x2BF36114,
	        0xC6D2B124, 0x296186F1, 0xCB427A19, 0xB9DC39F0,
	        0x8F7ADDB1, 0x613B190F, 0x6D9F0F8C, 0xB11F8B3F,
	        0xD81678C2, 0xBE324C28, 0x79B030DB, 0x56615D61,
	        0xE75626F4, 0xAB56AC95, 0x58499963, 0x64E91541,
	        0x56CB3A6B, 0x2BAC11B7, 0xB5CD5D35, 0x1242E9CF,
	        0xA25587B0, 0x7D73EA94, 0xB4EF1512, 0x6470BD2B,
	        0x2CAAC65E, 0x751932F7, 0xCF5D3F17, 0x9C88941F,
	        0xB0D7BB34, 0x6D25D05D, 0x7B335482, 0x29968778,
	        0x3C904999, 0x6C4CBAB0, 0xC5C0E91C, 0x67292294,
	        0x62D90ACD, 0xFC22AA92, 0x497DAD61, 0x5EED8133,
	        0xF0855E5E, 0xEA8676B2, 0xDD272403, 0xEC661C89,
	        0x248A3F82, 0xD497ADC6, 0x106E70F4, 0x84F5433A,
	        0x2F0C4583, 0xA5852105, 0x6AC9F14B, 0x9F209C5F,
	        0x22C76943, 0xF7EA6D9B, 0x680D9D62, 0xACD489F1,
	        0x6B52A1D3, 0xD9553069, 0x9710A829, 0xAC5234A4,
	        0x6FF00C6D, 0x147B3CE5, 0xBB3CF151, 0x7FFC2B99,
	        0xA2F2661E, 0x3AB00277, 0x67CB5A3F, 0x83440F89,
	        0x8DEF871A, 0x4670A0B5, 0x7E85A6C4, 0x3C8C5FBF,
	        0xE17076D9, 0x86C22174, 0x411B45A0, 0x57C26BA7,
	        0x4FD4AB63, 0x37407807, 0x1CFFE073, 0x439C033E,
	        0x38D1D825, 0xD10B1349, 0xDC10EBD4, 0x4AF2C19C,
	        0x085473CA, 0x819A1C7C, 0x26D57AD9, 0xF7E9DFF8,
	        0xE4FF511B, 0xB77A4D3C, 0x986DE1BE, 0x05C107BB,
	        0xC2AA50B7, 0x41A061C5, 0x5F5D9FC3, 0x1A6B2564,
	        0x69FC70B0, 0x3F6D54B6, 0x143AB3EC, 0x3C53ED70,
	        0x6EFD5220, 0x9C31962D, 0xCD824645, 0xB05FBE0A,
	        0xBFE4D105, 0xDF344BFE, 0x67102908, 0x1A2F4CB4,
	        0xC1CCA958, 0x46C97510, 0xD30C603A, 0xBAD4FCDC,
	        0x567AC1BE, 0x1B61330B, 0xD7A201C7, 0x412D737A,
	        0x68A026FF, 0xFC20A4CD, 0x8FA6EAF9, 0xDC3323F9,
	        0x3D7617E0, 0xFE626C16, 0x30511FC9, 0xAE0653AC,
	        0x333EB6FB, 0xFE248861, 0x54327C49, 0x3F01E083,
	        0x9F5D58BC, 0xCB708DA1, 0x1B88572F, 0xE0186ADC,
	        0xD643A9F7, 0x297F00C4, 0xAD6833C7, 0x8D505674,
	        0x6A5C28B1, 0xBCCB59C9, 0xE200A45E, 0xB9F112A1,
	        0x11FB3E99, 0xFE2284B9, 0x4BFDB66D, 0x2ED2D45C,
	        0x9B54E57A, 0xB7F94666, 0xD38F4ABD, 0x4CFC9891,
	        0xE2DEF3DB, 0xA5CC7F34, 0x63FC1442, 0xCFE5C7E9,
	        0xF021CBDB, 0x37784D02, 0xD17F9FFF, 0x2CF220B5,
	        0x96DCDB4E, 0xAF919299, 0xEBAE8F72, 0x6C94D6A1,
	        0xD18FD2D1, 0xB0C826E1, 0x8F3D5C30, 0x8F7695B8,
	        0x90F7E3FC, 0xF3132C65, 0x8989B913, 0x910EF11D,
	        0x50AE5FA1, 0x6990C41D, 0xD2D0F292, 0xB4A9C2AE,
	        0x30302319, 0xBF0F1878, 0xEB762EFF, 0x8C0320A2,
	        0xE6A1CD10, 0xB67075E9, 0x19ADF4D7, 0xCF8AE39A,
	        0xB5A950E1, 0xFE14E1B8, 0x7DC53C82, 0xD3AEA9DA,
	        0x1760A367, 0x81967806, 0x94CD7415, 0x221B1578,
	        0xE7AE2166, 0x78B6FB87, 0xC85543F6, 0xFC9E36D0,
	        0xECCEB00D, 0x7C3F8AA1, 0xD7421CD4, 0xAF1F7F4A,
	        0x01260F2E, 0x2172B45F, 0x236901BC, 0x58B9E1B0,
	        0x2565379C, 0xF10ABA1F, 0x5664921E, 0x5AE0A7AB,
	        0x79C2448A, 0xDA5B5480, 0x217E5CA3, 0x03E6BAC6,
	        0x84270477, 0x6396D0AA, 0x12C91A69, 0x4F744B42,
	        0xB4482ECB, 0x7C15AA4B, 0x1C520153, 0x9B542A16,
	        0xD7105840, 0xBD9CC7E5, 0x2C61A577, 0x82E77501,
	        0x09BB70B6, 0x581CEA20, 0xF397ED6C, 0x2B0EDA16,
	        0xB7646622, 0xE8BAFAB7, 0x0035062F, 0xC6865765,
	        0x54B12E5E, 0xAAA090A2, 0x09BB489A, 0x6F86086B,
	        0x4C7B71EA, 0xB6B42A45, 0xDC760A2F, 0xC51A2724,
	        0xAE6FA7B1, 0x4AA8E07E, 0x9DEF61B9, 0x90EEB367,
	        0xEDAB8D72, 0x6A9B1800, 0x5765536D, 0xC3B29FE2,
	        0x1A3703A6, 0x760A4D2A, 0xA15A1441, 0xE5193B3F,
	        0x4055999B, 0x5C439E66, 0x6C90E5D7, 0x9AF840D7,
	        0xA2D39D08, 0xF0E931F6, 0x4E2E39E7, 0xF1265EC2,
	        0x4DDE2187, 0x8571EC27, 0x6483EAC7, 0x031FCD5F,
	        0x0A696C40, 0x3FBBF0CA, 0x3D981915, 0x6C6B71A2,
	        0x69803685, 0x53A1E387, 0xB89D5406, 0xAB510838,
	        0x3F08851D, 0x80DFAF5D, 0x8F7E45ED, 0x5817F3B9,
	        0xB13BDB38, 0xF1510D0E, 0xF11D2005, 0x0301B400,
	        0xAF0DF61B, 0x3DB675B3, 0x26847B59, 0xDD0A22BE,
	        0xD29214FA, 0x7DAA30F7, 0x95334874, 0x23F64802,
	        0x3BE6E682, 0x38C3DBDD, 0xC9B67735, 0x9BF4DEA8,
	        0xAA456247, 0x10D1040F, 0xEDC9C83F, 0xA5761F42,
	        0xE339CE9A, 0x3CEB0F30, 0x3381BCA2, 0x193FB432,
	        0x4F558C39, 0x506EBA09, 0x70430E04, 0xF70B05C0,
	        0x2DB91391, 0x25987D7A, 0x577AB173, 0xBDB08AB0,
	        0xDF9B7820, 0xDA940911, 0xB48CAF13, 0xDDD0402F,
	        0x56137320, 0x2F6C7225, 0x511BDEE7, 0xA085CE88,
	        0x7B594819, 0x7509DB18, 0xBDA09BBD, 0xEA4C7E8D,
	        0xED7BED3B, 0xDC861EFB, 0x640A4467, 0xC565C4D3,
	        0xF01D1948, 0x3316DA09, 0xDE443C38, 0x25C3BB17,
	        0x13A24E44, 0x2B66C552, 0x51950103, 0x143BE5DE,
	        0x72E0F99F, 0x11324F56, 0x82AD78D7, 0x60121A9C,
	        0x053657F2, 0xD8A4C86C, 0x3D12193C, 0x5A25A60A,
	        0xF390E7EE, 0x98F2FCFB, 0x9FBBC02D, 0x1F163D6F,
	        0x87E44671, 0xEBEA70B2, 0x870F5F0B, 0x5B3F2BB4,
	        0x7820E81D, 0x4F3E07FB, 0x2A66DDBA, 0x9AE81E10,
	        0x813F8AD7, 0x5367C926, 0x2F4DCA79, 0x9D11B46B,
	        0xC7160FBB, 0x95E3EB79, 0xA6FD3D54, 0x1F0B2EF5,
	        0xF3F84FA8, 0x371E2C3E, 0x1A3A2710, 0x1AC37A61,
	        0x5324A809, 0xF81413B7, 0xECAEFF6F, 0xEBC42067,
	        0xE4BD4696, 0xA77CC984, 0xB28038D2, 0x028D0029,
	        0xC433DEF0, 0xBF6D5BA6, 0x66592286, 0x69AC9903,
	        0xEFCFA610, 0xDC30963C, 0x2BF07EAE, 0x5C6F3085,
	        0x1622B729, 0x2A086271, 0xEDDE4876, 0x62A01611,
	        0x14CDA931, 0xEC62BE97, 0x0435FF1F, 0xAB0464D0,
	        0xB6745D91, 0x4D71A33A, 0xD69F9F0C, 0xCCABDF15,
	        0xEFCD87BD, 0x61632DA8, 0x9DAC5DAC, 0xB3F4856F,
	        0x658C1FB0, 0x1ABEF1CB, 0xA1246ABA, 0x665BBC51,
	        0x41695B33, 0x3D2BB5B4, 0x329FEAD6, 0xC122B9F8,
	        0x9C550C1A, 0x8860A19A, 0x96F89A7F, 0x633E7EA9,
	        0xF938899B, 0x98E42E78, 0x12EE9460, 0x17691382,
	        0x0F36892A, 0xC8E720D7, 0x97DFE0A2, 0x7959BB9A,
	        0x58F685A6, 0x1C237364, 0x9C84C400, 0x1BC34797,
	        0xCEB40BEC, 0x542F3155, 0x90DA49E5, 0x6EBD3229,
	        0x59ECF3F0, 0x35C700EB, 0xFF29EE62, 0xEF7D3D74,
	        0x5E4B15DA, 0xE965B8E4, 0x43115E15, 0x213F14E1,
	        0x46EFE3B7, 0xA4ABACEB, 0xDC6D5016, 0xFBCC50D1,
	        0xC843F543, 0xF06BBCB6, 0x66503C1E, 0x42CE2206,
	        0xD91F7A9F, 0x87864EC8, 0xE54C486B, 0x3E826351,
	        0xD063A2F3, 0x5C8E2747, 0xFD8984A1, 0xC2C8B7A4,
	        0x801625C4, 0x6ACC7593, 0x48858B0C, 0x5793B386,
	        0x0A5CC001, 0xAE1A499E, 0x1563B275, 0x24830F01,
	        0x59438E2B, 0x0D56F6EB, 0x1EAEF53F, 0x24407162,
	        0x3473F193, 0x8E947F42, 0xD760EDF2, 0x6D233CDC,
	        0x7DDF385A, 0xCCEF7561, 0x4186F3A8, 0xCF78336F,
	        0xA7088185, 0x1AF9519F, 0xE9F0D956, 0x62DA9836,
	        0xAA6AA8AB, 0xC60D07C3, 0x5B05ACFD, 0x810CCBDD,
	        0x9F457B2F, 0xC4463585, 0xFED66806, 0x0F1F9FCA,
	        0xDC74DCD4, 0x115689CE, 0x6860DB7A, 0xE4684441,
	        0xC6C53566, 0x723F39D9, 0x3E29F99F, 0xF26E0021,
	        0x163F22E8, 0x90B13E4B, 0xE7E4A02C, 0xDC84AEF8,
	        0xEA3E5B69, 0x958241F8, 0xF74D271D, 0x956A2A35,
	        0x421621F8, 0x7703D5F8, 0xBDF56C2F, 0xD5A30169,
	        0xD5092572, 0x3421F56B, 0x44B8D5B8, 0x510162B0,
	        0x1F3AF72F, 0x98254647, 0x15225075, 0xC08C8941,
	        0x4E96FD1E, 0x97B692B0, 0x71F5DED4, 0x67A13046,
	        0xC0BD0AED, 0x04BE9886, 0x80AD6ED1, 0x32CC8605,
	        0x97EC28B4, 0x56FE3A42, 0xDB2648E7, 0xACCB0B9B,
	        0x29517926, 0x54052AF5, 0x0B2D87DB, 0xEAB76EFC,
	        0x69DD1563, 0xD8496A01, 0x690FC1A5, 0x28A28EEF,
	        0x5040FFA3, 0xE988AE8D, 0xB68DE107, 0x7BF5D7B7,
	        0xABCF1F7D, 0xD43860ED, 0xCF79A49A, 0x416C2B43,
	        0x21FF9F36, 0xDAF486BA, 0xEF3AD8AC, 0x3C134F8C,
	        0x1ECAFBF8, 0x4C6E1957, 0x27A46732, 0xEBE498B3,
	        0x3B6FFB75, 0xDE5C4433, 0x6942E8F8, 0xCB7921FC,
	        0xFC0BF64F, 0xD9FFB498, 0x464157AD, 0xBB499628,
	        0x56543B3B, 0x21848E88, 0xFF6CAAB8, 0xD197964C,
	        0x56A968BD, 0xA2169B59, 0xCDAA2A64, 0x9AE2DC34,
	        0xA72B4B57, 0x403226FA, 0x5FF57F1D, 0x912A327D,
	        0xFEF9E903, 0x05283071, 0x81BC165D, 0x06292DE4,
	        0x96C21649, 0xE5C76E23, 0x49C21440, 0xC81087DD,
	        0x08FACAEF, 0x42052010, 0x41487AA5, 0x5E896F18,
	        0x336052EC, 0xD69CC1D2, 0xF3BDC290, 0x42123665,
	        0x267C7935, 0x612B9D61, 0xE0F9E9A4, 0x20646D1C,
	        0x0F13B5C3, 0x03E2339F, 0xB06750D2, 0xCBD28216,
	        0x6C2496E1, 0x343F93E2, 0x3C250C63, 0xEFBFBA23,
	        0x86B3A30F, 0xE7BB0E9A, 0xDF730D8D, 0x2EA3F829,
	        0xD1137946, 0x96B895FE, 0x657E0963, 0xE8CDF6F1,
	        0x554AA470, 0x887E49FB, 0xC49EFE28, 0xF43F8E1F,
	        0x0B486442, 0x9A2F0075, 0x3B706FAC, 0xF5F9FE38,
	        0xA913DD61, 0xA2ECDEF9, 0x9A1CE24D, 0xDC6F6C0E,
	        0xC77C5611, 0x6E682D38, 0x2866D53C, 0xDDD1E905,
	        0xF22A0EC8, 0xCD0100A4, 0xB63A1093, 0x6A10EE0C,
	        0x677CA0FC, 0xCFDC7E9D, 0xA192D00C, 0xDA165FA4,
	        0xBC143089, 0x525CAE25, 0x7C957AC0, 0x773CD7EC,
	        0x383A2FB4, 0xCD125A7A, 0x8127E398, 0xF52F322E,
	        0x6943AEA8, 0xC76B2C3C, 0x13764DCD, 0x792FF21D,
	        0x6B134338, 0xB89352E8, 0x07A2BCE7, 0x4CFC6451,
	        0x1B6C1119, 0x12CBEEFB, 0x3E26BED9, 0xE3E2C4CA,
	        0x4543175A, 0x0B131487, 0xDA0DED6F, 0xD6ACEB2B,
	        0x65B0684F, 0xDB87A960, 0xBFC0EA89, 0x65E5C4FF,
	        0x9EBD8158, 0xF1F8C187, 0x61797CF9, 0x6104614E,
	        0xD2FE8447, 0xF73920B1, 0x7846AF05, 0xD837FDCD,
	        0x84436C34, 0xF11FAC72, 0xB1814288, 0x3D015F60,
	        0x78A158BF, 0xBEE9AF25, 0x5647439A, 0xC0592F62,
	        0x4F59F590, 0xF3DEFEA3, 0xF575F039, 0x888ABEC3,
	        0x5467FAC4, 0xC9B48F75, 0xB576F356, 0x47FDDABA,
	        0x7BEC2762, 0x8C1EE085, 0x856B0F7A, 0x926096E3,
	        0x476F5A8F, 0x21B55871, 0x8DD65692, 0xCA03DF4D,
	        0xBA0CADE2, 0xBC8306D1, 0x12A96349, 0x7675AA9F,
	        0xB8801AB7, 0xE1AADD0A, 0x672E0AA2, 0xC5334734,
	        0xE95B2003, 0x0AF1BF8D, 0x4B9AA126, 0x1E6FFF11,
	        0x1BBA3E1E, 0x0CA6A5E0, 0xA287F310, 0x2969F26A,
	        0xDDB8DB84, 0x583A07FF, 0xA2E3CF9C, 0x50CE8053,
	        0x51125F02, 0xA80784FB, 0xA103B6C5, 0x0EE7D128,
	        0x9BF98D28, 0x78408742, 0xC4614D07, 0x62A907B6,
	        0xF1187B29, 0xC1F687E1, 0x016159AB, 0x31DD7E63,
	        0x12E79FD8, 0x2439EB64, 0x54C3DE95, 0xC3C31735,
	        0xBCCCEF57, 0x91BDB7DF, 0xECFD7EA2, 0xCF5A1E77,
	        0x7006E50A, 0x4C7D0289, 0x3A730B3E, 0x7D937D25,
	        0x87E47360, 0x734E9EBA, 0x1BC25CB5, 0xD49FB9FD,
	        0xEE555679, 0x09FDA6B6, 0xD93E7DD4, 0x4EAE10C5,
	        0x1F51F05F, 0xB262E7F9, 0xA38615DA, 0x6D52143D,
	        0x70D6C8E8, 0x57E24FC5, 0x372BC0CF, 0xDEC7C938,
	        0xD89B3335, 0x93648313, 0x680FFB8F, 0x416101E1,
	        0x3B3ACF38, 0xD4FBF6D0, 0xACC37838, 0x5BC62E1C,
	        0x5DB1689F, 0x50A43843, 0xD4832841, 0x9ABD9CBF,
	        0xD6128F9E, 0xC0107416, 0xD72E1D7F, 0xC801C57C,
	        0xB88D1C6C, 0x22A29146, 0xB36FB2BF, 0x6B376FB5,
	        0x5849AC30, 0xBD956F7A, 0xC7A477D3, 0x664AC3C9,
	        0x5410F9EF, 0x478EDF7E, 0xD6740B1E, 0x4DD14EC7,
	        0x2A3ABCDC, 0xAABB4751, 0xAD9627E9, 0xBF5FE405,
	        0xA2FBD6F1, 0x6B2E529B, 0x64F08DE3, 0x9B87EF23,
	        0xC18AC3B9, 0x44252FF7, 0xA61F04AB, 0x9DF3D1A5,
	        0x84C162BB, 0x9CEA6B4E, 0x90E61651, 0xBB655CD7,
	        0x2927A3FA, 0xA83B3BE2, 0x4CAA9687, 0xF05663EA,
	        0xC830F0D4, 0xF853F8DB, 0x4005706A, 0x78FB0B5A,
	        0x81E5AA16, 0x88B18702, 0x9C0AE7AE, 0x3C3FE694,
	        0xEA91FE5B, 0x9F35D898, 0x2DF1B8DA, 0x032C8C52,
	        0x97D6AD3B, 0x027EA77E, 0xD2D03FD7, 0x7D7E2E29,
	        0x20A026D0, 0xAEF3B99C, 0x5BD7B573, 0x5B89F64D,
	        0xE12AAD72, 0xE11AA6E7, 0x48B1ADFE, 0xEE94FB9C,
	        0xE9D4C58E, 0x293C58CD, 0xF9D6672A, 0x7A142F29,
	        0x79600292, 0xEE766156, 0xF8970F45, 0xE4D45F8D,
	        0x16066ED5, 0x89F56EBB, 0x04A26226, 0x0665F1BE,
	        0xC4EC9F16, 0x3D9158A3, 0x98281BED, 0xAA3B082B,
	        0x1C406E9C, 0x1F6422F6, 0xF69D67FC, 0x27DDF41A,
	        0x7634DA29, 0xB256FEF6, 0x04573583, 0x8BBB3DBC,
	        0x29527812, 0xC30BDAF9, 0xACCD5268, 0xCDAE9360,
	        0x4EE91852, 0x3931DD8F, 0x389E5963, 0x9421FA92,
	        0xEB7B91C3, 0xFC3F7CCF, 0x5222CF65, 0x7850BF33,
	        0xA9B7E47F, 0xC42A3E47, 0x49DF546A, 0x6514E781,
	        0xA3AF0911, 0xDE6EB325, 0x6A862EFE, 0x0A082267,
	        0xB49B470B, 0x6546C1DE, 0x596DDFD0, 0x1D21C9AF,
	        0x5CBFF8DE, 0x1C598E41, 0xCDD30280, 0x6CB5E4BC,
	        0xDEA36B7F, 0x3B5A0046, 0x3F360B45, 0xBDB5CED6,
	        0x73EBCFA9, 0xFB6585BC, 0x8E6713AF, 0xC03D7048,
	        0xD39CE564, 0x55305E9F, 0xAFC3781C, 0xF74F6471,
	        0x750F0E8E, 0xE85C1458, 0xF9731772, 0xB0547E5E,
	        0x4141CC09, 0x4FB5E3CD, 0x35D3476B, 0x0216B085,
	        0xE2B10529, 0x96993B1E, 0x07B9A0B5, 0xCF6FA149,
	        0x70403C83, 0x3621AC83, 0x021B1E4C, 0x287328F9,
	        0x621661B2, 0xE89440DD, 0xBC3B7A2C, 0x354626BE,
	        0xA1893AE2, 0x52CF7A4C, 0x3033CAB8, 0xA120BBCA,
	        0xE11DC97F, 0xBDC8D2F7, 0xD00212C4, 0xA2E9ABC8,
	        0x1B91884A, 0xD550BE9B, 0xD1DBDFCC, 0xD60BDB39,
	        0x043AC42B, 0xC7923768, 0x8EFA327D, 0xE1B22C50,
	        0xF89F5AB8, 0x44F6BC3B, 0xF3D61A00, 0x28DA469D,
	        0xC098232D, 0x16E7FD2B, 0x1092FD72, 0x9C951626,
	        0xFBE69462, 0xCFB79DEC, 0xC3A9655A, 0x13BBA9D2,
	        0xB7C2085F, 0xE4066B0D, 0x11D35166, 0xCC04A543,
	        0xE1ED6F0F, 0x1799DC3C, 0x4D99A1BF, 0x3379EA65,
	        0xA0209633, 0xE1D493E0, 0xD4A1352C, 0x8A72F31F,
	        0x1C0B7542, 0x4CA4358D, 0xC6BF7221, 0xC47733D9,
	        0xE036A08E, 0x9C9A302F, 0xE70C7048, 0x10E4F21E,
	        0xE64DDB55, 0x1FDBD992, 0xCF637AD0, 0xCE3F7F70,
	        0x1719B267, 0xFE2D1E06, 0x8590D3C6, 0xF7FC239A,
	        0xF624F458, 0xA7337724, 0x94A93632, 0x57CDCE03,
	        0xADF18263, 0x5B76ECB6, 0x6F173798, 0x89D374CD,
	        0xDF976393, 0x82BA4AD1, 0x4D51911C, 0x72C75715,
	        0xE7C7C8BE, 0x337B150B, 0x46E2D107, 0xC4F37C9B,
	        0xCAAB54FE, 0x63A91001, 0xBC26C0E3, 0x36BED3F7,
	        0x72136A06, 0xB3050323, 0xB7CCD07D, 0xCE779D2C,
	        0x54123FC1, 0x1741E4D4, 0x39ACBE61, 0x2648AEF1,
	        0xBB39219D, 0xF847CF77, 0x78B0A2C6, 0x21766161,
	        0x86CCFF4F, 0x8BE98ED9, 0x7BABFAB1, 0x4DFAAB7F,
	        0x1A49C35D, 0x03FC8B8D, 0x02C46BE5, 0xD7ECE2FA,
	        0x91D5F96A, 0xA75DDFA1, 0x400A262E, 0xC309E7A0,
	        0xB84F6233, 0xCF78E35C, 0x5890E0E4, 0x3BC473E7
        };
        #endregion
    }
}
