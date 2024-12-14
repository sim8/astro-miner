# TODO

## Gameplay - asteroid

- only have solid rock near center
- swap core range for sliding range?
- ~~interior floor - shouldn't be totally solid~~
- Explosive pieces + chain reaction
    - OR gas/lava holes/wells which bubble over
        - lava - turn into rock
    - holes can appear under rock!
- ~~Bigger asteroid~~
- Mystery box cell - similar design to explosive?
- IS there a risk/reward element?
- variable yield? skill based?
- Make it clearer where center is - clearer sections?
    - Should hard rock only start appearing at certain range?
- Besides usual randomness, should we have different "types" of asteroid?
    - Asteroids don't really have characteristics

## Gameplay - other

- In/out of miner needs balancing
    - Miner doesn't mine?
    - Better protection in miner?
    - ~~Miner 2x2 (3x3?) - harder to navigate?~~
- Tools
    - grapple? human only!
    - rpg
    - depth charge - in hole. but only if not exploding!
    - big explosive
    - scanner
- fuel?
- Upgrades
    - speed
    - drill speed
    - color/design
    - tools
- Endgame state
    - Timer runs out
    - Drop off edge
- player / miner health
- Add zoom
- NPC helpers
    - Recruit between digs
    - Give tasks
        - Focus on x mineral
        - Follow in miner?
    - Unique dialogue
    - End up in hospital too!

## Quality

- ~~Easy grid view for ast gen + new entry point~~
- Player placement when exiting
- Renderer inheritance?
- Control mapping
- Test on Xbox?

## Rendering / art

- Change palette colors - blue shadows
- ~~Improve direction light art~~
- Additive light pass for glares (bright lights, explosions)
- Player animation
- Ground tileset
- Redo rock tileset - 1 central piece per block
    - How do gems work?
- Clouds / background animation
- Drilling animation
- Darker overlay near center
- Better gems
- Gem entities (collectable)

## Sound design

- Drill sound - powerup / drill / powerdown?
    - Higher pitched for player?
- Reverb - higher when more central
- Wind noise - higher when nearer edge
- Music?
- Rumbling for eruption?

## Performance

- Replace pos / boxsize with rectangle - avoid creating new
- Move tileset stuff to update loop + cache?

## Meta

- Do I release this?
