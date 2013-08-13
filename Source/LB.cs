////////////////////////////////////////////////
////////////////PROGRAM INFO////////////////////
// "Kon" is a "learning bot" written in C#.   //
// The bot will connect to an IRC server and  //
// "watch" the conversations.  It will then   //
// "learn" from them.  You'll then be able to //
// talk to it and hopefully it'll hold a conv-//
// ersation.                                  //
////////////////////////////////////////////////
// The "LB Brain" was based on a chatterbot by//
// BishounenNightBird of Esper.net called "LB"//
// ("Language Bot").  While this brain does   //
// not follow LB exactly, he still inspired me//
// and I want to thank him.                   //
////////////////////////////////////////////////
// "Kon" is an ongoing project started by     // 
// James Iyouboushi.                          //
// Emails: jmp1139@my.gulfcoast.edu           //
//         Iyouboushi@gmail.com               //
////////////////////////////////////////////////
// This file was last updated on: 8/13/2013  //
////////////////////////////////////////////////

using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;

namespace Kon
{
    class LBbrain
    {

#region variables
        public  static StreamWriter brain;
        public         bool         brainInUse               = false;
        public  static Filter       filter;
        private        string       BRAIN_FILE               = "lb.brain";
        private const  string       BRAIN_FILE_QUESTIONS     = "lb-question.brain";  // defunct, but leaving for now.
        private const  string       BRAIN_FILE_STATEMENTS    = "lb-statements.brain"; // defunct, but leaving for now.
        private const  string       KON_BRAIN                = "kon.brain";
        private        Random       randnum                  = new Random();
        private        string       lastLine                 = "";
        private        string       previousSearch           = "";
        private        bool         shortDepth               = false;
        private        bool         oldStyleTopicSearch      = false;
        private        string[]     commonWords              = { "THERE", "THAT", "THIS", "HERE", "YOUR", "THEY", "IT'S", "OTHER", "HAVE", "STILL", "VERY", "THEIR", "ITS", "WHY", "HOW", "WHO" };
        private        string[]     questionWords            = { "WHO", "WHAT", "WHERE ARE ", "WHERE IS ", "WHERE DO ", "WHEN ARE", "WHEN IS", "WHY", "HOW", "ARE", "CAN", "SHOULD", "WHICH", "WILL ", "WHOSE", "WHO'S", "ISN'T", "IS ", "DO ", "Y U NO ", "WONDER WHAT", "SO, WHY", "SO WHY", "DID THEY" };
        private const String        regExPattern             = @".*?(?<kon>.*?)(?<character>.*?).*?";
        private       String        keyword1;
        private       String        keyword2;
        private       String        conversationOriginal     = "";

#endregion

