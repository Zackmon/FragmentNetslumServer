using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace FragmentServerWV.Services
{
    public static class Extensions
    {

        /// <summary>
        /// Helper function for safely logging out binary data to a supplied <see cref="ILogger"/>
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/></param>
        /// <param name="data">The data to parse</param>
        /// <param name="code">HEX code address</param>
        /// <param name="index">The client's index number</param>
        /// <param name="action"></param>
        /// <param name="check1"></param>
        /// <param name="check2"></param>
        public static void LogData(
            this ILogger logger,
            byte[] data,
            ushort code,
            int index,
            string action,
            ushort check1,
            ushort check2)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Client #{index}: {action} (Code 0x{code:X4}); Checksum 1 (from packet): 0x{check1:X4}; Checksum 2 (from server): 0x{check2:X4}");
            try
            {
                sb.AppendLine(HexDump(data));
            }
            catch (Exception e)
            {
                sb.AppendLine($"Failed to invoke {nameof(HexDump)}: {e}");
            }
            try
            {
                logger?.Information(sb.ToString());
            }
            catch
            {
                Console.WriteLine("LogData failed");
            }
        }


        /// <summary>
        /// I kept this from the original source; there might be a better way to do this but for now it's fine
        /// </summary>
        /// <param name="bytes">The byte array to convert</param>
        /// <param name="bytesPerLine">how many bytes should be displayed per line</param>
        /// <returns>A string representation</returns>
        public static string HexDump(this byte[] bytes, int bytesPerLine = 16)
        {
            if (bytes == null) return "<null>";
            int bytesLength = bytes.Length;

            char[] HexChars = "0123456789ABCDEF".ToCharArray();

            int firstHexColumn =
                  8                   // 8 characters for the address
                + 3;                  // 3 spaces

            int firstCharColumn = firstHexColumn
                + bytesPerLine * 3       // - 2 digit for the hexadecimal value and 1 space
                + (bytesPerLine - 1) / 8 // - 1 extra space every 8 characters from the 9th
                + 2;                  // 2 spaces 

            int lineLength = firstCharColumn
                + bytesPerLine           // - characters to show the ascii value
                + Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)

            char[] line = (new string(' ', lineLength - Environment.NewLine.Length) + Environment.NewLine).ToCharArray();
            int expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
            var result = new StringBuilder(expectedLines * lineLength);

            for (int i = 0; i < bytesLength; i += bytesPerLine)
            {
                line[0] = HexChars[(i >> 28) & 0xF];
                line[1] = HexChars[(i >> 24) & 0xF];
                line[2] = HexChars[(i >> 20) & 0xF];
                line[3] = HexChars[(i >> 16) & 0xF];
                line[4] = HexChars[(i >> 12) & 0xF];
                line[5] = HexChars[(i >> 8) & 0xF];
                line[6] = HexChars[(i >> 4) & 0xF];
                line[7] = HexChars[(i >> 0) & 0xF];

                int hexColumn = firstHexColumn;
                int charColumn = firstCharColumn;

                for (int j = 0; j < bytesPerLine; j++)
                {
                    if (j > 0 && (j & 7) == 0) hexColumn++;
                    if (i + j >= bytesLength)
                    {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    }
                    else
                    {
                        byte b = bytes[i + j];
                        line[hexColumn] = HexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = HexChars[b & 0xF];
                        line[charColumn] = (b < 32 ? '·' : (char)b);
                    }
                    hexColumn += 3;
                    charColumn++;
                }
                result.Append(line);
            }
            return result.ToString();
        }

        /// <summary>
        /// Swaps the first and second bytes around
        /// </summary>
        /// <param name="data">The 16 bit number to swap</param>
        /// <returns>A byte swapped 16 bit number</returns>
        /// <remarks>
        /// Imagine the following example, if you would:
        /// <code>
        /// var c = (ushort)20;
        /// </code>
        /// The actual byte composition of that unsigned short is as follows:
        /// <code>
        /// 0  1
        /// 14 00
        /// </code>
        /// Byte 0 is of value 14
        /// Byte 1 is of value 00
        /// When you put that number into Swap(), it does the following:
        /// <code>
        /// 0  1
        /// 00 14
        /// </code>
        /// It literally swaps the locations
        /// </remarks>
        public static ushort Swap(this ushort data) => 
            (ushort)((data >> 8) + ((data & 0xFF) << 8));

        /// <summary>
        /// Swaps bytes zero and one with bytes two and three
        /// </summary>
        /// <param name="data">The 32 bit number to swap</param>
        /// <returns>A byte swapped 32 bit number</returns>
        /// Imagine the following example, if you would:
        /// <code>
        /// var c = (uint)20;
        /// </code>
        /// The actual byte composition of that unsigned short is as follows:
        /// <code>
        /// 0  1  2  3
        /// 14 00 00 00
        /// </code>
        /// Byte 0 is of value 14
        /// Byte 1 is of value 00
        /// Byte 2 is of value 00
        /// Byte 3 is of value 00
        /// When you put that number into Swap(), it does the following:
        /// <code>
        /// 0  1  2  3
        /// 00 00 14 00
        /// </code>
        /// It literally swaps the locations of the byte pairs
        /// </remarks>
        public static uint Swap(this uint data)
        {
            uint result = 0;
            result |= (data & 0xFF) << 24;
            result |= ((data >> 8) & 0xFF) << 16;
            result |= ((data >> 16) & 0xFF) << 8;
            result |= (data >> 24) & 0xFF;
            return result;
        }

        public static IEnumerable<string> ChunksUpto(this string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }

    }
}
