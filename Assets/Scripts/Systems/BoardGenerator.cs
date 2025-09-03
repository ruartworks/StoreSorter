using UnityEngine;
using System.Collections.Generic;

public static class BoardGenerator
{
    public static void Generate(BoardModel board, BoardConfig cfg, ItemBag bag)
    {
        var visibleCaps = new int[board.cells.Length];
        var waitingCaps = new int[board.cells.Length];
        for (int i = 0; i < board.cells.Length; i++)
        {
            visibleCaps[i] = Mathf.Clamp(cfg.maxInitialVisible, 0, 3);
            waitingCaps[i] = Mathf.Clamp(cfg.maxInitialWaiting, 0, 3);
        }

        List<int> pairableCells = new List<int>();
        for (int i = 0; i < board.cells.Length; i++)
            if (visibleCaps[i] >= 2) pairableCells.Add(i);

        List<(int cell, bool isVisible)> thirdSpots = new List<(int, bool)>();
        for (int i = 0; i < board.cells.Length; i++)
        {
            for (int v = 0; v < visibleCaps[i]; v++) thirdSpots.Add((i, true));
            for (int w = 0; w < waitingCaps[i]; w++) thirdSpots.Add((i, false));
        }

        int maxTriplesByPairs = pairableCells.Count;
        int totalThirdSpots = thirdSpots.Count;
        int maxTriplesByThirds = totalThirdSpots - (2 * maxTriplesByPairs) > 0 
            ? Mathf.Min(maxTriplesByPairs, (totalThirdSpots) / 3) 
            : Mathf.Min(maxTriplesByPairs, totalThirdSpots / 3);

        int triplesToPlace = Mathf.Min(maxTriplesByPairs, maxTriplesByThirds);
        if (triplesToPlace <= 0) return;

        Shuffle(pairableCells);
        Shuffle(thirdSpots);

        int thirdIndex = 0;
        for (int t = 0; t < triplesToPlace; t++)
        {
            int cellIdx = pairableCells[t];
            var cell = board.cells[cellIdx];

            int itemId = cfg.itemDatabase.RandomId();

            List<int> visSlots = new List<int> { 0, 1, 2 };
            Shuffle(visSlots);
            for (int k = 0; k < 2; k++)
                cell.PlaceVisibleAt(visSlots[k], itemId);
            visibleCaps[cellIdx] -= 2;

            (int cell, bool isVisible) spot = (-1, false);
            for (; thirdIndex < thirdSpots.Count; thirdIndex++)
            {
                var s = thirdSpots[thirdIndex];
                if (s.cell == cellIdx && s.isVisible && visibleCaps[cellIdx] <= 0) continue;
                if (s.isVisible && visibleCaps[s.cell] <= 0) continue;
                if (!s.isVisible && waitingCaps[s.cell] <= 0) continue;

                spot = s;
                thirdIndex++;
                break;
            }
            if (spot.cell == -1)
            {
                for (int i = 0; i < thirdSpots.Count; i++)
                {
                    var s = thirdSpots[i];
                    if (s.isVisible && visibleCaps[s.cell] <= 0) continue;
                    if (!s.isVisible && waitingCaps[s.cell] <= 0) continue;
                    spot = s; thirdIndex = i + 1; break;
                }
            }
            if (spot.cell == -1) break;

            var cellB = board.cells[spot.cell];
            if (spot.isVisible)
            {
                for (int i = 0; i < 3; i++)
                    if (cellB.visible[i] == -1) { cellB.PlaceVisibleAt(i, itemId); break; }
                visibleCaps[spot.cell]--;
            }
            else
            {
                cellB.EnqueueWaiting(itemId);
                waitingCaps[spot.cell]--;
            }
        }
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
