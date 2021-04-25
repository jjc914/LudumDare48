using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InfiniteTerrain : MonoBehaviour
{
    [SerializeField] private int enemyCount;

    private float chunkLength;

    private float flooredYPos;
    private float prevFlooredYPos;

    private int exitLocation;

    private int chunk = 1;

    [SerializeField] private int difficultyIncrement;
    [SerializeField] private int enemyCap;

    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject _playerCamera;
    [SerializeField] private Tilemap _baseTilemap;
    [SerializeField] private GameObject _treasureTilemap;
    private MapGeneration _mg;
    private TreasureController _tc;
    private EnemySpawner _es;

    private void Start()
    {
        _mg = GetComponent<MapGeneration>();
        _tc = _treasureTilemap.GetComponent<TreasureController>();
        _es = GetComponent<EnemySpawner>();

        chunkLength = _mg._baseTilemap.CellToWorld(new Vector3Int(0, _mg.mapSize.y + _mg.mapBuffer.y, 0)).y - _mg._baseTilemap.CellToWorld(new Vector3Int(0, -_mg.mapBuffer.y, 0)).y;

        flooredYPos = FloorTo(_player.transform.position.y, chunkLength, -(_mg.mapSize.y + _mg.mapBuffer.y));
        prevFlooredYPos = flooredYPos;

        InitGeneration(out exitLocation);
    }

    private void Update()
    {
        flooredYPos = FloorTo(_player.transform.position.y, chunkLength, -(_mg.mapSize.y + _mg.mapBuffer.y));

        if (flooredYPos != prevFlooredYPos)
        {
            exitLocation = GenerateNextChunk(exitLocation);
            DestroyPreviousChunk();

            _playerCamera.GetComponent<CameraController>().bottomLeftBound.y -= chunkLength;
            _playerCamera.GetComponent<CameraController>().topRightBound.y -= chunkLength;

            Vector3[] locations = GenerateEnemySpawnLocations(enemyCount);
            _es.Spawn(locations, chunk);

            StartCoroutine(CycleRespawn(10f));

            StartCoroutine(_player.GetComponent<PlayerDamageController>().IFrames(1.5f, _player.GetComponent<PlayerDamageController>().flashCount * 3));
            
            _tc.NewLevel();
        }

        prevFlooredYPos = flooredYPos;
    }

    // TODO: this floor fucntion doesn't actually work lmao but it's good enough
    private float FloorTo(float value, float toValue, float offset)
    {
        float ratio = (value + offset) / toValue;
        return (Mathf.Floor(ratio) * toValue);
    }

    // first pass of generation
    private void InitGeneration(out int exitLocation)
    {
        _mg.ClearChunk(_mg.mapPositionOffset);
        bool[,] treasureMap = new bool[_mg.mapSize.y, _mg.mapSize.x];
        bool[,] cellMap = _mg.GenerateCellMap(out treasureMap);

        int[,] objectMap = _mg.GenerateObjectMap(cellMap, treasureMap);
        
        objectMap = _mg.GenerateExit(objectMap, out exitLocation);

        int spawnLocation = Random.Range(0, _mg.mapSize.x - _mg.exitWidth);
        objectMap = _mg.GenerateEnterance(objectMap, spawnLocation);

        _player.transform.position = new Vector3(_treasureTilemap.GetComponent<Tilemap>().CellToWorld(new Vector3Int(spawnLocation, 0, 0)).x + _mg.mapPositionOffset.x / 2f, _player.transform.position.y);

        _mg.ShowMap(objectMap, _mg.mapPositionOffset);

        Vector3[] locations = GenerateEnemySpawnLocations(enemyCount);
        _es.Spawn(locations, chunk);

        _mg.seed += 1;

        cellMap = _mg.GenerateCellMap(out treasureMap);
        objectMap = _mg.GenerateObjectMap(cellMap, treasureMap);

        objectMap = _mg.GenerateEnterance(objectMap, exitLocation);
        objectMap = _mg.GenerateExit(objectMap, out exitLocation);

        StartCoroutine(_mg.ShowMapCoroutine(objectMap, _mg.mapPositionOffset + new Vector3Int(0, -(_mg.mapSize.y + _mg.mapBuffer.y * 2) * chunk, 0), _mg.generationSpeed));
        chunk++;

        StartCoroutine(_player.GetComponent<PlayerDamageController>().IFrames(1.5f, _player.GetComponent<PlayerDamageController>().flashCount * 3));

        StartCoroutine(CycleRespawn(10f));
    }

    private Vector3[] GenerateEnemySpawnLocations(int enemyCount)
    {
        Vector3[] locations = new Vector3[enemyCount];
        for (int i = 0; i < enemyCount; i++)
        {
            int k = 0;
            float x = Random.Range(_playerCamera.GetComponent<CameraController>().bottomLeftBound.x, _playerCamera.GetComponent<CameraController>().topRightBound.x);
            float y = Random.Range(_playerCamera.GetComponent<CameraController>().bottomLeftBound.y, _playerCamera.GetComponent<CameraController>().topRightBound.y);

            Vector3 pos = Camera.main.WorldToScreenPoint(new Vector3(x, y, 0f));

            while (_mg._baseTilemap.GetTile(_mg._baseTilemap.WorldToCell(new Vector3(x, y, 0f))) != null && (pos.x >= 0 && pos.x <= Screen.width && pos.y >= 0 && pos.y <= Screen.width))
            {
                x = Random.Range(_playerCamera.GetComponent<CameraController>().bottomLeftBound.x, _playerCamera.GetComponent<CameraController>().topRightBound.x);
                y = Random.Range(_playerCamera.GetComponent<CameraController>().bottomLeftBound.y, _playerCamera.GetComponent<CameraController>().topRightBound.y);

                pos = Camera.main.WorldToScreenPoint(new Vector3(x, y, 0f));

                k++;

                if (k >= 20)
                {
                    throw new System.Exception();
                }
            }
            locations[i] = new Vector3(x, y, 0);
        }
        return locations;
    }

    private IEnumerator CycleRespawn(float delay)
    {
        int c = chunk;
        yield return new WaitForSeconds(delay);
        //Debug.Log(c);
        //Debug.Log(chunk);
        if (c == chunk)
        {
            Vector3[] locations = GenerateEnemySpawnLocations(enemyCount);
            _es.Spawn(locations, chunk);

            StartCoroutine(CycleRespawn(delay));
        }
    }

    private int GenerateNextChunk(int exitLocation)
    {
        if (chunk % difficultyIncrement == 0)
        {
            enemyCount++;
            enemyCount = Mathf.Clamp(enemyCount, 0, enemyCap);

            if (chunk >= difficultyIncrement * 2f && _mg.treasureThreshold >= 5)
            {
                _mg.treasureThreshold--;
            }
            if (chunk >= difficultyIncrement * 4f && _mg.treasureThreshold <= 4)
            {
                _mg.density += 0.02f;
            }
            if (chunk >= difficultyIncrement * 6f && _mg.treasureThreshold <= 4)
            {
                _mg.density += 0.02f;
            }
        }
        bool[,] treasureMap = new bool[_mg.mapSize.y, _mg.mapSize.x];
        bool[,] cellMap = _mg.GenerateCellMap(out treasureMap);
        int[,] objectMap = _mg.GenerateObjectMap(cellMap, treasureMap);

        _mg.seed += 1;
        cellMap = _mg.GenerateCellMap(out treasureMap);
        objectMap = _mg.GenerateObjectMap(cellMap, treasureMap);

        objectMap = _mg.GenerateEnterance(objectMap, exitLocation);
        objectMap = _mg.GenerateExit(objectMap, out exitLocation);

        StartCoroutine(_mg.ShowMapCoroutine(objectMap, _mg.mapPositionOffset + new Vector3Int(0, -(_mg.mapSize.y + _mg.mapBuffer.y * 2) * chunk, 0), _mg.generationSpeed));
        chunk++;

        return exitLocation;
    }

    private void DestroyPreviousChunk()
    {
        StartCoroutine(_mg.ClearChunkCoroutine(_mg.mapPositionOffset + new Vector3Int(0, -(_mg.mapSize.y + _mg.mapBuffer.y * 2) * (chunk - 4), 0)));
    }
}
