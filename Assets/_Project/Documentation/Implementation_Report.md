# Campfire Adventure 3D - Implementation Report (Phases 0-10)

Unity 6000.3.10f1, URP 17.3, new Input System. Scene: `Assets/_Project/Scenes/Gameplay/Gameplay.unity` (SampleScene untouched).

## Controls
| Key | Action |
|---|---|
| 1 | Go to lumberyard |
| 2 | Collect wood (2 collections needed) |
| 3 | Go to quarry |
| 4 | Collect stones (2 collections needed) |
| 5 | Return to camp |
| 6 | Prepare campfire |
| 7 | Ignite campfire (first try sparks only, second try lights) |
| E | Interact with the nearby point |
| F5 | Restart adventure |
| Esc | Pause menu |

One key press = one named action: the character auto-navigates (NavMeshAgent), faces the target, performs the action once, returns to idle.

## Architecture (all under `Assets/_Project/Scripts`)
- **Core**: `GameActionController` (state machine Idle/Navigating/PerformingAction/Completing/Failed), `IGameActionSource` and the other input interfaces, `AdventureSession`, `AdventureStats`, `SettingsController`, `LightingMoodController`, `TimeOfDayDirector`, `StartupSelfCheck`.
- **Input**: `KeyboardActionSource`, `MenuInputSource`. A future validator source only needs to implement `IGameActionSource` (not implemented, by design).
- **Player**: `AutoNavigationController`, `PlayerAnimationDriver`, `AxeSwingAnimator`, `FootstepDust`.
- **Gathering / Objectives / Campfire / Interaction / Camera / Audio / UI**: resource points, piles, inventory, ScriptableObject objectives, campfire states, isometric camera, audio players, HUD and menu views.

## Scene setup notes
- NavMesh baked into `Gameplay_NavMesh.asset`. The explorer's NavMeshAgent is saved disabled and enabled in `AutoNavigationController.Start()` (fixes "no valid NavMesh" in builds).
- Grass cost 6, trail/pad cost 1 so the character prefers the paths.
- UI: Canvas Scaler 1920x1080, match 0.5; checked at 9 resolutions including 4:3, 16:10, 21:9.

## Placeholders and missing assets
Placeholder audio (`Audio/SFX/Placeholders/PLACEHOLDER_*.wav`): spark strike, fire whoosh, fire loop, completion chime, UI hover/click/back, day/night ambience, music pad. Replace with real clips.
Real audio in use: wood chop, rock hit, rock break, footsteps (trimmed copies; originals untouched).
Art still primitives: log stack, planks, workbench, stone blocks, pickaxe.
Animations: no dedicated carry/ignite/place clips (pickup/procedural reused). Character is the chibi sample, scaled 1.5.

## Known issues / not verified by automation
- Real key presses, audio levels by ear and gamepad were not tested.
- Build Settings lists only SampleScene; product/company names are still defaults. Builds used an explicit scene list. Changing these is a project-wide setting, so it needs the owner's approval.
- One non-blocking build warning; player log shows only a harmless d3d12 info-queue message.

## Build
`Builds/CampfireAdventure_Windows/CampfireAdventure.exe` (about 180 MB). Player log confirms `[SelfCheck] Navigation OK`.
