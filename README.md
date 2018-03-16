# Pinball Front End
Pinball Front End is a high speed interface for pinball cabinets that allows launching of different digital pinball emulators / software.



## PFE Table Manager

![alt text](https://github.com/Nutmegz/pinball_front_end/blob/master/PinballFrontEnd/PinballFrontEnd/Images/PFE%20Table%20Manager.png "PFE Table Manager")

## Default Keybindings:
| Function | Keybind |
| :----------- | :----- |
| Table Manager | F1 |
| Exit Table Manager | Escape |
| Random Table | F2 |
| Next Table | Right |
| Previous Table | Left |
| Start Table | Enter |
| Exit Table | Escape |
| Exit Front End | Escape |

## Install Instructions
Pinball Front End requires a couple of libaries to run. 

1. Download PinballFrontEndSetup.exe  from https://github.com/Nutmegz/pinball_front_end/PinballFrontEndSetup.exe and install it.
2. Download VLC Player from https://www.videolan.org/ and install it.
3. Download ffmpeg from https://ffmpeg.zeranoe.com/builds/ and unzip it into a folder you will remember. Typlically it is added to "C:\ffmepg\"
4. The paths then need to be configured in the PinballFrontEnd.exe.config file. Open it in a text editor and modify the PATH_VLC and PATH_FFMPEG value attribute to the respective folders that VLC and ffmpeg were installed into.
