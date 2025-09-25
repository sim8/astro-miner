# Tileset

Walls + floor using two different systems:

## Walls

### Overview

- Asteroid tileset is rendered per cell quadrant
- Top row - top left/right quadrants
- Bottom row - bottom left/right quadrants
- Both rows alternate between left/right

### Tessalation

- Top edge of top row <> Bottom edge of bottom row (including different tilesets)
- Each diagonal has two corresponding neighbors

### Adding a new tileset

1. Start with top square - ensure tesselation with base rock and with itself
2. Front face - should tesselate with itself on top and itself + base either side
  - Easier to do this in a separate file
3. Sides - tesselate with itself on either side + edge
4. Corners

## Floors

TODO is this overcomplicated? Wondering if we could do regular dual grid (not quadrants)
perhaps w/ underlaying lava etc

Based heavily on Dual Grid System https://x.com/OskSta/status/1448248658865049605
High level steps of rendering:
1. Iterate each cell (back to front) and each corner of the cell (back to front)
2. Find the tile to render based on corner's 3 neighbors
3. Render single floor quadrant (corner of cell)
- Only quadrant rendered as opposed to whole tile, as neighboring cell
might be different type