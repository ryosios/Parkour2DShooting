using ParkourShooter.Runtime.Movement;
using ParkourShooter.Runtime.Audio;
using ParkourShooter.Runtime.Visuals;
using ParkourShooter.Runtime.Combat;
using ParkourShooter.Runtime.Bosses;
using ParkourShooter.Runtime.Cards;
using ParkourShooter.Runtime.Characters;
using ParkourShooter.Runtime.Enemies;
using ParkourShooter.Runtime.Score;
using ParkourShooter.Runtime.Skills;
using ParkourShooter.Runtime.UI;
using ParkourShooter.Runtime.Vfx;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ParkourShooter.Editor
{
    public static class ParkourSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";

        [MenuItem("Parkour Shooter/Rebuild First Playable Scene")]
        public static void RebuildFirstPlayableScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var gameRoot = CreateRoot("GameRoot");
            var managers = CreateChild(gameRoot, "Managers");
            CreateChild(managers, "GameManager");
            var scoreManager = CreateChild(managers, "ScoreManager").AddComponent<ScoreManager>();
            var cardManager = CreateChild(managers, "CardManager").AddComponent<CardManager>();
            CreateAudioManager(managers.transform);
            CreateChild(managers, "PoolManager");
            CreateChild(managers, "VfxManager").AddComponent<VfxManager>();

            var playerRoot = CreateChild(gameRoot, "PlayerRoot");
            var characterA = CreatePlayerCharacter(playerRoot.transform, scoreManager, "CharacterA", new Color(0.2f, 0.85f, 1f), SkillEffectType.AttackBoost);
            var characterB = CreatePlayerCharacter(playerRoot.transform, scoreManager, "CharacterB", new Color(0.45f, 1f, 0.35f), SkillEffectType.BulletClear);
            var characterC = CreatePlayerCharacter(playerRoot.transform, scoreManager, "CharacterC", new Color(1f, 0.55f, 0.25f), SkillEffectType.AttackBoostAndBulletClear);
            var activeCharacter = characterA;

            var enemyRoot = CreateChild(gameRoot, "EnemyRoot");
            CreateEnemy(enemyRoot.transform, "Enemy_00", new Vector3(6f, -1.4f, 0f), 2, 100);
            CreateEnemy(enemyRoot.transform, "Enemy_01", new Vector3(11f, 1.4f, 0f), 3, 150);
            CreateEnemy(enemyRoot.transform, "Enemy_02", new Vector3(17f, -1.4f, 0f), 4, 200);
            var bossRoot = CreateChild(gameRoot, "BossRoot");
            var boss = CreateBoss(bossRoot.transform, activeCharacter);

            var stageRoot = CreateChild(gameRoot, "StageRoot");
            var groundRoot = CreateChild(stageRoot, "GroundColliders");
            var grazeRoot = CreateChild(stageRoot, "GrazeAreas");
            CreateChild(stageRoot, "SpawnPoints");
            CreateBox(groundRoot.transform, "Ground_00", new Vector2(40f, 1f), new Vector3(10f, -2.5f, 0f), new Color(0.25f, 0.22f, 0.18f));
            CreateGraze(grazeRoot.transform, "WallGraze_00", GrazeType.Wall, new Vector2(5.5f, 6f), new Vector3(8f, 0.5f, 0f));
            CreateGraze(grazeRoot.transform, "CeilingGraze_00", GrazeType.Ceiling, new Vector2(13f, 1.6f), new Vector3(14f, 3.5f, 0f));

            var backgroundRoot = CreateChild(gameRoot, "BackgroundRoot");
            CreateBackgroundLayer(backgroundRoot.transform, "Far", 80f, new Color(0.08f, 0.1f, 0.16f), 145f, 54f);
            CreateBackgroundLayer(backgroundRoot.transform, "Mid", 45f, new Color(0.13f, 0.16f, 0.22f), 105f, 36f);
            CreateBackgroundLayer(backgroundRoot.transform, "Near", 20f, new Color(0.18f, 0.18f, 0.2f), 68f, 20f);

            var cameraRoot = CreateChild(gameRoot, "CameraRoot");
            var cinemachineCamera = CreateCamera(cameraRoot.transform, activeCharacter);
            var teamController = CreateTeamController(playerRoot, new[] { characterA, characterB, characterC }, cinemachineCamera, boss);
            ConfigureCardManager(cardManager, scoreManager, teamController);
            var uiRoot = CreateChild(gameRoot, "UIRoot");
            CreateHud(uiRoot.transform, scoreManager, cardManager, teamController, boss);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static GameObject CreateRoot(string name)
        {
            return new GameObject(name);
        }

        private static GameObject CreateChild(GameObject parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent.transform);
            child.transform.localPosition = Vector3.zero;
            return child;
        }

        private static AudioManager CreateAudioManager(Transform parent)
        {
            var audioObject = new GameObject("AudioManager");
            audioObject.transform.SetParent(parent);
            audioObject.transform.localPosition = Vector3.zero;

            var bgmSource = CreateAudioSource(audioObject.transform, "BgmSource", true);
            var seSource = CreateAudioSource(audioObject.transform, "SeSource", false);
            var voiceSource = CreateAudioSource(audioObject.transform, "VoiceSource", false);
            var audioManager = audioObject.AddComponent<AudioManager>();

            var serializedAudio = new SerializedObject(audioManager);
            serializedAudio.FindProperty("bgmSource").objectReferenceValue = bgmSource;
            serializedAudio.FindProperty("seSource").objectReferenceValue = seSource;
            serializedAudio.FindProperty("voiceSource").objectReferenceValue = voiceSource;
            serializedAudio.ApplyModifiedPropertiesWithoutUndo();
            return audioManager;
        }

        private static AudioSource CreateAudioSource(Transform parent, string name, bool loop)
        {
            var sourceObject = new GameObject(name);
            sourceObject.transform.SetParent(parent);
            sourceObject.transform.localPosition = Vector3.zero;

            var source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            return source;
        }

        private static Transform CreatePlayerCharacter(
            Transform parent,
            ScoreManager scoreManager,
            string name,
            Color color,
            SkillEffectType skillEffect)
        {
            var player = new GameObject(name);
            player.transform.SetParent(parent);
            player.transform.position = new Vector3(-5f, -1.25f, 0f);

            var body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 3.2f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            var collider = player.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.8f, 1.4f);

            var sprite = player.AddComponent<SpriteRenderer>();
            sprite.sprite = CreateSprite();
            sprite.color = color;
            sprite.sortingOrder = 10;
            player.transform.localScale = new Vector3(0.8f, 1.4f, 1f);

            var motor = player.AddComponent<ParkourPlayerMotor2D>();
            var visual = player.AddComponent<SimplePlayerVisual>();
            var muzzle = CreateChild(player, "Muzzle");
            muzzle.transform.localPosition = new Vector3(0.75f, 0.15f, 0f);
            var autoAttack = player.AddComponent<AutoAttackController2D>();
            var skill = player.AddComponent<SkillController2D>();

            var serializedVisual = new SerializedObject(visual);
            serializedVisual.FindProperty("motor").objectReferenceValue = motor;
            serializedVisual.ApplyModifiedPropertiesWithoutUndo();

            var serializedMotor = new SerializedObject(motor);
            serializedMotor.FindProperty("scoreManager").objectReferenceValue = scoreManager;
            serializedMotor.FindProperty("grazeScorePerSecond").floatValue = 20f;
            serializedMotor.ApplyModifiedPropertiesWithoutUndo();

            var serializedAutoAttack = new SerializedObject(autoAttack);
            serializedAutoAttack.FindProperty("muzzle").objectReferenceValue = muzzle.transform;
            serializedAutoAttack.FindProperty("fireRate").floatValue = 3f;
            serializedAutoAttack.FindProperty("projectileSpeed").floatValue = 18f;
            serializedAutoAttack.FindProperty("projectileLifetime").floatValue = 2.5f;
            serializedAutoAttack.ApplyModifiedPropertiesWithoutUndo();

            var serializedSkill = new SerializedObject(skill);
            serializedSkill.FindProperty("effectType").enumValueIndex = (int)skillEffect;
            serializedSkill.FindProperty("autoAttack").objectReferenceValue = autoAttack;
            serializedSkill.FindProperty("durationSeconds").floatValue = 4f;
            serializedSkill.FindProperty("cooldownSeconds").floatValue = 8f;
            serializedSkill.FindProperty("attackDamageMultiplier").floatValue = 2f;
            serializedSkill.FindProperty("additionalProjectiles").intValue = 1;
            serializedSkill.ApplyModifiedPropertiesWithoutUndo();

            return player.transform;
        }

        private static TeamController2D CreateTeamController(
            GameObject playerRoot,
            Transform[] characters,
            CinemachineCamera cinemachineCamera,
            Boss2D boss)
        {
            var teamController = playerRoot.AddComponent<TeamController2D>();
            var serializedTeam = new SerializedObject(teamController);
            var charactersProperty = serializedTeam.FindProperty("characters");
            charactersProperty.arraySize = characters.Length;
            for (var i = 0; i < characters.Length; i++)
            {
                charactersProperty.GetArrayElementAtIndex(i).objectReferenceValue = characters[i];
            }

            serializedTeam.FindProperty("followCamera").objectReferenceValue = cinemachineCamera;
            serializedTeam.FindProperty("boss").objectReferenceValue = boss;
            serializedTeam.FindProperty("transitionSeconds").floatValue = 0.18f;
            serializedTeam.ApplyModifiedPropertiesWithoutUndo();
            return teamController;
        }

        private static void ConfigureCardManager(
            CardManager cardManager,
            ScoreManager scoreManager,
            TeamController2D teamController)
        {
            var serializedCardManager = new SerializedObject(cardManager);
            serializedCardManager.FindProperty("scoreManager").objectReferenceValue = scoreManager;
            serializedCardManager.FindProperty("teamController").objectReferenceValue = teamController;

            var cardsProperty = serializedCardManager.FindProperty("cards");
            cardsProperty.arraySize = 4;
            ConfigureCard(cardsProperty.GetArrayElementAtIndex(0), "Attack Up I", 100, CardEffectType.AttackUp, 1f);
            ConfigureCard(cardsProperty.GetArrayElementAtIndex(1), "Move Speed Up I", 250, CardEffectType.MoveSpeedUp, 1f);
            ConfigureCard(cardsProperty.GetArrayElementAtIndex(2), "Projectile Count Up I", 450, CardEffectType.ProjectileCountUp, 1f);
            ConfigureCard(cardsProperty.GetArrayElementAtIndex(3), "Graze Bonus I", 700, CardEffectType.GrazeBonus, 0.5f);
            serializedCardManager.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureCard(
            SerializedProperty cardProperty,
            string cardName,
            int scoreThreshold,
            CardEffectType effectType,
            float value)
        {
            cardProperty.FindPropertyRelative("cardName").stringValue = cardName;
            cardProperty.FindPropertyRelative("scoreThreshold").intValue = scoreThreshold;
            cardProperty.FindPropertyRelative("effectType").enumValueIndex = (int)effectType;
            cardProperty.FindPropertyRelative("value").floatValue = value;
        }

        private static void CreateHud(
            Transform parent,
            ScoreManager scoreManager,
            CardManager cardManager,
            TeamController2D teamController,
            Boss2D boss)
        {
            var canvasObject = new GameObject("HUD");
            canvasObject.transform.SetParent(parent);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            var scoreText = CreateHudText(canvasObject.transform, "ScoreText", new Vector2(16f, -16f), "Score: 0");
            var activeCharacterText = CreateHudText(canvasObject.transform, "ActiveCharacterText", new Vector2(16f, -44f), "Active: CharacterA");
            var bossHpText = CreateHudText(canvasObject.transform, "BossHpText", new Vector2(16f, -72f), "Boss HP: -");
            var cardText = CreateHudText(canvasObject.transform, "CardText", new Vector2(16f, -100f), "Cards: -");

            var hud = canvasObject.AddComponent<GameHudUI>();
            var serializedHud = new SerializedObject(hud);
            serializedHud.FindProperty("scoreManager").objectReferenceValue = scoreManager;
            serializedHud.FindProperty("cardManager").objectReferenceValue = cardManager;
            serializedHud.FindProperty("teamController").objectReferenceValue = teamController;
            serializedHud.FindProperty("boss").objectReferenceValue = boss;
            serializedHud.FindProperty("scoreText").objectReferenceValue = scoreText;
            serializedHud.FindProperty("activeCharacterText").objectReferenceValue = activeCharacterText;
            serializedHud.FindProperty("bossHpText").objectReferenceValue = bossHpText;
            serializedHud.FindProperty("cardText").objectReferenceValue = cardText;
            serializedHud.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Text CreateHudText(Transform parent, string name, Vector2 anchoredPosition, string text)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent);

            var rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(520f, 26f);

            var uiText = textObject.AddComponent<Text>();
            uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            uiText.fontSize = 20;
            uiText.alignment = TextAnchor.MiddleLeft;
            uiText.color = new Color(0.95f, 0.96f, 1f);
            uiText.text = text;
            return uiText;
        }

        private static void CreateBox(Transform parent, string name, Vector2 size, Vector3 position, Color color)
        {
            var box = new GameObject(name);
            box.transform.SetParent(parent);
            box.transform.position = position;
            box.transform.localScale = new Vector3(size.x, size.y, 1f);
            box.AddComponent<BoxCollider2D>().size = Vector2.one;
            var sprite = box.AddComponent<SpriteRenderer>();
            sprite.sprite = CreateSprite();
            sprite.color = color;
        }

        private static void CreateGraze(Transform parent, string name, GrazeType type, Vector2 size, Vector3 position)
        {
            var graze = new GameObject(name);
            graze.transform.SetParent(parent);
            graze.transform.position = position;
            graze.transform.localScale = new Vector3(size.x, size.y, 1f);

            var collider = graze.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var area = graze.AddComponent<GrazeArea2D>();
            var serializedArea = new SerializedObject(area);
            serializedArea.FindProperty("grazeType").enumValueIndex = (int)type;
            serializedArea.FindProperty("attractionStrength").floatValue = type == GrazeType.Wall ? 14f : 10f;
            serializedArea.ApplyModifiedPropertiesWithoutUndo();

            var sprite = graze.AddComponent<SpriteRenderer>();
            sprite.sprite = CreateSprite();
            sprite.color = type == GrazeType.Wall
                ? new Color(1f, 0.35f, 0.65f, 0.35f)
                : new Color(0.5f, 0.9f, 1f, 0.35f);
            sprite.sortingOrder = -1;
        }

        private static void CreateEnemy(Transform parent, string name, Vector3 position, int hp, int score)
        {
            var enemy = new GameObject(name);
            enemy.transform.SetParent(parent);
            enemy.transform.position = position;
            enemy.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            var body = enemy.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            var collider = enemy.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var renderer = enemy.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateSprite();
            renderer.color = new Color(0.95f, 0.24f, 0.18f);
            renderer.sortingOrder = 8;

            var enemyComponent = enemy.AddComponent<Enemy2D>();
            var serializedEnemy = new SerializedObject(enemyComponent);
            serializedEnemy.FindProperty("maxHp").intValue = hp;
            serializedEnemy.FindProperty("scoreValue").intValue = score;
            serializedEnemy.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Boss2D CreateBoss(Transform parent, Transform followTarget)
        {
            var boss = new GameObject("Boss_00");
            boss.transform.SetParent(parent);
            boss.transform.position = followTarget.position + new Vector3(10f, 1.5f, 0f);
            boss.transform.localScale = new Vector3(1.8f, 2.4f, 1f);

            var body = boss.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            var collider = boss.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var renderer = boss.AddComponent<SpriteRenderer>();
            renderer.sprite = CreateSprite();
            renderer.color = new Color(0.58f, 0.18f, 0.85f);
            renderer.sortingOrder = 9;

            var muzzle = CreateChild(boss, "Muzzle");
            muzzle.transform.localPosition = new Vector3(-0.65f, 0.1f, 0f);

            var bossComponent = boss.AddComponent<Boss2D>();
            var serializedBoss = new SerializedObject(bossComponent);
            serializedBoss.FindProperty("followTarget").objectReferenceValue = followTarget;
            serializedBoss.FindProperty("muzzle").objectReferenceValue = muzzle.transform;
            serializedBoss.FindProperty("maxHp").intValue = 30;
            serializedBoss.FindProperty("fireRate").floatValue = 1.2f;
            serializedBoss.FindProperty("bulletSpeed").floatValue = 8f;
            serializedBoss.FindProperty("bulletLifetime").floatValue = 5f;
            serializedBoss.FindProperty("scoreValue").intValue = 1000;
            serializedBoss.ApplyModifiedPropertiesWithoutUndo();
            return bossComponent;
        }

        private static void CreateBackgroundLayer(Transform parent, string name, float z, Color color, float width, float height)
        {
            var layer = new GameObject(name);
            layer.transform.SetParent(parent);
            layer.transform.position = new Vector3(8f, 0f, z);
            layer.transform.localScale = new Vector3(width, height, 1f);
            var sprite = layer.AddComponent<SpriteRenderer>();
            sprite.sprite = CreateSprite();
            sprite.color = color;
            sprite.sortingOrder = -20;
        }

        private static CinemachineCamera CreateCamera(Transform parent, Transform followTarget)
        {
            var cameraObject = new GameObject("MainCamera");
            cameraObject.transform.SetParent(parent);
            cameraObject.transform.position = new Vector3(4f, 0f, -17f);
            cameraObject.tag = "MainCamera";

            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = false;
            camera.fieldOfView = 38f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 150f;
            camera.backgroundColor = new Color(0.06f, 0.08f, 0.12f);
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<CinemachineBrain>();

            var cinemachineCameraObject = CreateChild(parent.gameObject, "CinemachineCamera");
            cinemachineCameraObject.transform.position = followTarget.position + new Vector3(4f, 0f, -17f);
            var cinemachineCamera = cinemachineCameraObject.AddComponent<CinemachineCamera>();
            cinemachineCamera.Follow = followTarget;
            cinemachineCamera.Lens.ModeOverride = LensSettings.OverrideModes.Perspective;
            cinemachineCamera.Lens.FieldOfView = 38f;
            cinemachineCamera.Lens.NearClipPlane = 0.3f;
            cinemachineCamera.Lens.FarClipPlane = 150f;

            var follow = cinemachineCameraObject.AddComponent<CinemachineFollow>();
            follow.FollowOffset = new Vector3(4f, 0f, -17f);
            return cinemachineCamera;
        }

        private static Sprite CreateSprite()
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        }
    }
}
