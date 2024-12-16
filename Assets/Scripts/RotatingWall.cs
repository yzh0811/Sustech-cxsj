using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RotatingWall : MonoBehaviour
{
    private Tilemap tilemap;
    private float rotationAngle = 45f; // ��ת�Ƕ�
    private List<GameObject> tileObjects = new List<GameObject>(); // �洢��Ƭ�� GameObject �б�

    void Start()
    {
        tilemap = GetComponent<Tilemap>(); // ��ȡ Tilemap ���
        CreateTileObjects(); // ����ÿ����Ƭ�� GameObject
        RotateTilemapTiles(rotationAngle); // ������ת����
    }

    void Update()
    {
        // �����Ҫ��̬������ת�Ƕȣ����Է�������
    }

    // ����ÿ����Ƭ�� GameObject
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
                    // ��ȡ��Ƭ��������Ӧ�� GameObject
                    TileBase tile = tilemap.GetTile(position);
                    GameObject tileObject = new GameObject("Tile_" + x + "_" + y);
                    tileObject.transform.position = tilemap.CellToWorld(position); // ������������
                    SpriteRenderer spriteRenderer = tileObject.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = tilemap.GetSprite(position); // ��ȡ��Ƭ��ͼ��
                    tileObjects.Add(tileObject); // �洢��Ƭ GameObject
                }
            }
        }
    }

    // ��ת Tilemap �е�������Ƭ
    void RotateTilemapTiles(float angle)
    {
        // ������ת����
        Quaternion rotation = Quaternion.Euler(angle,0,0);

        // ����������Ƭ�� GameObject ��������ת
        foreach (GameObject tileObject in tileObjects)
        {
            // ��תÿ����Ƭ�� GameObject
            tileObject.transform.rotation = rotation;
        }
    }
}
