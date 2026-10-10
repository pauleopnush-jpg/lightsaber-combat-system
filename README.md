# lightsaber-combat-system

A Unity-ready, playable Jedi-inspired lightsaber combat prototype inspired by the flow, aggression, and rhythm of Jedi: Fallen Order.

What is included
- 3rd-person movement with dodge, jump, and lock-on style facing
- melee combo chain with light and heavy attacks
- blocking and parry flow
- enemy AI that closes distance and attacks
- weapon trail and glow effects
- auto scene bootstrap for quick playable setup

This repo is designed to be opened in Unity and built as a Windows executable.

Quick start
1. Open Unity Hub and create a new 3D project using Unity 2022 LTS or newer.
2. Clone this repo into a local folder or copy the `Assets` folder into your Unity project.
3. Open the project and keep the generated `Assets` folder structure intact.
4. Create an empty GameObject in the scene and add `CombatBootstrap` to it.
5. Press Play.
6. Build to Windows: File > Build Settings > PC, Mac & Linux Standalone > Windows > Build.

Controls
- Left click: light slash combo
- Right click: heavy attack
- Q: block
- E: parry
- Left Shift: dodge
- Space: jump

Notes
- This is a prototype inspired by the feel of a large combat action game, not a direct asset copy of a commercial game.
- It is intentionally built to be easily extended with animation clips, FX, enemy variants, and polished UI.

Recommended next upgrades
- Add actual humanoid animation clips and animator controller
- Add more enemy types, boss fight logic, and combo counters
- Add hit stop, camera shake, and particle impacts
- Create a proper UI health/stamina HUD and kill screen

Files in this repo
- `Assets/Scripts/Combat/PlayerController.cs`
- `Assets/Scripts/Combat/EnemyCombatant.cs`
- `Assets/Scripts/Combat/CameraFollow.cs`
- `Assets/Scripts/Combat/LightsaberWeapon.cs`
- `Assets/Scripts/Runtime/CombatBootstrap.cs`

This repo is suitable for turning into a full game in Unity.
