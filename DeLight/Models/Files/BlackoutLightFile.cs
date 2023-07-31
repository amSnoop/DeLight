using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeLight.Models.Files
{
    public class BlackoutLightFile : LightFile
    {
        public BlackoutReason Reason { get; set; }
        public BlackoutLightFile(BlackoutReason reason)
        {
            Reason = reason;
        }
    }
}
