using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class TerrainBehaviour : MonoBehaviour
{
    public Terrain mainTerrain;

    private float flatFieldSize = 80;
    private const int mapResolution = 4;

    private float[] fieldHeights = new float[mapResolution*mapResolution] 
      { 0.1f, 0.2f, 0.1f, 0.05f,
        0.15f, 0.12f, 0.09f, 0.13f,
        0.25f, 0.3f, 0.15f, 0.14f,
        0.14f, 0.15f, 0.2f, 0.2f };

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
        int xMap = Math.Min(mapResolution-1, transformHeightmapCoordToMap(x));
        int yMap = Math.Min(mapResolution-1, transformHeightmapCoordToMap(y));
        int xHMapCenter = transformMapCoordToHeigthmap(xMap);
        int yHMapCenter = transformMapCoordToHeigthmap(yMap);
        int distX = x - xHMapCenter;
        int distY = y - yHMapCenter;
        int fieldDist = (int)(mainTerrain.terrainData.heightmapResolution / mapResolution - flatFieldSize);
        int id = (mapResolution - 1) * xMap + yMap;
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

        if (isInsideField || isCorner || isBorder)
        {
            return fieldHeights[id];
        }
        //Interpolate
        else if (Math.Abs(distX) > fieldMargin && Math.Abs(distY) > fieldMargin && 
            !(xMap==0 && distX<0) && !(xMap==mapResolution-1 && distX>0) &&
            !(yMap==0 && distY<0) && !(yMap==mapResolution-1 && distY>0))
        {
            float paramX = (Math.Abs(distX) - fieldMargin) / (fieldDist);
            float paramY = (Math.Abs(distY) - fieldMargin) / (fieldDist);
            int IDTopRight = (mapResolution - 1) * (xMap + Math.Sign(distX)) + yMap;
            int IDBottomLeft = (mapResolution - 1) * xMap + yMap + Math.Sign(distY);
            int IDBottomRight = (mapResolution - 1) * (xMap + Math.Sign(distX)) + yMap + Math.Sign(distY);

            float heightTopLeft = fieldHeights[id];
            float heightTopRight = fieldHeights[id];
            float heightBottomLeft = fieldHeights[id];
            float heightBottomRight = fieldHeights[id];
            if (idExists(IDTopRight))
            {
                heightTopRight = fieldHeights[IDTopRight];
            }

            if (idExists(IDBottomLeft))
            {
                heightBottomLeft = fieldHeights[IDBottomLeft];
            }

            if (idExists(IDBottomRight))
            {
                heightBottomRight = fieldHeights[IDBottomRight];
            }
            float interpolateX1 = getSinInterpolation(heightTopLeft, heightTopRight, paramX);
            float interpolateX2 = getSinInterpolation(heightBottomLeft, heightBottomRight, paramX);
            return getSinInterpolation(interpolateX1, interpolateX2, paramY);
        }
        else if (Math.Abs(distY) > fieldMargin &&
            !(yMap == 0 && distY < 0) && !(yMap == mapResolution - 1 && distY > 0)) 
        {
            //Case 1: y is outside the field
            float param = (Math.Abs(distY) - fieldMargin) / (fieldDist);
            int nextID = (mapResolution - 1) * xMap + yMap + Math.Sign(distY);
            float height = fieldHeights[id];
            float nextHeight = fieldHeights[id];
            if (idExists(nextID))
            {
                nextHeight = fieldHeights[nextID];
            }
            return getSinInterpolation(height, nextHeight, param);
        }
        else if( Math.Abs(distX) > fieldMargin)
        {
            //Case 2: x is outside the field
            float param = (Math.Abs(distX) - fieldMargin) / (fieldDist);
            int nextID = (mapResolution - 1) * ( xMap + Math.Sign(distX)) + yMap;
            float height = fieldHeights[id];
            float nextHeight = fieldHeights[id];
            if (idExists(nextID))
            {
                nextHeight = fieldHeights[nextID];
            }
            return getSinInterpolation(height, nextHeight, param);
        }
        return 0;
    }

    private float getSinInterpolation(float z1, float z2, float param)
    {
        float p = Math.Max(0, param);
        p = Math.Min(1, p);
        float factor = 0.5f * Mathf.Sin(Mathf.PI * p - Mathf.PI / 2) + 0.5f;
        return (z2-z1)*factor+z1;
    }

    private bool idExists(int id)
    {
        return id >= 0 && id < fieldHeights.Length;
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
        float heightMapSize = mainTerrain.terrainData.heightmapResolution;
        return (int) Math.Round((x - heightMapSize/(2*mapSize)) / ((float)heightMapSize) * mapSize);
    }
}
