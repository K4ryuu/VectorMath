using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

namespace VectorMath.Models.Calculations
{
	public class VectorSpaceBasis
	{
		public static List<VectorSpaceBasis> List = new List<VectorSpaceBasis>();

		public List<Line> BasisVectors { get; private set; }
		public bool IsLinearlyIndependent { get; private set; }

		private List<Line> _visualizationLines = new List<Line>();

		public VectorSpaceBasis(List<Line> basisVectors)
		{
			BasisVectors = basisVectors;
			IsLinearlyIndependent = CheckLinearIndependence();

			CreateVisualization();
			List.Add(this);
		}

		private bool CheckLinearIndependence()
		{
			// Egyszerűsített ellenőrzés 2D és 3D esetekre
			if (BasisVectors.Count == 2)
			{
				Vector v1 = BasisVectors[0].End - BasisVectors[0].Start;
				Vector v2 = BasisVectors[1].End - BasisVectors[1].Start;
				return Math.Abs(v1.X * v2.Y - v1.Y * v2.X) > 0.001f;
			}
			else if (BasisVectors.Count == 3)
			{
				Vector v1 = BasisVectors[0].End - BasisVectors[0].Start;
				Vector v2 = BasisVectors[1].End - BasisVectors[1].Start;
				Vector v3 = BasisVectors[2].End - BasisVectors[2].Start;
				Vector cross = new Vector(
					v1.Y * v2.Z - v1.Z * v2.Y,
					v1.Z * v2.X - v1.X * v2.Z,
					v1.X * v2.Y - v1.Y * v2.X
				);
				return Math.Abs(cross.X * v3.X + cross.Y * v3.Y + cross.Z * v3.Z) > 0.001f;
			}
			return false; // Más dimenziók esetén további implementáció szükséges
		}

		private void CreateVisualization()
		{
			Color[] colors = { Color.Red, Color.Green, Color.Blue };
			for (int i = 0; i < BasisVectors.Count; i++)
			{
				_visualizationLines.Add(new Line(BasisVectors[i].Start, BasisVectors[i].End, colors[i % colors.Length]));
			}

			// Lineáris függetlenség vizualizálása
			if (IsLinearlyIndependent)
			{
				// Feszített parallelogramma/paralelepipedon megjelenítése
				if (BasisVectors.Count == 2)
				{
					Vector v1 = BasisVectors[0].End - BasisVectors[0].Start;
					Vector v2 = BasisVectors[1].End - BasisVectors[1].Start;
					_visualizationLines.Add(new Line(BasisVectors[0].End, BasisVectors[0].End + v2, Color.Yellow));
					_visualizationLines.Add(new Line(BasisVectors[1].End, BasisVectors[1].End + v1, Color.Yellow));
				}
				else if (BasisVectors.Count == 3)
				{
					// 3D paralelepipedon megjelenítése (egyszerűsített)
					Vector v1 = BasisVectors[0].End - BasisVectors[0].Start;
					Vector v2 = BasisVectors[1].End - BasisVectors[1].Start;
					Vector v3 = BasisVectors[2].End - BasisVectors[2].Start;
					_visualizationLines.Add(new Line(BasisVectors[0].End, BasisVectors[0].End + v2, Color.Yellow));
					_visualizationLines.Add(new Line(BasisVectors[0].End, BasisVectors[0].End + v3, Color.Yellow));
					_visualizationLines.Add(new Line(BasisVectors[1].End, BasisVectors[1].End + v1, Color.Yellow));
					_visualizationLines.Add(new Line(BasisVectors[1].End, BasisVectors[1].End + v3, Color.Yellow));
					_visualizationLines.Add(new Line(BasisVectors[2].End, BasisVectors[2].End + v1, Color.Yellow));
					_visualizationLines.Add(new Line(BasisVectors[2].End, BasisVectors[2].End + v2, Color.Yellow));
				}
			}
		}

		public void Dispose()
		{
			foreach (var line in BasisVectors)
			{
				line.Dispose();
			}
			foreach (var line in _visualizationLines)
			{
				line.Dispose();
			}
			List.Remove(this);
		}
	}
}