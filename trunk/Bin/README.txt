=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
Kon the C# Chatterbot - Version 1.5.3
Programmed by James "Iyouboushi" (Iyouboushi@gmail.com)
=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

Table of Contents:
 About
 Setup
 Commands
 What's New?
 Known Issues
 Contact

 _______________________________________________________________________
/                                                                       \
|                                ABOUT                                  |
\_______________________________________________________________________/


"Kon" is my attempt at creating a "chatterbot" written in C#. This bot will 
connect to an IRC server and join a channel whereupon it will "watch" the 
conversations. It will then try to "learn" from them so that people can hold 
conversations with it.

It has several different "brains" to learn/use in conversations. It has a 
basic brain that is simply a "monkey-see, monkey-do." This brain will repeat 
something someone has said earlier in full, without manipulating it. It has 
an AIML brain (read the link for more information on AIML).  Finally it has a 
"Language Bot" brain which is based on a chatterbot called "Language Bot" 
(written by BishounenNightBird of Esper.net). This brain attempts to take 
what people says and manipulate it so it can create semi-original thoughts. 
This brain is currently set to default.

Hopefully with time it will grow and one day be able to pass a Turing Test


 _______________________________________________________________________
/                                                                       \
|                                SETUP                                  |
\_______________________________________________________________________/


Setup is simple.  Extract the zip into a folder of your choice.  
If this is your first time running Kon, you will want to edit the config
file.  

Open config.xml in notepad or whatever editor you would like to use,
and change the values for your IRC server, port, the bot's nick, the
default channel you would like the bot to join, and what value you
would like to set "Random Talk" (the bot's ability to speak randomly
without being addressed).  Finally you will want to set your nick as
the admin.

After you have finished editing the config file, just double click 
kon.exe and it will automatically join the server and channel you
have set.

To address the bot, use

Kon: text goes here

OR

YourBot'sName: text goes here


** Note: although the bot comes with a small LB Brain filled with some
Internet memes and other sayings, until the bot views some conversations 
it will still have very little to say.


 _______________________________________________________________________
/                                                                       \
|                               COMMANDS                                |
\_______________________________________________________________________/


There are several commands in the bot that can be used.

CONSOLE COMMANDS
These are the commands that can only be used by the owner of the bot in 
the bot's console window.

/QUIT
Causes the bot to quit the IRC server.

/JOIN #channel
Will cause the bot to join the #channel specified.

