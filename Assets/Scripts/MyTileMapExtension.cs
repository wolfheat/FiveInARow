using UnityEngine;
using UnityEngine.Tilemaps;

namespace TileMapExtensions
{
    public static class MyTileMapExtension
    {
        public static Tilemap ShiftTileMap(this Tilemap tilemap, Vector2Int amount)
        {
            for (int i = tilemap.size.x - 1; i >= 0; i--)
            {
                for (int j = tilemap.size.y - 1; j >= 0; j--)
                {
                    tilemap.SetTile(new Vector3Int(i + amount.x, j + amount.y, 1), tilemap.GetTile(new Vector3Int(i, j, 1)));
                }
            }
            return tilemap;
        }
        public static Tilemap AddTileMapBlock(this Tilemap tilemap, Vector2Int startPos, Vector2Int amount, Tile vacant)
        {
            for (int i = 0; i < amount.x; i++)
            {
                for (int j = 0; j < amount.y; j++)
                {
                    tilemap.SetTile(new Vector3Int(startPos.x + i, startPos.y + j, 1), vacant);
                }
            }
            return tilemap;
        }

    }
}
