
using UnityEngine;

public static class Haptics
{
    public static void MatchBuzz(int durationMs = 30, int amplitude = 128)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var context = currentActivity.Call<AndroidJavaObject>("getApplicationContext"))
            using (var vibrator = context.Call<AndroidJavaObject>("getSystemService", "vibrator"))
            {
                if (vibrator == null) return;

                using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                {
                    int sdkInt = version.GetStatic<int>("SDK_INT");
                    if (sdkInt >= 26)
                    {
                        using (var vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                        {
                            amplitude = Mathf.Clamp(amplitude, 1, 255);
                            using (var effect = vibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", durationMs, amplitude))
                            {
                                vibrator.Call("vibrate", effect);
                            }
                        }
                    }
                    else
                    {
                        vibrator.Call("vibrate", (long)durationMs);
                    }
                }
            }
        }
        catch { }
#endif
    }
}
