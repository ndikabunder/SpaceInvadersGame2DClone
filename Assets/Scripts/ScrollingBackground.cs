using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public float speed = 1.5f;
    public float tileHeight = 10f;

    void Update()
    {
        foreach (Transform tile in transform)
        {
            tile.position += Vector3.down * speed * Time.deltaTime;
            if (tile.position.y <= -tileHeight)
                tile.position += Vector3.up * tileHeight * 2f;
        }
    }
}