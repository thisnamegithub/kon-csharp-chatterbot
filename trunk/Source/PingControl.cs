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
// This file was last updated on: 10/22/2009  //
////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Kon
{
    partial class IRC
    {
#region PingControl
        ////////////////////////////////////////////////
        ////////////////PING CONROL/////////////////////
        ////////////////////////////////////////////////
        // PING control is a set of two functions that//
        // will ping the server every 15 seconds on a //
        // seperate thread until we are no longer     //
        // connected to the server.                   //
        ////////////////////////////////////////////////

        private   int      pongAttempt    = 0;
        private   Thread   pingSender;
        private   bool     canPing        = false;

        public void PingControl()
        {
            // Start a new thread for the Ping Control
            pingSender = new Thread(new ThreadStart(PingRun));

            // Begin the control.
            pingSender.Start();
        }


        // Send PING to irc server every 15 seconds
        private void PingRun()
        {
            
            // Is the client still running?  If so, we need to ping the server
            while (canQuit == false)
            {
                sendPing();
                Thread.Sleep(15000);
            }
        }

        private void checkPongAttempts()
        {
            if (pongAttempt > 5)
            {
                pongAttempt = 0;
                setConnectionState(false);
            }
        }

        private void sendPing()
        {
            try
            {
                if (canPing)
                {
                    if ((!canQuit) && (connected))
                    {
                        writer.WriteLine(PING + ircServer);
                        writer.Flush();
                    }
                }
                Thread.Sleep(100);
            }
            catch (Exception e)
            {
                if (showPongReply)
                    Console.WriteLine("Error: " + e.ToString());
            }
            pongAttempt++;
        }

        public void setCanPing(bool canWePing)
        {
            canPing = canWePing;
        }

#endregion
    }
}
