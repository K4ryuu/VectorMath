using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Utils;
using VectorMath.Models;
using VectorMath.Models.Calculations;

namespace VectorMath
{
	[MinimumApiVersion(250)]
	public sealed partial class Plugin : BasePlugin
	{
		public override string ModuleName => "Vector Math Visualizer";
		public override string ModuleAuthor => "K4ryuu";
		public override string ModuleDescription => "This is a simple project to my university math class, which connects to programming.";
		public override string ModuleVersion => "1.0.0";

		public Dictionary<CCSPlayerController, SetupState> _setupStates = new Dictionary<CCSPlayerController, SetupState>();
		public List<Line> _lines = [];
		private Line? _lastCreatedLine = null;

		public override void Load(bool hotReload)
		{
			AddCommand("css_vektor", "Vector készítés szemléltetéshez", (player, info) =>
			{
				if (player is null)
				{
					info.ReplyToCommand($" {ChatColors.White}[ {ChatColors.LightBlue}VectorMath{ChatColors.White} ]{ChatColors.Red} Csak játékosok használhatják ezt a parancsot.");
					return;
				}

				if (info.ArgCount < 2)
				{
					info.ReplyToCommand($" {ChatColors.White}[ {ChatColors.LightBlue}VectorMath{ChatColors.White} ]{ChatColors.Yellow} Használat: !css_vektor <line|wall|zone>");
					return;
				}

				if (_setupStates.ContainsKey(player))
				{
					info.ReplyToCommand($" {ChatColors.White}[ {ChatColors.LightBlue}VectorMath{ChatColors.White} ]{ChatColors.Red} Már van egy aktív vektor készítési folyamatod.");
					return;
				}

				if (Enum.TryParse(info.ArgByIndex(1), true, out SetupType type))
				{
					_setupStates[player] = new SetupState(type);
					player.PrintToChat($" {ChatColors.White}[ {ChatColors.LightBlue}VectorMath{ChatColors.White} ]{ChatColors.Yellow} Kérlek jelölj ki egy pontot.");
				}
				else
				{
					info.ReplyToCommand($" {ChatColors.White}[ {ChatColors.LightBlue}VectorMath{ChatColors.White} ]{ChatColors.Red} Hibás típus. Használat: !css_vektor <line|wall|zone>");
				}
			});

			AddCommand("css_vektortav", "Két játékos közötti távolság megjelenításe", (player, info) =>
			{
				if (player is null)
				{
					info.ReplyToCommand($" {ChatColors.White}[ {ChatColors.LightBlue}VectorMath{ChatColors.White} ]{ChatColors.Red} Csak játékosok használhatják ezt a parancsot.");
					return;
				}

				if (info.ArgCount < 2)
				{
					info.ReplyToCommand($" {ChatColors.White}[ {ChatColors.LightBlue}VectorMath{ChatColors.White} ]{ChatColors.Yellow} Használat: !css_vektortav <céj-játékos>");
					return;
				}

				TargetResult? target = info.GetArgTargetResult(1);
				if (!target.Any())
				{
					info.ReplyToCommand($" {ChatColors.White}[ {ChatColors.LightBlue}VectorMath{ChatColors.White} ]{ChatColors.Red} Hibás játékos azonosító.");
					return;
				}

				_ = new Distance(this, player, target.First());

				player.PrintToChat($" {ChatColors.White}[ {ChatColors.LightBlue}VectorMath{ChatColors.White} ]{ChatColors.Yellow} A két játékos közötti távolság követés aktív 20 másodpercig.");
			});

			AddCommand("css_vektoradd", "Vektor összeadás szemléltetése", (player, info) =>
			{
				if (player == null || !player.IsValid)
				{
					return;
				}

				if (info.ArgCount < 4)
				{
					player.PrintToChat($" {ChatColors.Red}Használat: css_vektoradd <x> <y> <z>");
					return;
				}

				if (!float.TryParse(info.GetArg(1), out float x) ||
					!float.TryParse(info.GetArg(2), out float y) ||
					!float.TryParse(info.GetArg(3), out float z))
				{
					player.PrintToChat($" {ChatColors.Red}Érvénytelen koordináták. Használj számokat.");
					return;
				}

				Vector additionVector = new Vector(x, y, z);
				Vector playerPos = player.PlayerPawn.Value!.AbsOrigin!;

				if (_lastCreatedLine == null)
				{
					_lastCreatedLine = new Line(playerPos, playerPos + additionVector, Color.Red);
					player.PrintToChat($" {ChatColors.Green}Első vektor létrehozva!");
				}
				else
				{
					VectorAddition addition = new VectorAddition(_lastCreatedLine, additionVector);
					_lastCreatedLine.ChangeColor(Color.Gray);
					_lastCreatedLine = addition.TransformedLine;
					player.PrintToChat($" {ChatColors.Green}Vektor hozzáadva és eltolva!");
				}

				player.PrintToChat($" {ChatColors.Yellow}Utolsó vektor vége: ({_lastCreatedLine.End.X}, {_lastCreatedLine.End.Y}, {_lastCreatedLine.End.Z})");
			});

			AddCommand("css_vektorszorzas", "Vektor szorzás szemléltetése", (player, info) =>
			{
				if (player == null || !player.IsValid || _lastCreatedLine == null)
				{
					return;
				}

				if (info.ArgCount < 4)
				{
					player.PrintToChat($" {ChatColors.Red}Használat: css_vektorszorzas <x> <y> <z>");
					return;
				}

				if (!float.TryParse(info.GetArg(1), out float x) ||
					!float.TryParse(info.GetArg(2), out float y) ||
					!float.TryParse(info.GetArg(3), out float z))
				{
					player.PrintToChat($" {ChatColors.Red}Érvénytelen koordináták. Használj számokat.");
					return;
				}

				Vector playerPos = player.PlayerPawn.Value!.AbsOrigin!;
				Line vectorB = new Line(playerPos, playerPos + new Vector(x, y, z), Color.Blue);

				_lastCreatedLine = new VectorMultiplication(_lastCreatedLine, vectorB).ResultLine;

				player.PrintToChat($" {ChatColors.Green}Vektor szorzás szemléltetése létrehozva!");
			});

			AddCommand("css_transzformacio", "Lineáris transzformáció szemléltetése", (player, info) =>
			{
				if (player == null || !player.IsValid || _lastCreatedLine == null)
				{
					return;
				}

				if (info.ArgCount < 3)
				{
					player.PrintToChat($" {ChatColors.Red}Használat: css_transzformacio <típus> <paraméter>");
					player.PrintToChat($" {ChatColors.Red}Típusok: rotation, scaling, reflection");
					return;
				}

				if (!Enum.TryParse(info.GetArg(1), true, out LinearTransformation.TransformationType type) ||
					!float.TryParse(info.GetArg(2), out float parameter))
				{
					player.PrintToChat($" {ChatColors.Red}Érvénytelen típus vagy paraméter.");
					return;
				}

				_lastCreatedLine = new LinearTransformation(_lastCreatedLine, type, parameter).TransformedVector;
				player.PrintToChat($" {ChatColors.Green}Lineáris transzformáció szemléltetése létrehozva!");
			});

			AddCommand("css_vektorbazis", "Vektortér bázis szemléltetése", (player, info) =>
			{
				if (player == null || !player.IsValid)
				{
					return;
				}

				if (info.ArgCount < 7 || info.ArgCount > 10)
				{
					player.PrintToChat($" {ChatColors.Red}Használat: css_vektorbazis <x1> <y1> <z1> <x2> <y2> <z2> [<x3> <y3> <z3>]");
					return;
				}

				List<Line> basisVectors = new List<Line>();
				Vector playerPos = player.PlayerPawn.Value!.AbsOrigin!;

				for (int i = 1; i <= 9; i += 3)
				{
					if (i + 2 >= info.ArgCount) break;

					if (!float.TryParse(info.GetArg(i), out float x) ||
						!float.TryParse(info.GetArg(i + 1), out float y) ||
						!float.TryParse(info.GetArg(i + 2), out float z))
					{
						player.PrintToChat($" {ChatColors.Red}Érvénytelen koordináták. Használj számokat.");
						return;
					}

					basisVectors.Add(new Line(playerPos, playerPos + new Vector(x, y, z), Color.FromArgb(255, 0, 0)));
				}

				VectorSpaceBasis basis = new VectorSpaceBasis(basisVectors);
				player.PrintToChat($" {ChatColors.Green}Vektortér bázis szemléltetése létrehozva!");
				player.PrintToChat($" {ChatColors.Yellow}A bázis {(basis.IsLinearlyIndependent ? "lineárisan független" : "lineárisan függő")}.");
			});

			AddCommand("css_kovetes", "Két játékos közötti távolság megjelenításe", (player, info) =>
			{
				if (player is null)
				{
					info.ReplyToCommand($" {ChatColors.White}[ {ChatColors.LightBlue}VectorMath{ChatColors.White} ]{ChatColors.Red} Csak játékosok használhatják ezt a parancsot.");
					return;
				}

				_ = new PlayerInfo(this, player);

				player.PrintToChat($" {ChatColors.White}[ {ChatColors.LightBlue}VectorMath{ChatColors.White} ]{ChatColors.Yellow} A játékos információ követés aktív 20 másodpercig.");
			});
		}

