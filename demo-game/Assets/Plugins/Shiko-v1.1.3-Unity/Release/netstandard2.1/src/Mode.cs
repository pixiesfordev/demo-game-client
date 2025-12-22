using Shiko.Internal.Mode;

namespace Shiko
{
    public static class ClientMode
    {
        public static readonly Mode TestMode = Mode.TEST;
        public static readonly Mode DevMode = Mode.DEV;
        public static readonly Mode ReleaseMode = Mode.RELEASE;

        // Interface of ModeManager
        public static void SetMode(Mode mode)
        {
            ModeManager.SetMode(mode);
        }
    }
}
