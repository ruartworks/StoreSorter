public static class MatchSystem
{
    // Returns true if cell currently holds a triple, with the matching itemId
    public static bool TryGetMatch(CellModel cell, out int matchId)
    {
        return cell.IsVisibleTripleMatch(out matchId);
    }
}
