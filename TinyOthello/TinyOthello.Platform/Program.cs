using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TinyOthello.Engines;
using TinyOthello.Learning;

namespace TinyOthello.Platform {
	class Program {
		static void Main(string[] args) {
			//BoardTest();
			GameTest();
			//Engine_Test();
			//FightTest();
			//Board_DefaultCtro_Test();
			//Engine_Test();
			//GenKnowledge_Test();
			//NeuralLearning_Test();
			//LoadNetwork_Test();
			//CalDelta_Test();
			//NeuralEngine_Test("□○●□□○●□□○●□□□□□",13);

			Console.WriteLine("press any keys to exit...");
			Console.Read();
		}

		private static void CalDelta_Test() {
			var know = new NeuralLearning();
			know.CalDelta();
		}

		private static void LoadNetwork_Test() {
			var know = new NeuralLearning();
			know.LoadNetwork();
		}

		private static void NeuralLearning_Test() {
			var know = new NeuralLearning();
			know.Learn();
		}

		private static void GenKnowledge_Test() {
			var know = new Knowledge(1000000);
			know.Generate();
		}
		 
		private static void Engine_Test() {
			var boardTexts = new string[] { 
				"□○○○□●●●●●○□□□○□",
				"□○○○□●○○●●●○□□○●",
				"○○●○○●○○○○○○○●□□",
				"□○○○□●○□●●●□□□□□",
 				"●●●○□●●○□○○○□□□□"
			};
			var bestMoves = new int[] {
				15,
				13,
			    14,
				7,
				0
			};

			for (int i = 0; i < boardTexts.Length; i++) {
				EndGame_Test(boardTexts[i], bestMoves[i]);
			}
		}

		static void FightTest() {
			var game = new Game();
			var engines = new IEngine[] { new EndGameEngine(), new EndGameEngine() }; //new CoolEngine(),new RandomEngine("Bar")
			var boardText = "□○○○○●●●●○●□□□○●";//"□○○○□●●●●●○□□□○□";//"□○○○□●○○●●●○□□○●";
			var targetPath = Path.Combine(Environment.CurrentDirectory, "fight-results");
			game.Fight(engines[0], engines[1], targetPath,new Board(boardText), StoneType.White);
		}

		static void GameTest() {
			IGame game = new Game();
			var engines = new IEngine[] { new EndGameEngine(), new NeuralEngine("4-6-2012-04-24.net"), new RandomEngine("Foo") }; // new RandomEngine("Bar")
			game.Run(engines);
		}

		static void BoardTest() {
			Board borad = new Board();
			borad[2, 2].Type = StoneType.Black;
			borad[2, 3].Type = StoneType.White;
			borad[3, 2].Type = StoneType.White;
			borad[3, 3].Type = StoneType.Black;
			//borad[3, 4].Type = StoneType.Dummy;
			Console.WriteLine(borad);
		}

		static void Board_DefaultCtro_Test() {
			Board borad = new Board(); 
			Console.WriteLine(borad);
		}

		static void EndGame_Test(string boardText, int bestMove) {
			IEngine engine = new EndGameEngine();
			var board = new Board(boardText);
			var searchResult = engine.Search(board, StoneType.Black, 16);
			Console.WriteLine("Best Move Act:{0},Exp:{1} | Score Act:{2}, {3}",
				searchResult.Move, bestMove, searchResult.Score/ Constants.HighestScore, searchResult.Message);

		}

		static void NeuralEngine_Test(string boardText, int bestMove) {
			IEngine engine = new EndGameEngine();
			var board = new Board(boardText);
			var searchResult = engine.Search(board.Copy(), StoneType.Black, 16);
			Console.WriteLine("EndGameEngine Best Move Act:{0},Exp:{1} | Score Act:{2}, {3}, Nodes:{4}, Times:{5}",
				searchResult.Move, bestMove, searchResult.Score/ Constants.HighestScore, searchResult.Message,
				searchResult.Nodes, searchResult.TimeSpan);

			engine = new NeuralEngine("4-6-2012-04-24.net");
			searchResult = engine.Search(board.Copy(), StoneType.Black, (Constants.StoneCount - 4) / 2);
			Console.WriteLine("NeuralEngine Best Move Act:{0},Exp:{1} | Score Act:{2}, {3}, Nodes:{4}, Times:{5}",
				searchResult.Move, bestMove, searchResult.Score, searchResult.Message,
				searchResult.Nodes, searchResult.TimeSpan);

		}
	}
}
