using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Assets.Scripts.Base;
using Assets.Scripts.Core.Model;

public class TerrainBehaviour : MonoBehaviour
{
    public Terrain mainTerrain;

    private float fieldSize = 8f;
    private float flatFieldSize = 4f;

    private float scalingFactorX = 1.0f;
    private float scalingFactorY = 1.0f;

    private int countX = 0;
    private int countY = 0;
    private float zFactor = 0;
    private float xOffset = 0;
    private float yOffset = 0;
    private float zOffset = 0;

    private float maxHeight = 0.01f;

    private void Awake()
    {
        if(Core.Game.State == default)
        {
            Core.Game.Start();
        }
    }

    // Start is called before the first frame update    
    void Start()
    {
        int minX = 0;
        int minY = 0;
        float minZ = 0;
        int maxX = 0;
        int maxY = 0;
        float maxZ = 0;
        foreach (var field in Core.Game.State.GameField.Fields)
        {
            minX = (int)Math.Min(field.Coords.X, minX);
            minY = (int)Math.Min(field.Coords.Y, minY);
            minZ = Mathf.Min(field.Height, minZ);
            maxX = (int)Math.Max(field.Coords.X, maxX);
            maxY = (int)Math.Max(field.Coords.Y, maxY);
            maxZ = Mathf.Max(field.Height, maxZ);
        }
        countX = (maxX - minX)+1;
        countY = (maxY - minY)+1;

        zFactor = maxHeight / (maxZ-minZ);
        xOffset = minX;
        yOffset = minY;
        zOffset = minZ;
            
        mainTerrain.terrainData.size = new Vector3(countX*fieldSize, 500, countY*fieldSize);

        scalingFactorX = mainTerrain.terrainData.heightmapResolution / (countX * fieldSize);
        scalingFactorY = mainTerrain.terrainData.heightmapResolution / (countY * fieldSize);

        int mapSize = mainTerrain.terrainData.heightmapResolution;

        float[,] heights = new float[mapSize, mapSize];
        for (int i = 0; i < mapSize; i++)
        {
            for(int j = 0; j < mapSize; j++)
            {
                heights[j,i] = getHeightmapValue(i, j);
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
        Vector2Int mapPos = transformHeightmapCoordToMap(new Vector2Int(x, y));
        Vector2Int hMapCenter = transformMapCoordToHeigthmap(mapPos);
        int distX = x - hMapCenter.x;
        int distY = y - hMapCenter.y;
        int fieldDistX = (int)(scalingFactorX * ( fieldSize - flatFieldSize) );
        int fieldDistY = (int)(scalingFactorY * ( fieldSize - flatFieldSize) );   
        float fieldMarginX = scalingFactorX * flatFieldSize / 2;
        float fieldMarginY = scalingFactorY * flatFieldSize / 2;

        //Main Field
        Field field = getField(mapPos.x, mapPos.y);
        if (field == null )
        {
            return 0;
        }

        //Is inside flat field
        bool isInsideField = Math.Abs(distX) <= fieldMarginX && Math.Abs(distY) <= fieldMarginY;
        
        bool isCorner = mapPos.x == 0 && mapPos.y == 0 && distX <= fieldMarginX && distY <= fieldMarginY;
        isCorner |= mapPos.x == 0 && mapPos.y == countY - 1 && distX <= fieldMarginX && distY >= fieldMarginY;
        isCorner |= mapPos.x == countX - 1 && mapPos.y == 0 && distX >= fieldMarginX && distY <= fieldMarginY;
        isCorner |= mapPos.x == countX - 1 && mapPos.y == countY - 1 && distX >= fieldMarginX && distY >= fieldMarginY;

        bool isBorder = mapPos.x == 0 && distX <= fieldMarginX && Math.Abs(distY) <= fieldMarginY;
        isBorder |= mapPos.x == countX - 1 && distX >= fieldMarginX && Math.Abs(distY) <= fieldMarginY;
        isBorder |= mapPos.y == 0 && distY <= fieldMarginY && Math.Abs(distX) <= fieldMarginX;
        isBorder |= mapPos.y == countY - 1 && distY >= fieldMarginY && Math.Abs(distX) <= fieldMarginX;

        if (isInsideField || isCorner || isBorder)
        {
            return getFieldHeight(field);
        }
        //Interpolate
        else if (Math.Abs(distX) > fieldMarginX && Math.Abs(distY) > fieldMarginY && 
            !(mapPos.x==0 && distX<0) && !(mapPos.x == countX - 1 && distX>0) &&
            !(mapPos.y==0 && distY<0) && !(mapPos.y == countY - 1 && distY>0))
        {
            float paramX = (Math.Abs(distX) - fieldMarginX) / (fieldDistX);
            float paramY = (Math.Abs(distY) - fieldMarginY) / (fieldDistY);

            float heightTopLeft = getFieldHeight(field);
            float heightTopRight = getFieldHeight(field);
            float heightBottomLeft = getFieldHeight(field);
            float heightBottomRight = getFieldHeight(field);

            Field topRightField = getField(mapPos.x + Math.Sign(distX), mapPos.y);
            if (topRightField != null)
            {
                heightTopRight = getFieldHeight(topRightField);
            }

            Field bottomLeftField = getField(mapPos.x, mapPos.y + Math.Sign(distY));
            if (bottomLeftField != null)
            {
                heightBottomLeft = getFieldHeight(bottomLeftField);
            }

            Field bottomRightField = getField(mapPos.x + Math.Sign(distX), mapPos.y + Math.Sign(distY));
            if (bottomRightField != null)
            {
                heightBottomRight = getFieldHeight(bottomRightField);
            }
            float interpolateX1 = getSinInterpolation(heightTopLeft, heightTopRight, paramX);
            float interpolateX2 = getSinInterpolation(heightBottomLeft, heightBottomRight, paramX);
            return getSinInterpolation(interpolateX1, interpolateX2, paramY);
        }
        else if (Math.Abs(distY) > fieldMarginY &&
            !(mapPos.y == 0 && distY < 0) && !(mapPos.y == countY - 1 && distY > 0)) 
        {
            //Case 1: y is outside the field
            float param = (Math.Abs(distY) - fieldMarginY) / (fieldDistY);
            float height = getFieldHeight(field);
            float nextHeight = getFieldHeight(field);
            Field nextField = getField(mapPos.x, mapPos.y + Math.Sign(distY));
            if (nextField != null)
            {
                nextHeight = getFieldHeight(nextField);
            }
            return getSinInterpolation(height, nextHeight, param);
        }
        else if( Math.Abs(distX) > fieldMarginX)
        {
            //Case 2: x is outside the field
            float param = (Math.Abs(distX) - fieldMarginX) / (fieldDistX);
            float height = getFieldHeight(field);
            float nextHeight = getFieldHeight(field);
            Field nextField = getField(mapPos.x + Math.Sign(distX), mapPos.y);
            if (nextField != null)
            {
                nextHeight = getFieldHeight(nextField);
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

    private Vector2Int transformMapCoordToHeigthmap(Vector2Int pos)
    {
        float heightMapSize = mainTerrain.terrainData.heightmapResolution;
        pos.x = (int)((2 * pos.x + 1) * heightMapSize / (2 * countX ));
        pos.y = (int)((2 * pos.y + 1) * heightMapSize / (2 * countY));
        return pos;
    }

    private Vector2Int transformHeightmapCoordToMap(Vector2Int pos)
    {
        float heightMapSize = mainTerrain.terrainData.heightmapResolution;
        pos.x = (int)Math.Floor(pos.x * countX / heightMapSize);
        pos.y = (int)Math.Floor(pos.y * countY / heightMapSize);
        return pos;
//        return (int) Math.Floor(x * mapSize / heightMapSize);
    }

    private Field getField(int x, int y)
    {
        //Shift negative to 0
        Field f = Core.Game.State.GameField.Fields.Find(field => field.Coords.X==x+xOffset && field.Coords.Y==y+yOffset);
        return f;
    }

    private float getFieldHeight(Field f) 
    { 
        return (f.Height - zOffset) * zFactor; 
    }
}
