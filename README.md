Slayer
======

If you find yourself regularly needing to kill particular processes, this application makes it quicker and easier to do so.

I built this to provide a quick way to kill processes that I found myself needing to kill on a regular or semi-regular basis. The primary goal was that it should offer a way to select which process to kill if there was more than one of them running (the tool I was previously using would just kill all running instances of an executable).

## Setup

The simplest way to set up the application is to pin the .exe file to your taskbar and then run it. The first time Slayer is run it will install a default configuration file which you can modify by opening the jump menu (right-click on the taskbar icon after executing the exe at least once) and selecting **"Edit configuration"**. It is also possible to pass parameters to Slayer.exe. I'll cover that in future documentation.

### TODO

* Document use of the configuration file
* Document command line switches
* Add a small link or similar to the UI that will take you to the github page and/or documentation

## History

### v2.4.3

* Now handles the scenario where the computer doesn't have an application associated with the `.config` extension. Jump menu now correctly created/updated when this situation is encountered.

### v2.4.2

* Would fail to show to show UI when trying to get the image for a process a process running as administrator. Will now try to get the image from another process, falling back to no image if all processes are running with administrator privileges.

### v2.4.1

* Shows the icon of the process at the top of the process list.

### v2.4

* Application now opens as close to the mouse as possible while still being fully visible

### v2.3.5

* Fix to make the UI update when a process is killed and the application remains open

### v2.3.4

* Slayer now closes after clicking the "Kill Others" button