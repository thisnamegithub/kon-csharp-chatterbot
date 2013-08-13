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
// This file was last updated on: 08/13/2013  //
////////////////////////////////////////////////
////////////////TO DO///////////////////////////
// find a better method of filtering out some///
// of the similar elements to help increase  ///
// speed.                                    ///
////////////////////////////////////////////////

using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Kon
{
    class Filter
    {

#region variables
        private Random randnum = new Random();
#endregion


        public Filter()
        {
        }

#region canAddToBrain
        // This function goes through a few basic filters that will be present in the basic and LB brains and determines if the line should be added or not.
        // returns false if it should be ignored, or true if it should continue on.
        public static bool canAddToBrain(string conversation, string nick)
        {
            // These are never going to change and can be filtered here.
            if (((((conversation.StartsWith("http:")) || (conversation.StartsWith("https:")) || (conversation.StartsWith("www.")) || (conversation.StartsWith("ftp:"))))))
                return false;  // We don't want to add URLs.

            if (conversation.StartsWith("!"))
                return false;

            if (conversation.StartsWith(""))
                return false;

            if (conversation.StartsWith("["))
                return false;

            if (conversation.StartsWith("<"))
                return false;

            if (conversation.StartsWith("#"))
                return false;

            // We don't want to capture stuff that's supposed to be for or from LB either.
            if (conversation.StartsWith("lb:"))
                return false;

            if (((nick.ToUpper() == "LB4") || (nick.ToUpper() == "LB5") || (nick.ToUpper() == "LB6")))
                return false;

            if (nick.ToUpper() == "ESPERBOT")
                return false;

            if ((conversation.ToUpper().StartsWith("PING")) || (conversation.ToUpper().StartsWith("VERSION")))
                return false;

            // we don't want stuff that's too short.
            if (conversation.Length < 5)
                return false;

            return true;
        }
#endregion


#region replaceWithNull_emoticons
        public String replaceWithNull_emoticons(String input)
        {
            input = input.Replace(":P", "");
            input = input.Replace("=P", "");
            input = input.Replace(":)", "");
            input = input.Replace(":D", "");
            input = input.Replace("=D", "");
            input = input.Replace("XD", "");
            input = input.Replace("=P", "");
            input = input.Replace(":(", "");
            input = input.Replace(":<", "");
            input = input.Replace(":]", "");
            input = input.Replace(":[", "");
            input = input.Replace(">:(", "");
            input = input.Replace(">:D", "");
            input = input.Replace(";-;", "");
            input = input.Replace("o.o", "");
            input = input.Replace("0_o", "");
            input = input.Replace("@_@", "");
            input = input.Replace(">.>", "");
            input = input.Replace("<.<", "");
            input = input.Replace("D:", "");
            input = input.Replace("`_`", "");
            input = input.Replace(">_>", "");
            input = input.Replace("<_<", "");
            input = input.Replace("X_x", "");
            input = input.Replace("x_x", "");
            input = input.Replace("x_X", "");
            return input;
        }
#endregion

#region replaceWithCustom_emoticons
        public String replaceWithCustom_emoticons(String input)
        {
            input = input.Replace(":P", "RANDOM_EMOTICON_HAPPY");
            input = input.Replace("=P", "RANDOM_EMOTICON_HAPPY");
            input = input.Replace(":)", "RANDOM_EMOTICON_HAPPY");
            input = input.Replace(":D", "RANDOM_EMOTICON_HAPPY");
            input = input.Replace("=D", "RANDOM_EMOTICON_HAPPY");
            input = input.Replace("XD", "RANDOM_EMOTICON_HAPPY");
            input = input.Replace("=P", "RANDOM_EMOTICON_HAPPY");
            input = input.Replace(":]", "RANDOM_EMOTICON_HAPPY");
            input = input.Replace(":O", "RANDOM_EMOTICON_HAPPY");
            input = input.Replace(">:D", "RANDOM_EMOTICON_HAPPY");
            input = input.Replace(">.>", "RANDOM_EMOTICON_HAPPY");
            input = input.Replace("<.<", "RANDOM_EMOTICON_HAPPY");
            input = input.Replace(":(", "RANDOM_EMOTICON_SAD");
            input = input.Replace(":<", "RANDOM_EMOTICON_SAD");
            input = input.Replace(":[", "RANDOM_EMOTICON_SAD");
            input = input.Replace(">:(", "RANDOM_EMOTICON_SAD");
            input = input.Replace(";-;", "RANDOM_EMOTICON_SAD");
            input = input.Replace("o.o", "RANDOM_EMOTICON_SAD");
            input = input.Replace("0_o", "RANDOM_EMOTICON_SAD");
            input = input.Replace("@_@", "RANDOM_EMOTICON_SAD");
            input = input.Replace("D:", "RANDOM_EMOTICON_SAD");
            input = input.Replace("`_`", "RANDOM_EMOTICON_HAPPY");
            input = input.Replace(">_>", "RANDOM_EMOTICON_HAPPY");
            input = input.Replace("<_<", "RANDOM_EMOTICON_HAPPY");
            input = input.Replace("X_x", "RANDOM_EMOTICON_SAD");
            input = input.Replace("x_x", "RANDOM_EMOTICON_SAD");
            input = input.Replace("x_X", "RANDOM_EMOTICON_SAD");
            return input;
        }
#endregion

#region replaceWithNull_HTML
        public String replaceWithNull_HTML(String input)
        {
            input = input.Replace("<b>", "");
            input = input.Replace("</b>", "");
            input = input.Replace("<i>", "");
            input = input.Replace("</i>", "");
            input = input.Replace("&gt;", "");
            input = input.Replace("&lt;", "");
            input = input.Replace("&amp;", "");
            input = input.Replace("&quot;", "");
            input = input.Replace("&nbsp;", "");
            input = input.Replace("<a href=", "");
            input = input.Replace("</a>", "");
            input = input.Replace("<img src=", "");
            input = input.Replace("</div>", "");
            input = input.Replace("<script>", "");
            input = input.Replace("</script>", "");
            input = input.Replace("<br>", "");
            input = input.Replace("<nobr>", "");
            input = input.Replace("<font face=", "");
            input = input.Replace("<font color=", "");
            input = input.Replace("<font size=", "");
            input = input.Replace("</font>", "");
            input = input.Replace("<span class=a", "");
            input = input.Replace("<td>", "");
            input = input.Replace("</td>", "");
            input = input.Replace("<tr>", "");
            input = input.Replace("</tr>", "");
            input = input.Replace("<table>", "");
            input = input.Replace("</table>", "");
            input = input.Replace("<h2>","");
            input = input.Replace("</h2>","");
            input = input.Replace("<table border=0", "");
            input = input.Replace("cellpadding=0", "");
            input = input.Replace("cellspacing=0", "");
            input = input.Replace(">", "");
            input = input.Replace("<", "");
            input = input.Replace("/td", "");
            input = input.Replace("/tr", "");
            input = input.Replace("/table", "");
            input = input.Replace("width=", "");
            input = input.Replace("div id=", "");
            input = input.Replace("resdiv", "");
            input = input.Replace("div class=", "");
            input = input.Replace("class=", "");
            
            return input;
        }
#endregion

#region replaceWithNull_Punctuation
        public String replaceWithNull_Punctuation(String input)
        {
            input = input.Replace("!", "");
            input = input.Replace("?", "");
            input = input.Replace(";", "");
            input = input.Replace(".", "");
            input = input.Replace(",", "");
            return input;
        }
#endregion

#region replaceWithNull_Punctuation_ForLBBrain
        public String replaceWithNull_Punctuation_ForLBBrain(String input)
        {

            input = input.Trim();

            string[] inputWords = input.Split(' ');
            int wordCount = inputWords.Length - 1;

            for (int i = 0; i <= wordCount; i++)
            {
                string currentWord = inputWords[i].ToString();

                if (currentWord.EndsWith("!"))
                    inputWords[i] = inputWords[i].Replace("!", "");

                if (currentWord.EndsWith("?"))
                    inputWords[i] = inputWords[i].Replace("?", "");

                if (currentWord.EndsWith(";"))
                    inputWords[i] = inputWords[i].Replace(";", "");

                if (currentWord.EndsWith("."))
                    inputWords[i] = inputWords[i].Replace(".", "");
            }
            
            input = "";
            for (int i = 0; i < inputWords.Length; i++)
            {
                input += inputWords[i] + " ";
            }

            input = input.Trim();

            return input;
        }
#endregion

#region replaceWithNull_ImageExt
        public String replaceWithNull_ImageExt(String input)
        {
            input = input.Replace(".jpg", "");
            input = input.Replace(".png", "");
            input = input.Replace(".gif", "");
            input = input.Replace(".exe", "");
            input = input.Replace(".bmp", "");

            return input;
        }
#endregion

#region replace_custom
        public String replace_custom(String input)
        {
            input = input.Replace("...", "");
            input = input.Replace("|", "");
            input = input.Replace("  ", " ");
            input = input.Replace("? micascalin  view mica scalins profile on linkedin squeeze the pan skulladay hello hilarious mefeedia", "Hi");
            input = input.Replace("hello was recorded in dictionaries in", "");
             input = input.Replace("???", "");
            input = input.Replace("array ", "");
            input = input.Replace("spacer", "");
            input = input.Replace("jesus", "UNNAMED_USER");
            input = input.Replace("Jesus", "UNNAMED_USER");
            input = input.Replace("god", "UNNAMED_USER");
            input = input.Replace("God", "UNNAMED_USER");
            input = input.Replace(" / ", " ");
            input = input.Replace("am est", "");  
            input = input.Replace("pm est", "");  
            input = input.Replace("am cst", "");
            input = input.Replace("pm cst", "");
            input = input.Replace("am mnt", "");
            input = input.Replace("pm mnt", "");
            input = input.Replace("am pac", "");
            input = input.Replace("pm pac", "");
            input = input.Replace("may refer to:", "");
            input = input.Replace("(song)", "");
            input = input.Replace(" lol ", "");
            input = input.Replace("()", "");
            input = input.Replace("other uncategorized", "");
            input = input.Replace("this site also houses", "");
            input = input.Replace("search?q=", "");
            input = input.Replace("? image. (", "");
            input = input.Replace(" + ", " ");
            return input;
        }
#endregion

#region replaceWithNull_webbased
        public String replaceWithNull_webbased(String input)
        {
            input = input.Replace("click here", "");
            input = input.Replace("click here.", "");
            input = input.Replace("to learn more about privacy and security in", "");
            input = input.Replace("will never sell or distribute your email address or account information.", "");
            input = input.Replace("will never sell or distribute your email address", "");
            input = input.Replace("posted at", "");
            input = input.Replace("posted on", "");
            input = input.Replace("posted by", "");
            input = input.Replace(".net", "");
            input = input.Replace(".com", "");
            input = input.Replace(".org", "");
            input = input.Replace(".html", "");
            input = input.Replace(".htm", "");
            input = input.Replace(".php", "");
            input = input.Replace("http://www.", "");
            input = input.Replace("http://", "");
            input = input.Replace("ftp://", "");
            input = input.Replace("www", "");
            input = input.Replace("subscribe in a reader", "");
            input = input.Replace("(1 year ago)", "");
            input = input.Replace(" 1 day ago by ", "");
            input = input.Replace("(1 min ago)", "");
            input = input.Replace("(0.15 seconds)", "");
            input = input.Replace("show hide var", "");
            input = input.Replace("show hide", "");
            input = input.Replace("( open in a new window )", "");
            input = input.Replace("open in a new window", "");
            input = input.Replace("other keywords for this site:", "");
            input = input.Replace(".ytmnd", "");
            input = input.Replace("[link]", "");
            input = input.Replace("[url]", "");
            input = input.Replace("at yahoo! movies", "");
            input = input.Replace("view my complete profile", "");
            input = input.Replace("go to the top of the page.", "");
            input = input.Replace("quote post", "");
            input = input.Replace("free step by step drawing tutorial", "");
            input = input.Replace("privacy cookies policy", "");
            input = input.Replace("help terms of use", "");
            input = input.Replace("about the bbc", "");
            input = input.Replace(".deviantart", "");
            input = input.Replace("(user does not allow im)", "");
            input = input.Replace("(user does not allow email)", "");
            input = input.Replace("user does not allow im", "");
            input = input.Replace("user does not allow email", "");
            input = input.Replace("topic posted", "");
            input = input.Replace("wikipedia, the free", "");
            input = input.Replace("you will be sent to this page automatically.", "");
            input = input.Replace("[get winamp]", "");
            input = input.Replace("[download help]", "");
            input = input.Replace("get winamp", "");
            input = input.Replace("download help", "");
            return input;
        }
#endregion

#region replace_swears
        public String replaceWithNull_swears(String input)
        {
            input = input.Replace("fucked", "screwed");
            input = input.Replace("fuck", "screw");
            input = input.Replace("FUCKED", "SCREWED");
            input = input.Replace("FUCK", "SCREW");
            input = input.Replace("pussy", "");
            input = input.Replace("shit", "poo");
            input = input.Replace("Shit", "poo");
            input = input.Replace("SHIT", "poo");
            input = input.Replace("cock", "");
            input = input.Replace("bitch", "");
            input = input.Replace("nigger", "");
            input = input.Replace("nigga", "someone");
            input = input.Replace("bastard", "");
            input = input.Replace("prick", "");
            input = input.Replace("asshole", "");
            input = input.Replace("damn", "");
            input = input.Replace("Damn", "");
            input = input.Replace("DAMN", "");
            return input;
        }
#endregion

#region randomEmoticon_happy
        public string randomEmoticon_happy()
        {
            ArrayList emoticons = new ArrayList();
            emoticons.Add(":P ");
            emoticons.Add("=P ");
            emoticons.Add(":) ");
            emoticons.Add(":D ");
            emoticons.Add("=D ");
            emoticons.Add("XD ");
            emoticons.Add("=D ");
            emoticons.Add(":] ");
            emoticons.Add(">:D ");
            emoticons.Add(">.> ");
            emoticons.Add("<.< ");
            emoticons.Add(">_> ");
            emoticons.Add("<_< ");

            Thread.Sleep(50);
            int rnd = randnum.Next(emoticons.Count);
            string randomEmoticon = (string)emoticons[rnd];

            return randomEmoticon;
        }
#endregion

#region randomEmoticon_sad
        public string randomEmoticon_sad()
        {
            ArrayList emoticons = new ArrayList();
            emoticons.Add(":( ");
            emoticons.Add(":< ");
            emoticons.Add(":[ ");
            emoticons.Add(">:( ");
            emoticons.Add(";-; ");
            emoticons.Add("o.o ");
            emoticons.Add("0_o ");
            emoticons.Add("@_@ ");
            emoticons.Add(">:( ");
            emoticons.Add("D:");

            Thread.Sleep(50);
            int rnd = randnum.Next(emoticons.Count);
            string randomEmoticon = (string)emoticons[rnd];

            return randomEmoticon;

        }
#endregion

#region randomPunctuation
        public string randomPunctuation()
        {
            ArrayList punctuation = new ArrayList();
            punctuation.Add(".");
            punctuation.Add("!");
            punctuation.Add(""); // Sometimes you don't want to add punctuation at all.

            Thread.Sleep(50);
            int rnd = randnum.Next(punctuation.Count);
            string randomPunctuation = (string)punctuation[rnd];

            return randomPunctuation;
        }
#endregion

#region replaceKonWithUNNAMED_USER(conversation)
        public string replaceKonWithUNNAMED_USER(string conversation)
        {

            conversation = conversation.Trim();

            string[] conversationWords = conversation.Split(' ');
            int wordCount = conversationWords.Length - 1;

            for (int i = 0; i <= wordCount; i++)
            {
                string currentWord = conversationWords[i].ToString();
                string currentWordFiltered = replaceWithNull_Punctuation(currentWord);

                if (currentWordFiltered.ToUpper() == "KON:")
                {
                    conversationWords[i] = conversationWords[i].Replace("Kon", "UNNAMED_USER");
                    conversationWords[i] = conversationWords[i].Replace("KON", "UNNAMED_USER");
                    conversationWords[i] = conversationWords[i].Replace("kon", "UNNAMED_USER");
                    conversationWords[i] = conversationWords[i].Replace("kOn", "UNNAMED_USER");
                    conversationWords[i] = conversationWords[i].Replace("koN", "UNNAMED_USER");
                    conversationWords[i] = conversationWords[i].Replace("KoN", "UNNAMED_USER");
                }

                else if (currentWordFiltered.ToUpper() == "KON")
                {
                    conversationWords[i] = conversationWords[i].Replace("Kon", "UNNAMED_USER");
                    conversationWords[i] = conversationWords[i].Replace("KON", "UNNAMED_USER");
                    conversationWords[i] = conversationWords[i].Replace("kon", "UNNAMED_USER");
                    conversationWords[i] = conversationWords[i].Replace("kOn", "UNNAMED_USER");
                    conversationWords[i] = conversationWords[i].Replace("koN", "UNNAMED_USER");
                    conversationWords[i] = conversationWords[i].Replace("KoN", "UNNAMED_USER");
                }

                else if (currentWordFiltered.ToUpper() == IRC.nickNameMain.ToUpper() + ":")
                    conversationWords[i] = conversationWords[i].Replace(IRC.nickNameMain, "UNNAMED_USER:");

                else if (currentWordFiltered.ToUpper() == IRC.nickNameMain.ToUpper())
                    conversationWords[i] = conversationWords[i].Replace(IRC.nickNameMain, "UNNAMED_USER");

            }

            conversation = "";
            for (int i = 0; i < conversationWords.Length; i++)
            {
                conversation += conversationWords[i] + " ";
            }

            conversation = conversation.Trim();

            return conversation;
        }

#endregion

#region replaceHTMLWithNull(conversation)
        public string replaceHTMLWithNull(string conversation)
        {

            conversation = conversation.Trim();

            string[] conversationWords = conversation.Split(' ');
            int wordCount = conversationWords.Length - 1;

            for (int i = 0; i <= wordCount; i++)
            {
                string currentWord = conversationWords[i].ToString();
                string currentWordFiltered = replaceWithNull_Punctuation(currentWord);

                if ((currentWordFiltered.ToUpper().StartsWith("WWW")) || currentWordFiltered.ToUpper().StartsWith("(WWW"))
                    conversationWords[i] = conversationWords[i].Replace(currentWord, "");

                if ((currentWordFiltered.ToUpper().StartsWith("HTTP://")) || currentWordFiltered.ToUpper().StartsWith("(HTTP://"))
                    conversationWords[i] = conversationWords[i].Replace(currentWord, "");

                if ((currentWordFiltered.ToUpper().StartsWith("FTP://")) || currentWordFiltered.ToUpper().StartsWith("(FTP://"))
                    conversationWords[i] = conversationWords[i].Replace(currentWord, "");

                if ((currentWordFiltered.ToUpper().StartsWith("HTTPS://")) || currentWordFiltered.ToUpper().StartsWith("(HTTPS://"))
                    conversationWords[i] = conversationWords[i].Replace(currentWord, "");

                if ((currentWordFiltered.ToUpper().EndsWith(".COM")) || currentWordFiltered.ToUpper().EndsWith(".COM)"))
                    conversationWords[i] = conversationWords[i].Replace(currentWord, "");

                if ((currentWordFiltered.ToUpper().EndsWith(".NET")) || currentWordFiltered.ToUpper().EndsWith(".NET)"))
                    conversationWords[i] = conversationWords[i].Replace(currentWord, "");

                if ((currentWordFiltered.ToUpper().EndsWith(".ORG")) || currentWordFiltered.ToUpper().EndsWith(".ORG)"))
                    conversationWords[i] = conversationWords[i].Replace(currentWord, "");
            }

            conversation = "";
            for (int i = 0; i < conversationWords.Length; i++)
            {
                conversation += conversationWords[i] + " ";
            }

            conversation = conversation.Trim();

            return conversation;
        }

#endregion

    }
}
