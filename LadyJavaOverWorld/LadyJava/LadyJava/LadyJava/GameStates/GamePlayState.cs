﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Engine;
using Microsoft.Xna.Framework.Input;

namespace LadyJava
{
    class GamePlayState : GameState
    {
        Camera camera;
        Dictionary<AreaType, Player> player;
        int[] interactingWith;

        Dictionary<string, RescueInfo> toBeRescued;

        Texture2D collisionLayerImage;
        
        Dictionary<string, TileMap> campus;
        string currentArea;
        string previousArea;

        bool drawCollision = false;

        public GamePlayState(ContentManager newContent, GraphicsDevice newGraphicsDevice)
        {
            exitStatus = Status.Transition;

            int screenWidth = newGraphicsDevice.Viewport.Width;
            int screenHeight = newGraphicsDevice.Viewport.Height;

            SpriteFont speechText = newContent.Load<SpriteFont>("Fonts\\SpeechFont");

            id = State.GamePlay;

            bgSong = bgSong = newContent.Load<Song>("Music\\Chandelier");

            currentArea = Global.MainArea;
            campus = new Dictionary<string, TileMap>();

            campus.Add("TileMaps\\overworld.map", new TileMap(Global.ContentPath, "TileMaps\\overworld.map", newContent, screenWidth, screenHeight, speechText));
            campus.Add("TileMaps\\house1.map", new TileMap(Global.ContentPath, "TileMaps\\house1.map", newContent, screenWidth, screenHeight, speechText));
            campus.Add("TileMaps\\house2.map", new TileMap(Global.ContentPath, "TileMaps\\house2.map", newContent, screenWidth, screenHeight, speechText));
            campus.Add("TileMaps\\house3.map", new TileMap(Global.ContentPath, "TileMaps\\house3.map", newContent, screenWidth, screenHeight, speechText));
            campus.Add("TileMaps\\house4.map", new TileMap(Global.ContentPath, "TileMaps\\house4.map", newContent, screenWidth, screenHeight, speechText));
            campus.Add("TileMaps\\PC.map", new TileMap(Global.ContentPath, "TileMaps\\PC.map", newContent, screenWidth, screenHeight, speechText));
            campus.Add("TileMaps\\ShirsStudy.map", new TileMap(Global.ContentPath, "TileMaps\\ShirsStudy.map", newContent, screenWidth, screenHeight, speechText));
            campus.Add("TileMaps\\Gym.map", new TileMap(Global.ContentPath, "TileMaps\\Gym.map", newContent, screenWidth, screenHeight, speechText));
            campus.Add("TileMaps\\BoatHouse.map", new TileMap(Global.ContentPath, "TileMaps\\BoatHouse.map", newContent, screenWidth, screenHeight, speechText));
            campus.Add("TileMaps\\D1Water.map", new TileMap(Global.ContentPath, "TileMaps\\D1Water.map", newContent, screenWidth, screenHeight, speechText));
            campus.Add("TileMaps\\D2Basement.map", new TileMap(Global.ContentPath, "TileMaps\\D2Basement.map", newContent, screenWidth, screenHeight, speechText));
            campus.Add("TileMaps\\D3Tree.map", new TileMap(Global.ContentPath, "TileMaps\\D3Tree.map", newContent, screenWidth, screenHeight, speechText));
            campus.Add("TileMaps\\D4Final.map", new TileMap(Global.ContentPath, "TileMaps\\D4Final.map", newContent, screenWidth, screenHeight, speechText));
            campus.Add("TileMaps\\D1End.map", new TileMap(Global.ContentPath, "TileMaps\\D1End.map", newContent, screenWidth, screenHeight, speechText));
            campus.Add("TileMaps\\D2End.map", new TileMap(Global.ContentPath, "TileMaps\\D2End.map", newContent, screenWidth, screenHeight, speechText));
            campus.Add("TileMaps\\D3End.map", new TileMap(Global.ContentPath, "TileMaps\\D3End.map", newContent, screenWidth, screenHeight, speechText));
            campus.Add("TileMaps\\D4End.map", new TileMap(Global.ContentPath, "TileMaps\\D4End.map", newContent, screenWidth, screenHeight, speechText));

            AnimationInfo[] overworldAnimations = { new AnimationInfo(Global.Still, 32, 46, 1, 0, Animation.None),
                                                    new AnimationInfo(Global.Down, 32, 46, 4, 100, Animation.None),
                                                    new AnimationInfo(Global.Moving, 32, 46, 4, 100, Animation.None),
                                                    new AnimationInfo(Global.Up, 32, 46, 4, 100, Animation.None) };

            player = new Dictionary<AreaType, Player>();
            Texture2D overworldImage = newContent.Load<Texture2D>("Sprites\\LadyJavaBigOverWorld");
            player.Add(AreaType.OverWorld,
                       new OverWorldPlayer(new Sprite(overworldImage, campus[currentArea].StartingPosition, overworldAnimations, 1.0f), 
                                           campus[currentArea].TileWidth, campus[currentArea].TileHeight));

            camera = new Camera(screenWidth, screenHeight,
                                player[campus[currentArea].CurrentAreaType].Position,
                                player[campus[currentArea].CurrentAreaType].Origin,
                                campus[currentArea].PixelWidth, campus[currentArea].PixelHeight);
            
            Texture2D dungeonImage = newContent.Load<Texture2D>("Sprites\\LadyJavaDungeon");
            AnimationInfo[] dungeonAnimations = { new AnimationInfo(Global.Still, 30, 48, 1, 0, Animation.None),
                                                  new AnimationInfo(Global.Moving, 30, 48, 8, 100, Animation.None),
                                                  new AnimationInfo(Global.StartingAttack, 30, 48, 3, 50, Global.Attacking),
                                                  new AnimationInfo(Global.Attacking, 30, 48, 1, 500, Animation.None, false),
                                                  new AnimationInfo(Global.Dying, 32, 48, 9, 100, Animation.None, false) };
            player.Add(AreaType.Dungeon,
                       new DungeonPlayer(new Sprite(dungeonImage, Vector2.Zero, dungeonAnimations, 1f),
                                         newContent.Load<Texture2D>("Sprites\\ladyJavaAttack"),
                                         newContent.Load<Texture2D>("Sprites\\hpMarker"),
                                         screenWidth, screenHeight));

            collisionLayerImage = newContent.Load<Texture2D>("tileSelector");

            toBeRescued = new Dictionary<string, RescueInfo>();
            RescueInfo[] rescueInfo = new RescueInfo[Global.ToBeRecused.Length];
            for(int i = 0; i < Global.ToBeRecused.Length; i++)
                toBeRescued.Add(Global.ToBeRecused[i], new RescueInfo(Global.RecuseAreas[i]));

            interactingWith = new int[0];
        }

