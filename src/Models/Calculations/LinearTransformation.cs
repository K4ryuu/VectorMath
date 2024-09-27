using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

namespace VectorMath.Models.Calculations
{
	public class LinearTransformation
	{
		public static List<LinearTransformation> List = new List<LinearTransformation>();

		public Line OriginalVector { get; private set; }
		public Line TransformedVector { get; private set; }
		public TransformationType Type { get; private set; }

		private List<Line> _visualizationLines = [];

		public enum TransformationType
		{
			Rotation,
			Scaling,
			Reflection
		}

		public LinearTransformation(Line originalVector, TransformationType type, float parameter)
		{
			OriginalVector = originalVector;
			Type = type;

			Vector transformed = ApplyTransformation(originalVector.End - originalVector.Start, type, parameter);
			TransformedVector = new Line(originalVector.Start, originalVector.Start + transformed, Color.Green);

			CreateVisualization();
			List.Add(this);
		}

		private Vector ApplyTransformation(Vector v, TransformationType type, float parameter)
		{
			return type switch
			{
				TransformationType.Rotation => RotateVector(v, parameter),
				TransformationType.Scaling => ScaleVector(v, parameter),
				TransformationType.Reflection => ReflectVector(v, parameter),
				_ => v,
			};
		}

		private static Vector RotateVector(Vector v, float angle)
		{
			float radians = angle * (float)Math.PI / 180;
			return new Vector(
				v.X * (float)Math.Cos(radians) - v.Y * (float)Math.Sin(radians),
				v.X * (float)Math.Sin(radians) + v.Y * (float)Math.Cos(radians),
				v.Z
			);
		}

		private static Vector ScaleVector(Vector v, float scale)
		{
			return new Vector(v.X * scale, v.Y * scale, v.Z * scale);
		}

		private static Vector ReflectVector(Vector v, float axis)
		{
			// Tükrözés az X tengely mentén (ha axis == 0), Y tengely mentén (ha axis == 1), vagy Z tengely mentén (ha axis == 2)
			return axis switch
			{
				0 => new Vector(v.X, -v.Y, -v.Z),
				1 => new Vector(-v.X, v.Y, -v.Z),
				2 => new Vector(-v.X, -v.Y, v.Z),
				_ => v,
			};
		}

		private void CreateVisualization()
		{
			// Eredeti és transzformált vektor összekötése
			_visualizationLines.Add(new Line(OriginalVector.End, TransformedVector.End, Color.Yellow));
		}

		public void Dispose()
		{
			OriginalVector.Dispose();
			TransformedVector.Dispose();
			foreach (var line in _visualizationLines)
			{
				line.Dispose();
			}
			List.Remove(this);
		}
	}
}