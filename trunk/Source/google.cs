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
// This file was last updated on: 7/01/2008   //
////////////////////////////////////////////////

using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;

namespace Kon
{
    class googleBrain
    {

#region variables
        private               int    lastMatchUsed     = 0;
        private const String regExPattern = @"<a\shref=(?<siteURL>.*?)>.*?</a>(?<rawNotUsed>.*?)<div\s*class=std>.*?<b>(?<title>.*?)</b>(?<content>.*?)<br>.*?";
        
        
#endregion

        public googleBrain()
        {
        }

        public String getReply(String conversation)
        {
            String reply = "";
            int amountOfMatches = 0;
            int minMatch = 0;

            try
            {
                // First things first, let's set up what we're searching for.
                String googleUrl = "http://www.google.com/search?q=" + conversation;

                // Now we'll grab the results page and store it into memory
                String html = GetHtml(googleUrl);


                // It's an HTML file and we don't want all the HTML stuff.  We just want our result. To do that,
                // we need to first sort out all of the titles so that we can filter certain sites.
                Regex r = new Regex(regExPattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);


                // In case people are flooding the bot, let's slow the thread down for a moment.
                Thread.Sleep(200); 

                // Search for our match
                MatchCollection matches = r.Matches(html, 250);
                amountOfMatches = matches.Count;


                // If it found any matches we should randomly pick one.  
                if (amountOfMatches > 0)
                {
                    // First, let's filter out Youtube.
                    ArrayList content = new ArrayList();
                    content = SiteCheck(matches, "myspace", amountOfMatches, content);

                    // Okay, reset the amount of matches to match how many content items we found.
                    amountOfMatches = content.Count;

                    Random randChance = new Random();
                    int random;

                    if (amountOfMatches == 0)
                        random = 0;

                    else
                        random = randChance.Next(minMatch, (amountOfMatches-1));
    

                    // This should cut down on having the same results back to back, if it can.
                    if (amountOfMatches > 1)
                    {
                        int contentTest = 0;
                        while (((random == lastMatchUsed)) && (contentTest < 3))
                        {
                            random = randChance.Next(minMatch, amountOfMatches);
                            contentTest++;
                        }
                    }

                    // Get the reply.
                    if (content.Count > 0)
                    {
                        reply = content[random].ToString();
                        lastMatchUsed = random;
                    }
                 
                    // If the reply isn't null, we need to do a few things to it before we return it.
                    if (reply != "")
                    {
                        // Make the reply all lowercase.
                        reply = reply.ToLower();

                        // Filter out the reply twice.  This is a really shoddy way of catching things that the 
                        // first pass doesn't.
                        reply = filterReply(reply);
                        reply = filterReply(reply);
                    }

                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Error: " + e.ToString());
            }

            // Return the reply.
            return reply;
        }


#region GetHTML
        private static String GetHtml(String url)
        {
            using (StreamReader sr = new StreamReader(GetStreamFromUrl(url)))
            { 
                return sr.ReadToEnd(); 
            }
        }
#endregion

#region SiteCheck
        private static ArrayList SiteCheck(MatchCollection matches, String siteName, int numberOfMatches, ArrayList content)
        {
            siteName = "\"" + siteName;
            String title = "";
            siteName = siteName.ToLower();
            bool addContent = true;
                       

            // Move through each title and check for the sites.  

            for (int i = 0; i < matches.Count - 1; i++)
            {

                title = matches[i].Groups["siteURL"].Value.ToString().ToLower();
                title = filterReply(title);
                String[] split = title.Split('.');

                if (title.StartsWith(siteName, StringComparison.CurrentCulture))
                    addContent = false;
                if (title.StartsWith("\"/url?q=youtube", StringComparison.CurrentCulture))
                    addContent = false;
                if (title.StartsWith("\"youtube", StringComparison.CurrentCulture))
                    addContent = false;
                if (title.StartsWith("\"iyouboushi.proboards3", StringComparison.CurrentCulture))
                    addContent = false;
                if (title.StartsWith("\"images.google/images?q", StringComparison.CurrentCulture))
                    addContent = false;
                if (title.StartsWith("\"multipong.multiply/guest", StringComparison.CurrentCulture))
                    addContent = false;

                foreach (String s in split)
                {
                    if (s == "deviantart")
                        addContent = false;
                    if (s == "proboards3/index")
                        addContent = false;
                    if (s == "wikipedia")
                        addContent = false;
                    if (s == "google")
                        addContent = false;
                    if (s == "images")
                        addContent = false;
                }

                // Set up one more array for catching a few more sites.
                String[] split2 = title.Split('/');
                foreach (String s in split2)
                {
                    if (s.StartsWith("spana fl href=", StringComparison.CurrentCulture))
                        addContent = false;
                    if (s == "\"images.google")
                        addContent = false;
                    if (s == "amazon")
                        addContent = false;
                    if (s == "products")
                        addContent = false;
                    if (s == "wiki")
                        addContent = false;
                }

                // One last desperate attempt to catch something that might otherwise be missed.
                String[] contentSplit = matches[i].Groups["content"].Value.ToString().Split('/');

                foreach (String s in contentSplit)
                {
                    if (s.StartsWith("span><nobr><a class=fl", StringComparison.CurrentCulture))
                        addContent = false;
                    if (s == "\"images.google")
                        addContent = false;
                    if (s.StartsWith("search?hl=en&ie=UTF-8&q=related", StringComparison.CurrentCulture))
                        addContent = false;
                }

                if (addContent)
                {
                    content.Add(matches[i].Groups["content"].Value.ToString());
                }

                addContent = true;

            }

            return content;
        }
#endregion



#region GetStreamFromURL
        private static Stream GetStreamFromUrl(String url)
        {
            return new WebClient().OpenRead(url);
        }
#endregion

#region intConverstionChecker
        private static bool intConverstionCheck(char charToCheck)
        {
            try
            {
                Int32.Parse(charToCheck.ToString());
                return true;
            }

            catch (Exception)
            {
                return false;
            }
        }
#endregion

#region filterReply
        private static String filterReply(String reply)
        {
            // Now we have to strip out any extra HTML tags, file extensions, and a few other things
            // from the reply before we return it.

            Filter filter = new Filter();
            reply = filter.replaceWithNull_emoticons(reply);
            reply = filter.replaceWithNull_HTML(reply);
            reply = filter.replaceWithNull_ImageExt(reply);
            reply = filter.replaceWithNull_webbased(reply);
            reply = filter.replaceWithNull_swears(reply);
            reply = filter.replace_custom(reply);

            // Remove whitespace at the beginning and ending
            reply = reply.Trim();

            // Check to see if the first few chars are ints.  If so, remove them.
            while (intConverstionCheck(reply[0]) == true)
            {
                // remove the starting int
                reply = reply.Remove(0, 1);

                // remove the new whitespace it creates
                reply = reply.Trim();
            }

            if (reply.StartsWith("."))
            {
                reply.Remove(0, 1);

                // remove the new whitespace it creates
                reply = reply.Trim();
            }

            if (reply.StartsWith(";."))
            {
                reply.Remove(0, 2);

                // remove the new whitespace it creates
                reply = reply.Trim();
            }

            if (reply.StartsWith(";"))
            {
                reply.Remove(0, 1);

                // remove the new whitespace it creates
                reply = reply.Trim();
            }

            if (reply.StartsWith("??"))
            {
                reply.Remove(0, 2);

                // remove the new whitespace it creates
                reply = reply.Trim();
            }

            if (reply.StartsWith("?"))
            {
                reply.Remove(0, 1);

                // remove the new whitespace it creates
                reply = reply.Trim();
            }
            // now we need to see if the line ends with a space followed by a floating period.  If so, remove it.
            if (reply.EndsWith(" ."))
            {
                reply = reply.Remove(reply.Length - 1, 1);

                // remove the new whitespace it creates
                reply = reply.Trim();
            }

            // now we need to see if the line ends with a comma.  If so, remove it.
            if (reply.EndsWith(","))
            {
                reply = reply.Remove(reply.Length - 1, 1);
            }
            return reply;
        }
#endregion




    
    }
}
