
using UnityEngine;

[CreateAssetMenu(fileName = "BoardConfig", menuName = "Game/Board Config")]
public class BoardConfig : ScriptableObject
{
    [Min(1)] public int rows = 4;
    [Min(1)] public int cols = 4;

    [Header("Spawn")]
    [Range(0,3)] public int maxInitialVisible = 2;
    [Range(1,3)] public int waitingVisibleSlots = 3;
    [Min(0)] public int waitingMaxDepth = 20;

    public ItemDatabase itemDatabase;

    [Header("Haptics (Android)")]
    public bool enableHaptics = true;
    [Range(5,100)] public int matchHapticDurationMs = 30;
    [Range(1,255)] public int matchHapticAmplitude = 128;
}
