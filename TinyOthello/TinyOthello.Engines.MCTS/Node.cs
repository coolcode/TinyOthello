using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace TinyOthello.Engines.MCTS
{
    public class Node
    {
        [JsonIgnore]
        public Node parent = null;
        public List<Node> children = new List<Node>();
        public int[] values = new int[2];
        public int visits = 0;
        public int action = 0;
        public int player = 0;
        public int depth = 0;
        public double score { get; set; }

        private double _ucb1 = 0d;
        private const double C = 1.0d;

        public double ucb1
        {
            get
            {
                if (visits == 0)
                {
                    return 0d;
                }

                if (parent == null)
                {
                    _ucb1 = values[player-1] / (double)visits;
                }
                else
                {
                    var i = player-1;
                    //find root
                    var current = parent;
                    //while (current.parent != null)
                    //{
                    //    current = current.parent;
                    //}

                    _ucb1 = (values[i] / (double)visits) + C * Math.Sqrt((2.0 * Math.Log(current.visits)) / visits);
                }
                return _ucb1;
            }
            set
            {
                _ucb1 = value;
            }
        }

        public string seq
        {
            get
            {
                var posStack = new Stack<int>();
                var current = this;
                do
                {
                    posStack.Push(current.action);
                    current = current.parent;
                } while (current != null);

                var text = string.Join("->", posStack);

                return text;
            }
        }

        public double winrate
        {
            get { return values[player-1] / (double)visits; }
        }

        public long bits { get; set; } = 0x0;

        public Node(Node parent, int action, int player, int depth)
        {
            this.parent = parent;
            this.action = action;
            this.player = player;
            this.depth = depth;
        }

        public void Add(Node item)
        {
            children.Add(item);
        }

        public Node FindBestNode()
        {
            var bestWinRate = double.MinValue;
            Node bestNode = null;
            foreach (var node in children)
            {
                if (node.winrate > bestWinRate)
                {
                    bestWinRate = node.winrate;
                    bestNode = node;
                }
            }

            return bestNode;
        }

        public static Node Load(string path)
        {
            var text = File.ReadAllText(path, Encoding.UTF8);
            var node = JsonConvert.DeserializeObject<Node>(text);
            Visit(node, (current, child) => child.parent = current);

            return node;
        }

        public Node Search(int b)
        {
            return Search(this, b);
        }

        private Node Search(Node current, int b)
        {
            if (current.bits == b)
            {
                return current;
            }

            foreach (var child in current.children)
            {
                var node = Search(child, b);

                if (node != null)
                {
                    return node;
                }
            }

            return null;
        }

        public void Visit(Action<Node, Node> action)
        {
            Visit(this, action);
        }

        private static void Visit(Node current, Action<Node, Node> action)
        {
            foreach (var child in current.children)
            {
                action(current, child);
                Visit(child, action);
            }
        }

        public void Save(string path)
        {
            var text = JsonConvert.SerializeObject(this);
            File.WriteAllText(path, text, Encoding.UTF8);
        }

        public override string ToString()
        {
            if (parent == null)
            {
                return "Root Node";
            }

            return $"{seq} p{player}'s move: {action} Vi/Va[b]: {visits}/{values[0]} Vi/Va[w]: {visits}/{values[1]} ucb1: {ucb1.ToString("f4")}, win%: {winrate.ToString("p2")}, score: {score} [{new Board(bits).ToText()}] depth: {depth}";
        }

        public string DrawTree()
        {
            return DrawTree("", true);
        }

        private string DrawTree(string indent, bool last)
        {
            var sb = new StringBuilder();

            sb.Append(indent);
            if (last)
            {
                sb.Append("\\-");
                indent += "  ";
            }
            else
            {
                sb.Append("|-");
                indent += "| ";
            }
            sb.AppendLine(ToString());

            for (int i = 0; i < children.Count; i++)
            {
                var childText = children[i].DrawTree(indent, i == children.Count - 1);
                sb.Append(childText);
            }

            return sb.ToString();
        }
    }
}
