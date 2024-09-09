using System.Collections;
using System.Collections.Generic;
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

    // Start is called before the first frame update
    void Start()
    {
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
            ) + Vector3.up * 5f;

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
        currentCube.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.HSVToRGB((level / 100f) % 1f, 1f, 1f));
        level++;

        // Adjust the camera position and look direction
        Camera.main.transform.position = currentCube.transform.position + new Vector3(-175, 145, 170); // Move the camera to the back and above the tower
        Camera.main.transform.LookAt(currentCube.transform.position + Vector3.down * 10f); // Look slightly down to center the tower
    }


    //private void newBlock()
    //{
    //    if (lastCube != null)
    //    {
    //        currentCube.transform.position = new Vector3(MathF.Round(currentCube.transform.position.x),
    //            currentCube.transform.position.y,
    //            MathF.Round(currentCube.transform.position.z));
    //        currentCube.transform.localScale = new Vector3(lastCube.transform.localScale.x - MathF.Abs(currentCube.transform.position.x - lastCube.transform.position.x),
    //                                                       lastCube.transform.position.y,
    //                                                       lastCube.transform.localScale.z - MathF.Abs(currentCube.transform.position.z - lastCube.transform.position.z));
    //        currentCube.transform.position = Vector3.Lerp(currentCube.transform.position, lastCube.transform.position, 0.5f) + Vector3.up * 5f;
    //        if (currentCube.transform.localScale.x <= 0f ||
    //            currentCube.transform.localScale.z <= 0f)
    //        {
    //            done = true;
    //            text.gameObject.SetActive(true);
    //            text.text = "Your Score " + level;
    //            StartCoroutine(x());
    //            return;
    //        }
    //    }
    //    lastCube = currentCube;
    //    currentCube = Instantiate(lastCube);
    //    currentCube.name = level + "";
    //    currentCube.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.HSVToRGB((level / 100f) % 1f, 1f, 1f));
    //    level++;
    //    Camera.main.transform.position = currentCube.transform.position + new Vector3(100, 100, 100);
    //    Camera.main.transform.LookAt(currentCube.transform.position);
    //}

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
        var pos1 = lastCube.transform.position + Vector3.up * 10f; // Centered above the tower
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

        // Create a new block when Space is pressed
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
