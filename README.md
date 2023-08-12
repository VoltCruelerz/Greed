# The Greed Mod Manager

_A mod deconfliction tool for [Sins of a Solar Empire II](https://www.sinsofasolarempire2.com/)_

[Github](https://github.com/VoltCruelerz/Greed)

> _**WARNING**: At present, Greed only supports deconfliction in the `entities` and `localized_text` folders, though future updates will add further support._

![screenshot](assets/Screenshot.png)

## Installation

1. Download `Greed.zip` from the [latest github release](https://github.com/VoltCruelerz/Greed/releases)
2. Extract the file wherever you please on your machine.
3. Edit `Greed.dll.config` to have your own file paths using backslashes.
    1. `sinsDir` should be the location of Sins II's exe. (eg `E:\Epic Games\SinsII`)
    2. `modDir` should be the location of the mod folder. (eg `C:\Users\VoltC\AppData\Local\sins2\mods`)
4. Run `Greed.exe`
5. In the window, you'll find a list of Greed-compatible mods to the left, which you can activate or deactivate as you wish.
6. When you have the mods activated that you want, click `[Export]`.
7. Congratulations, those mods are now active for your game! Start up Sins II and have at it!

## For Mod Developers

To create a Greed-compatible mod, you need only add a `greed.json` file to your mod's folder, as seen [here](https://github.com/VoltCruelerz/pd-strings/blob/main/greed.json).

```json
{
    "name": "Point Defense Strings",
    "author": "Volt Cruelerz",
    "url": "https://github.com/VoltCruelerz/pd-strings",
    "description": "Blurb goes here.",
    "version": "1.0.0",
    "sinsVersion": "1.14.3.0",
    "dependencies": [],
    "conflicts": [],
    "priority": 100
}
```

### Priority

Priority is the mod load order. A mod with priority 10 will load before one with priority 20. This means that any conflicting files between the two will have the latter mod's contents unless it is a file for which deconfliction is handled like an `entity_manifest` file.

### Contributing to Greed

I welcome contributions. Just open a pull request.

If at some point in the future, I can no longer maintain Greed, it's MIT licensed.

## Future Work

- folders beyond `entities` and `localized_text`
- conflicts and dependencies
- allow users to reorder priority lists
- add installer from online repository
