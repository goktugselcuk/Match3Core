using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;

    public int typeID;

    public void Init(int x, int y, int typeID)
    {
        this.x = x;
        this.y = y;
        this.typeID = typeID;
        name = $"Tile_{x}_{y}";
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
