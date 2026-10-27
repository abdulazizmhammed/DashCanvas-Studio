# DashCanvas Studio 3

An English-language Windows texture designer for Geometry Dash. It indexes your installed game assets (the count depends on the game version): individual sprites, standalone images, player-icon layers, bitmap-font characters and full sheets. Quality variants are grouped together.

Copyright (c) 2026 **abdulazizmhammed**. Licensed under [MIT](../LICENSE). See [third-party notices](../THIRD_PARTY_NOTICES.md).

## Use it

1. Open DashCanvas.exe. The game folder is detected automatically.
2. Choose an item from a category, a clickable preview, or search.
3. Choose a picture, or click **Use current** to decorate the existing texture. Use the **Picture**, **Text & Fonts** and **Frames** tabs to style it.
4. Close Geometry Dash, click **Apply selected**, and reopen the game.

Keep the executable in this folder so preferences and backups stay together.

## Find your items

**Level Browser** opens by default. Its 15 buttons are Create Levels, Saved Levels, Leaderboards, Quests, Daily Level, Weekly Demon, Event Level, Gauntlets, Featured Levels, Level Lists, Paths, Map Packs, Search Levels, Adventure Map and Versus. Click the square in **Level browser preview** to customize it.

**Folders & Lists** contains folder buttons, folder icons, list controls and headings. **Stars & Ratings** contains star icons and rating buttons. Both have direct shortcut buttons.

Other categories cover Main Menu, Account & Social, Logos, Buttons, Difficulty Faces, Player Icons, Backgrounds, Grounds, Level Objects, Level Editor, Effects, Fonts & Characters, Interface & Other, Other Images and Full Sheets (Advanced). **My Changes** lists your customized items.

Search covers **all categories**, even when another category is selected. Try `saved`, `played`, `folder`, `demon list`, `star`, `featured`, `map packs`, `extreme` or `cube 01`. The original filename is also searchable and appears in the item's tooltip.

Main-menu preview keeps the profile on the left and Play in the center. Both scene previews are illustrative; they are not a live rendering of the running game.

## Picture styles

Six choices: Original shape, Rectangle, Circle, Rounded, Diamond and Hexagon. Overlay choices include Search, Play, Account, Star and Original badge. For example, keep a search lens above your picture or a saved-folder badge in the corner. Symbols can sit in the center, bottom-right or top-right.

Auto crop estimates a focal point using image detail. Processing stays local: no account, API key, cloud AI or internet is needed. Input formats are PNG, JPEG, BMP, GIF (one frame) and TIFF. Convert WebP, HEIC or AVIF to PNG/JPEG first.

The app preserves sprite dimensions, transparency, rotation handling, logical sizes and anchors. It keeps the existing position of controls in the game. Some player icons and animations contain separate base, secondary, detail, glow and animation-frame images; edit those layers individually. Game tint, scaling and animation can affect the appearance.

The catalogue covers the installed game's image assets. The contents of a particular online level list, remotely fetched level thumbnails, sounds and game logic are outside texture editing.

## Text, fonts and decorative frames

**Text & Fonts** adds a label to any selected texture. Choose an installed Windows font, bold or italic, a custom color, and Solid, Outline, Shadow, Neon or Gold effects. Placement and size controls keep the text inside the texture.

Click **Browse game fonts** to select a bitmap-font character. **Redraw glyph** replaces that character using the selected font and color. **Style entire font** styles all supported characters in the selected game font together; **Restore font** removes those character edits while preserving your other designs. Whole-font styling uses the font controls, without picture overlays or decorative frames. Existing character boxes, spacing and advance metrics are retained, so replacement letter shapes are fitted into the original slots. The game's own text tint can affect the final colors.

**Frames** offers Neon, Double Neon, Antique Gold, Carved Wood, Ornate Corners and Scrollwork, alongside plain or no border. Choose a color, thickness and glow strength. Preset buttons provide cyan or pink neon and vintage gold, wood and scrollwork looks. Frames and glow stay inside the original texture dimensions.

All changes appear in **Your design** before you apply them. You can decorate the current game texture without uploading a picture; click **Use current** to switch back after choosing a photo.

## Layers in Your design

Use the layer selector under the right-hand preview to choose **Image**, **Symbol**, **Text** or **Frame**. The list runs from front to back. **Forward** and **Back** move one visible layer at a time; **More** contains **Bring to front**, **Send to back**, and a reset for layer order and opacity. Empty layers and deleted layers are marked in the list.

**Delete** removes the selected layer from the design. **Restore** brings it back at its previous position. The opacity control runs from 0% (transparent) to 100% (opaque). Changes appear immediately, and **Apply selected** or **Export pack ZIP** uses the same layer order and opacity. Deleting the image layer leaves other elements on a transparent background. Deleting every layer produces an empty, transparent texture.

