Medieval Warfare
=======

- [COMP-361 Software Engineering Project 2014-2015](http://www.cs.mcgill.ca/~joerg/SEL/COMP-361_Handouts.html)
- The project requirements are available in the `/ref` folder.

Usage
=======
###Randomly-Generated Maps
Simply open the game and enter a seed (or click Random). Give yourself a unique name, let people join your room (max 5) and just play the game until someone wins!

###Saved Games
On Windows, Unity does file I/O in a folder that is usually `C:\Users\Username\AppData\LocalLow\PaulsTeam\MedievalWarfare`.
In order to save a game, hit `F5` during the game and everybody will have it saved to its local machine with the name `<seed>.json`. To load a saved game, it must be called `load.json` in that same folder.

###Gameplay
While it might be unintuitive, here are the keyboard "shortcuts" to play the game:

Key  | Action
------------- | -------------
`Enter` | End your turn
`C` | Cultivate meadow under selected unit
`B` | Build road under selected unit
`Esc` | Dismiss current action or message
`1` to `6` | Select type of unit to spawn
`F4` | Quit game
`F5` | Save game
`F6` | Load game


Requirements
=======
In order to build from source, you need to have Unity *4.6.3f1* (free version) installed, available for free from the [Unity archives](http://unity3d.com/get-unity/download/archive) (~1GB download). This runs on Windows and supposedly on Mac OS X.

To simply run the binary, you may only need the Unity plugin / web-player.

Paul's Team
=======
- Guillaume Labranche: Networking, UI, git maestro and serialization
- Paul Suddaby: Game algorithms and debugging maestro
- Ali Bhojani: Game algorithms and debugging maestro
- Andrej Gomizelj: Game development, UI and 3D modeling maestro
- Julie Morrissey: Project management, planning and software modeling

Resources Used
=======
- [Unity 3D](http://unity3d.com/)
- [Photon Unity Networking](https://www.exitgames.com/en/PUN)
- [SimpleJSON](http://wiki.unity3d.com/index.php/SimpleJSON)
- [SketchUp](http://www.sketchup.com/)
