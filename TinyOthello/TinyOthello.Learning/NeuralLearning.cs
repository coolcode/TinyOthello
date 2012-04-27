using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AForge.Neuro;
using AForge.Neuro.Learning;
using TinyOthello.Engines;

namespace TinyOthello.Learning {
	public class NeuralLearning {

		public void CreateKnowledge() {

			//int endGameDepth = Math.Min((Constants.StoneCount - 4) / 2, 20);

			int initialCount = 6;
			int leftEmpthCount = initialCount;

			while (leftEmpthCount <= Constants.StoneCount - 4) {

				string knowFile = string.Format("{0}-{1}-{2:yyyy-MM-dd}.know",
											Constants.Line,
											leftEmpthCount,
											DateTime.Now);
				IEngine engine;
				if(leftEmpthCount > initialCount+1  ) {
					 engine = new NeuralEngine();
				}
				else {
					engine = new EndGameEngine();
				}

				var know = new Knowledge(1000000, leftEmpthCount, leftEmpthCount > initialCount+1 ? 2 : 12);
				know.Engine = engine;
				know.Generate(knowFile);

				var networkFile = Learn(knowFile);

				leftEmpthCount++;
			}


		}

		public string Learn(string knowFile) {
			var network = new ActivationNetwork(new BipolarSigmoidFunction(), Constants.StoneCount, 1);

			var teacher = new BackPropagationLearning(network); //new PerceptronLearning(network);

			var data = LoadData(knowFile);

			double error = int.MaxValue;

			int index = 0;
			while (error > 1.0 && index++ < 5000) {
				error = teacher.RunEpoch(data.Item1, data.Item2);
			}

			var networkFile = knowFile + ".net";

			network.Save(networkFile);

			Console.WriteLine("Learn: {0}, Gen: {1}", knowFile, networkFile);

			return networkFile;
		}

		public void Learn() {
			var network = new ActivationNetwork(new BipolarSigmoidFunction(), Constants.StoneCount, 1);

			var teacher = new BackPropagationLearning(network);//new PerceptronLearning(network);

			var data = LoadData("4-6-2012-04-24.know");

			double error = 1.0;

			int index = 0;
			while (error > 0.001 && index < 100000) {
				error = teacher.RunEpoch(data.Item1, data.Item2);
				index++;
			}

			network.Save("4-6-2012-04-24.bp.net");

			var text = "□○○○●○○□○●●□□●□□";
			var i = ToDouble(text);//-2
			var o = network.Compute(i);

			var eval = o[0] * 2 * Constants.StoneCount - Constants.StoneCount;

			Console.WriteLine("{0} {1}", text, eval);
		}

		private Tuple<double[][], double[][]> LoadData(string fileName) {
			var input = new List<double[]>();
			var output = new List<double[]>();

			var targetPath = Path.Combine(Environment.CurrentDirectory, "learning-results");
			targetPath = Path.Combine(targetPath, fileName);

			using (var reader = new StreamReader(targetPath)) {
				while (!reader.EndOfStream) {
					var text = reader.ReadLine();
					var splits = text.Split(' ');
					var d = ToDouble(splits[0]);
					double o = Convert.ToInt32(splits[1]);

					if (splits[2] == "○") {
						o = -o;
					}

					o = (o + Constants.StoneCount) / (double)(2 * Constants.StoneCount);

					input.Add(d);
					output.Add(new double[] { o });
				}
			}

			return new Tuple<double[][], double[][]>(input.ToArray(), output.ToArray());
		}

		private double[] ToDouble(string text) {
			return text.Select(c => (c == '●' ? 0.9 : (c == '○' ? 0.1 : 0.5))).ToArray();
		}

		public void CalDelta() {
			var data = LoadData("4-6-2012-04-24.know");
			var d1 = calDelta(data, "4-6-2012-04-24.bp.net") / 2300 * 2 * Constants.StoneCount;
			var d2 = calDelta(data, "4-6-2012-04-24.net") / 2300 * 2 * Constants.StoneCount;
			Console.WriteLine("{0}\n{1}", d1, d2);
		}

		private double calDelta(Tuple<double[][], double[][]> data, string file) {
			var network = Network.Load(file);
			double delta = 0;
			int i = 0;
			foreach (var item in data.Item1) {
				var o = network.Compute(item);
				delta += Math.Pow(data.Item2[i][0] - o[0], 2);
				i++;
			}

			return delta;
		}

		public void LearnDemo() {
			ActivationNetwork network = new ActivationNetwork(new ThresholdFunction(), 2, 1);//Constants.StoneCount

			PerceptronLearning teacher = new PerceptronLearning(network);

			double[][] input = new double[4][];
			double[][] output = new double[4][];

			input[0] = new double[] { 0, 0 };
			output[0] = new double[] { 0 };
			input[1] = new double[] { 0, 1 };
			output[1] = new double[] { 0 };
			input[2] = new double[] { 1, 0 };
			output[2] = new double[] { 0 };
			input[3] = new double[] { 1, 1 };
			output[3] = new double[] { 1 };

			double error = 1.0;

			while (error > 0.001) {
				error = teacher.RunEpoch(input, output);
			}

			var k = network.Compute(new double[] { 0.9, 0.7 });
			var o = network.Output;


			network.Save("a.txt");
		}

		public void LoadNetwork() {
			Network network = Network.Load("a.txt");

			double[][] input = new double[4][];

			input[0] = new double[] { 0, 0 };
			input[1] = new double[] { 0, 1 };
			input[2] = new double[] { 1, 0 };
			input[3] = new double[] { 1, 1 };

			foreach (var i in input) {
				var o = network.Compute(i);
				Console.WriteLine(o[0]);
			}
		}
	}
}
