﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Menu
{
    public class AboutItems
    {
        private string text;
        public About about;
        private List<About> items;
        private Game game;
        private float height;
        public AboutItems(Game game)
        {
            text = "";
            height = 18;
            this.game = game;
            items = new List<About>();
        }
        //přidání itemu do about
        public void AddItem(string text)
        {
            Vector2 posit = new Vector2(950, Game.height / 2 + items.Count * height);  //určení pozice přidané položky
            About about = new About(text, posit);
            items.Add(about);        //vložení do listu
        }

        public void DrawAbout()  //výpis about cyklem foreach tzn... vypíše všechny položky about
        {
            foreach (About about in items)
            {
                Color color = Color.Red;
                game.spriteBatch.DrawString(game.normalFont, about.text, about.position,color);
            }
        }
    }
}
