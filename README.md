# SphereWorld

This project started with an idea I had to try and explore a 3D world of various spherical, euclidean, and hyperbolic faces that arose
from a cubical model where every vertex would have 3, 4, or 5 faces adjoining.

The rendering and modeling, done in OpenGL, was also a project for me to learn a bit more about how this might perform using C#.net.

The current state of this is sort of a "maze game", where the goal is to get from the dirt tile, to the strawberry tile.  It is a pretty boring game, but I'm quite happy with the visual effects and the interactive motion (careful that you don't get too seasick, on this matter... at higher movement speeds (adjust the MainWindow.MovementSpeed variable), it can get a little rollercoastery) due to there being no true "up", and the rotational aspect of always sticking to the surface as you race around.

The world model itself is created from a 256x256x256 bit vector to define the geometry, which is seeded then generated to create a connected set of cubes within the constraints I had set out to achieve (3, 4, 5 faces at each vertex.)

## Running the application

I typically just run this from Visual Studio within the debugger.

![Screen Shot](./screenshot.jpg?raw=true "Optional Title")

When running the application, the following input controls are used:

| key | description |
| - | - |
| ESC | exit the program. |
| Space | display the text display and approximate rendering speeds. |
| Tab | toggle from filled to polygon outline of the geometry. |
| 1 - 5 | set the catmull clark subdivision level from cubes (1) to smooth (5). |
| F11 | toggle full screen mode |
| arrow keys or ASDF | move or straff the current viewpoint along the surface. |
| mouse | orient the view. |

Note that when moving around, the view is pinned to the surface of the geometry.

## OpenGL performance

Performance will depend on the specific graphics card that is used (the pipeline is definitely GPU bound).

On my system (i7 @ 2.30GHz 2.30 GHz / RTX 3080 Laptop GPU), it's pushing about 300M-350M triangles per second (1.7M/frame) on a level 5 Catmull Clark subdivision model (723K vertices) build around the original random model containing 800 cubes.

## Shaders

Two shaders are utilized (each one, a pair with a vertex shader, and a fragment shader):

| shader | description |
| - | - |
| ScreenText | Renders the console display - this was a fun one to write. |
| SimpleColor | Renders the model faces using texture(s) and a minor adjustment for "lighting". |

