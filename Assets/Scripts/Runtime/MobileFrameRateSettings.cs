using UnityEngine;

namespace JannahGarden.Runtime
{
    /// <summary>
    /// Sets a consistent frame target for Android. Unity otherwise defaults to 30 FPS on mobile.
    /// </summary>
    internal static class MobileFrameRateSettings
    {
        private const int TargetFramesPerSecond = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ConfigureFrameRate()
        {
            Application.targetFrameRate = TargetFramesPerSecond;
        }
    }
}
