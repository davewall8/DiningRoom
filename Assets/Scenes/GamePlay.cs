using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlay : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject chairOriginal;
    public GameObject tableOriginal;
    public GameObject sceneContainer;

    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(800, 600, false);       // full screen = false.
        CreateTable();
        CreateChairs();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void CreateTable()
    {
        GameObject tableClone = Instantiate(tableOriginal);
        tableClone.name = "tableClone";
        tableClone.transform.parent = sceneContainer.transform;
    }
    private void CreateChairs()
    {
        // One inch in chair/table space is 0.0254 in scene space (meters).
        // transform.Rotate(new Vector3(0f, 100f, 0f) * Time.deltaTime)
        float y = chairOriginal.transform.position.y;
        for (int i = 0; i < 2; i++)
        {
            float x = -0.5f + (i * 0.55f);
            float angle = -180f*i;
            // Rotate the cube by converting the angles into a quaternion.
            Quaternion target = Quaternion.Euler(0, angle, 0);
            for (int j = 0; j < 2; j++)
            {
                float z = -0.3f + (j * 0.75f);
                GameObject chairClone;
                chairClone = Instantiate(chairOriginal, new Vector3(x, y, z), target);
                chairClone.name = "chairClone[" + i + "," + j + "]";
                chairClone.transform.parent = sceneContainer.transform;
            }
        }
    }
}
