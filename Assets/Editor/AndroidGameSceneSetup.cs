using System.Collections.Generic;
using System.Linq;
using JannahGarden.Gameplay;
using JannahGarden.Input;
using JannahGarden.Player;
using JannahGarden.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace JannahGarden.EditorTools
{
    public static class AndroidGameSceneSetup
    {
        private const string JoystickPrefabPath = "Assets/StandardAssets/Joystick Pack/Prefabs/Dynamic Joystick.prefab";
        private const string UiSpriteSheetPath = "Assets/StandardAssets/300Mind/2D Game UI Kit/Sprites/UI-pack_Sprite_1.png";

        [MenuItem("Tools/Jannah Garden/Set Up Android Gameplay")]
        public static void ApplyToActiveScene()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
                throw new System.InvalidOperationException("Save the prepared scene before running the Android gameplay setup.");

            GameObject boundary = GameObject.Find("Cylinder");
            GameObject ground = GameObject.Find("Plane");
            Camera mainCamera = Camera.main;
            if (boundary == null || ground == null || mainCamera == null)
                throw new System.InvalidOperationException("The active scene must contain the prepared Cylinder, Plane, and Main Camera.");

            GameObject managers = GetOrCreateObject("Game Managers");
            GetOrAdd<GameStateManager>(managers);
            InputManager inputManager = GetOrAdd<InputManager>(managers);

            Renderer boundaryRenderer = boundary.GetComponent<Renderer>();
            if (boundaryRenderer != null)
                SetEnabled(boundaryRenderer, false);
            Collider boundaryCollider = boundary.GetComponent<Collider>();
            if (boundaryCollider != null)
                SetEnabled(boundaryCollider, false);

            GameObject player = GetOrCreateObject("Player");
            CharacterController controller = GetOrAdd<CharacterController>(player);
            SetControllerDimensions(controller);

            GameObject visual = player.transform.Find("Character Visual") != null
                ? player.transform.Find("Character Visual").gameObject
                : GameObject.CreatePrimitive(PrimitiveType.Capsule);
            if (visual.transform.parent != player.transform)
            {
                Undo.RegisterCreatedObjectUndo(visual, "Create Player Capsule Visual");
                visual.name = "Character Visual";
                Undo.SetTransformParent(visual.transform, player.transform, "Parent Player Capsule Visual");
            }
            CapsuleCollider visualCollider = visual.GetComponent<CapsuleCollider>();
            if (visualCollider != null)
                Undo.DestroyObjectImmediate(visualCollider);
            visual.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = new Vector3(0.62f, 0.9f, 0.62f);

            float groundY = ground.GetComponent<Renderer>() != null
                ? ground.GetComponent<Renderer>().bounds.max.y
                : ground.transform.position.y;
            player.transform.position = new Vector3(boundary.transform.position.x, groundY, boundary.transform.position.z);
            MobileCharacterMotor motor = GetOrAdd<MobileCharacterMotor>(player);
            Transform cameraPivot = GetOrCreateCameraPivot(player.transform);
            mainCamera.transform.SetParent(cameraPivot, false);
            mainCamera.transform.localPosition = new Vector3(0f, 0f, -4.5f);
            mainCamera.transform.localRotation = Quaternion.identity;
            mainCamera.transform.tag = "MainCamera";
            cameraPivot.localPosition = new Vector3(0f, 1.4f, 0f);
            cameraPivot.localRotation = Quaternion.Euler(18f, 0f, 0f);
            CameraFollowTarget oldCameraFollow = mainCamera.GetComponent<CameraFollowTarget>();
            if (oldCameraFollow != null)
                Undo.DestroyObjectImmediate(oldCameraFollow);
            motor.Configure(boundary.transform, mainCamera, visual.transform);
            EditorUtility.SetDirty(motor);

            Canvas canvas = GetOrCreateCanvas();
            Transform safeArea = GetOrCreateSafeArea(canvas.transform);
            EventSystem eventSystem = EnsureEventSystem();

            MobileTouchInputSurface touchSurface = GetOrCreateTouchSurface(canvas.transform);
            GameObject joystickObject = EnsureJoystick(canvas.transform);
            Joystick joystick = joystickObject.GetComponent<Joystick>();
            inputManager.BindMovementJoystick(joystick);
            touchSurface.Configure(joystick, cameraPivot);
            EditorUtility.SetDirty(touchSurface);
            EditorUtility.SetDirty(inputManager);

            Sprite pauseButtonSprite = LoadSprite("UI-pack_Sprite_1_8");
            Sprite quitSprite = LoadSprite("UI-pack_Sprite_1_79");
            Button pauseButton = CreateButton("Pause Play Button", safeArea, pauseButtonSprite,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-52f, -188f), new Vector2(132f, 132f));
            Button settingsButton = CreateButton("Settings Button", safeArea, pauseButtonSprite,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-52f, -52f), new Vector2(112f, 112f));
            Button quitButton = CreateButton("Quit Button", safeArea, quitSprite,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(52f, -52f), new Vector2(120f, 120f));

            ShapeIconGraphic pausePlayIcon = CreatePausePlayIcon(pauseButton.transform);
            CreateSettingsIcon(settingsButton.transform);
            MobileGameHud hud = GetOrAdd<MobileGameHud>(canvas.gameObject);
            hud.Configure(pauseButton, quitButton, pausePlayIcon, joystick);
            EditorUtility.SetDirty(hud);

            CreateFpsCounter(safeArea, canvas.gameObject);
            ConfigureSettings(canvas.transform, safeArea, settingsButton, quitSprite, mainCamera, out _);

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.Portrait;

            AddActiveSceneToBuildSettings(scene.path);
            Undo.RecordObject(eventSystem, "Set Up Android Gameplay");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Android gameplay setup complete: touch-zone joystick, orbit camera, portrait orientation, graphics settings, and mobile HUD.");
        }

        private static Transform GetOrCreateCameraPivot(Transform player)
        {
            Transform pivot = player.Find("Camera Orbit");
            if (pivot == null)
            {
                GameObject pivotObject = new GameObject("Camera Orbit");
                Undo.RegisterCreatedObjectUndo(pivotObject, "Create Camera Orbit");
                Undo.SetTransformParent(pivotObject.transform, player, "Parent Camera Orbit");
                pivot = pivotObject.transform;
            }

            return pivot;
        }

        private static MobileTouchInputSurface GetOrCreateTouchSurface(Transform canvas)
        {
            Transform existing = canvas.Find("Touch Input Surface");
            GameObject surfaceObject;
            if (existing == null)
            {
                surfaceObject = new GameObject("Touch Input Surface", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(MobileTouchInputSurface));
                Undo.RegisterCreatedObjectUndo(surfaceObject, "Create Touch Input Surface");
                Undo.SetTransformParent(surfaceObject.transform, canvas, "Parent Touch Input Surface");
            }
            else
            {
                surfaceObject = existing.gameObject;
            }

            RectTransform rect = (RectTransform)surfaceObject.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = GetOrAdd<Image>(surfaceObject);
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = true;
            surfaceObject.transform.SetAsFirstSibling();
            return GetOrAdd<MobileTouchInputSurface>(surfaceObject);
        }

        private static void SetControllerDimensions(CharacterController controller)
        {
            Undo.RecordObject(controller, "Configure Player Character Controller");
            controller.height = 1.8f;
            controller.radius = 0.32f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.stepOffset = 0.25f;
            controller.slopeLimit = 45f;
            controller.skinWidth = 0.04f;
            controller.minMoveDistance = 0.001f;
        }

        private static Canvas GetOrCreateCanvas()
        {
            GameObject canvasObject = GameObject.Find("Mobile HUD");
            if (canvasObject == null)
            {
                canvasObject = new GameObject("Mobile HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Undo.RegisterCreatedObjectUndo(canvasObject, "Create Mobile HUD");
            }

            Canvas canvas = GetOrAdd<Canvas>(canvasObject);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            CanvasScaler scaler = GetOrAdd<CanvasScaler>(canvasObject);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            GetOrAdd<GraphicRaycaster>(canvasObject);
            return canvas;
        }

        private static Transform GetOrCreateSafeArea(Transform canvas)
        {
            Transform safeArea = canvas.Find("Safe Area");
            if (safeArea == null)
            {
                GameObject safeAreaObject = new GameObject("Safe Area", typeof(RectTransform), typeof(SafeAreaFitter));
                Undo.RegisterCreatedObjectUndo(safeAreaObject, "Create Safe Area");
                safeAreaObject.transform.SetParent(canvas, false);
                safeArea = safeAreaObject.transform;
            }

            RectTransform rect = (RectTransform)safeArea;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            GetOrAdd<SafeAreaFitter>(safeArea.gameObject);
            return safeArea;
        }

        private static EventSystem EnsureEventSystem()
        {
            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create Event System");
                eventSystem = eventSystemObject.GetComponent<EventSystem>();
                eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            else
            {
                InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
                if (inputModule == null)
                {
                    StandaloneInputModule legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
                    if (legacyModule != null)
                        Undo.DestroyObjectImmediate(legacyModule);
                    inputModule = Undo.AddComponent<InputSystemUIInputModule>(eventSystem.gameObject);
                    inputModule.AssignDefaultActions();
                }
                else if (inputModule.actionsAsset == null)
                {
                    inputModule.AssignDefaultActions();
                }
            }

            return eventSystem;
        }

        private static GameObject EnsureJoystick(Transform parent)
        {
            GameObject prior = GameObject.Find("Movement Joystick");
            if (prior != null && prior.transform.parent != parent)
                Undo.DestroyObjectImmediate(prior);

            Transform existing = parent.Find("Movement Joystick");
            GameObject joystickObject;
            if (existing != null)
            {
                joystickObject = existing.gameObject;
            }
            else
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(JoystickPrefabPath);
                if (prefab == null)
                    throw new System.InvalidOperationException("Fixed Joystick prefab was not found at " + JoystickPrefabPath);

                joystickObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                Undo.RegisterCreatedObjectUndo(joystickObject, "Add Fixed Joystick");
                joystickObject.name = "Movement Joystick";
                Undo.SetTransformParent(joystickObject.transform, parent, "Parent Fixed Joystick");
            }

            RectTransform rect = joystickObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
            Image raycastImage = joystickObject.GetComponent<Image>();
            if (raycastImage != null)
                raycastImage.raycastTarget = false;
            Graphic[] joystickGraphics = joystickObject.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < joystickGraphics.Length; i++)
                joystickGraphics[i].raycastTarget = false;
            return joystickObject;
        }

        private static Button CreateButton(string name, Transform parent, Sprite sprite, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
        {
            Transform existing = parent.Find(name);
            GameObject buttonObject;
            if (existing == null)
            {
                buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                Undo.RegisterCreatedObjectUndo(buttonObject, "Create " + name);
                Undo.SetTransformParent(buttonObject.transform, parent, "Parent " + name);
            }
            else
            {
                buttonObject = existing.gameObject;
            }

            RectTransform rect = (RectTransform)buttonObject.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = GetOrAdd<Image>(buttonObject);
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = true;
            Button button = GetOrAdd<Button>(buttonObject);
            button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            return button;
        }

        private static ShapeIconGraphic CreatePausePlayIcon(Transform button)
        {
            Transform existing = button.Find("Pause Play Icon");
            GameObject iconObject;
            if (existing == null)
            {
                iconObject = new GameObject("Pause Play Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(ShapeIconGraphic));
                Undo.RegisterCreatedObjectUndo(iconObject, "Create Pause Play Icon");
                Undo.SetTransformParent(iconObject.transform, button, "Parent Pause Play Icon");
            }
            else
            {
                iconObject = existing.gameObject;
            }

            RectTransform rect = (RectTransform)iconObject.transform;
            rect.anchorMin = new Vector2(0.26f, 0.26f);
            rect.anchorMax = new Vector2(0.74f, 0.74f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            ShapeIconGraphic icon = GetOrAdd<ShapeIconGraphic>(iconObject);
            icon.color = Color.white;
            icon.raycastTarget = false;
            return icon;
        }

        private static ShapeIconGraphic CreateSettingsIcon(Transform button)
        {
            Transform existing = button.Find("Settings Icon");
            GameObject iconObject;
            if (existing == null)
            {
                iconObject = new GameObject("Settings Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(ShapeIconGraphic));
                Undo.RegisterCreatedObjectUndo(iconObject, "Create Settings Icon");
                Undo.SetTransformParent(iconObject.transform, button, "Parent Settings Icon");
            }
            else
            {
                iconObject = existing.gameObject;
            }

            RectTransform rect = (RectTransform)iconObject.transform;
            rect.anchorMin = new Vector2(0.26f, 0.26f);
            rect.anchorMax = new Vector2(0.74f, 0.74f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            ShapeIconGraphic icon = GetOrAdd<ShapeIconGraphic>(iconObject);
            icon.Shape = ShapeIconGraphic.IconShape.Settings;
            icon.color = Color.white;
            icon.raycastTarget = false;
            return icon;
        }

        private static void CreateFpsCounter(Transform parent, GameObject canvasObject)
        {
            Transform existing = parent.Find("FPS Counter");
            GameObject labelObject;
            if (existing == null)
            {
                labelObject = new GameObject("FPS Counter", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(MobileFpsCounter));
                Undo.RegisterCreatedObjectUndo(labelObject, "Create FPS Counter");
                Undo.SetTransformParent(labelObject.transform, parent, "Parent FPS Counter");
            }
            else
            {
                labelObject = existing.gameObject;
            }

            RectTransform rect = (RectTransform)labelObject.transform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -24f);
            rect.sizeDelta = new Vector2(190f, 48f);
            Text label = GetOrAdd<Text>(labelObject);
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 28;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.UpperCenter;
            label.color = Color.white;
            label.raycastTarget = false;
            label.text = "FPS --";
            MobileFpsCounter counter = GetOrAdd<MobileFpsCounter>(labelObject);
            counter.Configure(label);
            EditorUtility.SetDirty(canvasObject);
        }

        private static void ConfigureSettings(Transform canvas, Transform safeArea, Button openButton, Sprite closeSprite, Camera targetCamera, out MobileGraphicsSettings settings)
        {
            Transform existing = safeArea.Find("Graphics Settings Panel");
            GameObject panel;
            if (existing == null)
            {
                panel = new GameObject("Graphics Settings Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                Undo.RegisterCreatedObjectUndo(panel, "Create Graphics Settings Panel");
                Undo.SetTransformParent(panel.transform, safeArea, "Parent Graphics Settings Panel");
            }
            else
            {
                panel = existing.gameObject;
            }

            RectTransform panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = panelRect.anchorMax = panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(660f, 600f);
            Image panelImage = GetOrAdd<Image>(panel);
            panelImage.color = new Color(0.07f, 0.1f, 0.14f, 0.96f);
            panelImage.raycastTarget = true;
            panel.SetActive(true);

            Text title = CreateText("Settings Title", panel.transform, "Graphics Settings", 38, TextAnchor.MiddleCenter,
                new Vector2(0.08f, 0.78f), new Vector2(0.8f, 0.96f));
            title.fontStyle = FontStyle.Bold;
            Button closeButton = CreateButton("Close Settings Button", panel.transform, closeSprite,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-52f, -52f), new Vector2(84f, 84f));

            Toggle shadows = CreateGraphicsToggle("Shadows Toggle", panel.transform, "Shadows", 0.68f);
            Toggle antiAliasing = CreateGraphicsToggle("Anti Aliasing Toggle", panel.transform, "Anti-aliasing", 0.48f);
            Toggle renderScale = CreateGraphicsToggle("Render Scale Toggle", panel.transform, "High render scale", 0.28f);
            panel.SetActive(false);

            settings = GetOrAdd<MobileGraphicsSettings>(GameObject.Find("Mobile HUD"));
            settings.Configure(openButton, closeButton, panel, shadows, antiAliasing, renderScale);
            EditorUtility.SetDirty(settings);
            EditorUtility.SetDirty(targetCamera);
        }

        private static Toggle CreateGraphicsToggle(string name, Transform parent, string labelText, float y)
        {
            Transform existing = parent.Find(name);
            GameObject toggleObject;
            if (existing == null)
            {
                toggleObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Toggle));
                Undo.RegisterCreatedObjectUndo(toggleObject, "Create " + name);
                Undo.SetTransformParent(toggleObject.transform, parent, "Parent " + name);
            }
            else
            {
                toggleObject = existing.gameObject;
            }

            RectTransform rect = (RectTransform)toggleObject.transform;
            rect.anchorMin = new Vector2(0.1f, y);
            rect.anchorMax = new Vector2(0.9f, y + 0.13f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            Image background = GetOrAdd<Image>(toggleObject);
            background.color = new Color(0.12f, 0.16f, 0.21f, 1f);

            Text label = CreateText("Label", toggleObject.transform, labelText, 30, TextAnchor.MiddleLeft,
                new Vector2(0.04f, 0.05f), new Vector2(0.75f, 0.95f));
            label.color = Color.white;

            Transform existingCheck = toggleObject.transform.Find("Checkmark");
            GameObject checkObject;
            if (existingCheck == null)
            {
                checkObject = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                Undo.RegisterCreatedObjectUndo(checkObject, "Create Toggle Checkmark");
                Undo.SetTransformParent(checkObject.transform, toggleObject.transform, "Parent Toggle Checkmark");
            }
            else
            {
                checkObject = existingCheck.gameObject;
            }
            RectTransform checkRect = (RectTransform)checkObject.transform;
            checkRect.anchorMin = new Vector2(0.82f, 0.2f);
            checkRect.anchorMax = new Vector2(0.96f, 0.8f);
            checkRect.offsetMin = checkRect.offsetMax = Vector2.zero;
            Image checkImage = checkObject.GetComponent<Image>();
            checkImage.color = new Color(0.35f, 0.85f, 0.58f, 1f);
            checkImage.raycastTarget = false;

            Toggle toggle = GetOrAdd<Toggle>(toggleObject);
            toggle.targetGraphic = background;
            toggle.graphic = checkImage;
            toggle.isOn = true;
            toggle.navigation = new Navigation { mode = Navigation.Mode.None };
            return toggle;
        }

        private static Text CreateText(string name, Transform parent, string value, int size, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax)
        {
            Transform existing = parent.Find(name);
            GameObject textObject;
            if (existing == null)
            {
                textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                Undo.RegisterCreatedObjectUndo(textObject, "Create " + name);
                Undo.SetTransformParent(textObject.transform, parent, "Parent " + name);
            }
            else
            {
                textObject = existing.gameObject;
            }

            RectTransform rect = (RectTransform)textObject.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            Text label = GetOrAdd<Text>(textObject);
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = value;
            label.fontSize = size;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;
            return label;
        }

        private static Sprite LoadSprite(string spriteName)
        {
            return AssetDatabase.LoadAllAssetsAtPath(UiSpriteSheetPath)
                .OfType<Sprite>()
                .FirstOrDefault(sprite => sprite.name == spriteName);
        }

        private static GameObject GetOrCreateObject(string name)
        {
            GameObject existing = GameObject.Find(name);
            if (existing != null)
                return existing;

            GameObject created = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(created, "Create " + name);
            return created;
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            if (component != null)
                return component;

            return Undo.AddComponent<T>(target);
        }

        private static void SetEnabled(Renderer renderer, bool enabled)
        {
            Undo.RecordObject(renderer, "Hide Cylinder Boundary");
            renderer.enabled = enabled;
            EditorUtility.SetDirty(renderer);
        }

        private static void SetEnabled(Collider collider, bool enabled)
        {
            Undo.RecordObject(collider, "Disable Cylinder Collider");
            collider.enabled = enabled;
            EditorUtility.SetDirty(collider);
        }

        private static void AddActiveSceneToBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            EditorBuildSettingsScene activeScene = scenes.FirstOrDefault(entry => entry.path == scenePath);
            if (activeScene != null)
            {
                activeScene.enabled = true;
                scenes.Remove(activeScene);
            }
            else
            {
                activeScene = new EditorBuildSettingsScene(scenePath, true);
            }

            scenes.Insert(0, activeScene);
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
