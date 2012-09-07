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
// This file was last updated on: 7/18/2012   //
////////////////////////////////////////////////

using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;
using XihSolutions.DotMSN;
using XihSolutions.DotMSN.Core;

namespace Kon
{
    class msn
    {

#region variables
        private XihSolutions.DotMSN.Messenger messenger = new Messenger();
        private static StreamWriter msnLogger;

#endregion

        public msn()
        {
        }

        public msn(string msnUserName, string msnPassword)
        {


            messenger.NameserverProcessor.ConnectionEstablished += new EventHandler(NameserverProcessor_ConnectionEstablished);
            messenger.Nameserver.SignedIn += new EventHandler(Nameserver_SignedIn);
            messenger.Nameserver.SignedOff += new SignedOffEventHandler(Nameserver_SignedOff);
            messenger.Nameserver.ExceptionOccurred += new XihSolutions.DotMSN.Core.HandlerExceptionEventHandler(Nameserver_ExceptionOccurred);
            messenger.Nameserver.AuthenticationError += new XihSolutions.DotMSN.Core.HandlerExceptionEventHandler(Nameserver_AuthenticationError);
           
            msnLogger = new StreamWriter("msnlog.txt", true);
            msnLogger.WriteLine("---------[STARTING: " + DateTime.Now + "]");

            if (messenger.Connected)
            {
                messenger.Disconnect();
            }

            messenger.Credentials.ClientID = "msmsgs@msnmsgr.com";
            messenger.Credentials.ClientCode = "Q1P7W2E4J9R8U3S5";

            messenger.Credentials.Account = msnUserName;
            messenger.Credentials.Password = msnPassword;

            // try to connect to the messenger network.			
            messenger.Connect();
        }

        private void NameserverProcessor_ConnectionEstablished(object sender, EventArgs e)
        {
            Console.WriteLine("Connected to MSN server");
        }

        private void Nameserver_SignedIn(object sender, EventArgs e)
        {
            Console.WriteLine("Signed into the MSN messenger network as " + messenger.Owner.Name);

        }

        private void Nameserver_SignedOff(object sender, SignedOffEventArgs e)
        {
            Console.WriteLine("Signed off from the MSN messenger network");
        }

        private void Nameserver_ExceptionOccurred(object sender, ExceptionEventArgs e)
        {
            // ignore the unauthorized exception, since we're handling that error in another method.
            if (e.Exception is UnauthorizedException)
                return;

            Console.WriteLine("MSN Nameserver exception");
        }

        private void NameserverProcessor_ConnectingException(object sender, ExceptionEventArgs e)
        {
            Console.WriteLine("MSN Connecting failed");
        }

        private void Nameserver_AuthenticationError(object sender, ExceptionEventArgs e)
        {
            Console.WriteLine("MSN Authentication failed");
        }

        static void Switchboard_TextMessageReceived(object sender, TextMessageEventArgs e)
        {
            Console.WriteLine(e.ToString());
            msnLogger.WriteLine("MSN: " + e.ToString());
            XihSolutions.DotMSN.SBMessageHandler handler = (XihSolutions.DotMSN.SBMessageHandler)sender;

            string returnMessage = getReply(e.Message.Text, true);
            handler.SendTextMessage(new TextMessage("->" + returnMessage));
            msnLogger.WriteLine(">> " + "MSN: "+ returnMessage);
        }

    
        private static string getReply(string conversation, bool topics)
        {
            if (IRC.LB)
            {
                string reply = IRC.myLB.pullFromBrain(conversation, topics);

                // Is the last character a comma, semi-colon or colon? If so, let's remove it.
                string lastReplyCharacter = reply.Substring(reply.Length - 1, 1);
                if (((lastReplyCharacter == ",") || (lastReplyCharacter == ";") || (lastReplyCharacter == ":")))
                    reply = reply.Remove(reply.Length - 1, 1);

                // Let's clean up the reply a little bit.
                if ((reply.StartsWith(" ACTION")) || (reply.StartsWith("ACTION")))
                {
                    reply = reply.Replace("ACTION", "");
                    reply = reply.Replace("", "");
                    reply = reply.Trim();
                    reply = "ACTION " + reply + "";
                }
                else
                {
                    reply = reply.Replace(" ACTION", " *");
                    reply = reply.Replace("ACTION", "*");
                }

                reply = reply.Trim();


                return reply;
            }

            else
                return "Error in the LB brain.";
        }



        

    }
}
