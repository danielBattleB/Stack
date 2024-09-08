using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

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
            currentCube.transform.position = new Vector3(MathF.Round(currentCube.transform.position.x),
                currentCube.transform.position.y,
                MathF.Round(currentCube.transform.position.z));
            currentCube.transform.localScale = new Vector3(lastCube.transform.localScale.x - MathF.Abs(currentCube.transform.position.x - lastCube.transform.position.x),
                                                           lastCube.transform.position.y,
                                                           lastCube.transform.localScale.z - MathF.Abs(currentCube.transform.position.z - lastCube.transform.position.z));
            currentCube.transform.position = Vector3.Lerp(currentCube.transform.position, lastCube.transform.position, 0.5f) + Vector3.up * 5f;
            if (currentCube.transform.localScale.x <= 0f ||
                currentCube.transform.localScale.z <= 0f)
            {
                done = true;
                text.gameObject.SetActive(true);
                text.text = "Your Score " + level;
                StartCoroutine(x));
                return;
            }
        }
        lastCube = currentCube;
        currentCube = Instantiate(lastCube);
        currentCube.name = level + "";
        currentCube.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.HSVToRGB((level / 100f) % 1f, 1f, 1f));
        level++;
        Camera.main.transform.position = currentCube.transform.position + new Vector3(100,100,100);
        Camera.main.transform.LookAt(currentCube.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (done)
        {
            return;
        }

        var time = MathF.Abs(Time.realtimeSinceStartup % 2f - 1f);

        var pos1 = lastCube.transform.position + Vector3.up * 10f;
        var pos2 =  pos1 + ((level % 2 == 0) ? Vector3.left : Vector3.forward) * 120;

        if (level % 2 == 0)
        {
            currentCube.transform.position = Vector3.Lerp(pos2, pos1, time);
        }
        else
        {
            currentCube.transform.position = Vector3.Lerp(pos1, pos2, time);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            newBlock();
        }
    }

    IEnumerator x()
    {
        yield return new WaitForSeconds(3f);
        sce
    }

}
