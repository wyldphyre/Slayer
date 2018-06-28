# Slayer

If you find yourself regularly needing to kill particular processes, this application makes it quicker and easier to do so.

I built this to provide a quick way to kill processes that I found myself needing to kill on a regular or semi-regular basis. The primary goal was that it should offer a way to select which process to kill if there was more than one of them running (the tool I was previously using would just kill all running instances of an executable).

## Setup

The simplest way to set up the application is to pin the .exe file to your taskbar and then run it. The first time Slayer is run it will install a default configuration file which you can modify by opening the jump menu (right-click on the taskbar icon after executing the exe at least once) and selecting **"Edit configuration"**. It is also possible to pass parameters to Slayer.exe. I'll cover that in future documentation.

### TODO

- Document use of the configuration file
- Document command line switches
- Add a small link or similar to the UI that will take you to the github page and/or documentation

## History

### 2.5 - 2017-06-28

- Added a `Restart` button to combine killing and restarting a process.

### 2.4.4 - 2017-11-28

- Disable the `Kill Others` button when only a single instance of the process is running.

### 2.4.3 - 2017-08-10

#### Fixed

- Now handles the scenario where the computer doesn't have an application associated with the `.config` extension. Jump menu now correctly created/updated when this situation is encountered.

### 2.4.2 - 2017-06-21

#### Fixed

- Would fail to show to show UI when trying to get the image for a process a process running as administrator. Will now try to get the image from another process, falling back to no image if all processes are running with administrator privileges.

### 2.4.1 - 2016-11-20

#### Added

- Shows the icon of the process at the top of the process list.

### 2.4 - 2016-11-20

#### Added

- Application now opens as close to the mouse as possible while still being fully visible

### 2.3.5 - 2016-01-28

#### Fixed

- Fix to make the UI update when a process is killed and the application remains open

### 2.3.4 - 2016-01-21

#### Changed

- Slayer now closes after clicking the "Kill Others" button

### 2.3.3 - 2015-12-02

- Internal code clean up. No functional changes.

### 2.3.1 - 2015-06-01

#### Changed

- Now only show a single header at the top of the app, instead of a header per process.

### 2.3 - 2015-02-08

#### Removed

- Configurable colour themes