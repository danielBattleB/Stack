using System.Collections;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverUI; // Reference to the game over UI panel
    public TextMeshProUGUI finalScoreText; // Reference to the final score text on the Game Over screen
    public Button playAgainButton;

    public GameObject currentCube;
    public GameObject lastCube;
    public TextMeshProUGUI text;
    public int level;
    public bool gameOver;
    private float snapThreshold = 5f; // Define a threshold for snapping
    private float cubeHeight = 40f;

    public float cameraMoveSpeed = 1f; // Speed at which the camera moves
    private float baseCameraMoveSpeed = 1f; // Store the base camera move speed for reference
    private float baseCubeMoveSpeed = 0.5f; // Base cube speed
    private float cubeMoveSpeed; // Current speed at which the cubes move

    [SerializeField] private Vector3 initialCameraPositionOffset = new Vector3(-345, 600, 345); // Initial camera offset
    private Vector3 targetCameraPosition; // Target position for the camera
    private Quaternion initialCameraRotation = Quaternion.Euler(135, 315, 180); // To store the initial camera rotation

    public Material backgroundMaterial;

    private int perfectPiecesCount = 0; // Track how many pieces were missed
    private const int MaxPerfectPieces = 8; // When this count is reached, grow the next perfect cube

    // Data structure for game speed milestones
    private Dictionary<int, float> speedMilestones = new Dictionary<int, float>()
    {
        { 15, 1.5f }, // 5% increase at level 80
        { 200, 2f } // 10% increase at level 200
    };
    private float speedModifier = 1f; // Tracks the current speed multiplier


    void Start()
    {
        Physics.gravity = new Vector3(0, -20f, 0);

        // Set initial cube and camera speeds
        cubeMoveSpeed = baseCubeMoveSpeed;

        Camera.main.orthographic = true;  // Set camera to orthographic mode
        // Set initial position and angle for the camera to align properly with the tower
        Camera.main.transform.position = currentCube.transform.position + initialCameraPositionOffset;
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

            // Check if the block is too small
            if (newScaleX <= 0f || newScaleZ <= 0f)
            {
                CreateFallingPiece(currentCube.transform.position, currentCube.transform.localScale, currentCube.GetComponent<MeshRenderer>().material);
                gameOver = true;
                text.gameObject.SetActive(false); // Hide the in-game score text

                // Show the Game Over screen
                gameOverUI.SetActive(true);

                // Set the final score text on the Game Over screen
                finalScoreText.text = "Your Score: " + (level - 1).ToString();

                return;
            }

            // Adjust the current cube size and position
            Vector3 currentPos = currentCube.transform.position;
            currentCube.transform.localScale = new Vector3(newScaleX, lastCube.transform.localScale.y, newScaleZ);
            currentCube.transform.position = Vector3.Lerp(currentCube.transform.position, lastCube.transform.position, 0.5f) + Vector3.up * cubeHeight / 2;

            // Create the falling piece based on the excess
            CreateFallingPiece(currentPos, diffX, diffZ, newScaleX, newScaleZ, currentCube.GetComponent<MeshRenderer>().material);
        }

        // Set the lastCube as the current one and instantiate a new currentCube
        lastCube = currentCube;
        currentCube = Instantiate(lastCube);
        currentCube.name = level.ToString();

        // Change the color of the current cube
        float hue = (level / 200f) % 1f; // Use a larger number to make the hue change more gradual
        Color newColor = Color.HSVToRGB(hue, 1f, 1f);
        currentCube.GetComponent<MeshRenderer>().material.SetColor("_Color", newColor);
        backgroundMaterial.color = newColor;
        level++;
        text.text = ""+(level-1);
        // Update the camera's target position to gradually increase in the Y direction
        targetCameraPosition = new Vector3(Camera.main.transform.position.x, initialCameraPositionOffset.y + (level * cubeHeight), Camera.main.transform.position.z);

        // Check if we need to update speed
        UpdateSpeedModifier();
    }

    private void UpdateSpeedModifier()
    {
        foreach (var milestone in speedMilestones)
        {
            if (level >= milestone.Key)
            {
                speedModifier = milestone.Value; // Apply the speed multiplier for this milestone
            }
        }

        // Apply the speed modifier to both cube movement and camera movement
        cameraMoveSpeed = baseCameraMoveSpeed * speedModifier;
        cubeMoveSpeed = baseCubeMoveSpeed * speedModifier;

        //Debug.Log($"Speed updated! Modifier: {speedModifier}, Camera Speed: {cameraMoveSpeed}, Cube Move Speed: {cubeMoveSpeed}");
    }

    // Method to create the falling piece based on the difference in position and scale
    private void CreateFallingPiece(Vector3 currentPos, float diffX, float diffZ, float newScaleX, float newScaleZ, Material cubeMaterial)
    {
        Vector3 fallingPieceScale = Vector3.zero;
        Vector3 fallingPiecePosition = Vector3.zero;

        // Minimum scale threshold to avoid creating 2D or zero-sized pieces
        const float minScaleThreshold = 0.1f;

        // Calculate the actual excess based on current and last cube positions
        float excessX = currentPos.x - lastCube.transform.position.x;
        float excessZ = currentPos.z - lastCube.transform.position.z;

        if (MathF.Abs(excessX) > snapThreshold) // Excess on the X axis
        {
            float fallingPieceWidth = MathF.Abs(excessX);

            // Only create falling piece if there's significant excess
            if (fallingPieceWidth > minScaleThreshold)
            {
                fallingPieceScale = new Vector3(fallingPieceWidth, currentCube.transform.localScale.y, currentCube.transform.localScale.z);
                fallingPiecePosition = currentPos + new Vector3((fallingPieceWidth / 2) * MathF.Sign(excessX), 0, 0);
                fallingPiecePosition.x = lastCube.transform.position.x + MathF.Sign(excessX) * (lastCube.transform.localScale.x / 2 + fallingPieceScale.x / 2);
            }
        }
        else if (MathF.Abs(excessZ) > snapThreshold) // Excess on the Z axis
        {
            float fallingPieceDepth = MathF.Abs(excessZ);

            // Only create falling piece if there's significant excess
            if (fallingPieceDepth > minScaleThreshold)
            {
                fallingPieceScale = new Vector3(currentCube.transform.localScale.x, currentCube.transform.localScale.y, fallingPieceDepth);
                fallingPiecePosition = currentPos + new Vector3(0, 0, (fallingPieceDepth / 2) * MathF.Sign(excessZ));
                fallingPiecePosition.z = lastCube.transform.position.z + MathF.Sign(excessZ) * (lastCube.transform.localScale.z / 2 + fallingPieceScale.z / 2);
            }
        }

        // Create the falling piece only if the scale is above the threshold
        if (fallingPieceScale.x > minScaleThreshold && fallingPieceScale.z > minScaleThreshold)
        {
            // Reset the perfect streak count because the cube wasn't placed perfectly
            perfectPiecesCount = 0;

            GameObject fallingPiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fallingPiece.transform.localScale = fallingPieceScale;
            fallingPiece.transform.position = fallingPiecePosition;

            // Apply the material correctly
            MeshRenderer renderer = fallingPiece.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = new Material(cubeMaterial); // Clone the material
            }
            else
            {
                Debug.LogError("Falling piece MeshRenderer not found.");
            }

            // Add Rigidbody for physics-based falling
            Rigidbody rb = fallingPiece.AddComponent<Rigidbody>();
            rb.mass = 5f; // Increased mass for faster falling
            rb.useGravity = true;
            rb.drag = 0.1f; // Decreased drag for faster falling
            fallingPiece.AddComponent<DestroyWhenOutOfView>();
            Debug.Log("Created falling piece at " + fallingPiece.transform.position + " with scale " + fallingPiece.transform.localScale);
        }
        else
        {
            //Debug.LogWarning("Falling piece not created due to insufficient scale. Current scale: " + fallingPieceScale);
            perfectPiecesCount++;
            if (perfectPiecesCount>=MaxPerfectPieces)
            {
                GrowPerfectCube();
            }
        }
    }
    private void GrowPerfectCube()
    {
        if (level % 2 == 0 && currentCube.transform.localScale.x < 120) // On X-axis
        {
            currentCube.transform.localScale += new Vector3(10f, 0f, 0f); // Increase scale in X
        }
        else if (level % 2 != 0 && currentCube.transform.localScale.z < 120) // On Z-axis
        {
            currentCube.transform.localScale += new Vector3(0f, 0f, 10f); // Increase scale in Z
        }
    }


    // Overloaded method to create the falling piece when the game ends
    private void CreateFallingPiece(Vector3 position, Vector3 scale, Material cubeMaterial)
    {
        GameObject fallingPiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fallingPiece.transform.localScale = scale;
        fallingPiece.transform.position = position;

        // Apply the correct material to the falling piece
        MeshRenderer renderer = fallingPiece.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            // Clone the material to ensure it's applied correctly
            renderer.material = new Material(cubeMaterial);
        }
        else
        {
            Debug.LogError("Falling piece MeshRenderer not found.");
        }

        // Add Rigidbody to make the piece fall
        Rigidbody rb = fallingPiece.AddComponent<Rigidbody>();
        rb.mass = 5f;
        rb.useGravity = true;
        // Add the script to destroy the object when it leaves the view
        fallingPiece.AddComponent<DestroyWhenOutOfView>();
        Debug.Log("Created final falling piece at " + fallingPiece.transform.position + " with scale " + fallingPiece.transform.localScale);
    }

    private float moveTimer = 0f; // This will control the lerp timing

    void Update()
    {
        if (gameOver)
        {
            return;
        }

        // Check if game is over and listen for spacebar to restart
        if (gameOver && Input.GetKeyDown(KeyCode.Space))
        {
            OnPlayAgain(); // Call the Play Again method when space is pressed
        }
        // Increment the custom timer
        moveTimer += Time.deltaTime;

        // Define the ping-pong time value for smoother continuous movement
        var time = Mathf.PingPong(moveTimer * cubeMoveSpeed, 1f); // Time value between 0 and 1

        // Define positions to move between
        var pos1 = lastCube.transform.position + Vector3.up * cubeHeight; // Centered above the tower
        var pos2 = pos1 + ((level % 2 == 0) ? Vector3.left : Vector3.forward) * 260; // Extends beyond the tower

        // Correct the positions for the starting point
        if (level % 2 == 0)
        {
            pos2 = pos1 + Vector3.left * 130; // End at the left edge
            pos1 = pos1 + Vector3.right * 130; // Start at the right edge
        }
        else
        {
            pos2 = pos1 + Vector3.forward * 130; // End at the front edge
            pos1 = pos1 + Vector3.back * 130; // Start at the back edge
        }

        // Use Lerp to move between the two points based on the continuous timer value
        currentCube.transform.position = Vector3.Lerp(pos1, pos2, time);

        // Camera movement: smoothly rise the camera upwards in Y direction
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetCameraPosition, cameraMoveSpeed * Time.deltaTime);

        // Check for block placement
        if (Input.GetKeyDown(KeyCode.Space))
        {
            newBlock();
            moveTimer = 0f; // Reset the timer when a new block is placed
        }
    }


    // Function to restart the game when the Play Again button is clicked
    public void OnPlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene
    }
}
