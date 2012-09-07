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
// This file was last updated on: 1/02/2009   //
////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Kon
{
    partial class IRC
    {

#region logger
        ////////////////////////////////////////////////
        ///////////////////LOGGER///////////////////////
        ////////////////////////////////////////////////
        // Dump the lines into a log file for later   //
        // review.                                    //
        ////////////////////////////////////////////////
        private static void LogFileOn()
        {
            // If logging is enabled, let's turn it on and indicate what time we started
            if (logging == true)
            {
                logger = new StreamWriter(LOG_FILE, true);
                LogFile("---------[STARTING: " + DateTime.Now + "]");
            }
        }

        private static void LogFile(string line)
        {
            if (IRC.logging)
            {
                try
                {
                    logger.WriteLine(line);
                    logger.Flush();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    IRC.LogFileOn();
                }
            }
        }
#endregion

    }
}
