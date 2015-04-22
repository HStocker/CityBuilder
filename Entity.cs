using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Wintellect.PowerCollections;
using System.Collections;


namespace City_Builder
{

    class Unit : Selectable
    {
        Queue<Tile> path = new Queue<Tile>();
        Random rand = new Random();

        Tile destination;
        int danceIndex = 0;
        int danceTimer = 64;
        int moveTimer = 64;
        int idleTimer = 5000;
        int idleRadius = 10;
        int[] idleCenter;

        int happiness = 7;
        int woodCutting = 2;
        int mining = 1;
        int farming = 3;

        public Unit(int[] coords, string inTag, Scene scene)
        {
            this.parent = scene;
            this.tag = inTag;
            this.coords = coords;
            this.destination = parent.getTile(coords);
            this.setSpriteRectangle(new Rectangle(64, 0, 16, 20));
            this.contructInfo();
            currentAction = "Idle";

            choiceTree.Add("Root", new string[] { "Move", "Build", "Idle", "Dance" });
            choiceTree.Add("Build", new string[] { "Quarry", "Logging Camp", "Farm" });
            choiceTree.Add("Farm", new string[] { "Wheat", "Rye", "Corn", "Potato", "Strawberry" });
            this.options = choiceTree["Root"];
            this.previousAction = "Nothing";
        }
        public void setDestination(int x, int y)
        {
            destination = parent.getTile(x,y);
            Tile current = parent.getTile(coords[0],coords[1]);
            List<Tile> closedSet = new List<Tile>();
            PrioQueue openSet = new PrioQueue();

            openSet.Enqueue(current,manhattanDistance(current,destination));

            while (openSet.Size() > 0)
            {
                if (current.getCoords()[0] == destination.getCoords()[0] && current.getCoords()[1] == destination.getCoords()[1]) { break; }

                Tile old = current;

                if (parent.tileExists(current.getCoords()[0]+1,current.getCoords()[1]) && !closedSet.Contains(parent.getTile(current.getCoords()[0]+1,current.getCoords()[1])) && parent.getTile(current.getCoords()[0]+1,current.getCoords()[1]).getNum() != 4) { openSet.Enqueue(parent.getTile(current.getCoords()[0]+1,current.getCoords()[1]), manhattanDistance(parent.getTile(current.getCoords()[0]+1,current.getCoords()[1]), destination)); }
                if (parent.tileExists(current.getCoords()[0],current.getCoords()[1]+1) && !closedSet.Contains(parent.getTile(current.getCoords()[0],current.getCoords()[1]+1)) && parent.getTile(current.getCoords()[0],current.getCoords()[1]+1).getNum() != 4) { openSet.Enqueue(parent.getTile(current.getCoords()[0],current.getCoords()[1]+1), manhattanDistance(parent.getTile(current.getCoords()[0],current.getCoords()[1]+1), destination)); }
                if (parent.tileExists(current.getCoords()[0]-1,current.getCoords()[1]) && !closedSet.Contains(parent.getTile(current.getCoords()[0]-1,current.getCoords()[1])) && parent.getTile(current.getCoords()[0]-1,current.getCoords()[1]).getNum() != 4) { openSet.Enqueue(parent.getTile(current.getCoords()[0]-1,current.getCoords()[1]), manhattanDistance(parent.getTile(current.getCoords()[0]-1,current.getCoords()[1]), destination)); }
                if (parent.tileExists(current.getCoords()[0],current.getCoords()[1]-1) && !closedSet.Contains(parent.getTile(current.getCoords()[0],current.getCoords()[1]-1)) && parent.getTile(current.getCoords()[0],current.getCoords()[1]-1).getNum() != 4) { openSet.Enqueue(parent.getTile(current.getCoords()[0],current.getCoords()[1]-1), manhattanDistance(parent.getTile(current.getCoords()[0],current.getCoords()[1]-1), destination)); }
                
                closedSet.Add(current);
                current = (Tile)openSet.Peek();
                openSet.Dequeue();
                path.Enqueue(current);

                Debug.Print(Convert.ToString(openSet.Size()));

                //!parent.getTile(current[0] + 1, current[1]).getTag().Equals("hill")
            }

        }
        public int manhattanDistance(int[] A, int[] B)
        {
            return Math.Abs(A[0] - B[0]) + Math.Abs(A[1] - B[1]);
        }
        public int manhattanDistance(Tile A, Tile B)
        {
            return Math.Abs(A.getCoords()[0] - B.getCoords()[0]) + Math.Abs(A.getCoords()[1] - B.getCoords()[1]);
        }

