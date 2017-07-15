# Marimo
A game project called Marimo.

## Unity Guidelines
### Quick Start
1. We're using Unity 2017.1, which you can download here:
[https://store.unity.com/download?ref=update](https://store.unity.com/download?ref=update)
You can probably leave the default selection options when installing. This game will be PC only (at first), and we're not using any built-in textures/sprites or anything like that.
2. Install Tiled from here (most recent version should be fine... we're currently on 0.18.2):
[https://thorbjorn.itch.io/tiled](https://thorbjorn.itch.io/tiled)
3. And Tiled2Unity, which we'll use to import Tiled files to Unity.
[http://www.seanba.com/tiled2unity](http://www.seanba.com/tiled2unity)

When opening the project in Unity, here are a few tips:
1. Start Unity, click Open, then browse to the Marimo folder to open the project. Unity doesn't have a specific project file.
2. To actually see things working, you'll need to open a "Scene". In the Project pane, look for Assets > Scenes, and double click the scene "Prototype".
3. Click the Play button at the top to run the Prototype scene.
4. To play with animations, in the Hierarchy pane, expand Robot and select Body. Open the Animation pane (not Animator pane) and choose an animation in the tiny drop down menu.
5. Now select Robot in the Hierarchy pane, and check out the Inspector pane. Lots of custom variables associated with the RobotController script, RigidBody properties, etc. to play around with.
5. All C# scripts are in Assets > Scripts, which you can find in the Project pane.

### Importing New Sprites
1. Import the sprites by dragging them into Unity
2. Select the sprites in Unity's **Project** pane
3. In the **Inspector** pane, set the following:
    * **Sprite Mode** = Single
    * **Pixels Per Unit** = 6
    * **Pivot** = Bottom Left
    * **Filter Mode** = Point (no filter)
    * **Compression** = None
4. Press **Apply**

## Story
Somewhere in space, there exists a civilization that thrives by finding resource-rich planets and profiting from them. Corporations scour the known universe looking for candidate planets, and deploy large machines that can rapidly reshape a planet's landscape, in preparation for mechanized mining colonies to be set up on the surface. These terraforming machines are monstrous, and for the most part self-sustaining, constantly being serviced and repaired by small maintenance robots that live within.

One particular machine has been reforming a green, swamp-covered planet, rich with plant life. Our game takes place entirely inside this machine. You play as two characters:
1. An unnamed, mildly defective maintenance robot. Our robot is programmed to fix plumbing issues within the machine, which has thousands of water pipes running throughout. 
2. A moss ball (marimo) named Muckle. Muckle is native to this planet, and somehow got sucked up into the machine's water system as it was destroying his home.

Muckle lives in the pipes that run throughout the machine. Robot will develop a fondness for Muckle, and go against his programming to help Muckle find a way out of the machine. 

Muckle swims well, and water pipes run everywhere. But sometimes the water is too hot. Sometimes the current is flowing in the wrong direction. Sometimes the pipes are blocked.

Robot, on the other hand, moves over land well. He cannot swim, but he has tools. He can also telescope to reach high places with his tools. At the start, Robot will have a wrench for turning valves, and a snake for cleaning out pipes.

Together, Muckle and Robot must work together to solve puzzles and help each other get from one area to the next. As they explore the machine, they will move upward through its various levels. From one level to the next, they will find not only new challenges, but new ecosystems. The further toward the top, the stranger the machine becomes. This particular machine has been infested by more than just Muckle! Plants and creatures grow quickly on this planet. Other resilient plantlife have taken up residence within the machine. The biological and the industrial have become intertwined. Dangerous obstacles are everywhere. 

In addition to swimming, Muckle has the alienlike ability to silently communicate with organic matter from a distance. This is pretty useless at first, but as Robot and Muckle make progress through the machine, Muckle's strange ability will be vital in helping Robot help him. Survival in this environment seems impossible, but all Muckle can do is keep moving.

## Gameplay
Marimo is a 2-D puzzle platformer, where a single player will switch between Robot and Muckle to explore areas and solve puzzles. 

Areas will be fairly large, and somewhat non-linear. But the goal will always be about getting both characters safely through to the next area.

At first, Muckle will move automatically. You'll control Robot, using his tools and telescoping to manipulate the pipes and other mechanics so that both he and Muckle can proceed. In level 1, for example, there will be conveyors moving  trash. Robot will find terminals that allow him to adjust the conveyors in various ways with his wrench. He'll also need to interact with pipes to clear a path for Muckle.

There will be combat, but Robot is not built for fighting, so these sequences will be more puzzle-like rather than just grinding through tons of enemies.

Later, the player will need to switch between controlling Robot and Muckle. The pipes will spit into multiple paths for Muckle to swim through. Muckle will need to use his telekenesis ability to communicate with nearby plantlife.

Robot is a plumber, and will begin the game with a wrench and a pipe snake. Along the way, he'll run into other types of maintenance robots such as welding robots and electrical robots. He will eventually unlock these abilities within himself.
