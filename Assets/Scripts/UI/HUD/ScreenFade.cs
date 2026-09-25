using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Fades the screen to black and back when the player changes room. It blocks the pointer while it is shown
/// </summary>
public class ScreenFade
{
    // Opacity per second
    private const float Speed = 5;

    private VisualElement root;

    public void Bind(VisualElement root)
    {
        this.root = root;
        root.style.opacity = 0;
        root.style.display = DisplayStyle.None;
    }

    public IEnumerator FadeIn()
    {
        if (root == null) yield break;
        root.style.display = DisplayStyle.Flex;
        yield return Fade(1);
    }

    public IEnumerator FadeOut()
    {
        if (root == null) yield break;
        yield return Fade(0);
        root.style.display = DisplayStyle.None;
    }

    private IEnumerator Fade(float targetOpacity)
    {
        float opacity = root.resolvedStyle.opacity;
        while (opacity != targetOpacity)
        {
            opacity = Mathf.MoveTowards(opacity, targetOpacity, Speed * Time.deltaTime);
            root.style.opacity = opacity;
            yield return null;
        }
    }
}
