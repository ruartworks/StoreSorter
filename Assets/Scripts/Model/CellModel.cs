
using System;

[Serializable]
public class CellModel
{
    public int[] visible = new int[3];
    public int visibleCount;
    public int[] waiting = new int[3];
    public int waitingCount;

    public CellModel()
    {
        for (int i = 0; i < 3; i++) { visible[i] = -1; waiting[i] = -1; }
        visibleCount = 0; waitingCount = 0;
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

    public void EnqueueWaiting(int itemId)
    {
        if (waitingCount >= 3) return;
        for (int i = 0; i < 3; i++)
        {
            if (waiting[i] == -1)
            {
                waiting[i] = itemId;
                waitingCount++;
                return;
            }
        }
    }

    public bool DequeueWaiting(out int itemId)
    {
        itemId = -1;
        if (waitingCount == 0) return false;
        itemId = waiting[0];
        waiting[0] = waiting[1];
        waiting[1] = waiting[2];
        waiting[2] = -1;
        waitingCount--;
        return true;
    }

    public bool PullFromWaitingToVisibleOnce()
    {
        bool moved = false;
        for (int i = 0; i < 3 && visibleCount < 3 && waitingCount > 0; i++)
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
