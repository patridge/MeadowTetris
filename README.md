# Meadow Tetris

Modified version of the [Meadow Tetris sample from the Meadow Project Samples](https://github.com/WildernessLabs/Meadow_Project_Samples/tree/main/source/Games/Tetris) and [covering hackster.io project](https://www.hackster.io/wilderness-labs/make-your-own-tetris-game-with-meadow-bc5643).

This game was presented as part of the [Developing hardware games
with Meadow and .NET session](https://docs.google.com/presentation/d/12e_5ahy1DEcmbZx4xR_ILKQyRJ-pq9zaKkN8ua8qIfE/edit?usp=sharing) session at [FRBCon 2021](https://flatredball.com/).

## Modifications

The original hackster.io project used an analog joystick. It has since been modified to buttons, and this one had a similar modification.

Additionally, this version has a couple tweaks.

* [Save high score to disk (persists across deployments)](https://github.com/patridge/MeadowTetris/blob/main/MeadowTetris/GameData.cs)
* [Display saved high score before starting game](https://github.com/patridge/MeadowTetris/blob/098447954749b918baffab2978cfaa1ac876c6fb/MeadowTetris/MeadowApp.cs#L165-L173)
* [Instant drop when the up button is pressed](https://github.com/patridge/MeadowTetris/blob/098447954749b918baffab2978cfaa1ac876c6fb/MeadowTetris/TetrisGame.cs#L150-L163)
