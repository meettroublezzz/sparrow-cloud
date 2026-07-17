using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Contracts.MountModels
{
    public class MountModel : MountAddReq
    {
        public required int MountId { get; set; }
        public DateTime? CreateAt { get; set; }
    }
}
