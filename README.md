Slayer
======

An Windows application for quickly killing targeted processes.

I built this to provide a quick way to kill processes that for one reason or another I found myself needing to kill on a regular or semi-regular basis. The primary goal was that it should provide a way to select which process to kill if there was more than one of them running (the tool I was previously using would just kill all running instances of an executable).

## Setup

The simplest way to setup is to pin the .exe file to your taskbase in Windows and have it run by taking it's configuration from the Slayer.exe.config file. It is also possible to pass parameters to Slayer.exe.


### TODO: 
* Add Jump Menu options to edit/locate the configuration file
* Update ReadMe on how to get started once the new configuration location code is in place
* Refactor code so that the user interface can be more easily replaced in the future
* Document use of the configuration file
* Document command line switches
