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
// This file was last updated on: 11/15/2010  //
////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Twitterizer;
using System.Xml;

namespace Kon
{
    partial class IRC
    {


        private     bool        useTwitter                   = false;
        private     bool        twitterClientOn              = false;
        private     bool        showTwitterMessages          = false;
        private     bool        sentInitialMessage           = false;
        private     string      twitterAccessToken           = "";
        private     string      twitterAccessTokenSecret     = "";
        private     string      twitterMessage               = "";
        private     Thread      twitterThread;
        private     OAuthTokens token                        = new OAuthTokens();


        public void TwitterControl()
        {
            // Start a new thread for the Twitter Client
            twitterThread = new Thread(new ThreadStart(twitterRun));

            // Begin the control.
            twitterThread.Start();

        }
        
        private void twitterRun()
        {
            while (useTwitter)
            {
                sendTwitterMessage();
                Thread.Sleep(3550000);
            }
        }

#region sendTwitterMessage()

        private void sendTwitterMessage()
        {
            if (twitterClientOn)
            {
                if (myLB != null)
                {
                    // Only try to send a message to Twitter if the LB Brain is not currently in use.
                    // This prevents a "file currently in use" error.
                    if (myLB.brainInUse == false)
                    {
                        try
                        {
                            twitterMessage = myLB.pullFromBrain(".", false);
                            if (channel != "")
                            {

                                string[] words = twitterMessage.Split(' ');
                                int wordCount = words.Length - 1;

                                for (int i = 0; i < wordCount; i++)
                                {
                                    if ((words[i].ToString() == "ACTION") || (words[i].ToString() == "ACTION"))
                                        words[i] = "*";
                                    if (words[i].ToString() == "UNNAMED_USER:")
                                        words[i] = randomChannelUser() + ":";
                                    else
                                        words[i] = words[i].Replace("UNNAMED_USER", randomChannelUser());
                                }

                                // now to rebuild the reply
                                twitterMessage = "";
                                for (int i = 0; i < words.Length; i++)
                                {
                                    twitterMessage += words[i] + " ";
                                }

                                twitterMessage = twitterMessage.Replace("", "");
                                twitterMessage = twitterMessage.Replace("UNNAMED_USER", "Kon");  // A final last effort if all else failed.
                                twitterMessage = twitterMessage.Trim();
                            }

                            Thread.Sleep(100);

                            // Update Twitter
                            TwitterStatus.Update(token, twitterMessage);
                                                      

                            if (showTwitterMessages)
                                Console.WriteLine(">> TWITTER : " + twitterMessage);
                        }
                        catch (Exception e)
                        {
                            if (e.Message != "Error Parsing Twitter Response.")
                            {
                                Console.WriteLine("An error with the Twitter Client has been found.  Are you really online?");
                                Console.WriteLine("Will try again during the next scheduled Twitter update.");
                                LogFile(e.ToString());
                            }
                        }
                    }
                }

                else
                    System.Console.WriteLine("LB Brain should be turned on to use the Twitter client");
            }
        }
#endregion


#region sendInitialTwitterMessage()
        private void sendInitialTwitterMessage()
        {
            if ((myLB != null) && (sentInitialMessage == false))
            {
                Thread.Sleep(200);
                try
                {
                    sentInitialMessage = true;
                    twitterMessage = myLB.pullFromBrain(".", false);
                    TwitterStatus.Update(token, twitterMessage);

                    if (showTwitterMessages)
                        Console.WriteLine(">> TWITTER : " + twitterMessage);
                }
                catch (Exception e)
                {
                    if (e.Message != "Error Parsing Twitter Response.")
                    {
                        Console.WriteLine("An error with the Twitter Client has been found.  Are you really online?");
                        Console.WriteLine("Will try again during the next scheduled Twitter update.");
                        LogFile(e.ToString());
                        return;
                    }
               }
            }
            else
                System.Console.WriteLine("LB Brain should be turned on to use the Twitter client");
        }
#endregion

    }
}