        public LBbrain()
        {
            // Let's start filtering out some stuff.
            filter = new Filter();
        }

#region Add To Brain
        public void addToBrain(string conversation, string nick)
        {

            bool canAddToBrain = Filter.canAddToBrain(conversation, nick);

            if (canAddToBrain == false)
                return;

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

                // One last thing we have to filter out.
                conversation = filter.replaceWithNull_Punctuation_ForLBBrain(conversation);

               // Let's get a list of all the starting sentences.. will be used later to filter out repeat sentence starters.
                string line;
                ArrayList startingLines = new ArrayList();


                using (StreamReader brainFile = File.OpenText(BRAIN_FILE))
                {
                    while ((line = brainFile.ReadLine()) != null)
                    {
                        if (line.StartsWith("START_SENTENCE"))
                            startingLines.Add(line.ToUpper());
                    }
                    brainFile.Close();
                }

                // Let's open the brain file and get ready to add stuff.
                brain = new StreamWriter(BRAIN_FILE, true);

                string[] messageArray;
                messageArray = conversation.Split(new char[] { ' ' });
                int lastWordPos = (messageArray.Length - 1);

                // Now we're going to write the brain file.  In 1.0.7 and 1.0.8 the LB Brain used a "short depth" structure
                // but I feel that this just isn't sufficient in larger brain files.  Thus I've now allowed a
                // long depth.  If you'd like to use the old shorter depth set the bool shortDepth to true.  
                // Otherwise, it'll try to use the longer depth when it can.
                if (shortDepth)
                {
                    int j = 1;

                    for (int i = 0; i < messageArray.Length; i++)
                    {
                        if (i == 0)
                        {
                            string startingLine = "";
                            if (lastWordPos == 0)
                                startingLine = "START_SENTENCE " + messageArray[i];

                            if (lastWordPos > 0)
                                startingLine = "START_SENTENCE " + messageArray[i] + " " + messageArray[i + 1];

                            if (!startingLines.Contains(startingLine.ToUpper()))
                                brain.WriteLine(startingLine);

                            brain.Flush();
                        }

                        else
                        {
                            if (i == lastWordPos)
                                brain.WriteLine(messageArray[i - 1] + " " + messageArray[i] + " END_SENTENCE");

                            else
                            {
                                brain.WriteLine(messageArray[i - j] + " " + messageArray[i] + " " + messageArray[i + 1]);
                            }

                            brain.Flush();
                        }
                    }

                    j++;
                }

                else
                {
                    int j = 1;

                    for (int i = 0; i < messageArray.Length; i++)
                    {
                        if (i == 0)
                        {
                            string startingLine = "";

                            if (lastWordPos == 0)
                                startingLine = "START_SENTENCE " + messageArray[i];

                            if ((lastWordPos > 0) && (lastWordPos < 2))
                                startingLine = "START_SENTENCE " + messageArray[i] + " " + messageArray[i + 1];

                            if (lastWordPos >= 2)
                                startingLine = "START_SENTENCE " + messageArray[i] + " " + messageArray[i + 1] + " " + messageArray[i + 2];

                            if (!startingLines.Contains(startingLine.ToUpper()))
                                brain.WriteLine(startingLine);

                            brain.Flush();
                        }

                        else
                        {
                            if (i == lastWordPos)
                                brain.WriteLine(messageArray[i - 1] + " " + messageArray[i] + " END_SENTENCE");
 
                            else
                            {
                                if ((i + 2) <= lastWordPos)
                                    brain.WriteLine(messageArray[i - j] + " " + messageArray[i] + " " + messageArray[i + 1] + " " + messageArray[i + 2]);
                                else
                                    brain.WriteLine(messageArray[i - j] + " " + messageArray[i] + " " + messageArray[i + 1]);
                            
                            }

                            brain.Flush();
                        }
                    }

                    j++;
                }

                
            }
            
