
using System;
using System.Collections.Generic;

[Serializable]
public class CellModel
{
    public int[] visible = new int[3];
    public int visibleCount;
    public List<int> waitingQueue = new List<int>();
    public int waitingCount => waitingQueue.Count;

    public CellModel()
    {
        for (int i = 0; i < 3; i++) visible[i] = -1;
        visibleCount = 0;
        waitingQueue.Clear();
    }

    public bool PlaceVisibleAt(int slot, int itemId)
    {
        if (slot < 0 || slot >= 3) return false;
        if (visible[slot] != -1) return false;
        visible[slot] = itemId;
        visibleCount++;
        return true;
    }

    public bool RemoveVisibleAt(int slot, out int itemId)
    {
        itemId = -1;
        if (slot < 0 || slot >= 3) return false;
        if (visible[slot] == -1) return false;
        itemId = visible[slot];
        visible[slot] = -1;
        visibleCount--;
        return true;
    }

    public bool IsVisibleTripleMatch(out int matchId)
    {
        matchId = -1;
        if (visibleCount < 3) return false;
        int a = visible[0], b = visible[1], c = visible[2];
        if (a != -1 && a == b && b == c) { matchId = a; return true; }
        return false;
    }

    public void ClearVisible()
    {
        for (int i = 0; i < 3; i++) visible[i] = -1;
        visibleCount = 0;
    }

    public bool EnqueueWaiting(int itemId, int maxDepth)
    {
        if (waitingQueue.Count >= maxDepth) return false;
        waitingQueue.Add(itemId);
        return true;
    }

    public bool DequeueWaiting(out int itemId)
    {
        itemId = -1;
        if (waitingQueue.Count == 0) return false;
        itemId = waitingQueue[0];
        waitingQueue.RemoveAt(0);
        return true;
    }

    public bool PullFromWaitingToVisibleOnce()
    {
        bool moved = false;
        for (int i = 0; i < 3 && visibleCount < 3 && waitingQueue.Count > 0; i++)
        {
            if (visible[i] == -1 && DequeueWaiting(out int id))
            {
                visible[i] = id;
                visibleCount++;
                moved = true;
            }
        }
        return moved;
    }
}
