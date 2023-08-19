# Magic-RDR v1.3.0

RPF Editor for Red Dead Redemption
Supports Xbox 360 & Nintendo Switch versions.

(The Nintendo Switch beta version can be found in the release tab)

# RAGE Resources
- #ft - Fragments (Complex models, props)
- #fd - Frag Drawable (Dictionary of multiple drawables, usually used for LODs)
- #vd - Volume Data (Static meshes, mostly buildings)
- #bd - Bounds Dictionary (Dictionary of static collisions)
- #td - Texture Dictionary (Dictionary of textures)
- #sf - ScaleForm (UI textures, single mipmapped)
- #si - Sector Items (Bounding boxes hierarchy, objects positions, curves, etc)
- #sc - Scripts (In-game scripts including story and online mode)
- #st - StringTable (R* defined strings and subtitles)
- .fxc, .nvn - Compiled Shader
- .awc - Audio container
- More informations in https://rage.re/docs?topic=28

# Features
 - Import/replace, remove/add files
 - Remove/add directories
 - Import directory
 - Extract regular files, resources and directories
 - Show files properties
 - Copy files paths
 - Save/create new .RPF
 - View text files and export
 - Search files and access their location
 - Basic Hex viewer
 - Hash generator
 - Sort entries (ascending/descending)
 - Open previous .RPF by default (if still exists)
 - Script decompiler (#sc)
 - Texture viewer & editor (#td, #sf, #ft, #fd, #vd)
 - Model viewer (#ft, #fd, #vd, #bd)
 - Basic data viewer (#si)
 - Basic shader data viewer (.fxc, .nvn)
 - Stringtable viewer (#st)
 - Audio player (.awc)

# Notes
If you want to edit regular files such as .xml, the best way to do it is to extract it, to use a text editor such as Notepad++. Then when you're good, save it and import it back in the .RPF. Don't forget to check the 'Compress' checkbox when you import it.
The compress option is necessary for all the non-resource files. You can see if it's a resource by clicking on the file properties in the viewer.

# Credits
- Im Foxxyyy
- XBLToothPiick
- revelations
- Sockstress
- apii intense
- aru

# Testers
- CabooseSayzWTF
- GuiCORLEONEx794
- FrostDragonZ
