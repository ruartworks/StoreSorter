
using UnityEngine;
using System.Collections.Generic;

public class BoardController : MonoBehaviour
{
    [Header("Config & Library")]
    [SerializeField] private BoardConfig config;
    [SerializeField] private ItemLibrary itemLibrary;

    [Header("Prefabs & Layout")]
    [SerializeField] private CellView2D cellPrefab2D;
    [SerializeField] private Transform worldParent;
    [SerializeField] private float cellSize = 1.5f;


    [Header("Match")]
    [SerializeField] private MatchAnimator matchAnimator;
    [SerializeField] private GoalManager goalManager;
    [SerializeField] private int levelIndex = 1; // or wherever you store it

    private BoardModel _board;
    private CellView2D[] _views;
    private ItemBag _bag;

    private void Start()
    {
        InitBoard();
        InitViews();

        // Important: build goals UI before spawning & allowing matches
        if (goalManager != null)
            goalManager.InitGoals(levelIndex);

        _bag = new ItemBag(itemLibrary.Database);
        BoardGenerator.Generate(_board, config, _bag);

        RefreshAllViews();
    }

    private void InitBoard() => _board = new BoardModel(config.rows, config.cols);

    private void InitViews()
    {
        int count = config.rows * config.cols;
        _views = new CellView2D[count];
        for (int r = 0; r < config.rows; r++)
        {
            for (int c = 0; c < config.cols; c++)
            {
                int idx = _board.Index(r, c);
                var view = Instantiate(cellPrefab2D, worldParent);
                view.transform.localPosition = new Vector3(c * cellSize, -r * cellSize, 0f);
                view.Init(idx);
                _views[idx] = view;
            }
        }
    }
    public bool TryMoveVisible(int fromCell, int fromSlot, int toCell, int toSlot)
    {
        var src = _board.Cell(fromCell);
        var dst = _board.Cell(toCell);
        if (src.visible[fromSlot] == -1) return false;

        int moving = src.visible[fromSlot];
        int target = dst.visible[toSlot];
        src.visible[fromSlot] = -1; src.visibleCount--;

        if (target == -1)
        {
            dst.visible[toSlot] = moving; dst.visibleCount++;
        }
        else
        {
            dst.visible[toSlot] = moving;
            src.visible[fromSlot] = target; src.visibleCount++;
        }

        // check matches in both cells
        CheckMatches(src, _views[fromCell]);
        if (toCell != fromCell)
            CheckMatches(dst, _views[toCell]);

        return true;
    }

    public void RefreshAllViews()
    {
        for (int i = 0; i < _views.Length; i++)
            _views[i].Bind(_board.Cell(i), itemLibrary.GetSprite);
    }


    private void HandleMatch(CellModel cell, CellView2D view, int matchId)
    {
        var matchedSRs = new List<SpriteRenderer>(view.visibleSR);

        // Convert the goal UI icon to a WORLD position
        Vector3 targetWorld = (goalManager != null)
            ? goalManager.GetGoalWorldPosition(matchId)
            : view.transform.position; // safe fallback

        StartCoroutine(matchAnimator.AnimateMatchTo(matchedSRs, targetWorld, () =>
        {
            cell.ClearVisible();
            cell.PullFromWaitingToVisibleOnce();
            view.Bind(cell, itemLibrary.GetSprite);

            if (goalManager != null)
                goalManager.OnMatch(matchId, 3);

            // In case the pull created a new triple, resolve again
            CheckMatches(cell, view);
        }));
    }

    private void CheckMatches(CellModel cell, CellView2D view)
    {
        if (MatchSystem.TryGetMatch(cell, out int matchId))
        {
            var matchedSRs = new List<SpriteRenderer>(view.visibleSR);

            Vector3 targetWorld = goalManager != null
                ? goalManager.GetGoalWorldPosition(matchId)
                : view.transform.position;

            StartCoroutine(matchAnimator.AnimateMatchTo(matchedSRs, targetWorld, () =>
            {
                cell.ClearVisible();
                cell.PullFromWaitingToVisibleOnce();
                view.Bind(cell, itemLibrary.GetSprite);
                if (goalManager != null) goalManager.OnMatch(matchId, 3);

                // cascade check in case a new triple formed
                CheckMatches(cell, view);
            }));
        }
        else
        {
            view.Bind(cell, itemLibrary.GetSprite);
        }
    }



}
