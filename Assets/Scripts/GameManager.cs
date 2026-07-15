using UnityEngine;
using System;

namespace NeonShooter
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public static Vector2 ScreenSize { get; private set; }
        public static float TotalTime { get; private set; }
        public static float DeltaTime { get; private set; }
        public static ParticleManager<ParticleState> ParticleManager { get; private set; }
        public static Grid Grid { get; private set; }

        public Texture2D PixelTexture { get; private set; }

        bool paused = false;
        bool useBloom = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitializeGame()
        {
            GameObject gameManagerObj = new GameObject("GameManager");
            Instance = gameManagerObj.AddComponent<GameManager>();
            DontDestroyOnLoad(gameManagerObj);

            GameObject rendererObj = new GameObject("GameRenderer");
            rendererObj.AddComponent<GameRenderer>();
            DontDestroyOnLoad(rendererObj);

            ConfigureCamera();
        }

        static void ConfigureCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<Camera>();
                cameraObj.tag = "MainCamera";
                cameraObj.AddComponent<AudioListener>();
                DontDestroyOnLoad(cameraObj);
            }

            // BatchRenderer必须挂在Camera所在GameObject上,
            // 这样Unity的OnPostRender()魔法方法才会被调用
            if (mainCamera.GetComponent<BatchRenderer>() == null)
                mainCamera.gameObject.AddComponent<BatchRenderer>();

            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = Screen.height / 2f;
            mainCamera.aspect = (float)Screen.width / Screen.height;
            mainCamera.nearClipPlane = -100;
            mainCamera.farClipPlane = 100;
            mainCamera.depth = -1;
            mainCamera.cullingMask = 0;
        }

        void OnEnable()
        {
            ScreenSize = new Vector2(Screen.width, Screen.height);
        }

        void OnDisable()
        {
            ScreenSize = Vector2.zero;
        }

        void Awake()
        {
            Instance = this;

            ScreenSize = new Vector2(Screen.width, Screen.height);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            PixelTexture = new Texture2D(1, 1);
            PixelTexture.SetPixels(new[] { Color.white });
            PixelTexture.Apply();
        }

        void Start()
        {
            ParticleManager = new ParticleManager<ParticleState>(1024 * 20, ParticleState.UpdateParticle);

            const int maxGridPoints = 1600;
            float spacing = (float)Math.Sqrt(Screen.width * Screen.height / maxGridPoints);
            Vector2 gridSpacing = new Vector2(spacing, spacing);
            Grid = new Grid(new Rect(0, 0, Screen.width, Screen.height), gridSpacing);

            Art.Load();
            Sound.Load();

            EntityManager.Add(PlayerShip.Instance);

            PlayMusic();
        }

        void PlayMusic()
        {
            AudioSource audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.clip = Sound.Music;
            audioSource.loop = true;
            audioSource.volume = 0.5f;
            audioSource.Play();
        }

        void Update()
        {
            TotalTime += Time.deltaTime;
            DeltaTime = Time.deltaTime;

            Input.Update();

            if (Input.WasKeyPressed(KeyCode.Escape))
                Application.Quit();

            if (Input.WasKeyPressed(KeyCode.P))
                paused = !paused;
            if (Input.WasKeyPressed(KeyCode.B))
                useBloom = !useBloom;

            if (!paused)
            {
                PlayerStatus.Update();
                EntityManager.Update();
                EnemySpawner.Update();
                ParticleManager.Update();
                Grid.Update();
            }
        }
    }
}