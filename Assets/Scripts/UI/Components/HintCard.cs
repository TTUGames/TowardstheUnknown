using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// A hint for a first-time player: each one is shown once ever (remembered in the PlayerPrefs, key <c>HintSeen.&lt;id&gt;</c>),
/// in an edition with <see cref="EditionProfile.hints"/>, until its owner hides it. Its keys are written from the bindings,
/// rebound ones included (<see cref="Key"/>)
/// </summary>
public class HintCard : SlantedLabel
{
    private const string PrefsPrefix = "HintSeen.";
    private const string ShownClassName = "hint--shown";
    private const string KeyboardPrefix = "<Keyboard>/";

    /// <summary>
    /// The id of the hint shown, null if none
    /// </summary>
    public string Current { get; private set; }

    public HintCard()
    {
        pickingMode = PickingMode.Ignore;
        enableRichText = true;
        corners = Corners.TopLeft | Corners.BottomRight;
        AddToClassList("hint");
        AddToClassList("panel");
    }

    /// <summary>
    /// Shows a hint, with the text of a UI key formatted with <paramref name="args"/>, unless it was already seen or the edition
    /// has none: true if it did. It counts as seen from then on
    /// </summary>
    public bool TryShow(string id, string textKey, params object[] args)
    {
        if (!Edition.Profile.hints || IsSeen(id)) return false;
        MarkSeen(id);
        Current = id;
        text = string.Format(Localization.UI(textKey), args);
        AddToClassList(ShownClassName);
        return true;
    }

    /// <summary>
    /// Hides the hint shown, or only the one of <paramref name="id"/>
    /// </summary>
    public void Hide(string id = null)
    {
        if (Current == null || (id != null && id != Current)) return;
        Current = null;
        RemoveFromClassList(ShownClassName);
    }

    public static bool IsSeen(string id) => PlayerPrefs.GetInt(PrefsPrefix + id, 0) == 1;

    /// <summary>
    /// The player did what the hint teaches without it: it won't be shown
    /// </summary>
    public static void MarkSeen(string id) => PlayerPrefs.SetInt(PrefsPrefix + id, 1);

    /// <summary>
    /// The key of an action as the player reads it, in bold: its first keyboard binding, or its first binding
    /// </summary>
    public static string Key(InputAction action)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];
            if (binding.isComposite || binding.isPartOfComposite || !binding.effectivePath.StartsWith(KeyboardPrefix)) continue;
            // A digit key is written as its digit, printed on it in every layout (the layout's own character on an AZERTY: &)
            string key = binding.effectivePath.Substring(KeyboardPrefix.Length);
            return $"<b>{(key.Length == 1 && char.IsDigit(key[0]) ? key : action.GetBindingDisplayString(i))}</b>";
        }
        return $"<b>{action.GetBindingDisplayString()}</b>";
    }
}
