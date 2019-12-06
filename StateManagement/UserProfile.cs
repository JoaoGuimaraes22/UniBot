using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniBotJG.StateManagement
{

    public class UserProfile
    {
        //Storing User Data
        public string Phone { get; set; }
        public string Email { get; set; }
        public string NIF { get; set; }
        public bool HasAccount { get; set; }
        public bool LivesInPortugal{ get; set; }
        public bool HelpMe { get; set; } = false;
        public bool HasBeenPrompted { get; set; } = false;
        public bool GavePermission { get; set; } = false;
        public bool ChosePhone { get; set; } = false;
        public bool IsClient { get; set; } = false;
        public bool ChoseEmail { get; set; } = false; 
    }
}
