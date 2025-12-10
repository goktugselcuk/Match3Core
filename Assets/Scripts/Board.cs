using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Next step → match detection

public class Board : MonoBehaviour
{
    [Header("BoardDimensions")]

    public int height = 8;
    public int width = 8;


    [Header("TileSetup")]

    [SerializeField] Tile tilePrefab;
    public float tileSize = 1f;

    private Tile[,] tiles;

    
    // Start is called before the first frame update
    void Start()
    {
        GenerateBoard();
    }

    private void GenerateBoard()
    {
        float offsetX = -(width - 1) * tileSize * 0.5f;
        float offsetY = -(height - 1) * tileSize * 0.5f;

        tiles = new Tile[height, width];

        for(int x=0; x < height; x++)
        {
            for(int y = 0; y < width; y++)
            {
                Vector3 spawnPos = new Vector3(x * tileSize + offsetX, y* tileSize + offsetY, 0f);

                Tile newTile = Instantiate(tilePrefab, spawnPos, Quaternion.identity,transform);

                newTile.Init(x, y, 0);

                tiles[x, y] = newTile;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
