using System;

namespace Recollection.UI.Windows.Views.Settings;

public interface ISetting {
    string Name { get; }
    string? Description { get; }
    Func<Configuration, bool>? IsDisabled { get; }
    bool IsChanged { get; set; }
    bool IsValid { get; }

    void Draw(Configuration draft);
    void Load(Configuration draft);
    void Apply(Configuration draft, Configuration configuration);
}

public abstract record Setting<T> : ISetting {
    public required string Name { get; init; }
    public string? Description { get; init; }
    public Func<Configuration, bool>? IsDisabled { get; init; }
    public required Func<Configuration, T> Getter { get; init; }
    public required Action<Configuration, T> Setter { get; init; }

    public bool IsChanged { get; set; } = false;
    public bool IsValid { get; protected set; } = true;

    public abstract void Draw(Configuration draft);
    public virtual void Load(Configuration draft) { } // by default nothing needs to be done

    public void Update(Configuration draft, T value) {
        Setter(draft, value);
        IsChanged = true;
    }

    public void Apply(Configuration draft, Configuration configuration) {
        if (IsChanged) Setter(configuration, Getter(draft));
    }
}
