using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIFade : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private GameObject fadeImage;
    [SerializeField] private float fadeSpeed;

    private Image image;

    private void Awake()
    {
        image = fadeImage.GetComponent<Image>();
        fadeImage.SetActive(false);
    }

    public IEnumerator FadeIn()
    {
        fadeImage.SetActive(true);
        yield return Fade(1);
    }

    public IEnumerator FadeOut() {
        yield return Fade(0);
        fadeImage.SetActive(false);
    }

    private IEnumerator Fade(float targetAlpha) {
        Color color = image.color;
        while (color.a != targetAlpha) {
            color.a = Mathf.MoveTowards(color.a, targetAlpha, fadeSpeed * Time.deltaTime);
            image.color = color;
            yield return null;
        }
    }
}
