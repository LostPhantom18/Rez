# Terminal Games

This repository contains samples and some utility code to help you make terminal games for the Fall 2025 Mohawk College Game Design Design Week challenge.

Team do not need to use this code, but it solves a number of challenges teams might face. You can ask questions and report issues on the [issues page](https://github.com/MohawkRaphaelT/terminal-games/issues) in this GitHub repository.

# Windows 10 Setup

Users of Windows 10 need to do a few steps to setup to be on-par with Windows 11 users.

1. Go to the Microsoft Store on your Windows 10 machine and look for "Windows Terminal" or navigate to [Windows Terminal page in your browser](https://apps.microsoft.com/detail/9n0dx20hk701?hl=en-GB&gl=CA).
2. Download and install Windows Terminal.
3. Run any console application in Visual Studio. In the window that opens, right-click the window and select Properties.
4. Navigate to the "Terminal" tab.
5. In the "Default Terminal Application" dropdown, select "Windows Terminal."
6. Press "ok."

![config-windows-terminal](https://github.com/MohawkRaphaelT/terminal-games/blob/main/res/config%20windows%20terminal.png)

You can test that things work by running the console application again.

Windows Terminal is both faster and has supports emoji text. 🔥😎🎉

# Configuring Windows Terminal

You can configure a number of parameters in Windows Terminal. Here are 3 things you should note.

### 1. Startup Size

You can configure the number of rows and columns the terminal window has on startup.

![windows terminal startup size](https://github.com/MohawkRaphaelT/terminal-games/blob/main/res/windows%20terminal%20startup%20size.png)

### 2. Startup Type

You can choose how the window opens, whether it be normal or maximized (full-screen).

![windows terminal startup type](https://github.com/MohawkRaphaelT/terminal-games/blob/main/res/windows%20terminal%20startup%20type.png)

### 3. Renderer

You can choose the renderer for Windows Terminal. If you seek performance, you should use the more modern renderer.

![windows terminal renderer](https://github.com/MohawkRaphaelT/terminal-games/blob/main/res/windows%20terminal%20renderer.png)

# About This Project Template

There are different modes in the template and sample projects to note. Some things are setup to run in real-time, while others are meant to run as a regular terminal.

Note that it is on you to choose the configuration. The two sample projects provided should give you a good idea as to what you could do, and how to do it.

### Sample Projects

* [See Dungeon Crawler sample](https://github.com/MohawkRaphaelT/terminal-games/tree/main/DungeonCrawlerSample).
* [See Text Adventure sample](https://github.com/MohawkRaphaelT/terminal-games/tree/main/TextAdventureSample).

### About Real-Time Terminal Games

Note that while you can aim to run at 60 FPS, clearing and writing the screen every "frame" is slow. Overwriting text by moving the cursor is much quicker.

Moreover, event at a consistent rate, the games timing isn't perfect, with reach frame deviating by 2-15 milliseconds depending on unknown factors.

### Provided Utilities

I recommend having a look at the code files in the [template sub-folder](https://github.com/MohawkRaphaelT/terminal-games/tree/main/MohawkTerminalGame/MohawkTerminalGame). There are a number of files meant to help remove grunt-work from common effects.

In particular, [Terminal.cs](https://github.com/MohawkRaphaelT/terminal-games/blob/main/MohawkTerminalGame/MohawkTerminalGame/Static%20Classes/Terminal.cs) is meant to be a full replacement of Console with a number of bells and whistles. Specifically, it can properly handle text wrapping, simple overloads for writing text with colors, and having cinema-esque computer typing rather than direct words dumps via Write/WriteLine.

There is a TerminalGrid and TerminalGridWithColor classes that are meant to help set up maps/levels in a terminal game. The [Dungeon Crawler sample](https://github.com/MohawkRaphaelT/terminal-games/tree/main/DungeonCrawlerSample) showcases this, and how to be performant given the limitations. While not shown, the grid comes with functions to draw rectangles and circles of characters to the grid which might be handy.

### Exiting

As a final note, CTRL+C is a universal key combination to force-exit a terminal application.  Use that if hitting escape doesn't terminate your game.
