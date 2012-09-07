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
// This file was last updated on: 1/7/2009    //
////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Kon
{
    partial class IRC
    {
        Thread connectionCheckThread;
        int    reconnectAttempts       = 0;

        public void connectionThread()
        {
            // Start a new thread to monitor connection checks
            connectionCheckThread = new Thread(new ThreadStart(connectionCheck));
            connectionCheckThread.Start();
        }

        public void connectionCheck()
        {
            while (canQuit == false)
            {
                if (connected == false)
                {
                    if ((reconnectAttempts <= retryAttempts) && (canQuit == false))
                    {
                        reconnectAttempts++;
                        reconnect();
                    }
                    else
                    {
                        if (reconnectAttempts > retryAttempts)
                        {
                            Console.WriteLine("There has been too many reconnect attempts. Please check the connection\nand restart the bot.");
                            setQuitState(true);
                        }
                    }
                }

                // We're still connected.. let's check to make sure the bot didn't ping out and is registering a false positive
                checkPongAttempts();
                Thread.Sleep(1000);
            }
        }


    }
}
