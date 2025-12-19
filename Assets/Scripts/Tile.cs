using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;

    public int typeID;

    SpriteRenderer sr;

    public void Init(int x, int y, int typeID)
    {
        this.x = x;
        this.y = y;
        this.typeID = typeID;
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

    public void SetColor(Color c)
    {
        sr.color = c;
    }

    public void SetDim(bool dim)
    {
        var c = GetComponent<SpriteRenderer>().color;
        c.a = dim ? 0.35f : 1f;
        GetComponent<SpriteRenderer>().color = c;
    }
}
