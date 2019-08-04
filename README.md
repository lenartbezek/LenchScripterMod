[![GitHub license](https://img.shields.io/github/license/lenartbezek/LenchScripterMod.svg)](https://github.com/lenartbezek/LenchScripterMod/blob/master/LICENSE.md)
[![GitHub release](https://img.shields.io/github/release/lenartbezek/LenchScripterMod.svg)](https://github.com/lenartbezek/LenchScripterMod/releases)
[![GitHub total downloads](https://img.shields.io/github/downloads/lenartbezek/LenchScripterMod/total.svg)](https://github.com/lenartbezek/LenchScripterMod/releases)

# Lench Scripter Mod

Python scripting mod for Besiege allows you to control machines with Python scripts. It runs on [IronPython](http://ironpython.net/) engine. This enables you to create controllers, stabilizators, autopilots, bots and more.

### Installation

You will need [Spaar's ModLoader](https://github.com/spaar/besiege-modloader) to use this mod.
To install, place LenchScripterMod.dll in Besiege_Data/Mods folder. All mod assets will be downloaded automatically when needed.

### How to use

By default the mod loads and runs script file from `Besiege_Data/Scripts` with the same name as your saved machine on simulation start. To change this, open the script options window (Ctrl+U). You can also embed code directly into bsg files to be shared on workshop.

If you defined a function named `Update` or `FixedUpdate`, it will be called on every frame or at a fixed rate.

```py
# get block reference
wheel = Besiege.GetBlock("WHEEL 1")
direction = 1

# set wheel toggle mode
wheel.SetToggleMode("AUTOMATIC", True)

def Update(): # called on every frame
  # direction variable is defined globally
  global direction

  # if U is held down, speed is 1
  if Input.GetKey(KeyCode.U):
    speed = 1
  else:
    speed = 0
    
  # invert direction if I is pressed
  if Input.GetKeyDown(KeyCode.I):
    direction *= -1
  # set wheel speed slider
  wheel.SetSliderValue("SPEED", speed * direction)
```

For more information, see the [wiki pages](https://github.com/lenartbezek/LenchScripterMod/wiki).
