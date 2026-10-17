# Jannah Garden: agent instructions

## Project facts

- This is a Unity project. Use the Editor version in `ProjectSettings/ProjectVersion.txt` (currently `6000.3.9f1`) and the pinned package versions in `Packages/manifest.json`.
- The project uses URP, the Input System, and Toony Colors Pro assets. Keep existing rendering, input, and asset workflows intact unless the task calls for changing them.
- `Assets/Scenes/AssetDemo.unity` is the first enabled build scene for the Android gameplay prototype; `SampleScene` remains enabled after it. Check `ProjectSettings/EditorBuildSettings.asset` before changing scene or build behavior.
- Keep Unity `.meta` files with their assets. Never regenerate or remove a `.meta` file for an existing asset; its GUID may be referenced by scenes, prefabs, and materials.

## Working in this project

- Treat the user's prompt and existing project assets as the source of truth for game design. Ask only when a missing choice blocks implementation; otherwise make a small, reversible choice and explain it.
- Put new gameplay code under `Assets/Scripts/`, with focused subfolders for features. Put editor-only tools under an `Editor` folder and tests under `Assets/Tests/EditMode` or `Assets/Tests/PlayMode`.
- Keep scripts focused, serialized references explicit, and runtime behavior testable without scene-specific hidden state. Use the Input System already installed in the project for new player input.
- Prefer existing art, materials, prefabs, and URP settings. Avoid importing large or licensed assets without the user's direction.
- Make scene, prefab, and other Unity asset edits through the project's running Editor and Unity Pipeline commands when that Editor is connected. Confirm `unity status --project-path .` identifies this checkout and the version in `ProjectVersion.txt` before sending commands. Do not control a different checkout or an Editor with a mismatched version.
- If no suitable Editor is connected, use the Unity CLI's batch commands for compilation, tests, and builds. Do not launch a second Editor against the same project while one is already open.
- Keep generated builds and test reports under `Builds/` or `Artifacts/`; both are ignored by Git.

## Android mobile development

- Treat Android touch input as the primary control path. Reuse the imported Joystick Pack and 300Mind UI assets where they fit; keep platform input behind `InputManager`.
- In this scene, a touch in the lower 30% of the screen controls the floating joystick; a drag in the rest of the screen orbits the player camera. Keep the orbit camera parented under the player, clamp its pitch, and clear active touches when pausing or backgrounding.
- Use safe-area-aware anchors and a Canvas Scaler for HUD controls. Keep touch targets comfortably sized and away from system bars and cutouts.
- Keep Android orientation locked to portrait. Persist player-facing graphics choices and avoid per-frame allocations in HUD counters.
- Handle application pause/resume without leaving `Time.timeScale` stuck at zero. Keep pause and quit UI interactive while gameplay is stopped.
- Preserve Android Player Settings and orientation unless the task needs a specific change. Check Android build support before building, never commit signing secrets, and distinguish Editor verification from a real device run.
- Read `.agents/skills/android-unity-game-dev/SKILL.md` for focused mobile gameplay and verification guidance.

## Feedback loop

1. Inspect the relevant scene, prefab, script, package, and build settings before editing.
2. Implement one coherent change at a time, preserving existing asset GUIDs and scene references.
3. For C# changes, check compilation with the connected Editor or a Unity CLI batch run. Run focused EditMode or PlayMode tests when the task changes behavior covered by tests.
4. For build requests, use the target and output the user specifies. If no target is specified, ask before producing a platform-specific deliverable.
5. Review the Unity Console or batch log for errors and summarize what was verified and any remaining limitation.

## Useful commands

Use the repository wrapper so Windows resolves the Unity CLI rather than an Editor executable named `Unity.exe`:

```powershell
./scripts/unity.ps1 --version
./scripts/unity.ps1 status --project-path (Get-Location).Path
./scripts/unity.ps1 open .
./scripts/unity.ps1 test . --mode EditMode
./scripts/unity.ps1 build . --target Android --output-path Builds/Android/JannahGarden.apk
```

The project-local Unity CLI skill is in `.agents/skills/unity-cli/`. The live Editor integration uses the experimental `com.unity.pipeline` package, pinned in the package manifest. Use it for live iteration only when `unity status` reports this project as ready.
