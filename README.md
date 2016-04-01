# Lench Scripter Mod
Lua scripting mod for Besiege v0.27

# Running scripts

The Mod looks for scripts to load in the /Besiege_Data/Scripts subfolder of your game's directory. Scripts are bound to their machines by sharing the same filename (with a different extension), so name your script file like this: \<machine name>.lua. The game decides which script to load by looking at the last saved/loaded machine's name, so make sure to save your machine first if you're building from scratch.

The scripts starts executing at the start of simulation and ends at the end of it.

`onUpdate()` is called on every frame.

`onKeyDown(key)` is called once for every keypress.

`onKeyHeld(key)` is called every frame a key is held.

Example script:

```lua

wheel_id = "WHEEL 1"
direction = 1
speed = 0

besiege:setToggleMode(wheel_id, "AUTOMATIC", true)

function onUpdate()
    -- updates the speed on every frame
    besiege:setSliderValue(wheel_id, "SPEED", speed * direction)
    -- reset values
	speed = 0
end

function onKeyDown(key)
    -- inverts the direction of the spinning when I is pressed
    if key == KeyCode.I then
        direction = direction * -1
		besiege:log(string.format("New direction: %d", direction))
    end
end

function onKeyHeld(key)
    -- the wheel spins when U is held down
    if key == KeyCode.U then
        speed = 1
    end
end
```

# Block identifiers

### Identifying blocks with sequential identifiers:

Block identifier is a case insensitive string, made of a blocks name and his sequential number. This is the fastest way to select blocks.

Examples:

* `STARTING BLOCK 1`
* `Flamethrower 17`
* `simple rope + winch 3`

### Identifying blocks with their GUIDs:

Alternatively, you can also identify blocks by their GUID. This way is independent of blocks order and thus more reliable.

Examples:

* `f987a650-2e7d-4f73-b2a5-84798506ad4e`

**To view blocks identifiers, press LeftShift while pointing at it to print it into console (Ctrl+K).**

# Lua functions

You can use .NET assemblies and UnityEngine in your Lua script.
For advanced use, you can manipulate game objects directly through C# reflection.
For most of your needs though, the following functions, accessible as methods of the `besiege` object, should suffice.

## Miscelaneous functions

### log(string msg)
Used to print into console.

`besiege:log("Hello World")`

### getTime()
Used to get time in milliseconds from the start of the simulation.

`ms = besiege:getTime()`

## Property functions

Following functions access blocks properties using additional property identifiers.

### setToggleMode(string blockId, string toggleName, bool value)
Used to toggle various block properties.

`besiege:setToggleMode("FLYING SPIRAL 3", "REVERSE", true)`

### setSliderValue(string blockId, string sliderName, float value)
Used to set slider value of various block properties.
Can be set over the limits of maximum and minimum slider values.

`besiege:setSliderValue("FLYING SPIRAL 3", "FLYING SPEED", 3.4)`

### getToggleMode(string blockId, string toggleName)
Used to get the toggle value of various block properties.
Throws an exception if the property is not found.

`is_automatic = besiege:setToggleMode("FLYING SPIRAL 3", "AUTOMATIC")`

### getSliderValue(string blockId, string sliderName)

Used to get the slider value of various block properties.
Throws an exception if the property is not found.

`range = besiege:getSliderValue("FLAMETHROWER 2", "RANGE")`

Functions **getSliderMin(...)** and **getSliderMax(...)** return the sliders editor minimum and maximum values, but since you can assign any value with setSliderValue function, they are pratcically useless.

## Key functions

Functions used to manipulate blocks key bindings do so with action identifiers. These are case insensitive and equal to the display name in the in-game key mapper window. For example, a wheel will have key bindings for 'FORWARDS' and 'REVERSE'.

### addKey(string blockId, string keyName, int keyValue)

Used to add key binding to the blocks action, specified by key name.

`besiege:addKey("WHEEL 1", "FORWARDS", KeyCode.F)`

### replaceKey(string blockId, string keyName, int keyValue)

Used to replace the first key binding of the blocks action, specified by key name.

`besiege:replaceKey("WHEEL 1", "FORWARDS", KeyCode.F)`

### getKey(string blockId, string keyName)

Returns the first key binding integer value of the blocks action, specified by key name.

```lua
key = besiege:getKey("ROCKET 1", "LAUNCH")
besiege:log(string.format("Rocket is launched by pressing: %d", key))
```

### clearKeys(string blockId, string keyName)

Clears all key bindings for the specified action.
`besiege:clearKeys("ROCKET 1", "LAUNCH")`

## Vector functions

