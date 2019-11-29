using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniBotJG.StateManagement
{
    
    public class UserProfile
    {
        //Storing User Data
        public string Name { get; set; }

        public int Age { get; set; }

        public bool GavePermission { get; set; } = false;
    }
}
