using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;
using SharpDX.Direct2D1.Effects;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using Weighted_Directed_Graph;

namespace PathfindingVisualizer
{
    using Point = System.Drawing.Point;
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        bool makeObstacle;
        bool makeNormal;

        bool followMouse;
        bool onStart;

        bool allowDiagnol;
        bool allowCrossingCorners;

        MouseState previousMouseState;

        Graph<Point> graph;
        Cell[,] cells;
        Grid grid;
        Cell start;
        Cell end;
        Cell previous;

        SpriteFont font;

        bool periodPressed;
        bool onePressed;
        bool twoPressed;
        bool threePressed;
        bool fourPressed;

        bool hideText;
        bool beginPathfiner;

        Menu currentMenu;
        Pathfinder currentPathfinder;
        Heuristic currentHeuristic;

        public void SnapCellOntoGrid()
        {
            if (onStart == true)
            {
                int cellX = (start.position.X + start.size / 2) / start.size;
                int cellY = (start.position.Y + start.size / 2) / start.size;
                cellX = Math.Clamp(cellX, 0, cells.GetLength(0) - 1);
                cellY = Math.Clamp(cellY, 0, cells.GetLength(1) - 1);

                start.position = cells[cellX, cellY].position;
                cells[cellX, cellY] = start;
            }
            else
            {
                int cellX = (end.position.X + end.size / 2) / end.size;
                int cellY = (end.position.Y + end.size / 2) / end.size;
                cellX = Math.Clamp(cellX, 0, cells.GetLength(0) - 1);
                cellY = Math.Clamp(cellY, 0, cells.GetLength(1) - 1);

                end.position = cells[cellX, cellY].position;
                cells[cellX, cellY] = end;
            }
        }


