using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

namespace VectorMath.Models
{
	public class Zone : IDisposable
	{
		public static List<Zone> List = [];

		private static int GlobalCounter = 0;

		// ** Public Variables */
		public int Id { get; }

		public bool EditMode { get; set; }
		public Vector PositionA { get; set; }
		public Vector PositionB { get; set; }
		public Color Color { get; set; } = Color.FromArgb(new Random().Next(0, 255), new Random().Next(0, 255), new Random().Next(0, 255));

		// ** Settings Variables */
		public float CheckEntryInterval { get; set; } = 0.05f;
		public float InsideActionInterval { get; set; } = 1.0f;

		// ** Inner Variables */
		private readonly Plugin Plugin;
		private List<Beam> DisplayBeams = new List<Beam>();
		private HashSet<CCSPlayerController> playersInsideZone = new HashSet<CCSPlayerController>();
		private CounterStrikeSharp.API.Modules.Timers.Timer entryTimer;
		private CounterStrikeSharp.API.Modules.Timers.Timer insideTimer;
		private bool disposedValue;

		// ** Actions */
		public Action<CCSPlayerController>? EntryAction { get; set; }
		public Action<CCSPlayerController>? InsideAction { get; set; }
		public Action<CCSPlayerController>? ExitAction { get; set; }

		public Zone(Plugin plugin, Vector positionA, Vector positionB)
		{
			Plugin = plugin;

			Id = GlobalCounter++;

			PositionA = positionA;
			PositionB = positionB;

			entryTimer = Plugin.AddTimer(CheckEntryInterval, CheckEntryExit, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
			insideTimer = Plugin.AddTimer(InsideActionInterval, PerformInsideActions, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);

			CreateBeams();

			List.Add(this);
		}

		private void CheckEntryExit()
		{
			var players = Utilities.GetPlayers()
				.Where(p => p?.IsValid == true && p.PlayerPawn.IsValid == true && !p.IsHLTV && p.Connected == PlayerConnectedState.PlayerConnected && p.LifeState == (int)LifeState_t.LIFE_ALIVE)
				.ToList();

			var currentPlayersInsideZone = new HashSet<CCSPlayerController>();

			players.ForEach(player =>
			{
				if (IsPlayerInZone(player))
				{
					currentPlayersInsideZone.Add(player);

					if (!playersInsideZone.Contains(player))
					{
						EntryAction?.Invoke(player);
					}
				}
			});

			var playersExitedZone = playersInsideZone.Except(currentPlayersInsideZone).ToList();
			playersExitedZone.ForEach(player => ExitAction?.Invoke(player));

			playersInsideZone = currentPlayersInsideZone;
		}

		public bool IsPlayerInZone(CCSPlayerController player)
		{
			Vector? bodyPosition = player.PlayerPawn.Value!.AbsOrigin!.With(z: player.PlayerPawn.Value!.AbsOrigin.Z + 10);
			Vector eyePosition = Plugin.GetEyePosition(player);

			return IsInside(bodyPosition) || IsInside(eyePosition);
		}

		private void PerformInsideActions()
		{
			playersInsideZone.ToList().ForEach(player =>
			{
				InsideAction?.Invoke(player);
			});
		}

		public bool IsInside(Vector point)
		{
			return point.X >= Math.Min(PositionA.X, PositionB.X) &&
				   point.X <= Math.Max(PositionA.X, PositionB.X) &&
				   point.Y >= Math.Min(PositionA.Y, PositionB.Y) &&
				   point.Y <= Math.Max(PositionA.Y, PositionB.Y) &&
				   point.Z >= Math.Min(PositionA.Z, PositionB.Z) &&
				   point.Z <= Math.Max(PositionA.Z, PositionB.Z);
		}

		public void SetEditMode(bool editMode)
		{
			EditMode = editMode;

			if (editMode)
			{
				CreateBeams();
			}
			else
			{
				RemoveBeams();
			}
		}

		private void CreateBeams()
		{
			RemoveBeams();

			Vector[] corners = GetCorners();

			int[,] edges = new int[,]
			{
				{ 0, 1 }, { 1, 2 }, { 2, 3 }, { 3, 0 }, // Bottom square
                { 4, 5 }, { 5, 6 }, { 6, 7 }, { 7, 4 }, // Top square
                { 0, 4 }, { 1, 5 }, { 2, 6 }, { 3, 7 }  // Vertical edges
            };

			for (int i = 0; i < edges.GetLength(0); i++)
			{
				Vector startPos = corners[edges[i, 0]];
				Vector endPos = corners[edges[i, 1]];

				Beam beam = new Beam(startPos, endPos, Color);
				DisplayBeams.Add(beam);
			}
		}

		public void RemoveBeams()
		{
			if (DisplayBeams.Count > 0)
			{
				DisplayBeams.Where(b => b.IsValid).ToList().ForEach(b => b.Remove());
				DisplayBeams.Clear();
			}
		}

		private Vector[] GetCorners()
		{
			return
			[
				new Vector(PositionA.X, PositionA.Y, PositionA.Z),
				new Vector(PositionA.X, PositionB.Y, PositionA.Z),
				new Vector(PositionB.X, PositionB.Y, PositionA.Z),
				new Vector(PositionB.X, PositionA.Y, PositionA.Z),
				new Vector(PositionA.X, PositionA.Y, PositionB.Z),
				new Vector(PositionA.X, PositionB.Y, PositionB.Z),
				new Vector(PositionB.X, PositionB.Y, PositionB.Z),
				new Vector(PositionB.X, PositionA.Y, PositionB.Z)
			];
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// Dispose managed state (managed objects)
					entryTimer?.Kill();
					insideTimer?.Kill();
					RemoveBeams();
					playersInsideZone.Clear();
				}

				// Free unmanaged resources (unmanaged objects) and override finalizer
				// Set large fields to null
				disposedValue = true;
			}
		}

		// Override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		~Zone()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
			List.Remove(this);
		}
	}
}