/PART [#channel]
By itself, /PART will cause the bot to quit the DEFAULT channel that's set in the config.xml. 
With a channel specified, the bot will try to leave the channel.

/SAY [#channel] message
By itself, /SAY message will speak to the DEFAULT channel that's set in the config.xml.
If you pick a channel, you can have the bot talk to more than one channel that it is sitting in.

/MSG person message
This command will let you message non-channels (i.e. users and services).

/RECONNECT
This should let the bot quit the IRC server and reconnect to it.

/RAW
This command will send RAW commands to the IRC server.  For those out there that
know how to use IRC commands via RAW that haven't been added yet to the bot, this
is how you do them.

/CLEAR
Will clear the console window.

/DINFO
Will display some information about the bot's current status.

/TWITTERINFO
Will display information about the built-in twitter client including user name/password and whether or not
the client is currently enabled/running.

/TOGGLEPONG
Will toggle the display of the ping/pong messages. Useful for debugging in some cases.



CHANNEL COMMANDS
These are the commands that are done in an IRC channel by users.  There are three
user levels that can be defined in config.xml, each allowing multiple commands. 
Note that ADMIN level has access to all commands, USER level has access to user and
everyone commands.


* ADMIN LEVEL COMMANDS
!addUser nick
Will add the nick specified to the user-level list.

!remUser (nick)
Will remove the nick specified from the user-level list.

!quit
Will cause the bot to quit. Same as the console command.

!reconnect
Will cause the bot to reconnect to the server. 

!msg #chan/nick message
Allows you to send a message via command.

!raw RAW IRC COMMAND STRUCTURE
Allows you to send a raw IRC command


* USER LEVEL COMMANDS
!reloadAIML
Will cause the bot to reload its AIML files in case any changes were made. 
Please note that this command will take awhile and should only be used sparingly.

!toggleAIML
Will turn the AIML Brain on/off.  Turning the brain on will turn the other 
brains off.

!toggleLB
Will turn on the Language Bot ("LB") Brain on/off.  Turning this brain on will
turn the others off.

!channelUsers
Will display the users in the channel the command is used.  May not always be
accurate.

!utter
Will cause the bot to utter a random line.

!toggleTopic
Will toggle the bot's ability to try and locate the "topic" of what people
say to it. 

!randomTalk [#] 
By itself, !randomTalk will tell the channel what setting the option has been set
to.  If the user specifies a number, the setting will be changed.  Setting it to 0
will turn it off.  100 will make the bot respond to every line in the channel.

!join #channel
Will cause the bot to join the channel specified.  Same as the console command.

!part #channel      
Will cause the bot to leave the channel specified.      

!toggleDoubleSentences
Will toggle the bot's ability to string two sentences together if the first reply
is too short.  Recommended to leave this off until the bot's LB brain is big
enough to handle it, or else you'll get repeating lines back to back.

!Dinfo
Will show a few tidbits of internal info (reconnect attempt/total, if ping control is set to true,
and the # of ping attempts so far).   Really only useful for myself, but you might find it interesting.


* EVERYONE LEVEL COMMANDS
!splash
Will cause the bot to utter its namesake's catch phrase from Bleach.

!konInfo
Will display the current version number, along with my email.

!brainInfo
Will display the number of lines in its basic brain.

!LBinfo
Will display the number of lines in the LB Brain.  LB Brain must be on in
order to use this command.

!haiku
Will cause the bot to attempt to create a haiku using the 5/7/5 syllable structure.  It's not
perfect and the counts will often be a little off but it's a start. Needs the LB Brain to be
on in order to work properly.


 _______________________________________________________________________
/                                                                       \
|                            AUTO PERFORM                               |
\_______________________________________________________________________/


Ever wished you could make your version of Kon automatically join a bunch 
of channels and identify to Nickserv?  Well now you can with the auto
perform.  Here's how it works.

The commands will be read from perform.kon which is a plain text file 
that can be opened in any plain text editor (notepad works fine for it).
The commands that are recognized for the auto perform are: 
JOIN, SAY, MSG and WAIT.


JOIN - Joins a channel
SAY and MSG both work the same way here and lets you have the bot send 
 a message to someone or say something to a channel when the bot joins 
 the server.
WAIT - This command causes the bot to pause for a few seconds inbetween 
 commands. Use this if you have a lot of commands set in the auto perform 
 so that the bot doesnÅft flood the server and get booted off.
RAW  - Lets you send RAW IRC COMMANDS

So what youÅfll do is open kon.perform in your favorite plain text editor
and add a command one to a line. For example:

JOIN #KonBot
MSG NICKSERV identify mypassword
WAIT
SAY #KonBot hello all my peeps!


If you add MYBOTNAME in the autoperform it will automatically be replaced
with the bot's current nick.  This is helpful for if you want to do certain
raw commands (like MODE) or just wish to have the bot state it's current
nick when it joins a channel or something.  It has to be all caps though
to work (i.e. MYBOTNAME not MyBotName or some other variation).


 _______________________________________________________________________
/                                                                       \
|                             WHAT'S NEW?                               |
\_______________________________________________________________________/


For a complete list, read changelog.txt.

* Added the USER LEVEL command: !haiku  This command will cause the bot to attempt to 
  create a haiku using the 5/7/5 syllable structure. It's not perfect and the counts 
  will often be a little off but it's a start. Needs the LB Brain to be on in order 
  to work properly.

* Fixed the Twitter aspect so that it works with the 1.1 OATH system.

 _______________________________________________________________________
/                                                                       \
|                          CONNECTING TO TWITTER                        |
\_______________________________________________________________________/


Twitter made some (not so) recent changes that made it much more difficult
for apps to connect and post to it.  Here's a very quick guide on how to
make your Kon bot connect to a twitter account of your choosing.

First, go here:  https://dev.twitter.com/apps

Sign in (or sign up if you don't have an account).  Sign into the account
that you want the bot to be posting to.

Create a new application.

Fill out the information. For the website you can use this:
http://iyouboushi.com/projects/programs/c/kon/

Make sure the application has read and write access.

Finish with that.  Now click on the application to get the information 
about it.  Scroll to the bottom of that page and you'll see a bunch of
keys, secrets, and tokens.

Open the config.xml and look for the four twitter settings:

  <twitterAccessToken>
  <twitterAccessTokenSecret>
  <twitterOATHAccessToken>
  <twitterOATHTokenSecret>

Add the values from Twitter.

Twitter Access Token is "Access token"

Twitter Access Token Secret is "Access token secret"

Twitter OATH Access Token is "Consumer key"

Twitter OATH Access Token Secret is "Consumer secret"


Make sure <useTwitter> is set to True.

Voila!

!!!!!!!!!!WARNING!!!!!!!!!!!!
This access token and access secret allows an application to have
complete and utter control over that particular twitter account.  As such,
do not share the two strings with anyone as they'll have complete
control over the account.  In fact, I find it very wise to set up a second
account on twitter that you don't care about losing (should such a thing
happen) and use that account for Kon.


 _______________________________________________________________________
/                                                                       \
|                            KNOWN ISSUES                               |
\_______________________________________________________________________/


There are some known issues with the bot, mostly concerning the IRC aspect.
Hopefully these will be fixed sometime in the future.

* Bot does not have a rejoin option if it is kicked out of a channel.
* Bot will only rejoin the main channel it was in upon reconnect 
  ^^^ This can be fixed by putting all the channels in the autoperform file. 
* Occasionally the server will give the bot a 451 error that it cannot get around.
  To fix this, just close Kon and restart the bot. 9/10 times that'll fix it.
* Occasionally the bot won't join the main channel upon connect.  Fix this by
  having the bot join the channel in the autoperform file.


 _______________________________________________________________________
/                                                                       \
|                               CONTACT                                 |
\_______________________________________________________________________/


If, for whatever reason, you need to contact me.. my email address is
provided:  Iyouboushi@gmail.com

If you run into errors using THIS program, PLEASE contact me with the error
and what you were doing to encounter it so that I may work on fixing it for the next
Updater version.

