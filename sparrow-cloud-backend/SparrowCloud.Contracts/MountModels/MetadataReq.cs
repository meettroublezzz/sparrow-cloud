using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Contracts.MountModels
{
    public class MetadataReq
    {
        public enum MetadataType
        {
            None = 0,

            Entry = 1,
            Tree = 2,
        }
    }
}
