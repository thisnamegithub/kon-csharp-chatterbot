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
// This file was last updated on: 08/11/2013  //
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

#region variables

        // Public variables

        public static AI              myAI;               // Kon's own brain (AI.cs)
        public static string          nickNameMain         = "Kon";
            
        // Private variables
        private        int             randomTalk          = 0;
        private        int             ircPort             = 6667;
        private        int             channelsCurrentlyIn = 0;
        private        int             retryAttempts       = 5;    // How many times will the bot try to reconnect before giving up?
        private        string          ircServer           = "bots.esper.net";
        private        string          nickNameBckup       = "KonPlushie";
        private        string          tempNick            = "";
        private        string          channel             = "#KonBot";
        private        string          version             = "Kon - Version 1.5.2 - created by James Iyouboushi [iyouboushi@gmail.com]";
        private        string          modifiedLine        = "";
        private        string          rawLine             = "";
        private        string          channelUsers;
        private        string          realName            = "Kon";
        private        string          ident               = "Splash";
 
        private        bool            connected           = false;
        private        bool            canQuit             = false;
        private        bool            AIML                = false;
        private        bool            topics              = true;
        public static  bool            LB                  = true;
        public static  bool            logging             = true;
        private        bool            showServerMsgs      = false;
        private        bool            showPongReply       = false;
        private        bool            readerStreamClosed  = true;
        private        bool            doubleSentences     = true;
        public static  bool            answerSearch        = true;

        private        TcpClient       ircClient;
        private        NetworkStream   stream;
        private        Thread          listenerThread;
        private static StreamWriter    writer;
        private static StreamWriter    logger;
        private static StreamReader    reader;
        private static AIMLbrain       myAIML;             // Kon's AIML brain (AIML.cs)
        public  static LBbrain         myLB;               // Kon's LB Brain (LB.cs)
    //    private static msn             msnClient;          // Kon's MSN client (msn.cs)
        private        Random          randnum             = new Random();
        
        // Private constants
        private const  string          CRLF                = "\r\n";
        private const  string          PING                = "PING :";
        private const  string          PONG                = "PONG ";
        private static string          LOG_FILE            = "KonLog.txt";

#endregion


#region constructors

        ////////////////////////////////////////////////
        //////////////CONSTRUCTORS//////////////////////
        // There are two constructors.  One will only //
        // use defaults found at the top of this file.//
        // That constructor shouldn't be called but is//
        // merely a failsafe.                         //
        ////////////////////////////////////////////////
        public IRC()
        {
            // Check to see if we need to turn on the log file.
            LogFileOn();

            // Connect to IRC
            connect();
        }

        public IRC(String server, int port, String mainNick, String backupNick, String chan, int randomTalkPercent, int retry, bool twitterUse, String tAccessToken, String tAccessTokenSecret, String tOATHAccessToken, String tOATHTokenSecret, String incomingIdent, String incomingRealName)
        {
            // Validate the incoming information.  Keep in mind that if any of these are wrong it 
            // will use defaults defined at the top of this file.
            if (server != "")
                ircServer = server;
            if (port != 0)
                ircPort = port;
            if (mainNick != "")
                nickNameMain = mainNick;
            if (backupNick != "")
                nickNameBckup = backupNick;
            if (chan != "")
                channel = chan;
            if (randomTalkPercent != 0)
                randomTalk = randomTalkPercent;
            if (retry != 0)
                retryAttempts = retry;

            ident = incomingIdent;
            realName = incomingRealName;

            // Check to see if we need to turn on the log file.
            LogFileOn();

            if (twitterUse)
            {
                if ((((tAccessToken != "null") && (tAccessTokenSecret != "null") && (tOATHAccessToken != "null") && (tOATHTokenSecret != "null"))))
                {
                    useTwitter = true;
                    twitterAccessToken = tAccessToken;
                    twitterAccessTokenSecret = tAccessTokenSecret;
                    oauth_consumer_key = tOATHAccessToken;
                    oauth_consumer_secret = tOATHTokenSecret;

                    // Now turn the Twitter Client on.
                    TwitterControl();
                }
                else
                    useTwitter = false;
            }

            try
            {
                // Connect to IRC using the settings
                connect();
            }
            catch (Exception e)
            {
                // Show the exception
                DisplayData("ERROR IN CONNECTING... " + e.ToString());

                // Sleep, before we try again
                Thread.Sleep(15000);
                setConnectionState(false);
            }

        }

