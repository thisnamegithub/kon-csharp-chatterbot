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
// This file was last updated on: 11/28/2011  //
////////////////////////////////////////////////
////////////////TO DO///////////////////////////
// Add more console commands                  //
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
#region ConsoleCommands
        ////////////////////////////////////////////////
        /////////////CONSOLE COMMAND PARSER/////////////
        // The command parser takes the incoming text //
        // from the console and splits it up into     // 
        // tokens as well as determines what command  //
        // has been used and what function should be  //
        // called.                                    //
        ////////////////////////////////////////////////
        public void getCommand()
        {
            if (connected)
            {
                // Get the command string from the console
                string rawCommandString = Console.ReadLine();
                // Break the command into their tokenized parts so we can determine what to do
                string[] commandString;
                commandString = rawCommandString.Split(new char[] { ' ' });

                // Get the first element of the command string and convert it to upper case
                string command = commandString[0].ToString();
                command = command.ToUpper();


                ///////////////////////////////////////////////////
                // The QUIT command.  Used to disconnect from the//
                // server.  It'll also cause our console to stop.//
                ///////////////////////////////////////////////////
                if (command == "/QUIT")
                {
                    // We need to check to see if the person has typed a quit message
                    int commandLength = commandString.Length;

                    if (commandLength >= 2)
                    {
                        // Determine what the message is.
                        commandLength = commandString.Length - 1;
                        string message = "";

                        for (int i = 1; i <= commandLength; i++)
                        {
                            message = message + commandString[i] + " ";
                        }

                        // Let's quit with our message
                        quit(message);
                    }

                    else
                        quit(version);  // Quit with a default message of the version.

                }
                

                ///////////////////////////////////////////////////
                // The JOIN command.  Used to join an IRC channel//
                // and may have a key (if the channel is locked) //
                ///////////////////////////////////////////////////
                if (((command == "/JOIN") || (command == "/J")))
                {
                    // Set our channel
                    string chan = commandString[1].ToString();

                    // Does the channel have the necessary #?
                    if (!chan.StartsWith("#"))
                    {
                        // We'll add it here.
                        chan = "#" + commandString[1].ToString();
                    }


                    // Does this join command have a key for the channel?
                    // Check for the max number of words in the array first.
                    int commandLength = commandString.Length;

                    if (commandLength >= 3)
                    {
                        // Is the next word a space or null?
                        if (commandString[2].ToString() != "")
                        {
                            // We have a key!  Let's get the key and join the channel.
                            string key = commandString[2].ToString();
                            join(chan, key);
                        }
                    }

                // No, it doesn't, so just join a regular channel
                    else
                        join(chan);
                }


                ///////////////////////////////////////////////////
                // The PART command.  Used to leave a channel    //
                ///////////////////////////////////////////////////
                if (((command == "/PART") || (command == "/LEAVE")))
                {
                    int commandLength = commandString.Length;

                    string channelToLeave = "";
                    if (commandLength >= 2)
                        channelToLeave = commandString[1];

                    if (channelToLeave == "")
                        channelToLeave = channel;

                    if (channelsCurrentlyIn > 0)
                    {
                        if (channelToLeave.StartsWith("#"))
                            part(channelToLeave);
                        else
                            Console.WriteLine("Error: Need a channel to leave. /PART #channel");
                    }
                
                }

                ///////////////////////////////////////////////////
                // The RECONNECT command.                        //
                ///////////////////////////////////////////////////
                if (command == "/RECONNECT")
                {
                    writer.WriteLine("QUIT :Reconnecting");
                    writer.Flush();
                    setConnectionState(false);
                }

                ///////////////////////////////////////////////////
                // The "Say" command to send a message to the    //
                // active channel.                               //
                ///////////////////////////////////////////////////
                if (command == "/SAY")
                {
                    if (channelsCurrentlyIn > 0)
                    {
                        string channelToSendMessage = "";
                        int messageStartingPoint = 1;

                        if (commandString.Length >= 2)
                            channelToSendMessage = commandString[1];
                        else
                            Console.WriteLine("Error: /SAY #channel message");

                        if (channelToSendMessage.StartsWith("#"))
                            messageStartingPoint = 2;
                        else
                            channelToSendMessage = channel;

                        // Determine what the message is.
                        int commandLength = commandString.Length - 1;
                        string message = "";

                        for (;  messageStartingPoint <= commandLength; messageStartingPoint++)
                        {
                            message = message + commandString[messageStartingPoint] + " ";
                        }

                        if (channelToSendMessage != "")
                            sendToChannel(message, channelToSendMessage);
                        else
                            Console.WriteLine("Error: /SAY #channel message");
                    }
                    else
                        Console.WriteLine("Must join a channel before sending any messages to it.");

                }


                ///////////////////////////////////////////////////
                // The "Msg" command to send a message to a      //
                // user.                                         //
                ///////////////////////////////////////////////////
                if (command == "/MSG")
                {
                    string whereToSendMessage = "";
                    int messageStartingPoint = 1;

                    if (commandString.Length >= 2)
                        whereToSendMessage = commandString[1];
                    else
                        Console.WriteLine("Error: /MSG person message");


                    messageStartingPoint = 2;
                    // Determine what the message is.
                    int commandLength = commandString.Length - 1;
                    string message = "";

                    for (; messageStartingPoint <= commandLength; messageStartingPoint++)
                    {
                        message = message + commandString[messageStartingPoint] + " ";
                    }

                    if (whereToSendMessage != "")
                        sendToChannel(message, whereToSendMessage);
                    else
                        Console.WriteLine("Error: /MSG person message");
                }


                ///////////////////////////////////////////////////
                // The "raw" command to send a message to the    //
                // server.                                       //
                ///////////////////////////////////////////////////
                if (command == "/RAW")
                {

                    // Determine what the message is.
                    int commandLength = commandString.Length - 1;
                    string message = "";

                    for (int i = 1; i <= commandLength; i++)
                    {
                        message = message + commandString[i] + " ";
                    }

                    // Send the message to the command
                    writer.WriteLine(message);
                    writer.Flush();
                }


                ///////////////////////////////////////////////////
                // The CLEAR command.                            //
                ///////////////////////////////////////////////////
                if (command == "/CLEAR")
                {
                    Console.Clear();
                }

                ///////////////////////////////////////////////////
                // The TOGGLEPONG command.                       //
                ///////////////////////////////////////////////////
                if (command == "/TOGGLEPONG")
                {
                    if (showPongReply)
                        showPongReply = false;
                    else
                        showPongReply = true;

                    Console.WriteLine("Show Pong Messages Set To: " + showPongReply);

                }

                ///////////////////////////////////////////////////
                // A small amount of debug info.                 //
                ///////////////////////////////////////////////////
                if (command == "/DINFO")
                {
                    Console.WriteLine("Reconnects: " + reconnectAttempts + "/" + retryAttempts);
                    Console.WriteLine("PING control is currently: " + canPing + " :: Ping Attempt #: " + pongAttempt + "/5");
                    Console.WriteLine("Connected: " + connected);
                }

                ///////////////////////////////////////////////////
                // A small amount of twitter debug info.         //
                ///////////////////////////////////////////////////
                if (command == "/TWITTERINFO")
                {
                    Console.WriteLine("Use Twitter Client?: " + useTwitter + " :: " + "Twitter Client is on?: " + twitterClientOn);
                    if (twitterClientOn)
                        Console.WriteLine("Current twitter message: " + twitterMessage);
                    // Console.WriteLine("Twitter username: " + twitterUserName + " :: " + "Twitter password: " + twitterPassword);
                }


            }
            else
            {
                Console.WriteLine("The bot is not online.  Connect first.");
            }

        }

#endregion

    }
}
