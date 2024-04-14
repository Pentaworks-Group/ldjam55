using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPaintBehaviour : MonoBehaviour
{
    public Terrain mainTerrain;

    public int grassLayerID = 0;
    public int stone1LayerID = 1;
    public int stone2LayerID = 2;
    public int slimeYellowLayerID = 3;
    public int slimeRedLayerID = 4;

    private float scaleFactorX = 1f;
    private float scaleFactorY = 1f;

    private float creepSize = 0;

    // Start is called before the first frame update
    void Start()
    {
        scaleFactorY = mainTerrain.terrainData.alphamapHeight / mainTerrain.terrainData.size.z;
        scaleFactorX = mainTerrain.terrainData.alphamapWidth / mainTerrain.terrainData.size.x;

        //Texture test
        float[,,] map = new float[mainTerrain.terrainData.alphamapWidth, mainTerrain.terrainData.alphamapHeight, mainTerrain.terrainData.alphamapLayers];

        //For each point on the alphamap
        for (int y = 0; y < mainTerrain.terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < mainTerrain.terrainData.alphamapWidth; x++)
            {
                // Get the normalized terrain coordinate that
                // corresponds to the point
                float normX = x * 1.0f / (mainTerrain.terrainData.alphamapWidth - 1);
                float normY = y * 1.0f / (mainTerrain.terrainData.alphamapHeight - 1);

                // Get the steepness value at the normalized coordinate
                var angle = mainTerrain.terrainData.GetSteepness(normY, normX);

                // Steepness is given as an angle, 0..90 degrees. Divide
                // by 90 to get an alpha blending value in the range 0..1.
                var frac = angle / 45.0;
                map[x, y, grassLayerID] = (float)(1 - frac);
                map[x, y, stone1LayerID] = 0;
                map[x, y, stone2LayerID] = (float)(frac);
                map[x, y, slimeYellowLayerID] = 0;
                map[x, y, slimeRedLayerID] = 0;
            }
        }
        mainTerrain.terrainData.SetAlphamaps(0, 0, map);

    }

    // Update is called once per frame
    void Update()
    {
        float delta = Time.deltaTime * 20f;
        creepSize += delta;

        paintCreep();
    }

    private void paintCreep()
    {
        //TODO: add Creep def
        Vector2Int pos = new Vector2Int((int) (100 - creepSize / 2), (int)(100 - creepSize / 2));
        paintSlimeArea(slimeRedLayerID, pos.x, pos.y, (int)creepSize, (int)(creepSize / 2));
    }

    private void paintSlimeArea(int slimeLayerID, int x, int y, int radius, int hardness)
    {
        if(x < 0) x = 0;
        if(y < 0) y = 0;

        int width = (int) (2 * radius * scaleFactorY);
        int height = (int)(2 * radius * scaleFactorX);

        width = Mathf.Min(width, mainTerrain.terrainData.alphamapHeight - x);
        height = Mathf.Min(height, mainTerrain.terrainData.alphamapWidth - y);

        float[,,] map = new float[width, height, mainTerrain.terrainData.alphamapLayers];
        float[,,] currentValues = mainTerrain.terrainData.GetAlphamaps( x , y , width, height);

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                float distance = Vector2.Distance(new Vector2(width/(2*scaleFactorY), height/(2*scaleFactorX)), new Vector2(i/scaleFactorY, j/scaleFactorX));

                for (int k = 0; k < mainTerrain.terrainData.alphamapLayers; k++)
                {
                    if (distance < radius - hardness )
                    {
                        if (k == slimeLayerID && distance <= radius)
                        {
                            map[i, j, k] = 1f;
                        }
                        else
                        {
                            map[i, j, k] = 0;
                        }
                    }
                    else if (distance <= radius)
                    {
                        float strength = 1f - Mathf.InverseLerp(radius - hardness, radius, distance);
                        if (k == slimeLayerID)
                        {
                            map[i, j, k] = strength;
                        }
                        else
                        {
                            map[i, j, k] = currentValues[0, 0, k] * (1-strength);
                        }

                    }
                    else
                    {
                        map[i, j, k] = currentValues[0, 0, k];

                    }
                }
            }
        }
        mainTerrain.terrainData.SetAlphamaps(x, y, map);
    }
}
