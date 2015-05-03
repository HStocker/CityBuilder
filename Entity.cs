using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Collections;


namespace City_Builder
{

    class Unit : Selectable
    {
        Queue<Tile> path = new Queue<Tile>();
        Random rand = new Random(Guid.NewGuid().GetHashCode());
        string[] buildings = new string[] { "Quarry", "Logging Camp", "Wheat", "Rye", "Corn", "Potato", "Strawberry" }; 
        Stack<int[]> workStack = new Stack<int[]>();
        Structure currentProject;
        public City home;
        Rectangle professionSprite;

        Tile destination;
        int danceIndex = 0;
        int danceTimer = 64;

        int gatherTimer = 128;
        int moveTimer = 64;
        int buildTimer = 0;
        int hungerTimer = 0;
        int hunger = 15;

        int idleTimer = 5000;
        int idleRadius = 3;
        int idleCap;
        int[] idleCenter;

        int happiness = 1;
        int woodCutting = 1;
        int woodExp = 0;

        int mining = 1;
        int mineExp = 0;

        int farming = 1;
        int farmExp = 0;

        Stack<string> inventory = new Stack<string>();
        int fullInventory = 32;
        int[] matRequirements;

        public Unit(int[] coords, string inTag, City home, Scene scene)
        {
            this.home = home;
            home.addUnit(this);
            this.parent = scene;
            this.tag = inTag;
            this.coords = coords;
            this.destination = parent.getTile(coords);
            this.setSpriteRectangle(new Rectangle(64, 0, 16, 20));
            this.professionSprite = getSpriteRectangle();
            this.contructInfo();
            this.idleCap = rand.Next(6000, 19000);
            currentAction = "Idle";

            choiceTree.Add("Root", new string[] { "Move", "Build", "Idle", "Assign", "Unload", "Dance" });
            choiceTree.Add("Build", new string[] { "Quarry", "Logging Camp", "Farm" });
            choiceTree.Add("Farm", new string[] { "Wheat", "Rye", "Corn", "Potato", "Strawberry" });
            this.options = choiceTree["Root"];
            this.previousAction = "Nothing";
        }
        public void setDestination(int x, int y)
        {
            if (coords[0] != destination.getCoords()[0] && coords[1] != destination.getCoords()[1] && previousAction != "Idle") { workStack.Push(new int[] { destination.coords[0], destination.coords[1] }); }
            path.Clear();
            destination = parent.getTile(x,y);
            Tile current = parent.getTile(coords);
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

                //!parent.getTile(current[0] + 1, current[1]).getTag().Equals("hill")
            }

        }