        public override State Update(GameTime gameTime, int screenWidth, int screenHeight)
        {
            //return to titlescreen
            if (InputManager.IsKeyDown(Commands.Exit))
            {
                ChangeStatus(Status.Paused);
                return State.TitleScreen;
            }
            else if (InputManager.HasKeyBeenUp(new Command(Microsoft.Xna.Framework.Input.Keys.C, Buttons.LeftShoulder)))
                drawCollision = !drawCollision;
            else if (InputManager.HasKeyBeenUp(new Command(Microsoft.Xna.Framework.Input.Keys.R, Buttons.RightShoulder)))
            {
                foreach (KeyValuePair<string, RescueInfo> npc in toBeRescued)
                    if(npc.Key != Global.ToBeRecused[Global.TheScrumMaster])
                        npc.Value.Rescue();
            }

            campus[currentArea].UpdateRescueList(toBeRescued);

            Vector2 entrancePixelLocation = player[campus[currentArea].CurrentAreaType]
                                                  .Update(gameTime,
                                                          camera,
                                                          interactingWith,
                                                          campus[currentArea].FinalNPCIndex,
                                                          campus[currentArea].PixelWidth,
                                                          campus[currentArea].PixelHeight,
                                                          campus[currentArea].PlayerHit,
                                                          campus[currentArea].BossArea,
                                                          campus[currentArea].BossIsAlive, 
                                                          campus[currentArea].BossAreaTrigger,
                                                          campus[currentArea].ToEntranceBox,
                                                          campus[currentArea].NPCTalkRadii,
                                                          campus[currentArea].EnemiesToBoundingBox,
                                                          campus[currentArea].
                                                            GetSurroundingBoundingBoxes(
                                                                player[campus[currentArea].CurrentAreaType].Position),
                                                          campus[currentArea].NPCsToBoundingBox);

            if (entrancePixelLocation != Global.InvalidVector2)
            {
                string newArea = campus[currentArea].Update(gameTime, 
                                                            entrancePixelLocation,
                                                            player[campus[currentArea].CurrentAreaType].PreviousPosition);

                if (newArea == Global.EndOfTheGame)
                {
                    status = exitStatus; 
                    return State.FinalStory;
                }
                //clear out the last position if the user doesn't reenter the same previous location
                if (previousArea != Global.MainArea && previousArea != null && previousArea != newArea)
                    campus[previousArea].SetLastPosition(Global.InvalidVector2);

                previousArea = currentArea;
                currentArea = newArea;

                //make Main Npc's Visible on overworld.map and not in the dungeon anymore
                foreach (KeyValuePair<string, RescueInfo> npc in toBeRescued)
                    if (previousArea == npc.Value.RescueArea)
                        npc.Value.Rescue();
                
                bool tileMapSwitch = true;
                if (campus[currentArea].LastPosition == Global.InvalidVector2)
                    player[campus[currentArea].CurrentAreaType].SetPosition(campus[currentArea].StartingPosition,
                                                campus[currentArea].TileWidth,
                                                campus[currentArea].TileHeight, true, tileMapSwitch);
                else
                {
                    player[campus[currentArea].CurrentAreaType].SetPosition(campus[currentArea].LastPosition,
                                                campus[currentArea].TileWidth,
                                                campus[currentArea].TileHeight, false, tileMapSwitch);
                    campus[currentArea].SetLastPosition(Global.InvalidVector2);
                }
            }

            if(!player[campus[currentArea].CurrentAreaType].InBossFight)
                camera.Update(gameTime, 
                              player[campus[currentArea].CurrentAreaType].InBossFight,
                              player[campus[currentArea].CurrentAreaType].Position,
                              player[campus[currentArea].CurrentAreaType].Origin, 
                              campus[currentArea].PixelWidth, campus[currentArea].PixelHeight);
            else
                camera.Update(gameTime, 
                              player[campus[currentArea].CurrentAreaType].InBossFight,
                              new Vector2(campus[currentArea].BossArea.X, campus[currentArea].BossArea.Y),
                              player[campus[currentArea].CurrentAreaType].Origin,
                              campus[currentArea].PixelWidth, campus[currentArea].PixelHeight);


            interactingWith = campus[currentArea].NPCUpdate(gameTime, toBeRescued, camera,
                                                            player[campus[currentArea].CurrentAreaType].Position,
                                                            player[campus[currentArea].CurrentAreaType].InBossFight,
                                                            player[campus[currentArea].CurrentAreaType].ToBoundingBox,
                                                            player[campus[currentArea].CurrentAreaType].CurrentPlayState,
                                                            player[campus[currentArea].CurrentAreaType].InteractingWith,
                                                            player[campus[currentArea].CurrentAreaType].SpokeWithFinalNPC,
                                                            screenWidth, screenHeight);

            if (campus[currentArea].PlayerHit)
                player[campus[currentArea].CurrentAreaType].WasHit();
            if (player[campus[currentArea].CurrentAreaType].PlayerNeedsReset)
            {    
                player[campus[currentArea].CurrentAreaType].ResetPlayer(campus[currentArea].StartingPosition,
                                                                        campus[currentArea].TileWidth,
                                                                        campus[currentArea].TileHeight);
                camera.ResetCamera();
                campus[currentArea].ResetBoss();
            }


            return State.GamePlay;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.TransformMatrix);
            campus[currentArea].Draw(spriteBatch, backgroundColor);

            player[campus[currentArea].CurrentAreaType].Draw(spriteBatch, backgroundColor);

            if(drawCollision)
                campus[currentArea].CollisionLayer.Draw(spriteBatch, collisionLayerImage);

            spriteBatch.End();
        }
    }
}