        static int TwoDToOneD(int x, int y, int width) // width = columns
            => x + y * width;
        public Graph<Point> GenerateGraph(int rows, int columns)
        {
            Point[] offsets = new Point[] { };
            if (allowDiagnol)
            {
                offsets = new Point[] { new Point(1, 0), new Point(-1, 0), new Point(0, -1), new Point(0, 1), new Point(1, 1), new Point(1, -1), new Point(-1, 1), new Point(-1, -1) };
            }
            if (!allowDiagnol)
            {
                offsets = new Point[] { new Point(1, 0), new Point(-1, 0), new Point(0, -1), new Point(0, 1) };
            }

            Dictionary<int, Vertex<Point>> graphValues = new Dictionary<int, Vertex<Point>>();
            Graph<Point> graph = new Graph<Point>();

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    int num = TwoDToOneD(x, y, columns) + 1;
                    if (graphValues.ContainsKey(num) == false)
                    {
                        var vertex = new Vertex<Point>(new Point(x, y));
                        graph.AddVertex(vertex);
                        graphValues.Add(num, vertex);
                    }

                    for (int i = 0; i < offsets.Length; i++)
                    {
                        int newX = x + offsets[i].X;
                        int newY = y + offsets[i].Y;
                        Point newPoint = new Point(newX, newY);
                        if (newPoint.X < 0 || newPoint.X >= columns || newPoint.Y < 0 || newPoint.Y >= rows)
                        {
                            continue;
                        }

                        int neighborValue = TwoDToOneD(newX, newY, columns) + 1;
                        if (!graphValues.ContainsKey(neighborValue))
                        {
                            var vertex = new Vertex<Point>(new Point(newX, newY));
                            graph.AddVertex(vertex);
                            graphValues.Add(neighborValue, vertex);
                        }

                        float distance = (float)Math.Sqrt(Math.Pow(newX - x, 2) + Math.Pow(newY - y, 2));
                        graph.AddEdge(graphValues[num], graphValues[neighborValue], distance);
                    }
                }
            }

            return graph;
        }

        public void StartDraggingStartCell()
        {
            cells[start.position.X / 50, start.position.Y / 50] = new Cell(new Point(start.position.X, start.position.Y), grid.tileSize, Color.White);
            onStart = true;
            followMouse = true;
        }

        public void StartDraggingEndCell()
        {
            cells[end.position.X / 50, end.position.Y / 50] = new Cell(new Point(end.position.X, end.position.Y), grid.tileSize, Color.White);
            onStart = false;
            followMouse = true;
        }

        public void StartMakingObstacleCells()
        {
            makeObstacle = true;
        }

        public void StartMakingNormalCells()
        {
            makeNormal = true;
        }

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 500;
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("Font");
            hideText = false;
            beginPathfiner = false;

            graph = new Graph<Point>();

            periodPressed = false;
            onePressed = false;
            twoPressed = false;
            threePressed = false;
            fourPressed = false;

            currentMenu = Menu.MinimizedOptions;
            currentPathfinder = Pathfinder.None;
            currentHeuristic = Heuristic.None;

            followMouse = false;

            allowCrossingCorners = false;
            allowDiagnol = false;

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            grid = new Grid(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 50);
            cells = new Cell[16, 10];

            int x = 0;
            int y = 0;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    if (x == grid.tileSize * 7 && y == grid.tileSize * 4)
                    {
                        start = new Cell(new Point(x, y), grid.tileSize, Color.LimeGreen);
                        cells[j, i] = start;
                    }
                    else if (x == grid.tileSize * 8 && y == grid.tileSize * 4)
                    {
                        end = new Cell(new Point(x, y), grid.tileSize, Color.IndianRed);
                        cells[j, i] = end;
                    }
                    else
                    {
                        cells[j, i] = new Cell(new Point(x, y), grid.tileSize, Color.White);
                    }
                    x += grid.tileSize;
                }
                y += grid.tileSize;
                x = 0;
            }
        }





        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (!beginPathfiner)
            {
                MouseState mouseState = Mouse.GetState();
                KeyboardState keyboardState = Keyboard.GetState();


                //keeping start and end cells inside the window
                if (start.position.X < 0 - start.size / 2 || start.position.X + start.size > grid.width || start.position.Y < 0 - start.size / 2 || start.position.Y + start.size > grid.height || end.position.X < 0 - end.size / 2 || end.position.X + end.size / 2 > grid.width || end.position.Y < 0 - end.size / 2 || end.position.Y + end.size / 2 > grid.height)
                {
                    followMouse = false;
                    SnapCellOntoGrid();
                }


                //checks on which type cell was clicked on
                if (!hideText && previousMouseState.LeftButton == ButtonState.Released && mouseState.LeftButton == ButtonState.Pressed && !followMouse)
                {
                    Point position = new Point(mouseState.Position.X, mouseState.Position.Y);

                    if (position.X >= start.position.X && position.X <= start.position.X + start.size && position.Y <= start.position.Y + start.size && position.Y >= start.position.Y && makeObstacle == false && makeNormal == false)
                    {
                        StartDraggingStartCell();
                    }
                    else if (position.X >= end.position.X && position.X <= end.position.X + end.size && position.Y <= end.position.Y + end.size && position.Y >= end.position.Y && makeObstacle == false && makeNormal == false)
                    {
                        StartDraggingEndCell();
                    }
                    else if (position.X > 0 && position.X < grid.width && position.Y > 0 && position.Y < grid.height)
                    {
                        int cellX = position.X / 50;
                        int cellY = position.Y / 50;

                        if (makeObstacle == false && makeNormal == false && cells[cellX, cellY].color == Color.White)
                        {
                            StartMakingObstacleCells();
                        }
                        else if (makeNormal == false && makeObstacle == false && cells[cellX, cellY].color == Color.LightGray)
                        {
                            StartMakingNormalCells();
                        }
                    }
                }



                //initiates when StartDraggingStartCell or StartDraggingEndCell happens
                //drags the cell
                if (!hideText && followMouse)
                {
                    Point position = new Point(mouseState.Position.X, mouseState.Position.Y);

                    int cellX = position.X / 50;
                    int cellY = position.Y / 50;
                    cellX = Math.Clamp(cellX, 0, cells.GetLength(0) - 1);
                    cellY = Math.Clamp(cellY, 0, cells.GetLength(1) - 1);
                    if (cells[cellX, cellY].color == Color.White)
                    {
                        previous = cells[cellX, cellY];
                    }

                    if (onStart == true)
                    {
                        start.position.X = mouseState.Position.X - start.size / 2;
                        start.position.Y = mouseState.Position.Y - start.size / 2;
                    }
                    else
                    {
                        end.position.X = mouseState.Position.X - end.size / 2;
                        end.position.Y = mouseState.Position.Y - end.size / 2;
                    }
                }



                //initiates when StartDraggingStartCell or StartDraggingEndCell happens
                //turns cells mouse is over into obstacles
                if (makeObstacle)
                {
                    Point position = new Point(mouseState.Position.X, mouseState.Position.Y);

                    int cellX = position.X / 50;
                    int cellY = position.Y / 50;

                    if (position.X > 0 && position.X < grid.width && position.Y > 0 && position.Y < grid.height && cells[cellX, cellY].color == Color.White)
                    {
                        cells[cellX, cellY].color = Color.LightGray;
                    }

                }

                //turns cells mouse is over into normal
                if (makeNormal)
                {
                    Point position = new Point(mouseState.Position.X, mouseState.Position.Y);

                    int cellX = position.X / 50;
                    int cellY = position.Y / 50;

                    if (position.X > 0 && position.X < grid.width && position.Y > 0 && position.Y < grid.height && cells[cellX, cellY].color == Color.LightGray)
                    {
                        cells[cellX, cellY].color = Color.White;
                    }
                }


                //logic for after clicking
                if (previousMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released && mouseState.Position.X > 0 && mouseState.Position.X < grid.width && mouseState.Position.Y > 0 && mouseState.Position.Y < grid.height)
                {
                    foreach (var c in cells)
                    {
                        if (c != start && c != end && c.color != Color.LightGray)
                        {
                            c.color = Color.White;
                        }
                    }

                    followMouse = false;
                    makeObstacle = false;
                    makeNormal = false;

                    Point position = new Point(mouseState.Position.X, mouseState.Position.Y);

                    int cellX = position.X / 50;
                    int cellY = position.Y / 50;

                    if (cells[cellX, cellY].color == Color.White)
                    {
                        SnapCellOntoGrid();
                    }
                    if (cells[cellX, cellY].color == Color.LightGray)
                    {
                        if (previous != null && previous.color != Color.LightGray && onStart == true)
                        {
                            start.position = previous.position;
                            previous = start; // 
                        }
                        else if (previous != null && previous.color != Color.LightGray && onStart == false)
                        {
                            end.position = previous.position;
                            previous = end; //
                        }
                    }

                }
                previousMouseState = mouseState;




                //finds which key is pressed
                if (!hideText && keyboardState.GetPressedKeyCount() > 0)
                {
                    if (periodPressed == false && keyboardState.IsKeyDown(Keys.OemPeriod))
                    {
                        periodPressed = true;
                    }

                    if (keyboardState.IsKeyDown(Keys.D1))
                    {
                        onePressed = true;
                    }

                    if (keyboardState.IsKeyDown(Keys.D2))
                    {
                        twoPressed = true;
                    }

                    if (keyboardState.IsKeyDown(Keys.D3))
                    {
                        threePressed = true;
                    }

                    if (keyboardState.IsKeyDown(Keys.D4))
                    {
                        fourPressed = true;
                    }
                }



                //does the key's job
                if (periodPressed && keyboardState.IsKeyUp(Keys.OemPeriod))
                {
                    if (currentMenu != Menu.StartOptions)
                    {
                        currentMenu = Menu.StartOptions;
                        currentPathfinder = Pathfinder.None;
                    }
                    else
                    {
                        currentMenu = Menu.MinimizedOptions;
                    }


                    periodPressed = false;
                }

                if (onePressed && keyboardState.IsKeyUp(Keys.D1))
                {
                    if (currentMenu == Menu.StartOptions || currentMenu == Menu.MinimizedOptions)
                    {
                        currentMenu = Menu.AStarOptions;

                        currentPathfinder = Pathfinder.AStar;
                    }
                    else if (currentMenu == Menu.AStarOptions)
                    {
                        currentMenu = Menu.PathFindingOptions;

                        currentHeuristic = Heuristic.Manhattan;
                    }
                    else if (currentMenu == Menu.PathFindingOptions)
                    {
                        if (!allowDiagnol)
                        {
                            allowDiagnol = true;
                        }
                        else
                        {
                            allowDiagnol = false;
                        }
                    }
                    onePressed = false;
                }

                if (twoPressed && keyboardState.IsKeyUp(Keys.D2))
                {
                    if (currentMenu == Menu.StartOptions || currentMenu == Menu.MinimizedOptions)
                    {
                        currentMenu = Menu.PathFindingOptions;
                        currentPathfinder = Pathfinder.Dijsktra;
                    }
                    else if (currentMenu == Menu.AStarOptions)
                    {
                        currentMenu = Menu.PathFindingOptions;

                        currentHeuristic = Heuristic.Euclidan;
                    }
                    else if (currentMenu == Menu.PathFindingOptions)
                    {
                        if (!allowCrossingCorners)
                        {
                            allowCrossingCorners = true;
                        }
                        else
                        {
                            allowCrossingCorners = false;
                        }
                    }
                    twoPressed = false;
                }

                if (threePressed && keyboardState.IsKeyUp(Keys.D3))
                {
                    if (currentMenu == Menu.StartOptions || currentMenu == Menu.MinimizedOptions)
                    {
                        currentMenu = Menu.PathFindingOptions;
                        currentPathfinder = Pathfinder.BFS;
                    }
                    else if (currentMenu == Menu.AStarOptions)
                    {
                        currentMenu = Menu.PathFindingOptions;

                        currentHeuristic = Heuristic.Octile;
                    }
                    else if (currentMenu == Menu.PathFindingOptions)
                    {
                        hideText = true;
                        currentMenu = Menu.MinimizedOptions;

                        beginPathfiner = true;
                    }
                    threePressed = false;
                }

                if (fourPressed && keyboardState.IsKeyUp(Keys.D4))
                {
                    if (currentMenu == Menu.AStarOptions)
                    {
                        currentMenu = Menu.PathFindingOptions;

                        currentHeuristic = Heuristic.Chebyshev;
                    }
                    fourPressed = false;
                }
            }
            else
            {
                //visualizes pathfinding

                foreach (var c in cells)
                {
                    if (c != start && c != end && c.color != Color.LightGray)
                    {
                        c.color = Color.White;
                    }
                }

                Heap<Vertex<Point>> priorityQueue = null;

                var graph = GenerateGraph(10, 16);
                var startingPoint = graph.Search(new Point((start.position.X + start.size / 2) / start.size, (start.position.Y + start.size / 2) / start.size));
                var endingPoint = graph.Search(new Point((end.position.X + end.size / 2) / end.size, (end.position.Y + end.size / 2) / end.size));

                List<Vertex<Point>> path = new List<Vertex<Point>>();

                for (int i = 0; i < 16; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (cells[i, j].color == Color.LightGray)
                        {
                            var currentObstacle = graph.Search(new Point((cells[i, j].position.X + cells[i, j].size / 2) / cells[i, j].size, (cells[i, j].position.Y + cells[i, j].size / 2) / cells[i, j].size));

                            Vertex<Point> upOne = null;
                            Vertex<Point> downOne = null;
                            Vertex<Point> leftOne = null;
                            Vertex<Point> rightOne = null;

                            if ((j - 1) >= 0)
                            {
                                upOne = graph.Search(new Point((cells[i, j - 1].position.X + cells[i, j - 1].size / 2) / cells[i, j - 1].size, (cells[i, j - 1].position.Y + cells[i, j - 1].size / 2) / cells[i, j - 1].size));
                            }
                            if ((j + 1) < 10)
                            {
                                downOne = graph.Search(new Point((cells[i, j + 1].position.X + cells[i, j + 1].size / 2) / cells[i, j + 1].size, (cells[i, j + 1].position.Y + cells[i, j + 1].size / 2) / cells[i, j + 1].size));
                            }
                            if ((i - 1) >= 0)
                            {
                                leftOne = graph.Search(new Point((cells[i - 1, j].position.X + cells[i - 1, j].size / 2) / cells[i - 1, j].size, (cells[i - 1, j].position.Y + cells[i - 1, j].size / 2) / cells[i - 1, j].size));
                            }
                            if ((i + 1) < 16)
                            {
                                rightOne = graph.Search(new Point((cells[i + 1, j].position.X + cells[i + 1, j].size / 2) / cells[i + 1, j].size, (cells[i + 1, j].position.Y + cells[i + 1, j].size / 2) / cells[i + 1, j].size));
                            }

                            if (allowDiagnol)
                            {
                                //top left = 0     -1, -1
                                //top = 1           0, -1
                                //top right = 2    +1, -1

                                //left = 3         -1,  0
                                //right = 4        +1,  0

                                //down = 5          0, +1
                                //down right = 6   +1, +1
                                //down left = 7    -1, +1

                                if (upOne != null && leftOne != null && cells[i - 1, j - 1].color == Color.LightGray)
                                {
                                    graph.RemoveEdge(upOne, leftOne);
                                    graph.RemoveEdge(leftOne, upOne);
                                }
                                if (upOne != null && rightOne != null && cells[i + 1, j - 1].color == Color.LightGray)
                                {
                                    graph.RemoveEdge(upOne, rightOne);
                                    graph.RemoveEdge(rightOne, upOne);
                                }

                                if (!allowCrossingCorners)
                                {
                                    if(upOne != null && rightOne != null && cells[i, j - 1].color == Color.White && cells[i + 1, j].color == Color.White)
                                    {
                                        graph.RemoveEdge(upOne, rightOne);
                                        graph.RemoveEdge(rightOne, upOne);
                                    }
                                    if (upOne != null && leftOne != null && cells[i, j - 1].color == Color.White && cells[i - 1, j].color == Color.White)
                                    {
                                        graph.RemoveEdge(upOne, leftOne);
                                        graph.RemoveEdge(leftOne, upOne);
                                    }
                                    if (downOne != null && rightOne != null && cells[i, j + 1].color == Color.White && cells[i + 1, j].color == Color.White)
                                    {
                                        graph.RemoveEdge(downOne, rightOne);
                                        graph.RemoveEdge(rightOne, downOne);
                                    }
                                    if (downOne != null && leftOne != null && cells[i, j + 1].color == Color.White && cells[i - 1, j].color == Color.White)
                                    {
                                        graph.RemoveEdge(downOne, leftOne);
                                        graph.RemoveEdge(leftOne, downOne);
                                    }
                                }
                            }

                            graph.RemoveVertex(currentObstacle);
                        }
                    }
                }
                ;
                if (currentPathfinder == Pathfinder.AStar)
                {
                    if (currentHeuristic == Heuristic.Manhattan)
                    {
                        path = graph.AStar(startingPoint, endingPoint, Heuristics.Manhattan, out priorityQueue);
                    }
                    else if (currentHeuristic == Heuristic.Euclidan)
                    {
                        path = graph.AStar(startingPoint, endingPoint, Heuristics.Euclidean, out priorityQueue);
                    }
                    else if (currentHeuristic == Heuristic.Octile)
                    {
                        path = graph.AStar(startingPoint, endingPoint, Heuristics.Octile, out priorityQueue);
                    }
                    else if (currentHeuristic == Heuristic.Chebyshev)
                    {
                        path = graph.AStar(startingPoint, endingPoint, Heuristics.Chebyshev, out priorityQueue);
                    }

                }
                else if (currentPathfinder == Pathfinder.Dijsktra)
                {
                    path = graph.Dijkstra(startingPoint, endingPoint, out priorityQueue);
                }
                else if (currentPathfinder == Pathfinder.BFS)
                {
                    path = graph.BFS(startingPoint, endingPoint);
                }

                for (int i = 1; i < path.Count - 1; i++)
                {
                    cells[path[i].Value.X, path[i].Value.Y].color = Color.Yellow;                  
                }      
                foreach (var v in graph.Vertices)
                {
                    if (v.visited && cells[v.Value.X, v.Value.Y].color != Color.Yellow
                        && cells[v.Value.X, v.Value.Y].color != Color.LimeGreen
                        && cells[v.Value.X, v.Value.Y].color != Color.IndianRed)
                    {
                        cells[v.Value.X, v.Value.Y].color = Color.CadetBlue;
                    }
                }
                if (currentPathfinder != Pathfinder.BFS)
                {
                    for (int i = 0; i < priorityQueue.count; i++)
                    {
                        if (cells[priorityQueue.items[i].Value.X, priorityQueue.items[i].Value.Y].color != Color.Yellow
                            && cells[priorityQueue.items[i].Value.X, priorityQueue.items[i].Value.Y].color != Color.LimeGreen
                            && cells[priorityQueue.items[i].Value.X, priorityQueue.items[i].Value.Y].color != Color.IndianRed
                            && cells[priorityQueue.items[i].Value.X, priorityQueue.items[i].Value.Y].color != Color.CadetBlue)
                        {
                            cells[priorityQueue.items[i].Value.X, priorityQueue.items[i].Value.Y].color = Color.LightBlue;
                        }
                    }
                }

                currentPathfinder = Pathfinder.None;
                currentHeuristic = Heuristic.None;
                hideText = false;
                beginPathfiner = false;
            }




            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            _spriteBatch.Begin();

            //Cell

            foreach (var cell in cells)
            {
                if (cell != start || cell != end)
                {
                    _spriteBatch.FillRectangle(new RectangleF(cell.position.X, cell.position.Y, cell.size, cell.size), cell.color);
                }
            }

            _spriteBatch.FillRectangle(new RectangleF(start.position.X, start.position.Y, start.size, start.size), start.color);
            _spriteBatch.FillRectangle(new RectangleF(end.position.X, end.position.Y, end.size, end.size), end.color);




            //Grid

            int x = 0;
            while (x <= grid.width)
            {
                _spriteBatch.DrawLine(new Vector2(x, 0), new Vector2(x, grid.height), Color.Gray, 1.0f, 1.0f);
                x += grid.tileSize;
            }
            int y = 0;
            while (y <= grid.height)
            {
                _spriteBatch.DrawLine(new Vector2(0, y), new Vector2(grid.width, y), Color.Gray, 1.0f, 1.0f);
                y += grid.tileSize;
            }




            //Options

            if (hideText == false)
            {
                if (currentMenu == Menu.AStarOptions)
                {
                    _spriteBatch.DrawString(font, "Press '.' to go back.", new Vector2(10, 10), Color.Black);

                    _spriteBatch.DrawString(font, "Press '1' for Manhattan.", new Vector2(10, 32), Color.Black);
                    _spriteBatch.DrawString(font, "Press '2' for Euclidan.", new Vector2(10, 50), Color.Black);
                    _spriteBatch.DrawString(font, "Press '3' for Octile.", new Vector2(10, 68), Color.Black);
                    _spriteBatch.DrawString(font, "Press '4' for Chebyshev.", new Vector2(10, 86), Color.Black);
                }
                if (currentMenu == Menu.PathFindingOptions)
                {
                    _spriteBatch.DrawString(font, "Press '.' to go back.", new Vector2(10, 10), Color.Black);

                    if (!allowDiagnol)
                    {
                        _spriteBatch.DrawString(font, "Press '1' to enable diagnol.", new Vector2(10, 32), Color.Black);
                    }
                    else
                    {
                        _spriteBatch.DrawString(font, "Press '1' to disable diagnol.", new Vector2(10, 32), Color.Black);
                    }
                    if (!allowCrossingCorners)
                    {
                        _spriteBatch.DrawString(font, "Press '2' to enable crossing corners.", new Vector2(10, 50), Color.Black);
                    }
                    else
                    {
                        _spriteBatch.DrawString(font, "Press '2' to disable crossing corners.", new Vector2(10, 50), Color.Black);

                    }
                    _spriteBatch.DrawString(font, "Press '3' to start visualiztaion.", new Vector2(10, 74), Color.Black);
                }
                if (currentMenu == Menu.MinimizedOptions)
                {
                    _spriteBatch.DrawString(font, "Press '.' to expand options.", new Vector2(10, 10), Color.Black);
                }
                if (currentMenu == Menu.StartOptions)
                {
                    _spriteBatch.DrawString(font, "Press '.' to shrink options.", new Vector2(10, 10), Color.Black);

                    _spriteBatch.DrawString(font, "Press '1' to start A*.", new Vector2(10, 32), Color.Black);
                    _spriteBatch.DrawString(font, "Press '2' to start Dijsktra.", new Vector2(10, 50), Color.Black);
                    _spriteBatch.DrawString(font, "Press '3' to start BFS.", new Vector2(10, 68), Color.Black);
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}