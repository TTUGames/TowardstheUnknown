using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UIElements;

/// <summary>
/// The controls page of the options: a line per rebindable key, its action on the left, its key on the right. Clicking
/// the key waits for a new one (Escape cancels); the overrides are saved by <see cref="GameInput"/>
/// </summary>
public class ControlsPage
{
    private const string ValueClassName = "setting__key";
    private const string WaitingClassName = "outline-button--selected";

    // The keyboard binding rebound for each action: its index in the action's bindings
    private readonly (InputAction action, int binding, string labelKey, int number)[] rows;
    private readonly SlantedButton[] buttons;
    private InputActionRebindingExtensions.RebindingOperation rebinding;

    public ControlsPage(VisualElement page)
    {
        Controls controls = GameInput.Controls;
        rows = new[]
        {
            (controls.Gameplay.EndTurn, 0, "ControlsEndTurn", 0),
            (controls.Menus.ToggleInventory, 0, "ControlsInventory", 0),
            (controls.Inventory.Rotate, 1, "ControlsRotate", 0),
            (controls.Gameplay.Skill1, 0, "ControlsArtifact", 1),
            (controls.Gameplay.Skill2, 0, "ControlsArtifact", 2),
            (controls.Gameplay.Skill3, 0, "ControlsArtifact", 3),
            (controls.Gameplay.Skill4, 0, "ControlsArtifact", 4),
            (controls.Gameplay.Skill5, 0, "ControlsArtifact", 5),
            (controls.Gameplay.Skill6, 0, "ControlsArtifact", 6),
            (controls.Gameplay.Skill7, 0, "ControlsArtifact", 7),
            (controls.Gameplay.Skill8, 0, "ControlsArtifact", 8),
            (controls.Gameplay.Skill9, 0, "ControlsArtifact", 9),
            (controls.Menus.SwitchEdition, 0, "ControlsSwitchEdition", 0),
        };
        buttons = new SlantedButton[rows.Length];
        for (int i = 0; i < rows.Length; i++)
        {
            var line = new VisualElement();
            line.AddToClassList("setting");
            var label = new Label();
            var text = new LocalizedString(Localization.UITable, rows[i].labelKey);
            if (rows[i].number > 0) text.Arguments = new object[] { rows[i].number };
            label.SetBinding("text", text);
            line.Add(label);
            var button = new SlantedButton { corners = Corners.TopLeft | Corners.BottomRight };
            button.AddToClassList("outline-button");
            button.AddToClassList("panel");
            button.AddToClassList(ValueClassName);
            int index = i;
            button.clicked += () => Rebind(index);
            line.Add(button);
            buttons[i] = button;
            page.Add(line);
        }
        page.RegisterCallback<DetachFromPanelEvent>(_ => Cancel());
    }

    /// <summary>
    /// Shows each action's key as it is now
    /// </summary>
    public void Refresh()
    {
        for (int i = 0; i < rows.Length; i++)
        {
            buttons[i].key = null;
            buttons[i].text = rows[i].action.GetBindingDisplayString(rows[i].binding);
            buttons[i].RemoveFromClassList(WaitingClassName);
        }
    }

    /// <summary>
    /// Gives back the default keys
    /// </summary>
    public void ResetToDefault()
    {
        Cancel();
        GameInput.ResetBindings();
        Refresh();
    }

    /// <summary>
    /// Stops waiting for a key, the page left or the options closed
    /// </summary>
    public void Cancel()
    {
        rebinding?.Cancel();
    }

    private void Rebind(int index)
    {
        Cancel();
        (InputAction action, int binding, _, _) = rows[index];
        buttons[index].key = "ControlsPressKey";
        buttons[index].AddToClassList(WaitingClassName);
        // The action can't be rebound while it listens, and the menus' keys (Escape closing the options) wait meanwhile
        bool wasEnabled = action.enabled;
        action.Disable();
        GameInput.Controls.Menus.Disable();
        rebinding = action.PerformInteractiveRebinding(binding)
            .WithControlsExcluding("<Mouse>")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(operation => Done(operation, action, wasEnabled, true))
            .OnCancel(operation => Done(operation, action, wasEnabled, false))
            .Start();
    }

    private void Done(InputActionRebindingExtensions.RebindingOperation operation, InputAction action, bool wasEnabled, bool completed)
    {
        operation.Dispose();
        if (rebinding == operation) rebinding = null;
        if (wasEnabled) action.Enable();
        GameInput.Controls.Menus.Enable();
        if (completed) GameInput.SaveBindings();
        Refresh();
    }
}
