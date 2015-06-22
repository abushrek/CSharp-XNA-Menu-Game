﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Menu.Classes
{
    public class Track
    {
        private Random rnd;
        private Vector2 position;
        private Game game;
        private const int value = 4;
        private List<Vector2> trackList;

        public Track(Game game)
        {
            this.game = game;
            position = new Vector2(0,800);
            trackList = new List<Vector2>();
            rnd = new Random();
        }

        public void GeneratingTrack()
        {
            position.X+=1;
            double rand = rnd.NextDouble();
            if (rand <=0.5)
            {
                position.Y++;
            }
            else if (rand>=0.5)
            {
                position.Y--;
            }
            trackList.Add(new Vector2(position.X,position.Y));
        }

        public void DrawTrack()
        {
            foreach (Vector2 vec in trackList)
            {
                game.spriteBatch.DrawString(game.normalFont, "-", vec, Color.White);
            }
        }

        public List<Vector2> GetTrackList()
        {
            return trackList;
        }
    }
}