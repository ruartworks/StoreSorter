
using UnityEngine;

[CreateAssetMenu(fileName = "BoardConfig", menuName = "Game/Board Config")]
public class BoardConfig : ScriptableObject
{
    [Min(1)] public int rows = 4;
    [Min(1)] public int cols = 4;

    [Header("Spawn")]
    [Tooltip("Max visible items per cell at start (0-3). Typically 0-2 to leave space.")]
    [Range(0,3)] public int maxInitialVisible = 2;
    [Tooltip("Max waiting items per cell at start (0-3).")]
    [Range(0,3)] public int maxInitialWaiting = 3;

    [Tooltip("Ensure ALL spawned items participate in a potential triple (no singletons).")]
    public bool ensureAllTriples = true;

    [Tooltip("All items that can spawn.")]
    public ItemDatabase itemDatabase;

    [Header("Haptics (Android)")]
    public bool enableHaptics = true;
    [Range(5, 100)] public int matchHapticDurationMs = 30;
    [Range(1, 255)] public int matchHapticAmplitude = 128;
}
