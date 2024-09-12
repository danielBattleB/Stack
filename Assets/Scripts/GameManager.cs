using System.Collections;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject currentCube;
    public GameObject lastCube;
    public TextMeshProUGUI text;
    public int level;
    public bool done;
    private float snapThreshold = 5f; // Define a threshold for snapping
    private float cubeHeight = 40f;

    public float cameraMoveSpeed = 1f; // Speed at which the camera moves
    [SerializeField] private Vector3 initialCameraPositionOffset = new Vector3(-210, 275, 210); // Initial camera offset
    private Vector3 targetCameraPosition; // Target position for the camera
    private Quaternion initialCameraRotation = Quaternion.Euler(155, 315, 180); // To store the initial camera rotation

    public Material backgroundMaterial;

    // Start is called before the first frame update
    void Start()
    {
        // Set initial position and angle for the camera to align properly with the tower
        Camera.main.transform.position = currentCube.transform.position + initialCameraPositionOffset;
        //initialCameraRotation = Quaternion.LookRotation(-initialCameraPositionOffset.normalized); // Set the camera to look directly at the tower's base
        //Camera.main.transform.rotation = initialCameraRotation;
        Camera.main.transform.rotation = initialCameraRotation;
        // Initialize the first block
        newBlock();
    }

    private void newBlock()
    {
        if (lastCube != null)
        {
            // Round current cube's position
            currentCube.transform.position = new Vector3(
                MathF.Round(currentCube.transform.position.x),
                currentCube.transform.position.y,
                MathF.Round(currentCube.transform.position.z)
            );

            // Check if the current cube is close enough to snap on top of the last cube
            float diffX = MathF.Abs(currentCube.transform.position.x - lastCube.transform.position.x);
            float diffZ = MathF.Abs(currentCube.transform.position.z - lastCube.transform.position.z);

            // Snap if within the threshold
            if (diffX <= snapThreshold)
            {
                currentCube.transform.position = new Vector3(
                    lastCube.transform.position.x,
                    currentCube.transform.position.y,
                    currentCube.transform.position.z
                );
            }
            if (diffZ <= snapThreshold)
            {
                currentCube.transform.position = new Vector3(
                    currentCube.transform.position.x,
                    currentCube.transform.position.y,
                    lastCube.transform.position.z
                );
            }

            // Calculate new scale based on overlap
            float newScaleX = lastCube.transform.localScale.x - MathF.Abs(currentCube.transform.position.x - lastCube.transform.position.x);
            float newScaleZ = lastCube.transform.localScale.z - MathF.Abs(currentCube.transform.position.z - lastCube.transform.position.z);

            // Maintain the same height for the block
            currentCube.transform.localScale = new Vector3(
                Mathf.Max(0, newScaleX), // Prevent negative scale
                lastCube.transform.localScale.y, // Keep the same height
                Mathf.Max(0, newScaleZ)  // Prevent negative scale
            );

            // Center the block between lastCube and currentCube positions and move it up by 5 units
            currentCube.transform.position = Vector3.Lerp(
                currentCube.transform.position,
                lastCube.transform.position,
                0.5f
            ) + Vector3.up * cubeHeight/2;

            // Check if the block is too small
            if (currentCube.transform.localScale.x <= 0f || currentCube.transform.localScale.z <= 0f)
            {
                done = true;
                text.gameObject.SetActive(true);
                text.text = "Your Score " + level;
                StartCoroutine(x());
                return;
            }
        }

        // Set the lastCube as the current one and instantiate a new currentCube
        lastCube = currentCube;
        currentCube = Instantiate(lastCube);
        currentCube.name = level.ToString();

        // Change the color of the current cube
        Color newColor = Color.HSVToRGB((level / 100f) % 1f, 1f, 1f);
        currentCube.GetComponent<MeshRenderer>().material.SetColor("_Color", newColor);
        backgroundMaterial.color = newColor;
        level++;

        // Update the camera's target position to gradually increase in the Y direction
        targetCameraPosition = new Vector3(Camera.main.transform.position.x, initialCameraPositionOffset.y + (level * cubeHeight), Camera.main.transform.position.z); // Increment Y by 10 units per level
    }

    private float moveTimer = 0f; // This will control the lerp timing

    // Update is called once per frame
    void Update()
    {
        if (done)
        {
            return;
        }

        // Increment the custom timer
        moveTimer += Time.deltaTime;

        // Ensure the moveTimer resets properly and use it for movement
        var time = Mathf.PingPong(moveTimer, 4f) / 4f; // Time value between 0 and 1 over 4 seconds

        // Define positions to move between
        var pos1 = lastCube.transform.position + Vector3.up * cubeHeight; // Centered above the tower
        var pos2 = pos1 + ((level % 2 == 0) ? Vector3.left : Vector3.forward) * 260; // Extends beyond the tower

        // Correct the positions for the starting point
        if (level % 2 == 0)
        {
            // Horizontal movement: left to right
            pos2 = pos1 + Vector3.left * 130; // End at the left edge
            pos1 = pos1 + Vector3.right * 130; // Start at the right edge
        }
        else
        {
            // Vertical movement: forward to back
            pos2 = pos1 + Vector3.forward * 130; // End at the front edge
            pos1 = pos1 + Vector3.back * 130; // Start at the back edge
        }

        // Use Lerp to move between the two points based on the custom timer
        currentCube.transform.position = Vector3.Lerp(pos1, pos2, time);

        // Camera movement: smoothly rise the camera upwards in Y direction
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetCameraPosition, cameraMoveSpeed * Time.deltaTime);

        // Maintain the initial camera rotation
        //Camera.main.transform.rotation = initialCameraRotation;

        // Check for block placement
        if (Input.GetKeyDown(KeyCode.Space))
        {
            newBlock();
            moveTimer = 0f; // Reset the timer when a new block is placed
        }
    }

    IEnumerator x()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("Game");
    }
}
