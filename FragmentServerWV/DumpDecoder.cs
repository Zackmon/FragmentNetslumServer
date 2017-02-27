using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FragmentServerWV
{
    public partial class DumpDecoder : Form
    {

        public Crypto server;
        public Crypto client = new Crypto();

        public DumpDecoder()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog d = new OpenFileDialog();
                d.Filter = "*.txt|*.txt";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string[] lines = File.ReadAllLines(d.FileName);
                    rtb1.Text = "";
                    server = new Crypto();
                    client = new Crypto();
                    StringBuilder sb = new StringBuilder();
                    foreach (string line in lines)
                    {
                        char c = line[0];
                        string sdata = line.Substring(1);
                        byte[] data = StringToByteArray(sdata);
                        switch (c)
                        {
                            case 's':
                                HandlePacket(data, true, sb);
                                break;
                            case 'r':
                                HandlePacket(data, false, sb);
                                break;
                            default:
                                throw new Exception();
                        }
                    }
                    rtb1.Text = sb.ToString();
                }
            }
            catch (Exception)
            { }
        }

        public void HandlePacket(byte[] data, bool isClient, StringBuilder sb)
        {
            ushort code = GameClient.swap16(BitConverter.ToUInt16(data, 2));
            string dir = isClient ? "Send" : "Recv";
            byte[] decrypted;
            MemoryStream m;
            switch (code)
            {
                case 0x34:
                    m = new MemoryStream();
                    m.Write(data, 4, data.Length - 4);
                    decrypted = client.Decrypt(m.ToArray());
                    sb.Append(PrintPacket(dir, decrypted));
                    m = new MemoryStream();
                    m.Write(decrypted, 4, 16);
                    client = new Crypto(m.ToArray());
                    break;
                case 0x35:
                    m = new MemoryStream();
                    m.Write(data, 4, data.Length - 4);
                    decrypted = server.Decrypt(m.ToArray());
                    sb.Append(PrintPacket(dir, decrypted));
                    m = new MemoryStream();
                    m.Write(decrypted, 22, 16);
                    server = new Crypto(m.ToArray());
                    break;
                case 0x36:
                    m = new MemoryStream();
                    m.Write(data, 4, data.Length - 4);
                    decrypted = new Crypto().Decrypt(m.ToArray());
                    sb.Append(PrintPacket(dir, decrypted));
                    break;
                case 0x30:
                    m = new MemoryStream();
                    m.Write(data, 4, data.Length - 4);
                    if (isClient)
                        decrypted = client.Decrypt(m.ToArray());
                    else
                        decrypted = server.Decrypt(m.ToArray());
                    sb.Append(PrintPacket(dir, decrypted));
                    break;                    
            }
        }

        public string PrintPacket(string dir, byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(dir+":");
            sb.AppendLine(HexDump(data));
            return sb.ToString();
        }

        public byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
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
