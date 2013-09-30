﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LadyJava
{
    class Animation
    {
        private int type;
        private int width;
        private int height;
        private int currentFrame;
        private int animateTime;
        private int nextFrame;
        //private int elapsedTimer;

        private float scale;

        private Frame[] frames;

        public int TotalFrames
        { get { return frames.Length; } }

        public float Scale
        { get { return scale; } }

        public int Width
        { get { return width; } }

        public int Height
        { get { return height; } }

        public Frame CurrentFrame
        { get { return frames[currentFrame]; } }

        public Animation(int frameWidth, int frameHeight, int totalFrames, int animationType, int aniamtionTime, int animation, float animationScale)
        {
            //add time variable so that the animation will adjust based on time elapsed

            type = animationType;
            width = frameWidth;
            height = frameHeight;
            nextFrame = aniamtionTime;
            animateTime = 0;
            
            scale = 1f;

            frames = new Frame[totalFrames];
            currentFrame = 0;

            for (int frame = 0; frame < TotalFrames; frame++)
                frames[frame] = new Frame(width, height, new Point(frame * width, animation * height), animationScale);
        }

        public void Update(GameTime animationTime)//, int animationType)
        {
            //based on time elapsed change current frame
            animateTime += animationTime.ElapsedGameTime.Milliseconds;
            if (nextFrame != 0 && animateTime >= nextFrame)
            {
                currentFrame++;
                animateTime = 0;
                if (currentFrame >= frames.Length)
                    currentFrame = 0;
            }

        }
    }
}
