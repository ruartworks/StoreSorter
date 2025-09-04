
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Goal
{
    public int itemId;
    public int remaining; // how many triples still needed
    public bool IsComplete => remaining <= 0;
}

public class GoalManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private BoardConfig config;
    [SerializeField] private ItemLibrary itemLibrary;

    [Header("UI")]
    [SerializeField] private Transform goalsUIParent;
    [SerializeField] private GoalView goalPrefab;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private Camera worldCamera;

    [Header("World Mapping")]
    [SerializeField] private Transform worldPlane; // use your board root
    [SerializeField] private float fallbackPlaneZ = 0f;

    [Header("Optional Fallback Target")]
    [SerializeField] private RectTransform genericGoalTarget;

    private readonly List<Goal> goals = new();
    private readonly List<GoalView> goalViews = new();
    public event System.Action AllGoalsComplete;

    public void InitGoalsFromSpawn(Dictionary<int,int> spawnCounts, int levelIndex)
    {
        goals.Clear();
        goalViews.Clear();
        for (int i = goalsUIParent.childCount - 1; i >= 0; i--)
            Destroy(goalsUIParent.GetChild(i).gameObject);

        int desiredTotalMatches = Mathf.Clamp(1 + levelIndex / 12, 1, 6);
        int goalCount = Mathf.Clamp(1 + levelIndex / 18, 1, 3);

        var candidates = new List<int>();
        foreach (var kv in spawnCounts) if (kv.Value >= 3) candidates.Add(kv.Key);
        if (candidates.Count == 0) foreach (var kv in spawnCounts) candidates.Add(kv.Key);

        for (int i = 0; i < candidates.Count; i++)
        {
            int j = Random.Range(i, candidates.Count);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        goalCount = Mathf.Min(goalCount, candidates.Count);
        int remainingPool = desiredTotalMatches;

        for (int i = 0; i < goalCount; i++)
        {
            int itemId = candidates[i];
            int maxTriples = spawnCounts[itemId] / 3;
            if (maxTriples <= 0) maxTriples = 1;

            int alloc = Mathf.Min(Mathf.Max(1, desiredTotalMatches / goalCount), maxTriples);
            alloc = Mathf.Min(alloc, remainingPool);
            if (i == goalCount - 1) alloc = Mathf.Max(1, remainingPool);

            remainingPool -= alloc;

            goals.Add(new Goal { itemId = itemId, remaining = alloc });

            var view = Instantiate(goalPrefab, goalsUIParent);
            view.Bind(itemLibrary.GetSprite(itemId), alloc);
            goalViews.Add(view);
        }

        int guard = 32;
        while (remainingPool > 0 && guard-- > 0)
        {
            for (int i = 0; i < goals.Count && remainingPool > 0; i++)
            {
                int itemId = goals[i].itemId;
                int maxTriples = spawnCounts[itemId] / 3;
                if (goals[i].remaining < maxTriples)
                {
                    goals[i].remaining++;
                    remainingPool--;
                    goalViews[i].UpdateRemaining(goals[i].remaining);
                }
            }
        }
    }

    public void OnMatch(int itemId)
    {
        for (int i = 0; i < goals.Count; i++)
        {
            if (goals[i].itemId == itemId && goals[i].remaining > 0)
            {
                goals[i].remaining--;
                goalViews[i].UpdateRemaining(goals[i].remaining);
            }
        }
        CheckCompletion();
    }

    public Vector3 GetGoalWorldPosition(int itemId)
    {
        for (int i = 0; i < goals.Count; i++)
            if (goals[i].itemId == itemId)
                return UIToWorld(goalViews[i].IconRect);

        if (genericGoalTarget != null) return UIToWorld(genericGoalTarget);
        return worldCamera != null ? worldCamera.transform.position : Vector3.zero;
    }

    private Vector3 UIToWorld(RectTransform uiRect)
    {
        if (worldCamera == null) worldCamera = Camera.main;
        float planeZ = worldPlane != null ? worldPlane.position.z : fallbackPlaneZ;
        float dist = Mathf.Abs(planeZ - worldCamera.transform.position.z);
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCamera, uiRect.position);
        var pos = worldCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, dist));
        pos.z = planeZ;
        return pos;
    }

    private void CheckCompletion()
    {
        foreach (var g in goals)
            if (!g.IsComplete) return;
        Debug.Log("Level Complete!");
        AllGoalsComplete?.Invoke();
    }
}
