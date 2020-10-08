using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace HRC.Framework.BL
{
    public enum DocumentFileStorage : byte
    {
        [DescriptionAttribute("None")]
        None = 0,
        [DescriptionAttribute("FileSystem")]
        FileSystem = 1,
        [DescriptionAttribute("DatabaseBinary")]
        DatabaseBinary = 2,
        [DescriptionAttribute("DatabaseFileStream")]
        DatabaseFileStream = 3,
    }
}
