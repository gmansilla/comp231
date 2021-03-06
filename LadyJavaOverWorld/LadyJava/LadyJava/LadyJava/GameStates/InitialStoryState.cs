﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using Engine;
using System.Collections.Generic;

namespace LadyJava
{
    class InitialStoryState : GameState
    {
        SpriteFont normalText;

        Dictionary<State, DisplayText> actionText;

        Color selectedColor;
        Color unSelectedColor;

        float scale;

        int width;
        int height;

        State selected;

        public InitialStoryState(ContentManager newContent, GraphicsDevice newGraphicsDevice)
        {
            id = State.InitialStory;
            selected = State.GamePlay;

            scale = 1f;

            Vector2 position;

            width = newGraphicsDevice.Viewport.Width;
            height = newGraphicsDevice.Viewport.Height;

            background = newContent.Load<Texture2D>("Screens\\InitialStory");
            bgSong = newContent.Load<Song>("Music\\Chandelier");

            normalText = newContent.Load<SpriteFont>("Fonts\\TitleText");

            selectedColor = Color.SteelBlue;
            unSelectedColor = Color.MintCream;
            actionText = new Dictionary<State, DisplayText>(); //new DisplayText[State.Quit + 1];

            position = new Vector2(width / 2f, height);

            string info = "Press the [SpaceBar] to Continue";
            actionText.Add(selected, new DisplayText(position, info, normalText, selectedColor));
            //actionText.Add(State.Options, new DisplayText(position, "Options", normalText, unSelectedColor));
            //actionText.Add(State.Quit, new DisplayText(position, "Quit", normalText, unSelectedColor));

            float menusHeight = 0f;
            foreach (KeyValuePair<State, DisplayText> text in actionText)
            {
                text.Value.MoveText(new Vector2(-text.Value.Width / 2f, -text.Value.Height));
                menusHeight -= text.Value.Height;
            }

        }
        
        public override State Update(GameTime gameTime, int newScreenWidth, int newScreenHeight)
        {
            width = newScreenWidth;
            height = newScreenHeight;

            if (InputManager.HasKeyBeenUp(Commands.Execute))
            {
                status = Status.Transition;
                return selected;
            }

            return State.InitialStory;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Matrix.CreateScale(new Vector3(scale, scale, scale)));

            spriteBatch.Draw(background, Vector2.Zero, new Rectangle(0, 0, width, height), backgroundColor);

            if (status != Status.Transition)
                foreach (KeyValuePair<State, DisplayText> text in actionText)
                    text.Value.DrawString(spriteBatch);

            spriteBatch.End();
        }
    
    }
}
