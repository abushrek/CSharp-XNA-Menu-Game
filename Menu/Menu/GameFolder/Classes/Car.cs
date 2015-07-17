﻿using System;
using Menu.GameFolder.Components;
using Menu.GameFolder.Interface;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Menu.GameFolder.Classes
{
    public enum ECar
    {
        Forward,
        Backward,
        InertiaForward,
        InertiaBackward,
        Stop
    }

    public class Car
    {
        public Vector2 position;
        private Game game;
        private float angle;
        private ECar ECar;
        private IPhysics physics;

        public Car(Game game)
        {
            this.game = game;
            angle = 0.0f;
            position = new Vector2(Game.width / 2, Game.height / 2);
            ECar = ECar.Stop;
            physics = new CarPhysics();
        }

        public void Move(GameTime gameTime)
        {
            #region Doleva

            if (game.keyState.IsKeyDown(Keys.Left))
            {
                if (ECar == ECar.Forward || ECar == ECar.InertiaForward)
                    //Podmínka, aby auto zatáčelo jen když jede vpřed
                    angle -= 0.02f;
                else if (ECar == ECar.Backward || ECar == ECar.InertiaBackward)
                    //Podmínka, aby auto zatáčelo jen když jede vzad
                    angle += 0.02f;
            }
            #endregion

            #region Doprava

            if (game.keyState.IsKeyDown(Keys.Right))
            {
                if (ECar == ECar.Forward || ECar == ECar.InertiaForward)
                    //Podmínka, aby auto zatáčelo jen když jede vpřed
                    angle += 0.02f;
                else if (ECar == ECar.Backward || ECar == ECar.InertiaBackward)
                    //Podmínka, aby auto zatáčelo jen když jede vzad
                    angle -= 0.02f;
            }
            #endregion

            #region Dopředu

            if (game.keyState.IsKeyDown(Keys.Up) && ECar != ECar.InertiaBackward) //Pokud jede vpřed
            {
                if (game.keyState.IsKeyUp(Keys.Down) && ECar != ECar.Backward) //pokud neni záčknuto dolu a auto nejede dozadu
                {
                    ECar = ECar.Forward;
                    CurrentPosition();
                }
            }

            #endregion

            #region Dozadu

            if (game.keyState.IsKeyDown(Keys.Down) && ECar != ECar.InertiaForward) //Pokud jede vzad
            {
                if (game.keyState.IsKeyUp(Keys.Up) && ECar != ECar.Forward) //pokud neni záčknuto nahoru a auto nejede dopředu
                {
                    ECar = ECar.Backward;
                    CurrentPosition();
                }
            }
            #endregion

            Inertia(gameTime); //Setrvačnost
            Braking(gameTime); //Brždění
            physics.Speed(gameTime, ECar); //Rychlost
            if (CurrentSpeed()==0) //Pokud je rychlost menší než nula nebo nula tak se do enumerátoru hodí stop
                ECar = ECar.Stop;
        }

        private float Rotation() //Zatáčení auta
        {
            float rotationAngle = 0;
            rotationAngle += angle;
            const float circle = MathHelper.Pi * 2;
            rotationAngle = rotationAngle % circle;
            return rotationAngle;
        }

        private void Braking(GameTime gameTime)
        {
            if (ECar == ECar.InertiaForward && game.keyState.IsKeyDown(Keys.Down))
            //Pokud je auto v setrvacnosti dopred a sipka dolu je stlacena tak se brzdi
            {
                physics.Brake(gameTime);
                CurrentPosition();
            }
            else if (ECar == ECar.InertiaBackward && game.keyState.IsKeyDown(Keys.Up))
            //Pokud je auto v setrvacnosti vzad a sipka nahoru je stracena tak se brzdi
            {
                physics.Brake(gameTime);
                CurrentPosition();
            }
        }

        private void Inertia(GameTime gameTime)
        {
            if ((game.keyState.IsKeyUp(Keys.Up) && game.keyState.IsKeyUp(Keys.Down) && physics.Velocity > 0))
            //Pokud se nedrží tlačítko vpřed nebo vzad a auto je rozjeté
            {
                switch (ECar)
                {
                    case ECar.Forward:
                        ECar = ECar.InertiaForward;
                        break;
                    case ECar.Backward:
                        ECar = ECar.InertiaBackward;
                        break;
                }
                physics.Inertia(gameTime);
                CurrentPosition();
            }
            else if (game.keyState.IsKeyDown(Keys.Down) && game.keyState.IsKeyDown(Keys.Up))
            //Pokud jsou obě tlačítka zmáčknuty tak se brzdí
            {
                physics.Brake(gameTime);
                CurrentPosition();
            }
        }

        private double Distance() //zjištění ujeté vzdálenosti pro výpočet pozice
        {
            double distance = 0;
            if (ECar == ECar.Forward || ECar == ECar.InertiaForward) //pokud jede dopřed nebo setrvačností dopřed
            {
                distance += physics.Velocity;
            }
            else if (ECar == ECar.Backward || ECar == ECar.InertiaBackward) //pokud jede vzad nebo setrvačností vzda
            {
                distance -= physics.Velocity;
            }
            return distance;
        }

        private void CurrentPosition() //Výpočet pozice
        {
            Distance();
            position.X = (float)(Math.Cos(angle) * Distance() + position.X);
            //X = Cos(uhlu) *ujeta vzdalenost + predchozi pozice
            position.Y = (float)(Math.Sin(angle) * Distance() + position.Y);
            //Y = Sin(uhlu) *ujeta vzdalenost + predchozi pozice
        }

        public int CurrentSpeed()
        {
            double velocity = physics.Velocity * 10;
            int speed = (int)velocity;
            return speed;
        }
        public Enum GetCarState()
        {
            return ECar;
        }
        public void DrawCar()
        {
            game.spriteBatch.Draw(game.spritCar, new Rectangle((int)position.X, (int)position.Y, game.spritCar.Width, game.spritCar.Height), null, Color.White, Rotation(), new Vector2(game.spritCar.Width / 2, game.spritCar.Height / 2), SpriteEffects.None, 0f);
        }
    }
}
