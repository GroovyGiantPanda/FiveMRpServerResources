# Source Code
Hi! I'm an ex-developer for The Family RP releasing my part of the source code. This is not the full project, but 60+% of the project counted by lines of code ([#](https://i.imgur.com/lHE9VJm.png)) at the time I left in the middle of November. I was planning to release this months ago, but just never got around to it.

I have excluded other developers' code from this release as good as I possibly could. (Do contact me if there is something else you would like me to see removed or if I did not credit you.) Due to this, you will have to fill in the gaps yourself or ask nicely.

Expect bugs that are not on the live server--they have been fixed by the other developer gods after I left.

# Featured Contents

**(Interaction) Menu**: This includes the full source for the menu system used on The Family RP. I wanted to write a framework that was able to recreate the vanilla GTA menus and make it as compat as possible, and would say I more than succeeded. Allows for all kinds of extensibility and includes checkboxes, listboxes and anything else you can think of. Includes e.g. the emote menu ([video](https://streamable.com/izku5)) as an example (along with all the other menus listed below.) Allows for quick implementation of everything from shops to settings menus and has over 30 customizable variables out of the box. The difference between this menu system and nearly all others is how it is rendered--this draws the menu directly using primitives, while other menu systems are HTML-based (block Push-to-Talk keys while open, hog FPS...)

**Loading Screen**: As seen on NoPixel and The Family RP ([live browser demo](https://groovygiantpanda.github.io/) or as seen on [The Family](https://streamable.com/7b39u))

**Revamped Chat**: As seen on SoE and The Family RP. Modded for multi-line support, scrolling through chat history with scroll wheel/PgUp/PgDn, command history, custom HTML objects and a large number of bug fixes.

**Character Customization Menu**: This is the full character customization menu as seen on the server. The menu is slightly different from what you have seen in The Family character creation, as another awesome developer (MsQ) made things a bit more user-friendly. This includes all the 100+ features you can pick from as a GTA multiplayer character. ([video](https://streamable.com/mozw5))

**Vehicle Customization/Spawning Menu**: This is a vehicle customization menu with support for all GTA vehicle customization GTA has to offer. ([video](https://streamable.com/k5gyt))

**Cinematic Mode**: As seen on both The Family RP and SoE - the ability to toggle all UI and add variable-size letterboxes. It ended up being as popular among the streamers as I personally hoped it would be.

**A Bunch of Game HUD Features**: Many of them scrapped and replaced by alternatives or hidden in the live version. I personally like these a lot too.

**LEO Features**: Includes a large number of LEO features, including tackling, soft cuffing, speed trap radar, police helicopter features, spike strips, police car siren binds, lockable doors, GSR testing and "triage" features.

**Scoreboard**: What I thought was a pretty sexy alternative to a scoreboard. Eventually scrapped on TFRP since it was decided against having a visible scoreboard. ([video](https://streamable.com/fr20m)) Also keeps track of e.g. the server uptime and how long the players have been on the server (not enabled in the clip above.)

**Queue System**: What it says on the tin. This is in a rather buggy state and had not been very well-tested at this point. The queue system on the live server is basically a full rewrite of this by blcd.

**Whitelisting System**:This includes everything except the actual whitelisting at your endpoint.

> This requires your community to have IPB with Steam, Discord and
> Twitch login plugins. This has a daemon that runs continuously on a
> server (every minute or so) and checks for new/updated IPs and changes
> in linked accounts and what forum groups the member is part of. Also
> checks for e.g. number of Steam bans on record, their friend count and the age of the
> accounts to filter out newly created accounts. Will communicate with
> IP whitelisting system the IPs of those that fulfill all requirements
> (correct forum groups etc.) When a user is banned, the ban is
> propagated to all linked accounts and will propagate further if that
> account is ever combined with some other non-banned account.

**XML Custom Interior Loader**: This is a combined C# pre-parser of GTA V map editor XML files and C# map loader. The advantage compared to other map loaders is that this re-parses the XML file and MsgPacks it, reducing map file sizes by up to 90% and speeding up load times by a ton.

**Polygon Boundaries**: Includes a helper class and an example application that allows e.g. the prison to have force field edges instead of having to teleport the player back to the center of the prison. Also used for e.g. the different fishing areas on the server. 

**Ped Damage**: A class that keeps track of what damaged both AI and users on the server, allowing for e.g. medics or cops to check weapon types, bones damaged etc. This is a buggy implementation, which was rewritten by Mooshe for the live server, but should give you a good idea on how to replicate this.

**Pointing/Hands Up**: It's easy to find on the forums nowadays, but this was a pretty cool feature when it was unique. Here's a C# implementation. ([video](https://streamable.com/1euq6))

**C# rewrite of FiveM SessionManager/SpawnManager/MapManager**: As I had sort of a dream of a server completely devoid of any Lua code, I decided to rewrite the FiveM SessionManager/SpawnManager/MapManager completely, in C#. This also lead to a lot of fat being trimmed. (For example, we never use the built-in configuration options, so it was rather unnecessary.)

**AFK Kick**: A sample AFK kick feature.
 
**Key Code Tester**: If you are a server developer this may be a useful tool. Instead of having to consult key lookup tables online you can turn it on and see all the key codes a single key press activates. ([video](https://streamable.com/edznf))

**Prototype Fishing and Hunting**: If you ever want to implement a fishing mini-game on your server, you probably want to use this method for water detection instead of the GTA natives. The GTA natives often give false readings, while this neat hack uses a completely different method. The hunting prototype also shows the power of the pool iteration natives that the FiveM developers added.
 
**Control Binds Class**: This is a C# class that makes it really easy to handle binds, with and without modifiers (like CTRL/SHIFT/ALT or any combination of those.) Has saved a ton of time.

**Blip Spawner and Marker Handler**: These classes centralize the process of adding blips (the minimap things) and markers (the ground circles) and makes these easier to manage, by e.g. allowing for the filtering of blips by category.

**Car AI/Locks**: This locks parked and running cars in the world with different probabilities. One feature is if a car is locked in traffic and you try to break in, all nearby cars lock their doors and run away. Also e.g. has a higher probability of running cars locking their doors if you have a weapon out. Includes lockpicking feature (but not the final one that is on the server.)

**Civilian Car Cop Lights**: Another pretty silly idea that allows civilian cars to become impromptu cop cars by simply swiftly shifting the car underglow between red/blue.

**Warp Points System**: Similar to above to make the addition of warp points as easy as possible. Has support for automatic loading and unloading of both interiors and interior prop sets.

**All Kinds of Other Time-Saving Classes**:  There are a bunch of other classes I think a developer may find helpful or educational, including the TriggerEventFor* classes which I find to be one semi-neat way to speed up development.

**Petrol Stations**: A great example of what you can do with the pool iteration natives the great FiveM developers added last year. By only adding the object hashes for the petrol pumps in GTA, there is no need to specify coordinates for petrol stations. You can do the same for ATMs or vending machines and save a lot of time.

**Drivable, Derailable Trains**: Yes, those trains! If you ever wanted to know what happened to these - they were never tested properly in multiplayer until after the TwitchCon event. Probably a good idea... In multiplayer it turned out that trains stuttered noticeably for other players and it was not something easily resolved. ([Freight](https://streamable.com/ydz0q), [Metro](https://streamable.com/y3di6))

**Custom JSON (De-)Serializer**: As a lot of UI is "NUI", or Chromium/web-based, this is a minimalistic JSON serializer to make communication easier when such is needed on the client-side (since FiveM does not include the necessary libraries for this on the client-side, I ended up simply writing my own serializer.) It also includes a nearly finished JSON deserializer.

**Dev Tools**: Sort of like a C# server-side trainer. Allows you to e.g. spawn any vehicle, pedestrian or object you can think of by typing only partial search terms. Includes features like no-clip, the ability to move and rotate most of the objects in the world by looking at them and pressing a hotkey or grab the object hash of any object in the world as well as the ability to load any custom interior in GTA. ([video A](https://streamable.com/2g6i9), [video B](https://streamable.com/sx1xq))

**Fuel Manager**: Adds fuel and refueling as a concept.

**Other Vehicle Features**: Window and door sub-menus (for opening/closing) and realistic turn signals, cruise control.

**Vehicle/Garage Pack and Unpack**: A convenient class to pack and unpack vehicles into/from MessagePacked strings/C# objects for storage in a database.

**Weather and Time Sync**: Synchronizes time and weather. Includes [cloud hats](https://streamable.com/kl5cx), which many other weather scripts don't include. Kudos to MsQ for the initial version, although this is a complete rewrite.

**Voip Range**: Shows how the VOIP range toggle works, with indicator.

**Player Markers**: The ground indicators as seen on TFRP.

**Object Hash List**: A really useful object hash list for reverse hash lookup (should really be part of FiveM C# libraries). 

**Discord Bot**: The internal Discord bot used to show player count on the server, players that are currently streaming on Twitch as well as status data for the forum as well as TeamSpeak server (e.g. number of registered/online users.)

**/r/GTAVRPClips Bots**: The source for the subreddit bot that used to upload mirrors for all linked Twitch clips as well as the source for the bot that used to assign flairs based on scores, listing of current GTA Twitch streamers etc. 

**A Large Number of Other Things**: There are a large number of things I excluded here that will come at a later point in time or that I didn't mention here for brevity. I cut way more stuff than I had to just because I could not be bothered to separate my stuff from my fellow developers'. I'll also be adding a number of new resources that are not on The Family RP (or anywhere else) when I get around to it. If there is anything in particular you would like to know about, feel free to ask.

## License
This is under the [MIT License](https://choosealicense.com/licenses/mit/), which is basically the most permissive widely used license out there. If you release a derivative work, include credit to the linked GitHub so others know where to find the rest (unless you make significant alterations, in which case you can do whatever you feel like doing).

## Why C#?
There are many reasons we went with C#. Three good reasons are better performance (up to 10x faster than Lua), its static typing and _much_ better support for object-oriented development. Subjectively, it allows for integration of many different modules into a whole on a completely different level. Visual Studio/IntelliSense and Resharper are also too good to live without. The higher performance of C# also means a lot less CPU hogging if you intend to perform intensive tasks like iterating the GTA entity pools with high frequency.

## Enjoy!
Hopefully some of you find this codebase helpful and/or educational!

I will add this to the subreddit closed-message in the old subreddit [/r/GTAVRPClips](http://www.reddit.com/r/GTAVRPClips/) so you always have a way to find it. (Don't worry; that subreddit is not coming back. Submissions will remain approval only and this subreddit will remain linked via sticky.) It will also remain permanently available on the GitHub.

Also, a large thanks to all the The Family developers and the rest of the community for the great couple of months we spent together. 