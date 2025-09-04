using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private GoalManager goalManager;
    [SerializeField] private float delayBeforeLoad = 0.6f;
    [SerializeField] private bool loopAtEnd = true;

    private void Awake()
    {
        if (goalManager == null) goalManager = FindFirstObjectByType<GoalManager>();
        if (goalManager != null) goalManager.AllGoalsComplete += OnAllGoalsComplete;
    }

    private void OnDestroy()
    {
        if (goalManager != null) goalManager.AllGoalsComplete -= OnAllGoalsComplete;
    }

    private void OnAllGoalsComplete()
    {
        Invoke(nameof(LoadNextScene), delayBeforeLoad);
    }

    private void LoadNextScene()
    {
        var cur = SceneManager.GetActiveScene().buildIndex;
        int next = cur + 1;

        if (next >= SceneManager.sceneCountInBuildSettings)
        {
            if (!loopAtEnd) return;
            next = 0;
        }

        SceneManager.LoadScene(next, LoadSceneMode.Single);
    }
}
