using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;



namespace FragmentServerWV
{
    public static class Log
    {

      
        public static readonly object _sync = new object();
        public static int LogTreshold = 0;
        public static int PacketCount = 0;
        public static int LogSize;
        public static LogEventDelegate LogEventDelegate;

        public static void InitLogs(LogEventDelegate logEventDelegate)
        {
            LogSize = Convert.ToInt32(Config.configs["logsize"]);

            LogEventDelegate = logEventDelegate;

            if (!Directory.Exists("log"))
                Directory.CreateDirectory("log");
            string[] files = Directory.GetFiles("log/");
            foreach (string file in files)
                File.Delete(file);
        }


        public static void Writeline(string s, int level = 4)
        {
            lock (_sync)
            {
                try
                {

                    if (level >= LogTreshold)
                    {
                        string text = DateTime.Now.ToLongTimeString() + ":" + s + "\r\n";
                        StreamWriter sw = File.AppendText("log\\log.txt");
                        sw.Write(text);
                        sw.Close();

                        //Trigger logging event
                        LogEventDelegate.LogRequestResponse(text, LogSize);

                    }


                }
                catch (Exception)
                { }
            }
        }

        public static void LogData(byte[] data, ushort code, int index, string action, ushort check1, ushort check2)
        {
            string text;
            text = "Client #" + index + " : " + action + " (code 0x" + code.ToString("X4") + ", checksums 0x" + check1.ToString("X4") + "-0x" + check2.ToString("X4") + ")";
            Writeline(text, 2);
            Writeline("Hexdump :\r\n"+ HexDump(data), 0);
            string path;
            if (code != 0x30)
                path = "log/" + (PacketCount++).ToString("D8") + "_" + DateTime.Now.ToLongTimeString().Replace(":", "-") + "_cl" + index.ToString("D4") + "_" + action.Replace(" ", "-") + ".bin";
            else
            {
                path = "log/" + (PacketCount++).ToString("D8") + "_" + DateTime.Now.ToLongTimeString().Replace(":", "-") + "_cl" + index.ToString("D4") + "_" + action.Replace(" ", "-") + "-code0x" + data[8].ToString("X2") + data[9].ToString("X2") + ".bin";
            }
            File.WriteAllBytes(path, data);
        }

        public static string HexDump(byte[] bytes, int bytesPerLine = 16)
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

            char[] line = (new String(' ', lineLength - Environment.NewLine.Length) + Environment.NewLine).ToCharArray();
            int expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
            StringBuilder result = new StringBuilder(expectedLines * lineLength);

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
    }
}
