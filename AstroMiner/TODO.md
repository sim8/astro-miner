# NEXT STEPS FOR HOME WORLD (temp)

- Have actual in/out component. Do we need miner reference?
- Verrrry basic launch animation

# TODO

## HOME WORLD

### Basics

- Setup hardcoded grid + entities?
- Launch/base cycle
- Interior spaces

###

- Launch animation

## RISK/REWARD IMPROVEMENTS

- Explosive cells
    - Almost invisible if not surfaced yet?
    - See how that feels - could add timed random exploding

## Gameplay - asteroid

- lower level
    - darker, fuzzy minimap?
    - drive up / down?
    - Much more dangerous / better loot
    - Collapsing floor
- Mystery box cell - similar design to explosive?
- Different "types" of asteroid
    - Higher concentration of lava/holes/explosive rock
    - Different skins - ice?
- More gems!
    - Base emerald/sapphire etc
    - Gem clusters? Worth 10 or more?
        - Could be a good "upsell" into better asteroids

## Gameplay - other

- In/out of miner needs balancing
    - Out of miner - no cell clear
- variable yield? skill based?
- Tools
    - grapple? human only!
    - rpg
    - depth charge - in hole. but only if not exploding!
    - big explosive
        - Display timer - 10 seconds?
        - Big white flash
    - scanner
    - boost
- fuel?
- Upgrades
    - speed
    - drill speed
    - color/design
    - tools
    - Different "base" - hovercraft?
- NPC helpers
    - Recruit between digs
    - Give tasks
        - Focus on x mineral
        - Follow in miner?
    - Unique dialogue
    - End up in hospital too!

## Quality

- Better handling of in/out of miner + positioning
- Better handling of world checks - very scattered atm
- Player placement when exiting
- Test on Xbox?
    - Is state serializable for things like saving?
- Add remainder of font data - pick a different font?
- Update collision detection - instead of rejecting moves, is there an inbetween?
- Add another base class between interiors/home?

## Rendering / art

- Fix zoom in/out!
    - Have another go at pixel perfection
- Drilling animation
- Darker overlay near center
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

- Bit more of a release plan
- Learn shaders!
