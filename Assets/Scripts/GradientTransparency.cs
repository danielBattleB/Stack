using UnityEngine;

public class GradientTextureGenerator : MonoBehaviour
{
    // Width and height of the texture
    public int textureWidth = 256;
    public int textureHeight = 256;

    // Colors for the gradient (semi-transparent white to transparent)
    public Color bottomColor = new Color(1f, 1f, 1f, 0.5f); // Semi-transparent white
    public Color topColor = new Color(1f, 1f, 1f, 0f);      // Fully transparent

    // Create a gradient texture
    public Texture2D GenerateGradientTexture()
    {
        Texture2D texture = new Texture2D(textureWidth, textureHeight);

        // Loop through each pixel to create a vertical gradient
        for (int y = 0; y < textureHeight; y++)
        {
            float t = (float)y / (textureHeight - 1); // Calculate the interpolation factor
            Color color = Color.Lerp(bottomColor, topColor, t); // Interpolate the color based on height

            for (int x = 0; x < textureWidth; x++)
            {
                texture.SetPixel(x, y, color); // Set pixel color
            }
        }

        texture.Apply(); // Apply the changes to the texture
        return texture;
    }

    // Apply the generated texture to a quad or material
    public void ApplyTextureToQuad(Renderer quadRenderer)
    {
        Texture2D gradientTexture = GenerateGradientTexture();
        Material quadMaterial = quadRenderer.material;
        quadMaterial.mainTexture = gradientTexture; // Assign texture to the quad's material
    }

    // Example: Apply the texture when the game starts
    void Start()
    {
        Renderer quadRenderer = GetComponent<Renderer>(); // Assume this script is on a quad
        ApplyTextureToQuad(quadRenderer); // Apply the generated gradient texture to the quad
    }
}