#endregion


#region connect
        private void connect()
        {
            // Tell people we're going to connect
            Console.WriteLine("Connecting to " + ircServer + ":" + ircPort + " ...\n");

            try
            {
                // Try to make a connection to the server
                ircClient = new TcpClient(ircServer, ircPort);
                stream = ircClient.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);
                readerStreamClosed = false;

                // We're connected, so let's set our bool to true.
                setConnectionState(true);

                // Let the ping control know we can ping now.
                setCanPing(true);

                // Set the current ping attempt to 0.  Really useful for when the bot has to reconnect for any reason.
                pongAttempt = 0;

                // Start up our incoming data listener
                IncomingData();

                // Authorize who we are.
                authorize();

                // Let's turn on the AI stuff.
                myAI = new AI();

                if (AIML == true)
                    myAIML = new AIMLbrain();

                if (LB == true)
                    myLB = new LBbrain();

                Thread.Sleep(1500);

                // Auto join the channel, if there is one
                if (channel != "")
                {
                    Thread.Sleep(500);
                    join(channel);
                }
                
            }
            catch (Exception e)
            {
                // Show the exception
                DisplayData("ERROR: " + e.ToString());
                setConnectionState(false);
            }

            Thread.Sleep(1000);
            // Let's turn on the twitter client, if it needs to be turned on.
            if (useTwitter)
            {
                twitterClientOn = true;
                Thread.Sleep(500);

                sendInitialTwitterMessage();
            }
        }
#endregion


#region reconnect
        public void reconnect()
        {
            DisplayData("** Will reconnect in a few minutes.. [Attempt #:" + reconnectAttempts + " of " + retryAttempts + "]");
            
            disconnect();
            Thread.Sleep(500);

            // Throttle the connection
            Thread.Sleep(180000);
                         
            // If logging is enabled, let's write that we're reconnecting
            if (logging == true)
                LogFile("---------[RECONNECTING: " + DateTime.Now + "]");
            
            // Now let's reconnect to the server.
            connect();
        }
    
#endregion


#region disconnect()
        private void disconnect()
        {
            // If logging is enabled, let's write that we've been disconnected.
            if (logging == true)
                LogFile("---------[DISCONNECTED: " + DateTime.Now + "]");

            setConnectionState(false);

            Thread.Sleep(100);

            // discard everything in the reader buffer before closing it.
            if (reader != null)
            {
                try
                {
                    reader.DiscardBufferedData();
                    reader.Close();
                    readerStreamClosed = true;
                }
                catch (Exception e)
                { }
            }

            if (writer != null)
                writer.Close();

            if (ircClient != null)
                ircClient.Close();

            if (stream != null)
                stream.Close();

            setCanPing(false);  // Will turn it to false so the ping control doesn't try to run while disconnected.

        }
#endregion


#region authorize
        private void authorize()
        {
            Console.WriteLine("Authorizing..");
            autorizeUser();
            Thread.Sleep(500);
 
            authorizeNick();
            Thread.Sleep(500);
        }

        private void autorizeUser()
        {
            // Send the ident information to the server (i.e. our user and who we are)
            string user = ident + " 8 * " + ":" + realName;

          //  string user = "Kon" + " 8 * " + ircServer + ":SPLASH !!!";
            writer.WriteLine("USER " + user);
            writer.Flush();
        }

        private void authorizeNick()
        {
            // Need to add the ability to switch from primary to backup nick if the first is taken
            writer.WriteLine("NICK " + nickNameMain);
            writer.Flush();
        }

        private void autorizeUserHost()
        {
            writer.WriteLine("USERHOST " + nickNameMain);
            writer.Flush();
        }
#endregion


