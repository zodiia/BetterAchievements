using System.Collections.Generic;

namespace BetterAchievements.UI.Windows.Views.Settings;

public class SettingsSection(string name, IReadOnlyList<ISetting> settings) {
    public readonly string Name = name;
    public readonly IReadOnlyList<ISetting> Settings = settings;
    public bool IsOpen = true;
}
