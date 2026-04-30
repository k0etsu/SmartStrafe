# SmartStrafe

A Dalamud plugin that intelligently switches keyboard controls between strafing and turning based on combat state.

> **Note:** Legacy movement type only.

## Features

- Automatically strafe in combat, turn out of combat (configurable)
- Optional backpedal suppression while strafing
- Manual override: holding both left and right simultaneously always activates strafing

## Modes

| Mode | Behavior |
|---|---|
| Turning | Left/right keys always turn |
| Strafing | Left/right keys always strafe |
| Strafing (no backpedaling) | Left/right keys strafe, unless moving backward |

## Usage

Open the configuration window via `/smartstrafe` or the Dalamud plugin menu.

## Credits

Original tweak by [Iryoku](https://github.com/Iryoku) as part of [SimpleTweaksPlugin](https://github.com/Caraxi/SimpleTweaksPlugin). Ripped out to be standalone because it's just that good.
