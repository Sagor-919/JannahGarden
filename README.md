# Jannah Garden

Unity project pinned to Editor **6000.3.9f1**, targeting Android with URP and the Input System. `Assets/Scenes/AssetDemo.unity` is the first enabled build scene; `SampleScene` remains enabled after it.

## Unity CLI and Codex workflow

The Unity CLI is installed for this Windows user. A project wrapper calls its installed binary directly, avoiding the `Unity.exe` name collision with Unity Editor installations:

```powershell
./scripts/unity.ps1 --version
./scripts/unity.ps1 editors list
./scripts/unity.ps1 status --project-path (Get-Location).Path
```

Open the project using the version pinned by `ProjectVersion.txt`:

```powershell
./scripts/unity.ps1 open .
```

The Codex Unity plugin is installed, and the Unity CLI skill is stored in `.agents/skills/unity-cli/`. Project-wide instructions are in `AGENTS.md`.

Android gameplay guidance is in `.agents/skills/android-unity-game-dev/SKILL.md`.

When a connected Editor is ready, use the Unity CLI Pipeline commands for live scene inspection, Play Mode, and iteration. The project includes `com.unity.pipeline` **0.8.0-exp.1**. This package is experimental; use the Unity CLI batch commands for builds and tests when live connection is unavailable.

## Build and test

Run tests when requested or when they directly verify a behavior change:

```powershell
./scripts/unity.ps1 test . --mode EditMode
./scripts/unity.ps1 test . --mode PlayMode
```

Build an Android APK into the ignored `Builds/` directory:

```powershell
New-Item -ItemType Directory -Force Builds/Android | Out-Null
./scripts/unity.ps1 build . --target Android --output-path Builds/Android/JannahGarden.apk
```

Choose another installed build target with `./scripts/unity.ps1 build --list-targets`. Add new scenes to Build Settings only when the game flow requires them.
