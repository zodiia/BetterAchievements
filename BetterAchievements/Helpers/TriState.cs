namespace BetterAchievements.Helpers;

public enum TriState
{
    False,
    True,
    Undefined,
}

public static class TriStateExtensions
{
    public static TriState Next(this TriState state)
    {
        switch (state)
        {
            case TriState.False:
                return TriState.Undefined;
            case TriState.Undefined:
                return TriState.True;
        }
        return TriState.False;
    }
}
