using UnityEngine;

public class DestroyWhenOutOfView : MonoBehaviour
{
    private Renderer objectRenderer;
    private Camera mainCamera;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        mainCamera = Camera.main; // Cache the main camera reference
    }

    void Update()
    {
        // Check if the object is outside the view frustum of the main camera
        if (!IsVisibleFrom(objectRenderer, mainCamera))
        {
            Destroy(gameObject);
        }
    }

    private bool IsVisibleFrom(Renderer renderer, Camera camera)
    {
        // Get the object's bounding box and check if it's within the camera's view frustum
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
}
