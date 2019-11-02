using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AlphaFading : MonoBehaviour {
    public float fadeSpeed = 2f;
    private Image _image;

    private void Awake() {
        _image = GetComponent<Image>();
    }

    public void AutoFadeInAndFadeOut() {
        StartCoroutine(FadeInAndOut());
    }

    public void AlphaFadeTo(float alphaVal) {
        StartCoroutine(FadeTo(alphaVal));
    }
    
    IEnumerator FadeTo(float aValue) {
        float alpha = _image.color.a;
        for (float t = 0.0f; t < 1.0f; t += fadeSpeed * Time.deltaTime) {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha, aValue, t));
            _image.color = newColor;
            yield return null;
        }
    }

    IEnumerator FadeInAndOut() {
        StartCoroutine(FadeTo(1f));
        yield return new WaitForSeconds(1f);
        StartCoroutine(FadeTo(0f));
    }
}
