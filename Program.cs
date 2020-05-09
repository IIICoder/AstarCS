using System;
using System.Collections.Generic;
using System.Linq;

namespace AstarCS
{
    class Program
    {
        static void Main(string[] args)
        {
            //NOTE: Use case
            int[,] grid = {
                {1,1,1,2,1,1,1,1,1,1,1},
                {1,1,1,2,1,1,1,1,1,1,1},
                {1,1,1,2,1,1,1,1,1,1,1},
                {1,1,2,2,2,2,1,1,1,1,1},
                {1,1,0,2,2,2,1,1,1,1,1},
                {1,1,0,1,1,1,1,1,1,1,1},
                {1,1,0,0,0,0,1,1,1,1,1},
                {0,1,1,0,0,0,1,1,1,1,1}
                };
            grid.PrintToConsole();

            var graph = new Graph(grid);

            Node start = graph.grid[2, 7];
            Node end = graph.grid[7, 7];
            Console.WriteLine($"Start: [{start.x}, {start.y}], weight: {start.weight}");
            Console.WriteLine($"End: [{end.x}, {end.y}], weight: {end.weight}");
            Stack<Node> path = search(graph, start, end, false);
            Console.WriteLine("\n~PATH POINTS~");
            while (path.Count > 0)
            {
                Console.WriteLine(path.Pop());
            }
        }
        ///<summary>Возвращает список узлов, которые являются оптимальным вариантом прокладывания пути</summary>
        public static Stack<Node> search(Graph graph, Node start, Node end, bool diagonals)
        {
            graph.cleanDirty();
            bool closest = false;
            Heuristic heuristic = new Heuristic();
            BinaryHeap openHeap = new BinaryHeap();
            Node closestNode = start;
            if (diagonals)
            {
                start.h = heuristic.diagonals(start, end);
            }
            else
            {
                start.h = heuristic.manhattan(start, end);
            }
            graph.markDirty(start);
            openHeap.add(start);
            while (openHeap.heapSize > 0)
            {
                Node currentNode = openHeap.getMin();
                if (currentNode == end)
                {
                    return pathTo(currentNode);
                }
                currentNode.closed = true;
                List<Node> neighbors = graph.neighbors(currentNode, diagonals);
                for (int i = 0, il = neighbors.Count; i < il; i++)
                {
                    Node neighbor = neighbors[i];
                    if (neighbor.closed || neighbor.isWall())
                    {
                        continue;
                    }
                    double gScore = currentNode.g + neighbor.getCost(currentNode, diagonals);
                    bool beenVisited = neighbor.visited;
                    if (!beenVisited || gScore < neighbor.g)
                    {
                        neighbor.visited = true;
                        neighbor.parent = currentNode;
                        if (diagonals)
                        {
                            neighbor.h = neighbor.h != 0 ? neighbor.h : heuristic.diagonals(neighbor, end);
                        }
                        else
                        {
                            neighbor.h = neighbor.h != 0 ? neighbor.h : heuristic.manhattan(neighbor, end);
                        }
                        neighbor.g = gScore;
                        neighbor.f = neighbor.g + neighbor.h;
                        graph.markDirty(neighbor);
                        if (closest)
                        {
                            if (neighbor.h < closestNode.h || (neighbor.h == closestNode.h && neighbor.g < closestNode.g))
                            {
                                closestNode = neighbor;
                            }
                        }
                        if (!beenVisited)
                        {
                            openHeap.add(neighbor);
                        }
                    }
                }
            }
            if (closest)
            {
                return pathTo(closestNode);
            }
            return new Stack<Node>();
        }

        ///<summary>Функция, восстаноавливающая путь по "хлебным крошкам" - через родителей узлов</summary>
        public static Stack<Node> pathTo(Node currentNode)
        {
            Node curr = currentNode;
            Stack<Node> path = new Stack<Node>();
            while (curr.parent != null)
            {
                path.Push(curr);
                curr = curr.parent;
            }
            return path;
        }
    }

