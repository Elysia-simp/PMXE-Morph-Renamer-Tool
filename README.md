# I am not liable for how badly you messed up using this

# Usage
You run the Plug-in and then you're prompted a UI to import a json to export to json. (You may need to unblock on Windows 10 and lower)

The json is structured as following:

`{"MorphToRename" : ["JPName", "EngishName", "FacialType"]}`


MorphToRename isn't too strict if you have a morph called 0006_Brow_morph0 but in the json you have it as Brow_morph0 It'll rename as it's largely the same.

Once you're satisfied with the modifications made to the json you can save and load and watch it work!

You can see an example of this [here](https://gist.github.com/Elysia-simp/14f10e4726f04014894630f0bc917d9c).

Valid facial types are as follow:

```
"hidden"

"brow"

"eye"

"mouth"

"other" 
```
# Bone Morphs

This addon can support bone morphs from this modified fork of [Minmode's noesis script](https://github.com/Elysia-simp/noesis-pmx)

You can probably make your own jsons in other programs as long as they are put into mmd space... But that's not on me lmfao.

There is a scale factor for if you scaled your models prior to running the addon.

# Build

You can download from [release](https://github.com/Elysia-simp/PMXE-Morph-Renamer-Tool/releases/tag/Release).

However, if you're paranoid you can build by simply including PEPlugin and SlimDX from PMXE in references and fixing NuGet packages.