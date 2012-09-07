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
using MSNPSharp;
using MSNPSharp.Services;
using MSNPSharp.Apps;
using MSNPSharp.Core;
using MSNPSharp.MSNWS.MSNABSharingService;
using MSNPSharp.IO;


namespace Kon
{
    class msn
    {

#region variables
        private Messenger messenger = new Messenger();
        private bool syncContactListCompleted = false;
        private PresenceStatus lastStatus = PresenceStatus.Online;
#endregion

        public Messenger Messenger
        {
            get
            {
                return messenger;
            }
        }

        public msn()
        {
        }

        public msn(string msnUserName, string msnPassword)
        {

            // Get notified when successfully signed in.
            messenger.Nameserver.SignedIn += new EventHandler<EventArgs>(Nameserver_SignedIn);

            // Get notified when the user signed off.
            messenger.Nameserver.SignedOff += new EventHandler<SignedOffEventArgs>(Nameserver_SignedOff);

            messenger.Credentials = new Credentials(msnUserName, msnPassword);
            messenger.Nameserver.BotMode = false;
            messenger.Nameserver.AutoSynchronize = true;

            messenger.ConnectionClosed += new EventHandler<EventArgs>(messenger_ConnectionClosed);
            messenger.Nameserver.SignedIn += (Nameserver_SignedIn);
            messenger.Nameserver.SignedOff += new EventHandler<SignedOffEventArgs>(Nameserver_SignedOff);
            messenger.Nameserver.ExceptionOccurred += new EventHandler<ExceptionEventArgs>(Nameserver_ExceptionOccurred);
            messenger.Nameserver.AuthenticationError += new EventHandler<ExceptionEventArgs>(Nameserver_AuthenticationError);
            messenger.Nameserver.ServerErrorReceived += new EventHandler<MSNErrorEventArgs>(Nameserver_ServerErrorReceived);
            messenger.ConnectionEstablished += new EventHandler<EventArgs>(NameserverProcessor_ConnectionEstablished);
            messenger.ConnectingException += new EventHandler<ExceptionEventArgs>(NameserverProcessor_ConnectingException);
            messenger.Nameserver.ContactOnline += new EventHandler<ContactStatusChangedEventArgs>(Nameserver_ContactOnline);
            messenger.Nameserver.ContactOffline += new EventHandler<ContactStatusChangedEventArgs>(Nameserver_ContactOffline);
            messenger.MessageManager.TextMessageReceived += new EventHandler<TextMessageArrivedEventArgs>(Nameserver_TextMessageReceived);
            messenger.WhatsUpService.GetWhatsUpCompleted += new EventHandler<GetWhatsUpCompletedEventArgs>(WhatsUpService_GetWhatsUpCompleted);

            // SynchronizationCompleted will fired after the updated operation for your contact list has completed.
            messenger.ContactService.SynchronizationCompleted += new EventHandler<EventArgs>(ContactService_SynchronizationCompleted);

            IRC.DisplayData("Starting MSN: " + DateTime.Now + "]");

            if (messenger.Connected)
            {
                messenger.Disconnect();
            }

            // Let's try to connect to MSN.
            connectToMSN();

        }

        private void connectToMSN()
        {

            // inform the user what is happening and try to connecto to the messenger network.
           IRC.DisplayData("Connecting to the MSN server");
            messenger.Connect();
        }

        private void NameserverProcessor_ConnectionEstablished(object sender, EventArgs e)
        {
            IRC.DisplayData("Connected to the MSN server");
        }

        private void Nameserver_SignedIn(object sender, EventArgs e)
        {
            IRC.DisplayData("Successfully signed into MSN!");
        }

        private void Nameserver_SignedOff(object sender, SignedOffEventArgs e)
        {
            IRC.DisplayData("Signed Out Of MSN");
        }

        private void messenger_ConnectionClosed(object sender, EventArgs e)
        {
            IRC.DisplayData("Disconnected from the MSN server");
        }

        private void NameserverProcessor_ConnectingException(object sender, ExceptionEventArgs e)
        {
            IRC.DisplayData("MSN Connecting Failed");
        }

        private void Nameserver_AuthenticationError(object sender, ExceptionEventArgs e)
        {
            IRC.DisplayData("MSN Authentication failed");
        }
               

        private void Nameserver_ExceptionOccurred(object sender, ExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception.ToString(), "Nameserver exception");
        }

        private void Nameserver_ServerErrorReceived(object sender, MSNErrorEventArgs e)
        {
            Console.WriteLine("Server Error exception");
        }

        private void Nameserver_ContactOnline(object sender, ContactStatusChangedEventArgs e)
        {
            Console.WriteLine("Contact " + e.Contact.Account + " updated to " + e.Contact.Status);
        }

        private void Nameserver_ContactOffline(object sender, ContactStatusChangedEventArgs e)
        {
            Console.WriteLine("Contact " + e.Contact.Account + " updated to " + e.Contact.Status);
        }

        List<ActivityDetailsType> activities = new List<ActivityDetailsType>();
        void WhatsUpService_GetWhatsUpCompleted(object sender, GetWhatsUpCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Console.WriteLine("ERROR: " + e.Error.ToString());
            }
            else
            {
                activities.Clear();

                foreach (ActivityDetailsType activityDetails in e.Response.Activities)
                {
                    // Show status news
                    if (activityDetails.ApplicationId == "6262816084389410")
                    {
                        activities.Add(activityDetails);
                    }

                    Contact c = messenger.ContactList.GetContactByCID(long.Parse(activityDetails.OwnerCID));

                    if (c != null)
                    {
                        c.Activities.Add(activityDetails);
                    }
                }

                if (activities.Count == 0)
                {
                    Console.WriteLine("No news");
                    return;
                }
            }
        }

        void ContactService_SynchronizationCompleted(object sender, EventArgs e)
        {
            syncContactListCompleted = true;
            Console.WriteLine("Getting your friends' news...");
            messenger.WhatsUpService.GetWhatsUp(200);
        }


       private void Nameserver_TextMessageReceived(object sender, TextMessageArrivedEventArgs e)
        {
            Console.WriteLine("Message received: " + e.ToString());
            MessageManager_MessageArrived(sender, e);
        }

       private void MessageManager_MessageArrived(object sender, MessageArrivedEventArgs e)
        {

            Console.WriteLine("sender: " + sender.ToString());


            if (e is TextMessageArrivedEventArgs)
            {
                Console.WriteLine(e.ToString());
                IRC.DisplayData("MSN: " + e.ToString());

                string returnMessage = getReply(e.ToString(), true);
                //    messenger.SendTextMessage(RemoteContact, returnMessage);

                IRC.DisplayData(">> " + "MSN: " + returnMessage);
            }
        }

        /*
        static void Switchboard_TextMessageReceived(object sender, TextMessageEventArgs e)
        {
            Console.WriteLine(e.ToString());
            msnLogger.WriteLine("MSN: " + e.ToString());
            XihSolutions.DotMSN.SBMessageHandler handler = (XihSolutions.DotMSN.SBMessageHandler)sender;

            string returnMessage = getReply(e.Message.Text, true);
            handler.SendTextMessage(new TextMessage("->" + returnMessage));
            msnLogger.WriteLine(">> " + "MSN: "+ returnMessage);
        }*/

    
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

    /*    public Contact RemoteContact
        {

            get
            {
               return remoteContact;
            }
        }
     */

    }
}
