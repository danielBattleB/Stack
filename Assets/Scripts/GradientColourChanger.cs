using UnityEngine;

public class GradientColorChanger : MonoBehaviour
{
    public Material gradientMaterial;
    public float changeInterval = 10f; // Interval in seconds for changing colors
    public float lerpSpeed = 0.05f;    // Speed of color change
    private Color targetTopColor;
    private Color targetBottomColor;
    private float timer;

    void Start()
    {
        // Set initial target colors
        targetTopColor = gradientMaterial.GetColor("_TopColor");
        targetBottomColor = gradientMaterial.GetColor("_BottomColor");
        ChangeGradientColors();
    }

    void Update()
    {
        // Smoothly interpolate colors
        gradientMaterial.SetColor("_TopColor", Color.Lerp(gradientMaterial.GetColor("_TopColor"), targetTopColor, lerpSpeed * Time.deltaTime));
        gradientMaterial.SetColor("_BottomColor", Color.Lerp(gradientMaterial.GetColor("_BottomColor"), targetBottomColor, lerpSpeed * Time.deltaTime));

        // Timer to trigger color changes
        timer += Time.deltaTime;
        if (timer >= changeInterval)
        {
            timer = 0f;
            ChangeGradientColors();
        }
    }

    void ChangeGradientColors()
    {
        // Set new random colors
        targetTopColor = Color.HSVToRGB(Random.Range(0f, 1f), 0.5f, 1f);
        targetBottomColor = Color.HSVToRGB(Random.Range(0f, 1f), 0.3f, 0.8f);
    }
}
