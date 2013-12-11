﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Engine;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;

namespace LadyJava
{
    abstract class Player
    {
        protected const float movement = 3.7f;

        protected Direction facingDirection;

        public abstract void Draw(SpriteBatch spriteBatch, Color transparency);

        public abstract Vector2 Update(GameTime gameTime,
                                       int newNPC, //npc index
                                       int finalNPC,  //final npc index
                                       int levelWidth, int levelHeight,
                                       Rectangle bossArea,
                                       BoundingBox bossAreaTrigger,
                                       BoundingBox[] entrances, 
                                       BoundingBox[] talkingRadii,
                                       params Object[] collisionObjects);

        protected Vector2 motion;

        protected Sprite sprite;
        protected string animation;
        protected bool switchedTileMap;

        protected bool jumpDone;

        protected bool inBossFight;
        public bool InBossFight
        { get { return inBossFight; } }

        protected BoundingBox boundingBox;

        protected bool speakingToFinalNPC;

        protected bool finishedTalkingToFinalNPC;
        public bool SpokeWithFinalNPC
        { get { return finishedTalkingToFinalNPC; } }

        protected Vector2 previousPosition;
        public Vector2 PreviousPosition
        { get { return previousPosition; } }

        protected int talkingTo;
        public int TalkingTo
        { get { return talkingTo; } }

        protected PlayState currentPlayState;
        public PlayState CurrentPlayState
        { get { return currentPlayState; } }
        
        public Vector2 Position
        { get { return sprite.Position; } }

        public Vector2 Origin
        { get { return sprite.Origin; } }

        public int Width
        { get { return sprite.Width; } }

        public int Height
        { get { return sprite.Height; } }

        public Vector2 Motion
        { get { return motion; } }

        //position needs to adjust based of width or height change in sprite class
        protected Vector2 cameraFocus;
        public Vector2 CameraFocus
        { get { return cameraFocus; } }

        protected Vector2 EntranceCollision(Vector2 newMotion, BoundingBox[] newEntrances)
        {
            boundingBox = getBounds(Position + newMotion, Width, Height);
            for (int i = 0; i < newEntrances.Length; i++) //For each tile
            {
                if (boundingBox.Intersects(newEntrances[i])) //compare Lady J's box with another square. 
                {
                    return new Vector2(newEntrances[i].Min.X, newEntrances[i].Min.Y);
                }
            }
            return Global.InvalidVector2;
        }

        protected Vector2 LockToLevel(Vector2 position, int levelW, int levelH)
        {
            if (position.X < 0)
                position.X = 0;
            if (position.Y < 0)
                position.Y = 0;
            if (position.X > levelW - Width)
                position.X = levelW - Width;
            if (position.Y > levelH - Height)
                position.Y = levelH - Height;
            return position;
        }

        protected Vector2 LockToFightArea(Vector2 position, Vector2 areaStart, float areaWidth, float areaHeight)
        {
            //also adjust by the origin of the player
            if (position.X < areaStart.X + Origin.X)
                position.X = areaStart.X + Origin.X;
            if (position.Y < areaStart.Y + Origin.Y)
                position.Y = areaStart.Y + Origin.Y;
            if (position.X > areaWidth - Origin.X)
                position.X = areaWidth - Origin.X;
            if (position.Y > areaHeight - Origin.Y)
                position.Y = areaHeight - Origin.Y;
            return position;
        }

        protected BoundingBox[] GetBoundingBoxes(Object[] objects)
        {
            Collection<BoundingBox> collisions = new Collection<BoundingBox>();

            for (int i = 0; i < objects.Length; i++)
                if (objects[i].GetType() == typeof(BoundingBox))
                    collisions.Add((BoundingBox)objects[i]);
                else if (objects[i].GetType().IsArray && objects[i].GetType().GetElementType() == typeof(BoundingBox))
                {
                    foreach (BoundingBox obj in (IEnumerable<BoundingBox>)objects[i])
                        collisions.Add(obj);
                }

            return collisions.ToArray<BoundingBox>();
        }

        public void SetPosition(Vector2 newPosition, int tileWidth, int tileHeight, bool centreToTile, bool switchingTileMap)
        {
            if (centreToTile)
            {
                Vector2 offsets = new Vector2(tileWidth / 2f - Width / 2f, tileHeight / 2f - Height / 2f);
                sprite.SetPosition(newPosition + offsets);
            }
            else
                sprite.SetPosition(newPosition);

            cameraFocus = sprite.Position;
            switchedTileMap = switchingTileMap;
        }
        
        protected BoundingBox NoCollision(BoundingBox bounds, BoundingBox[] collisions)
        {
            foreach (BoundingBox collision in collisions)
                if (collision.Intersects(bounds))
                    return collision;

            return Global.InvalidBoundingBox;
        }

        protected Vector2 AdjustForCollision(Vector2 position, Vector2 newMotion,
                                             int width, int height,
                                             BoundingBox[] collisions,
                                             bool checkRightCollision)
        {
            Vector2 newPosition = position;
            int incrementCount = (int)(newMotion.Length() * 2) + 1;
            Vector2 increment = newMotion / incrementCount;

            for (int i = 1; i <= incrementCount; i++)
            {
                Vector2 adjustedPosition = position + increment * i;
                BoundingBox newBounds = getBounds(adjustedPosition, width, height);

                BoundingBox collision = NoCollision(newBounds, collisions);
                if (collision == Global.InvalidBoundingBox)
                {
                    newPosition = adjustedPosition;
                }
                else
                {
                    bool isDiagonalMove = newMotion.X != 0 && newMotion.Y != 0;
                    if (isDiagonalMove)
                    {
                        int stepsLeft = incrementCount - (i - 1);

                        Vector2 newMotionX = increment.X * Vector2.UnitX * stepsLeft;
                        Vector2 newPositionX =
                            AdjustForCollision(newPosition, newMotionX, width, height, collisions, true);
                        newPosition += newPositionX;

                        Vector2 newMotionY = increment.Y * Vector2.UnitY * stepsLeft;
                        Vector2 newPositionY =
                            AdjustForCollision(newPosition, newMotionY, width, height, collisions, false);
                        newPosition += newPositionY;
                    }
                    if ((newPosition - position).Y == 0 && newMotion.Y < 0)
                        jumpDone = true;

                    break;
                }
            }
            
            return newPosition - position;
        }

        protected BoundingBox getBounds(Vector2 newPosition, int width, int height)
        {
            return new BoundingBox(new Vector3(newPosition, 0f),
                                   new Vector3(newPosition.X + width, newPosition.Y + height, 0f));
        }
    }
}