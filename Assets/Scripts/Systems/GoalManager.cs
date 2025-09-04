using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Goal
{
    public int itemId;
    public int targetCount;
    public int currentCount;
    public bool IsComplete => currentCount >= targetCount;
}

public class GoalManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private BoardConfig config;
    [SerializeField] private ItemLibrary itemLibrary;

    [Header("UI")]
    [SerializeField] private Transform goalsUIParent;   // container under Canvas
    [SerializeField] private GoalView goalPrefab;       // prefab with GoalView
    [SerializeField] private Camera uiCamera;           // Canvas = Screen Space - Camera? assign it. Overlay? leave null.
    [SerializeField] private Camera worldCamera;        // Main Camera

    [Header("Optional Fallback")]
    [SerializeField] private RectTransform genericGoalTarget; // optional “chest” icon

    private readonly List<Goal> goals = new();
    private readonly List<GoalView> goalViews = new();

    public void InitGoals(int levelIndex)
    {
        goals.Clear();
        goalViews.Clear();

        for (int i = goalsUIParent.childCount - 1; i >= 0; i--)
            Destroy(goalsUIParent.GetChild(i).gameObject);

        int goalCount = Mathf.Clamp(1 + levelIndex / 15, 1, 3);
        int baseTarget = 3 + Mathf.Min(levelIndex, 50) / 3;

        var used = new HashSet<int>();
        for (int i = 0; i < goalCount; i++)
        {
            int itemId = config.itemDatabase.RandomId();
            int guard = 50;
            while (used.Contains(itemId) && guard-- > 0)
                itemId = config.itemDatabase.RandomId();
            used.Add(itemId);

            int target = baseTarget + i;
            goals.Add(new Goal { itemId = itemId, targetCount = target, currentCount = 0 });

            var view = Instantiate(goalPrefab, goalsUIParent);
            view.Bind(itemLibrary.GetSprite(itemId), 0, target);
            goalViews.Add(view);
        }
    }

    public void OnMatch(int itemId, int count)
    {
        for (int i = 0; i < goals.Count; i++)
        {
            if (goals[i].itemId == itemId)
            {
                goals[i].currentCount = Mathf.Min(goals[i].currentCount + count, goals[i].targetCount);
                goalViews[i].UpdateProgress(goals[i].currentCount, goals[i].targetCount);
            }
        }
        CheckCompletion();
    }

    public Vector3 GetGoalWorldPosition(int itemId)
    {
        for (int i = 0; i < goals.Count; i++)
        {
            if (goals[i].itemId == itemId)
                return UIToWorld(goalViews[i].IconRect);
        }
        if (genericGoalTarget != null) return UIToWorld(genericGoalTarget);
        return worldCamera != null ? worldCamera.transform.position : Vector3.zero;
    }

    private Vector3 UIToWorld(RectTransform uiRect)
    {
        if (worldCamera == null) worldCamera = Camera.main;
        // Convert UI element position to screen point, then to world
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCamera, uiRect.position);
        Vector3 world = worldCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, Mathf.Abs(worldCamera.transform.position.z)));
        world.z = 0f;
        return world;
    }

    private void CheckCompletion()
    {
        foreach (var g in goals)
            if (!g.IsComplete) return;

        Debug.Log("Level Complete!");
        // TODO: level complete flow here
    }
}