#region quit
        private void quit()
        {
            // Message that we're quitting
            writer.WriteLine("QUIT ");
            writer.Flush();

            // Exit out of the program.
            quit_program();
        }

        private void quit(string message)
        {
            // Message that we're quitting
            writer.WriteLine("QUIT " + ":" + message);
            writer.Flush();

            // Exit out of the program.
            quit_program();
        }

        private void quit_program()
        {

            // If logging is enabled, let's write that we're quitting.
            if (logging)
                LogFile("---------[QUITTING: " + DateTime.Now + "]");

            disconnect();

            Thread.Sleep(500);

            if (logging)
                logger.Close();

            setQuitState(true);
        }
#endregion


#region joinChannel
        // Join a channel, no key
        private void join(string chan)
        {
            writer.WriteLine("JOIN " + chan);
            writer.Flush();

            channelsCurrentlyIn++;
            if (channelsCurrentlyIn == 1)
                channel = chan;
        }


        // Join a channel with a key
        private void join(string channel, string key)
        {
        }
#endregion


#region partChannel
        private void part(string chan)
        {

            // tell the server that we're leaving the channel.
            writer.WriteLine("PART " + chan);
            writer.Flush();

            // tell the user that we've parted.
            Console.WriteLine("** Leaving " + chan);

            channelsCurrentlyIn--;

            if (channelsCurrentlyIn == 0)
                channel = "";

            if (chan.ToUpper() == channel.ToUpper())
                channel = "";
        }
#endregion


#region sendToChannel
        // Send messages something to the channel
        private void sendToChannel(string message, string location)
        {
            if (connected)
            {
                // Let's pause a moment before sending the message
                Thread.Sleep(500);

                // Create the message
                writer.WriteLine("PRIVMSG " + location + " :" + message);

                // Log it, if the logging option is on
                LogFile(">> " + location + " : " + message);

                // Flush it
                writer.Flush();
            }
        }
#endregion