        public void update(GameTime gameTime)
        {
            Debug.Print(currentAction);
            this.contructInfo();
            danceTimer += gameTime.ElapsedGameTime.Milliseconds;
            moveTimer += gameTime.ElapsedGameTime.Milliseconds;
            idleTimer += gameTime.ElapsedGameTime.Milliseconds;

            switch (currentAction)
            {
                case "Dance":
                    {
                        previousAction = "Dance";
                        if (danceTimer >= 128)
                        {
                            danceTimer = 0;
                            this.setSpriteRectangle(new Rectangle(64 + (16 * danceIndex), 0, 16, 20));
                            danceIndex++;
                            if (danceIndex > 3) { danceIndex = 0; }
                        }
                        break;
                    }
                case "Idle":
                    {
                        if (!previousAction.Equals("Idle")) { idleCenter = coords; previousAction = "Idle"; }
                        if (this.destination.getCoords()[0] != this.coords[0] || this.destination.getCoords()[1] != this.coords[1])
                        {
                            this.move();
                        }
                        else if (idleTimer > 5000 && path.Count == 0)
                        {
                            path.Clear();
                            idleTimer = 0;
                            this.setDestination(rand.Next(((-1) * idleRadius) + idleCenter[0], idleRadius + idleCenter[0]), rand.Next(((-1) * idleRadius) + idleCenter[1], idleRadius + idleCenter[1]));
                        }
                        break;
                    }
                case "Move":
                    {
                        previousAction = "Move";
                        //this.move();
                        break;
                    }
                default: { break; }
            }
        }

        public void move()
        {
            Debug.Print(Convert.ToString(moveTimer));
            if (moveTimer > 225)
            //&& !parent.tileOccupied(path.Peek()[0],path.Peek()[1])
            {
                Debug.Print("get here?");
                moveTimer = 0;
                if (parent.tileExists(path.Peek().getCoords()[0], path.Peek().getCoords()[1]))
                {
                    Tile temp = path.Dequeue();
                    coords = new int[] { temp.getCoords()[0], temp.getCoords()[1] };
                }
                else { path.Clear(); if (!currentAction.Equals("Idle")) { currentAction = "Nothing"; } }
            }
        }
        public void contructInfo()
        {

            this.info = "";
            this.info += "Current Action: " + currentAction + "\n";
            this.info += "Happiness:\n" + "[" + String.Concat(Enumerable.Repeat((char)216, happiness)) + "\n";
            this.info += "Wood Cutting:\n" + "[" + String.Concat(Enumerable.Repeat((char)216, woodCutting)) + "\n";
            this.info += "Mining:\n" + "[" + String.Concat(Enumerable.Repeat((char)216, mining)) + "\n";
            this.info += "Farming:\n" + "[" + String.Concat(Enumerable.Repeat((char)216, farming)) + "\n";
        }
        public override string getInfo()
        {
            this.contructInfo();
            return base.getInfo();
        }
    }
    class City : Selectable
    {

