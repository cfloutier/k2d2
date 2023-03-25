# K2-D2
The KSP astromech

A set of tools to Help Space Navigation.

Simply execute the next maneuvre node for the moment.
The UI can be opened using `Alt-O` or using AppBar.

There is no use of the SAS for the moment (very early version). The pilot, wait for a good pointing attitude, do auto-warp and then auto-burn.

All of those phases must be enhanced.
It's working better than I can do by hand. But could me more precise. Especially the burning phase. There is always some small amount of dv Missing... It's based on Burn Duration of the Node and this is not perfect.

----------------
Project is renamed from AutoExecuteNode.

We are starting a collaboration with [Mole](https://github.com/Mole1803)

Thanks to Opus (#Opus#7354) for the name of the Mod !

# Installation

Open the game folder by right-clicking on the game in your Steam library, selecting "Manage," and then clicking "Browse local files."

Install the BepInEx mod loader:
https://spacedock.info/mod/3255/BepInEx%20for%20KSP%202

Install the Space Warp plugin. You need both BepInEx AND Space Warp:
https://spacedock.info/mod/3257/Space%20Warp

Download Auto Execute Node, open the zip file, and drag the included `BepInEx` folder into the game folder. (merge folders when asked)

# Thanks to

Thanks first for downloading.

* Big Thanks to [cheese3660](https://github.com/cheese3660)
1. for [SpaceWarp](https://github.com/Halbann). the base of all KSP2 MODs we can make for now on
2. and for [AutoBurn](https://github.com/cheese3660/AutoBurn) Very helpful code about how to start thrusts and how to get maneuvre node.

My method is totally based on his code.

* my first steps was based on [LazyOrbit](https://github.com/Halbann/LazyOrbit)
Thanks for this excellent first step ! a light code, simple and very well written.

# License

K2-D2 is distributed under the CC BY-SA 4.0 license. Read about the license here before redistributing:
https://creativecommons.org/licenses/by-sa/4.0/
