﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Engine
{
    public class TileLayer
    {
        static public int TransparentCell = -1;

        int[,] tileLayer;
        
        int tileWidth;
        int tileHeight;

        public int[,] Layer
        { get { return tileLayer; } }
 
        public int Width
        { get { return tileLayer.GetLength(1); } }
        public int Height
        { get { return tileLayer.GetLength(0); } }

        public int TileWidth
        { get { return tileWidth; } }
        public int TileHeight
        { get { return tileHeight; } }

        public TileLayer(int[,] newTileLayer, int newTileWidth, int newTileHeight)
        {
            tileLayer = newTileLayer;
            tileWidth = newTileWidth;
            tileHeight = newTileHeight;
        }

        public TileLayer(int Width, int Height, int newTileWidth, int newTileHeight)
        {
            tileLayer = new int[Height, Width];

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    SetCellIndex(x, y, -1);

            tileWidth = newTileWidth;
            tileHeight = newTileHeight;
        }

        public int GetCellIndex(int x, int y)
        {
            return tileLayer[y, x];
        }

        public void SetCellIndex(int x, int y, int cellIndex)
        {
            tileLayer[y, x] = cellIndex;
        }

        public void Draw(SpriteBatch spriteBatch, List<Tile> tiles, Color transparency)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int textureIndex = tileLayer[y, x];
                    if (textureIndex != -1)
                    {
                        spriteBatch.Draw(tiles[textureIndex].Image, new Rectangle(
                        x * tileWidth,
                        y * tileHeight,
                        tiles[textureIndex].Width, tiles[textureIndex].Height), transparency);
                    }
                }
            }
        }
    }
}
