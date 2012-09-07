////////////////////////////////////////////////
////////////////PROGRAM INFO////////////////////
// "Kon" is a "learning bot" written in C#.   //
// The bot will connect to an IRC server and  //
// "watch" the conversations.  It will then   //
// "learn" from them.  You'll then be able to //
// talk to it and hopefully it'll hold a conv-//
// ersation.                                  //
////////////////////////////////////////////////
// "Kon" is an ongoing project started by     // 
// James Iyouboushi.                          //
// Emails: jmp1139@my.gulfcoast.edu           //
//         Iyouboushi@gmail.com               //
////////////////////////////////////////////////
// This file was last updated on: 2/14/2008   //
////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using AIMLBot;


namespace Kon
{
    class AIMLbrain
    {

#region variables
        public static cBot AIMLbot; 
#endregion

        public AIMLbrain()
        {
            startAI();
        }

        public void startAI()
        {
            AIMLbot = new cBot(false);
        }

        public static String getReply(String reply, String conversation, String nick)
        {
            cResponse rawReply;
            rawReply = AIMLbot.chat(conversation, nick);  // needs to change a bit.

            reply = rawReply.getOutput();

            // Let's replace stuff that Kon wasn't able to find in AIML with something from his brain.

            reply = reply.Replace("NANIMONAI", IRC.myAI.pullFromBrain());
            reply = reply.Replace("REPLACETHISWITHSOMETHING", IRC.myAI.pullFromBrain());
            reply = reply.Replace("HEHADNOIDEAWHATHESAID", IRC.myAI.pullFromBrain());
            reply = reply.Replace("KONHASNOFREAKINCLUERIGHTNOW", IRC.myAI.pullFromBrain());
            reply = reply.Replace("REPLACETHISMORESOITSTOPSREPEATING", IRC.myAI.pullFromBrain());

            // Now we'll replace a few other things to help make it better.
            reply = reply.Replace(@"Perhaps some of my AIML files are missing. You can download them from http://www.alicebot.org/ and place them in the following directory: C:\Documents and Settings\James\My Documents\Visual Studio 2005\Projects\Kon\Kon\bin\Debug\aiml", IRC.myAI.pullFromBrain());
            reply = reply.Replace("I think you mean \"it's\" or \"it is\" not \"its\".", IRC.myAI.pullFromBrain());
            reply = reply.Replace("I answer a lot of silly questions.", IRC.myAI.pullFromBrain());
            reply = reply.Replace("Try asking me in simpler terms.", IRC.myAI.pullFromBrain());

            // Replace "un-named user" with the nick of the person speaking to the bot.
            reply = reply.Replace("un-named user", nick);

            return reply;
        }

        public static void unloadAIMLbrain()
        {
            AIMLbot.unloadAIML();
            AIMLbot = null;
        }

    }
}
