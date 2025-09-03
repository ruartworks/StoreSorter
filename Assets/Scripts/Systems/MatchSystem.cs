
public static class MatchSystem
{
    // Fully resolves a cell: keep clearing & pulling until no change
    public static bool ResolveCellFully(CellModel cell)
    {
        bool any = false;
        int guard = 16; // prevent infinite loops
        while (guard-- > 0)
        {
            if (cell.IsVisibleTripleMatch(out _))
            {
                cell.ClearVisible();
                cell.PullFromWaitingToVisibleOnce();
                any = true;
                continue;
            }
            // No triple; try to pull if there are empties
            bool pulled = cell.PullFromWaitingToVisibleOnce();
            if (!pulled) break; // stable now
        }
        return any;
    }
}
