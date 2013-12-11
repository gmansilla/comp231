﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Engine;
using System.Collections.ObjectModel;

namespace LadyJava
{
    class OverWorldPlayer : Player
    {
        //private float movement = 3.4f;

        public OverWorldPlayer(Sprite newSprite, int tileWidth, int tileHeight)
        {
            animation = Global.Still;
            sprite = newSprite;
            //centre lady java on tile
            SetPosition(Position, tileWidth, tileHeight, true, false);
            cameraFocus = sprite.Position;
            
            switchedTileMap = false;
            
            talkingTo = Global.InvalidInt;
            speakingToFinalNPC = false;
            finishedTalkingToFinalNPC = false;
            

            boundingBox = getBounds(Position, Width, Height);
        }
        
        public override Vector2 Update(GameTime gameTime, 
                                       int newNPC, //npc index
                                       int finalNPC, //final npc index
                                       int levelWidth, int levelHeight,
                                       Rectangle bossArea,
                                       BoundingBox bossAreaTrigger,
                                       BoundingBox[] entrances, BoundingBox[] talkingRadii,
                                       params Object[] collisionObjects)
        {
            Vector2 entranceLocation = Global.InvalidVector2;
            motion = Vector2.Zero;
            Vector2 position = sprite.Position;
            previousPosition = sprite.Position;

            talkingTo = newNPC;

            bool collision = false;
            BoundingBox[] collisions = GetBoundingBoxes(collisionObjects);

            animation = Global.Still;
            facingDirection = Direction.Right;
            if (currentPlayState == PlayState.Playing)
            {
                if ((!switchedTileMap && InputManager.IsKeyDown(Commands.Up)) ||
                    (switchedTileMap && InputManager.HasKeyBeenUp(Commands.Up)))
                {
                    animation = Global.Up;
                    motion.Y = -movement;
                    //motion = UpCollision(motion, collisions);
                    if (motion.Y != -movement)
                        collision = true;

                    if (switchedTileMap)
                        switchedTileMap = false;
                }
                if (!switchedTileMap && InputManager.IsKeyDown(Commands.Down) ||
                    (switchedTileMap && InputManager.HasKeyBeenUp(Commands.Down)))
                {
                    animation = Global.Down;
                    motion.Y = movement;
                    //motion = DownCollision(motion, collisions);
                    if (motion.Y != movement)
                        collision = true;

                    if (switchedTileMap)
                        switchedTileMap = false;
                }
                if (!switchedTileMap && InputManager.IsKeyDown(Commands.Right) ||
                    (switchedTileMap && InputManager.HasKeyBeenUp(Commands.Right)))
                {
                    animation = Global.Moving;
                    facingDirection = Direction.Right;

                    motion.X = movement;
                    //motion = RightCollision(motion, collisions);
                    if (motion.X != movement)
                        collision = true;

                    if (switchedTileMap)
                        switchedTileMap = false;
                }
                if (!switchedTileMap && InputManager.IsKeyDown(Commands.Left) ||
                    (switchedTileMap && InputManager.HasKeyBeenUp(Commands.Left)))
                {
                    animation = Global.Moving;
                    facingDirection = Direction.Left;
                    motion.X = -movement;
                    //motion = LeftCollision(motion, collisions);
                    if (motion.X != -movement)
                        collision = true;

                    if (switchedTileMap)
                        switchedTileMap = false;
                }
            }            

            for (int i = 0; i < talkingRadii.Length; i++)
            {
                if (InputManager.HasKeyBeenUp(Commands.Execute) &&
                   boundingBox.Intersects(talkingRadii[i]))
                {
                    //talking = !talking;
                    //if (talking)
                    if(talkingTo == Global.InvalidInt)
                    {
                        currentPlayState = PlayState.Message;
                        talkingTo = i;
                        if (talkingTo == finalNPC)
                            speakingToFinalNPC = true;
                        break;
                    }
                    else
                    {
                        if (speakingToFinalNPC)
                            finishedTalkingToFinalNPC = true;
                        currentPlayState = PlayState.Playing;
                    }
                }
            }

            if (!collision && motion != Vector2.Zero)
            {
                motion.Normalize();
                motion *= movement;
            }

            motion = AdjustForCollision(position, motion, Width, Height, collisions, false);

            position += motion;
            cameraFocus += motion;
            position = LockToLevel(position, levelWidth, levelHeight);
            entranceLocation = EntranceCollision(motion, entrances);
            sprite.Update(gameTime, animation, position, facingDirection);

            return entranceLocation;
        }

        public override void Draw(SpriteBatch spriteBatch, Color transparency)
        {
            sprite.Draw(spriteBatch, transparency);
        }
    }
}