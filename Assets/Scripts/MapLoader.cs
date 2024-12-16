using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapLoader : MonoBehaviour
{
    // Start is called before the first frame update
    public Tilemap tilemap;
    public Tile defaultTile;

    void Start()
    {
        LoadAndGenerateTilemap();
    }
    private void LoadAndGenerateTilemap()
    {
        // ֱ��ʹ�����·��  
        string imagePath = "D:\\����ʵ��\\AgentTest\\Assets\\Images\\generated_image.png";

        // ����ļ��Ƿ����  
        if (!File.Exists(imagePath))
        {
            Debug.LogError("δ�ҵ�ͼ���ļ�: " + imagePath);
            return;
        }

        Debug.Log("����ͼ��: " + imagePath);

        // ��������  
        byte[] fileData = File.ReadAllBytes(imagePath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(fileData);

        // ��ѡ�����ͼƬ̫���ʵ�����  
        TextureScale.Bilinear(texture, 256, 256);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                Color pixelColor = texture.GetPixel(x, y);

                // ���Ե���͸������ֵ  
                if (pixelColor.a > 0.1f)
                {
                    // �����µ�Tile��������ɫ  
                    Tile newTile = ScriptableObject.CreateInstance<Tile>();
                    newTile.color = pixelColor;

                    // �����Ĭ��Tile�������侫��  
                    if (defaultTile != null)
                    {
                        newTile.sprite = defaultTile.sprite;
                    }
                    else
                    {
                        // ����������������ΪTile  
                        Texture2D tileTexture = new Texture2D(1, 1);
                        tileTexture.SetPixel(0, 0, pixelColor);
                        tileTexture.Apply();

                        newTile.sprite = Sprite.Create(
                            tileTexture,
                            new Rect(0, 0, 1, 1),
                            new Vector2(0.5f, 0.5f)
                        );
                    }

                    // ����Tileλ��  
                    Vector3Int tilePosition = new Vector3Int(x, y, 0);
                    tilemap.SetTile(tilePosition, newTile);
                }
            }
        }

        Debug.Log("Tilemap��ϸ�������");
    }

    // �򵥵��������Ź�����  
    public static class TextureScale
    {
        public static void Bilinear(Texture2D tex, int newWidth, int newHeight)
        {
            Color[] orignPix = tex.GetPixels();
            Color[] newPix = new Color[newWidth * newHeight];
            float ratioX = ((float)tex.width) / newWidth;
            float ratioY = ((float)tex.height) / newHeight;

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    int origX = Mathf.FloorToInt(x * ratioX);
                    int origY = Mathf.FloorToInt(y * ratioY);
                    newPix[(y * newWidth) + x] = orignPix[(origY * tex.width) + origX];
                }
            }

            tex.Reinitialize(newWidth, newHeight); // Use Reinitialize instead of Resize
            tex.SetPixels(newPix);
            tex.Apply();
        }
    }
}
