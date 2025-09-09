using Microsoft.Xna.Framework;

namespace AstroMiner.Definitions;

public static class Coordinates
{
    public static class Grid
    {
        public const int OizusWidth = 40;
        public const int OizusHeight = 30;

        public const int ParallaxMountains1Y = 10;
        public const int ParallaxMountains2Y = -40;
        public const int ParallaxMountains3Y = -90;
        public const int UnderRocksX = 1;
        public const int UnderRocksY = 12;

        public static readonly Vector2 MinerShipStartPosCenter = new Vector2(33.5f, 6f);
        public static readonly (int x, int y) PlayerHomeStartPos = (7, 17);

        public static readonly (int x, int y) HomeToRigRoomPortal = (12, 20);
        public static readonly (int x, int y) RigToomToHomePortal = (4, 7);

        public static readonly (int x, int y) HomeToMinExPortal = (21, 18);
        public static readonly (int x, int y) MinExToHomePortal = (3, 7);

        public static readonly (int x, int y) KrevikToShipDownstairsPortal = (11, 6);
        public static readonly (int x, int y) ShipDownstairsToKrevikPortal = (28, 3);

        public static readonly (int x, int y) ShipDownstairsToShipUpstairsPortal = (24, 4);
        public static readonly (int x, int y) ShipUpstairsToShipDownstairsPortal = (24, 4);
    }
}