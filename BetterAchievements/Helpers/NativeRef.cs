using System;

namespace BetterAchievements.Helpers;

public unsafe delegate T* PtrFunc<T>() where T : unmanaged;

public sealed unsafe class NativeRef<T>(PtrFunc<T> ptrGetter) where T : unmanaged {
    public bool Valid => ptrGetter.Invoke() != null;
    public ref T Value => ref *ptrGetter.Invoke();
}
