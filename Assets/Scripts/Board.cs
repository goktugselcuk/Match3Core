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

    Tile selectedTileA=null;
    Tile selectedTileB=null;

    bool playerInputLock = false; // to limit selected tiles (max 2)

    int swapSpeed = 5;
        
    // Start is called before the first frame update
    void Start()
    {
        GenerateBoard();
        
        StartCoroutine(BoardResolver());
    }

    private void GenerateBoard()
    {
        float offsetX = -(width - 1) * tileSize * 0.5f;
        float offsetY = -(height - 1) * tileSize * 0.5f;

        tiles = new Tile[width, height];

        for(int x=0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Vector3 spawnPos = new Vector3(x * tileSize + offsetX, y* tileSize + offsetY, 0f);

                Tile newTile = Instantiate(tilePrefab, spawnPos, Quaternion.identity,transform);

                int randomType = UnityEngine.Random.Range(0, tileColors.Length);
                newTile.Init(x, y, randomType,this);
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
                Tile current = tiles[x, y];
                Tile previous = tiles[x - 1, y];

                if (current!=null && previous !=  null && current.typeID == previous.typeID)
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
                Tile current = tiles[x, y];
                Tile previous = tiles[x , y-1];

                if (current != null && previous != null && current.typeID == previous.typeID)
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
                    for(int aboveY=y+1; aboveY<height; aboveY++)
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

                    newTile.Init(x, y, randomType,this);
                    newTile.SetColor(tileColors[randomType]);

                    tiles[x, y] = newTile;
                }
            }
        }
    }

    public IEnumerator BoardResolver()
    {
        while (true)
        {
            HashSet<Tile> matches = FindAllMatches();

            if (matches.Count==0)
            {
                yield break;
            }

            ClearAllMatches(matches);
            ApplyGravity();
            RefillBoard();

            yield return null;
        }
    }


    public void OnTileClicked(Tile tile)
    {
        if (!playerInputLock)
        {
            if (selectedTileA == null)
            {
                selectedTileA = tile;
            }
            else
            {
                selectedTileB = tile;
                playerInputLock = true;
                SwapTiles();
            }
        }
       
    }

    private bool TileAdjacencyCheck(Tile tileA, Tile tileB)
    {
        float distance=Math.Abs(tileA.x - tileB.x) + Math.Abs(tileA.y - tileB.y);

        return distance == 1;
    }

    private void SwapTiles()
    {

        if (TileAdjacencyCheck(selectedTileA, selectedTileB))
        {


        

            StartCoroutine(TryTileSwap());


        }
        
    }

    private IEnumerator AnimateTileSwap(Tile tileA, Tile tileB,  Vector3 targetA, Vector3 targetB)
    {
        

        while (Vector3.Distance(tileA.transform.position,targetA) >0.01f || Vector3.Distance(tileB.transform.position, targetB) > 0.01 )
        {
            tileA.transform.position = Vector3.MoveTowards(tileA.transform.position, targetA, swapSpeed * Time.deltaTime);
            tileB.transform.position = Vector3.MoveTowards(tileB.transform.position, targetB, swapSpeed * Time.deltaTime);
            yield return null;
        }
        tileA.transform.position = targetA;
        tileB.transform.position = targetB;


    }

    private IEnumerator TryTileSwap()
    {
        playerInputLock = true;
        int firstX = selectedTileA.x;
        int firstY = selectedTileA.y;

        int secondX = selectedTileB.x;
        int secondY = selectedTileB.y;

        Vector3 tileA_originalPos = selectedTileA.transform.position;
        Vector3 tileB_originalPos = selectedTileB.transform.position ;

        Vector3 targetA = GetWorldPosition(secondX, secondY);
        Vector3 targetB = GetWorldPosition(firstX, firstY);

        LogicSwapHelper(selectedTileA, selectedTileB);




        yield return AnimateTileSwap(selectedTileA, selectedTileB, targetA, targetB);

        if (FindAllMatches().Count > 0)
        {
            yield return BoardResolver();
        }

        else
        {
            yield return AnimateTileSwap(selectedTileA, selectedTileB, tileA_originalPos, tileB_originalPos);
            LogicSwapHelper(selectedTileA, selectedTileB);
        }

        selectedTileA = null;
        selectedTileB = null;
        playerInputLock = false;
    }

    void LogicSwapHelper(Tile tileA, Tile tileB)
    {
         int tempX;
        int tempY;
        Tile tempTile;

        int firstX = tileA.x;
        int firstY = tileA.y;


        int secondX = tileB.x;
        int secondY = tileB.y;

        tempTile = tiles[firstX, firstY];
        tiles[firstX, firstY] = tiles[secondX, secondY];
        tiles[secondX, secondY] = tempTile;

        tempX = tileA.x;
        tileA.x = tileB.x;
        tileB.x = tempX;

        tempY = tileA.y;
        tileA.y = tileB.y;
        tileB.y = tempY;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
