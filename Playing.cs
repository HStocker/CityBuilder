using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace City_Builder
{
    class Playing : GameState
    {
        Scene scene;
        SpriteBatch spriteBatch;
        City_Builder ingame;
        string tag = "playing";

        public Playing(SpriteBatch spriteBatch, City_Builder ingame) 
        {
            this.spriteBatch = spriteBatch;
            this.ingame = ingame;
            this.scene = new Scene(spriteBatch, ingame);
            
        }

        public string getTag() { return this.tag; }

        public void update(GameTime gameTime) 
        {
            scene.update(gameTime);
        }
        public void draw() 
        {
            scene.draw();
        }

        public void entering() { }
        public void leaving() { }
    }
}
