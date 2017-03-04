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
        public int value = 0;
        public int visits = 0;
        public int action = 0;
        public int player = 0;
        public int depth = 0;
        public double ucb1 { get; set; } = 0d;

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

            return $"p{player}'s move: {action} Vi/Va: {visits}/{value} ucb1: {ucb1} [{new Board(bits).ToText()}] depth: {depth}";
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