    public class Heuristic
    {
        ///<summary>Эвристическая функция для прокладывания пути только горизонтально и вертикально</summary>
        public double manhattan(Node pos0, Node pos1)
        {
            double d1 = Math.Abs(pos1.x - pos0.x);
            double d2 = Math.Abs(pos1.y - pos0.y);
            return d1 + d2;
        }
        ///<summary>Эвристическая функция для прогладывания пути с учетом диагоналей</summary>
        public double diagonals(Node pos0, Node pos1)
        {
            int D = 1;
            double D2 = Math.Sqrt(2);
            double d1 = Math.Abs(pos1.x - pos0.x);
            double d2 = Math.Abs(pos1.y - pos0.y);
            return (D * (d1 + d2)) + ((D2 - (2 * D)) * Math.Min(d1, d2));
        }
    }

    public class Graph
    {
        private Stack<Node> dirtyNodes;
        public Node[,] grid;
        public bool diagonals;
        ///<summary>Инициализатор графа</summary>
        public Graph(int[,] gridln, bool diagonals = false)
        {
            this.diagonals = diagonals;
            this.dirtyNodes = new Stack<Node>();
            this.grid = new Node[gridln.GetLength(1), gridln.GetLength(0)];
            for (int r = 0, rlen = gridln.GetLength(0); r < rlen; r++)
            {
                for (int c = 0, clen = gridln.GetLength(1); c < clen; c++)
                {
                    Node node = new Node(c, r);
                    node.weight = gridln[r, c];
                    cleanNode(node);
                    this.grid[c, r] = node;
                }
            }
        }
        ///<summary>Сбрасывает элементы dirtyNodes</summary>
        public void cleanDirty()
        {
            foreach (Node node in this.dirtyNodes)
            {
                cleanNode(node);
            }
            dirtyNodes = null;
            dirtyNodes = new Stack<Node>();
        }
        ///<summary>Обнуляет значения Node</summary>
        public void cleanNode(Node node)
        {
            node.f = 0;
            node.g = 0;
            node.h = 0;
            node.visited = false;
            node.closed = false;
            node.parent = null;
        }
        ///<summary>Пушит node в dirtyNodes</summary>
        public void markDirty(Node node)
        {
            this.dirtyNodes.Push(node);
        }
        ///<summary>Выводит в консоль матрицу</summary>
        public void PrintToConsole()
        {
            Console.WriteLine("~ Graph ~");
            for (int c = 0; c < grid.GetLength(1); c++)
            {
                for (int r = 0; r < grid.GetLength(0); r++)
                {
                    Console.Write($"{grid[r, c].weight} ");
                }
                Console.WriteLine();
            }
        }
        ///<summary>Проверяет, является ли ячейка с координатами [x,y] частью матрицы</summary>
        public bool isPartOfGrid(int x, int y)
        {
            if (x >= this.grid.GetLength(0) || x < 0)
            {
                return false;
            }
            if (y >= this.grid.GetLength(1) || y < 0)
            {
                return false;
            }
            return true;
        }
        ///<summary>Находит и возвращает потомков текущего узла</summary>
        public List<Node> neighbors(Node node, bool diagonals)
        {
            List<Node> neighborsList = new List<Node>();
            int x = node.x;
            int y = node.y;
            Node[,] graph = this.grid;
            // West
            if (isPartOfGrid(x - 1, y))
            {
                neighborsList.Add(graph[x - 1, y]);
            }

            // East
            if (isPartOfGrid(x + 1, y))
            {
                neighborsList.Add(graph[x + 1, y]);
            }

            // South
            if (isPartOfGrid(x, y - 1))
            {
                neighborsList.Add(graph[x, y - 1]);
            }

            // North
            if (isPartOfGrid(x, y + 1))
            {
                neighborsList.Add(graph[x, y + 1]);
            }

            if (diagonals)
            {

                // Southwest
                if (isPartOfGrid(x - 1, y - 1))
                {
                    neighborsList.Add(graph[x - 1, y - 1]);
                }

                // Southeast
                if (isPartOfGrid(x + 1, y - 1))
                {
                    neighborsList.Add(graph[x + 1, y - 1]);
                }

                // Northwest
                if (isPartOfGrid(x - 1, y - 1))
                {
                    neighborsList.Add(graph[x - 1, y - 1]);
                }

                // Northeast
                if (isPartOfGrid(x + 1, y - 1))
                {
                    neighborsList.Add(graph[x + 1, y - 1]);
                }

            }
            return neighborsList;
        }
    }

