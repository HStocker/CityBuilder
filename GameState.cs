using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace City_Builder
{
    interface GameState
    {
        string getTag();
        void update(GameTime gameTime);
        void draw();
        void entering();
        void leaving();
    }
}