Layer controls edit the current design. Applying an edit keeps its source image available while you continue editing the same selected asset, so deleting a layer still removes that layer from the draft. Switching assets, reopening the app, or explicitly choosing Use current starts from the current flattened texture. The existing game texture is a single image layer; its embedded artwork cannot be separated into layers. Applied/exported textures are flattened images, and editable layer settings are not saved as a separate project for reopening.

In **Frames**, **Fit frame to picture** follows the picture's actual visible edge, including rounded corners, custom transparent silhouettes and holes. It also handles **Fit whole picture** with empty margins. The outer edge stays aligned when you change width, glow or frame style; thickness and neon glow extend inward. Leave **Inset %** at 0 for an edge-aligned frame, or increase it for an intentional inner border. **Match picture color** selects a prominent image color for the frame. Corner ornaments are clipped to the picture so they cannot stick out beyond its edge.

The editor scrolls in smaller windows so the preview and layer controls keep their usable size.

## Apply, restore and export

Geode Texture Loader is required for automatic application. DashCanvas writes its own pack, displayed as **DashCanvas Studio**, at first priority. Its folder remains named `DashCanvas Backgrounds` for compatibility with the first version.

Original Resources files are never overwritten. Individual sprite changes rebuild complete atlases from the underlying active packs. Unchanged sprites retain their pixels and anchors. Multiple edits in the same atlas are combined.

**Restore selected** removes one edit, preserving your other edits. Restoring the last item removes DashCanvas from the applied pack list. The app blocks conflicting full-sheet and individual edits in one sheet. If you change underlying packs externally, reopen DashCanvas and reapply an edit in affected sheets to refresh inherited artwork.

**Export pack ZIP** includes all applied designs plus the currently previewed design when an item is selected, including designs made with **Use current**. It includes required texture metadata and excludes internal project bookkeeping. Import it through Texture Loader.

## Backups

Every apply/restore stores recovery files in `data/backups/<timestamp>/` beside the executable. `journal.json` records exact destination paths, prior file contents and numbered backup files. Failed writes attempt rollback. Keep these backups if you need earlier artwork or settings.

For manual recovery, close the game and restore numbered backup files to the destinations recorded in the journal. Entries with `existed: false` had no file before that operation. Restoring `saved.json` restores the complete pack list from that moment.

## Validation and build

The English interface was rendered and visually checked. Twenty-four integration checks passed against a separate test installation: all three quality sizes, real rotated atlases, buttons, difficulty faces, logo and font edits, unchanged neighboring pixels and anchors, combined edits, conflict handling, ZIP export, selective restore and unchanged game Resources. Live gameplay has not been tested. No test image was applied to the real game during development.

Twenty-five additional style checks passed on a test installation: frame dimensions, text effects and transparency, decorating without a picture, export, whole-font styling across quality variants, preserved font metadata, selective font restore and unchanged original Resources. Neon and vintage frame interfaces were also rendered and visually checked.

Twenty-seven layer checks passed for ordering, deletion/restoration, opacity, frame alignment, palette matching, inset spacing, exact preview-to-applied pixels, export and usable controls at the minimum window size. The 24 atlas checks and 25 style checks also passed again after the layer update. Validation used test installations, not live gameplay.

The frame alignment correction was visually checked and tested across 210 combinations of shapes, frame styles, widths and texture sizes. Checks cover fixed outer edges, transparency, fitted pictures, explicit inset spacing and fully transparent pictures.

Source is in `source/`. Build.ps1 uses the .NET Framework compiler included with Windows and downloads no dependencies.

References: [Texture Loader pack format](https://github.com/geode-sdk/textureldr), [pack activation](https://github.com/geode-sdk/textureldr/blob/main/src/PackManager.cpp), [Cocos2d sprite metadata](https://github.com/cocos2d/cocos2d-x/blob/v2/cocos2dx/sprite_nodes/CCSpriteFrameCache.cpp).

## New in Studio 3

The Frames list contains 17 styles (plus None). New choices are Gradient, Rainbow, Chrome, Copper, Beveled, Double Line, Triple Line, Pearls, Marquee and Candy Stripe.

**Frame details** provides Solid, Dashed, Dotted and Dash-dot patterns, pattern spacing, a second color and gradient direction. Sunset, Ice Chrome, Pearl, Candy and Dashed Gold presets set a coordinated group of controls. Spacing is measured relative to frame width. Patterns break the line into intentional gaps while keeping it attached to the picture edge.

**Text & Fonts** now accepts multiple lines: press Enter in Label text. Its nine effects include Gradient, Double Outline, Emboss and Engraved. **Text details** adds left/center/right alignment, line spacing, a second gradient color, outline color/width, underline and strikethrough. These label controls do not change the game's font spacing metrics.

**About** and Windows executable properties identify the author as abdulazizmhammed. LICENSE contains the MIT terms. Use Test.ps1 for synthetic tests that do not require the game.
Studio 3 validation: 161 synthetic decoration checks passed; whole-font and restore checks passed on a separate test installation; the advanced tabs, presets and About attribution were rendered and checked.
