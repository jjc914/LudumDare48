using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class Tiles
{
    [SerializeField] public TileBase topBleed;
    [SerializeField] public TileBase top;
    [SerializeField] public TileBase middle;
    [SerializeField] public TileBase middle2;
    [SerializeField] public TileBase bottom;
    [SerializeField] public TileBase bottomBleed;
    [SerializeField] public TileBase background;
    [SerializeField] public TileBase unbreakable;
    [SerializeField] public TileBase topTreasure;
    [SerializeField] public TileBase bottomTreasure;
}

public class MapGeneration : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private bool update;
    [SerializeField] private bool randomSeed;
    [SerializeField] public int seed;
    [SerializeField] public Vector2Int mapSize;
    [SerializeField] public Vector2Int mapBuffer;

    [SerializeField] public Vector3Int mapPositionOffset;

    [SerializeField] public int exitWidth;
    [SerializeField] private Vector2Int mapBoarder;

    [SerializeField] public Tilemap _baseTilemap;
    [SerializeField] private Tilemap _bleedTilemap;
    [SerializeField] private Tilemap _backgroundTilemap;
    [SerializeField] private Tilemap _treasureTilemap;
    [SerializeField] public Tiles _tiles;

    [Header("Simulation Settings")]
    [Range(0,1)]
    [SerializeField] public float density;
    [SerializeField] private int birthThreshold;
    [SerializeField] private int deathThreshold;
    [SerializeField] public int treasureThreshold;
    [SerializeField] private int simSteps;

    [Header("Generation Optimization")]
    [SerializeField] public int generationSpeed;

    private bool[,] cellMap;
    private bool[,] treasureMap;
    private int[,] objectMap;

    private void OnValidate()
    {
        if (update)
        {
            ClearChunk(mapPositionOffset);
            cellMap = GenerateCellMap(out treasureMap);
            objectMap = GenerateObjectMap(cellMap, treasureMap);

            int temp;
            objectMap = GenerateExit(objectMap, out temp);

            ShowMap(objectMap, mapPositionOffset);

            update = false;
        }
    }

    private void Awake()
    {
        if (randomSeed)
        {
            seed = Random.Range(0, 1000000);
        }
        Random.InitState(seed);

        treasureMap = new bool[mapSize.y, mapSize.x];
    }

    #region CELLULAR_AUTOMATA
    // Uses cellular automata
    private bool[,] InitMap(float density)
    {
        bool[,] cellMap = new bool[mapSize.y, mapSize.x];
        
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                float val = Random.Range(0f, 1f);
                if (val < density)
                {
                    cellMap[y, x] = true;
                }
                if (x <= 0 || x >= mapSize.x - 1 || y <= 0 || y >= mapSize.y - 1)
                {
                    cellMap[y, x] = true;
                }
            }
        }

        return cellMap;
    }

    public bool[,] GenerateCellMap(out bool[,] treasureMap)
    {
        bool[,] cellMap = InitMap(density);
        treasureMap = new bool[mapSize.y, mapSize.x];

        for (int i = 0; i < simSteps - 1; i++)
        {
            cellMap = RunSimulationStep(cellMap);
        }
        cellMap = RunSimulationStep(cellMap, out treasureMap);

        return cellMap;
    }

    private bool[,] RunSimulationStep(bool[,] cellMap)
    {
        bool[,] newMap = new bool[mapSize.y, mapSize.x];
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                int neighbors = CountLivingNeighbors(cellMap, x, y);
                if (cellMap[y, x] && neighbors < deathThreshold)
                {
                    newMap[y, x] = false;
                }
                else if (!cellMap[y, x] && neighbors > birthThreshold)
                {
                    newMap[y, x] = true;
                }
                else
                {
                    newMap[y, x] = cellMap[y, x];
                }
            }
        }
        return newMap;
    }

    private bool[,] RunSimulationStep(bool[,] cellMap, out bool[,] treasureMap)
    {
        bool[,] newMap = new bool[mapSize.y, mapSize.x];
        treasureMap = new bool[mapSize.y, mapSize.x];
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                int neighbors = CountLivingNeighbors(cellMap, x, y);
                if (cellMap[y, x] && neighbors < deathThreshold)
                {
                    newMap[y, x] = false;
                }
                else if (!cellMap[y, x] && neighbors > birthThreshold)
                {
                    newMap[y, x] = true;
                }
                else
                {
                    newMap[y, x] = cellMap[y, x];
                }

                if (!cellMap[y, x] && neighbors > treasureThreshold)
                {
                    treasureMap[y, x] = true;
                }
            }
        }
        return newMap;
    }

    private int CountLivingNeighbors(bool[,] map, int xp, int yp)
    {
        int living = 0;
        for (int y = yp - 1; y <= yp + 1; y++)
        {
            for (int x = xp - 1; x <= xp + 1; x++)
            {
                if (!(x == xp && y == yp))
                {
                    if (!(x < 0 || x >= mapSize.x || y < 0 || y >= mapSize.y))
                    {
                        if (map[y, x])
                        {
                            living++;
                        }
                    }
                    else
                    {
                        living++;
                    }
                }
            }
        }
        return living;
    }
    #endregion

    /*
     * 0: empty
     * 1: top
     * 2: middle
     * 3: bottom
     * 4: top bleed
     * 5: bottom bleed
     * 6: treasure bottom
     * 7: treasure top
     * 8: extra middle
     */
    public int[,] GenerateObjectMap(bool[,] cellMap, bool[,] treasureMap)
    {
        int[,] objectMap = new int[mapSize.y, mapSize.x];
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                if (cellMap[y, x])
                {
                    // top empty
                    if (y + 1 < mapSize.y && !cellMap[y + 1, x])
                    {
                        objectMap[y, x] = 1;
                    }
                    // bottom empty
                    else if (y - 1 > 0 && !cellMap[y - 1, x])
                    {
                        objectMap[y, x] = 3;
                    }
                    // filled
                    else
                    {
                        if (Random.Range(0f, 1f) <= 0.985f)
                        {
                            objectMap[y, x] = 2;
                        }
                        else
                        {
                            objectMap[y, x] = 8;
                        }
                    }
                }
                else
                {
                    // bottom filled
                    if (y - 1 > 0 && cellMap[y - 1, x] && !treasureMap[y - 1, x])
                    {
                        objectMap[y, x] = 4;
                    }
                    // top filled
                    else if (y + 1 < mapSize.y && cellMap[y + 1, x]) 
                    {
                        objectMap[y, x] = 5;
                    }
                }

                if (treasureMap[y, x])
                {
                    if (cellMap[y - 1, x])
                    {
                        objectMap[y, x] = 7;
                    }
                    else if (cellMap[y + 1, x])
                    {
                        objectMap[y, x] = 6;
                    }
                }
            }
        }

        return objectMap;
    }

    public int[,] GenerateExit(int[,] objectMap, out int exitLocation)
    {
        exitLocation = Random.Range(3, mapSize.x - exitWidth - 3);
        objectMap[0, exitLocation] = -1;

        return objectMap;
    }

    public int[,] GenerateEnterance(int[,] objectMap, int exitLocation)
    {
        objectMap[mapSize.y - 1, exitLocation] = -2;
        return objectMap;
    }

    public void ShowMap(int[,] objectMap, Vector3Int mapPositionOffset)
    {
        for (int y = -mapBuffer.y; y < mapSize.y + mapBuffer.y - 1; y++)
        {
            for (int x = -mapBuffer.x; x < mapSize.x + mapBuffer.x; x++)
            {
                if (y >= 0 && y < mapSize.y && x >= 0 && x < mapSize.x)
                {
                    _backgroundTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.background);
                    continue;
                }
                else if (x <= -mapBoarder.x || x >= mapSize.x + mapBoarder.x -1 || y <= -mapBoarder.y || y >= mapSize.y + mapBoarder.y - 1)
                {
                    _baseTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.unbreakable);
                }
                else
                {
                    _backgroundTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.background);
                    _baseTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.middle);
                }
            }
        }
            
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                _backgroundTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.background);
                switch (objectMap[y, x])
                {
                    case -2:
                        // generate enterance
                        for (int exitY = y; exitY <= y + mapBuffer.y; exitY++)
                        {
                            for (int exitX = x; exitX < x + exitWidth; exitX++)
                            {
                                _baseTilemap.SetTile(new Vector3Int(exitX, exitY, 0) + mapPositionOffset, null);
                                _backgroundTilemap.SetTile(new Vector3Int(exitX, exitY, 0) + mapPositionOffset, _tiles.background);
                                _treasureTilemap.SetTile(new Vector3Int(exitX, exitY, 0) + mapPositionOffset, null);
                                _bleedTilemap.SetTile(new Vector3Int(exitX, exitY - 1, 0) + mapPositionOffset, null);
                            }
                        }
                        for (int xi = 1; xi < exitWidth; xi++)
                        {
                            objectMap[y, x + xi] = 0;
                            objectMap[y, x + xi] = 0;
                        }

                        int exitYObjectMap = y;
                        int sum = exitWidth;
                        while (sum >= exitWidth)
                        {
                            for (int exitX = x; exitX < x + exitWidth; exitX++)
                            {
                                _baseTilemap.SetTile(new Vector3Int(exitX, exitYObjectMap, 0) + mapPositionOffset, null);
                                _backgroundTilemap.SetTile(new Vector3Int(exitX, exitYObjectMap, 0) + mapPositionOffset, _tiles.background);
                                _treasureTilemap.SetTile(new Vector3Int(exitX, exitYObjectMap, 0) + mapPositionOffset, null);
                                _bleedTilemap.SetTile(new Vector3Int(exitX, exitYObjectMap - 1, 0) + mapPositionOffset, null);
                            }

                            sum = 0;
                            for (int exitX = x; exitX < x + exitWidth; exitX++)
                            {
                                sum += objectMap[exitYObjectMap - 1, exitX] != 0 ? 1 : 0;
                            }

                            exitYObjectMap--;
                        }
                        break;
                    case -1:
                        // generate exit
                        for (int exitY = y; exitY > y - 1 - mapBuffer.y; exitY--)
                        {
                            for (int exitX = x; exitX < x + exitWidth; exitX++)
                            {
                                _baseTilemap.SetTile(new Vector3Int(exitX, exitY, 0) + mapPositionOffset, null);
                                _backgroundTilemap.SetTile(new Vector3Int(exitX, exitY, 0) + mapPositionOffset, _tiles.background);
                                _treasureTilemap.SetTile(new Vector3Int(exitX, exitY, 0) + mapPositionOffset, null);
                                _bleedTilemap.SetTile(new Vector3Int(exitX, exitY + 1, 0) + mapPositionOffset, null);
                            }
                        }
                        for (int i = 1; i < exitWidth - 1; i++)
                        {
                            objectMap[y, x + i] = 0;
                            objectMap[y, x + i] = 0;
                        }
                        break;
                    case 0:
                        break;
                    case 1:
                        _baseTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.top);
                        break;
                    case 2:
                        _baseTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.middle);
                        break;
                    case 3:
                        _baseTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.bottom);
                        break;
                    case 4:
                        _bleedTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.topBleed);
                        break;
                    case 5:
                        _bleedTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.bottomBleed);
                        break;
                    case 6:
                        _treasureTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.topTreasure);
                        break;
                    case 7:
                        _treasureTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.bottomTreasure);
                        break;
                    case 8:
                        _baseTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.middle2);
                        break;
                    default:
                        throw new System.Exception();
                }
            }
        }
    }

    public IEnumerator ShowMapCoroutine(int[,] objectMap, Vector3Int mapPositionOffset, int generationSpeed)
    {
        int i = 0;
        for (int y = -mapBuffer.y; y < mapSize.y + mapBuffer.y; y++)
        {
            for (int x = -mapBuffer.x; x < mapSize.x + mapBuffer.x; x++)
            {
                if (y >= 0 && y < mapSize.y && x >= 0 && x < mapSize.x)
                {
                    _backgroundTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.background);
                    continue;
                }
                else
                {
                    if (x <= -mapBoarder.x || x >= mapSize.x + mapBoarder.x - 1 || y <= -mapBoarder.y || y >= mapSize.y + mapBoarder.y - 1)
                    {
                        _baseTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.unbreakable);
                    }
                    else
                    {
                        _backgroundTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.background);
                        _baseTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.middle);
                    }

                    if (i % generationSpeed == 0)
                    {
                        yield return new WaitForEndOfFrame();
                    }

                    i++;
                }
            }
        }

        i = 0;
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                _backgroundTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.background);
                switch (objectMap[y, x])
                {
                    case -2:
                        // generate enterance
                        for (int exitY = y; exitY <= y + mapBuffer.y; exitY++)
                        {
                            for (int exitX = x; exitX < x + exitWidth; exitX++)
                            {
                                _baseTilemap.SetTile(new Vector3Int(exitX, exitY, 0) + mapPositionOffset, null);
                                _backgroundTilemap.SetTile(new Vector3Int(exitX, exitY, 0) + mapPositionOffset, _tiles.background);
                                _treasureTilemap.SetTile(new Vector3Int(exitX, exitY, 0) + mapPositionOffset, null);
                                _bleedTilemap.SetTile(new Vector3Int(exitX, exitY - 1, 0) + mapPositionOffset, null);
                            }
                        }
                        for (int xi = 1; xi < exitWidth; xi++)
                        {
                            objectMap[y, x + xi] = 0;
                            objectMap[y, x + xi] = 0;
                        }

                        int exitYObjectMap = y;
                        int sum = exitWidth;
                        while (sum >= exitWidth) 
                        {
                            for (int exitX = x; exitX < x + exitWidth; exitX++)
                            {
                                _baseTilemap.SetTile(new Vector3Int(exitX, exitYObjectMap, 0) + mapPositionOffset, null);
                                _backgroundTilemap.SetTile(new Vector3Int(exitX, exitYObjectMap, 0) + mapPositionOffset, _tiles.background);
                                _treasureTilemap.SetTile(new Vector3Int(exitX, exitYObjectMap, 0) + mapPositionOffset, null);
                                _bleedTilemap.SetTile(new Vector3Int(exitX, exitYObjectMap - 1, 0) + mapPositionOffset, null);
                            }

                            sum = 0;
                            for (int exitX = x; exitX < x + exitWidth; exitX++)
                            {
                                if (exitYObjectMap - 1 >= 0)
                                {
                                    sum += objectMap[exitYObjectMap - 1, exitX] != 0 ? 1 : 0;
                                }
                            }

                            exitYObjectMap--;
                        } 
                        break;
                    case -1:
                        // generate exit
                        for (int exitY = y; exitY > y - 1 - mapBuffer.y; exitY--)
                        {
                            for (int exitX = x; exitX < x + exitWidth; exitX++)
                            {
                                _baseTilemap.SetTile(new Vector3Int(exitX, exitY, 0) + mapPositionOffset, null);
                                _backgroundTilemap.SetTile(new Vector3Int(exitX, exitY, 0) + mapPositionOffset, _tiles.background);
                                _treasureTilemap.SetTile(new Vector3Int(exitX, exitY, 0) + mapPositionOffset, null);
                                _bleedTilemap.SetTile(new Vector3Int(exitX, exitY + 1, 0) + mapPositionOffset, null);
                            }
                        }
                        objectMap[y, x + 1] = 0;
                        objectMap[y, x + 2] = 0;
                        break;
                    case 0:
                        break;
                    case 1:
                        _baseTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.top);
                        break;
                    case 2:
                        _baseTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.middle);
                        break;
                    case 3:
                        _baseTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.bottom);
                        break;
                    case 4:
                        _bleedTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.topBleed);
                        break;
                    case 5:
                        _bleedTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.bottomBleed);
                        break;
                    case 6:
                        _treasureTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.topTreasure);
                        break;
                    case 7:
                        _treasureTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.bottomTreasure);
                        break;
                    case 8:
                        _baseTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, _tiles.middle2);
                        break;
                    default:
                        throw new System.Exception();
                }

                if (i % generationSpeed == 0)
                {
                    yield return new WaitForEndOfFrame();
                }

                i++;
            }
        }
    }

    public void ClearChunk(Vector3Int mapPositionOffset)
    {
        for (int y = -mapBuffer.y; y <= mapSize.y + mapBuffer.y; y++)
        {
            for (int x = -mapBuffer.x * 2; x <= mapSize.x + mapBuffer.x; x++)
            {
                _baseTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, null);
                _bleedTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, null);
                _backgroundTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, null);
                _treasureTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, null);
            }
        }
    }

    public IEnumerator ClearChunkCoroutine(Vector3Int mapPositionOffset)
    {
        int i = 0;
        for (int y = -mapBuffer.y; y <= mapSize.y + mapBuffer.y; y++)
        {
            for (int x = -mapBuffer.x * 2; x <= mapSize.x + mapBuffer.x; x++)
            {
                _baseTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, null);
                _bleedTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, null);
                _backgroundTilemap.SetTile(new Vector3Int(x, y, 0) + mapPositionOffset, null);

                if (i % generationSpeed == 0)
                {
                    yield return new WaitForEndOfFrame();
                }
                i++;
            }
        }
    }
}
