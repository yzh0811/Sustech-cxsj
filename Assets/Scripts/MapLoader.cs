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
        // 直接使用相对路径  
        string imagePath = "D:\\创新实践\\AgentTest\\Assets\\Images\\generated_image.png";

        // 检查文件是否存在  
        if (!File.Exists(imagePath))
        {
            Debug.LogError("未找到图像文件: " + imagePath);
            return;
        }

        Debug.Log("加载图像: " + imagePath);

        // 加载纹理  
        byte[] fileData = File.ReadAllBytes(imagePath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(fileData);

        // 可选：如果图片太大，适当缩放  
        TextureScale.Bilinear(texture, 256, 256);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                Color pixelColor = texture.GetPixel(x, y);

                // 可以调整透明度阈值  
                if (pixelColor.a > 0.1f)
                {
                    // 创建新的Tile并设置颜色  
                    Tile newTile = ScriptableObject.CreateInstance<Tile>();
                    newTile.color = pixelColor;

                    // 如果有默认Tile，复制其精灵  
                    if (defaultTile != null)
                    {
                        newTile.sprite = defaultTile.sprite;
                    }
                    else
                    {
                        // 创建单像素纹理作为Tile  
                        Texture2D tileTexture = new Texture2D(1, 1);
                        tileTexture.SetPixel(0, 0, pixelColor);
                        tileTexture.Apply();

                        newTile.sprite = Sprite.Create(
                            tileTexture,
                            new Rect(0, 0, 1, 1),
                            new Vector2(0.5f, 0.5f)
                        );
                    }

                    // 设置Tile位置  
                    Vector3Int tilePosition = new Vector3Int(x, y, 0);
                    tilemap.SetTile(tilePosition, newTile);
                }
            }
        }

        Debug.Log("Tilemap精细生成完成");
    }

    // 简单的纹理缩放工具类  
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
