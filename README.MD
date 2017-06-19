a private lobby emulator for the game .hack//frägment

credits go to NCDyson for the actual help, thank you alot!

this is essentially an implementation in C# of this: https://github.com/Lord-Ptolemy/lobbyemu

in action: https://www.youtube.com/watch?v=VErjuUM4AQM

if you wanna know why I left this toxic community, let me cite "coldbird" himself:

"+Warranty Voider I honestly don't give a damn about this project anymore since a bunch of cheaters decided to use legit bans as a reason to start somewhat of a revolution on our Discord server and the sheep (people that can't think for themselves) decided to side with said cheaters and their lies over us, the actual developers and administrators.

Long story short, I don't approve of the way you treat my fellow team mate +Muguster.
And to prevent further meddling here's a list of all the people that were actively involved with the Fragment project and what exactly they were doing / contributing, so you know just what exactly you ARE using and who the author was, so you can make due on your promise and credit everyone involved.

Muguster (aka. The Mugi, aka. Kitsune):

He spent most of his time with fine-tuning translations on both the PS2 client as well as the Area Server, and helped me countless nights with analyzing AreaServer <-> PS2 client as well as Lobby network traffic to reverse engineer the network protocol.

Furthermore he was heavily involved with stat-analysis and file-analysis on the AreaServer, and helped us remove several dithering effects on graphics in both the game and the AreaServer.

If you happen to doubt his abilities, I would suggest you reconsider your choice of words, as he is one of the most talented people I've worked with so far. The only person to outrank him in direct comparison is most likely Dyson, as he is a ASM-god (even when put in direct comparison to myself).

Dyson (aka. NCDyson):

In what I would describe a marvel of debugging-expertise, Dyson has modified the PCSX2 sourcecode himself to allow extensive debugging that would otherwise not be possible, and in doing so managed to reconstruct the initial set of Lobby traffic packets without having reference material to go from. This marked the beginning of our lobby server protocol reconstruction in early stages of the project and was a big stepping stone that needed to be conquered before we could proceed further.

After that he continued to provide us with complex GUI-related ASM patches to make the game GUI a lot more usable on PCs running PCSX2.

For example the GU-style GUI we introduced further down the road was a collaboration between Muguster (graphics) and Dyson (ASM), allowing us to shrink the GUI into a more manageable version, allowing players to see more of the world inside .hack//frägment instead of having 70% of the screen real estate be covered in GUIs.

He was also heavily involved with cracking the encryption on the AreaServer, game save and game archive files, which allowed us to explore the game further.

Coldbird (good old me):

I started the project one year after the official server went down, and it was me that cracked the network encryption and protocol.

I also did the initial work on the game archive files and most of the programming that went down. The DNAS emulator patch in the game was my doing as well.

Furthermore I'm the author of the (WIP and unreleased) AreaEmu project, which is a fully opensource cross-plattform compatible lightweight variant of the AreaServer.

As of now this is also the only server software that can access the fifth root town, Lia Fail.
Which btw, Muguster was the one to help me with testing and unlocking (it required us to re-add some missing files from the old .hack//outbreak disc to .hack//frägment).

I'm also the guy that imported and tweaked most of the translations you see in the game, ever used a item or fought a monster? Navigated the menu? Yeah, that's my work.

Everyone content now?"
