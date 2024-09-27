using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

namespace VectorMath.Models.Calculations
{
	public class VectorMultiplication
	{
		public static List<VectorMultiplication> List = new List<VectorMultiplication>();

		public Line VectorA { get; private set; }
		public Line VectorB { get; private set; }
		public float ScalarProduct { get; private set; }
		public Vector CrossProduct { get; private set; }

		private List<Line> _visualizationLines = [];
		public Line? ResultLine { get; private set; }

		public VectorMultiplication(Line vectorA, Line vectorB)
		{
			VectorA = vectorA;
			VectorB = vectorB;
			CrossProduct = new Vector(0, 0, 0);

			CalculateProducts();
			CreateVisualization();
			List.Add(this);
		}

		private void CalculateProducts()
		{
			Vector a = VectorA.End - VectorA.Start;
			Vector b = VectorB.End - VectorB.Start;

			// Skaláris szorzat
			ScalarProduct = a.X * b.X + a.Y * b.Y + a.Z * b.Z;

			// Vektoriális szorzat
			CrossProduct = new Vector(
				a.Y * b.Z - a.Z * b.Y,
				a.Z * b.X - a.X * b.Z,
				a.X * b.Y - a.Y * b.X
			);
		}

		private void CreateVisualization()
		{
			// Vektoriális szorzat megjelenítése
			ResultLine = new Line(VectorA.Start, VectorA.Start + CrossProduct, Color.Purple);
			_visualizationLines.Add(ResultLine);

			// Skaláris szorzat megjelenítése (a VectorA irányában)
			Vector scalarDirection = Plugin.NormalizeVector(VectorA.End - VectorA.Start);
			Vector scalarVisualization = VectorA.Start + MultiplyVector(scalarDirection, ScalarProduct);
			_visualizationLines.Add(new Line(VectorA.Start, scalarVisualization, Color.Orange));
		}

		private static Vector MultiplyVector(Vector v, float scalar)
		{
			return new Vector(v.X * scalar, v.Y * scalar, v.Z * scalar);
		}

		public void Dispose()
		{
			foreach (var line in _visualizationLines)
			{
				line.Dispose();
			}
			List.Remove(this);
		}
	}
}