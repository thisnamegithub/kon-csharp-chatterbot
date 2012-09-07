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
// This file was last updated on: 12/02/2011  //
////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading;


namespace Kon
{
    class Program
    {
        static void Main(string[] args)
        {


#region FileCheck
            System.Console.WriteLine("Checking for config file..\n");

            String configFile = "config.xml";  // the Configuration File
            String fullpath = configFile;

            if (!File.Exists(fullpath))
            {
                System.Console.WriteLine("Generating config file...\n");
                generateConfig(fullpath);
                System.Console.WriteLine("REMEMBER to edit config file and add your nick to the admin list to have");
                System.Console.WriteLine("full control over the bot.");
                Thread.Sleep(2000);
                System.Console.WriteLine("Press Enter to quit the program.");
                System.Console.ReadLine();
                System.Environment.Exit(-1);
            }

            System.Console.WriteLine("Checking for Auto Perform file..\n");

            String performFile = "perform.kon"; 
            String fullPerformPath = performFile;
            if (!File.Exists(fullPerformPath))
            {
                System.Console.WriteLine("Generating Auto Perform file...\n");
                FileStream fs = null;
                using (fs = File.Create(fullPerformPath)) { }
                System.Console.WriteLine("You can edit this file in notepad or another simple text editor.\n");
                System.Console.WriteLine("Perform commands recognized: JOIN, MSG, SAY, WAIT\n");
                Thread.Sleep(2000);
            }


#endregion
            
            // Now we're ready to read the information from the config file and start the bot.
            System.Console.WriteLine("Starting the client..\n");

#region getConfigValues
            String server = getData(fullpath, "server");
            int port;
            int randomTalk;
            int retryAttempts;
            String mainNick = getData(fullpath, "mainNick");
            String backupNick = getData(fullpath, "backupNick");
            String channel = getData(fullpath, "channel");
            String twitterAccessToken = "";
            String twitterAccessTokenSecret = "";
            String ident = "";
            String realName = "";
            bool useTwitter = false;
            

            try
            {
                port = System.Convert.ToInt32(getData(fullpath, "port"));
            }

            catch (Exception e)
            {
                System.Console.WriteLine("Error found in the port: " + e.ToString());
                // This will make sure the port isn't null and is a valid number.  
                // Note that it won't catch ports that are out of range.  Yet.
                port = 0;
            }

            try
            {
                randomTalk = System.Convert.ToInt32(getData(fullpath, "randomTalk"));
            }

            catch (Exception e)
            {
                System.Console.WriteLine("Error found in the randomTalk percent: " + e.ToString());
                // This will make sure the port isn't null and is a valid number.  
                // Note that it won't catch ports that are out of range.  Yet.
                randomTalk = 0;
            }

            try
            {
                retryAttempts = System.Convert.ToInt32(getData(fullpath, "retryAttempts"));
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Error found in the retry attempts number: " + e.ToString());
                retryAttempts = 5;
            }

            try
            {
                if (getData(fullpath, "useTwitter").ToString() == "true")
                    useTwitter = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                useTwitter = false;
            }

            try
            {
                twitterAccessToken = getData(fullpath, "twitterAccessToken");
            }
            catch (Exception e)
            {
                twitterAccessToken = "null";
                useTwitter = false;
            }

            try
            {
                twitterAccessTokenSecret = getData(fullpath, "twitterAccessTokenSecret");
            }
            catch (Exception e)
            {
                twitterAccessTokenSecret = "null";
                useTwitter = false;
            }

            try
            {
                ident = getData(fullpath, "ident");
            }
            catch (Exception e)
            {
                ident = "Splash !!!";
            }

            try
            {
                realName = getData(fullpath, "realName");
            }
            catch (Exception e)
            {
                realName = "Kon";
            }

#endregion

            IRC irc;
            irc = new IRC(server, port, mainNick, backupNick, channel, randomTalk, retryAttempts, useTwitter, twitterAccessToken, twitterAccessTokenSecret, ident, realName);

            bool programCanQuit = false;
            bool connected;

            irc.connectionThread();  // Start the connection checking.
            irc.PingControl();  // Start the ping control.

            // The main program loop to check to see if we're still connected.
            while (programCanQuit == false)
            {
                programCanQuit = irc.getQuitState();
                connected = irc.getConnectionState();

                if (connected)
                    irc.getCommand();
            }

            System.Console.WriteLine("\nKon can now be closed successfully...\n");
            System.Console.ReadLine();
        }



#region generateConfig
        static void generateConfig(String fullpath)
        {
            
            // No config file exists.  Let's generate a default one.
            XmlTextWriter configWriter = new XmlTextWriter(fullpath, null);
            try
            {
                // Set up the formatting of the XML file.
                configWriter.Formatting = Formatting.Indented;
                configWriter.Indentation = 6;
                configWriter.Namespaces = false;

                // Write the first element in the tree.  In this case, we're going to write it under config.
                configWriter.WriteStartElement("", "config", "");

                // Now we're going to write the default server
                configWriter.WriteStartElement("", "server", "");
                configWriter.WriteString("bots.esper.net");
                configWriter.WriteEndElement();

                // Now we're going to write the default port
                configWriter.WriteStartElement("", "port", "");
                configWriter.WriteString("6667");
                configWriter.WriteEndElement();

                // Now we're going to write the default ident.
                configWriter.WriteStartElement("", "ident", "");
                configWriter.WriteString("SPLASH !!!");
                configWriter.WriteEndElement();

                // Now the "Real Name"
                configWriter.WriteStartElement("", "realName", "");
                configWriter.WriteString("Kon");
                configWriter.WriteEndElement();

                // Now we're going to write the default mainNick
                configWriter.WriteStartElement("", "mainNick", "");
                configWriter.WriteString("KonBot");
                configWriter.WriteEndElement();

                // Now we're going to write the default backupNick
                configWriter.WriteStartElement("", "backupNick", "");
                configWriter.WriteString("KonPlushie");
                configWriter.WriteEndElement();

                // We need to have a channel for the bot to sit in.
                configWriter.WriteStartElement("", "channel", "");
                configWriter.WriteString("#konBot");
                configWriter.WriteEndElement();

                // Now we're going to write the randomTalk percentage; default to 1%
                configWriter.WriteStartElement("", "randomTalk", "");
                configWriter.WriteString("1");
                configWriter.WriteEndElement();

                // Set the # of reconnect attempts to 5 by default.
                configWriter.WriteStartElement("", "retryAttempts", "");
                configWriter.WriteString("5");
                configWriter.WriteEndElement();

                // Add an admin.  This part will be a placeholder.
                configWriter.WriteStartElement("", "admin", "");
                configWriter.WriteString("WriteYourNickHere");
                configWriter.WriteEndElement();

                // Do we want the bot to connect to Twitter?  Off by default.
                configWriter.WriteStartElement("", "useTwitter", "");
                configWriter.WriteString("false");
                configWriter.WriteEndElement();

                // Twitter user-name.
                configWriter.WriteStartElement("", "twitterAccessToken", "");
                configWriter.WriteString("Your twitter account access token goes here");
                configWriter.WriteEndElement();

                // Twitter password
                configWriter.WriteStartElement("", "twitterAccessTokenSecret", "");
                configWriter.WriteString("Your twitter account access token secret goes here");
                configWriter.WriteEndElement();

                // End the file
                configWriter.WriteEndElement();

                configWriter.Flush();
                configWriter.Close();
            }

            catch (Exception e)
            {
                System.Console.WriteLine("Error in creating the default config file! " + e.ToString());
            }

            }
#endregion


#region readConfig
        static public String getData(String file, String node)
        {
            XmlTextReader configReader = new XmlTextReader(file);
            configReader.WhitespaceHandling = WhitespaceHandling.None;
            configReader.Namespaces = false;
            String data = "";

            try
            {
                while (configReader.Read())
                {
                    if (configReader.NodeType == XmlNodeType.Element)
                    {
                        if (configReader.LocalName.Equals(node))
                        {
                            data = configReader.ReadString();
                            break;
                        }
                    }
                }
            }

            catch (Exception e)
            {
                System.Console.WriteLine("Error found: " + e.ToString() + "\n");
            }

            configReader.Close();

            // Finally, return with the data we were after.
            return data;
        }
#endregion


    }
}
