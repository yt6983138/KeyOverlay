
# KeyOverlay
 A simple key overlay for osu! streaming
To change the keys used please use config.ini
# [Download Link](https://github.com/Friedchicken-42/KeyOverlay/releases/tag/v2.0)
IF YOU ARE HAVING PROBLEMS WITH THE PROGRAM WHEN OSU! IS ON FULLSCREEN, TRY USING GAME CAPTURE INSTEAD OF WINDOW CAPTURE IN OBS!


# config.ini properties
## General

height, width - Used to change the resolution of the program.

keySize - Changes the size of the key (excluding border).

barSpeed - Adjusts the speed at which the bars are travelling upwards.

backgroundColor - Changes the color of the background.

margin - Adjusts the margin of the keys from the sides.

outlineThickness - Changes the thickness of a square border.

fading - yes/no - Adds/removes the fading effect on top.

counter - yes/no - Adds a keycounter beneath each key that counts total clicks in a session.

fps - Sets the target FPS for the program to run.

## Keys
key1, key2 ... - Keys the program should use (UPPERCASE), for numbers and symbols [please refer to this table](https://www.sfml-dev.org/documentation/2.5.1/classsf_1_1Keyboard.php#acb4cacd7cc5802dec45724cf3314a142), for mouse buttons add m before the [mouse button options](https://www.sfml-dev.org/documentation/2.5.1/classsf_1_1Mouse.php#a4fb128be433f9aafe66bc0c605daaa90) ex. mLeft mRight. If you want more keys just add more fields.

## Display
key1, key2, ... - If the name of the key you are using is too large, or you would like to use a different symbol, you can use this property to override the original key name that is going to be displayed.

## Colors
key1, key2, ... - Use a custom color for the key instead of white.
