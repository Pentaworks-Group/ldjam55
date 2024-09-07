using Assets.Scripts.Base;
using Assets.Scripts.Core.Model;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPaintBehaviour : MonoBehaviour
{
    [SerializeField]
    private Terrain mainTerrain;

    [SerializeField]
    private TerrainBehaviour terrainBehaviour;

    [SerializeField]
    private CreepBehaviour creepBehaviour;

    public int grassLayerID = 0;
    public int stone1LayerID = 1;
    public int stone2LayerID = 2;
    public int slimeYellowLayerID = 3;
    public int slimeRedLayerID = 4;
    public int rottenGroundLayerID = 5;

    private float scaleFactorX = 1f;
    private float scaleFactorY = 1f;

    int _alphamapWidth;
    float _alphamapWidthM1;
    int _alphamapHeight;
    float _alphamapHeightM1;
    int _alphamapLayers;

    TerrainData _terrainData;

    private void Awake()
    {
        creepBehaviour.OnCreeperChanged.Add((field, oldCreeper) => PaintCreep(field, true));
        creepBehaviour.OnFieldCreatedEvent.Add((field) => UpdateTerrain());
    }


    public void UpdateTerrain()
    {
        if (Core.Game.State.CurrentLevel.GameField.Fields.Count == 0)
        {
            return;
        }

        _terrainData = mainTerrain.terrainData;
        _alphamapWidth = _terrainData.alphamapWidth;
        _alphamapWidthM1 = _alphamapWidth - 1;
        scaleFactorX = _alphamapWidth / _terrainData.size.x;
        _alphamapHeight = _terrainData.alphamapHeight;
        _alphamapHeightM1 = _alphamapHeight - 1;
        scaleFactorY = _alphamapHeight / _terrainData.size.z;

        _alphamapLayers = _terrainData.alphamapLayers;

        //Texture test
        float[,,] map = new float[_alphamapWidth, _alphamapHeight, _alphamapLayers];

        Dictionary<int, Dictionary<int, Field>> fieldMap = CreateFieldCache();
        //For each point on the alphamap
        for (int y = 0; y < _alphamapHeight; y++)
        {
            for (int x = 0; x < _alphamapWidth; x++)
            {
                Vector2Int mapPos = GetMapCoordFromTextureCoord(new Vector2Int(y, x));
                Field field = GetFieldFieldFromCache(mapPos.x, mapPos.y, fieldMap);
                int layerID = rottenGroundLayerID;
                if (field == null)
                {
                    layerID = grassLayerID;
                }
                UpdateTerrainAtPoint(map, x, y, layerID);
            }
        }
        _terrainData.SetAlphamaps(0, 0, map);
    }

    private static Dictionary<int, Dictionary<int, Field>> CreateFieldCache()
    {
        Dictionary<int, Dictionary<int, Field>> fieldMap = new Dictionary<int, Dictionary<int, Field>>();
        foreach (var field in Core.Game.State.CurrentLevel.GameField.Fields)
        {
            int x = (int)field.Coords.X;
            int y = (int)field.Coords.Y;
            if (!fieldMap.TryGetValue(x, out Dictionary<int, Field> subMap))
            {
                subMap = new Dictionary<int, Field>();
                fieldMap.Add(x, subMap);
            }
            subMap.Add(y, field);
        }

        return fieldMap;
    }


    private Field GetFieldFieldFromCache(int x, int y, Dictionary<int, Dictionary<int, Field>> fieldMap)
    {
        Field field = null;
        if (fieldMap.TryGetValue(x + (int)terrainBehaviour.XOffset, out var subMap))
        {
            subMap.TryGetValue(y + (int)terrainBehaviour.YOffset, out field);
        }
        return field;
    }

    private void UpdateTerrainAtPoint(float[,,] map, int x, int y, int layerID, int offsetX = 0, int offsetY = 0)
    {
        float frac = GetSteepnessFactor(x + offsetX, y + offsetY);
        for (int z = 0; z < _alphamapLayers; z++)
        {
            map[x, y, z] = GetSteepnessAlpha(z, frac, layerID);
        }
    }

    //private void UpdateTerrain()
    //{
    //    scaleFactorX = mainTerrain.terrainData.alphamapWidth / mainTerrain.terrainData.size.x;
    //    scaleFactorY = mainTerrain.terrainData.alphamapHeight / mainTerrain.terrainData.size.z;

    //    //Texture test
    //    float[,,] map = new float[mainTerrain.terrainData.alphamapWidth, mainTerrain.terrainData.alphamapHeight, mainTerrain.terrainData.alphamapLayers];

    //    //For each point on the alphamap
    //    for (int y = 0; y < mainTerrain.terrainData.alphamapHeight; y++)
    //    {
    //        for (int x = 0; x < mainTerrain.terrainData.alphamapWidth; x++)
    //        {
    //            float frac = getSteepnessFactor(x, y);
    //            Vector2Int mapPos = getMapCoordFromTextureCoord(new Vector2Int(y, x));
    //            Field field = getField(mapPos.x, mapPos.y);
    //            for (int z = 0; z < mainTerrain.terrainData.alphamapLayers; z++)
    //            {
    //                int layerID = rottenGroundLayerID;
    //                if (field == null)
    //                {
    //                    layerID = grassLayerID;
    //                }
    //                map[x, y, z] = getSteepnessAlpha(z, frac, layerID);
    //            }
    //        }
    //    }
    //    mainTerrain.terrainData.SetAlphamaps(0, 0, map);
    //}

    // Update is called once per frame
    void Update()
    {
        if (Core.Game.isRunning)
        {
            foreach (var field in Core.Game.State.CurrentLevel.GameField.Fields)
            {
                if (field.Creep != null)
                {
                    PaintCreep(field);
                }
            }
        }

    }

    private void PaintCreep(Field field, bool forceUpdate = false)
    {
        //TODO: add Creep def
        int sizeX = (int)(_terrainData.size.x / terrainBehaviour.FieldCountX);
        int radius = (int)Math.Ceiling(Mathf.Min(field.Creep.Value, 1f) * 2f * sizeX);
        if (radius != field.Creep.PaintRadiusOld || forceUpdate)
        {
            field.Creep.PaintRadiusOld = radius;

            Vector2Int mapPos = new Vector2Int((int)(field.Coords.X - terrainBehaviour.XOffset), (int)(field.Coords.Y - terrainBehaviour.YOffset));
            Vector2Int creepCenter = GetTextureMapCoord(mapPos);
            int layerID = GetSlimeLayerID(field.Creep.Creeper);
            int sizeY = (int)(_terrainData.size.z / terrainBehaviour.FieldCountY);

            PaintSlimeArea(layerID, creepCenter.x - sizeX / 2, creepCenter.y - sizeY / 2, sizeX, sizeY, radius, radius / 2);
        }
    }

    private int GetSlimeLayerID(Creeper creeper)
    {
        //TODO: change
        if (creeper != null && creeper.Parameters != null && creeper.Parameters.Material != null && creeper.Parameters.Material == "Water")
        {
            return slimeYellowLayerID;
        }
        return slimeRedLayerID;
    }

    private void PaintSlimeArea(int slimeLayerID, int x, int y, int width, int height, int radius, int hardness)
    {
        if (x < 0) x = 0;
        if (y < 0) y = 0;

        width = (int)(width * scaleFactorY);
        height = (int)(height * scaleFactorX);
        x = (int)(x * scaleFactorX);
        y = (int)(y * scaleFactorY);

        //        width = Mathf.Min(width, mainTerrain.terrainData.alphamapWidth - x - 1);
        //        height = Mathf.Min(height, mainTerrain.terrainData.alphamapHeight - y - 1);

        //        Debug.Log("Paint Slime: " + x + ", " + y + ", " + width + ", " + height + ", " + radius);

        //        width = 80;
        //        height = 30;

        float[,,] map = new float[width, height, _terrainData.alphamapLayers];

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                float distance = Vector2.Distance(new Vector2(width / (2 * scaleFactorY), height / (2 * scaleFactorX)), new Vector2(i / scaleFactorY, j / scaleFactorX));
                float frac = GetSteepnessFactor(y + i, x + j);

                for (int k = 0; k < _terrainData.alphamapLayers; k++)
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
                            map[i, j, k] = GetSteepnessAlpha(k, frac, rottenGroundLayerID) * (1 - strength);
                        }

                    }
                    else
                    {
                        map[i, j, k] = GetSteepnessAlpha(k, frac, rottenGroundLayerID);
                    }
                }
            }
        }
        _terrainData.SetAlphamaps(x, y, map);
    }

    private Vector2Int GetTextureMapCoord(Vector2Int pos)
    {
        pos.x = (int)((2 * pos.x + 1) * _terrainData.size.x / (2 * terrainBehaviour.FieldCountX));
        pos.y = (int)((2 * pos.y + 1) * _terrainData.size.z / (2 * terrainBehaviour.FieldCountY));
        return pos;
    }

    private Vector2Int GetMapCoordFromTextureCoord(Vector2Int pos)
    {
        pos.x = (int)Mathf.Floor(pos.x * terrainBehaviour.FieldCountX / _alphamapWidth);
        pos.y = (int)Mathf.Floor(pos.y * terrainBehaviour.FieldCountY / _alphamapHeight);
        return pos;
    }

    private float GetSteepnessAlpha(int layerID, float frac, int grassLayerID)
    {
        if (layerID == grassLayerID)
        {
            return (float)(1 - frac);
        }
        else if (layerID == stone2LayerID)
        {
            return frac;
        }
        return 0;
    }

    private float GetSteepnessFactor(int x, int y)
    {
        // Get the normalized terrain coordinate that
        // corresponds to the point
        float normX = x / _alphamapWidthM1;
        float normY = y / _alphamapHeightM1;

        // Get the steepness value at the normalized coordinate
        var angle = _terrainData.GetSteepness(normY, normX);

        // Steepness is given as an angle, 0..90 degrees. Divide
        // by 90 to get an alpha blending value in the range 0..1.
        return angle / 45.0f;
    }

    private Field GetField(int x, int y)
    {
        //Shift negative to 0
        Field f = Core.Game.State.CurrentLevel.GameField.Fields.Find(field => field.Coords.X == x + terrainBehaviour.XOffset && field.Coords.Y == y + terrainBehaviour.YOffset);
        return f;
    }

}
