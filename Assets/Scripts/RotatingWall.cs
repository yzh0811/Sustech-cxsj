using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RotatingWall : MonoBehaviour
{
    private Tilemap tilemap;
    private float rotationAngle = 45f; // 旋转角度
    private List<GameObject> tileObjects = new List<GameObject>(); // 存储瓦片的 GameObject 列表

    void Start()
    {
        tilemap = GetComponent<Tilemap>(); // 获取 Tilemap 组件
        CreateTileObjects(); // 创建每个瓦片的 GameObject
        RotateTilemapTiles(rotationAngle); // 调用旋转方法
    }

    void Update()
    {
        // 如果需要动态更新旋转角度，可以放在这里
    }

    // 创建每个瓦片的 GameObject
    void CreateTileObjects()
    {
        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(position))
                {
                    // 获取瓦片并创建对应的 GameObject
                    TileBase tile = tilemap.GetTile(position);
                    GameObject tileObject = new GameObject("Tile_" + x + "_" + y);
                    tileObject.transform.position = tilemap.CellToWorld(position); // 设置世界坐标
                    SpriteRenderer spriteRenderer = tileObject.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = tilemap.GetSprite(position); // 获取瓦片的图形
                    tileObjects.Add(tileObject); // 存储瓦片 GameObject
                }
            }
        }
    }

    // 旋转 Tilemap 中的所有瓦片
    void RotateTilemapTiles(float angle)
    {
        // 创建旋转矩阵
        Quaternion rotation = Quaternion.Euler(angle,0,0);

        // 遍历所有瓦片的 GameObject 并进行旋转
        foreach (GameObject tileObject in tileObjects)
        {
            // 旋转每个瓦片的 GameObject
            tileObject.transform.rotation = rotation;
        }
    }
}
