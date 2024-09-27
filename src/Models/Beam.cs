using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace VectorMath.Models
{
	public class Beam : IDisposable
	{
		private bool disposed = false;

		public CBeam? BeamEntity { get; private set; }

		public Beam(Vector startPos, Vector endPos, Color color, float width = 5f)
		{
			BeamEntity = Utilities.CreateEntityByName<CBeam>("beam");

			if (BeamEntity != null)
			{
				BeamEntity.Render = color;
				BeamEntity.Width = width;

				BeamEntity.Teleport(startPos, QAngle.Zero, startPos);
				BeamEntity.EndPos.X = endPos.X;
				BeamEntity.EndPos.Y = endPos.Y;
				BeamEntity.EndPos.Z = endPos.Z;
				BeamEntity.DispatchSpawn();
			}
		}

		public bool IsValid => BeamEntity?.IsValid == true;

		public void Remove()
		{
			BeamEntity?.Remove();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					// Dispose managed resources here
					Remove();
				}

				// Dispose unmanaged resources here
				disposed = true;
			}
		}

		// Override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		~Beam() => Dispose(disposing: false);

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