            brain.Close();
        }
#endregion


#region Pull From Brain
        public String pullFromBrain(string conversation, bool topic)
        {
            // Does the bot have a brain?  If not, we can't proceede without crashing.
            if (!File.Exists(BRAIN_FILE))
            {
                Console.WriteLine(BRAIN_FILE + " does not exist.");
                return "I need to see some conversations before I can reply using this brain!";
            }

            // Okay, it does.  Continue.
            brainInUse = true;
            conversation = conversation.Trim();
            conversationOriginal = conversation.Trim();
            String randomLine = "";
            String convoStart;
            keyword1 = keyword2 = "";

            
            // Are we using the "Answer Search" option to try and create more accurate answers?
            if (IRC.answerSearch)
                conversation = doAnswerSearch(conversation);

            if (topic)
                convoStart = getTopicFromConvo(conversation);

            else
                convoStart = "";


            // If a topic was chosen we need to build a sentence AROUND the topic.  We shouldn't start the sentence
            // with the topic.  Although that'll work, it isn't always coherent.  So let's try to find a word or
            // two before it.

            if (convoStart != "")
                randomLine = getTopicStarter(convoStart);

            
            // Let's grab the start of our sentence now.
            randomLine = getStartingSentence(randomLine);

            string searchWord = "";
            string originalLine = randomLine;
            int count = 0;


            // While the searchWord is not "START_SENTENCE" let's continue to build the sentence.
            do
            {
                searchWord = getFirstWord(randomLine);
                if (searchWord != "")
                {
                    randomLine = getPreviousLine(searchWord);
                    originalLine = randomLine + originalLine;
                    count++;
                }

                if (searchWord == "")
                    searchWord = "START_SENTENCE";

                ///Console.WriteLine("current line: " + randomLine);
            } while ((!searchWord.StartsWith("START_SENTENCE") && (count < 300)));

            randomLine = originalLine;

            searchWord = "";

            // While the searchWord is not "END_SENTENCE" let's continue to build the sentence.
            do
            {
                searchWord = getLastWord(randomLine);
                if (searchWord != "")
                    randomLine += getLine(searchWord);

                ///Console.WriteLine("current line: " + randomLine);
            } while (searchWord != "END_SENTENCE END_SENTENCE");

            // Last step: clean up.
            // remove "START_SENTENCE" and "END_SENTENCE", trim up the space and replace the emoticons.
            randomLine = randomLine.Replace("START_SENTENCE", "");
            randomLine = randomLine.Replace("END_SENTENCE", "");
            randomLine = randomLine.Replace("  ", "");
            randomLine = randomLine.Trim();

            // Let's remove some random things.
            if (randomLine.EndsWith("\"") && (!randomLine.StartsWith("\"")))
                randomLine = randomLine.Replace("\"", "");
            if (randomLine.StartsWith("\"") && (!randomLine.EndsWith("\"")))
                randomLine = randomLine.Replace("\"", "");
            if (randomLine.EndsWith("") && (!randomLine.StartsWith("ACTION")))
                randomLine = randomLine.Replace("", "");
            if ((randomLine.Contains("(")) && (!randomLine.Contains(")")))
                randomLine = randomLine + ")";
            if ((randomLine.Contains(")")) && (!randomLine.Contains("(")))
                randomLine = randomLine.Replace(")", "");

            // If the reply starts with a question word, let's add a question mark.
            int strNumber;
            for (strNumber = 0; strNumber < questionWords.Length; strNumber++)
            {
                if (randomLine.ToUpper().StartsWith(questionWords[strNumber].ToString()))
                {
                    randomLine = randomLine + "?";
                    break;
                }
            }

            // Let's try to add some punctuation if it needs one.
            string lastReplyCharacter = randomLine.Substring(randomLine.Length - 1, 1);
            
            // check to see if we need to add another one.
            if ((((((lastReplyCharacter != ".") && (lastReplyCharacter != ",") && ((lastReplyCharacter != "?") && ((lastReplyCharacter != "!") && ((lastReplyCharacter != ":") && (lastReplyCharacter != ";")))))))))
            {
                if ((!randomLine.EndsWith("RANDOM_EMOTICON_HAPPY") && (!randomLine.EndsWith("RANDOM_EMOTICON_SAD"))))
                    randomLine = randomLine + filter.randomPunctuation();
            }

            // Replace an emoticon with a random one.
            randomLine = randomLine.Replace("RANDOM_EMOTICON_HAPPY", filter.randomEmoticon_happy());
            randomLine = randomLine.Replace("RANDOM_EMOTICON_SAD", filter.randomEmoticon_sad());

            // Finally, we have our response.  Let's return it.
            brainInUse = false;
            return randomLine;
        }
#endregion


#region getTopicFromConvo
        private String getTopicFromConvo(string conversation)
        {
            string topic = "";
            string[] conversationBroken;
            ArrayList topics = new ArrayList();
            conversationBroken = conversation.Split(new char[] { ' ' });
            int numberOfWords = conversationBroken.Length - 1;

            if (oldStyleTopicSearch)
            {
                // NOTE: This function is not gramatically correct.  It doesn't find the REAL topic of the sentence.
                // Instead, it tries to throw away the word "there" and very small words, leaving behind less common words.

                for (int i = 0; i <= numberOfWords; i++)
                {
                    if (conversationBroken[i].ToString().Length > 4)
                    {
                        if (conversationBroken[i].ToString() != "there")
                            topics.Add(conversationBroken[i].ToString());
                    }
                }




            }
            else
            {
                // With the new code, it will attempt to filter out common words and pick a random topic.
                
                string currentWord;
                int strNumber;
                int strIndex = 0;

                for (int i = 0; i <= numberOfWords; i++)
                {
                    currentWord = conversationBroken[i].ToString();

                    for (strNumber = 0; strNumber < commonWords.Length; strNumber++)
                    {
                        strIndex = commonWords[strNumber].IndexOf(currentWord.ToUpper());

                        // If it finds the word in the array, we need to break and try the next word.
                        if (strIndex != -1)
                            break;

                        // Else, we need to try to add it to the list of topics..
                        else
                        {
                            if (currentWord.Length > 4)
                            {
                                topics.Add(currentWord);
                                break;
                            }

                        }
                    }
   
                }
            }

            // If the ArrayList isn't null let's pick a topic.
            if (topics.Count > 0)
            {
                if (topics.Count < 2)
                {
                    Thread.Sleep(50);
                    int rnd = randnum.Next(topics.Count);
                    keyword1 = (string)topics[rnd];
                }

                else
                {
                    int rnd; 
                    int attempts = 0;

                    // Let's grab keyword #1
                    Thread.Sleep(50);
                    rnd = randnum.Next(topics.Count);
                    keyword1 = (string)topics[rnd];
                    Thread.Sleep(50);
                  

                    // Now let's grab keyword #2
                    do  {
                        rnd = randnum.Next(topics.Count);
                        keyword2 = (string)topics[rnd];
                        attempts++;
                    } while (((keyword2.ToUpper() == keyword1.ToUpper()) && (attempts < 5)));

                }

            }

            topic = keyword1;

            if (keyword2 != "")
                topic = keyword1 + " " + keyword2;

            // Then strip the "topic" of all punctuation, just in case.
            topic = topic.Replace(".", "");
            topic = topic.Replace(",", "");
            topic = topic.Replace("!", "");
            topic = topic.Replace("?", "");

            return topic;
        }
#endregion


#region getStartingSentence()
        private String getStartingSentence(string convoStart)
        {
            ArrayList startingLines = new ArrayList();
            string line = "";
            int numberOfLines;
            
            if (convoStart != "")
            {
                // If a topic has been found then we need to build a sentence around that topic.
                using (StreamReader brainFile = File.OpenText(BRAIN_FILE))
                {
                    while ((line = brainFile.ReadLine()) != null)
                    {
                        string upperLine = line.ToUpper();

                        if (upperLine.StartsWith(convoStart.ToUpper()))
                            startingLines.Add(line);
                    }
                    brainFile.Close();
                }
                
            }
          
            // If no starting sentence was found that relates to the topic we need to pick a random one.

            if (startingLines.Count == 0)
            {
                using (StreamReader brainFile = File.OpenText(BRAIN_FILE))
                {
                    while ((line = brainFile.ReadLine()) != null)
                    {
                        if (line.StartsWith("START_SENTENCE"))
                            startingLines.Add(line);
                    }
                    brainFile.Close();
                }
            }
            
            
            numberOfLines = startingLines.Count;
            
            // Now we need to randomly pick a line and pull a line.
            Thread.Sleep(50);
            int rnd = randnum.Next(numberOfLines);
            string randomStart = (string)startingLines[rnd];
            
            
            return randomStart;
        }

#endregion

#region getLastWord(randomLine)
        private String getLastWord(string randomLine)
        {
            string[] messageArray;
            messageArray = randomLine.Split(new char[] { ' ' });
            int lastWord = (messageArray.Length - 1);

            if (lastWord >= 1)
                return messageArray[lastWord - 1] + " " + messageArray[lastWord];
            else
                return messageArray[lastWord];
        }
#endregion

#region getFirstWord(randomLine)
        private String getFirstWord(string randomLine)
        {
            string[] messageArray;
            string firstWord;
            messageArray = randomLine.Split(new char[] { ' ' });
            int length = messageArray.Length - 1;

            if (length <= 1)
                firstWord = messageArray[0];
            else
                firstWord = messageArray[0] + " " + messageArray[1];

            return firstWord;
        }
#endregion

#region getTopicStarter
        /*
          This method is pretty messy at the moment.  I'm sure there's a way to clean it up
          but at the moment it's just going to have to say like this.
         */
        private String getTopicStarter(string currentLine)
        {
            if (currentLine != "")
            {
                ArrayList startingLines = new ArrayList();
                string line = "";
                int numberOfLines;

                using (StreamReader brainFile = File.OpenText(BRAIN_FILE))
                {
                    string lineWithoutPunct = "";

                    while ((line = brainFile.ReadLine()) != null)
                    {
                         lineWithoutPunct = filter.replaceWithNull_Punctuation(line);
                         keyword1 = filter.replaceWithNull_Punctuation(keyword1);

                         if (keyword2 != "")
                         {
                             keyword2 = filter.replaceWithNull_Punctuation(keyword2);
                             if (lineWithoutPunct.ToUpper().Contains(keyword1.ToUpper()) && lineWithoutPunct.ToUpper().Contains(keyword2.ToUpper()))
                                 startingLines.Add(line);
                         }
                       
                         else
                         {

                             if (lineWithoutPunct.ToUpper().Contains(keyword1.ToUpper()))
                                 startingLines.Add(line);
                         }
                    }
                    brainFile.Close();
                }


                numberOfLines = startingLines.Count;

                // If we find no lines, then let's attempt to find a line that STARTS with it..
                if ((numberOfLines == 0) && (keyword2 == ""))
                    currentLine = getStartingSentence(currentLine);

                if ((numberOfLines == 0) && (keyword2 != ""))
                {
                    using (StreamReader brainFile = File.OpenText(BRAIN_FILE))
                    {
                        string lineWithoutPunct = "";

                        while ((line = brainFile.ReadLine()) != null)
                        {
                            lineWithoutPunct = filter.replaceWithNull_Punctuation(line);
                            if (lineWithoutPunct.ToUpper().Contains(keyword1.ToUpper()))
                                startingLines.Add(line);
                        }

                        brainFile.Close();
                    }
                }
                numberOfLines = startingLines.Count;


                if (numberOfLines == 0)
                    currentLine = getStartingSentence(currentLine);


                if (numberOfLines > 0)
                {
                    // Now we need to randomly pick a line and pull a line.
                    Thread.Sleep(50);
                    int rnd = randnum.Next(numberOfLines);
                    currentLine = (string)startingLines[rnd];
                }
            }

            return currentLine;
        }

#endregion

#region getLine(string searchWord)
        private String getLine(string searchWord)
        {
            using (StreamReader brainFile = File.OpenText(BRAIN_FILE))
            {
                // This will add all of the lines in the brain file to an array so we can use them
                ArrayList lines = new ArrayList();
                string line;
                int numberOfLines;
                searchWord = searchWord.Replace("START_SENTENCE", "");
                searchWord = searchWord.Trim();

                while ((line = brainFile.ReadLine()) != null)
                {
                    if (line.StartsWith(searchWord))
                        lines.Add(line);
                }

                numberOfLines = lines.Count;

                if (numberOfLines == 0)
                    return " END_SENTENCE";

                if (previousSearch == searchWord)
                    return " END_SENTENCE";


                // Now we need to randomly pick a line and pull a line.
                Thread.Sleep(50);
                int rnd = randnum.Next(numberOfLines);
                string randomLine = (string)lines[rnd];
                lastLine = randomLine;

                // If I don't do this it'll repeat the word twice in the sentence.
                randomLine = randomLine.Replace(searchWord, "");
                previousSearch = searchWord;

                brainFile.Close();
                return randomLine;
            }
        }
#endregion

#region getPreviousLine(string searchWord)
        private String getPreviousLine(string searchWord)
        {
            using (StreamReader brainFile = File.OpenText(BRAIN_FILE))
            {
                // This will add all of the lines in the brain file to an array so we can use them
                ArrayList lines = new ArrayList();
                string line;
                int numberOfLines;
                searchWord = searchWord.Replace("END_SENTENCE", "");
                searchWord = searchWord.Trim();

                while ((line = brainFile.ReadLine()) != null)
                {
                    if (line.EndsWith(searchWord))
                        lines.Add(line);
                }

                numberOfLines = lines.Count;

                if (numberOfLines == 0)
                    return " START_SENTENCE";

                if (previousSearch == searchWord)
                    return " START_SENTENCE";


                // Now we need to randomly pick a line and pull a line.
                Thread.Sleep(50);
                int rnd = randnum.Next(numberOfLines);
                string randomLine = (string)lines[rnd];
                lastLine = randomLine;

                // If I don't do this it'll repeat the word twice in the sentence.
                randomLine = randomLine.Replace(searchWord, "");
                previousSearch = searchWord;

                brainFile.Close();
                return randomLine;
            }
        }
#endregion


#region getBrainLength()
        public int getBrainLength()
        {
            int totalCount = 0;
            using (StreamReader brainFile = File.OpenText(BRAIN_FILE))
            {
                // This will add all of the lines in the brain file to an array so we can count them
                ArrayList lines = new ArrayList();
                string line;

                while ((line = brainFile.ReadLine()) != null)
                    lines.Add(line);

                brainFile.Close();

                totalCount = lines.Count;
            }

            return totalCount;
        }
#endregion


#region DoAnswerSearch
        string doAnswerSearch(string originalConversation)
        {
            string tempConvo = originalConversation;


            using (StreamReader brainFile = File.OpenText(KON_BRAIN))
            {
                // This will add all of the lines in the brain file to an array so we can use them
                string searchLine = "";
                bool foundLine = false;

                ArrayList AnswerLines = new ArrayList();
                 int numberOfAnswerLines;


                // Let's start filtering out some stuff.

                // This method will call upon a bit of lengthy code to replace all instances of "kon" with "UNNAMED_USER"
                tempConvo = filter.replaceKonWithUNNAMED_USER(tempConvo);

                // This method will call upon a bit of lengthy code to replace words in the conversation that are urls
                tempConvo = filter.replaceHTMLWithNull(tempConvo);

                // filter emoticons
                tempConvo = filter.replaceWithCustom_emoticons(tempConvo);

                // filter image extensions.
                tempConvo = filter.replaceWithNull_ImageExt(tempConvo);

                // filter punctuation
                tempConvo = filter.replaceWithNull_Punctuation(tempConvo);

                // filter out some things that I personally don't want in it.
                tempConvo = filter.replace_custom(tempConvo);

                // filter out the HTML and web-based stuff
                tempConvo = filter.replaceWithNull_HTML(tempConvo);
                tempConvo = filter.replaceWithNull_webbased(tempConvo);

                // Finally, we don't want the bot to have a bad potty mouth.
                tempConvo = filter.replaceWithNull_swears(tempConvo);

                // Tidy up some stuff.
                tempConvo = tempConvo.Trim();

                if (tempConvo == "")
                    return originalConversation;

                while ((searchLine = brainFile.ReadLine()) != null)
                {
                    searchLine = searchLine.ToUpper();
                    searchLine = filter.replaceWithNull_Punctuation(searchLine);

                    if (searchLine.Contains(tempConvo.ToUpper()))
                    {
                        // Change temp convo to the next line
                        try
                        {

                            searchLine = brainFile.ReadLine();
                            AnswerLines.Add(searchLine);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            break;
                        }
                    }
                }
                brainFile.Close();


                numberOfAnswerLines = AnswerLines.Count;

                if (numberOfAnswerLines > 0)
                    foundLine = true;
                else
                    foundLine = false;

                // Get a random line here.
                if (foundLine)
                {

                    Thread.Sleep(50);
                    int rnd = randnum.Next(numberOfAnswerLines);
                    string randomLine = (string)AnswerLines[rnd];
                    searchLine = randomLine;
                }
                
                if (searchLine == null)
                    searchLine = originalConversation;

                if (!foundLine)
                    tempConvo = originalConversation;
                else
                    tempConvo = searchLine;

                return tempConvo;
            }
        }

#endregion

#region TotalNonsense()
        public static string totalNonsense()
        {
            string randomLine = "";
            int count = 0;
            Random randnum = new Random();
            ArrayList lines = new ArrayList();
            string line;
            int numberOfLines;

            using (StreamReader brainFile = File.OpenText("lb.brain"))
            {
                // This will add all of the lines in the brain file to an array so we can use them

                while ((line = brainFile.ReadLine()) != null)
                {
                        lines.Add(line);
                }
                brainFile.Close();
            }

            numberOfLines = lines.Count;

            if (numberOfLines == 0)
                return "No brain file found :<";

            do
            {

                // Now we need to randomly pick a line and pull a line.
                Thread.Sleep(50);
                int rnd = randnum.Next(numberOfLines);
                string rndLine = (string)lines[rnd];
                randomLine = randomLine + " " + rndLine;
                count++;

            } while (count < 20);
           
            // Last step: clean up.
            // remove "START_SENTENCE" and "END_SENTENCE", trim up the space and replace the emoticons.
            randomLine = randomLine.Replace("START_SENTENCE", " ");
            randomLine = randomLine.Replace("END_SENTENCE", " ");
            randomLine = randomLine.Replace("  ", "");
            randomLine = randomLine.Trim();

            // Let's start cleaning up this mess of a line.

            // Let's remove some random things.
            if (randomLine.EndsWith("\"") && (!randomLine.StartsWith("\"")))
                randomLine = randomLine.Replace("\"", "");
            if (randomLine.StartsWith("\"") && (!randomLine.EndsWith("\"")))
                randomLine = randomLine.Replace("\"", "");
            if (randomLine.EndsWith("") && (!randomLine.StartsWith("ACTION")))
                randomLine = randomLine.Replace("", "");
            if ((randomLine.Contains("(")) && (!randomLine.Contains(")")))
                randomLine = randomLine + ")";
            if ((randomLine.Contains(")")) && (!randomLine.Contains("(")))
                randomLine = randomLine.Replace(")", "");

            randomLine = randomLine.Replace("ACTION", "*");
            randomLine = randomLine.Replace("UNNAMED_USER", "someone"); 
            randomLine = randomLine.Replace("RANDOM_EMOTICON_HAPPY", filter.randomEmoticon_happy());
            randomLine = randomLine.Replace("RANDOM_EMOTICON_SAD", filter.randomEmoticon_sad());

            return randomLine;
        }
#endregion

#region Haiku
        public static string generateHaiku(string inputLine)
        {
            // Haiku = 5 / 7 / 5

            String haikuLine = "";
            String haiku1 = "";
            String haiku2 = "";
            String haiku3 = "";

          //  String haiku2Temp = totalNonsense();
          //  String haiku3Temp = totalNonsense();

            haiku1 = generateHaikuLine(5, inputLine);
            haiku2 = generateHaikuLine(7, haiku1);
            haiku3 = generateHaikuLine(5, haiku2);

            haikuLine = haiku1 + " / " + haiku2 + " / " + haiku3;
            
            haikuLine = haikuLine.Replace("(", "");
            haikuLine = haikuLine.Replace(")", "");
            haikuLine = haikuLine.Replace("*", "");

            return haikuLine;

        }

