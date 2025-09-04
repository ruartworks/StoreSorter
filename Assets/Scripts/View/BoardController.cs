
using UnityEngine;
using System.Collections.Generic;

public class BoardController : MonoBehaviour
{
    [SerializeField] private BoardConfig config;
    [SerializeField] private ItemLibrary itemLibrary;
    [SerializeField] private CellView2D cellPrefab2D;
    [SerializeField] private Transform worldParent;
    [SerializeField] private float cellSize = 1.5f;

    [Header("Systems")]
    [SerializeField] private GoalManager goalManager;
    [SerializeField] private MatchAnimator matchAnimator;
    [SerializeField] private int levelIndex = 1;

    private BoardModel _board;
    private CellView2D[] _views;
    private ItemBag _bag;

    private void Start()
    {
        InitBoard();
        InitViews();

        _bag = new ItemBag(itemLibrary.Database);
        BoardGenerator.Generate(_board, config, _bag);

        var spawnCounts = CountSpawnedItems();
        if (goalManager != null)
            goalManager.InitGoalsFromSpawn(spawnCounts, levelIndex);

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

    private Dictionary<int, int> CountSpawnedItems()
    {
        var dict = new Dictionary<int, int>();
        foreach (var cell in _board.cells)
        {
            for (int i = 0; i < 3; i++)
            {
                if (cell.visible[i] != -1)
                {
                    int id = cell.visible[i];
                    dict[id] = dict.ContainsKey(id) ? dict[id] + 1 : 1;
                }
            }
            foreach (var id in cell.waitingQueue)
                dict[id] = dict.ContainsKey(id) ? dict[id] + 1 : 1;
        }
        return dict;
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

        CheckMatches(src, _views[fromCell]);
        if (toCell != fromCell) CheckMatches(dst, _views[toCell]);

        return true;
    }

    private void CheckMatches(CellModel cell, CellView2D view)
    {
        if (MatchSystem.TryGetMatch(cell, out int matchId))
        {
            var matchedSRs = new List<SpriteRenderer>(view.visibleSR);
            Vector3 targetWorld = goalManager != null ? goalManager.GetGoalWorldPosition(matchId) : view.transform.position;

            StartCoroutine(matchAnimator.AnimateMatchTo(matchedSRs, targetWorld, () =>
            {
                cell.ClearVisible();
                cell.PullFromWaitingToVisibleOnce();
                view.Bind(cell, itemLibrary.GetSprite);
                if (goalManager != null) goalManager.OnMatch(matchId);
                CheckMatches(cell, view);
            }));
        }
        else
        {
            view.Bind(cell, itemLibrary.GetSprite);
        }
    }

    private void RefreshAllViews()
    {
        for (int i = 0; i < _views.Length; i++)
        {
            _views[i].Bind(_board.cells[i], itemLibrary.GetSprite);
        }
    }

}
