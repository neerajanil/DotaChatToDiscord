# DotaChatToDiscord
Forward chat inside dota game into Discord with text to speech enabled.

**Disclamer**: Only tested on Dota 64bit. Also it uses hooking so not guaranteed to be VAC safe.

## Commands
\#muteallchat
\#muteteamchat
\#mute:playername
\#unmute:playername
\#enabledonlymode
\#clear

## How does it work
It uses EasyHook library to inject DotaChatHook.dll into dota. DotaChatHook.dll then installs a local hook into the function in client.dll which is called when in game chat is received. The new function uses IPC to call the main console and the procceeds with the original function.

The main function on receiving the IPC call proceeds to process the comman/send the chat to the discord webhook.

## Learnings
The hardest parts were
  1) Finding the right tools to debug/disassemble client.dll in dota2
  2) Making sense of the assembly code of client.dll
  3) Finding the memory location of a suitable function to hook
  4) Reverse engineering an approximate function parameter list from the assembly code
 
I ended up using [x64dbg](https://github.com/x64dbg/x64dbg) to attach to Dota2.exe. Then it was an exhaustive process of putting breakpoints next on places referencing strings that seemed related to chat to eventually find a function that is only triggered by in game chat and had access to the required data. This page on [x64 Software Conventions](https://msdn.microsoft.com/en-us/library/7kcdt6fy.aspx) helped a lot in making sense of the assembly code and in guessing the number and format of the parameters.

## Credits
[DotaTranslator](https://github.com/ur0/DotATranslator)  
[x64dbg](https://github.com/x64dbg/x64dbg)  
[EasyHook](https://github.com/EasyHook/EasyHook)  
[JIL](https://github.com/kevin-montrose/Jil)  
RedDev!l and Kappa322 for helping me test in game!
