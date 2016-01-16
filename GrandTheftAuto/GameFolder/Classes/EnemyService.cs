﻿using System;
using System.Collections.Generic;
using System.Linq;
using GrandTheftAuto.GameFolder.Classes.GunFolder;
using GrandTheftAuto.MenuFolder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GrandTheftAuto.GameFolder.Classes
{
    public class EnemyService
    {
        private enum EHit
        {
            EnemyHit,
            BulletHit,
            None
        }
        public List<Enemy> EnemyList { get; set; }
        public List<DiedEnemy> DiedList { get; set; }
        public Enemy EnemyTarget { get; private set; }
        public int Score { get; set; }

        private GameClass game;
        public EnemyAi enemyAi;
        private DropOption dropOption;
        private double attackTimer;
        private double hitTimer;
        private Vector2 hitPosition;
        private Random rnd;
        private int enemyHitDamage;
        private int bulletHitDamage;
        private int cryticalHitDamage;
        private EHit eHit;

        public delegate void EnemyDie(Enemy enemy, Character character);

        public event EnemyDie EventEnemyDie;
        public EnemyService(GameClass game, List<Rectangle> obstactleList)
        {
            this.game = game;
            EventEnemyDie += EnemyDead;
            EventEnemyDie += Experiences;
            EnemyList = new List<Enemy>();
            DiedList = new List<DiedEnemy>();
            enemyAi = new EnemyAi(obstactleList);
            dropOption = new DropOption(game);
            Score = 0;
            hitPosition = Vector2.Zero;
            enemyHitDamage = 0;
            cryticalHitDamage = 0;
            bulletHitDamage = 0;
            rnd = new Random();
            eHit = EHit.None;
        }

        private void AddEnemy(Vector2 position, Texture2D texture2d)
        {
            Random rnd = new Random();
            string name = "";
            int hp = 0;
            float speed = 0;
            int damage = 0;
            float angle = 0;
            int score = 0;
            int exp = 0;
            double chanceToMiss = 0;
            int rand = rnd.Next(10, 100);
            Texture2D texture = texture2d;
            EnemyOption enemyOption = new EnemyOption(game);
            enemyOption.GetEnemy(ref name, rand, ref hp, ref damage, ref speed, ref texture, ref score, ref chanceToMiss, ref exp);
            EnemyList.Add(new Enemy(name, hp, damage, speed, texture, position, angle, score, chanceToMiss, exp));
        }

        public void DrawEnemyHp()
        {
            foreach (Enemy enemy in EnemyList)
            {
                //Grafické vykreslení
                game.spriteBatch.Draw(game.spritEnemyHealthBar, new Rectangle((int)enemy.Position.X - game.spritEnemy[0].Width / 2, (int)enemy.Position.Y, (int)((enemy.Hp / enemy.MaxHp) * game.spritEnemy[0].Width), 5), Color.White);
                //Vykreslený hodnoty
                game.spriteBatch.DrawString(game.smallFont, enemy.Hp.ToString(), new Vector2(enemy.Position.X - game.smallFont.MeasureString(enemy.Hp.ToString()).X / 2, enemy.Position.Y - game.smallFont.MeasureString(enemy.Hp.ToString()).Y), Color.Red);
            }
        }

        public void Attack(ref double hp, Rectangle attackedRectangle, GameTime gameTime, Camera camera)
        {
            if (hp > 0)
                foreach (Enemy enemy in EnemyList.Where(enemy => attackedRectangle.Intersects(enemy.Rectangle)))
                {
                    attackTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (1000 < attackTimer)     //attack timer pro řízení úderů od enemy
                    {
                        HitMethod(camera, enemy.Position, game.spritCharacter[0], false);
                        attackTimer = 0;
                        if (rnd.Next(0, 100) > enemy.ChanceToMiss)  //podmínka pokud je random číslo větší než chance to miss tak zaútočí jinak je úder vedle
                        {
                            enemyHitDamage = enemy.Damage + rnd.Next(-5, 5);
                            hp -= enemyHitDamage;
                        }
                        else enemyHitDamage = 0;
                    }

                }
            else
            {
                hp = 0;
                game.EGameState = EGameState.GameOver;
            }
        }
        /*
        public void CarKill(Rectangle car,int speed)
        {           
            foreach (Enemy enemy in EnemyList)
            {
                if (enemy.Rectangle.Contains(car.Location))
                {
                    enemy.Hp = 0;
                    enemy.Alive = false;
                }
            }
        }
        */
        public void GetDamage(List<Bullet> bulletList, Camera camera, Character character)
        {
            if (bulletList.Count != 0 && EnemyList.Count != 0)
            {
                for (int i = 0; i <= EnemyList.Count - 1; i++)
                {
                    for (int j = 0; j <= bulletList.Count - 1; j++)
                    {
                        if (EnemyList[i].Rectangle.Contains(bulletList[j].Position.X, bulletList[j].Position.Y))
                        {
                            EnemyList[i].IsAngry = true;
                            bulletHitDamage = bulletList[j].Damage;
                            EnemyList[i].Hp -= bulletHitDamage;
                            cryticalHitDamage = bulletList[j].MaxDamage;
                            HitMethod(camera, EnemyList[i].Position, EnemyList[i].Texture);
                            EnemyTarget = EnemyList[i];
                            bulletList.Remove(bulletList[j]);
                        }
                    }
                    if (EnemyList[i].Hp <= 0)
                    {
                        dropOption.SelectListOfItems(EnemyList[i], character);
                        OnEventEnemyDie(EnemyList[i], character);
                    }
                }
            }
        }

        public void HitMethod(Camera camera, Vector2 position, Texture2D texture, bool bulletHit = true)
        {
            eHit = bulletHit ? EHit.BulletHit : EHit.EnemyHit;
            Random rnd = new Random();
            hitPosition = new Vector2(rnd.Next((int)(position.X - texture.Width / 2), (int)(position.X + texture.Width / 2)) - camera.Centering.X
                , rnd.Next((int)(position.Y - texture.Height / 2), (int)(position.Y + texture.Height / 2)) - camera.Centering.Y);
        }

        public void PathFinding(Vector2 characterPosition, Rectangle characterRectangle = default(Rectangle), Rectangle carRectangle = default(Rectangle))
        {
            foreach (Enemy enemy in EnemyList)
            {
                enemy.UpdateEachRectangle();
                enemy.Position = enemyAi.PathFinding(enemy.Position, characterPosition, enemy, characterRectangle, carRectangle);
            }
        }

        public void GeneratingEnemies(GameTime gameTime, Camera camera, Rectangle characterRectangle)
        {
            if (EnemyList.Count < 200)
            {
                Texture2D texture = game.spritCharacter[0];
                Random rand = new Random();
                AddEnemy(enemyAi.GeneratePosition(rand, characterRectangle, texture), texture);
            }
        }

        public void DrawEnemy(Camera camera, GameTime gameTime)
        {
            //mrtvý
            foreach (DiedEnemy diedEnemy in DiedList)
            {
                game.spriteBatch.Draw(diedEnemy.Texture, new Rectangle((int)(diedEnemy.Position.X - camera.Centering.X - diedEnemy.Texture.Width / 2), (int)(diedEnemy.Position.Y - camera.Centering.Y - diedEnemy.Texture.Height / 2), diedEnemy.Texture.Width, diedEnemy.Texture.Height), Color.White);
            }
            //živý
            foreach (Enemy enemy in EnemyList)
            {
                game.spriteBatch.Draw(enemy.Texture, new Rectangle((int)(enemy.Position.X - camera.Centering.X), (int)(enemy.Position.Y - camera.Centering.Y), enemy.Texture.Width, enemy.Texture.Height), null, Color.White, enemy.Angle, new Vector2(enemy.Texture.Width / 2, enemy.Texture.Height / 2), SpriteEffects.None, 0f);
            }
            DrawDamageDone(gameTime);
        }
        private void DrawDamageDone(GameTime gameTime)
        {
            if (eHit != EHit.None && EnemyList.Count != 0)
            {
                string enemyDamage = enemyHitDamage != 0 ? enemyHitDamage.ToString() : "MISS";
                string bulletDamage = bulletHitDamage < cryticalHitDamage ? bulletHitDamage.ToString() : bulletHitDamage + " CriticalHit!!";
                Color color = eHit == EHit.EnemyHit ? Color.Red : Color.White;
                if (eHit == EHit.BulletHit)
                    game.spriteBatch.DrawString(game.smallFont, bulletDamage, hitPosition, color);
                if (eHit == EHit.EnemyHit)
                    game.spriteBatch.DrawString(game.smallFont, enemyDamage, hitPosition, color);
                hitTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (hitTimer > 500)
                {
                    eHit = EHit.None;
                    hitTimer = 0;
                }

            }
        }

        private void Experiences(Enemy enemy, Character character)
        {
            character.ActualExperiences += enemy.Exp;
        }

        private void EnemyDead(Enemy enemy, Character character)
        {
            character.EnemyKilled = enemy;
            enemy.Alive = false;
            Score += enemy.Score;
            DiedList.Add(new DiedEnemy(enemy.Position, game.spritBlood, enemy.Angle));
            if (DiedList.Count > 50)   //pokud je zabitých více jak 50 zombie tak se postupně maže první ze zabitých
                DiedList.RemoveAt(0);
            EnemyList.Remove(enemy);
        }

        protected virtual void OnEventEnemyDie(Enemy enemy, Character character)
        {
            if (EventEnemyDie != null) EventEnemyDie(enemy, character);
        }
    }
}