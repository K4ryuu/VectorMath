using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

namespace VectorMath.Models.Calculations
{
	public class VectorAddition
	{
		public static List<VectorAddition> List = [];

		public Line OriginalLine { get; private set; }
		public Line TransformedLine { get; private set; }
		public Vector AdditionVector { get; private set; }

		private readonly List<Line> _visualizationLines = new List<Line>();

		public VectorAddition(Line originalLine, Vector additionVector)
		{
			OriginalLine = originalLine;
			AdditionVector = additionVector;

			Vector newStart = OriginalLine.Start + AdditionVector;
			Vector newEnd = OriginalLine.End + AdditionVector;

			TransformedLine = new Line(newStart, newEnd, originalLine.Color);

			CreateVisualization();
			List.Add(this);
		}

		private void CreateVisualization()
		{
			_visualizationLines.Add(new Line(OriginalLine.Start, TransformedLine.Start, Color.Yellow));
			_visualizationLines.Add(new Line(OriginalLine.End, TransformedLine.End, Color.Yellow));
		}

		public void Dispose()
		{
			OriginalLine.Dispose();
			TransformedLine.Dispose();
			foreach (var line in _visualizationLines)
			{
				line.Dispose();
			}
			List.Remove(this);
		}
	}
}