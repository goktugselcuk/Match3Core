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

    [Header("ColorSetup")]

    public Color[] tileColors;


    // Start is called before the first frame update
    void Start()
    {
        GenerateBoard();
        var tileMatches=FindAllMatches();

        foreach( Tile tile in tileMatches)
        {
            tile.SetDim(true);
        }



        ClearAllMatches(tileMatches);
        ApplyGravity();
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

                int randomType = UnityEngine.Random.Range(0, tileColors.Length);
                newTile.Init(x, y, randomType);
                newTile.SetColor(tileColors[randomType]);

                tiles[x, y] = newTile;
            }
        }
    }

    public HashSet<Tile> FindAllMatches()
    {
       
        HashSet<Tile> matches = new HashSet<Tile>();

        // HORIZONTAL CHECK

        for(int y = 0; y< height; y++)
        {
            int runStreak = 1;
            for (int x=1; x < width; x++)
            {
                if (tiles[x, y].typeID == tiles[x-1, y].typeID)
                {
                    runStreak++;
                }

                else
                {
                    if (runStreak >= 3)
                    {
                        for(int k = 0; k < runStreak; k++)
                        {
                            matches.Add(tiles[x - 1 - k, y]);
                        }
                       
                    }


                    runStreak = 1;
                }
            }

             // END OF THE ROW HANDLING
            if (runStreak >= 3)
            {
                for (int k = 0; k < runStreak; k++)
                {
                    matches.Add(tiles[width - 1 - k, y]);
                }
            }

        }

        // VERTICAL CHECK
        
        for(int x = 0; x < width; x++)
        {
            int runStreak = 1;

            for(int y = 1; y < height; y++)
            {
                if(tiles[x,y].typeID == tiles[x, y - 1].typeID)
                {
                    runStreak++;
                }

                else
                {
                    if (runStreak >= 3)
                    {
                        for(int k = 0; k < runStreak; k++)
                        {
                            matches.Add(tiles[x, y - 1 - k]);
                        }
                    }
                    runStreak = 1;
                }
            }

            // END OF COLUMN HANDLING


            if(runStreak >= 3)
                    {
                for (int k = 0; k < runStreak; k++)
                {
                    matches.Add(tiles[x, height- 1 - k]);
                }
            }


        }

        return matches;
    }

    // Clear all tiles in matches from the array and from the scene
    void ClearAllMatches(HashSet<Tile> matches)
    {
        foreach(Tile tile in matches)
        {
            int x = tile.x;
            int y = tile.y;

            tiles[x, y] = null;
            Destroy(tile.gameObject);
        }
    }

    void ApplyGravity()
    {
        for(int x=0; x< width; x++)
        {
            for(int y=0; y < height; y++)
            {
                if (tiles[x, y] == null)
                {
                    for(int aboveY=y; aboveY<height; aboveY++)
                    {
                        if (tiles[x, aboveY] != null)
                        {
                            tiles[x, y] = tiles[x, aboveY];
                            tiles[x, aboveY] = null;
                            tiles[x, y].y = y;

                            tiles[x, y].transform.position = new Vector3(tiles[x, y].transform.position.x,
                                                                                                tiles[x, y].transform.position.y - (aboveY - y) * tileSize, 0f);

                            break;
                        }
                    }
                }
            }
        }
    }


    private Vector3 GetWorldPosition(int x, int y)
    {
        float offsetX = -(width - 1) * tileSize * 0.5f;
        float offsetY = -(height - 1) * tileSize * 0.5f;

        return new Vector3(
            x * tileSize + offsetX,
            y * tileSize + offsetY,
            0f
        );
    }

    public void RefillBoard()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y<height; y++)
            {
                if (tiles[x, y] == null)
                {
                    Tile newTile = Instantiate(tilePrefab, GetWorldPosition(x, y), Quaternion.identity, transform);
                    int randomType = UnityEngine.Random.Range(0, tileColors.Length);

                    newTile.Init(x, y, randomType);
                    newTile.SetColor(tileColors[randomType]);

                    tiles[x, y] = newTile;
                }
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
