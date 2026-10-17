---
name: android-unity-game-dev
description: Build and review Unity gameplay, controls, UI, lifecycle behavior, and player builds for Android mobile devices.
---

# Android Unity game development

Apply this skill to gameplay or UI work intended for Android in this project. Follow the repository's `AGENTS.md` and the Unity CLI skill for project-specific conventions and live Editor operations.

- Treat touch as the primary input. Reuse the imported Joystick Pack and connect it through the active EventSystem/input module. Keep gameplay input behind the project's input service so characters do not depend on a specific on-screen control.
- In the Jannah Garden prototype, route touches from the lower 30% of the screen to the floating movement joystick and upper-screen drags to the player-parented orbit camera. Clamp camera pitch at both ends and release held touch input whenever the game pauses or the app backgrounds.
- Anchor HUD elements to the safe area and use a Canvas Scaler configured for multiple aspect ratios. Make controls comfortable to tap, keep them clear of system cutouts and gesture areas, and use icons with adequate contrast.
- Keep this project's Android orientation locked to portrait. Keep the FPS counter inexpensive, and persist player-facing graphics toggles such as shadows, anti-aliasing, and render scale with platform runtime settings.
- Handle Android pause/resume and focus changes explicitly. Do not leave simulation time scaled to zero after an interruption; keep menu controls responsive while gameplay is paused.
- Keep movement independent of frame rate, use CharacterController collision movement for capsule characters, and enforce world bounds in a way that does not depend on visible boundary geometry.
- Preserve user-selected orientation, graphics APIs, and Android Player Settings unless the task requires a change. Check the installed Android build module before building; never introduce signing credentials into source control or command history.
- Profile on representative Android hardware before adding platform-specific optimization. Prefer measured reductions in overdraw, shader cost, allocations, and battery use over speculative complexity.
- Verify touch movement, boundary behavior, pause/resume, quit, app background/resume, and safe-area layout in Play Mode and on an Android device when available. Clearly distinguish Editor checks from device verification.