    public class Node
    {
        public Node(int x, int y, int f = 0)
        {
            this.x = x;
            this.y = y;
            this.f = f;
            this.g = 0;
            this.h = 0;
            this.weight = 1;
            this.visited = false;
            this.closed = false;
        }
        public int x { get; }
        public int y { get; }

        public double f;
        public double g;
        public double h;
        public double weight;
        public bool visited;
        public bool closed;
        public Node parent;

        public bool isWall()
        {
            return this.weight == 0;
        }
        public double getCost(Node fromNeighbor, bool diagonals)
        {
            if (diagonals)
            {
                if (fromNeighbor.x != this.x && fromNeighbor.y != this.y)
                {
                    return this.weight * 1.41421;
                }
            }
            return this.weight;
        }
        public override string ToString()
        {
            return $"NODE => Point({x}, {y}), F={f}, G={g}, H={h}, Cost={weight}, Visited={visited}, Closed={closed}.";
        }
    }

    public static class GridExtends
    {
        ///<summary>Выводит двумерный массив в терминал</summary>
        public static void PrintToConsole(this int[,] arr)
        {
            Console.WriteLine("~ GRID ~");
            Console.Write("  ");
            for (int c = 0; c < arr.GetLength(1); c++)
            {
                Console.Write($"{c}|");
            }
            Console.WriteLine();
            for (int r = 0; r < arr.GetLength(0); r++)
            {
                Console.Write($"{r}|");
                for (int c = 0; c < arr.GetLength(1); c++)
                {
                    Console.Write($"{arr[r, c]} ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }

    ///<summary>Класс, реализующий работу с бинарной кучей</summary>
    public class BinaryHeap
    {
        private List<Node> list = new List<Node>();
        ///<summary>Возвращает размер бинарной кучи</summary>
        public int heapSize
        {
            get
            {
                return this.list.Count;
            }
        }
        ///<summary>Добавляет новый элемент в бинарную кучу</summary>
        public void add(Node value)
        {
            list.Add(value);
            int i = heapSize - 1;
            int parent = (i - 1) / 2;

            while (i > 0 && list[parent].f > list[i].f)
            {
                Node temp = list[i];
                list[i] = list[parent];
                list[parent] = temp;

                i = parent;
                parent = (i - 1) / 2;
            }
        }
        ///<summary>Сортирует элементы бинарной кучи</summary>
        public void heapify(int i)
        {
            int leftChild;
            int rightChild;
            int largestChild;

            for (; ; )
            {
                leftChild = 2 * i + 1;
                rightChild = 2 * i + 2;
                largestChild = i;

                if (rightChild < heapSize && list[rightChild].f < list[largestChild].f)
                {
                    largestChild = rightChild;
                }

                if (leftChild < heapSize && list[leftChild].f < list[largestChild].f)
                {
                    largestChild = leftChild;
                }

                if (largestChild == i)
                {
                    break;
                }

                Node temp = list[i];
                list[i] = list[largestChild];
                list[largestChild] = temp;
                i = largestChild;
            }
        }
        ///<summary>Создает бинарную кучу из массива и сортирует ее</summary>
        public void buildHeap(Node[] sourceArray)
        {
            list = sourceArray.ToList();
            for (int i = heapSize / 2; i >= 0; i--)
            {
                heapify(i);
            }
        }
        ///<summary>Возврящает максимальное значение из кучи и удаляет его оттуда</summary>
        public Node getMin()
        {
            if (this.list.Count <= 0)
            {
                Console.WriteLine("ERROR: Куча пуста");
            }
            Node result = list[0];
            list[0] = list[heapSize - 1];
            list.RemoveAt(heapSize - 1);
            heapify(0);
            return result;
        }
        ///<summary>Выводит в консоль двоичную кучу</summary>
        public void PrintToConsole()
        {
            Console.WriteLine("[");
            for (int i = 0; i < this.list.Count; i++)
            {
                Console.WriteLine($"{list[i]} ");
            }
            Console.WriteLine("]");

        }
    }
}