Following functions that return a vector do so using [UnityEngine.Vector3](http://docs.unity3d.com/ScriptReference/Vector3.html). You can call any of its C# fields and methods through NLua reflection.

Examples:
```lua
v = Vector3(1, 3, 5.3)
besiege:log(v:ToString())
x_component = v.x
m = v.magnitude
angle = Vector3:Angle(a, b)
```

### getPosition(string blockId)
Return the coordinates of the block in a vector. Default argument is starting block.

`rocket_pos_vector3 = besiege:getPosition("ROCKET 3")`   
`rocket_pos_x = rocket_pos_vector3.x`

### getVelocity(string blockId)
Returns the velocity vector of the blocks rigidbody in units per second. Default argument is starting block.

`wheel_velocity = besiege:getVelocity("WHEEL 4")`   
`wheel_vertical_velocity = wheel_velocity.y`

### getEulerAngles(string blockId)
Returns the euler angles vector of the specified block, respective to the blocks forward, right, up vectors. If no argument is used, starting block is used. Values are returned in range from 0 to 360 degrees.

`rotation = besiege:getEulerAngles('PROPELLER 4')`

### getAngularVelocity(string blockId)
Returns the angular velocity vector of the blocks rigidbody in degrees per second. Default argument is starting block.

`wheel_angular = besiege.getAngular("WHEEL 7")`

### getRaycastHit()

Returns the point where mouse cursor is pointing in a form of a vector.

Following angle functions are swapped in a way to fit starting blocks initial position. This means that at the start of the simulation, starting blocks angles will be 0, 0, 0.

**getHeading(string blockId)**

Returns the directed heading angle in degrees, ranging from 0 to 360.

**getPitch(string blockId)**

Returns the directed pitch angle in degrees, ranging from -180 to 180.

**getRoll(string blockId)**

Returns the directed roll angle in degrees, ranging from -180 to 180.

**getYaw(string blockId)**

Returns the directed yaw angle in degrees, ranging from -180 to 180.

## Marks

With the following functions, user can easily put a mark (small coloured sphere object) to a desired location.

### createMark(Vector3 pos)

Initializes and returns a Mark game object at the given position.

```lua
mousepointer_location = besiege:getRaycastHit()
mark = createMark(mousepointer_location)
```

### clearMarks()

Clears all initialized marks.

Marks can be manipulated by calling methods from their class.

* move(Vector3 target)
* setColor(Color c)
* clear()

Example:
```lua
m = createMark(Vector3(1, 1, 1))
m:move(2, 2, 2)
m:setColor(Color(0, 1, 0)) --green
m:clear()
```

Property identifier is a case insensitive string, identical to the display name of the property in the in-game property tuning window.

It is used as an argument to slider, toggle and key functions.

`besiege.setSliderValue("STEERING HINGE 2", "ROTATION SPEED", 0.5)`
`besiege.addKey("ROCKET 1", "LAUNCH", KeyCode.L)`

Incomplete table of blocks and their properties:

| Block               | Toggle                                         | Slider                                          | Action               |
|---------------------|------------------------------------------------|-------------------------------------------------|----------------------|
| WHEEL               | AUTOMATIC<br> TOGGLE MODE<br> AUTO-BRAKE       | SPEED                                           | FORWARDS<br> REVERSE |
| PISTON              | TOGGLE MODE                                    | SPEED                                           | EXTEND               |
| STEERING            |                                                | ROTATION SPEED                                  | LEFT<br> RIGHT       |
| STEERING HINGE      | LIMIT ANGLE                                    | ROTATION SPEED                                  | LEFT<br> RIGHT       |
| CONTRACTABLE SPRING | TOGGLE MODE                                    | STRENGHT                                        | CONTRACT             |
| EXPLOSIVE DECOUPLER |                                                |                                                 | EXPLODE              |
| SUSPENSION          |                                                | SPRING                                          |                      |
| SPINNING            |                                                | SPEED                                           |                      |
| GRABBER             | GRAB STATIC<br> GRAB STATIC ONLY<br> AUTO-GRAB |                                                 | DETACH               |
| SIMPLE ROPE + WINCH | START UNWOUND                                  | SPEED                                           | WIND<br>  UNWIND     |
| CIRCULAR SAW        |                                                | SPEED                                           |                      |
| DRILL               | AUTOMATIC<br> TOGGLE MODE<br> AUTO-BRAKE       | SPEED                                           |                      |
| CANNON              |                                                | POWER                                           | SHOOT                |
| SHRAPNEL CANNON     |                                                |                                                 | SHOOT                |
| FLAMETHROWER        | HOLD TO FIRE                                   | RANGE                                           | IGNITE               |
| WATER CANNON        | HOLD TO SHOOT                                  | POWER                                           | SHOOT                |
| ROCKET              |                                                | FLIGHT DURATION<br> THRUST<br> EXPLOSIVE CHARGE | LAUNCH               |
| FLYING SPIRAL       | AUTOMATIC<br> TOGGLE MODE<br> REVERSE          | FLYING SPEED                                    | SPIN                 |
| BALLOON             |                                                | BUOYANCY<br> STRING LENGTH                      |                      |
| BALLAST             |                                                | MASS                                            |                      |
