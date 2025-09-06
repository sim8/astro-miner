# Gem Runners - Asteroid mining mechanics

The main gameplay loop of Gem Runners is asteroid mining. This is where the player spends the majority of time and is where they find gems to be sold for better equipment or to advance the story.

The player is droppped on the left most edge of the asteroid. There is a 5 minute counter visible to the player, after which time the asteroid burns up. The player must get off the asteroid via any of its edges before the itme runs out, otherwise they lose everything they mined plus some money.

## Asteroid composition

Asteroids are procedurally generated grids (roughly 150x150) and the player starts on the left hand side of them.

Each grid cell has a type and a floor type. The types are:

- Rock: can be drilled quickly to remove
- Loose rock: can be drilled instantly to remove
- Solid rock: can only be removed with explosives
- Gems: can be drilled to yield gems. Various types such as diamond, ruby etc of varying value + drill time.
- Explosive rock: if disturbed will explode, causing harm to any nearby entities and potentially disturbing other cells. At present these are only triggered by attempting to drill them or by nearby explosions.

Floor types:
- Floor
- Space
- Lava: hurts any entities over lava for > 2 seconds.
- Lava cracks: floor that if disturbed will collapse into lava. Typically spawning around lava, and may randomly collapse when near the player, also collapsing any adjoining lava cracks in a cascading manner.

Asteroids have 3 layers (concentric circles on the map). Note the center of the circle leans to the right of the grid, giving the player the option to drill straight through the middle or try to go round:
- Crust - This is where the player spawns, and must return to to jump off the asteroid when the time runs out. The general make up here is rock, loose rock, and some low-value gems.
- Mantle - Characterised by lots of lava, lava cracks, some solid rock, and moderately valuable gems.
- Core - Lots of solid rock, explosive rock, and most valuable gems.

## Player controls

The player spawns in a mining vehicle and has the option to leave it any time. The mining vehicle is slightly bigger than the player (taking up 2x2) so is slightly less agile, but has a slightly higher top speed. The mining vehicle currently can only drill but I'm exploring adding different attachments later in the game.

The player has 10 spaces in their inventory and can quickly toggle between them to use different tools. The only ones applicable for mining currently are a drill (equivalent to the mining vehicle drill), a limited number of dynamite, and health potions.
