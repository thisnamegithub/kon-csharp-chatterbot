“Kon” is my attempt at creating a “chatterbot” written in C#. This bot will connect to an IRC server and join a channel whereupon it will “watch” the conversations. It will then try to “learn” from them so that people can hold conversations with it.

It has several different “brains” to learn/use in conversations. It has a basic brain that is simply a “monkey-see, monkey-do.” This brain will repeat something someone has said earlier in full, without manipulating it. It has an AIML brain (read the link for more information on AIML). Finally it has a “Language Bot” brain which is based on a chatterbot called “Language Bot” (written by BishounenNightBird of Esper.net). This brain attempts to take what people says and manipulate it so it can create semi-original thoughts. This brain is currently set to default.