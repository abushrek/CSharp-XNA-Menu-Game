﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Menu.Classes
{
    public class ControlItems
    {
        private string text;
        private Controls controls;
        private Game game;
        private List<Controls> items;
        private float height;
        public ControlItems(Game game)
        {
            this.game = game;
            text = "";
            height = 16;
            items = new List<Controls>();
        }
        public void AddItem(string text)
        {
            Vector2 posit = new Vector2(950, Game.height / 2 + items.Count * height);  //určení pozice přidané položky
            Controls controls = new Controls(text, posit);
            items.Add(controls);        //vložení do listu
        }

        public void DrawControls() //výpis controls cyklem foreach tzn... vypíše všechny položky controls
        {
            foreach (Controls controls in items)
            {
                Color color = Color.White;
                game.spriteBatch.DrawString(game.normalFont, controls.text, controls.position, color);
            }
        }
    }
}
