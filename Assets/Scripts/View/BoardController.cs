
using UnityEngine;

public class BoardController : MonoBehaviour
{
    [Header("Config & Library")]
    [SerializeField] private BoardConfig config;
    [SerializeField] private ItemLibrary itemLibrary;

    [Header("Prefabs & Layout")]
    [SerializeField] private CellView2D cellPrefab2D;
    [SerializeField] private Transform worldParent;
    [SerializeField] private float cellSize = 1.5f;

    private BoardModel _board;
    private CellView2D[] _views;
    private ItemBag _bag;

    private void Start()
    {
        InitBoard();
        InitViews();

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
        if (fromSlot < 0 || fromSlot >= 3 || toSlot < 0 || toSlot >= 3) return false;
        if (src.visible[fromSlot] == -1) return false;

        int moving = src.visible[fromSlot];
        int target = dst.visible[toSlot];

        // Move or swap
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

        // Fully resolve both cells (cascade clears and pulls)
        bool a = MatchSystem.ResolveCellFully(src);
        bool b = MatchSystem.ResolveCellFully(dst);

        if ((a || b) && config.enableHaptics)
            Haptics.MatchBuzz(config.matchHapticDurationMs, config.matchHapticAmplitude);

        // Refresh views
        _views[fromCell].Bind(src, itemLibrary.GetSprite);
        if (toCell != fromCell) _views[toCell].Bind(dst, itemLibrary.GetSprite);

        return true;
    }

    public void RefreshAllViews()
    {
        for (int i = 0; i < _views.Length; i++)
            _views[i].Bind(_board.Cell(i), itemLibrary.GetSprite);
    }
}
