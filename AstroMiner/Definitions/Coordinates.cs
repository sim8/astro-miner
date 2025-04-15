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
        public const int UnderRocksX = 10;
        public const int UnderRocksY = 20;

        public static readonly (int x, int y) MinerHomeStartPosCenter = (5, 14);
        public static readonly (int x, int y) PlayerHomeStartPos = (7, 17);

        public static readonly (int x, int y) HomeToRigRoomPortal = (12, 20);
        public static readonly (int x, int y) RigToomToHomePortal = (4, 7);
    }

    public static class Px
    {
        public const int LaunchLight1X = 215;
        public const int LaunchLight2X = 221;
        public const int LaunchLight3X = 227;
        public const int LaunchLightsY = 417;

        public const int LaunchPadsX = 122;
        public const int LaunchPadFrontStartY = 446;
        public const int LaunchPadRearStartY = 415;
    }
}