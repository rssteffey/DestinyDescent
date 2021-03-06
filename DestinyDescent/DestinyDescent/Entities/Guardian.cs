﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DestinyDescent.Entities
{
    class Guardian : Entity
    {
        #region Global Declarataions
        string playerClass;
        private int speedVar;
        private int gravityVar;
        private int boostTimer;
        private int boostCooldown;
        private float velocity;
        private int index;
        private int spriteChange;
        private int xMin;
        private int xMax;

        bool facingRight;
        bool boosting;
        bool boostReady;
        bool falling;

        Texture2D spriteSheet;

        List<Rectangle> guardianLeft;   // Bounds for sprite sheet
        List<Rectangle> guardianRight;  // "                     "

        KeyboardState oldState;
        List<Keys> keysPressed;
        #endregion

        #region BoundingBox
        public override Rectangle BoundingBox
        {
            get
            {
                return new Rectangle((int)position.X, (int)position.Y, guardianRight[0].Width, guardianRight[0].Height);
            }
        }
        #endregion

        #region Constructor
        public Guardian(Game g, string pClass, int sp, int gr, int width, int height, Texture2D sheet) : base(g, width, height)
        {
            playerClass = pClass;
            speedVar = sp;
            gravityVar = gr;
            boostTimer = 0;
            boostCooldown = 0;
            velocity = 0;
            index = 5;
            spriteChange = 0;
            facingRight = true;
            boosting = false;
            boostReady = true;
            falling = true;
            guardianLeft = new List<Rectangle>();
            guardianRight = new List<Rectangle>();
            spriteSheet = sheet;
            keysPressed = new List<Keys>();
            oldState = Keyboard.GetState();
        }
        #endregion

        #region Add Bounds
        #region Add Left Sprite
        public void addLeftSprite(Rectangle bounds)
        {
            guardianLeft.Add(bounds);
        }
        #endregion

        #region Add Right Sprite
        public void addRightSprite(Rectangle bounds)
        {
            guardianRight.Add(bounds);
        }
        #endregion
        #endregion

        #region Get Functions
        #region Falling
        public bool isFalling()
        {
            return falling;
        }
        #endregion

        #region Boosting
        public bool isBoosting()
        {
            return boosting;
        }
        #endregion

        #region Boost Ready
        public bool isBoostReady()
        {
            return boostReady;
        }
        #endregion

        #region Get X
        public float getX()
        {
            return position.X;
        }
        #endregion

        #region Get Y
        public float getY()
        {
            return position.Y;
        }
        #endregion

        #region Get Bottom
        public float getBottom()
        {
            return position.Y + guardianRight[0].Height;
        }
        #endregion
        #endregion

        #region Toggle Falling
        public void toggleFalling(bool b)
        {
            falling = b;
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (facingRight)
            {
                if (boosting)
                {
                    spriteBatch.Draw(spriteSheet, new Vector2(position.X - 30, position.Y), guardianRight[index], Color.White);
                    spriteBatch.Draw(spriteSheet, new Vector2(position.X - 15, position.Y), guardianRight[index], Color.White);
                }
                spriteBatch.Draw(spriteSheet, position, guardianRight[index], Color.White);
            }
            else
            {
                if (boosting)
                {
                    spriteBatch.Draw(spriteSheet, new Vector2(position.X + 30, position.Y), guardianLeft[index], Color.White);
                    spriteBatch.Draw(spriteSheet, new Vector2(position.X + 15, position.Y), guardianLeft[index], Color.White);
                }
                spriteBatch.Draw(spriteSheet, position, guardianLeft[index], Color.White);
            }
        }
        #endregion

        #region Move Guardian
        public void moveGuardian(float tmpSpeed)
        {
            position.Y -= tmpSpeed;
        }
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            KeyboardState newState = Keyboard.GetState();
            xMin = 0;
            xMax = getGameWidth() - guardianRight[index].Width; // Get sprite width here

            if (speed.X < 0.001 && speed.X > -0.001)
                changeVelocity(0.0f);

            #region Boost Ready Logic
            if (!boostReady)
            {
                boostCooldown += gameTime.ElapsedGameTime.Milliseconds;

                if (boostCooldown >= 7000)
                {
                    boostReady = true;
                    boostCooldown = 0;
                }
            }
            #endregion

            #region Boosting
            if (boosting)
            {
                boostTimer += gameTime.ElapsedGameTime.Milliseconds;

                // Facing right
                if (facingRight)
                {
                    if (velocity < 1)
                    {
                        changeVelocity(0.1f);
                    }

                    speed.X = speedVar * velocity * 3;

                    if (position.X > xMax)
                    {
                        speed.X = 0.0f;
                        position.X = xMax;
                        boosting = false;
                    }
                }

                // Facing left
                else
                {
                    if (velocity > -1)
                    {
                        changeVelocity(-0.1f);
                    }

                    speed.X = speedVar * velocity * 3;

                    if (position.X < xMin)
                    {
                        speed.X = 0.0f;
                        position.X = xMin;
                        boosting = false;
                    }
                }

                // Boost depleted
                if (boostTimer > 150)
                    boosting = false;

                index = 5;

                if (!boosting)
                {
                    index = 4;
                    boostTimer = 0;
                    boostReady = false;
                    speed.X = 0.0f;
                    keysPressed.Remove(Keys.Space);
                }
            }
            #endregion

            #region Not Boosting
            else
            {
                if (keysPressed.Exists(x => x.Equals(Keys.Right)) && keysPressed.Exists(x => x.Equals(Keys.Left)))
                {
                    #region Left and Right Both Down
                    if (velocity > 0)
                        changeVelocity(-0.1f);

                    else if (velocity < 0)
                        changeVelocity(0.1f);

                    if (newState.IsKeyUp(Keys.Right))
                        keysPressed.Remove(Keys.Right);

                    if (newState.IsKeyUp(Keys.Left))
                        keysPressed.Remove(Keys.Left);

                    if (!falling)
                        index = 4;
                    #endregion
                }

                else
                {
                    #region Boost Activated
                    if (newState.IsKeyDown(Keys.Space) && !keysPressed.Exists(x => x.Equals(Keys.Space)) && !falling && boostReady)
                    {
                        keysPressed.Add(Keys.Space);
                        boosting = true;
                    }
                    #endregion

                    #region Move Right
                    if (newState.IsKeyDown(Keys.Right) && !keysPressed.Exists(x => x.Equals(Keys.Right)))
                        keysPressed.Add(Keys.Right);

                    else if (newState.IsKeyUp(Keys.Right) && keysPressed.Exists(x => x.Equals(Keys.Right)))
                        keysPressed.Remove(Keys.Right);

                    if (keysPressed.Exists(x => x.Equals(Keys.Right)) && !keysPressed.Exists(x => x.Equals(Keys.Left)))
                    {
                        if (velocity < 1)
                        {
                            changeVelocity(0.1f);

                            if (velocity > 0)
                                facingRight = true;
                            else
                                spriteChange--; // Doesn't change sprites while "sliding"
                        }

                        spriteChange++;

                        if (position.X > xMax)
                        {
                            speed.X = 0.0f;
                            position.X = xMax;
                            spriteChange--;
                        }

                        if (spriteChange > 4)
                        {
                            spriteChange = 0;

                            if (index < 3)
                                index++;
                            else
                                index = 0;
                        }
                    }
                    #endregion

                    #region Move Left
                    if (newState.IsKeyDown(Keys.Left) && !keysPressed.Exists(x => x.Equals(Keys.Left)))
                        keysPressed.Add(Keys.Left);

                    else if (newState.IsKeyUp(Keys.Left) && keysPressed.Exists(x => x.Equals(Keys.Left)))
                        keysPressed.Remove(Keys.Left);

                    if (keysPressed.Exists(x => x.Equals(Keys.Left)) && !keysPressed.Exists(x => x.Equals(Keys.Right)))
                    {


                        if (velocity > -1)
                        {
                            changeVelocity(-0.1f);

                            if (velocity < 0)
                                facingRight = false;
                            else
                                spriteChange--; // Doesn't change sprites while "sliding"
                        }

                        spriteChange++;

                        if (position.X < xMin)
                        {
                            speed.X = 0.0f;
                            position.X = xMin;
                            spriteChange--;
                        }

                        if (spriteChange > 4)
                        {
                            spriteChange = 0;

                            if (index < 3)
                                index++;
                            else
                                index = 0;
                        }
                    }
                    #endregion

                    #region Standing Still
                    if (!keysPressed.Exists(x => x.Equals(Keys.Left)) && !keysPressed.Exists(x => x.Equals(Keys.Right)))
                    {
                        if (velocity > 0)
                            changeVelocity(-0.1f);

                        else if (velocity < 0)
                            changeVelocity(0.1f);

                        if (position.X > xMax)
                            position.X = xMax;

                        if (position.X < xMin)
                            position.X = xMin;

                        index = 4;

                    }
                    #endregion
                }
            }
            #endregion

            #region Falling
            if (falling)
            {
                index = 5;
                position.Y += gravityVar;
            }
            #endregion

            // Update saved state.
            oldState = newState;
        }
        #endregion

        #region Velocity Change
        private void changeVelocity(float val)
        {
            if (val == 0.0f)
                velocity = 0.0f;

            else
                velocity += val;

            speed.X = velocity * speedVar;
        }
        #endregion
    }
}
