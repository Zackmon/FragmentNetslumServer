using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FragmentNetslumServer.Services
{
    public class BaseManagementService 
    {
        
        protected Encoding _encoding;
        public BaseManagementService()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _encoding = Encoding.GetEncoding("Shift-JIS");
        }
        
        
        public List<byte[]> GetClassList()
        {
            List<byte[]> classList = new List<byte[]>();
            MemoryStream m = new MemoryStream();

            m.Write(BitConverter.GetBytes(swap16(1)));
            m.Write(_encoding.GetBytes("All"));
            m.Write(new byte[] {0x00});
            classList.Add(m.ToArray());

            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(2)));
            m.Write(_encoding.GetBytes("Twin Blade"));
            m.Write(new byte[] {0x00});
            classList.Add(m.ToArray());

            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(3)));
            m.Write(_encoding.GetBytes("Blademaster"));
            m.Write(new byte[] {0x00});
            classList.Add(m.ToArray());

            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(4)));
            m.Write(_encoding.GetBytes("Heavy Blade"));
            m.Write(new byte[] {0x00});
            classList.Add(m.ToArray());

            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(5)));
            m.Write(_encoding.GetBytes("Heavy Axe"));
            m.Write(new byte[] {0x00});
            classList.Add(m.ToArray());

            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(6)));
            m.Write(_encoding.GetBytes("Long Arm"));
            m.Write(new byte[] {0x00});
            classList.Add(m.ToArray());


            m = new MemoryStream();
            m.Write(BitConverter.GetBytes(swap16(7)));
            m.Write(_encoding.GetBytes("Wavemaster"));
            m.Write(new byte[] {0x00});
            classList.Add(m.ToArray());

            return classList;
        }


        [Obsolete("replace with direct calls to Swap() in the Extensions class")]
        public ushort swap16(ushort data) => data.Swap();

        [Obsolete("replace with direct calls to Swap() in the Extensions class")]
        public uint swap32(uint data) => data.Swap();

    }

}