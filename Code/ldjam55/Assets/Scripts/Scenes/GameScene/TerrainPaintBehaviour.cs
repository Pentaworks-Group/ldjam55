using Assets.Scripts.Base;
using Assets.Scripts.Core.Model;
using System;
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


    private void Awake()
    {
        creepBehaviour.OnCreeperChanged.Add((field, oldCreeper) => paintCreep(field, true));
    }

    // Start is called before the first frame update
    void Start()
    {
        scaleFactorX = mainTerrain.terrainData.alphamapWidth / mainTerrain.terrainData.size.x;
        scaleFactorY = mainTerrain.terrainData.alphamapHeight / mainTerrain.terrainData.size.z;

        //Texture test
        float[,,] map = new float[mainTerrain.terrainData.alphamapWidth, mainTerrain.terrainData.alphamapHeight, mainTerrain.terrainData.alphamapLayers];

        //For each point on the alphamap
        for (int y = 0; y < mainTerrain.terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < mainTerrain.terrainData.alphamapWidth; x++)
            {
                float frac = getSteepnessFactor(x, y);
                Vector2Int mapPos = getMapCoordFromTextureCoord(new Vector2Int(y, x));
                Field field = getField(mapPos.x, mapPos.y);
                for (int z = 0; z < mainTerrain.terrainData.alphamapLayers; z++)
                {
                    int layerID = rottenGroundLayerID;
                    if (field == null)
                    {
                        layerID = grassLayerID;
                    }
                    map[x, y, z] = getSteepnessAlpha(z, frac, layerID);
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
        if (Core.Game.isRunning)
        {
            foreach (var field in Core.Game.State.CurrentLevel.GameField.Fields)
            {
                if (field.Creep != null)
                {
                    paintCreep(field);
                }
            }
        }

    }

    private void paintCreep(Field field, bool forceUpdate = false)
    {
        //TODO: add Creep def
        int sizeX = (int)(mainTerrain.terrainData.size.x / terrainBehaviour.FieldCountX);
        int radius = (int)Math.Ceiling(Mathf.Min(field.Creep.Value, 1f) * 2f * sizeX);
        if (radius != field.Creep.PaintRadiusOld || forceUpdate)
        {
            field.Creep.PaintRadiusOld = radius;

            Vector2Int mapPos = new Vector2Int((int)(field.Coords.X - terrainBehaviour.XOffset), (int)(field.Coords.Y - terrainBehaviour.YOffset));
            Vector2Int creepCenter = getTextureMapCoord(mapPos);
            int layerID = getSlimeLayerID(field.Creep.Creeper);
            int sizeY = (int)(mainTerrain.terrainData.size.z / terrainBehaviour.FieldCountY);

            paintSlimeArea(layerID, creepCenter.x - sizeX / 2, creepCenter.y - sizeY / 2, sizeX, sizeY, radius, radius / 2);
        }
    }

    private int getSlimeLayerID(Creeper creeper)
    {
        //TODO: change
        if (creeper != null && creeper.Parameters != null && creeper.Parameters.Material != null && creeper.Parameters.Material == "Water")
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

        //        width = Mathf.Min(width, mainTerrain.terrainData.alphamapWidth - x - 1);
        //        height = Mathf.Min(height, mainTerrain.terrainData.alphamapHeight - y - 1);

        //        Debug.Log("Paint Slime: " + x + ", " + y + ", " + width + ", " + height + ", " + radius);

        //        width = 80;
        //        height = 30;

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
                            map[i, j, k] = getSteepnessAlpha(k, frac, rottenGroundLayerID) * (1 - strength);
                        }

                    }
                    else
                    {
                        map[i, j, k] = getSteepnessAlpha(k, frac, rottenGroundLayerID);
                    }
                }
            }
        }
        mainTerrain.terrainData.SetAlphamaps(x, y, map);
    }

    private Vector2Int getTextureMapCoord(Vector2Int pos)
    {
        pos.x = (int)((2 * pos.x + 1) * mainTerrain.terrainData.size.x / (2 * terrainBehaviour.FieldCountX));
        pos.y = (int)((2 * pos.y + 1) * mainTerrain.terrainData.size.z / (2 * terrainBehaviour.FieldCountY));
        return pos;
    }

    private Vector2Int getMapCoordFromTextureCoord(Vector2Int pos)
    {
        pos.x = (int)Mathf.Floor(pos.x * terrainBehaviour.FieldCountX / mainTerrain.terrainData.alphamapWidth);
        pos.y = (int)Mathf.Floor(pos.y * terrainBehaviour.FieldCountY / mainTerrain.terrainData.alphamapHeight);
        return pos;
    }

    private float getSteepnessAlpha(int layerID, float frac, int grassLayerID)
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

    private Field getField(int x, int y)
    {
        //Shift negative to 0
        Field f = Core.Game.State.CurrentLevel.GameField.Fields.Find(field => field.Coords.X == x + terrainBehaviour.XOffset && field.Coords.Y == y + terrainBehaviour.YOffset);
        return f;
    }

}
