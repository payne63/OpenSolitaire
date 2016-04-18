﻿/* ©2016 Hathor Gaia 
 * http://HathorsLove.com
 * 
 * Licensed Under GNU GPL 3:
 * http://www.gnu.org/licenses/gpl-3.0.html
 */

using EmptyKeys.UserInterface;
using EmptyKeys.UserInterface.Controls;
using EmptyKeys.UserInterface.Generated;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace OpenSolitaireMG {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class OpenSolitare : Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        const int WindowWidth = 1024;
        const int WindowHeight = 768;


        private Root root;

        private int nativeScreenWidth;
        private int nativeScreenHeight;


        const float cardRatio = 1.452f;

        int spacer, cardWidth, cardHeight;

        bool initialSetup = false;

        
        Texture2D cardSlotTex, cardSlotTex2;
        Texture2D cardBackTex;

        Texture2D refreshMe;
        Rectangle refreshRect;

        List<Card> cards = new List<Card>();
        List<Texture2D> cardTex = new List<Texture2D>();
        List<Deck> tablePile = new List<Deck>();
        List<Deck> scorePile = new List<Deck>();
        List<Vector2> animationQueue = new List<Vector2>();
     
        Deck drawPile, discardPile;
        Rectangle drawPileRect, discardPileRect;

        
        List<Rectangle> cardSlot = new List<Rectangle>();
        List<Rectangle> scoreSlot = new List<Rectangle>();

        private DragAndDropController<Item> _dragDropController;

        public float ScaledWidth(float width) {
            return Window.ClientBounds.Width / WindowWidth;
        }
        public float ScaledHeight(float height) {
            return Window.ClientBounds.Height / WindowHeight;
        }


        public OpenSolitare() {
            graphics = new GraphicsDeviceManager(this);

            // set the screen resolution
            graphics.PreferredBackBufferWidth = WindowWidth;
            graphics.PreferredBackBufferHeight = WindowHeight;

            this.Window.Title = "Open Solitaire";


            this.Window.AllowUserResizing = true;
            IsMouseVisible = true;

            Content.RootDirectory = "Content";

            this.Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);



            //add emptykeys stuff here
            graphics.PreparingDeviceSettings += graphics_PreparingDeviceSettings;
            graphics.DeviceCreated += graphics_DeviceCreated;




            _setSize();
        }

        void Window_ClientSizeChanged(object sender, EventArgs e) { _setSize(); }

        private void _setSize() {

            spacer = Window.ClientBounds.Width / 180;
            cardWidth = (int)(Window.ClientBounds.Width / 7.4);

            float cardHeightF = (float)cardWidth * cardRatio;
            cardHeight = (int)cardHeightF;

            //need some way to call this on resize
            if (_dragDropController != null) {

                SetupDraggableItems();

            }

            if (refreshMe != null) { SetupTable(); }
            

        }


        /// <summary>
        /// Empty Keys Stuff
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void graphics_DeviceCreated(object sender, EventArgs e) {
            Engine engine = new MonoGameEngine(GraphicsDevice, nativeScreenWidth, nativeScreenHeight);
        }

        private void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e) {
            nativeScreenWidth = graphics.PreferredBackBufferWidth;
            nativeScreenHeight = graphics.PreferredBackBufferHeight;

            graphics.PreferredBackBufferWidth = WindowWidth;
            graphics.PreferredBackBufferHeight = WindowHeight;
            graphics.PreferMultiSampling = true;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.SynchronizeWithVerticalRetrace = true;
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 16;
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            
            

            for (int i = 0; i < 7; i++) {
            //    cards.Add(deck.drawCard());
            }                        

            //don't accidentally delete this :O
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {

            //empty keys stuff

            SpriteFont font = Content.Load<SpriteFont>("Segoe_UI_15_Bold");
            //FontManager.DefaultFont = Engine.Instance.Renderer.CreateFont(font);
            root = new Root();

            FontManager.Instance.LoadFonts(Content);
            
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
        
            //add drag and drop support to my cards
            _dragDropController = new DragAndDropController<Item>(this, spriteBatch);
            Components.Add(_dragDropController);


            cardSlotTex = Content.Load<Texture2D>("assets/cardslot");
            cardSlotTex2 = Content.Load<Texture2D>("assets/cardslot2");
            cardBackTex = Content.Load<Texture2D>("assets/back_purple");
            refreshMe = Content.Load<Texture2D>("assets/refresh");


            drawPile = new Deck(cardBackTex);
            drawPile.freshDeck();
            drawPile.shuffle();

            discardPile = new Deck(cardBackTex);

            for (int i = 0; i < 7; i++) {
                tablePile.Add(new Deck(cardBackTex));
            }

            foreach (Card loadTexture in drawPile.cards) {

                loadTexture.SetTexture(Content.Load<Texture2D>(loadTexture.asset));

            }


            SetupDraggableItems();
            SetupTable();

        }

        private void SetupTable() {

            for (int i = 0; i < 4; i++) scoreSlot.Add(new Rectangle());
            for (int i = 0; i < 7; i++) cardSlot.Add(new Rectangle());
            
            int x, y;

            x = spacer * 3;
            y = spacer * 3;

            drawPileRect = new Rectangle(x, y, cardWidth, cardHeight);
            
            foreach (Card card in drawPile.cards) card.SetRectangle(new Rectangle(x, y, cardWidth, cardHeight));
            
            x += cardWidth + spacer;

            discardPileRect = new Rectangle(x, y, cardWidth, cardHeight);

            x += cardWidth * 2 - spacer;

            int newspacer = x;

            for (int i = 0; i < 4; i++) {

                x = (i * (cardWidth + spacer)) + (spacer * 3);
                y = spacer * 3;

                scoreSlot[i] = new Rectangle(x + newspacer, y, cardWidth, cardHeight);
            }

            refreshRect = new Rectangle(drawPileRect.X + cardWidth / 2 - refreshMe.Width, 
                drawPileRect.Y + cardHeight / 2 - refreshMe.Height, refreshMe.Width * 2, refreshMe.Height * 2);


            animationQueue.Clear();

            for (int i = 0; i < 7; i++) {

                x = (i * (cardWidth + spacer)) + (spacer * 3);
                y = cardHeight + spacer * 7;

                cardSlot[i] = new Rectangle(x, y, cardWidth, cardHeight);
                                
                animationQueue.Add(new Vector2(cardSlot[i].X, cardSlot[i].Y));

                //for (int j = 0; j < i+1; j++) { }
                
            }
            

        }

        private void SetupDraggableItems() {

           // _setSize();
            _dragDropController.Clear();

            int x, y;
                    
            //todo: add better logic
            /*
            for (int i = 0; i < 7; i++) {
                
                x = (i * (cardWidth + spacer)) + (spacer * 3);
                y = cardHeight + spacer * 7;
                
                Item item = new Item(spriteBatch, cardTex[i], new Vector2(x, y));
                _dragDropController.Add(item);
            }
            */
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            // empty keys update
            root.UpdateInput(gameTime.ElapsedGameTime.TotalMilliseconds);
            root.UpdateLayout(gameTime.ElapsedGameTime.TotalMilliseconds);


            // TODO: Add your update logic here




            // int x = 1;

            for (int i = 1; i < animationQueue.Count+1; i++) {
                
                Rectangle sprite = drawPile.cards[i-1].rect;
                bool hasArrived = AnimateSprite(ref sprite, animationQueue[i - 1]);

                if (hasArrived) {
                    if (!drawPile.cards[i-1].faceUp) {
                        drawPile.cards[i-1].flipCard();
                    }
                }
                    
                drawPile.cards[i-1].SetRectangle(sprite);

                /*
                if (hasArrived) {

                    Card card = drawPile.cards[i-1];
                    drawPile.cards[i] = card;

                }

                if (hasArrived) {

                    //playCard pops a card object off the deck at the specified index
                    Card card = drawPile.playCard(i-1);
                    if (!card.faceUp) {
                        card.flipCard();
                    }

                    tablePile[i-1].addCard(card);
  //                  animationQueue.Remove(v);
                }
                else {
                    drawPile.cards[drawPile.cards.Count - i].SetRectangle(sprite);     
                }
                */
                
            }




            // Console.WriteLine(sprite.X + "," + sprite.Y);

            base.Update(gameTime);
        }


        bool AnimateSprite(ref Rectangle sprite, Vector2 destination) {

            bool hasArrived = false;

            if (sprite.X < destination.X) sprite.X++; 
            else if (sprite.X > destination.X) sprite.X--;

            if (sprite.Y < destination.Y) sprite.Y++;
            else if (sprite.Y > destination.Y) sprite.Y--;

            if ((sprite.X == destination.X) && (sprite.Y == destination.Y)) hasArrived = true;

            return hasArrived;
            
        }




        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            
            // empty keys draw
            //root.Draw(gameTime.ElapsedGameTime.TotalMilliseconds);

            // z-index is determined by the order which the spriteBatch.Draw methods are called

            spriteBatch.Begin();


            spriteBatch.Draw(cardSlotTex, drawPileRect, Color.Black);
            spriteBatch.Draw(cardSlotTex, discardPileRect, Color.Black);

            foreach (Rectangle slot in scoreSlot) {

                spriteBatch.Draw(cardSlotTex2, slot, Color.Black);

            }

            foreach (Rectangle slot in cardSlot) {

                spriteBatch.Draw(cardSlotTex, slot, Color.Black);

            }

            spriteBatch.Draw(refreshMe, refreshRect, Color.White);

            float ratio = .0019f * Window.ClientBounds.Width / 7;
            

            foreach (var item in _dragDropController.Items) { item.Draw(gameTime,ratio); }

            foreach (Deck deck in tablePile) {

                foreach (Card card in deck.cards) { spriteBatch.Draw(card.texture, card.rect, Color.White); }
                
            }

            for (int i = 0; i < drawPile.cards.Count; i++) { 
                
               spriteBatch.Draw(drawPile.cards[i].texture, drawPile.cards[i].rect, Color.White);

            }

            
            spriteBatch.End();




            base.Draw(gameTime);
        }
    }
}