        private static int syllableCount(string word)
        {
            word = word.ToLower().Trim();
            int count = System.Text.RegularExpressions.Regex.Matches(word, "[aeiouy]+").Count;
            if ((word.EndsWith("e") || (word.EndsWith("es") || word.EndsWith("ed"))) && !word.EndsWith("le"))
                count--;
            return count;
        }

        private static string generateHaikuLine(int maxSyllable, string inputLine)
        {
            int syllableTotal = 0;
            string line = "";
                   
          //  String haikuTemp = totalNonsense();
            if (inputLine == "") { inputLine = "thisisjustatestyoushouldn'thaveanylinesinthislulz"; }
            String haikuTemp = IRC.myLB.pullFromBrain(inputLine, false);

            string[] nonsenseLine;
            String currentWord;
            nonsenseLine = haikuTemp.Split(new char[] { ' ' });
            int lastWordPos = (nonsenseLine.Length - 1);
            int currentWordNumber = 0;
            do
            {
                if (currentWordNumber <= lastWordPos)
                {
                    currentWord = nonsenseLine[currentWordNumber];
                    syllableTotal = syllableTotal + syllableCount(currentWord);
                    if (line != "") { line = line + " " + currentWord; }
                    if (line == "") { line = currentWord; }
                    currentWordNumber++;
                }
                if (currentWordNumber > lastWordPos)
                {
                    syllableTotal = maxSyllable;
                }

            } while (syllableTotal < maxSyllable);

            return line;
        }

#endregion


    }
}
