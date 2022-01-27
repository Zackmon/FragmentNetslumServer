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
        
        
        


        [Obsolete("replace with direct calls to Swap() in the Extensions class")]
        public ushort swap16(ushort data) => data.Swap();

        [Obsolete("replace with direct calls to Swap() in the Extensions class")]
        public uint swap32(uint data) => data.Swap();

    }

}