        public City(int[] coords, string inTag, Scene scene)
        {
            this.parent = scene;
            this.tag = inTag;
            this.coords = coords;
            this.setSpriteRectangle(new Rectangle(0, 220, 80, 100));
            this.info = "Put information here\ntesting size\nHAPPINESS:\n[X|X|X| | | | |]";
            choiceTree.Add("Root", new string[] { "Build", "Free", "Dance" });
            choiceTree.Add("Build", new string[] { "Quarry", "Logging Camp", "Farm" });
            this.options = choiceTree["Root"];
        }
        public void update(GameTime gameTime) { }
    }


    class Selectable : Drawable
    {
        public Dictionary<string, string[]> choiceTree = new Dictionary<string, string[]>();
        public string currentAction;
        public string previousAction;
        public string info = "";
        public string[] options;
        public Scene parent;

        public string[] getOptions() { return options; }
        public void setIndex(string index) { if (choiceTree.ContainsKey(index)) { this.options = choiceTree[index]; } }
        public void setAction(string action) { this.currentAction = action; }
        public bool hasChildren(string key) { return choiceTree.ContainsKey(key); }
        public void reset() { this.options = choiceTree["Root"]; }
        public virtual string getInfo() { return this.info; }
    }

    class Drawable
    {
        public string tag { get; set; }
        public double time = 0;
        public int[] coords = new int[2];
        public int level;
        public double rotation;
        //use level for animated sprites

        public bool toBeDeleted = false;
        public bool escaped = false;

        Rectangle spriteRectangle;
        Rectangle locationRectangle;

        public string getTag() { return tag; }
        public void setTag(string tagIn) { this.tag = tagIn; }
        public void setSpriteRectangle(Rectangle rectangle) { this.spriteRectangle = rectangle; }
        public void setLocationRectangle(Rectangle rectangle) { this.locationRectangle = rectangle; }
        public void setCoords(int x, int y) { coords[0] = x; coords[1] = y; }
        public Rectangle getSpriteRectangle() { return this.spriteRectangle; }
        public Rectangle getLocationRectangle() { return this.locationRectangle; }
        public bool collides(Drawable other)
        {
            return this.locationRectangle.Intersects(other.getLocationRectangle());
        }
    }

    public class PrioQueue
    {
        int total_size;
        SortedDictionary<int, Queue> storage;

        public PrioQueue()
        {
            this.storage = new SortedDictionary<int, Queue>();
            this.total_size = 0;
        }

        public int Size() { return total_size; }

        public bool IsEmpty()
        {
            return (total_size == 0);
        }

        public object Dequeue()
        {
            if (IsEmpty())
            {
                throw new Exception("Please check that priorityQueue is not empty before dequeing");
            }
            else
                foreach (Queue q in storage.Values)
                {
                    // we use a sorted dictionary
                    if (q.Count > 0)
                    {
                        total_size--;
                        return q.Dequeue();
                    }
                }

            Debug.Assert(false, "not supposed to reach here. problem with changing total_size");

            return null; // not supposed to reach here.
        }

        // same as above, except for peek.

        public object Peek()
        {
            if (IsEmpty())
                throw new Exception("Please check that priorityQueue is not empty before peeking");
            else
                foreach (Queue q in storage.Values)
                {
                    if (q.Count > 0)
                        return q.Peek();
                }

            Debug.Assert(false, "not supposed to reach here. problem with changing total_size");

            return null; // not supposed to reach here.
        }

        public object Dequeue(int prio)
        {
            total_size--;
            return storage[prio].Dequeue();
        }

        public void Enqueue(object item, int prio)
        {
            if (!storage.ContainsKey(prio))
            {
                storage.Add(prio, new Queue());
            }
            storage[prio].Enqueue(item);
            total_size++;

        }
    }

}


/* unit
 * display information on the bottom
 * change the screen height to fit
 * Current Action:  whatever
 * HAPPINESS:                               WOOD:             PICTURE_____
 *[XXXXXXXXXXX]                            [XXXXXXXXXXXX]           /     \
 * MINING:                                  FARMING:                | ' ' |
 *[XXXXXXXXXXX]                            [XXXXXXXXXXXX]
*/