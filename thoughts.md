# Gameplay
- The player is a sharp object that can move around the map by 'slingshotting' themselves
- The player can cut through the bubbles that are on the map
- Walls are not penetrable
- Win state?
- Failure state?

# Levels
- Start with one level
- If time permits, add other levels

# AI
- There are two types of Bubbles on the map, manually placed ones that are present at level start and generated ones that are spawned by a bubble spawner. The generated bubbles leave the vicinity of the spawner and go on one of the predefined patrol routes. For all bubble types, if they are close to the player (within a certain radius), they will start seeking the player.
- After entering seek radius, the bubble could wait for a short time and then accelerate towards the player at speed * chase mult
- Before exploding the bubble should have a small delay

# Tutorial
- Just text on screen

# Dev
- Bubble Settings/type should be settable on the spawner.
- Maybe have an array of spawnable types on the spawner?

# Collision layers
1: Player
  Interacts with: 4, 9
5: Player Tip
  Interacts with: 2
2: Bubbles
  Interacts with: 1, 5
4: Walls
  Interacts with: 1, 2
9: Explosion
  Interacts with: 1
