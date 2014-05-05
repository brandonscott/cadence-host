using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CadenceHost.Models
{
    public class DebugInfo
    {
        private int _id;
        public static int MasterID { get; set; }

        public int ID
        {
            get { return _id; }
            set { _id = MasterID;
                MasterID++;
            }
        }

        public string Type { get; set; }
        public string Message { get; set; }

    }
}
