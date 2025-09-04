
using UnityEngine;
using System.Collections.Generic;

public static class BoardGenerator
{
    public static void Generate(BoardModel board, BoardConfig cfg, ItemBag bag)
    {
        int cellCount = board.cells.Length;
        var visibleCaps = new int[cellCount];
        var waitingCaps = new int[cellCount];
        for (int i = 0; i < cellCount; i++)
        {
            visibleCaps[i] = Mathf.Clamp(cfg.maxInitialVisible, 0, 3);
            waitingCaps[i] = cfg.waitingMaxDepth;
        }

        List<int> pairableCells = new List<int>();
        for (int i = 0; i < cellCount; i++)
            if (visibleCaps[i] >= 2) pairableCells.Add(i);

        int totalThirdSpots = 0;
        for (int i = 0; i < cellCount; i++)
            totalThirdSpots += visibleCaps[i] + waitingCaps[i];

        int maxTriples = Mathf.Min(pairableCells.Count, totalThirdSpots / 3);
        if (maxTriples <= 0) return;

        Shuffle(pairableCells);
        int placed = 0;
        foreach (var pairCellIdx in pairableCells)
        {
            if (placed >= maxTriples) break;
            var cellA = board.cells[pairCellIdx];
            if (visibleCaps[pairCellIdx] < 2) continue;

            int itemId = cfg.itemDatabase.RandomId();
            List<int> visSlots = new List<int> { 0, 1, 2 };
            Shuffle(visSlots);
            cellA.PlaceVisibleAt(visSlots[0], itemId);
            cellA.PlaceVisibleAt(visSlots[1], itemId);
            visibleCaps[pairCellIdx] -= 2;

            if (!TryPlaceThird(board, cfg, itemId, visibleCaps, waitingCaps, true))
                TryPlaceThird(board, cfg, itemId, visibleCaps, waitingCaps, false);

            placed++;
        }
    }

    private static bool TryPlaceThird(BoardModel board, BoardConfig cfg, int itemId,
                                      int[] visibleCaps, int[] waitingCaps, bool preferVisible)
    {
        int cellCount = board.cells.Length;
        if (preferVisible)
        {
            for (int i = 0; i < cellCount; i++)
            {
                if (visibleCaps[i] <= 0) continue;
                var cell = board.cells[i];
                for (int s = 0; s < 3; s++)
                {
                    if (cell.visible[s] == -1)
                    {
                        cell.PlaceVisibleAt(s, itemId);
                        visibleCaps[i]--;
                        return true;
                    }
                }
            }
        }
        for (int i = 0; i < cellCount; i++)
        {
            if (waitingCaps[i] <= 0) continue;
            var cell = board.cells[i];
            if (cell.EnqueueWaiting(itemId, cfg.waitingMaxDepth))
            {
                waitingCaps[i]--;
                return true;
            }
        }
        if (!preferVisible)
        {
            for (int i = 0; i < cellCount; i++)
            {
                if (visibleCaps[i] <= 0) continue;
                var cell = board.cells[i];
                for (int s = 0; s < 3; s++)
                {
                    if (cell.visible[s] == -1)
                    {
                        cell.PlaceVisibleAt(s, itemId);
                        visibleCaps[i]--;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
