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
// This file was last updated on: 7/06/2011   //
////////////////////////////////////////////////

using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Threading;

namespace Kon
{
    class AI
    {

#region variables

        public static       StreamWriter    brain;
        private       const string          BRAIN_FILE         = "kon.brain";
        private             string          lastLine           = "";
        private             Random          randnum            = new Random();
#endregion

        public AI()
        {
        }

        public void addToBrain(string conversation, string nick)
        {
            // Do a simple filter check to see if it's something we want to add to the basic brain.

            // Who needs actions?
            if (conversation.StartsWith("ACTION"))
                return;

            bool canAddToBrain = Filter.canAddToBrain(conversation, nick);
            
            if (canAddToBrain == false)
                return;
            
            // Time to filter out some stuff.
            Filter filter = new Filter();

            // This method will call upon a bit of lengthy code to replace all instances of "kon" with "UNNAMED_USER"
            conversation = filter.replaceKonWithUNNAMED_USER(conversation);

            // This method will call upon a bit of lengthy code to replace words in the conversation that are urls
            conversation = filter.replaceHTMLWithNull(conversation);

            // filter emoticons
            conversation = filter.replaceWithCustom_emoticons(conversation);

            // filter image extensions.
            conversation = filter.replaceWithNull_ImageExt(conversation);

            // filter out some things that I personally don't want in it.
            conversation = filter.replace_custom(conversation);

            // filter out the HTML and web-based stuff
            conversation = filter.replaceWithNull_HTML(conversation);
            conversation = filter.replaceWithNull_webbased(conversation);

            // Finally, we don't want the bot to have a bad potty mouth.
            conversation = filter.replaceWithNull_swears(conversation);

            // Tidy up some stuff.
            conversation = conversation.Trim();

            // Well, that's good enough for now.  Let's make sure it's not null and add it to the brain!
            if (conversation != "")
            {
                brain = new StreamWriter(BRAIN_FILE, true);
                brain.WriteLine(conversation);
                brain.Flush();
                brain.Close();
            }
        }

        public String pullFromBrain()
        {
          
            // Does the bot have a brain?  If not, we can't proceede without crashing.
            if (!File.Exists(BRAIN_FILE))
            {
                Console.WriteLine(BRAIN_FILE + " does not exist.");
                return "I need a brain to work!";
            }        
            
            // Okay, it does.  Continue.
            String randomLine;
            randomLine = getLineFromBrain();
            lastLine = randomLine;
            Filter filter = new Filter();
            
            // If the line is really, really short.. let's tack on another line from his brain to it to make it longer.
            if (randomLine.Length <= 5)
            {
                string newLine = getLineFromBrain();
                while (newLine == lastLine) 
                {
                    newLine = getLineFromBrain();
                }
                randomLine = randomLine + " " + newLine;
            }
            
            // Let's see if the line is too short (at the moment 18 characters) and add some stuff.
            if (((randomLine.Length > 5) && (randomLine.Length <= 18)))
            {

                Thread.Sleep(50);
                int rndC = randnum.Next(6);

                if (rndC == 1)
                    randomLine = randomLine + " " + filter.randomEmoticon_happy();

                if (rndC == 2)
                    randomLine = randomLine + " " + filter.randomEmoticon_sad();

                if (rndC == 3)
                    randomLine = filter.randomEmoticon_sad() + " " + randomLine;

                if (rndC == 4)
                    randomLine = filter.randomEmoticon_happy() + " " + randomLine;

                if (rndC == 5)
                    randomLine = randomLine.ToUpper();

                if (rndC == 6)
                    randomLine = randomLine.Trim() + randomPunctuation();
            }
            
            if (((randomLine.Length > 18) && (randomLine.Length < 60)))
            {
                Thread.Sleep(50);
                int rndC = randnum.Next(3);

                if (rndC == 1)
                    randomLine = randomLine.Trim() + randomPunctuation();

                if (rndC == 2)
                    randomLine = randomLine;

                if (rndC == 3)
                    randomLine = randomLine + "splash!";
            }

            // Let's do some replacing before we send it back out
            randomLine = randomLine.Replace("RANDOM_EMOTICON_HAPPY", filter.randomEmoticon_happy());
            randomLine = randomLine.Replace("RANDOM_EMOTICON_SAD", filter.randomEmoticon_sad());

            return randomLine;
        }

        public string getLineFromBrain()
        {
            using (StreamReader brainFile = File.OpenText(BRAIN_FILE))
            {
                // This will add all of the lines in the brain file to an array so we can use them
                ArrayList lines = new ArrayList();
                string line;
                int numberOfLines;

                while ((line = brainFile.ReadLine()) != null)
                    lines.Add(line);

                numberOfLines = lines.Count;

                // Now we need to randomly pick a line and pull a line.

                Thread.Sleep(50);
                int rnd = randnum.Next(numberOfLines);
                string randomLine = (string)lines[rnd];

                brainFile.Close();

                return randomLine;
            }
        }

        public int getBrainLength()
        {
            using (StreamReader brainFile = File.OpenText(BRAIN_FILE))
            {
                // This will add all of the lines in the brain file to an array so we can count them
                ArrayList lines = new ArrayList();
                string line;

                while ((line = brainFile.ReadLine()) != null)
                    lines.Add(line);

                brainFile.Close();

                return lines.Count;
            }
        }

        private string randomPunctuation()
        {
            ArrayList punctuation = new ArrayList();
            punctuation.Add(".");
            punctuation.Add("?");
            punctuation.Add("!");
            punctuation.Add("...");
            punctuation.Add("!?");
            punctuation.Add("??");

            Thread.Sleep(50);
            int rnd = randnum.Next(punctuation.Count);
            string randompunctuation = (string)punctuation[rnd];

            return randompunctuation;
        }

    }
}