        public void setDestination(int[] y)
        {
            if (coords[0] != destination.getCoords()[0] && coords[1] != destination.getCoords()[1] && previousAction != "Idle") { workStack.Push(destination.coords); }
            path.Clear();
            destination = parent.getTile(y);
            Tile current = parent.getTile(coords);
            List<Tile> closedSet = new List<Tile>();
            PrioQueue openSet = new PrioQueue();

            openSet.Enqueue(current, manhattanDistance(current, destination));

            while (openSet.Size() > 0)
            {
                if (current.getCoords()[0] == destination.getCoords()[0] && current.getCoords()[1] == destination.getCoords()[1]) { break; }

                Tile old = current;

                if (parent.tileExists(current.getCoords()[0] + 1, current.getCoords()[1]) && !closedSet.Contains(parent.getTile(current.getCoords()[0] + 1, current.getCoords()[1])) && parent.getTile(current.getCoords()[0] + 1, current.getCoords()[1]).getNum() != 0) { openSet.Enqueue(parent.getTile(current.getCoords()[0] + 1, current.getCoords()[1]), manhattanDistance(parent.getTile(current.getCoords()[0] + 1, current.getCoords()[1]), destination)); }
                if (parent.tileExists(current.getCoords()[0], current.getCoords()[1] + 1) && !closedSet.Contains(parent.getTile(current.getCoords()[0], current.getCoords()[1] + 1)) && parent.getTile(current.getCoords()[0], current.getCoords()[1] + 1).getNum() != 0) { openSet.Enqueue(parent.getTile(current.getCoords()[0], current.getCoords()[1] + 1), manhattanDistance(parent.getTile(current.getCoords()[0], current.getCoords()[1] + 1), destination)); }
                if (parent.tileExists(current.getCoords()[0] - 1, current.getCoords()[1]) && !closedSet.Contains(parent.getTile(current.getCoords()[0] - 1, current.getCoords()[1])) && parent.getTile(current.getCoords()[0] - 1, current.getCoords()[1]).getNum() != 0) { openSet.Enqueue(parent.getTile(current.getCoords()[0] - 1, current.getCoords()[1]), manhattanDistance(parent.getTile(current.getCoords()[0] - 1, current.getCoords()[1]), destination)); }
                if (parent.tileExists(current.getCoords()[0], current.getCoords()[1] - 1) && !closedSet.Contains(parent.getTile(current.getCoords()[0], current.getCoords()[1] - 1)) && parent.getTile(current.getCoords()[0], current.getCoords()[1] - 1).getNum() != 0) { openSet.Enqueue(parent.getTile(current.getCoords()[0], current.getCoords()[1] - 1), manhattanDistance(parent.getTile(current.getCoords()[0], current.getCoords()[1] - 1), destination)); }

                closedSet.Add(current);
                current = (Tile)openSet.Peek();
                openSet.Dequeue();
                path.Enqueue(current);

                //!parent.getTile(current[0] + 1, current[1]).getTag().Equals("hill")
            }
        }
        public void setBuilding(Structure currentProject)
        {
            this.currentProject = currentProject;
            matRequirements = currentProject.getCost();
        }
        public int manhattanDistance(int[] A, int[] B)
        {
            return Math.Abs(A[0] - B[0]) + Math.Abs(A[1] - B[1]);
        }
        public int manhattanDistance(Tile A, Tile B)
        {
            return Math.Abs(A.getCoords()[0] - B.getCoords()[0]) + Math.Abs(A.getCoords()[1] - B.getCoords()[1]);
        }
        public bool notAtDestination() { return !(coords[0] == destination.coords[0] && coords[1] == destination.coords[1]); }
        public void update(GameTime gameTime)
        {
            //Debug.Print(this.tag + ": "+currentAction);
            //this.contructInfo();
            danceTimer += gameTime.ElapsedGameTime.Milliseconds;
            moveTimer += gameTime.ElapsedGameTime.Milliseconds;
            idleTimer += gameTime.ElapsedGameTime.Milliseconds;
            gatherTimer += gameTime.ElapsedGameTime.Milliseconds;
            hungerTimer += gameTime.ElapsedGameTime.Milliseconds;

            switch (currentAction)
            {
                case "Nothing":
                    {
                        if (previousAction != "Build" && parent.validBuildSpot(this, this.coords)) { currentAction = "Build"; }
                        else if (path.Count > 0) { currentAction = "Move"; }
                        else if (workStack.Count > 0) { this.setDestination(workStack.Pop()); currentAction = "Move"; }
                        else if (this.coords[0] == home.coords[0] + 2 && this.coords[1] == home.coords[1] + 2) { if (previousAction.Equals("Eat") && currentProject != null) { setDestination(currentProject.coords); currentAction = "Move"; } else { currentAction = "Unload"; } }
                        else if (this.coords[0] == currentProject.coords[0] && this.coords[1] == currentProject.coords[1]) { if (!currentProject.finished) { currentAction = "Build"; } else { useBuilding(currentProject); } }
                      
                        break;
                    }
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
                        else if (idleTimer > idleCap && path.Count == 0)
                        {
                            path.Clear();
                            idleTimer = 0;
                            int x = rand.Next(((-1) * idleRadius) + idleCenter[0], idleRadius + idleCenter[0]);
                            int y = rand.Next(((-1) * idleRadius) + idleCenter[1], idleRadius + idleCenter[1]);
                            if (parent.tileExists(x, y))
                            {
                                this.setDestination(x, y);
                            }
                        }
                        break;
                    }
                case "Move":
                    {
                        previousAction = "Move";
                        if (path.Count > 0)
                        {
                            this.move();
                        }
                        else { this.setAction("Nothing"); }
                        break;
                    }
                case "Build":
                    {
                        previousAction = "Build";
                        buildTimer += gameTime.ElapsedGameTime.Milliseconds;
                        if (buildTimer > 1000 && inventory.Count > 0) {
                            if (inventory.Peek().Equals("Wood") && currentProject.requirements[0] < currentProject.getCost()[0]) { currentProject.requirements[0]++; ; inventory.Pop(); }
                            else if (inventory.Peek().Equals("Stone") && currentProject.requirements[1] < currentProject.getCost()[1]) { currentProject.requirements[1]++; ; inventory.Pop(); }
                            else if (inventory.Peek().Equals("Food") && currentProject.requirements[2] < currentProject.getCost()[2]) { currentProject.requirements[2]++; ; inventory.Pop(); }
                            setSpriteRectangle(new Rectangle(64, 20, 16, 20));
                            buildTimer = 0;
                        } 
                        else 
                        { 
                            if (buildTimer > 64) 
                            { 
                                setSpriteRectangle(new Rectangle(64, 0, 16, 20));
                                if (inventory.Count == 0 && currentProject.reqsFull()) {
                                    currentProject.setSpriteRectangle(new Rectangle(222,0,47,39));
                                    currentProject.finished = true;
                                    useBuilding(currentProject);
                                }
                                else if (inventory.Count == 0) { currentAction = "Load"; } 
                                
                            } 
                        } 
                        break;
                    }
                case "Farm":
                    {
                        if (gatherTimer >= 128 * (20 - farming))
                        {
                            if (inventory.Count < fullInventory)
                            {
                                inventory.Push("Food");
                                farmExp += 5;
                                setSpriteRectangle(new Rectangle(96, 20, 16, 20));
                                gatherTimer = 0;
                                if (farmExp > farming * 100 && farming <= 20) { farming++; }
                            }
                            else { this.setDestination(home.coords[0] + 2, home.coords[1] + 2); currentAction = "Move"; setSpriteRectangle(new Rectangle(64, 0, 16, 20)); }
                        }
                        else { if (gatherTimer > 64) { setSpriteRectangle(new Rectangle(96, 0, 16, 20)); } } 
                        break;
                    }
                case "Mine":
                    {
                        if (gatherTimer >= 128 * (20 - mining))
                        {
                            if (inventory.Count < fullInventory)
                            {
                                inventory.Push("Stone");
                                mineExp += 5;
                                setSpriteRectangle(new Rectangle(80, 20, 16, 20));
                                gatherTimer = 0;
                                if (mineExp > mining * 100 && mining <= 20) { mining++; }
                            }
                            else { this.setDestination(home.coords[0]+2,home.coords[1]+2); currentAction = "Move"; setSpriteRectangle(new Rectangle(64, 0, 16, 20)); }
                        }
                        else { if (gatherTimer > 64) { setSpriteRectangle(new Rectangle(80, 0, 16, 20)); } }
                        break;
                    }
                case "Cut Wood":
                    {
                        if (gatherTimer >= 128 * (20 - woodCutting))
                        {
                            if (inventory.Count < fullInventory)
                            {
                                inventory.Push("Wood");
                                woodExp += 5;
                                setSpriteRectangle(new Rectangle(112, 20, 16, 20));
                                gatherTimer = 0;
                                if (woodExp > woodCutting * 100 && woodCutting <= 20) { woodCutting++; }
                            }
                            else { this.setDestination(home.coords[0] + 2, home.coords[1] + 2); currentAction = "Move"; setSpriteRectangle(new Rectangle(64, 0, 16, 20)); }
                        }
                        else { if (gatherTimer > 64) { setSpriteRectangle(new Rectangle(112, 0, 16, 20)); } }
                        break;
                    }
                case "Load":
                    {
                        if (destination.coords[0] != home.coords[0] + 2 || destination.coords[1] != home.coords[1] + 2) { this.setDestination(home.coords[0] + 2, home.coords[1] + 2); }
                        else if (coords[0] != home.coords[0] + 2 || coords[1] != home.coords[1] + 2) { this.move(); }
                        else if (gatherTimer > 1000 && inventory.Count <= fullInventory) {
                            if (matRequirements[0] > 0) { matRequirements[0]--; ; inventory.Push(home.loadUnit("Wood")); }
                            else if (matRequirements[1] > 0) { matRequirements[1]--; ; inventory.Push(home.loadUnit("Stone")); }
                            else if (matRequirements[2] > 0) { matRequirements[2]--; ; inventory.Push(home.loadUnit("Food")); }
                            setSpriteRectangle(new Rectangle(64, 20, 16, 20));
                            gatherTimer = 0;
                        } 
                        else 
                        { 
                            if (gatherTimer > 64) 
                            { 
                                setSpriteRectangle(new Rectangle(64, 0, 16, 20));
                                if (inventory.Count == fullInventory || matRequirements.Max() == 0)
                                {
                                    setDestination(currentProject.coords); currentAction = "Move";
                                }                                
                            } 
                        } 
                        break; 
                    }
                case "Unload": 
                    {
                        if (destination.coords[0] != home.coords[0] + 2 || destination.coords[1] != home.coords[1] + 2) { this.setDestination(home.coords[0] + 2, home.coords[1] + 2); }
                        else if (coords[0] != home.coords[0] + 2 || coords[1] != home.coords[1] + 2) { this.move(); }
                        else if (gatherTimer > 1000 && inventory.Count > 0) { 
                            home.add(inventory.Pop()); 
                            setSpriteRectangle(new Rectangle(64, 20, 16, 20));
                            gatherTimer = 0;
                        } 
                        else 
                        { 
                            if (gatherTimer > 64) 
                            { 
                                setSpriteRectangle(new Rectangle(64, 0, 16, 20));
                                if (inventory.Count == 0) { if (currentProject != null) { setDestination(currentProject.coords); currentAction = "Move"; } else { idleRadius = 2; currentAction = "Idle"; } }
                                
                            } 
                        } 
                        break; }
                case "Eat":
                    {
                        this.setSpriteRectangle(new Rectangle(32, 0, 16, 20));
                        if (!previousAction.Equals("Eat")) { previousAction = "Eat"; }
                        if (notAtDestination()) { this.move(); }
                        else { home.feed(hunger); this.setSpriteRectangle(professionSprite); currentAction = "Nothing"; }
                        break;
                    }
                default: { break; }
            }
            if (hungerTimer >= 50000) { this.setDestination(home.coords[0] + 2, home.coords[1] + 2); currentAction = "Eat"; hungerTimer = 0; }
        }

        public override void checkStateChange()
        {
            if (buildings.Contains(currentAction)) { parent.startBuilding(currentAction); currentAction = "Nothing"; }
        }

        public void useBuilding(Structure instruct) 
        {

            switch (instruct.getTag())
            {
                case "Potato": { currentAction = "Farm"; this.setSpriteRectangle(new Rectangle(96, 0, 16, 20)); this.professionSprite = this.getSpriteRectangle(); break; }
                case "Strawberry": { currentAction = "Farm"; this.setSpriteRectangle(new Rectangle(96, 0, 16, 20)); this.professionSprite = this.getSpriteRectangle(); break; }
                case "Corn": { currentAction = "Farm"; this.setSpriteRectangle(new Rectangle(96, 0, 16, 20)); this.professionSprite = this.getSpriteRectangle(); break; }
                case "Wheat": { currentAction = "Farm"; this.setSpriteRectangle(new Rectangle(96, 0, 16, 20)); this.professionSprite = this.getSpriteRectangle(); break; }
                case "Rye": { currentAction = "Farm"; this.setSpriteRectangle(new Rectangle(96, 0, 16, 20)); this.professionSprite = this.getSpriteRectangle(); break; }
                case "Quarry": { currentAction = "Mine"; this.setSpriteRectangle(new Rectangle(80, 0, 16, 20)); this.professionSprite = this.getSpriteRectangle(); break; }
                case "Logging Camp": { currentAction = "Cut Wood"; this.setSpriteRectangle(new Rectangle(112, 0, 16, 20)); this.professionSprite = this.getSpriteRectangle(); break; }
            }
        }
        public void move()
        {
            if (moveTimer > 225 && !parent.tileOccupied(path.Peek().coords[0], path.Peek().coords[1]))
            //&& !parent.tileOccupied(path.Peek()[0],path.Peek()[1])
            {
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
            this.info += string.Format("Current Action: {0}\n",currentAction);
            this.info += string.Format("Happiness:                 Mining:                   INVENTORY({0}):\n",fullInventory);
            this.info += string.Format("[{0}{1}]     ", String.Concat(Enumerable.Repeat((char)216, happiness)), String.Concat(Enumerable.Repeat(" ", 20 - happiness)));
            this.info += string.Format("[{0}{1}]    Food = {2}\n", String.Concat(Enumerable.Repeat((char)216, mining)), String.Concat(Enumerable.Repeat(" ", 20 - mining)), Convert.ToString(inventory.Count(item => item.Equals("Food"))));
            this.info += string.Format("Wood Cutting:              Farming:                  Stone = {0}\n", Convert.ToString(inventory.Count(item => item.Equals("Stone"))));
            this.info += string.Format("[{0}{1}]     ", String.Concat(Enumerable.Repeat((char)216, woodCutting)), String.Concat(Enumerable.Repeat(" ", 20 - woodCutting)));
            this.info += string.Format( "[{0}{1}]    Wood = {2}", String.Concat(Enumerable.Repeat((char)216, farming)), String.Concat(Enumerable.Repeat(" ", 20 - farming)), Convert.ToString(inventory.Count(item => item.Equals("Wood"))));
            
        }
        public override string getInfo()
        {
            this.contructInfo();
            return base.getInfo();
        }
    }
    class Structure : Selectable
    {
        int[] dimensions;
        public bool finished = false;
        public int[] requirements;

        public Structure(int[] coords, int[] dimensions, string inTag, Scene scene)
        {
            this.dimensions = dimensions;
            this.parent = scene;
            this.tag = inTag;
            this.coords = coords;
            this.setSpriteRectangle(new Rectangle(128, 0, 47, 39));
            this.setLocationRectangle(new Rectangle(coords[0] * 16, coords[1] * 20, dimensions[0] * 16, dimensions[1] * 20));
            choiceTree.Add("Root", new string[] { "blah", "blahblah", "asd", "asdf" });
            this.options = choiceTree["Root"];
            requirements = new int[] {0,0,0};
        }
        public void update(GameTime gameTime) { }
        public bool reqsFull() { return requirements[0] == getCost()[0] && requirements[1] == getCost()[1] && requirements[2] == getCost()[2]; }
        public string buildWithItem(string type)
        {
            switch (type)
            {
                case "Food": { requirements[0]--; return type; }
                case "Stone": { requirements[1]--; return type; }
                case "Wood": { requirements[2]--; return type; }
                default: { return ""; }
            }
        }
        public int[] getCost() 
        {
            switch (tag)
            {
                //WOOD STONE FOOD
                case "Potato": { return new int[] { 25, 0, 10 };  }
                case "Strawberry": { return new int[] { 25, 0, 10 };  }
                case "Corn": { return new int[] { 25, 0, 10 };  }
                case "Wheat": { return new int[] { 25, 0, 10 };  }
                case "Rye": { return new int[] { 25, 0, 10 };  }
                case "Quarry": { return new int[] { 20, 50, 15 };  }
                case "Logging Camp": { return new int[] { 20, 50, 15 };  }
                default: { return new int[] { 0, 0, 0 };  }
            }
        }
        public override string getInfo()
        {
            this.contructInfo();
            return base.getInfo();
        }
        public void contructInfo()
        {
            this.info = "";
            this.info += String.Format("Current Action: " + currentAction + "\nWOOD: {0:D5}  STONE: {1:D5}\nFOOD:  {2:D5}", requirements[0], requirements[1], requirements[2]);
        }
        public int[] getDimensions() { return this.dimensions; }
    }
    class City : Selectable
    {
        int wood = 100;
        int stone = 100;
        int food = 500;
        
        int population = 0;
        List<Unit> unitList = new List<Unit>();

        int trainingTimer = 0;
        
        public City(int[] coords, string inTag, Scene scene)
        {
            currentAction = "Nothing";
            this.parent = scene;
            this.tag = inTag;
            this.coords = coords;
            this.setSpriteRectangle(new Rectangle(0, 220, 80, 100));
            this.setLocationRectangle(new Rectangle(coords[0] * 16, coords[1] * 20, 5 * 16, 5 * 20));
            choiceTree.Add("Root", new string[] { "Build", "Free", "Train Worker" });
            choiceTree.Add("Build", new string[] { "Quarry", "Logging Camp", "Farm" });
            this.options = choiceTree["Root"];
        }
        public void update(GameTime gameTime) 
        {

            switch (currentAction)
            {
                case "Train Worker":
                    {
                        trainingTimer += gameTime.ElapsedGameTime.Milliseconds;
                        if (trainingTimer >= 5000)
                        {
                            parent.addUnit(coords[0], coords[1], "whatever"); 
                            currentAction = "Nothing";
                            trainingTimer = 0;
                        }
                        break;
                    }
            }
        }
        public string loadUnit(string type)
        {
            switch (type)
            {
                case "Food": { food--; return type; }
                case "Stone": { stone--; return type; }
                case "Wood": { wood--; return type; }
                default: { return ""; }
            }
        }
        public void addUnit(Unit inunit) { unitList.Add(inunit); population++; if (food >= 50) { food -= 50; } else { /* add code for error display */ } }
        public void feed(int hunger) { this.food -= hunger; }
        public void add(string inItem) 
        {
            switch (inItem)
            {
                case "Food": { food++; break; }
                case "Stone": { stone++; break; }
                case "Wood": { wood++; break; }
            }
        }
        public void contructInfo()
        {
            this.info = "";
            this.info += String.Format("Current Action: "+ currentAction+"\nSTONE: {0:D5}  WOOD: {1:D5}\nFOOD:  {2:D5}  POP:  {3:D5}", stone, wood, food, population);
        }
        public bool tryBuild(int[] cost)
        {
            if (cost[0] < wood && cost[1] < stone && cost[2] < food) { wood -= cost[0]; stone -= cost[1]; food -= cost[2]; return true; }
            else { return false; }
        }
        public override string getInfo()
        {
            this.contructInfo();
            return base.getInfo();
        }
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
        public virtual void checkStateChange() { }
        public void setAction(string action) { this.currentAction = action; }
        public bool hasChildren(string key) { return choiceTree.ContainsKey(key); }
        public void reset() { this.setIndex("Root");  }
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
        public bool collides(Rectangle other)
        {
            return this.locationRectangle.Intersects(other);
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
 * HAPPINESS:                               WOOD:             PICTURE/v^^\
 *[XXXXXXXXXXX]                            [XXXXXXXXXXXX]           v^^^V^V
 * MINING:                                  FARMING:                | 0 0 |
 *[XXXXXXXXXXX]                            [XXXXXXXXXXXX]           \ --- /
 *                                                                   VVVVV       
*/                                                                    