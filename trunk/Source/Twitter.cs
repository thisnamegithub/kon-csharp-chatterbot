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
using System.Xml;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.IO;

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
        private     string      oauth_consumer_key           = "";
        private     string      oauth_consumer_secret        = "";
        private     string      twitterMessage               = "";
        private     Thread      twitterThread;

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
                        
                        twitterMessage = myLB.pullFromBrain("thisisjustatestthereshouldn'tbeanythinginthebrainforthis!", false);
                        
                        Thread.Sleep(1000);

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
                        if (twitterMessage != "")
                        {
                            twitterSendTweet();

                            if (showTwitterMessages)
                                Console.WriteLine(">> TWITTER : " + twitterMessage);
                        }
                        else
                            System.Console.WriteLine("Error: no message to send to Twitter");
                    }

                    else
                        System.Console.WriteLine("Error: LB Brain is currently in use..cannot send a tweet");
                }

                else
                    System.Console.WriteLine("Error: LB Brain should be turned on to use the Twitter client");
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

                    twitterSendTweet();

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


#region twitterSendTweet()

        private void twitterSendTweet()
        {
            Console.WriteLine("attempting to send the tweet: " + twitterMessage);

            var oauth_version = "1.0";
            var oauth_signature_method = "HMAC-SHA1";
            var oauth_nonce = Convert.ToBase64String(
                                              new ASCIIEncoding().GetBytes(
                                                   DateTime.Now.Ticks.ToString()));
            var timeSpan = DateTime.UtcNow
                                              - new DateTime(1970, 1, 1, 0, 0, 0, 0,
                                                   DateTimeKind.Utc);
            var oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();
            var resource_url = "https://api.twitter.com/1.1/statuses/update.json";
            var status = twitterMessage;
            
            var baseFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" +
                "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}&status={6}";

            var baseString = string.Format(baseFormat,
                                        oauth_consumer_key,
                                        oauth_nonce,
                                        oauth_signature_method,
                                        oauth_timestamp,
                                        twitterAccessToken,
                                        oauth_version,
                                        Uri.EscapeDataString(status)
                                        );

            baseString = string.Concat("POST&", Uri.EscapeDataString(resource_url),
                         "&", Uri.EscapeDataString(baseString));


            var compositeKey = string.Concat(Uri.EscapeDataString(oauth_consumer_secret),
                        "&", Uri.EscapeDataString(twitterAccessTokenSecret));

            string oauth_signature;
            using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey)))
            {
                oauth_signature = Convert.ToBase64String(
                    hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
            }

            var headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                   "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                   "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                   "oauth_version=\"{6}\"";

            var authHeader = string.Format(headerFormat,
                                    Uri.EscapeDataString(oauth_nonce),
                                    Uri.EscapeDataString(oauth_signature_method),
                                    Uri.EscapeDataString(oauth_timestamp),
                                    Uri.EscapeDataString(oauth_consumer_key),
                                    Uri.EscapeDataString(twitterAccessToken),
                                    Uri.EscapeDataString(oauth_signature),
                                    Uri.EscapeDataString(oauth_version)
                            );


            var postBody = "status=" + Uri.EscapeDataString(status);

            ServicePointManager.Expect100Continue = false;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resource_url);
            request.Headers.Add("Authorization", authHeader);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            using (Stream stream = request.GetRequestStream())
            {
                byte[] content = ASCIIEncoding.ASCII.GetBytes(postBody);
                stream.Write(content, 0, content.Length);
            }

            WebResponse response = request.GetResponse();
        }

#endregion


    }
}
