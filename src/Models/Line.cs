using System.Drawing;
using CounterStrikeSharp.API.Modules.Utils;
using VectorMath.Models;

public class Line
{
	public static List<Line> List = new List<Line>();

	public Vector Start { get; set; }
	public Vector End { get; set; }
	public Color Color { get; private set; }

	private List<Beam> _lineBeams = new List<Beam>();

	public Line(Vector start, Vector end, Color? color = null, bool addition = false)
	{
		Start = addition ? start.With(z: start.Z + 10) : start;
		End = addition ? end.With(z: end.Z + 10) : end;
		Color = color ?? Color.FromArgb(new Random().Next(0, 255), new Random().Next(0, 255), new Random().Next(0, 255));

		CreateBeams();
		List.Add(this);
	}

	public void ChangeColor(Color newColor)
	{
		Color = newColor;
		UpdateBeams();
	}

	private void CreateBeams()
	{
		_lineBeams.Add(new Beam(Start, End, Color));

		Vector direction = new Vector(End.X - Start.X, End.Y - Start.Y, End.Z - Start.Z);
		float length = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z);
		direction.X /= length;
		direction.Y /= length;
		direction.Z /= length;

		Vector perp1 = new Vector(-direction.Y, direction.X, 0);
		Vector perp2 = new Vector(direction.Y, -direction.X, 0);

		float arrowLength = 30f;
		perp1 *= arrowLength;
		perp2 *= arrowLength;

		Vector arrowEnd1 = new Vector(
			End.X - direction.X * arrowLength + perp1.X,
			End.Y - direction.Y * arrowLength + perp1.Y,
			End.Z - direction.Z * arrowLength
		);
		Vector arrowEnd2 = new Vector(
			End.X - direction.X * arrowLength + perp2.X,
			End.Y - direction.Y * arrowLength + perp2.Y,
			End.Z - direction.Z * arrowLength
		);

		_lineBeams.Add(new Beam(End, arrowEnd1, Color));
		_lineBeams.Add(new Beam(End, arrowEnd2, Color));
	}

	private void UpdateBeams()
	{
		foreach (var beam in _lineBeams)
		{
			beam.Remove();
		}
		_lineBeams.Clear();
		CreateBeams();
	}

	public void Dispose()
	{
		foreach (var beam in _lineBeams)
		{
			beam.Remove();
		}
		List.Remove(this);
	}
}