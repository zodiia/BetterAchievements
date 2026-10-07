using System.Numerics;
using Recollection.UI.Component;
using Dalamud.Bindings.ImGui;

namespace Recollection.UI.Windows.Views;

public class TodoView : IView {
    private const string Text = "Feature coming soon!";

    public void Draw() {
        var ySize = ImGui.GetContentRegionAvail().Y;
        if (!ImGui.BeginChild("MainContent", ImGui.GetContentRegionAvail() with { Y = ySize }, true)) {
            return;
        }

        using (UiFonts.FontSize150().Push()) {
            var available = ImGui.GetContentRegionAvail();
            var textSize = ImGui.CalcTextSize(Text);
            var cursorPos = ImGui.GetCursorPos();

            ImGui.SetCursorPos(new Vector2 { X = cursorPos.X + (available.X - textSize.X) / 2, Y = cursorPos.Y + (available.Y - textSize.Y) / 2 });
            ImGui.TextColored(UiColors.Text(), Text);
        }

        ImGui.EndChild();
    }
}
