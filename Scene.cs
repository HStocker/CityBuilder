using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace City_Builder
{

    class Scene
    {
        Constructor constructor;
        List<City> blocks = new List<City>();
        List<Unit> units = new List<Unit>();
        UI ui;
        Cursor cursor;
        KeyboardState state;

        //pixels
        int screenHeight;
        int screenWidth;
        int tileHeight = 20;
        int tileWidth = 16;

        //tiles
        int screenTileHeight;
        int screenTileWidth;
        int[] screenCenter = { 20, 30 };
        int[] currentCorner = { 0, 0 };

        //timers
        private int moveUpdate = 100;
        private int pauseUpdate = 128;
        private int escapeUpdate = 128;
        private int selectUpdate = 128;

        //indices
        private int framePerMove = 6;
        private int moveMod = 1;
        private int selectIndex = 1;

        //misc 
        bool pause = false;
        string controller = "moving";
        string currentIndex = "Root";
        Stack<string> selectionStack = new Stack<string>();

        //XNA contructs and components
        Selectable selection;
        ingameMenu menu;
        SpriteBatch spriteBatch;
        City_Builder ingame;
        Texture2D spriteSheet;
        SpriteFont output18;
        SpriteFont output12;

        //game states for playing

        public Scene(SpriteBatch spriteBatch, City_Builder ingame)
        {
            this.ingame = ingame;
            constructor = new Constructor();
            this.spriteBatch = spriteBatch;

            //test blocks
            blocks.Add(new City(new int[2] { 20, 20 }, "Starter Block", this));
            units.Add(new Unit(new int[2] { 10, 20 }, "Emma", this));

            ui = new UI(ingame.GraphicsDevice.Viewport.Width);
            ui.setLocationRectangle(new Rectangle(ingame.GraphicsDevice.Viewport.Width - 240, 0, 240, 800));

            cursor = new Cursor();
            spriteSheet = ingame.Content.Load<Texture2D>("StrategySpriteSheet.png");
            output18 = ingame.Content.Load<SpriteFont>("Output18pt");
            output12 = ingame.Content.Load<SpriteFont>("Output12pt");

            menu = new ingameMenu(ingame, output18);

            screenHeight = ingame.GraphicsDevice.Viewport.Height;
            screenWidth = ingame.GraphicsDevice.Viewport.Width;
            screenTileHeight = screenHeight / tileHeight;
            screenTileWidth = screenWidth / tileWidth;

        }
        public void resize()
        {
            screenHeight = ingame.GraphicsDevice.Viewport.Height;
            screenWidth = ingame.GraphicsDevice.Viewport.Width;
            screenTileHeight = screenHeight / tileHeight;
            screenTileWidth = screenWidth / tileWidth;
        }

        public void update(GameTime gameTime)
        {
            state = Keyboard.GetState();

            moveUpdate += gameTime.ElapsedGameTime.Milliseconds;
            pauseUpdate += gameTime.ElapsedGameTime.Milliseconds;
            escapeUpdate += gameTime.ElapsedGameTime.Milliseconds;
            selectUpdate += gameTime.ElapsedGameTime.Milliseconds;


            switch (controller)
            {
                case "moving":
                    {
                        cursor.coords = new int[] { (screenTileWidth - 15) / 2, screenTileHeight / 2 };
                        this.moving();
                        foreach (City block in blocks) { block.update(gameTime); }
                        foreach (Unit unit in units) { unit.update(gameTime); }
                        break;
                    }
                case "selecting": { this.selecting(); break; }
                case "escaped": { this.escaped(); break; }
                case "paused": { this.paused(); break; }
                //760,0,240,800
            }
        }

        public void draw()
        {
            //DRAW tiles, blocks, units
            this.drawTiles();
            foreach (City block in blocks) { spriteBatch.Draw(spriteSheet, new Rectangle((block.coords[0] - currentCorner[0]) * tileWidth, (block.coords[1] - currentCorner[1]) * tileHeight, 80, 100), block.getSpriteRectangle(), Color.White); }
            foreach (Unit unit in units) { spriteBatch.Draw(spriteSheet, new Rectangle((unit.coords[0] - currentCorner[0]) * tileWidth, (unit.coords[1] - currentCorner[1]) * tileHeight, 16, 20), unit.getSpriteRectangle(), Color.White); }

            //paused game state updates
            if (pause)
            {
                //draw Cursor
                //Debug.Print("cursor: " + Convert.ToString(cursor.coords[0]) + "," + Convert.ToString(cursor.coords[1]));
                spriteBatch.Draw(spriteSheet,
                    new Rectangle(cursor.coords[0] * tileWidth, cursor.coords[1] * tileHeight, 16, 20),
                    cursor.getSpriteRectangle(), Color.White);
                //draw UI
                spriteBatch.Draw(spriteSheet, ui.getLocationRectangle(), ui.getSpriteRectangle(), Color.White);

                //while hovering over
                if (pause && selection != null)
                {
                    spriteBatch.DrawString(output18, selection.getTag(), ui.getSlot(0), Color.Black);

                    //selection game state updates
                    if (controller.Equals("selecting"))
                    {
                        for (int i = 0; i < selection.getOptions().Length; i++)
                        {
                            Color color = Color.White;
                            if (selection.getOptions()[i] == selection.currentAction) { color = Color.Turquoise; }
                            if (i == selectIndex) { spriteBatch.DrawString(output18, "> " + selection.getOptions()[i], ui.getSlot(i + 1), color); }
                            else { spriteBatch.DrawString(output18, selection.getOptions()[i], ui.getSlot(i + 1), color); }

                            //draw info box
                            Texture2D dummyTexture = new Texture2D(ingame.GraphicsDevice, 1, 1);
                            Texture2D dummyTexture2 = new Texture2D(ingame.GraphicsDevice, 1, 1);
                            dummyTexture.SetData(new Color[] { Color.Gray });
                            spriteBatch.Draw(dummyTexture, new Rectangle(130, 150, 900, 600), Color.Gray);
                            dummyTexture2.SetData(new Color[] { Color.Black });
                            spriteBatch.Draw(dummyTexture2, new Rectangle(145, 165, 870, 570), Color.Gray);

                            spriteBatch.DrawString(output18, selection.getInfo(), new Vector2(225, 175), Color.White);
                        }

                    }
                }
            }

            if (controller.Equals("escaped"))
            {
                //makes black box for menu input
                Texture2D dummyTexture = new Texture2D(ingame.GraphicsDevice, 1, 1);
                Texture2D dummyTexture2 = new Texture2D(ingame.GraphicsDevice, 1, 1);
                dummyTexture.SetData(new Color[] { Color.Gray });
                spriteBatch.Draw(dummyTexture, new Rectangle(200, 150, 900, 600), Color.Gray);
                dummyTexture2.SetData(new Color[] { Color.Black });
                spriteBatch.Draw(dummyTexture2, new Rectangle(215, 165, 870, 570), Color.Gray);

                menu.update();
                spriteBatch.DrawString(output18, menu.draw(), new Vector2(225, 175), Color.White);
            }
        }
        public void togglePause()
        {
            pause = !pause; pauseUpdate = 0;
            if (pause) { screenTileWidth -= 15; screenCenter[0] = 23; }
            else { screenTileWidth += 15; screenCenter[0] = 30; }; pauseUpdate = 0;
            if (currentCorner[0] > constructor.getDimensions()[0] - screenTileWidth) { currentCorner[0] = constructor.getDimensions()[0] - screenTileWidth; }

        }
        public void moving()
        {

            //movement scheme for pause/unpaused playing states

            if (moveUpdate >= 16 * framePerMove)
            {

                if (state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift)) { moveMod = 10; }
                else { moveMod = 1; }

                if (state.IsKeyDown(Keys.W) || state.IsKeyDown(Keys.Up))
                {
                    currentCorner[1] -= moveMod; moveUpdate = 0;
                    if (currentCorner[1] < 0) { currentCorner[1] = 0; }
                }
                if (state.IsKeyDown(Keys.A) || state.IsKeyDown(Keys.Left))
                {
                    currentCorner[0] -= moveMod; moveUpdate = 0;
                    if (currentCorner[0] < 0) { currentCorner[0] = 0; }
                }
                if (state.IsKeyDown(Keys.D) || state.IsKeyDown(Keys.Right))
                {
                    currentCorner[0] += moveMod; moveUpdate = 0;
                    if (currentCorner[0] > constructor.getDimensions()[0] - screenTileWidth) { currentCorner[0] = constructor.getDimensions()[0] - screenTileWidth; }

                }
                if (state.IsKeyDown(Keys.S) || state.IsKeyDown(Keys.Down))
                {
                    currentCorner[1] += moveMod; moveUpdate = 0;
                    if (currentCorner[1] > constructor.getDimensions()[1] - screenTileHeight) { currentCorner[1] = constructor.getDimensions()[1] - screenTileHeight; }

                }

            }
            //end movement for pause/unpause playing state

            //pause toggle
            if (pauseUpdate >= 192 && state.IsKeyDown(Keys.Space)) { this.togglePause(); controller = "paused"; }
            //during pause, selected drawing
            //different escape commands for pause/unpause
            if (escapeUpdate >= 128 && state.IsKeyDown(Keys.Escape)) { escapeUpdate = 0; controller = "escaped"; }
        }
        public void paused()
        {

            if (moveUpdate >= 16 * framePerMove)
            {

                if (state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift)) { moveMod = 10; }
                else { moveMod = 1; }

                if (state.IsKeyDown(Keys.W) || state.IsKeyDown(Keys.Up))
                {
                    cursor.coords[1] -= moveMod;
                    if (cursor.coords[1] <= 0 && currentCorner[1] != 0)
                    { currentCorner[1] -= 10; cursor.coords[1] += 10; };
                    if (cursor.coords[1] < 0)
                    { cursor.coords[1] = 0; };
                    moveUpdate = 0;
                    if (currentCorner[1] < 0) { currentCorner[1] = 0; }
                }
                if (state.IsKeyDown(Keys.A) || state.IsKeyDown(Keys.Left))
                {
                    cursor.coords[0] -= moveMod;
                    if (cursor.coords[0] <= 0 && currentCorner[0] != 0)
                    { currentCorner[0] -= 10; cursor.coords[0] += 10; };
                    if (cursor.coords[0] < 0)
                    { cursor.coords[0] = 0; };
                    moveUpdate = 0;
                    if (currentCorner[0] < 0) { currentCorner[0] = 0; }
                }
                if (state.IsKeyDown(Keys.D) || state.IsKeyDown(Keys.Right))
                {
                    cursor.coords[0] += moveMod;
                    if (cursor.coords[0] >= screenTileWidth - 1 && currentCorner[0] != constructor.getDimensions()[0] - screenTileWidth)
                    { currentCorner[0] += 10; cursor.coords[0] -= 10; };
                    if (cursor.coords[0] > screenTileWidth - 1)
                    { cursor.coords[0] = screenTileWidth - 1; };
                    moveUpdate = 0;
                    if (currentCorner[0] > constructor.getDimensions()[0] - screenTileWidth) { currentCorner[0] = constructor.getDimensions()[0] - screenTileWidth; }

                }
                if (state.IsKeyDown(Keys.S) || state.IsKeyDown(Keys.Down))
                {
                    cursor.coords[1] += moveMod;
                    if (cursor.coords[1] >= screenTileHeight - 1 && currentCorner[1] != constructor.getDimensions()[1] - screenTileHeight)
                    { currentCorner[1] += 10; cursor.coords[1] -= 10; };
                    if (cursor.coords[1] > screenTileHeight - 1)
                    { cursor.coords[1] = screenTileHeight - 1; };
                    moveUpdate = 0;
                    if (currentCorner[1] > constructor.getDimensions()[1] - screenTileHeight) { currentCorner[1] = constructor.getDimensions()[1] - screenTileHeight; }

                }

                selection = null;
                foreach (Unit unit in units) { if (cursor.coords[0] + currentCorner[0] == unit.coords[0] && cursor.coords[1] + currentCorner[1] == unit.coords[1]) { ui.display(unit); selection = unit; } }
                foreach (City block in blocks) { if ((cursor.coords[0] + currentCorner[0] >= block.coords[0] && cursor.coords[0] + currentCorner[0] <= block.coords[0] + 4) && (cursor.coords[1] + currentCorner[1] >= block.coords[1] && cursor.coords[1] + currentCorner[1] <= block.coords[1] + 4)) { ui.display(block); selection = block; } }
                if (selection != null && state.IsKeyDown(Keys.Enter)) { controller = "selecting"; selectUpdate = 0; }

                if (pauseUpdate >= 128 && state.IsKeyDown(Keys.Space)) { pauseUpdate = 0; this.togglePause(); controller = "moving"; }
                if (escapeUpdate >= 128 && state.IsKeyDown(Keys.Escape)) { escapeUpdate = 0; this.togglePause(); controller = "moving"; }
            }
        }
        public void selecting()
        {

            if (moveUpdate >= 128 && (state.IsKeyDown(Keys.W) || state.IsKeyDown(Keys.Up))) { moveUpdate = 0; selectIndex--; if (selectIndex <= 0) { selectIndex = 0; } }
            if (moveUpdate >= 128 && (state.IsKeyDown(Keys.S) || state.IsKeyDown(Keys.Down))) { moveUpdate = 0; selectIndex++; if (selectIndex >= selection.getOptions().Length - 1) { selectIndex = selection.getOptions().Length - 1; } }

            if (selectUpdate >= 128 && (state.IsKeyDown(Keys.Enter) || state.IsKeyDown(Keys.Right) || state.IsKeyDown(Keys.D)))
            {
                if (selection.hasChildren(selection.getOptions()[selectIndex]))
                {
                    Debug.Print("currentIndex: " + Convert.ToString(currentIndex));
                    Debug.Print("selectIndex: " + Convert.ToString(selectIndex));
                    Debug.Print("action: " + Convert.ToString(selection.getOptions()[selectIndex]));
                    selectionStack.Push(currentIndex);
                    currentIndex = selection.getOptions()[selectIndex];
                    selection.setIndex(currentIndex);
                }
                else { selection.setAction(selection.getOptions()[selectIndex]); }
                selectUpdate = 0;
            }
            else if (selectUpdate >= 128 && (state.IsKeyDown(Keys.Left) || state.IsKeyDown(Keys.A)))
            {
                if (selectionStack.Count > 0)
                {
                    string action = selectionStack.Pop();
                    selection.setIndex(action); selectUpdate = 0;
                    selectIndex = Array.IndexOf(selection.getOptions(), currentIndex);
                    currentIndex = action;
                }
            }

            if (escapeUpdate >= 128 && state.IsKeyDown(Keys.Escape)) { selection.reset(); escapeUpdate = 0; selectIndex = 0; controller = "moving"; this.togglePause(); }
            if (pauseUpdate >= 128 && state.IsKeyDown(Keys.Space)) { selection.reset(); pauseUpdate = 0; selectIndex = 0; controller = "paused"; }
        }
        public void escaped()
        {
            if (escapeUpdate >= 128 && state.IsKeyDown(Keys.Escape)) { escapeUpdate = 0; controller = "moving"; }
        }
        public Tile getTile(int x, int y) { return constructor.getTile(x, y); }
        public bool tileExists(int x, int y) { return constructor.includesTile(x,y); }
        public bool tileOccupied(int x, int y) { foreach (Unit unit in units) { if (unit.coords.Equals(new int[] { x, y })) { return true; } } return false; }
        public Tile getTile(int[] coords) { return constructor.getTile(coords[0], coords[1]); }
        public bool tileExists(int[] coords) { return constructor.includesTile(coords[0], coords[1]); }
        public bool tileOccupied(int[] coords) { foreach (Unit unit in units) { if (unit.coords.Equals(new int[] { coords[0], coords[1] })) { return true; } } return false; }
        public int xCorner() { return currentCorner[0]; }
        public int yCorner() { return currentCorner[1]; }
        public void drawTiles()
        {
            for (int i = 0; i < screenTileWidth; i++)
            {
                for (int j = 0; j < screenTileHeight; j++)
                {
                    if (constructor.includesTile(i + currentCorner[0], j + currentCorner[1]))
                    {
                        spriteBatch.Draw(spriteSheet,
                            new Rectangle((i) * tileWidth, (j) * tileHeight, tileWidth, tileHeight),
                            constructor.getTile(i + currentCorner[0], j + currentCorner[1]).getSpriteRectangle(),
                            Color.White);
                    }
                    else
                    {
                        spriteBatch.Draw(spriteSheet,
                             new Rectangle((i) * tileWidth, (j) * tileHeight, tileWidth, tileHeight),
                             new Rectangle(0, 0, 16, 20),
                             Color.White);
                    }
                }
            }
        }


    }

    class Constructor
    {
        Tile[,] tileArray;
        string[] tileTypes = { "blank", "error", "plains", "forest", "hill", "river", "desert", "ocean", "lake", "beach", "snow" };
        int[] connectedTextures = { 2, 3, 4, 5, 7, 8, 9, 10 };
        string currentLevel;

        public Constructor()
        {
            this.build();
            currentLevel = "tileSheet.csv";
        }

        public void build()
        {
            Random rand = new Random();
            string[] ioArray = File.ReadAllText("Content//tileSheet.csv").Split('\n');
            tileArray = new Tile[(ioArray[0].Length + 1) / 2, ioArray.GetLength(0)];
            for (int i = 0; i < ioArray.Length; i++)
            {
                string[] lineSplit = ioArray[i].Split(',');
                for (int j = 0; j < lineSplit.Length; j++)
                {
                    int tileNumber = Convert.ToInt16(lineSplit[j]);
                    if (tileNumber > tileTypes.Length - 1 || tileNumber < 0) { tileNumber = 1; }
                    tileArray[j, i] = new Tile(tileNumber, tileTypes[tileNumber],j,i, rand);
                }
            }
            this.connectWalls(rand);
        }

        public void connectWalls(Random rand)
        {
            int num = -1;
            for (int i = 0; i < tileArray.GetLength(0); i++)
            {
                for (int j = 0; j < tileArray.GetLength(1); j++)
                {
                    char[] surroundings = { 'F', 'F', 'F', 'F' };
                    char[] corners = { 'F', 'F', 'F', 'F' };
                    if (connectedTextures.Contains(this.getTile(i, j).getNum())) { num = this.getTile(i, j).getNum(); }
                    //Debug.Print(Convert.ToString(this.getTile(i,j+1))+", "+Convert.ToString(num));
                    if (tileArray[i, j].getNum() == num)
                    {
                        if (!this.includesTile(i - 1, j) || this.getTile(i - 1, j).getNum() == num) { surroundings[0] = 'T'; }
                        if (!this.includesTile(i - 1, j + 1) || this.getTile(i - 1, j + 1).getNum() == num) { corners[0] = 'T'; }
                        if (!this.includesTile(i, j + 1) || this.getTile(i, j + 1).getNum() == num) { surroundings[1] = 'T'; }
                        if (!this.includesTile(i + 1, j + 1) || this.getTile(i + 1, j + 1).getNum() == num) { corners[1] = 'T'; }
                        if (!this.includesTile(i + 1, j) || this.getTile(i + 1, j).getNum() == num) { surroundings[2] = 'T'; }
                        if (!this.includesTile(i + 1, j - 1) || this.getTile(i + 1, j - 1).getNum() == num) { corners[2] = 'T'; }
                        if (!this.includesTile(i, j - 1) || this.getTile(i, j - 1).getNum() == num) { surroundings[3] = 'T'; }
                        if (!this.includesTile(i - 1, j - 1) || this.getTile(i - 1, j - 1).getNum() == num) { corners[3] = 'T'; }
                    }
                    switch (new string(surroundings))
                    {
                        case "FFFF": { tileArray[i, j].setMeta(5); break; }
                        case "FFFT": { tileArray[i, j].setMeta(6); break; }
                        case "FFTF": { tileArray[i, j].setMeta(7); break; }
                        case "FFTT": { tileArray[i, j].setMeta(8); break; }
                        case "FTFF": { tileArray[i, j].setMeta(9); break; }
                        case "FTFT": { tileArray[i, j].setMeta(10); break; }
                        case "FTTF": { tileArray[i, j].setMeta(11); break; }
                        case "FTTT": { tileArray[i, j].setMeta(12); break; }
                        case "TFFF": { tileArray[i, j].setMeta(13); break; }
                        case "TFFT": { tileArray[i, j].setMeta(14); break; }
                        case "TFTF": { tileArray[i, j].setMeta(15); break; }
                        case "TFTT": { tileArray[i, j].setMeta(16); break; }
                        case "TTFF": { tileArray[i, j].setMeta(17); break; }
                        case "TTFT": { tileArray[i, j].setMeta(18); break; }
                        case "TTTF": { tileArray[i, j].setMeta(19); break; }
                        case "TTTT":
                            {
                                switch (new string(corners))
                                {
                                    case "FFFF": { tileArray[i, j].setMeta(20); break; }
                                    case "FFFT": { tileArray[i, j].setMeta(21); break; }
                                    case "FFTF": { tileArray[i, j].setMeta(22); break; }
                                    case "FFTT": { tileArray[i, j].setMeta(23); break; }
                                    case "FTFF": { tileArray[i, j].setMeta(24); break; }
                                    case "FTFT": { tileArray[i, j].setMeta(25); break; }
                                    case "FTTF": { tileArray[i, j].setMeta(26); break; }
                                    case "FTTT": { tileArray[i, j].setMeta(27); break; }
                                    case "TFFF": { tileArray[i, j].setMeta(28); break; }
                                    case "TFFT": { tileArray[i, j].setMeta(29); break; }
                                    case "TFTF": { tileArray[i, j].setMeta(30); break; }
                                    case "TFTT": { tileArray[i, j].setMeta(31); break; }
                                    case "TTFF": { tileArray[i, j].setMeta(32); break; }
                                    case "TTFT": { tileArray[i, j].setMeta(33); break; }
                                    case "TTTF": { tileArray[i, j].setMeta(34); break; }
                                    case "TTTT": { tileArray[i, j].setMeta(rand.Next(0, 4)); break; }
                                }
                                break;
                            }
                    }

                }
            }
        }

        public int[] getDimensions()
        {
            int[] temp = new int[2];
            temp[0] = this.tileArray.GetLength(0);
            temp[1] = this.tileArray.GetLength(1);
            return temp;
        }

        public bool includesTile(int x, int y)
        {
            //Debug.Print("x:"+Convert.ToString(x)+"\ny:"+Convert.ToString(y)+"\ndim0:"+Convert.ToString(tileArray.GetLength(0))+"\ndim1:"+Convert.ToString(tileArray.GetLength(1)));
            if (x < 0 || x >= this.tileArray.GetLength(0)) { return false; }
            if (y < 0 || y >= this.tileArray.GetLength(1)) { return false; }
            return true;
        }

        public Tile getTile(int x, int y)
        {
            return tileArray[x, y];
        }

    }
    class Cursor : Drawable
    {

        public Cursor()
        {
            this.setSpriteRectangle(new Rectangle(16, 20, 16, 20));
            setCoords(23, 20);
        }
        public void update()
        {

        }
    }
    class UI : Drawable
    {
        int screenWidth;
        Rectangle selected = new Rectangle(527, 767, 510, 32);
        public UI(int screenWidth)
        {
            this.screenWidth = screenWidth;
            this.setSpriteRectangle(new Rectangle(719, 0, 240, 800));
        }
        public void update() { }
        public void display(Selectable info) { }
        public Vector2 getSlot(int input)
        {
            switch (input)
            {
                case 0: return new Vector2(screenWidth - 210, 168);
                case 1: return new Vector2(screenWidth - 210, 218);
                case 2: return new Vector2(screenWidth - 210, 268);
                case 3: return new Vector2(screenWidth - 210, 318);
                case 4: return new Vector2(screenWidth - 210, 368);
                case 5: return new Vector2(screenWidth - 210, 418);
                case 6: return new Vector2(screenWidth - 210, 468);
                case 7: return new Vector2(screenWidth - 210, 518);
                case 8: return new Vector2(screenWidth - 210, 568);
                case 9: return new Vector2(screenWidth - 210, 618);
                case 10: return new Vector2(screenWidth - 210, 668);
                case 11: return new Vector2(screenWidth - 210, 718);
                case 12: return new Vector2(screenWidth - 210, 768);
                default: return new Vector2(0, 0);
            }
        }
    }
    class Tile : Drawable
    {
        string tileType;
        int tileNum;
        public Tile(int tileNum, string tileType,int x, int y, Random rand)
        {
            this.tileNum = tileNum;
            this.tileType = tileType;
            this.coords = new int[] { x, y };

            if (tileNum == 0 || tileNum == 1)
            {
                this.setSpriteRectangle(new Rectangle(0, tileNum * 20, 16, 20));
            }
            else { this.setSpriteRectangle(new Rectangle(rand.Next(0, 5) * 16, tileNum * 20, 16, 20)); }
        }
        public int[] getCoords() { return coords; }
        public void setMeta(int metaData)
        {
            this.setSpriteRectangle(new Rectangle(metaData * 16, tileNum * 20, 16, 20));
        }
        public int getNum() { return this.tileNum; }
        public string getType() { return this.tileType; }
    }
    class ingameMenu
    {
        City_Builder ingame;
        KeyboardState keyboard;
        KeyboardState oldkeyboardState;
        SpriteFont output;

        string entry = "";
        string outputText = "";

        public ingameMenu(City_Builder ingame, SpriteFont output) { this.ingame = ingame; this.output = output; }


        public string draw()
        {
            return "> " + entry + "\n" + outputText;
        }
        public void clear() { entry = ""; outputText = ""; }

        public void update()
        {
            keyboard = Keyboard.GetState();
            char key;
            TryConvertKeyboardInput(keyboard, oldkeyboardState, out key);
            oldkeyboardState = keyboard;
            //Debug.Print(Convert.ToString(key));
            if (!key.Equals('\0') && entry.Length <= 56)
            {
                entry += Convert.ToString(key);
            }
            string[] outputSplit = outputText.Split('\n');
            if (outputSplit.Length > 570 / 32)
            {
                outputText = "\n" + string.Join("\n", outputSplit.Skip(outputSplit.Length - 570 / 32));
                //outputText = outputSplit.Skip(outputSplit.Length - 25).; 
            }
        }

        public void parseCommand(string command)
        {
            string[] key = command.ToUpper().Split(' ');
            switch (key[0])
            {
                case "HELP": { outputText += "\n<" + command + ">" + "\nCOMMANDS:\nNew           -> Start a new game\nSaved         -> List of saved games\nLoad [name]   -> Load a saved game\nDelete [name] -> Delete a saved game\nClear         -> Clear the console\nAbout         -> About the game\nQuit          -> Exit the game"; break; }
                case "CLEAR": { outputText = ""; break; }
                case "NEW": { outputText += "\n<" + command + ">" + "\nNEW"; break; }
                case "LOAD": { outputText += "\n<" + command + ">" + "\nOutput text if is broken"; break; }
                case "SAVED": { outputText += "\n<" + command + ">" + "\nList saved games here"; break; }
                case "ABOUT": { outputText += "\n<" + command + ">" + "\nThis is a game by Hank!  Thank you for playing!"; break; }
                case "DELETE": { outputText += "\n<" + command + ">" + "\nFile successfully deleted (not currently implemented)"; break; }
                case "QUIT": { ingame.Exit(); break; }
                case "EXIT": { ingame.Exit(); break; }
                case "MENU": { ingame.changeGameState("initialize"); break; }
                default: { outputText += "\n<" + command + ">" + "\nI do not recognize your command: '" + command.Split(' ')[0] + "' \n-- Please type 'help' for a list of accepted commands."; break; }
            }
        }

        public bool TryConvertKeyboardInput(KeyboardState keyboard, KeyboardState oldKeyboard, out char key)
        {
            Keys[] keys = keyboard.GetPressedKeys();

            bool shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
            for (int i = 0; i < keys.Length; i++)
            {
                if (!oldKeyboard.IsKeyDown(keys[i]))
                {
                    switch (keys[i])
                    {
                        //Alphabet keys
                        case Keys.A: if (shift) { key = 'A'; } else { key = 'a'; } return true;
                        case Keys.B: if (shift) { key = 'B'; } else { key = 'b'; } return true;
                        case Keys.C: if (shift) { key = 'C'; } else { key = 'c'; } return true;
                        case Keys.D: if (shift) { key = 'D'; } else { key = 'd'; } return true;
                        case Keys.E: if (shift) { key = 'E'; } else { key = 'e'; } return true;
                        case Keys.F: if (shift) { key = 'F'; } else { key = 'f'; } return true;
                        case Keys.G: if (shift) { key = 'G'; } else { key = 'g'; } return true;
                        case Keys.H: if (shift) { key = 'H'; } else { key = 'h'; } return true;
                        case Keys.I: if (shift) { key = 'I'; } else { key = 'i'; } return true;
                        case Keys.J: if (shift) { key = 'J'; } else { key = 'j'; } return true;
                        case Keys.K: if (shift) { key = 'K'; } else { key = 'k'; } return true;
                        case Keys.L: if (shift) { key = 'L'; } else { key = 'l'; } return true;
                        case Keys.M: if (shift) { key = 'M'; } else { key = 'm'; } return true;
                        case Keys.N: if (shift) { key = 'N'; } else { key = 'n'; } return true;
                        case Keys.O: if (shift) { key = 'O'; } else { key = 'o'; } return true;
                        case Keys.P: if (shift) { key = 'P'; } else { key = 'p'; } return true;
                        case Keys.Q: if (shift) { key = 'Q'; } else { key = 'q'; } return true;
                        case Keys.R: if (shift) { key = 'R'; } else { key = 'r'; } return true;
                        case Keys.S: if (shift) { key = 'S'; } else { key = 's'; } return true;
                        case Keys.T: if (shift) { key = 'T'; } else { key = 't'; } return true;
                        case Keys.U: if (shift) { key = 'U'; } else { key = 'u'; } return true;
                        case Keys.V: if (shift) { key = 'V'; } else { key = 'v'; } return true;
                        case Keys.W: if (shift) { key = 'W'; } else { key = 'w'; } return true;
                        case Keys.X: if (shift) { key = 'X'; } else { key = 'x'; } return true;
                        case Keys.Y: if (shift) { key = 'Y'; } else { key = 'y'; } return true;
                        case Keys.Z: if (shift) { key = 'Z'; } else { key = 'z'; } return true;

                        //Decimal keys
                        case Keys.D0: if (shift) { key = ')'; } else { key = '0'; } return true;
                        case Keys.D1: if (shift) { key = '!'; } else { key = '1'; } return true;
                        case Keys.D2: if (shift) { key = '@'; } else { key = '2'; } return true;
                        case Keys.D3: if (shift) { key = '#'; } else { key = '3'; } return true;
                        case Keys.D4: if (shift) { key = '$'; } else { key = '4'; } return true;
                        case Keys.D5: if (shift) { key = '%'; } else { key = '5'; } return true;
                        case Keys.D6: if (shift) { key = '^'; } else { key = '6'; } return true;
                        case Keys.D7: if (shift) { key = '&'; } else { key = '7'; } return true;
                        case Keys.D8: if (shift) { key = '*'; } else { key = '8'; } return true;
                        case Keys.D9: if (shift) { key = '('; } else { key = '9'; } return true;

                        //Decimal numpad keys
                        case Keys.NumPad0: key = '0'; return true;
                        case Keys.NumPad1: key = '1'; return true;
                        case Keys.NumPad2: key = '2'; return true;
                        case Keys.NumPad3: key = '3'; return true;
                        case Keys.NumPad4: key = '4'; return true;
                        case Keys.NumPad5: key = '5'; return true;
                        case Keys.NumPad6: key = '6'; return true;
                        case Keys.NumPad7: key = '7'; return true;
                        case Keys.NumPad8: key = '8'; return true;
                        case Keys.NumPad9: key = '9'; return true;

                        //Special keys
                        case Keys.OemTilde: if (shift) { key = '~'; } else { key = '`'; } return true;
                        case Keys.OemSemicolon: if (shift) { key = ':'; } else { key = ';'; } return true;
                        case Keys.OemQuotes: if (shift) { key = '"'; } else { key = '\''; } return true;
                        case Keys.OemQuestion: if (shift) { key = '?'; } else { key = '/'; } return true;
                        case Keys.OemPlus: if (shift) { key = '+'; } else { key = '='; } return true;
                        case Keys.OemPipe: if (shift) { key = '|'; } else { key = '\\'; } return true;
                        case Keys.OemPeriod: if (shift) { key = '>'; } else { key = '.'; } return true;
                        case Keys.OemOpenBrackets: if (shift) { key = '{'; } else { key = '['; } return true;
                        case Keys.OemCloseBrackets: if (shift) { key = '}'; } else { key = ']'; } return true;
                        case Keys.OemMinus: if (shift) { key = '_'; } else { key = '-'; } return true;
                        case Keys.OemComma: if (shift) { key = '<'; } else { key = ','; } return true;
                        case Keys.Space: key = ' '; return true;
                        case Keys.Back: key = '\0'; if (entry.Length > 0) { entry = entry.Substring(0, entry.Length - 1); }; return true;
                        //change enter command to put result up and clear entry
                        case Keys.Enter: key = '\0'; this.parseCommand(entry); entry = ""; return true;
                    }
                }
            }

            key = (char)0;
            return false;
        }
    }
}
