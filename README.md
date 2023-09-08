# The Greed Mod Manager

_A mod loader for [Sins of a Solar Empire II](https://www.sinsofasolarempire2.com/)_

- [Github](https://github.com/VoltCruelerz/Greed)
- [Forum](https://forums.sinsofasolarempire2.com/522050/page/1)
- [Discord](https://discord.com/channels/266693357093257216/1146234939315069008/1146234939315069008)

![screenshot](assets/Screenshot.png)

## Installation

### Greed Installation

1. Download `Greed.zip` from the [latest github release](https://github.com/VoltCruelerz/Greed/releases)
2. Extract the file wherever you please on your machine.
3. Run `Greed.exe`
4. Set the directories on the Settings tab. They will autosave if the paths exist.
    1. **Sins Directory** should be the location of Sins II's exe. (eg `C:\Epic Games\SinsII`)
    2. **Mods Directory** should be the location of the mod folder. (eg `C:\Users\YOUR_USER\AppData\Local\sins2\mods`)
5. In the main tab, you'll find a list of installed Greed-compatible mods to the left (see below for instructions to install mods), which you can activate or deactivate as you wish either with the button or double-clicking.
6. When you have the mods activated that you want, click `[Export Greedy Selections]`.

### Mod Installation

1. Download the mod from GitHub, ModDB, or wherever else.
    1. If needed, unzip it.
2. Deposit it in `C:\Users\YOUR_USER\AppData\Local\sins2\mods` (change the path to use your Windows username).
    1. If the mod's name is `example-mod`, you should be able to find `greed.json` at `C:\Users\YOUR_USER\AppData\Local\sins2\mods\example-mod\greed.json`
3. Open Greed or press the Refresh button to see the new mod appear.

## Advantages for Users

- **No More Manual Deconfliction**: Deconfliction of commonly modified files happens automatically for you.
- **Easy Activation**: Enable/disable mods with a click.
- **Easy Viewing**: Easily view the metadata, readme, and source files for any mod you download.
- **Simple Load Order Control**: Drag-and-drop elements in the order of your choice.
- **Conflict Detection**: You are warned if you try to enable mods with a known conflict between them.
- **Dependency Management**: You are be warned if you attempt to enable a mod without its dependencies active.
- **Online Mod List**: View an online listing of available Greedy mods.
- **Automated Installation**: You can automatically mods and their dependencies from a curated list.

## Greedy Mod Development

First, some definitions:

- **Gold Files**: the gold copy, which is the original Sins II data files. These are _never_ changed by Greed.
- **Source Files**: the files from the individual greedy mods.
- **Greed Files**: the files that are going into the greed mod folder. When Greed activates a list of mods, it initializes any relevant greed files from gold copies and then sequentially applies the edits of each mod in the list, ultimately producing an execution product.

### Why Develop Greedy Mods?

- **Comments**: You can add C-style comments to your source files.
- **Selective Inclusion**: Your mod only needs to include what you changed about a particular source file (eg add a new property, delete a property, or add an array element), _drastically_ reducing the risk of collisions between mods.
- **Intelligent Merge**: Intelligent merging of localization files.
- **File Diff**: Greed ships with a diff tool specifically for Sins II data files, allowing you to readily see exactly what you've done.
- **Reduced Boilerplate**: Fractional files reduces the effort required to make small mods
- **Improved Exposure**: Greed ships with a viewer of curated Greedy mods. By getting added to that list, your mods can reach more users.
- **Automated Updates**: Users can update to the latest version of your mod.
- **Dependency Autoinstall**: Users can automatically install your mod's dependencies.

![screenshot](assets/DiffScreenshot.png)

### Greed Compatibility

To make a mod compatible with Greed, you need only add a `greed.json` file to your mod's folder, as seen below.

```json
{
    "name": "Your Mod",
    "author": "Your Name",
    "url": "https://github.com/YourName/your-mod",
    "description": "Blurb goes here.",
    "version": "1.0.0",
    "sinsVersion": "1.15.1.0",
    "greedVersion": "1.6.0",
    "dependencies": [
        {
            "id": "other-mod",
            "version": "1.0.0"
        }
    ],
    "conflicts": ["conflicting-mod"]
}
```

While I recommend you take advantage of Greed's more powerful features like merge file extensions, merely adding the above will make any mod interactible for Greed.

### Merge Types

The ultimate goal of Greed is to allow multiple mods to gracefully integrate together so that we can release smaller, more targeted mods, rather than these massive bundles that nobody really knows what's inside them. To facilitate this, Greed acts as an arbiter, compiling greedy mods into a single "mod" in the mods directory which is then listed as active for Sins II to read from.

Ideally, a mod should change **_as little as it can_** to have its desired effect so that multiple mods could even modify the same file.

#### Greed Merge Extensions

To that end, Greed introduces several json file extensions that allow targeted edits within a given file. For example, if you want to edit the Kol, you might name your file `trader_battle_capital_ship.unit.gmr`.

- `.gmr` **(Greed Merge Replace)**: for each element in the the mod file's arrays, it replaces the greed file's corresponding element at that index, per **Newtonsoft**'s `MergeArrayHandling.Replace`.
    - eg `[1, 2, 3]` + `[4, 5]` = `[4, 5, 3]`
    - [Example](https://github.com/VoltCruelerz/constituent-components/blob/master/entities/trader_heavy_gauss_slugs.unit_item.gmr) removing an element from an array
- `.gmc` **(Greed Merge Concatenate)**: for each element in the mod's array, it concatenates them onto the original array, per **Newtonsoft**'s `MergeArrayHandling.Concat`.
    - eg `[1, 2]` + `[2, 3]` = `[1, 2, 2, 3]`
    - [Example](https://github.com/VoltCruelerz/constituent-components/blob/master/entities/buff.entity_manifest.gmc) of adding elements to an entity_manifest
- `.gmu` **(Greed Merge Union)**: for each element in the mod's array that does not already exist in the original array, add it to the end of the array, per **Newtonsoft**'s `MergeArrayHandling.Union`.
    - eg `[1, 2]` + `[2, 3]` = `[1, 2, 3]`
    - [Example](https://github.com/VoltCruelerz/constituent-components/blob/master/entities/trader_antimatter_engine_unit_item.ability.gmu) of adding autocast

In all of these cases, if you leave an object's field undefined, it will not be edited. So, if you wanted to edit just one of the many arrays in a `.player` file to add a new ship type, you'd only declare the one array you want to edit, and you'd probably do it in a `.gmc` file so you can concatenate what you want to what's already there without redeclaring everything.

#### Null Removal

- If a field within your mod is null (not undefined, but actually `null`), it will **delete** that field from the greed file under construction. 
    - This applies regardless of whether you are using a `.gm*` file type or not.
- If inside an array in a `*.gmr`, it will delete the element at that original index from an array.
    - For example, in the array `[ 1, 2, 3, 4 ]`, if you want to remove the middle two elements, your mod would have `[ 1, null, null ]`, which would result in `[ 1, 4 ]`.

[Example](https://github.com/VoltCruelerz/constituent-components/blob/master/entities/trader_reserve_squadron_hangar.unit_item.gmr) removing a field from an item

> _**Note**: be very careful when removing elements by index that you know _exactly_ what is there already._

#### Localized Text

If `*.localized_text` files were actually key-value pairs like they _should_ be (and will be in a future update), you could just use `*.gmr`, but they're not. They're weird little arrays of size 2. Additionally, a mod must have the _full_ list of all strings for it to be legal. This complicates merging them, so I've created a custom solution for them:

You should write your localized text files as if they were truncated to just the things you add or update. Greed will automatically upsert them for you when the greed files are generated.

```json
{
    "text": [
        [
            "keyA",
            "This is the first string added. Maybe this one's key already exists, and I'm overriding."
        ],
        [
            "keyB",
            "This is another string. Maybe this one's key doesn't exist yet, so it's new."
        ]
    ]
}
```

[Here's](https://github.com/VoltCruelerz/krosov-prova-guns/blob/main/localized_text/core_en.localized_text) another example.

## Contributing to Greed

I welcome contributions. Just open a pull request.

If at some point in the future, I can no longer maintain Greed, it's MIT licensed.

### Bug Reports

In the event of an error, consult the log (viewable both in the textbox at the bottom of the frame and also in `log.txt`, found in whatever directory you've got Greed in).

- If the error appears to be one with Greed itself, please [create a GitHub issue](https://github.com/VoltCruelerz/Greed/issues/new).
    - Be sure to use an appropriate label and include the log when submitting so I can hunt down the error easier.
- Otherwise, contact the appropriate mod developer.

## Contributing Mods

To contribute a mod to the curated list, open a merge request [here](https://github.com/League-of-Greedy-Modders/Greedy-Mods).
