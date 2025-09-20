# Tileset

## Overview

- Asteroid tileset is rendered per cell quadrant
- Top row - top left/right quadrants
- Bottom row - bottom left/right quadrants
- Both rows alternate between left/right

## Tessalation

- Top edge of top row <> Bottom edge of bottom row (including different tilesets)
- Each diagonal has two corresponding neighbors

## Adding a new tileset

1. Start with top square - ensure tesselation with base rock and with itself
2. Front face - should tesselate with itself on top and itself + base either side
  - Easier to do this in a separate file
3. Sides - tesselate with itself on either side + edge
4. Corners