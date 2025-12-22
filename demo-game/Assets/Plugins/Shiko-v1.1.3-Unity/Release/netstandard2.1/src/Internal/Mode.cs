namespace Shiko.Internal.Mode
{
    public enum Mode
    {
        TEST,    // Mode for testing
        DEV,     // Mode for debug
        RELEASE  // Mode for release
    }

    internal static class ModeManager
    {
        // Singleton
        private static Mode self = Mode.DEV;

        public static Mode GetMode()
        {
            return self;
        }

        public static void SetMode(Mode mode)
        {
            switch (mode)
            {
                case Mode.TEST:
                case Mode.DEV:
                case Mode.RELEASE:
                    self = mode;
                    break;
            }
        }
    }

}