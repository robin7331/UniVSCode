# UniVSCode
UniVSCode is a script that simplifies using Visual Studio Code with Unity on a Mac.

##Requirements
#####1. Install and Setup VSCode: http://unreferencedinstance.com/how-to-integrate-visual-studio-code-with-unity3d-project/
#####2. Copy UniVSCode.cs and SimpleJSON.cs into somewhere your project folder
[UniVSCode.cs](https://github.com/robin7331/UniVSCode/blob/master/UniVSCode.cs)

[SimpleJSON.cs](https://github.com/robin7331/UniVSCode/blob/master/SimpleJSON.cs)
#####3. You are now ready to use UniVSCode!

##This is what you can do:
#####1. Easily open the whole project folder in VSCode from Unity
![menu screenshot 1](https://raw.githubusercontent.com/robin7331/UniVSCode/master/readme/open_project.jpg)
#####2. Open c# files through a double click in your project view 
This will open the file in your already opened project window of VSCode
#####3. Open c# files at the correct line via double click in your console
Very convenient since it works with Logs, Errors, Warnings, ...
#####4. Send all debugging port information automatically to VSCode
![menu screenshot 2](https://raw.githubusercontent.com/robin7331/UniVSCode/master/readme/send_port.jpg)
![menu screenshot 2](https://raw.githubusercontent.com/robin7331/UniVSCode/master/readme/debug.jpg)

The port will be written to your launch.json file in VSCode so you can simply hit "Play" in VSCode to debug.
Be sure that you are in Play mode in Unity before hitting Play in VSCode.
Every time you hit Play in Unity, you have to send the debug information to VSCode. 
This is because Unity is changing it's debugging port on every session.

#####NOTE: Unity will crash when you debug your code. This is a bug in Unity which I already filed. Once there is a fix for this it will be published here.


####To disable UniVSCode and use the app you've selected in your Unity settings, simply deactivate UniVSCode in the edit menu.
![menu screenshot](https://raw.githubusercontent.com/robin7331/UniVSCode/master/readme/menu.png)
