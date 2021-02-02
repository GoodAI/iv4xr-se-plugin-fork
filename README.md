# iv4xr-se-plugin

This is a fork of the [iv4xr-se-plugin](https://github.com/iv4xr-project/iv4xr-se-plugin) repository which is a plugin for [Space Engineers](https://www.spaceengineersgame.com/) which makes it possible to control the game via a TCP/IP socket.

## Ingame commands

This fork adds several ingame commands which can be activated by opening the chat window (ENTER key by default) and typing the command there. All commands are case-insensitive.

### General

#### */ToggleSensors*

Toggles sensors visualization in order to better understand what the agent senses.

#### */ToggleMaxSpeed*

By default, the game is capped at 60 physics simulation updates per second. This command can disable this cap to get much better simulation performance.

#### */BDs load \<filename\>*

Loads a file with behaviour descriptors which can be then visualized with the following commands.

#### */BDs show*

Shows behaviour descriptors represented as red cubes in the world. The command goes through all generations found in the loaded file and shows all behaviour descriptors from that generation for a second before moving to the next generation.

#### */BDs stop*

Can be called after */BDs show* to stop at the currently shown generation.

#### */BDs stop*

Same as */BDs stop* but also hides all behaviour descriptors.

### Robotic leg grid

#### */grid teleport*

Teleports the player close to the (possibly not yet initialized) grid.

#### */grid spawn \<X\>*

Spawns an **X** by **X** grid and initializes all controllers. Can be called even if there is already an existing grid in the world.


#### */grid spawn \<X\> \<Y\>*

Spawns an **X** by **Y** grid and initializes all controllers.


#### */grid load \<X\>*

Initializes controlles for an **X** by **X** grid. This command needs to be called when working with an already existing grid after reloading the world.


#### */grid load \<X\> \<Y\>*

Initializes controlles for an **X** by **X** grid.

#### */grid random run*

Starts random moves of robotic legs. This command can be called only after the grid is spawned or loaded.

#### */grid random stop*

Stops random moves of robotic legs.

#### */grid clear all*

Removes the grid from the world.

#### */grid clear \<X\> \<Y\>*

Removes an experiment at the position *(X,Y)* (starting at (0,0)).

#### */grid restart \<X\> \<Y\>*

Restarts an experiment at the position *(X,Y)* (starting at (0,0)). This removes the experiment and then pastes the initial state of the blueprint.

## How to run the game with this plugin

1. Obtain the binary release of Space Engineers (buy it on Steam or get a key). Install the game.
2. Obtain the binary release of the plugin. Look for [releases](https://github.com/iv4xr-project/iv4xr-se-plugin/releases) in this repository and for Assets of the chosen release. Download the two DLL libraries.
3. IMPORTANT: Make sure Windows is OK to run the libraries. Windows 10 blocks "randomly" downloaded libraries. To unblock them, right-click each of them and open file properties. Look for Security section on the bottom part of the General tab. You might see a message: "*This file came from another computer and might be blocked...*". If so, check the `Unblock` checkbox.
   (If you skip this step, the game will probably crash with a message: `System.NotSupportedException`: *An attempt was made to load an assembly from a network location...*)
4. Put the plugin libraries into the folder with the game binaries. A common location is `C:\Program Files (x86)\Steam\steamapps\common\SpaceEngineers\Bin64`.
   Tip: It's you can put the libraries into a subfolder (such as `ivxr-debug`). Or, it can be a symbolic link to the build folder of the plugin project. In that case, you must prefix the name of each library with `ivxr-debug\` in the following step. 
5. Right-click on the game title in the Steam library list and open its properties window. Click on the **Set launch options...** button. Add the option `-plugin` and list the libraries. The result should be something like this: `-plugin Ivxr.PlugIndependentLib.dll Ivxr.SePlugin.dll`.
6. Run the game. (If the game crashes, make sure you've seen step 3.)
7. Start a scenario. (It's necessary to do it manually for now. Should be done automatically by the testing framework in the future.)
8. If the plugin works correctly, a TCP/IP server is listening for JSON-based commands on a fixed port number. (The current development version uses the port number 9678.) 
   Another sign of life is a log file present in user's app data folder such as: `C:\Users\<username>\AppData\Roaming\SpaceEngineers\ivxr-plugin.log`

## How to build

Requires Space Engineers codebase (which is not open) to compile. The resulting plug-in (a couple of .NET libraries), however, works with the official Steam version of Space Engineers without any modification of the game.
