# Magic-RDR
Best RPF Editor for Red Dead Redemption
Supports only Xbox 360 files, also works if you use Xenia.
Some things can be buggy like viewing mipmaps or editing files so I recommend to make backups
My Discord : Im Foxxyyy#1111

# Supported RAGE Resources
- .xft - Fragments (Complex models, props)
- .xfd - Frag Drawable (Dictionary of multiple drawables, usually used for LODs)
- .xvd - Volume Data (Static meshes, mostly buildings)
- .xbd - Bounds Dictionary (Dictionary of static collisions)
- .xtd - Texture Dictionary (Dictionary of textures)
- .xsf - ScaleForm (UI textures, single mipmapped)
- .xsi - Sector Items (Bounding boxes hierarchy, objects positions, curves, etc)
- .xsc - Scripts (In-game scripts including story and online mode)
- .xst - StringTable (R* defined strings and subtitles)
- .fxc - Compiled Shader
- .awc - Audio container

# Features you can get
- Import, replace, remove files
- Import directory with multiple files
- Create, remove directories
- Extract files/resources (compressed or uncompressed) and directories
- Display files properties
- Copy files paths
- Save .rpf (sorting entries by offset is recommanded)
- View most text files and export (editing can be buggy depending on what file you want to edit)
- Search files and access their location
- Basic Hex viewer
- Hash generator
- Options to sort the list view (ascending by default/descending)
- Options to automatically load previous opened .rpf (by default)
- Custom background color for the model viewer

# Script decompiler (.xsc)
  - View most scripts
  - Export to C code

# Texture viewer & editor (.xtd, .xsf, .xft, .xfd, .xvd)
  - Export texture(s)
  - Import custom .dds with mipmaps
  - Supported formats at import : DXT1, DXT3, DXT5

# Model viewer (.xft, .xfd, .xvd, .xbd)
  - View buildings, peds, props, etc
  - View, import & export embedded texture(s)
  - View used shader parameters
  - View bones positions
  - View specific .xvd geometries
  - Show bounds, vertices and/or faces
  - Export models to .obj with vertex normals and texcoords included
  - Change in-game .xvd's models position (still in beta)
  - Export custom .xvd's to .xml for GTAV CodeWalker (still in beta)
  - Disable in-game static .xbd collisions (still in beta, file backups recommanded)

# Basic data viewer (.xsi)
  - View sector bounds
  - Display sector objects names and positions

# Basic shader data viewer (.fxc)
  - View shader parameters, rendering techniques

# Stringtable viewer (.xst)
  - View and export text
  - Colored RDR strings such as <yellow>Yellow text</yellow

# Audio player (.awc)
  - Play most sounds
  - Export sounds to .wav

# Credits
- Im Foxxyyy
- XBLToothPiick
- revelations
- apii intense
- aru

# Testers
- CabooseSayzWTF
- FrostDragonZ
