using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



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


    [Header("TouchInput")]

    private float swipeThreshold = 30f;

    [SerializeField] private LayerMask tileLayer;
    private Vector2 pressedPos;
    private Tile pressedTile;
    private bool isSwipeDone;

    
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
            tile.SetDim(true);
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


        if (LocalSwapMatchCheck(selectedTileA,selectedTileB))
        {
            yield return BoardResolver();
        }

        else
        {
            yield return AnimateTileSwap(selectedTileA, selectedTileB, tileA_originalPos, tileB_originalPos);
            
            LogicSwapHelper(selectedTileA, selectedTileB);
        }

        if (selectedTileA!=null)
        {
            selectedTileA.SetDim(false);
        }

        if (selectedTileB!= null)
        {
            selectedTileB.SetDim(false);
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

    private Tile TileFromScreenPos(Vector2 pressedPos)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(pressedPos);  // convert the clicked screen position to actual unity world position.

        Collider2D hit = Physics2D.OverlapPoint(worldPos, tileLayer);            //  hit test using colliders

        if (hit==null)
        {
            return null;
        }

        return hit.GetComponent<Tile>();      // return the tile if hit happens

    }

    private void SwipeConnecter(Tile startTile, Vector2 delta) // connect the swipe motion from starting tile to target tile and start the swap
    {
        int directionX = 0;
        int directionY = 0;

        if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
        {
            directionY = delta.y > 0 ? 1 : -1;
        }

        else
        {
            directionX = delta.x > 0 ? 1 : -1;
        }

        int targetX = startTile.x + directionX;
        int targetY = startTile.y + directionY;

        if (targetX < 0 || targetX >= width || targetY < 0 || targetY >= height) //board boundary check
        {
            return;

        }

        Tile targetedNeighbor = tiles[targetX, targetY];

        if (targetedNeighbor == null) return;

        selectedTileA = startTile;
        selectedTileB = targetedNeighbor;

        SwapTiles();

    }

    private void TouchInputHandler(Touch touch)
    {
        if(touch.phase == TouchPhase.Began)
        {
            pressedPos = touch.position;
            pressedTile = TileFromScreenPos(pressedPos);
            isSwipeDone = false;
            return;
        }

        if(touch.phase  == TouchPhase.Moved)
        {
            if (pressedTile == null) return;
            if (isSwipeDone) return;

            Vector2 deltaPos = touch.position - pressedPos;

            if (deltaPos.magnitude < swipeThreshold) return;

            SwipeConnecter(pressedTile, deltaPos);

            isSwipeDone = true; // allow only one swipe per touch
            return;
        }

        if( touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            pressedTile = null;    // reset selected tile
            isSwipeDone = false; // reset this to allow future swipes
            return;
        }
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pressedPos = Input.mousePosition;
            pressedTile = TileFromScreenPos(pressedPos);
            isSwipeDone = false;
        }

        if (Input.GetMouseButton(0))
        {
            if (pressedTile == null) return;
            if (isSwipeDone) return;

            Vector2 delta = (Vector2)Input.mousePosition - pressedPos;
            if (delta.magnitude < swipeThreshold) return;

            SwipeConnecter(pressedTile, delta);
            isSwipeDone = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            pressedTile = null;
            isSwipeDone = false;
        }
    }

    private bool InboundCheck(int x , int y)
    {
        if(x<0 || x>=width || y<0 || y >= height) { return false; }
        else { return true; }
    }

    private List<Tile> DirectionalMatchChecker(Tile startTile, int dx , int dy)
    {
        List<Tile> collectedTiles = new List<Tile>();
        if (startTile != null)
        {

            int targetType = startTile.typeID;

            int currentX = startTile.x + dx; // dx and dy help us to give a direction to this search (1,0) means right (0,-1) means down
            int currentY = startTile.y + dy;


            // if it's still inside board, not null and same color. go on
            while (InboundCheck(currentX, currentY) && tiles[currentX, currentY] != null && tiles[currentX, currentY].typeID == targetType) 
            {
                collectedTiles.Add(tiles[currentX, currentY]);
                currentX += dx;
                currentY += dy;

            }

        }

        return collectedTiles;
        
    }

    private HashSet<Tile> FourWayMatchCollecter(Tile centerTile)
    {
        HashSet<Tile> collectedMatchingTiles = new HashSet<Tile>();

        if (centerTile != null) // null check
        {
            // horizontal line check //////

            List<Tile> tilesOnLeft = DirectionalMatchChecker(centerTile, -1, 0); // go left starting from center tile. until it's broken with a different colored tile.
            List<Tile> tilesOnRight = DirectionalMatchChecker(centerTile, 1, 0); // go right starting from center tile. until it's broken with a different colored tile.

            if (1 + tilesOnLeft.Count + tilesOnRight.Count >= 3) // if center tile + streak on left + streak on right equal or greater than 3 it is a match
            {
                collectedMatchingTiles.UnionWith(tilesOnLeft); // add tiles on right that form a match

                collectedMatchingTiles.Add(centerTile); // add center tile

                collectedMatchingTiles.UnionWith(tilesOnRight); // add tiles on left that form a match
            }

            // vertical line  check //////

            List<Tile> tilesOnUp = DirectionalMatchChecker(centerTile, 0, 1);  // go up starting from center tile. until it's broken with a different colored tile.
            List<Tile> tilesOnDown = DirectionalMatchChecker(centerTile, 0, -1);   // go down starting from center tile. until it's broken with a different colored tile.

            if (1 + tilesOnUp.Count + tilesOnDown.Count >= 3) // if center tile + streak upward + streak downward equal or greater than 3 it is a match
            {
                collectedMatchingTiles.UnionWith(tilesOnDown); // add tiles above that form a match

                collectedMatchingTiles.Add(centerTile);

                collectedMatchingTiles.UnionWith(tilesOnUp); // add tiles below that form a match
            }


        }

        return collectedMatchingTiles;

    }

    private bool LocalSwapMatchCheck(Tile tileA , Tile tileB)
    {
        HashSet<Tile> matchesOfA = FourWayMatchCollecter(tileA); //matches occured by first tiles' position change
        HashSet<Tile> matchesOfB = FourWayMatchCollecter(tileB);  //matches occured by second tiles' position change

        matchesOfA.UnionWith(matchesOfB); // merge those 2 hashsets for total matches occured.

        return matchesOfA.Count > 0; // if there is any match from any of the tiles, it's a valid swap
       
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInputLock) return;

        if (Input.touchCount > 0)
        {
            TouchInputHandler(Input.GetTouch(0));
        }

        else { HandleMouse(); }
    }
}
