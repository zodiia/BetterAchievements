using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace Recollection.Helpers;

public static class Pointers {
    public static unsafe T? GetOrNull<T>(T* value) where T : unmanaged {
        if (value is null) {
            return null;
        }

        return *value;
    }
    
    public static unsafe UIState? UiStateInstanceOrNull() {
        return GetOrNull(UIState.Instance());
    }

    public static unsafe MonsterNoteManager? MonsterNoteManagerInstanceOrNull() {
        return GetOrNull(MonsterNoteManager.Instance());
    }
}
