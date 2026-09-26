using UnityEngine.UIElements;

/// <summary>
/// Shakes an element sideways to refuse an action, with a class held while it shakes (its colors of refusal in USS)
/// </summary>
public static class RefuseShake
{
    private static readonly float[] offsets = { -7, 7, -5, 4, -2, 0 };
    private const long Step = 45;
    // The class stays a little after the shake
    private const long Hold = 200;

    public static void Play(VisualElement element, string refusedClass)
    {
        element.AddToClassList(refusedClass);
        for (int step = 0; step < offsets.Length; step++)
        {
            float offset = offsets[step];
            element.schedule.Execute(() => element.style.translate = new Translate(offset, 0)).StartingIn(step * Step);
        }
        element.schedule.Execute(() => {
            element.style.translate = StyleKeyword.Null;
            element.RemoveFromClassList(refusedClass);
        }).StartingIn(offsets.Length * Step + Hold);
    }
}
