using Assets.Scripts.Base;
using Assets.Scripts.Core.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using static UnityEditor.PlayerSettings;

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

    private int countX = 0;
    private int countY = 0;
    private float xOffset = 0;
    private float yOffset = 0;

    private float time = 0;

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
        countX = (maxX - minX) + 1;
        countY = (maxY - minY) + 1;

        xOffset = minX;
        yOffset = minY;

        scaleFactorY = mainTerrain.terrainData.alphamapHeight / mainTerrain.terrainData.size.z;
        scaleFactorX = mainTerrain.terrainData.alphamapWidth / mainTerrain.terrainData.size.x;

        //Texture test
        float[,,] map = new float[mainTerrain.terrainData.alphamapWidth, mainTerrain.terrainData.alphamapHeight, mainTerrain.terrainData.alphamapLayers];

        //For each point on the alphamap
        for (int y = 0; y < mainTerrain.terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < mainTerrain.terrainData.alphamapWidth; x++)
            {
                float frac = getSteepnessFactor(x, y);
                for (int z = 0; z < mainTerrain.terrainData.alphamapLayers; z++) {
                    map[x, y, z] = getSteepnessAlpha(z, frac);
                }
            }
        }
        mainTerrain.terrainData.SetAlphamaps(0, 0, map);

        //        Field f = new Field { Coords = new GameFrame.Core.Math.Vector2(0, 0) };
        //        paintCreep(f);

    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        foreach (var field in Core.Game.State.GameField.Fields)
        {
            if(field.Creep != null)
            {
                paintCreep(field);
            }
        }
    }

    private void paintCreep(Field field)
    {
        //TODO: add Creep def
        int sizeX = (int)(mainTerrain.terrainData.size.x / countX);
        int radius = (int)(Mathf.Min(field.Creep.Value, 1f) * 2* sizeX);
        if(radius != field.Creep.PaintRadiusOld)
        {
            field.Creep.PaintRadiusOld = radius;

            Vector2Int mapPos = new Vector2Int((int)(field.Coords.X - xOffset), (int)(field.Coords.Y - yOffset));
            Vector2Int creepCenter = getTextureMapCoord(mapPos);
            int layerID = getSlimeLayerID(field.Creep.Creeper);
            int sizeY = (int)(mainTerrain.terrainData.size.z / countY);

            paintSlimeArea(layerID, creepCenter.x - sizeX / 2, creepCenter.y - sizeY / 2, sizeX, sizeY, radius, 0);
        }

    }

    private int getSlimeLayerID(Creeper creeper)
    {
        //TODO: change
        if(creeper != null && creeper.Parameters!= null && creeper.Parameters.Material != null && creeper.Parameters.Material=="Water")
        {
            return slimeYellowLayerID;
        }
        return slimeRedLayerID;
    }

    private void paintSlimeArea(int slimeLayerID, int x, int y, int width, int height, int radius, int hardness)
    {
        if (x < 0) x = 0;
        if (y < 0) y = 0;

        width = (int)(width * scaleFactorY);
        height = (int)(height * scaleFactorX);
        x = (int)(x * scaleFactorX);
        y = (int)(y * scaleFactorY);

        width = Mathf.Min(width, mainTerrain.terrainData.alphamapHeight - x - 1);
        height = Mathf.Min(height, mainTerrain.terrainData.alphamapWidth - y - 1);

        float[,,] map = new float[width, height, mainTerrain.terrainData.alphamapLayers];

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                float distance = Vector2.Distance(new Vector2(width / (2 * scaleFactorY), height / (2 * scaleFactorX)), new Vector2(i / scaleFactorY, j / scaleFactorX));
                float frac = getSteepnessFactor(y + i, x + j);

                for (int k = 0; k < mainTerrain.terrainData.alphamapLayers; k++)
                {
                    if (distance < radius - hardness)
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
                            map[i, j, k] = getSteepnessAlpha(k, frac) * (1 - strength);
                        }

                    }
                    else
                    {
                        map[i, j, k] = getSteepnessAlpha(k, frac);
                    }
                }
            }
        }
        mainTerrain.terrainData.SetAlphamaps(x, y, map);
    }

    private Vector2Int getTextureMapCoord(Vector2Int pos)
    {
        pos.x = (int)((2 * pos.x + 1) * mainTerrain.terrainData.size.x / (2 * countX));
        pos.y = (int)((2 * pos.y + 1) * mainTerrain.terrainData.size.z / (2 * countY));
        return pos;
    }

    private float getSteepnessAlpha(int layerID, float frac)
    {
        if(layerID == grassLayerID)
        {
            return (float)(1 - frac);
        } else if(layerID == stone2LayerID) 
        {
            return frac;
        }
        return 0;
    }

    private float getSteepnessFactor(int x, int y)
    {
        // Get the normalized terrain coordinate that
        // corresponds to the point
        float normX = x * 1.0f / (mainTerrain.terrainData.alphamapWidth - 1);
        float normY = y * 1.0f / (mainTerrain.terrainData.alphamapHeight - 1);

        // Get the steepness value at the normalized coordinate
        var angle = mainTerrain.terrainData.GetSteepness(normY, normX);

        // Steepness is given as an angle, 0..90 degrees. Divide
        // by 90 to get an alpha blending value in the range 0..1.
        return angle / 45.0f;
    }
}
