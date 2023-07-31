using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeLight.Models.Files
{
    //used if the filepath is not valid
    public partial class BlackoutScreenFile : ScreenFile
    {
        public BlackoutReason Reason { get; set; }
        public BlackoutScreenFile(BlackoutReason reason)
        {
            Reason = reason;
        }
    }
}