		public override void Unload(bool hotReload)
		{
			foreach (KeyValuePair<CCSPlayerController, SetupState> setupState in _setupStates)
			{
				setupState.Key.PrintToChat($" {ChatColors.White}[ {ChatColors.LightBlue}VectorMath{ChatColors.White} ]{ChatColors.Red} A vektor készítési folyamatod megszakadt.");
			}

			_setupStates.Clear();

			foreach (var line in Line.List.ToList())
			{
				line.Dispose();
			}

			foreach (var distance in Zone.List.ToList())
			{
				distance.Dispose();
			}
		}

		[GameEventHandler(HookMode.Pre)]
		public HookResult BulletImpact(EventBulletImpact @event, GameEventInfo info)
		{
			CCSPlayerController? player = @event.Userid;
			if (player?.IsValid == true && player.PlayerPawn.IsValid == true)
			{
				if (_setupStates.TryGetValue(player, out SetupState? setupState))
				{
					Vector bulletDestination = new Vector(@event.X, @event.Y, @event.Z);

					if (setupState.PositionA is null)
					{
						setupState.PositionA = bulletDestination;

						Server.NextFrame(() => player.PrintToChat($" {ChatColors.White}[ {ChatColors.LightBlue}VectorMath{ChatColors.White} ]{ChatColors.Yellow} Kérlek jelölj ki egy második pontot."));
					}
					else
					{
						setupState.PositionB = bulletDestination;
						float distance = DistanceTo(setupState.PositionA, setupState.PositionB);

						if (distance < 50)
						{
							Server.NextFrame(() => player.PrintToChat($" {ChatColors.White}[ {ChatColors.LightBlue}VectorMath{ChatColors.White} ]{ChatColors.Red} A kijelölt területnek legalább {distance:F2} egység távolságra kell lennie."));
						}
						else
						{
							switch (setupState.Type)
							{
								case SetupType.Zone:
									_ = new Zone(this, setupState.PositionA, setupState.PositionB)
									{
										EntryAction = (p) =>
										{
											p.PrintToChat($" {ChatColors.White}[ {ChatColors.LightBlue}VectorMath{ChatColors.White} ]{ChatColors.LightRed} Ebbe a zónába nem léphetsz be.");
											BounceBack(p);
										},
									};
									break;
								case SetupType.Wall:
									// Create a wall ha kell
									break;
								case SetupType.Line:
									_lastCreatedLine = new Line(setupState.PositionA, setupState.PositionB, null, true);
									break;
							}

							_setupStates.Remove(player);
							Server.NextFrame(() =>
							{
								player.PrintToChat($" {ChatColors.White}[ {ChatColors.LightBlue}VectorMath{ChatColors.White} ]{ChatColors.Green} A vektor készítési folyamat sikeresen befejeződött.");
								player.PrintToChat($" ");
								player.PrintToChat($" --- {ChatColors.Yellow}Vektor információk ---");
								player.PrintToChat($" {ChatColors.Yellow}Típus: {ChatColors.White}{setupState.Type}");
								player.PrintToChat($" {ChatColors.Yellow}Kezdő pont: {ChatColors.White}{setupState.PositionA}");
								player.PrintToChat($" {ChatColors.Yellow}Vég pont: {ChatColors.White}{setupState.PositionB}");
							});
						}
					}

					return HookResult.Continue;
				}
			}

			return HookResult.Continue;
		}
	}
}