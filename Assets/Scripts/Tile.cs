using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;

    public int typeID;

    SpriteRenderer sr;

    private Board board;

    public void Init(int x, int y, int typeID,Board  board)
    {
        this.x = x;
        this.y = y;
        this.typeID = typeID;
        this.board = board;
        name = $"Tile_{x}_{y}";
    }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /*
    private void OnMouseDown()
    {
        SetDim(true);
        board.OnTileClicked(this);
        Debug.Log($"{this.name} is clicked");
    }
    */

    public void SetColor(Color c)
    {
        sr.color = c;
    }

    public void SetDim(bool dim)
    {
        var c = GetComponent<SpriteRenderer>().color;
        c.a = dim ? 0.33f : 1f;
        GetComponent<SpriteRenderer>().color = c;
    }
}
