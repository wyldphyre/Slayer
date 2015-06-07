Slayer
======

If you find yourself regularly needing to kill a certain kind of process, this application can make it quicker and easier to do so.

<<<<<<< HEAD
I built this to provide a quick way to kill processes that, for one reason or another, I found myself needing to kill on a regular or semi-regular basis. The primary goal was that it should provide a way to select which process to kill if there was more than one of them running (the tool I was previously using would just kill all running instances of an executable).

## Setup

The simplest way to setup is to pin the .exe file to your taskbar and then run it. The first time Slayer is run it will install a default configuration file which you can modify by opening the jump menu (right click on the taskbar icon after executing the exe at least once) and selecting **"Edit configuration"**. It is also possible to pass parameters to Slayer.exe. I'll cover that in future documentation.
=======
I built this to provide myself a quick way to kill processes I often needed to kill during the development process. The primary goal was that it should provide a way to select which process to kill if there was more than one of them running (the tool I was previously using would just kill all running instances of an executable).

## Setup

The simplest way to setup is to pin the .exe file to your taskbase in Windows and then clicking on it. Nothing will happen at this point, but the default configuration file will be put in place. Right clicking on the taskbar button will provide a jump list of the processes that it has been configured to be able to kill. It is also possible to pass parameters to Slayer.exe directly.
>>>>>>> f92b0e2... Update README.md


### TODO: 
* Refactor code so that the user interface can be more easily replaced in the future
* Document use of the configuration file
* Document command line switches
