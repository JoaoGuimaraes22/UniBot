using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniBotJG.StateManagement
{
    public class ConversationData
    {
        //Storing Conversation Data

        //Check if user gave permission for data storage during conversation
        public bool PromptedUserForName { get; set; }
    }
}
