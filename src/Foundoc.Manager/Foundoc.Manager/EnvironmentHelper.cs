namespace Foundoc.Manager
{
    public class EnvironmentHelper
    {
        public static bool IsDevelopment()
        {
#if DEBUG
            return true;
#endif
            return false;
        }
    }
}