# Contributing

Build on Windows with `powershell -File .\Build.ps1` and run `powershell -File .\Test.ps1` before submitting a change. The synthetic tests need no game installation.

Describe the problem, expected result, actual result and steps to reproduce it. Include your Windows version and selected asset/style. Remove personal filesystem paths from logs. Do not upload game resources, commercial fonts, other people's texture packs, `data/`, backups or save files.

Keep the interface in English. Preserve original texture dimensions, alpha, sprite anchors and bitmap-font metrics. Keep the rendered preview consistent with applied and exported textures. Apply/restore changes must retain transaction rollback and must not overwrite the game's original Resources directory.

Use a copy of a game installation for integration tests. A normal build or synthetic test run must not change a real installation. Run new visual styles at small and large dimensions and inspect transparency and edges.

Contributions are made under the repository's MIT License. Preserve existing copyright and license notices.