#region IncomingData
        ////////////////////////////////////////////////
        /////////////INCOMING DATA//////////////////////
        ////////////////////////////////////////////////
        // These methods will handle the incoming     //
        // data and process it.                       //
        ////////////////////////////////////////////////

        private void IncomingData()
        {
            // Start a new thread for the incoming data
            listenerThread = new Thread(new ThreadStart(ReceiveData));
            listenerThread.Start();
        }

        private void ReceiveData()
        {

            while ((connected == true) && (readerStreamClosed == false))
            {
                try
                {

                    try
                    {
                        rawLine = reader.ReadLine();
                    }
                    catch (Exception e)
                    {
                        rawLine = null;
                    }


                    if (rawLine != null)
                    {
                        // We have to take a looksie at the line and determine if it's going to be shown/logged.
                        // Break the raw line into tokenized parts so we can determine what to do
                        string[] IncomingDataString;
                        string messageSwitch = " ";
                        IncomingDataString = rawLine.Split(new char[] { ' ' });

                        if (IncomingDataString.Length > 1)
                            messageSwitch = IncomingDataString[1].ToString();
                        else
                            messageSwitch = " ";
                        

                        // Before we even check anything else, is it a PING request? 
                        if (IncomingDataString[0] == "PING")
                        {
                            if (showPongReply)
                                DisplayData(rawLine);

                            handlePingRequest(IncomingDataString);
                        }

                        // Is it a PONG?
                        if ((IncomingDataString[0] == "PONG") || (IncomingDataString[1] == "PONG"))
                        {
                            if (showPongReply)
                                DisplayData(rawLine);

                            pongAttempt = 0;
                        }

                        // is it an error?
                        if (IncomingDataString[0] == "ERROR")
                            handleErrors(IncomingDataString, rawLine);

                        else
                        {
                            switch (messageSwitch)
                            {
                                // Normal messages
                                case "JOIN":
                                    handleJoin(IncomingDataString);
                                    break;
                                case "NOTICE":
                                    handleNotice(IncomingDataString);
                                    break;
                                case "QUIT":
                                    handleQuit(IncomingDataString);
                                    break;
                                case "NICK":
                                    handleNick(IncomingDataString);
                                    break;
                                case "PART":
                                    handlePart(IncomingDataString);
                                    break;
                                case "MODE":
                                    handleMode(IncomingDataString, rawLine);
                                    break;
                                case "TOPIC":
                                    DisplayData(rawLine);
                                    break;
                                case "KICK":
                                    handleKick(IncomingDataString);
                                    break;
                                case "PRIVMSG":
                                    handlePrivMsg(IncomingDataString, IncomingDataString[2].ToString());
                                    break;

                                // Server messages
                                case "001":
                                    DisplayData(rawLine);
                                    autorizeUserHost();
                                    reconnectAttempts = 0;  // It's connected, we can set the attempts to 0 again, like most IRC clients.
                                    Thread.Sleep(600);
                                    break;
                                case "002":
                                    DisplayData(rawLine);
                                    break;
                                case "003":
                                    DisplayData(rawLine);
                                    break;
                                case "004":
                                    DisplayData(rawLine);
                                    autoPerform();   // Now that we know for sure that we're connected, let's auto-perform.
                                    break;
                                case "005":
                                    DisplayData(rawLine);
                                   break;                                   
                                case "251":        // visible & invisible users on # servers
                                    if (showServerMsgs)
                                        DisplayData(rawLine);
                                    break;
                                case "252":        // IRC Operators
                                    if (showServerMsgs)
                                        DisplayData(rawLine);
                                    break;
                                case "253":        // Unknown connections
                                    if (showServerMsgs)
                                        DisplayData(rawLine);
                                    break;
                                case "254":        // # of Channels
                                    if (showServerMsgs)
                                        DisplayData(rawLine);
                                    break;
                                case "255":        // Clients and servers
                                    if (showServerMsgs)
                                        DisplayData(rawLine);
                                    break;
                                case "265":        // Current Local Users
                                    if (showServerMsgs)
                                        DisplayData(rawLine);
                                    break;
                                case "266":        // Current Global Users
                                    if (showServerMsgs)
                                        DisplayData(rawLine);
                                    break;
                                case "332":        // Channel Topic
                                    // handleChannelTopic(IncomingDataString);
                                    if (showServerMsgs)
                                        DisplayData(rawLine);
                                    break;
                                case "333":        // ?? Something to do with the channel
                                    if (showServerMsgs)
                                        DisplayData(rawLine);
                                    break;
                                case "353":        // Who is in the channel
                                    buildChannelUsers(IncomingDataString);
                                    if (showServerMsgs)
                                        DisplayData(rawLine);
                                    break;
                                case "366":        // End of /NAMES list
                                    if (showServerMsgs)
                                        DisplayData(rawLine);
                                    break;
                                case "372":        // MOTD
                                    if (showServerMsgs)
                                        DisplayData(rawLine);
                                    break;
                                case "375":        // MOTD
                                    if (showServerMsgs)
                                        DisplayData(rawLine);
                                    break;
                                case "376":        // END OF MOTD
                                    if (showServerMsgs)
                                        DisplayData(rawLine);
                                    break;
                                case "402":       // No such server
                                    setConnectionState(false);
                                    break;
                                case "409":
                                    setConnectionState(false);
                                    break;
                                case "433":       // Nickname already in use
                                    DisplayData(rawLine);
                                    tempNick = nickNameMain;
                                    nickNameMain = nickNameBckup;
                                    nickNameBckup = tempNick;
                                    Thread.Sleep(500);
                                    authorize();
                                    break;
                                case "442":       // Not in channel
                                    Console.WriteLine("Not currently in that channel.");
                                    channelsCurrentlyIn++;  
                                    break;
                                case "451":       // "Register First" or "You have not registered"
                                    DisplayData(rawLine);
                                    Thread.Sleep(300);
                                    authorize();
                                    Thread.Sleep(500);
                                    if (channel != "")
                                    {
                                        Thread.Sleep(500);
                                        join(channel);
                                    }
                                    break;
                                case "462" :   // "May not re-register"
                                   // if (showServerMsgs)
                                    DisplayData(rawLine);

                                    if (channel != "")
                                    {
                                        Thread.Sleep(500);
                                        join(channel);
                                    }
                                    break;
                                default:
                                    // redundency check.  without this, it actually shows some odd "PINGS" that the previous
                                    // didn't catch.  Since i personally don't like to SEE these pings, I'll set this up to
                                    // remove them from my eyes.
                                    if (IncomingDataString[0] == "PING")
                                    {
                                        if (showPongReply)
                                            DisplayData(rawLine);
                                    }
                                    else if (IncomingDataString[1] == "PONG")
                                    {
                                        if (showPongReply)
                                            DisplayData(rawLine);
                                    }
                                    // For now, let's log everything so I can later catch it.
                                    else
                                        DisplayData(rawLine);
                                    break;
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    // Show the exception
                    Console.WriteLine(e.ToString());
                    Thread.Sleep(5000);
                    setConnectionState(false);
                }

            }
        }

#endregion


#region Methods for Handling Various Messages

        private void handlePrivMsg(string[] IncomingDataString, string location)
        {
            // Determine what the message is.
            int msgLength = IncomingDataString.Length - 1;
            string message = "";
            
            for (int i = 3; i <= msgLength; i++)
            {
                message = message + IncomingDataString[i] + " ";
            }

            // Let's chop off the ":" that starts each message, leaving us with the true message.
            message = message.Remove(0, 1);
           
            // We have the message, let's get the nick.
            string nickname = getNick(IncomingDataString[0].ToString());

            modifiedLine = "[" + location + "] <" + nickname + "> ";
            modifiedLine += message;

            // Okay, we're ready to display/log it.
            DisplayData(modifiedLine);
            
            // Try to prevent an exception
            if ((message == "") || (message == " "))
                message = "null";

            // And now we're ready to see if it's a command the bot will recognize.
            incomingCommand(message, nickname, location);
        }

        private void handleNotice(string[] IncomingDataString)
        {
            // Determine what the message is.
            int msgLength = IncomingDataString.Length - 1;
            string message = "";

            for (int i = 3; i <= msgLength; i++)
            {
                message = message + IncomingDataString[i] + " ";
            }

            // Let's chop off the ":" that starts each message, leaving us with the true message.
            message = message.Remove(0, 1); 

            // We have the message, let's get the nick.
            string nickname = getNick(IncomingDataString[0].ToString());

            modifiedLine = "[NOTICE] " + "<" + nickname + "> " + message;

            // Okay, we're ready to display/log it.
            DisplayData(modifiedLine);

            // Try to prevent an exception
            if ((message == "") || (message == " "))
                message = "null";
        }


        private void handleQuit(string[] IncomingDataString)
        {
            string nickname = getNick(IncomingDataString[0].ToString());
            string message = "";
            int msgLength = IncomingDataString.Length - 1;

            for (int i = 3; i <= msgLength; i++)
            {
                message = message + IncomingDataString[i] + " ";
            }

            DisplayData("* " + nickname + " has quit (" + message + ")");
        }

        private void handleJoin(string[] IncomingDataString)
        {
            string nickname = getNick(IncomingDataString[0].ToString());
            DisplayData("* " + nickname + " has joined " + IncomingDataString[2].ToString().Replace(":",""));
        }

        private void handlePart(string[] IncomingDataString)
        {
            // Needs to be expanded to handle multiple channels in the future
            string nickname = getNick(IncomingDataString[0].ToString());
            DisplayData("* " + nickname + " has left " + IncomingDataString[2].ToString());
        }

        private void handleMode(string[] IncomingDataString, string rawLine)
        {
            // Needs to be expanded to handle multiple channels in the future
            string nickname = getNick(IncomingDataString[0].ToString());


            if (IncomingDataString.Length == 4)
                DisplayData("* " + nickname + " sets mode" + IncomingDataString[3].ToString());
            if (IncomingDataString.Length == 5)
                DisplayData("* " + nickname + " sets mode: " + IncomingDataString[3].ToString() + " " + IncomingDataString[4].ToString() + " in " + IncomingDataString[2].ToString());
            if (IncomingDataString.Length == 6)
                DisplayData("* " + nickname + " sets mode: " + IncomingDataString[3].ToString() + " " + IncomingDataString[4].ToString() + " " + IncomingDataString[5].ToString() + " in " + IncomingDataString[2].ToString());
            if (IncomingDataString.Length == 7)
                DisplayData("* " + nickname + " sets mode: " + IncomingDataString[3].ToString() + " " + IncomingDataString[4].ToString() + " " + IncomingDataString[5].ToString() + " in " + IncomingDataString[2].ToString());
            if (IncomingDataString.Length == 8)
                DisplayData("* " + nickname + " sets mode: " + IncomingDataString[3].ToString() + " " + IncomingDataString[4].ToString() + " " + IncomingDataString[5].ToString() + " " + IncomingDataString[6].ToString() + " in " + IncomingDataString[2].ToString());
            if ((IncomingDataString.Length > 8) || (IncomingDataString.Length < 4))
                DisplayData(rawLine);
        }

        private void handleKick(string[] IncomingDataString)
        {
            string nickname = getNick(IncomingDataString[0].ToString());
            string message = "";
            int msgLength = IncomingDataString.Length - 1;

            for (int i = 4; i <= msgLength; i++)
            {
                message = message + IncomingDataString[i] + " ";
            }
            message = message.Remove(0, 1);
            message = message.Trim();
            DisplayData("* " + IncomingDataString[3] + " was kicked out of " + IncomingDataString[2] + " by " + nickname + " (" + message + ")");
        }

        private void handleNick(string[] IncomingDataString)
        {
            string nickname = getNick(IncomingDataString[0].ToString());
            DisplayData("* " + nickname + " is now known as " + IncomingDataString[2].ToString().Replace(":", ""));
        }

        private void handleErrors(string[] IncomingDataString, string rawLine)
        {
            string typeOfError = IncomingDataString[1] + " " + IncomingDataString[2];

            if (typeOfError == ":Closing Link:")
            {
                string closingType = IncomingDataString[4];

                if (closingType == "(Ping")
                {
                    //Throttle the connection
                    Thread.Sleep(5000);

                    DisplayData(rawLine);
                    setConnectionState(false);
                }

                if (closingType == "(Quit)")
                    DisplayData("** Quitting IRC");
            }
            else
                DisplayData(rawLine);
        }

        private void handlePingRequest(string[] IncomingDataString)
        {
            string pingHash = "";
            for (int i = 1; i < IncomingDataString.Length; i++)
            {
                pingHash += IncomingDataString[i] + " ";
            }
            writer.WriteLine("PONG " + pingHash);
            writer.Flush();

            if (showPongReply)
                DisplayData("PONG " + pingHash);
        }
#endregion


#region DisplayData(rawLine)
        public static void DisplayData(string rawLine)
        {
            string command = "";
            string[] IncomingMessage = rawLine.Split(new char[] { ' ' });
            try
            {
                command = IncomingMessage[2].ToString();
            }
            catch
            {
                Console.WriteLine("Error");
            }

            if (command.ToUpper() == "!MSN")
            {
                Console.WriteLine(rawLine);
            }

            else
            {
                // If logging is enabled, let's write it to a log file.
                LogFile(rawLine);

                // Write the line to the console, after a small throttle.
                Thread.Sleep(100);
                Console.WriteLine(rawLine);
            }
        }
#endregion


#region getNick(rawName)
        private string getNick(string rawName)
        {
            string nickname = "";
            int nameLength = rawName.Length - 1;

            for (int i = 1; i <= nameLength; i++)
            {
                if (rawName[i].ToString() == "!")
                    break;
                else
                    nickname = nickname + rawName[i];
            }

            return nickname;
        }
#endregion


#region Channel Users commands

        private void buildChannelUsers(string[] IncomingDataString)
        {
            Thread.Sleep(10);
            channelUsers = "";

            // :excelsior.esper.net 353 Iyouboushi @ #kyoto :@Kia_Purity Striker @Iyouboushi Thndr ItsOnlyTheEd Silver-Streak Tolan Tontetsu Rooks @BishounenNightBird
            for (int i = 5; i < IncomingDataString.Length; i++)
            {
                string currentName = IncomingDataString[i].ToString();
                currentName = currentName.Replace(":", "");
                currentName = currentName.Replace("@", "");
                currentName = currentName.Replace("+", "");

                if (currentName != nickNameMain)
                    channelUsers += currentName + " ";
            }
        }

        private string randomChannelUser()
        {
            // Take the current channel user list, split it by spaces.  Add them to an array.  Randomly pick one from
            // the array.  Return that user.
            string[] channelUsersArray;
            channelUsersArray = channelUsers.Split(new char[] { ' ' });

            // Get a random user.
            Thread.Sleep(60);
            int rnd = randnum.Next((channelUsersArray.Length-1));
            string user = (string)channelUsersArray[rnd];

            return user;
        }

#endregion

#region autoPerform()
        private void autoPerform()
        {
            Thread.Sleep(1000);

            String performFile = "perform.kon";
            String fullPerformPath = performFile;
            String performLine = "";
            if (File.Exists(fullPerformPath))
            {
 
                // Read the file, get the lines, do the commands.
                StreamReader autoPerform = null;
                try
                {
                    autoPerform = new StreamReader(performFile);
                    while ((performLine = autoPerform.ReadLine()) != null)
                    {
                        performLine = performLine.Replace("MYBOTNAME", nickNameMain);

                        string[] IncomingCommand;
                        string commandSwitch = " ";
                        IncomingCommand = performLine.Split(new char[] { ' ' });

                        if (IncomingCommand.Length > 1)
                        {
                            commandSwitch = IncomingCommand[0].ToString();
                            commandSwitch = commandSwitch.Replace("/", "");
                            commandSwitch = commandSwitch.ToUpper();
                        }

                        if ((IncomingCommand[0].ToString().ToUpper() == "WAIT") || (IncomingCommand[0].ToString().ToUpper() == "/WAIT")) 
                            commandSwitch = "WAIT";

                        switch (commandSwitch)
                        {
                            case "WAIT":
                                Thread.Sleep(2000);
                                break;
                            case "JOIN":
                                join(IncomingCommand[1].ToString());
                                Thread.Sleep(100);
                              break;
                            case "SAY":
                            case "MSG":
                              string whereToSendMessage = "";
                              int messageStartingPoint = 1;

                              if (IncomingCommand.Length >= 2)
                                  whereToSendMessage = IncomingCommand[1];
                              else
                                  Console.WriteLine("Auto Perform Error: MSG/SAY person/channel message");

                              messageStartingPoint = 2;
                              // Determine what the message is.
                              int commandLength = IncomingCommand.Length - 1;
                              string message = "";

                              for (; messageStartingPoint <= commandLength; messageStartingPoint++)
                              {
                                  message = message + IncomingCommand[messageStartingPoint] + " ";
                              }

                              if (whereToSendMessage != "")
                                  sendToChannel(message, whereToSendMessage);

                              Thread.Sleep(100);
                                break;
                            case "RAW":
                                int rawStart = 1;

                                if (IncomingCommand.Length == 0)
                                    Console.WriteLine("Auto Perform Error: RAW command");

                                messageStartingPoint = 1;
                                // Determine what the message is.
                                int rawLength = IncomingCommand.Length - 1;
                                string rawMsg = "";

                                for (; rawStart <= rawLength; rawStart++)
                                {
                                    rawMsg = rawMsg + IncomingCommand[rawStart] + " ";
                                }


                                writer.WriteLine(rawMsg);
                                writer.Flush();

                                Thread.Sleep(100);
                                break;

                            default:
                                Console.WriteLine("Command not found");
                                Thread.Sleep(100);
                                break;
                        }


                    }
                }


                finally
                {
                    if (performFile != null)
                        autoPerform.Close();
                }



            }
            
        }

#endregion



#region ConnectionState
        private void setConnectionState(bool state)
        {
            connected = state;
        }

        public bool getConnectionState()
        {
            return connected;
        }
#endregion


#region QuitState
        private void setQuitState(bool state)
        {
            canQuit = state;
        }

        public bool getQuitState()
        {
            return canQuit;
        }
#endregion


    }
}
