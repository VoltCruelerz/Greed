# Mutations

## Overview

Mutations are an advanced file manipulation tool that allows far more granular control than the `.gm*` extensions.

Rather than writing out what you wish to add to a file, you instead write out the list of precise alterations you wish to make.

For example, if you wanted to remove the TEC loyalist's useless Andvanced Fire Control research, you'd create something like the following...

```jsonc
// trader_loyalist.player.gmr
{
    "greed": {
        "mutations": [
            {
                // Filter the research subjects to those that are not the research we want to remove.
                "type": "FILTER",
                "path": "research.research_subjects[i]",
                "condition": {
                    "type": "NEQ",
                    "params": [ "$element_i", "trader_unlock_starbase_improve_weapon_item" ]
                }
            }
        ]
    }
}
```

## Execution Order

Mutations execute after the _Greed Merge_ (if applicable), and execute in the order they are provided.

## Action Paths

Mutations handle node exploration with something called _Action Paths_. Essentially, when you have a path to some field in the object, Action Paths allow you to do work on it. In the example above, the action path would be `research.research_subjects[i]`, which is broken up internally into three depths:

- Depth 0: `research` (field)
- Depth 1: `research_subjects` (field)
- Depth 2: `[i]` (array)

When Action Paths explore an object, they will navigate through fields and iterate through all array elements.

As another example, if you had `a[i][j][k].b`, that would generate the following path elements:

- Depth 0: `a` (field)
- Depth 1: `[i]` (array)
- Depth 2: `[j]` (array)
- Depth 3: `[k]` (array)
- Depth 4: `b` (field)

## Schemas

### Resolvable

Resolvables are objects that can be resolved from context to a specific value. They come in three flavors:

- `Mutation`: an operation that does some amount of logic
- `ConstantReference`: if you provide a string value, a number, or a boolean. In the Advanced Fire Control removal example, the name of the research to delete is a constant string.
- `VariableReference`: a reference to a variable, which will have its value populated _somehow_.

#### Truthiness

All resolvables can, in addition to their normal values, also be resolved to a boolean (`TRUE` or `FALSE`). This is known as _Truthiness_.

The following values resolve to `FALSE`. **All** others resolve to `TRUE`.

- `FALSE`
- `0`
- `""`
- `NULL`

### Variables

Variables are automatically-generated stores of data accessible to your Mutations. Variables are denoted in Mutation configurations as strings led by a `$` (eg `$element_i` above). While there are some operations that will generate more variables for your use, you will always have access to the following:

- `$root`: the root element of the json file

### Mutation

| Field | Type | Description | Default Value |
|:------|:-----|:------------|:--------------|
| `type`* | `MutationType` | The type of operation to perform, such as `NEQ`, `EQ`, or `FILTER` | |

#### Array Mutations

| Field | Type | Description | Default Value |
|:------|:-----|:------------|:--------------|
| `path`* | `string` | The path to the innermost array, eg `a.b[i].c[j][apple]`. For each _Array Action Path_ you pass, variables will be created for the specified index and the corresponding element.<br><br>For example, in the above example, the following variables would be accessible: <ul><li>`$i`</li><li>`$element_i`</li><li>`$j`</li><li>`$element_j`</li><li>`$apple`</li><li>`$element_apple`</li></ul>  | |
| `condition` | `Resolvable` | If the condition is truthy, the operation is applied. If not, it is not. Not all array operations require this field. | |
| `resolutionDepth` | `integer` | When working with an array operation that requires resolution (such as `FILTER`), this field specifies which _Action Path Depth_ the resolution event will be handled by. For example, if you have nested arrays that you are filtering, if you find a constraint violation in the inner array, this allows you to specify whether you want to remove the element from the inner array or from the outer array. | action path length - 1 |
| `index` | `integer` | When using `INSERT`, this specifies at what index you wish to insert the element. | -1 |
| `value` | `Resolvable` | When inserting a value into an array, this field is used. | `null` |

##### Array Operation Types

Array operations execute in-place and return the resulting array's length.

- `FILTER`: filters an array to only those where the condition is truthy.
- `APPEND`: adds the `value` element to the end of the array
- `INSERT`: inserts the `value` into the array at the position specified by the `index` field. Returns array length.
- `REPLACE`: when the condition is truthy, replace the current element with the `value`. Returns array length.
- **[TODO]** `DISTINCT`: `TRUE` if all parameters resolve to different values.
- **[TODO]** `INDEX_OF`: Returns the index if `value` is included in the array, or -1 if not.

#### Function Mutations

Functions are Mutations that are typically passed to other Mutations as parameters. They do not have side effects, meaning their primary purpose is to return a value. The notable exception to this is `SET`, which can be used to change the data structure.

| Field | Type | Description | Default Value |
|:------|:-----|:------------|:--------------|
| `params`* | `Resolvable[]` | The list of Resolvables that will be fed into this logical operation. | |

##### Logical Functions

- `AND`: `TRUE` if all parameters resolve to `TRUE`
- `OR`: `TRUE` if any parameter resolves to `TRUE`
- `XOR`: `TRUE` if exactly one parameter resolves to `TRUE`

##### Comparison Functions

- `EQ`: `TRUE` if the parameters all resolve to the same value
- `NEQ`: `TRUE` if params[0] != params[1]
- `NOT`: `TRUE` if params[0] resolves to `FALSE`
- `GT`: `TRUE` if params[0] > params[1]
- `GTE`: `TRUE` if params[0] >= params[1]
- `LT`: `TRUE` if params[0] < params[1]
- `LTE`: `TRUE` if params[0] <= params[1]

##### Parameter Set Functions

- `IN`: `TRUE` if params[0] is contained somewhere else within the list of parameters.
- `NIN`: `TRUE` if params[0] is NOT contained somewhere else within the list of parameters.

##### String Functions

- `SUBSTRING`: Takes the string at params[0] and creates a substring starting at params[1] (inclusive) and ending at params[2] (inclusive).
- `CONCAT`: Concatenates the parameters.

##### Variable Functions

- `SET`: Upserts a global-scope variable with the name of params[0] and the value of params[1]. Resolves to the variable's new value.
- `CLEAR`: Deletes a global-scope variable with the name found in params[0], if one exists. Resolves to the variable's value.
