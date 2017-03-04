using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyOthello.Engines.MCTS
{
    public class MCTSEngine : BaseEngine
    {
        private static readonly Random rand = new Random(1024);

        public override SearchResult Search(Board board, int color, int depth)
        {
            //var endGameEngine = new EndGameEngine();
            //var perfectResult = endGameEngine.Search(board, color, 12);

            var searchResult = new SearchResult();

            var clock = new Clock();
            clock.Start();

            var bm = GetBestMove(board, color);

            clock.Stop();

            searchResult.Move = bm;
            searchResult.Score = 0;
            //searchResult.Message = $"mcts move, [perfect result: {perfectResult}] ";
            searchResult.TimeSpan = clock.Elapsed;

            return searchResult;
        }

        //THE EXECUTING FUNCTION
        public int GetBestMove(Board b, int color)
        {
            var board = b.Copy();
            //Setup root and initial variables
            Node root = new Node(null, 0, color.Opp(), 0);

            root.bits = board.ToBitBoard();

            //four phases: descent, roll-out, update and growth done iteratively X times
            //-----------------------------------------------------------------------------------------------------
            for (int iteration = 0; iteration < 10000; iteration++)
            {
                Node current = Selection(root, color);
                int value = Rollout(current, color);
                Update(current, value);
            }

            root.Save($"{DateTime.Now.ToString("yyyy-MM-dd")}.json");// DrawTree();

            return BestChildUCB(root, 0).action;
        }

        //#1. Select a node if 1: we have more valid feasible moves or 2: it is terminal 
        public Node Selection(Node root, int color)
        {
            var current = root;
            var board = new Board(current.bits);
            while (!board.IsGameOver(color) )
            {
                var moves = rule.FindFlips(board, color).ToList();

                if (moves.Count == 0)
                {
                    color = color.Opp();
                    continue;
                }

                if (moves.Count > current.children.Count)
                {
                    return Expand(current, color);
                }

                var bestChild = BestChildUCB(current, 1); /*1.44*/
                if (bestChild == null)
                {
                    return bestChild;
                }

                current = bestChild;
                board = new Board(current.bits);
                color = current.player.Opp();
            }

            return current;
        }

        //#1. Helper
        public Node BestChildUCB(Node current, double C)
        {
            Node bestChild = null;
            double best = double.NegativeInfinity;

            foreach (Node child in current.children)
            {
                double UCB1 = ((double)child.value / (double)child.visits) + C * Math.Sqrt((2.0 * Math.Log((double)current.visits)) / (double)child.visits);
                child.ucb1 = UCB1;

                if (UCB1 > best)
                {
                    bestChild = child;
                    best = UCB1;
                }
            }

            return bestChild;
        }

        //#2. Expand a node by creating a new move and returning the node
        private Node Expand(Node current, int color)
        {
            var board = new Board(current.bits);
            var moves = rule.FindFlips(board, color).ToList();

            for (int i = 0; i < moves.Count; i++)
            {
                var move = moves[i].Pos;
                //We already have evaluated this move
                if (current.children.Exists(a => a.action == move))
                    continue;

                var opp = color.Opp();

                Node node = new Node(current, move, color, current.depth + 1);
                current.children.Add(node);

                //Do the move in the game and save it to the child node
                var fc = board.MakeMove(move, color);

                node.bits = board.ToBitBoard();

                //Can be removed
                board.Reback(move, fc, opp);

                return node;
            }

            throw new Exception("Error");
        }

        //#3. Roll-out. Simulate a game with a given policy and return the value
        public int Rollout(Node current, int startColor)
        {
            var board = new Board(current.bits);
            var opp = startColor.Opp();
            //If this move is terminal and the opponent wins, this means we have previously made a move where the opponent can always find a move to win.. not good
            if (board.IsGameOver(opp))
            {
                if (board.Diff(startColor) < 0)
                {
                    current.parent.value = -999999;
                    return 0;
                }
                else
                {
                    return 1;
                }
            }

            var player = opp; //current.player.Opp();

            //Do the policy until a winner is found for the first (change?) node added
            while (!board.IsGameOver(player))
            {
                //Random
                var moves = rule.FindFlips(board, player).ToList();

                if (moves.Count == 0)
                {
                    player = player.Opp();
                    continue;
                }
                var move = moves[rand.Next(0, moves.Count)];
                var fc = board.MakeMove(move.Pos, player);
                player = player.Opp();
            }

            if (board.Diff(startColor) >= 0)
            {
                return 1;
            }

            return 0;

        }

        //#4. Update
        public void Update(Node current, int value)
        {
            do
            {
                current.visits++;
                current.value += value;
                current = current.parent;
            }
            while (current != null);
        }


    }
}
