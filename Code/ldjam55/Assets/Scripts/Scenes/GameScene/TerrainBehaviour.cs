using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class TerrainBehaviour : MonoBehaviour
{
    public Terrain mainTerrain;

    private float flatFieldSize = 128;

    private float[] fieldHeights = new float[16] 
      { 0.1f, 0.2f, 0.1f, 0.05f,
        0.6f, 0.2f, 0.4f, 0.5f,
        0.2f, 0.5f, 0.8f, 0.3f,
        0.4f, 0f, 0.2f, 0.2f };

    // Start is called before the first frame update
    void Start()
    {
        int mapSize = mainTerrain.terrainData.heightmapResolution;
        float[,] heights = new float[mapSize, mapSize];
        System.Random random = new System.Random();
        for (int i = 0; i < mapSize; i++)
        {
            for(int j = 0; j < mapSize; j++)
            {
                heights[i,j] = getHeightmapValue(i, j);
            }
        }

        mainTerrain.terrainData.SetHeights(0, 0, heights );
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private float getHeightmapValue(int x, int y)
    {
        int mapResolution = 4;
        int xMap = Math.Min(mapResolution-1, transformHeightmapCoordToMap(x));
        int yMap = Math.Min(mapResolution-1, transformHeightmapCoordToMap(y));
        int xHMapCenter = transformMapCoordToHeigthmap(xMap);
        int yHMapCenter = transformMapCoordToHeigthmap(yMap);
        int distX = x - xHMapCenter;
        int distY = y - yHMapCenter;
        int fieldDist = (int)(mainTerrain.terrainData.heightmapResolution / 4 - flatFieldSize);
        int id = 3 * xMap + yMap;
        float fieldMargin = flatFieldSize / 2;

        //Is inside flat field
        bool isInsideField = Math.Abs(distX) <= fieldMargin && Math.Abs(distY) <= fieldMargin;
        
        bool isCorner = xMap == 0 && yMap == 0 && distX <= fieldMargin && distY <= fieldMargin;
        isCorner |= xMap == 0 && yMap == mapResolution - 1 && distX <= fieldMargin && distY >= fieldMargin;
        isCorner |= xMap == mapResolution - 1 && yMap == 0 && distX >= fieldMargin && distY <= fieldMargin;
        isCorner |= xMap == mapResolution - 1 && yMap == mapResolution - 1 && distX >= fieldMargin && distY >= fieldMargin;

        bool isBorder = xMap == 0 && distX <= fieldMargin && Math.Abs(distY) <= fieldMargin;
        isBorder |= xMap == mapResolution - 1 && distX >= fieldMargin && Math.Abs(distY) <= fieldMargin;
        isBorder |= yMap == 0 && distY <= fieldMargin && Math.Abs(distX) <= fieldMargin;
        isBorder |= yMap == mapResolution - 1 && distY >= fieldMargin && Math.Abs(distX) <= fieldMargin;

        if ( isInsideField || isCorner || isBorder)
        {
            return fieldHeights[id];
        }
        //Interpolate
/*        else if(Math.Abs(distX) > fieldMargin)
        {
            float param = distX - fieldMargin;
            int nextID = 3 * (xMap + Math.Sign(distX)) + yMap;
            if (nextID>0 && nextID<16)
            {
                float height = fieldHeights[id];
                float nextHeight = fieldHeights[nextID];
                return (nextHeight - height) / fieldDist * (distX - flatFieldSize / 2) + height;
            }
        }
/*                else
                {
                    if (yMap == 0 && distY < 0)
                    {
                        if(Math.Abs(distX) <= fieldMargin || (xMap == 0 && distX < 0)) { return fieldHeights[id]; }
                    } 
                    else if (yMap == 3 && distY > 0) 
                    {
                        if(Math.Abs(distX) <= fieldMargin || (xMap == 3 && distX > 0)) { return fieldHeights[id]; }
                    }
        /*            int nextID = 3 * xMap + yMap + 1;
                    float height = fieldHeights[id];
                    float nextHeight = fieldHeights[nextID];
                    return (nextHeight - height) / fieldDist * (distY-flatFieldSize/2) + height;
                }*/
        return 0;
    }

    private int transformMapCoordToHeigthmap(int x)
    {
        //TODO: take map size into account
        float mapSize = 4f;
        float heightMapSize = mainTerrain.terrainData.heightmapResolution;
        return (int) (x/mapSize*heightMapSize + heightMapSize/(2*mapSize)); 
    }

    private int transformHeightmapCoordToMap(int x)
    {
        //TODO: take map size into account
        int mapSize = 4;
        int heightMapSize = mainTerrain.terrainData.heightmapResolution;
        return (int) Math.Round(((float)x) / ((float)heightMapSize) * mapSize);
    }
}
