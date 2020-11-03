using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FragmentServerWV.Services
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
        
        
        //Copy from the GameClient Code
        public ushort swap16(ushort data)
        {
            ushort result = 0;
            result = (ushort) ((data >> 8) + ((data & 0xFF) << 8));
            return result;
        }


        public uint swap32(uint data)
        {
            uint result = 0;
            result |= (uint) ((data & 0xFF) << 24);
            result |= (uint) (((data >> 8) & 0xFF) << 16);
            result |= (uint) (((data >> 16) & 0xFF) << 8);
            result |= (uint) ((data >> 24) & 0xFF);
            return result;
        }
    }
}