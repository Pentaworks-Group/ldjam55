using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private TerrainBehaviour terrainBehaviour;

    [SerializeField]
    private Terrain mainTerrain;

    // Start is called before the first frame update
    public void UpdatePosition()
    {
        Debug.Log("Update Camera");
        float xPos = mainTerrain.transform.position.x + mainTerrain.terrainData.size.x / 2f ;
        float zPos = -11;
        cam.transform.position = new Vector3(xPos, cam.transform.position.y, zPos);
        cam.transform.rotation = Quaternion.Euler(new Vector3(45,0,0));   
    }

}
