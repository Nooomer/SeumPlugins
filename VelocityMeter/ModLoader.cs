#region сборка Assembly-CSharp, Version=9.0.0.0, Culture=neutral, PublicKeyToken=null
// расположение неизвестно
// Decompiled with ICSharpCode.Decompiler 9.1.0.7988
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
using VelocityMeter;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ModLoader : MonoBehaviour
{
    private static bool initialized;

    private static bool initializing;

    private static ModLoader instance;

    private string onlineRevUrl;

    private float onlineRev;

    private float currentRev;

    private bool hasCheckedForUpdate;

    private bool updateAvailable;

    private LevelMetadata.Record thisLevelMetaData;

    private string thisLevelName;

    private CharacterMotor motor;

    private GameManager manager;

    private GameManager.GameplayState state;

    private FPSInputController controller;

    private CharacterController charController;

    private float horizontalVelocity;

    private float verticalVelocity;

    private float peakSpeed;

    private float highestPeakSpeed;

    private float lowestSpeedDip;

    private int xRes;

    private int yRes;

    private bool showMenu;

    private bool showVerticalVelocity;

    private bool showPeakSpeed;

    private bool alwaysShowPeakSpeed;

    private bool resetPeakOnGround;

    private bool showAngleX;

    private bool showAngleY;

    private bool speedDip;

    private bool speedDipWall;

    private int speedDipCounter;

    private bool canGotoTemple;

    private bool menuOpen;

    private bool doingRun;

    private bool failedRun;

    private bool onGround;

    private float floatRealTime;

    private float realTime;

    private float oldMouseSense;

    private bool sensWarning;

    private float playerAngleX;

    private float playerAngleY;

    private float distanceTraveled;

    private bool distanceCalculated;

    private float averageSpeed;

    private int ticksThisRun;

    private List<float> distanceList;

    private float levelStartTime;

    private LeaderboardCollection lbCurrentLevel;

    private int lbTime;

    private float realLbTime;

    private bool calculateRealtime;

    private int menuLeftRect;

    private int menuTopRect;

    private int menuWidthRect;

    private int menuHeightRect;

    private int lastRunStatsLeftRect;

    private int lastRunStatsTopRect;

    private int lastRunStatsWidthRect;

    private bool showAngleMenu;

    private List<float> speeds;

    private float lastSpeed;

    private bool accelerating;

    private bool inReplay;

    private double replayDistance;

    private int replayFrameCount;

    private Vector3 playerPosition;

    private Vector3 currentPos;

    private Vector3 lastPos;

    private List<double> replayDistanceList;

    private double replayDistanceAverage;

    private XDocument mainLeaderboardXml;

    private XDocument currentLevelLeaderboardXml;

    private LevelMetadata.Record currentLevelMetaData;

    private UnityEngine.Random lbUrlExtRandom;

    private string lbUrlExt;

    private bool fetchedLBData;

    private int fetchedLBDataForLevel;

    private string thisLevelLeaderboardEntries;

    private string thisLevelLBUrl;

    private string timeComparisonEntryHodler;

    private string timeComparisonEntryRank;

    private bool fetchedLBEntryData;

    private int scoreRealTime;

    private bool calculateLastRunRank;

    private bool cantCalculateBecauseSpeedrun;

    private bool openOptionsMenu;

    private bool hideVelocityMeter;

    private int anglePrecision;

    private bool velocityMeterMenu;

    private bool enableVelometerGreenLimit;

    private string velometerGreenThreshold;

    private float velometerGreenThresholdFloat;

    private bool canGotoSpeddy;

    private bool characterCollisionSideGround;

    private bool characterCollisionSideAir;

    private int wallTouches;

    private bool addToWallTouchCounter;

    private CollisionFlags flag;

    private float restartOffset;

    private float levelRestartTime;

    private bool showCycleSnapshotTooltip;

    private int snapShotLevel;

    private bool loadedSettings;

    private bool showInfoScreen;

    private bool showStatSpeedDips;

    private bool showStatWallTouches;

    private bool showStatSpeedDipLowest;

    private bool showstatHighestPeak;

    private bool showStatDistance;

    private bool showStatAverageSpeed;

    private float mouseSens;

    private int totalMetersRan;

    private int thisSessionTotalMetersRan;

    private bool displayEndlessStats;

    private bool addToStats;

    private bool readStats;

    private bool newEndlessSession;

    private int workshopLevelInfoWidthRect;

    private int workshopLevelInfoLeftRect;

    private bool isWorkshop;

    private bool seumVelInitiated;

    private bool MGRMode;

    private float MGROldFOV;

    private bool MGROldDrunkMode;

    private bool MGROldHellium;

    private int MGROldSelectedHand;

    private float MGRoldMaxForwardSpeed;

    private bool showKeybindsMenu;

    private KeyCode modMenuBoundKey;

    private bool bindMenuButton;

    private KeyCode replayButtonBoundKey;

    private bool bindReplayButton;

    private KeyCode shortcutsBoundKey;

    private bool bindShortcutsButton;

    private KeyCode cycleSaveBoundKey;

    private bool bindCycleSaveButton;

    private KeyCode cycleLoadBoundKey;

    private bool bindCycleLoadButton;

    private KeyCode hideMeterBoundKey;

    private bool bindHideMeter;

    private bool bindingButton;

    private bool keybound;

    private int menuWidthRectmedium;

    private int menuWidthRectGame;

    private bool def;

    private float accumulatedFrameTime;

    private int numAccumulatedFrames;

    private float avgFrameTime;

    private static Rect windowRect;

    private Rect scrollRect;

    private float scrollWidth;

    private Rect drawRect;

    private float drawingPos;

    private bool testlist;

    private float vSbarValue;

    private Vector2 scrollPos;

    public Rect scrollViewRect;

    private Vector2 scrollViewVector;

    private string[] countrys;

    private int n;

    private int i;

    private int wichcountry;

    private GUIStyle BoxStyle;

    private GUIStyle BoxStyle2;

    private GUIStyle BoxStyle3;

    private float hscrollbarValue;

    public Vector2 scrollPosition;

    private string innerText;

    public static Texture2D Image;

    public static Texture2D ontexture;

    public static Texture2D onpresstexture;

    public static Texture2D offtexture;

    public static Texture2D offpresstexture;

    public static Texture2D backtexture;

    public static Texture2D btntexture;

    public static Texture2D btnpresstexture;

    private float num6;

    public static Texture2D Image2;

    private string test;

    private bool GameSetting;

    private string keyopen;

    private float num90;

    private float num190;

    public Font font;

    public float uiBaseScreenHeight;

    private Vector3 scale;

    private float orginalWidth;

    private float orginalHeight;

    private string HeightRez;

    private string WeightRez;

    public string stringToEditRez1;

    public string stringToEditRez2;

    public int stringToEditRez1Int;

    public int stringToEditRez2Int;

    public string stringToEdit;

    private string Converter;

    public static bool TrailRed;

    public static bool TrailYellow;

    public static bool TrailMagenta;

    public static bool TrailBlue;

    public static bool TrailCyan;

    public static bool TrailGreen;

    public static bool TrailWhite;

    public static bool TrailBlack;

    public static bool TrailLerp1;

    public static bool TrailLerp2;

    public static bool TrailLerp3;

    public static bool TrailLerp4;

    public static bool TrailLerp5;

    public static bool TrailLerp6;

    public static bool CrossColor1;

    public static bool CrossColor2;

    public static bool CrossColor3;

    public static bool CrossColor4;

    public static bool CrossColor5;

    public static bool CrossColor6;

    private string Closed;

    private float colorD;

    private bool DisplayisWorkshop;

    private bool Location;

    public GameObject character;

    public KeyCode TrailsKey;

    public static bool CrossColor7;

    public static bool CrossColor8;

    public static bool CrossColor9;

    public static bool CrossColor10;

    private bool MenuSub1;

    private bool MenuSub2;

    private bool MenuSub3;

    private bool MenuSub4;

    private static string Sub1String;

    private static string Sub2String;

    private static string Sub3String;

    private static string Sub4String;

    public Vector2 scrollPosition3;

    private string innerText3;

    private bool Theme1;

    private bool Theme2;

    private bool Theme3;

    private bool Theme4;

    private bool Theme5;

    private bool Theme6;

    private bool Theme7;

    private bool Theme8;

    private bool Theme9;

    private bool Theme10;

    public int StringRangeInt2;

    public string StringRange2;

    private bool ShowPosition;

    private GameObject playerObj;

    private bool showStatAverageSpeed1;

    private bool menutest;

    private bool ParticlesO;

    private bool notheme;

    private bool menutest1;

    private float groundPos;

    private float highestPos;

    private float peaktet;

    public CharacterMotor character2;

    public float groundPos2;

    public float highestPos2;

    public double peak2;

    public bool JumpHeight;

    private bool GhostMod;

    public FPSInputController player;

    public GameManager gameManager;

    public GameObject replayController;

    public Replay.ReplaySession savedReplay;

    public MeshRenderer meshRenderer;

    public float timer;

    private float alphaModifier;

    private bool EnableGhost;

    private bool GhostWhite;

    private bool GhostRed;

    private bool GhostGreen;

    private bool GhostBlue;

    private bool GhostYellow;

    private bool GhostPurple;

    private bool GOpcaity1;

    private bool GOpcaity2;

    private bool GOpcaity3;

    private bool GOpcaity4;

    private bool Decals;

    private string velometerGreenThreshold2;

    private float velometerGreenThresholdFloat2;

    private bool Menu_Size;

    private float orginalWidth2;

    private float orginalHeight2;

    private float infox;

    private float infoy;

    private bool enableDoublePeakList;

    private List<double> doublePeakList;

    private float PeakListFloat;

    private string PeakList;

    private bool enableCycleSet;

    private float setCycleFloat;

    private string setCycle;

    private Vector3 lastPosition;

    private Vector3 lastReplayPos;

    private double displayHSpd;

    private double displayVSpd;

    private float ghostDistanceGap;

    private bool isPlayerAhead;

    private float timeGap;

    private bool playerIsAhead;

    public static bool enableAngleSet;

    public static float targetAngleX;

    public static float targetAngleY;

    private string setAngleXStr = "0";

    private string setAngleYStr = "0";

    private bool NoFireballs;

    private bool NoBlockBreak;

    public static bool showHitboxes;

    private bool trailsEnabled;

    private Vector3 lastTrailPos;

    private List<GameObject> trails = new List<GameObject>();

    private GameObject cachedReplayTrail;

    private bool showGhostTrail;

    private bool wasTrailOn;

    public static ModLoader Instance
    {
        get
        {
            if (!initialized || instance == null)
            {
                Initialize();
            }

            return instance;
        }
    }

    public static Texture2D NewTexture2D => new Texture2D(1, 1);

    public static void Initialize()
    {
        if (!initialized && !initializing)
        {
            initializing = true;
            if (instance == null)
            {
                GameObject obj = new GameObject("ModLoader");
                ModLoader modLoader = obj.AddComponent<ModLoader>();
                UnityEngine.Object.DontDestroyOnLoad(obj);
                Debug.Log("ModLoader GameObject created");
                instance = modLoader;
            }

            initialized = true;
            initializing = false;
        }
    }

    private void Start()
    {
        Debug.Log("ModLoader Started");
    }

    private void OnDestroy()
    {
        Debug.Log("ModLoader Destroyed");
    }

    public void InitSeumVelocity()
    {
        if (loadedSettings)
        {
            return;
        }

        IniFile iniFile = new IniFile("Settings.ini");
        if (iniFile.KeyExists("CalculateRealTime"))
        {
            calculateRealtime = iniFile.Read("CalculateRealTime").Equals("True");
        }

        if (iniFile.KeyExists("CalculateLastRunRank"))
        {
            calculateLastRunRank = iniFile.Read("CalculateLastRunRank").Equals("True");
        }

        if (iniFile.KeyExists("ShowVerticalVelocity"))
        {
            showVerticalVelocity = iniFile.Read("ShowVerticalVelocity").Equals("True");
        }

        if (iniFile.KeyExists("AlwaysShowPeakspeed"))
        {
            alwaysShowPeakSpeed = iniFile.Read("AlwaysShowPeakspeed").Equals("True");
        }

        if (iniFile.KeyExists("ResetPeakOnGround"))
        {
            resetPeakOnGround = iniFile.Read("ResetPeakOnGround").Equals("True");
        }

        if (iniFile.KeyExists("EnableVelometerGreenLimit"))
        {
            enableVelometerGreenLimit = iniFile.Read("EnableVelometerGreenLimit").Equals("True");
        }

        if (iniFile.KeyExists("VelometerGreenThreshold"))
        {
            velometerGreenThreshold = iniFile.Read("VelometerGreenThreshold");
        }

        if (iniFile.KeyExists("VelometerGreenThresholdFloat"))
        {
            velometerGreenThresholdFloat = float.Parse(iniFile.Read("VelometerGreenThresholdFloat"));
        }

        if (iniFile.KeyExists("velometerGreenThresholdFloat2"))
        {
            velometerGreenThresholdFloat2 = float.Parse(iniFile.Read("velometerGreenThresholdFloat2"));
        }

        if (iniFile.KeyExists("ShowAngleX"))
        {
            showAngleX = iniFile.Read("ShowAngleX").Equals("True");
        }

        if (iniFile.KeyExists("ShowAngleY"))
        {
            showAngleY = iniFile.Read("ShowAngleY").Equals("True");
        }

        if (iniFile.KeyExists("AnglePrecision"))
        {
            if (iniFile.Read("AnglePrecision") == "0")
            {
                anglePrecision = 0;
            }
            else if (iniFile.Read("AnglePrecision") == "1")
            {
                anglePrecision = 1;
            }
            else if (iniFile.Read("AnglePrecision") == "2")
            {
                anglePrecision = 2;
            }
            else if (iniFile.Read("AnglePrecision") == "3")
            {
                anglePrecision = 3;
            }
        }

        if (iniFile.KeyExists("ShowStatSpeedDips"))
        {
            showStatSpeedDips = iniFile.Read("ShowStatSpeedDips").Equals("True");
        }

        if (iniFile.KeyExists("enableDoublePeakList"))
        {
            enableDoublePeakList = iniFile.Read("enableDoublePeakList").Equals("True");
        }

        if (iniFile.KeyExists("PeakList"))
        {
            PeakList = iniFile.Read("PeakList");
        }

        if (iniFile.KeyExists("PeakListFloat"))
        {
            PeakListFloat = float.Parse(iniFile.Read("PeakListFloat"));
        }

        if (iniFile.KeyExists("velometerGreenThresholdFloat"))
        {
            velometerGreenThresholdFloat = float.Parse(iniFile.Read("velometerGreenThresholdFloat"));
        }

        if (iniFile.KeyExists("velometerGreenThresholdFloat2"))
        {
            velometerGreenThresholdFloat2 = float.Parse(iniFile.Read("velometerGreenThresholdFloat2"));
        }

        if (iniFile.KeyExists("ShowStatWallTouches"))
        {
            showStatWallTouches = iniFile.Read("ShowStatWallTouches").Equals("True");
        }

        if (iniFile.KeyExists("ShowStatSpeedDipLowest"))
        {
            showStatSpeedDipLowest = iniFile.Read("ShowStatSpeedDipLowest").Equals("True");
        }

        if (iniFile.KeyExists("ShowStatHighestPeak"))
        {
            showstatHighestPeak = iniFile.Read("ShowStatHighestPeak").Equals("True");
        }

        if (iniFile.KeyExists("ShowStatDistance"))
        {
            showStatDistance = iniFile.Read("ShowStatDistance").Equals("True");
        }

        if (iniFile.KeyExists("ShowStatAverageSpeed"))
        {
            showStatAverageSpeed = iniFile.Read("ShowStatAverageSpeed").Equals("True");
        }

        if (iniFile.KeyExists("orginalWidth"))
        {
            orginalWidth = float.Parse(iniFile.Read("orginalWidth"));
        }

        if (iniFile.KeyExists("orginalHeight"))
        {
            orginalHeight = float.Parse(iniFile.Read("orginalHeight"));
        }

        if (iniFile.KeyExists("infox"))
        {
            infox = float.Parse(iniFile.Read("infox"));
        }

        if (iniFile.KeyExists("infoy"))
        {
            infoy = float.Parse(iniFile.Read("infoy"));
        }

        if (iniFile.KeyExists("def"))
        {
            def = iniFile.Read("def").Equals("True");
        }

        if (iniFile.KeyExists("CrossColor2"))
        {
            CrossColor2 = iniFile.Read("CrossColor2").Equals("True");
        }

        if (iniFile.KeyExists("CrossColor3"))
        {
            CrossColor3 = iniFile.Read("CrossColor3").Equals("True");
        }

        if (iniFile.KeyExists("CrossColor4"))
        {
            CrossColor4 = iniFile.Read("CrossColor4").Equals("True");
        }

        if (iniFile.KeyExists("CrossColor5"))
        {
            CrossColor5 = iniFile.Read("CrossColor5").Equals("True");
        }

        if (iniFile.KeyExists("CrossColor6"))
        {
            CrossColor6 = iniFile.Read("CrossColor6").Equals("True");
        }

        if (iniFile.KeyExists("CrossColor1"))
        {
            CrossColor1 = iniFile.Read("CrossColor1").Equals("True");
        }

        if (iniFile.KeyExists("CrossColor7"))
        {
            CrossColor7 = iniFile.Read("CrossColor7").Equals("True");
        }

        if (iniFile.KeyExists("CrossColor8"))
        {
            CrossColor8 = iniFile.Read("CrossColor8").Equals("True");
        }

        if (iniFile.KeyExists("CrossColor9"))
        {
            CrossColor9 = iniFile.Read("CrossColor9").Equals("True");
        }

        if (iniFile.KeyExists("TrailLerp1"))
        {
            TrailLerp1 = iniFile.Read("TrailLerp1").Equals("True");
        }

        if (iniFile.KeyExists("TrailLerp2"))
        {
            TrailLerp2 = iniFile.Read("TrailLerp2").Equals("True");
        }

        if (iniFile.KeyExists("TrailLerp3"))
        {
            TrailLerp3 = iniFile.Read("TrailLerp3").Equals("True");
        }

        if (iniFile.KeyExists("TrailLerp4"))
        {
            TrailLerp4 = iniFile.Read("TrailLerp4").Equals("True");
        }

        if (iniFile.KeyExists("TrailLerp5"))
        {
            TrailLerp5 = iniFile.Read("TrailLerp5").Equals("True");
        }

        if (iniFile.KeyExists("TrailLerp6"))
        {
            TrailLerp6 = iniFile.Read("TrailLerp6").Equals("True");
        }

        if (iniFile.KeyExists("TrailGreen"))
        {
            TrailGreen = iniFile.Read("TrailGreen").Equals("True");
        }

        if (iniFile.KeyExists("TrailYellow"))
        {
            TrailYellow = iniFile.Read("TrailYellow").Equals("True");
        }

        if (iniFile.KeyExists("TrailRed"))
        {
            TrailRed = iniFile.Read("TrailRed").Equals("True");
        }

        if (iniFile.KeyExists("TrailMagenta"))
        {
            TrailMagenta = iniFile.Read("TrailMagenta").Equals("True");
        }

        if (iniFile.KeyExists("TrailBlue"))
        {
            TrailBlue = iniFile.Read("TrailBlue").Equals("True");
        }

        if (iniFile.KeyExists("TrailCyan"))
        {
            TrailCyan = iniFile.Read("TrailCyan").Equals("True");
        }

        if (iniFile.KeyExists("TrailBlack"))
        {
            TrailBlack = iniFile.Read("TrailBlack").Equals("True");
        }

        if (iniFile.KeyExists("TrailWhite"))
        {
            TrailWhite = iniFile.Read("TrailWhite").Equals("True");
        }

        if (iniFile.KeyExists("ModMenuBoundKey"))
        {
            modMenuBoundKey = getKeyFromIni(modMenuBoundKey, iniFile.Read("ModMenuBoundKey"));
        }

        if (iniFile.KeyExists("JumpHeight"))
        {
            JumpHeight = iniFile.Read("JumpHeight").Equals("True");
        }

        if (iniFile.KeyExists("CycleSaveBoundKey"))
        {
            cycleSaveBoundKey = getKeyFromIni(cycleSaveBoundKey, iniFile.Read("CycleSaveBoundKey"));
        }

        if (iniFile.KeyExists("CycleLoadBoundKey"))
        {
            cycleLoadBoundKey = getKeyFromIni(cycleLoadBoundKey, iniFile.Read("CycleLoadBoundKey"));
        }

        if (iniFile.KeyExists("HideMeterBoundKey"))
        {
            hideMeterBoundKey = getKeyFromIni(hideMeterBoundKey, iniFile.Read("HideMeterBoundKey"));
        }

        if (iniFile.KeyExists("Theme1"))
        {
            Theme1 = iniFile.Read("Theme1").Equals("True");
        }

        if (iniFile.KeyExists("Theme2"))
        {
            Theme2 = iniFile.Read("Theme2").Equals("True");
        }

        if (iniFile.KeyExists("Theme3"))
        {
            Theme3 = iniFile.Read("Theme3").Equals("True");
        }

        if (iniFile.KeyExists("Theme4"))
        {
            Theme4 = iniFile.Read("Theme4").Equals("True");
        }

        if (iniFile.KeyExists("Theme5"))
        {
            Theme5 = iniFile.Read("Theme5").Equals("True");
        }

        if (iniFile.KeyExists("Theme6"))
        {
            Theme6 = iniFile.Read("Theme6").Equals("True");
        }

        if (iniFile.KeyExists("Theme7"))
        {
            Theme7 = iniFile.Read("Theme7").Equals("True");
        }

        if (iniFile.KeyExists("Theme8"))
        {
            Theme8 = iniFile.Read("Theme8").Equals("True");
        }

        if (iniFile.KeyExists("Theme9"))
        {
            Theme9 = iniFile.Read("Theme9").Equals("True");
        }

        if (iniFile.KeyExists("Theme10"))
        {
            Theme10 = iniFile.Read("Theme10").Equals("True");
        }

        if (iniFile.KeyExists("GhostRed"))
        {
            GhostRed = iniFile.Read("GhostRed").Equals("True");
        }

        if (iniFile.KeyExists("GhostBlue"))
        {
            GhostBlue = iniFile.Read("GhostBlue").Equals("True");
        }

        if (iniFile.KeyExists("GhostGreen"))
        {
            GhostGreen = iniFile.Read("GhostGreen").Equals("True");
        }

        if (iniFile.KeyExists("GhostYellow"))
        {
            GhostYellow = iniFile.Read("GhostYellow").Equals("True");
        }

        if (iniFile.KeyExists("GhostPurple"))
        {
            GhostPurple = iniFile.Read("GhostPurple").Equals("True");
        }

        if (iniFile.KeyExists("GOpcaity1"))
        {
            GOpcaity1 = iniFile.Read("GOpcaity1").Equals("True");
        }

        if (iniFile.KeyExists("GOpcaity2"))
        {
            GOpcaity2 = iniFile.Read("GOpcaity2").Equals("True");
        }

        if (iniFile.KeyExists("GOpcaity3"))
        {
            GOpcaity3 = iniFile.Read("GOpcaity3").Equals("True");
        }

        if (iniFile.KeyExists("GOpcaity4"))
        {
            GOpcaity4 = iniFile.Read("GOpcaity4").Equals("True");
        }

        if (iniFile.KeyExists("ParticlesO") && iniFile.KeyExists("ParticlesO"))
        {
            ParticlesO = iniFile.Read("ParticlesO").Equals("True");
            PluginState.OnParticles = ParticlesO;
        }

        if (iniFile.KeyExists("menutest"))
        {
            menutest = iniFile.Read("menutest").Equals("True");
            PluginState.OnEffect = menutest;
        }

        if (iniFile.KeyExists("menutest1"))
        {
            menutest1 = iniFile.Read("menutest1").Equals("True");
            Decals = menutest1;
        }

        if (iniFile.KeyExists("notheme"))
        {
            notheme = iniFile.Read("notheme").Equals("True");
            PluginState.DlcSky = notheme;
            PluginState.DlcNoTheme = notheme;
        }

        if (iniFile.KeyExists("NoFireballs"))
        {
            NoFireballs = iniFile.Read("NoFireballs").Equals("True");
            PluginState.NoFireballs = NoFireballs;
        }

        if (iniFile.KeyExists("NoBlockBreak"))
        {
            NoBlockBreak = iniFile.Read("NoBlockBreak").Equals("True");
            PluginState.NoBlockBreak = NoBlockBreak;
        }

        if (iniFile.KeyExists("BlockRestart"))
        {
            PluginState.RestartBlockEnabled = iniFile.Read("BlockRestart").Equals("True");
        }

        if (iniFile.KeyExists("EnableGhost"))
        {
            EnableGhost = iniFile.Read("EnableGhost").Equals("True");
            GhostMod = EnableGhost;
        }

        if (iniFile.KeyExists("GhostMod"))
        {
            GhostMod = iniFile.Read("GhostMod").Equals("True");
        }

        if (iniFile.KeyExists("notheme"))
        {
            notheme = iniFile.Read("notheme").Equals("True");
            PluginState.DlcSky = notheme;
            PluginState.DlcNoTheme = notheme;
            string text = (notheme ? "Hell6" : "Hell1");
            string text2 = (notheme ? "Hell6" : "Hell3");
            string text3 = (notheme ? "Hell6" : "Hell4");
            LevelSelector.zoneEnvironment[0] = text;
            LevelSelector.zoneEnvironment[1] = text;
            LevelSelector.zoneEnvironment[2] = text;
            LevelSelector.zoneEnvironment[3] = text2;
            LevelSelector.zoneEnvironment[4] = text2;
            LevelSelector.zoneEnvironment[5] = text2;
            LevelSelector.zoneEnvironment[6] = text3;
            LevelSelector.zoneEnvironment[7] = text3;
            LevelSelector.zoneEnvironment[8] = text3;
        }

        new IniFile("Config.yaml");
        loadedSettings = true;
        seumVelInitiated = true;
    }

    private void Update()
    {
        if (GhostMod)
        {
            if (player == null)
            {
                player = UnityEngine.Object.FindObjectOfType<FPSInputController>();
            }

            if (gameManager == null)
            {
                gameManager = UnityEngine.Object.FindObjectOfType<GameManager>();
            }

            if (replayController == null && player != null)
            {
                replayController = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                replayController.transform.position = player.transform.position;
                UnityEngine.Object.Destroy(replayController.GetComponent<CapsuleCollider>());
                meshRenderer = replayController.GetComponent<MeshRenderer>();
                meshRenderer.material.shader = AEShaders.shaders[1];
                replayController.transform.localScale = new Vector3(0.75f, 0.8f, 0.75f);
            }

            if (player != null && replayController != null)
            {
                float num = Vector3.Distance(player.transform.position, replayController.transform.position);
                if (num > 0.5f)
                {
                    if (GOpcaity1)
                    {
                        alphaModifier = Mathf.Clamp((-0.4f + num) / 20f, 0.05f, 0.3f);
                    }
                    else if (GOpcaity2)
                    {
                        alphaModifier = Mathf.Clamp((0.1f + num) / 20f, 0.05f, 0.3f);
                    }
                    else if (GOpcaity3)
                    {
                        alphaModifier = Mathf.Clamp((0.5f + num) / 20f, 0.05f, 0.5f);
                    }
                    else if (GOpcaity4)
                    {
                        alphaModifier = Mathf.Clamp((0.9f + num) / 20f, 0.05f, 0.9f);
                    }
                    else
                    {
                        alphaModifier = Mathf.Clamp((-0.4f + num) / 20f, 0.05f, 0.3f);
                    }
                }
                else
                {
                    alphaModifier = 0.1f;
                }
            }

            if (gameManager != null && gameManager.gameplayState == GameManager.GameplayState.IN_GAME)
            {
                if (savedReplay == null)
                {
                    replayController.transform.position = player.originalPosition;
                    replayController.transform.rotation = player.originalRotation;
                }
                else
                {
                    Color color = Color.white;
                    if (GhostRed)
                    {
                        color = Color.red;
                    }
                    else if (GhostBlue)
                    {
                        color = Color.blue;
                    }
                    else if (GhostGreen)
                    {
                        color = Color.green;
                    }
                    else if (GhostYellow)
                    {
                        color = Color.yellow;
                    }
                    else if (GhostPurple)
                    {
                        color = Color.magenta;
                    }

                    meshRenderer.material.color = new Color(color.r, color.g, color.b, alphaModifier);
                    float length = (float)savedReplay.frameCount * Time.fixedDeltaTime;
                    timer = Mathf.Repeat(timer + Time.deltaTime, length);
                    float num2 = timer - 0.0166666f;
                    if (num2 < 0f)
                    {
                        num2 = 0f;
                    }

                    int num3 = checked((int)(num2 / Time.fixedDeltaTime));
                    int num4 = num3 % savedReplay.frameCount;
                    float value = (num2 - (float)num3 * Time.fixedDeltaTime) / Time.fixedDeltaTime;
                    value = Mathf.Clamp01(value);
                    int num5 = checked(Mathf.Min(num4 + 1, savedReplay.frameCount - 1));
                    Replay.ReplayFullFrame replayFullFrame = savedReplay.frames[num4 / 60][num4 % 60];
                    Replay.ReplayFullFrame replayFullFrame2 = savedReplay.frames[num5 / 60][num5 % 60];
                    replayController.transform.position = Vector3.Lerp(replayFullFrame.position, replayFullFrame2.position, value);
                    replayController.transform.rotation = Quaternion.Slerp(Quaternion.Euler(0f, replayFullFrame.rotationX, 0f), Quaternion.Euler(0f, replayFullFrame2.rotationX, 0f), value);
                    float num6 = Vector3.Distance(player.transform.position, replayController.transform.position);
                    float num7 = ((replayFullFrame.forwardVelocity > 1f) ? replayFullFrame.forwardVelocity : 12.18f);
                    timeGap = num6 / num7;
                    Vector3 normalized = (replayFullFrame2.position - replayFullFrame.position).normalized;
                    Vector3 normalized2 = (replayController.transform.position - player.transform.position).normalized;
                    playerIsAhead = Vector3.Dot(normalized, normalized2) < 0f;
                }
            }
            else if (gameManager != null && gameManager.gameplayState == GameManager.GameplayState.START_LEVEL_AIM)
            {
                replayController.transform.position = player.originalPosition;
                timer = 0f;
            }
            else if (gameManager != null && gameManager.gameplayState == GameManager.GameplayState.REPLAY && Input.GetKeyDown(KeyCode.LeftShift))
            {
                savedReplay = Replay.replay;
                gameManager.gameplayState = GameManager.GameplayState.START_LEVEL_AIM;
                Game.restartLevel();
            }
        }

        if (!seumVelInitiated)
        {
            return;
        }

        if (GameSettings.settings.mouseSensitivity != mouseSens && mouseSens != 0f)
        {
            mouseSens = GameSettings.settings.mouseSensitivity;
        }

        if (Game.startedFrom == StartedFrom.WORKSHOP || Game.startedFrom == StartedFrom.DEFAULT)
        {
            isWorkshop = true;
        }
        else
        {
            isWorkshop = false;
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (!showInfoScreen)
            {
                showInfoScreen = true;
            }
            else
            {
                showInfoScreen = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            showHitboxes = !showHitboxes;
        }

        if ((bool)character)
        {
            character.GetComponent<GameManager>().performLateUpdate();
        }

        if ((bool)character)
        {
            OneLiners.levelChanged(Game.currentLevel, character.GetComponent<GameManager>());
        }

        if (charController == null)
        {
            charController = UnityEngine.Object.FindObjectOfType<CharacterController>();
        }
        else
        {
            flag = charController.collisionFlags;
            if (flag.Equals(CollisionFlags.Below))
            {
                characterCollisionSideGround = false;
            }
            else if (flag.Equals(CollisionFlags.Sides))
            {
                characterCollisionSideAir = true;
            }
            else
            {
                characterCollisionSideGround = true;
                characterCollisionSideAir = false;
            }
        }

        if (controller == null)
        {
            controller = UnityEngine.Object.FindObjectOfType<FPSInputController>();
        }
        else
        {
            if (controller.transform.eulerAngles.magnitude < 0.01f)
            {
                playerAngleX = 0.01f;
            }
            else
            {
                playerAngleX = controller.transform.eulerAngles.magnitude;
            }

            playerAngleY = controller.rotationY;
            playerPosition = controller.transform.position;
        }

        xRes = Screen.currentResolution.width;
        yRes = Screen.currentResolution.height;
        infox = infox;
        infoy = infoy;
        if (motor == null)
        {
            motor = UnityEngine.Object.FindObjectOfType<CharacterMotor>();
        }
        else
        {
            verticalVelocity = motor.movement.velocity.y;
            horizontalVelocity = Vector3.Magnitude(new Vector3(motor.movement.velocity.x, 0f, motor.movement.velocity.z));
            onGround = motor.grounded;
        }

        if (horizontalVelocity > peakSpeed)
        {
            peakSpeed = horizontalVelocity;
        }
        else if (onGround && (double)peakSpeed > 12.185 && resetPeakOnGround)
        {
            if (enableDoublePeakList && (double)peakSpeed < (double)PeakListFloat && (double)peakSpeed > 12.5)
            {
                doublePeakList.Add(peakSpeed);
            }

            peakSpeed = horizontalVelocity;
            showPeakSpeed = false;
        }

        if (peakSpeed > highestPeakSpeed && manager.gameplayState == GameManager.GameplayState.IN_GAME)
        {
            highestPeakSpeed = peakSpeed;
        }

        checked
        {
            if ((double)horizontalVelocity < 12.1 && ((characterCollisionSideGround && onGround) | (characterCollisionSideAir && !onGround)))
            {
                speedDipWall = true;
                addToWallTouchCounter = true;
                if (horizontalVelocity < lowestSpeedDip && manager.gameplayState == GameManager.GameplayState.IN_GAME)
                {
                    lowestSpeedDip = horizontalVelocity;
                }
            }
            else
            {
                if (addToWallTouchCounter)
                {
                    wallTouches++;
                    addToWallTouchCounter = false;
                }

                speedDipWall = false;
            }

            if ((double)horizontalVelocity < 11.9 && onGround && (double)highestPeakSpeed > 12.175)
            {
                speedDip = true;
                if (horizontalVelocity < lowestSpeedDip && manager.gameplayState == GameManager.GameplayState.IN_GAME)
                {
                    lowestSpeedDip = horizontalVelocity;
                }
            }
            else if ((((double)horizontalVelocity > 11.9) | !onGround) && speedDip)
            {
                speedDipCounter++;
                speedDip = false;
            }

            if (SceneManager.GetActiveScene().name == "LevelSelector")
            {
                levelRestartTime = 0f;
            }

            if (restartOffset != 0f)
            {
                showCycleSnapshotTooltip = true;
                if (snapShotLevel != Game.currentLevel)
                {
                    restartOffset = 0f;
                    levelRestartTime = 0f;
                }
            }
            else
            {
                showCycleSnapshotTooltip = false;
            }

            if (Input.GetKeyDown(modMenuBoundKey) && !showKeybindsMenu)
            {
                if (!showMenu)
                {
                    showMenu = true;
                    GameCursor.unlockCursor();
                    oldMouseSense = GameSettings.settings.mouseSensitivity;
                    GameSettings.settings.mouseSensitivity = 0f;
                }
                else
                {
                    showMenu = false;
                    GameCursor.lockCursor();
                    GameSettings.settings.mouseSensitivity = oldMouseSense;
                }
            }

            if (Input.GetKeyDown(hideMeterBoundKey) && !showKeybindsMenu)
            {
                if (!hideVelocityMeter)
                {
                    hideVelocityMeter = true;
                }
                else if (hideVelocityMeter)
                {
                    hideVelocityMeter = false;
                }
            }

            if (manager == null)
            {
                manager = UnityEngine.Object.FindObjectOfType<GameManager>();
            }
            else
            {
                if (manager.gameplayState != state && manager.gameplayState == GameManager.GameplayState.FINISH_LEVEL)
                {
                    doingRun = false;
                    if (!distanceCalculated)
                    {
                        for (int i = 0; i < distanceList.Count; i++)
                        {
                            distanceTraveled += distanceList[i];
                            ticksThisRun++;
                        }

                        averageSpeed = distanceTraveled / (float)ticksThisRun;
                        distanceTraveled += (float)scoreRealTime * 10f;
                        distanceCalculated = true;
                    }

                    if (!fetchedLBEntryData && calculateLastRunRank)
                    {
                        fetchedLBEntryData = true;
                        if (Game.startedFrom != StartedFrom.WORKSHOP)
                        {
                            if (!Game.isSpeedrun())
                            {
                                cantCalculateBecauseSpeedrun = false;
                                string url2 = thisLevelLBUrl + "&v=" + ((lbUrlExt != null) ? lbUrlExt.ToString() : "0");
                                int currentScore = scoreRealTime;
                                string levelName2 = ((currentLevelMetaData != null) ? currentLevelMetaData.name : "");
                                float endlessMeters = FPSInputController.metersRunned;
                                ThreadPool.QueueUserWorkItem(delegate
                                {
                                    try
                                    {
                                        foreach (XElement item in XDocument.Load(url2).Descendants("entry"))
                                        {
                                            int num8 = int.Parse(item.Element("score").Value);
                                            if (currentScore <= num8 && levelName2 != "Endless Mode")
                                            {
                                                timeComparisonEntryHodler = item.Element("steamid").Value;
                                                timeComparisonEntryRank = item.Element("rank").Value;
                                                break;
                                            }

                                            if (endlessMeters >= (float)num8 && levelName2 == "Endless Mode")
                                            {
                                                timeComparisonEntryRank = item.Element("rank").Value;
                                                break;
                                            }
                                        }
                                    }
                                    catch
                                    {
                                    }
                                });
                            }
                            else
                            {
                                cantCalculateBecauseSpeedrun = true;
                            }
                        }
                    }
                }
                else if (manager.gameplayState != state && manager.gameplayState == GameManager.GameplayState.START_LEVEL_AIM)
                {
                    levelRestartTime = Time.fixedTime;
                    doingRun = false;
                    if (failedRun)
                    {
                        failedRun = false;
                    }

                    if (Game.currentLevel >= 0 && Game.currentLevel < LevelMetadata.levels.Length)
                    {
                        if (Game.currentLevel >= 0 && Game.currentLevel < LevelMetadata.levels.Length)
                        {
                            thisLevelMetaData = LevelMetadata.levels[Game.currentLevel];
                        }
                        else
                        {
                            thisLevelMetaData = null;
                        }

                        thisLevelName = thisLevelMetaData.name;
                    }
                    else
                    {
                        thisLevelMetaData = null;
                        thisLevelName = "Workshop Level";
                    }

                    thisLevelName = thisLevelMetaData.name;
                }
                else if (manager.gameplayState != state && manager.gameplayState == GameManager.GameplayState.FAIL_LEVEL)
                {
                    doingRun = false;
                    failedRun = true;
                }

                if (manager.gameplayState != state && manager.gameplayState == GameManager.GameplayState.IN_GAME)
                {
                    doublePeakList.Clear();
                    doingRun = true;
                    speedDip = false;
                    speedDipWall = false;
                    failedRun = false;
                    speedDipCounter = 0;
                    wallTouches = 0;
                    highestPeakSpeed = 0f;
                    lowestSpeedDip = 12.18f;
                    distanceList.Clear();
                    distanceTraveled = 0f;
                    distanceCalculated = false;
                    ticksThisRun = 0;
                    fetchedLBEntryData = false;
                    showCycleSnapshotTooltip = false;
                    levelStartTime = Time.time;
                    if (!resetPeakOnGround)
                    {
                        peakSpeed = 0f;
                    }
                }

                if (manager.gameplayState != state)
                {
                    _ = manager.gameplayState;
                }

                if (manager.gameplayState == GameManager.GameplayState.IN_GAME)
                {
                    floatRealTime = manager.calculateCurrentLevelTime();
                    scoreRealTime = (int)Math.Floor(floatRealTime * 1000f);
                    realTime = (float)scoreRealTime * 0.001f;
                }

                if (manager.gameplayState == GameManager.GameplayState.START_LEVEL_AIM)
                {
                    try
                    {
                        if ((SceneManager.GetActiveScene().name != "MainMenu") | (SceneManager.GetActiveScene().name != "LevelSelector"))
                        {
                            lbTime = lbCurrentLevel.leaderboards[0].scores[0].time;
                            realLbTime = (float)lbTime * 0.001f;
                        }
                    }
                    catch
                    {
                    }

                    if (Input.GetKey(KeyCode.G) && Input.GetKey(KeyCode.E) && Input.GetKeyDown(KeyCode.N))
                    {
                        if (MGRMode)
                        {
                            MGRMode = false;
                            GameSettings.settings.fov = MGROldFOV;
                            GameSettings.settings.drunkMode = MGROldDrunkMode;
                            GameSettings.settings.hellium = MGROldHellium;
                            GameSettings.settings.selectedHand = MGROldSelectedHand;
                        }
                        else
                        {
                            AudioManager.play2DSound(AudioManager.sounds2D.skullWon);
                            MGROldFOV = GameSettings.settings.fov;
                            MGROldDrunkMode = GameSettings.settings.drunkMode;
                            MGROldHellium = GameSettings.settings.hellium;
                            MGROldSelectedHand = GameSettings.settings.selectedHand;
                            MGRoldMaxForwardSpeed = motor.movement.maxForwardSpeed;
                            MGRMode = true;
                            GameSettings.settings.fov = 200f;
                            GameSettings.settings.drunkMode = true;
                            GameSettings.settings.hellium = true;
                            GameSettings.settings.selectedHand = 2;
                        }
                    }

                    if (levelRestartTime == 0f)
                    {
                        levelRestartTime = Time.fixedTime;
                    }

                    if (Input.GetKeyDown(cycleSaveBoundKey) && !showKeybindsMenu)
                    {
                        if (restartOffset == 0f)
                        {
                            restartOffset = Time.fixedTime - levelRestartTime;
                            snapShotLevel = Game.currentLevel;
                        }
                        else
                        {
                            restartOffset = 0f;
                            Game.restartLevel();
                            levelRestartTime = Time.fixedTime;
                        }
                    }

                    if ((Input.mouseScrollDelta.y != 0f || Input.GetKeyDown(cycleLoadBoundKey)) && showCycleSnapshotTooltip && !showKeybindsMenu && Game.currentLevel != 43)
                    {
                        Vector3 eulerAngles = controller.transform.eulerAngles;
                        float rotationY = controller.rotationY;
                        Game.restartLevel(0f - restartOffset);
                        controller.transform.eulerAngles = eulerAngles;
                        controller.rotationY = rotationY;
                    }

                    if (!fetchedLBData && calculateLastRunRank)
                    {
                        fetchedLBData = true;
                        if (!Game.isSpeedrun())
                        {
                            if (Game.currentLevel >= 0 && Game.currentLevel < LevelMetadata.levels.Length)
                            {
                                thisLevelMetaData = LevelMetadata.levels[Game.currentLevel];
                            }
                            else
                            {
                                thisLevelMetaData = null;
                            }

                            string url3 = "http://steamcommunity.com/stats/457210/leaderboards/?xml=1&v=" + ((lbUrlExt != null) ? lbUrlExt.ToString() : "0");
                            string levelName3 = ((thisLevelMetaData != null) ? thisLevelMetaData.name : "");
                            int levelId = Game.currentLevel;
                            ThreadPool.QueueUserWorkItem(delegate
                            {
                                try
                                {
                                    foreach (XElement item2 in XDocument.Load(url3).Descendants("leaderboard"))
                                    {
                                        string value2 = item2.Element("url").Value;
                                        if (item2.Element("display_name").Value.Remove(0, 4).TrimStart('-', ' ') == levelName3 || (item2.Element("display_name").Value == "82 - ???" && levelName3 == "Beer Heaven") || (item2.Element("display_name").Value == "83 - ???" && levelName3 == "Toilette") || (item2.Element("display_name").Value == "Endless Mode" && levelName3 == "Endless Mode"))
                                        {
                                            thisLevelLBUrl = value2;
                                            thisLevelLeaderboardEntries = item2.Element("entries").Value;
                                            fetchedLBDataForLevel = levelId;
                                            break;
                                        }
                                    }
                                }
                                catch
                                {
                                }
                            });
                        }
                        else
                        {
                            thisLevelLeaderboardEntries = "Entries are not available in Speedrun mode";
                        }
                    }

                    if (fetchedLBDataForLevel != Game.currentLevel)
                    {
                        fetchedLBData = false;
                        timeComparisonEntryRank = "??";
                    }

                    if (Accounts.current.levels[90].skullWonTier > 0 && !Game.isSpeedrun() && !Game.isEndless() && Game.currentLevel != 149)
                    {
                        canGotoTemple = true;
                    }

                    if (Accounts.current.levels[181].skullWonTier > 0 && !Game.isSpeedrun() && !Game.isEndless() && Game.currentLevel != 181)
                    {
                        canGotoSpeddy = true;
                    }
                }
                else
                {
                    canGotoTemple = false;
                    canGotoSpeddy = false;
                }

                if (manager.gameplayState == GameManager.GameplayState.FINISH_LEVEL && Input.GetKeyDown(replayButtonBoundKey) && Game.startedFrom != StartedFrom.WORKSHOP && !Game.isSpeedrun() && !Game.isEndless())
                {
                    try
                    {
                        MethodInfo method = typeof(Hud).GetMethod("openReplay", BindingFlags.Static | BindingFlags.NonPublic);
                        if (method != null)
                        {
                            method.Invoke(null, new object[1]
                            {
                                new Score
                                {
                                    name = (SeumSteam.initialized ? SeumSteam.playerName : "Player"),
                                    time = scoreRealTime,
                                    replaySession = Replay.replay
                                }
                            });
                        }
                        else
                        {
                            manager.gameplayState = GameManager.GameplayState.REPLAY;
                            Game.restartLevel();
                        }
                    }
                    catch
                    {
                        manager.gameplayState = GameManager.GameplayState.REPLAY;
                        Game.restartLevel();
                    }
                }

                if (manager.gameplayState == GameManager.GameplayState.EMPTY)
                {
                    manager.gameplayState = GameManager.GameplayState.FINISH_LEVEL;
                }

                if (manager.gameplayState == GameManager.GameplayState.REPLAY && !hideVelocityMeter)
                {
                    inReplay = true;
                    if (Replay.replay != null)
                    {
                        replayFrameCount = Replay.replay.frameCount;
                    }

                    wasTrailOn = PluginState.ShowTrail;
                }
                else
                {
                    inReplay = false;
                    wasTrailOn = false;
                }

                if (PluginState.ShowTrail && Replay.replay != null)
                {
                    if (cachedReplayTrail == null)
                    {
                        cachedReplayTrail = GameObject.Find("ReplayTrail");
                        if (cachedReplayTrail == null)
                        {
                            ReplayBridge.CreateTrail();
                            cachedReplayTrail = GameObject.Find("ReplayTrail");
                        }
                    }
                }
                else if (inReplay)
                {
                    cachedReplayTrail = null;
                }

                lastSpeed = horizontalVelocity;
                state = manager.gameplayState;
            }

            if (!Game.isEndless())
            {
                return;
            }

            if (newEndlessSession)
            {
                thisSessionTotalMetersRan = 0;
                newEndlessSession = false;
            }

            if (manager == null)
            {
                manager = UnityEngine.Object.FindObjectOfType<GameManager>();
            }

            if (manager != null)
            {
                switch (manager.gameplayState)
                {
                    case GameManager.GameplayState.IN_GAME:
                        if (!addToStats)
                        {
                            addToStats = true;
                        }

                        if (!readStats)
                        {
                            readStats = true;
                        }

                        if (displayEndlessStats)
                        {
                            displayEndlessStats = false;
                        }

                        break;
                    case GameManager.GameplayState.START_LEVEL_AIM:
                        if (!displayEndlessStats)
                        {
                            displayEndlessStats = true;
                        }

                        if (readStats)
                        {
                            IniFile iniFile2 = new IniFile("Settings.ini");
                            if (iniFile2.KeyExists("endlessTotalDistance"))
                            {
                                totalMetersRan = (int)float.Parse(iniFile2.Read("endlessTotalDistance"));
                            }
                        }

                        break;
                    case GameManager.GameplayState.FAIL_LEVEL:
                        if (addToStats && controller != null)
                        {
                            IniFile iniFile = new IniFile("Settings.ini");
                            if (!iniFile.KeyExists("endlessTotalDistance"))
                            {
                                string key = "endlessTotalDistance";
                                iniFile.Write(key, controller.transform.position.z.ToString());
                            }
                            else
                            {
                                iniFile.Write("endlessTotalDistance", ((int)(float.Parse(iniFile.Read("endlessTotalDistance")) + controller.transform.position.z)).ToString());
                            }

                            thisSessionTotalMetersRan += (int)controller.transform.position.z;
                            addToStats = false;
                        }

                        break;
                }
            }
            else
            {
                newEndlessSession = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!seumVelInitiated || !(manager != null))
        {
            return;
        }

        if (manager.gameplayState == GameManager.GameplayState.IN_GAME)
        {
            if (character2 == null)
            {
                character2 = UnityEngine.Object.FindObjectOfType<CharacterMotor>();
            }
            else
            {
                float y = character2.transform.position.y;
                if (character2.grounded)
                {
                    groundPos2 = y;
                    peak2 = 0.0;
                    highestPos2 = 0f;
                }
                else
                {
                    if (highestPos2 < y)
                    {
                        highestPos2 = y;
                    }

                    peak2 = Math.Round(highestPos2 - groundPos2, 3);
                }
            }

            if (character == null)
            {
                UnityEngine.Object.FindObjectOfType<CharacterMotor>();
            }

            distanceList.Add(horizontalVelocity);
            if (!showMenu && GameSettings.settings.mouseSensitivity == 0f)
            {
                IniFile iniFile = new IniFile("Settings.ini");
                if (iniFile.KeyExists("MouseSensitivity"))
                {
                    GameSettings.settings.mouseSensitivity = float.Parse(iniFile.Read("MouseSensitivity"));
                }
                else
                {
                    GameSettings.settings.mouseSensitivity = 1.23456f;
                }
            }

            if (GameSettings.settings.mouseSensitivity == 1.23456f)
            {
                sensWarning = true;
            }
            else
            {
                sensWarning = false;
            }

            speeds.Add(horizontalVelocity);
            if (speeds.Count > 1)
            {
                if (speeds[1] > speeds[0] + 0.005f)
                {
                    accelerating = true;
                }
                else
                {
                    accelerating = false;
                }

                speeds.RemoveAt(0);
            }
        }

        if (manager.gameplayState != GameManager.GameplayState.REPLAY || !(controller != null))
        {
            return;
        }

        _ = lastPos;
        currentPos = controller.transform.position;
        if (currentPos != lastPos)
        {
            replayDistance = Math.Round((double)Vector2.Distance(new Vector2(currentPos.x, currentPos.z), new Vector2(lastPos.x, lastPos.z)) * Math.Pow(Time.fixedDeltaTime, -1.0), 2);
        }

        replayDistanceList.Add(replayDistance);
        if (replayDistanceList.Count > 9)
        {
            replayDistanceAverage = 0.0;
            for (int i = 0; i < replayDistanceList.Count; i = checked(i + 1))
            {
                replayDistanceAverage += replayDistanceList[i];
            }

            replayDistanceList.RemoveAt(0);
        }

        lastPos = currentPos;
    }

    private void OnGUI()
    {
        if (Decals)
        {
            DebugHud.decalsDisabled = true;
        }
        else
        {
            DebugHud.decalsDisabled = false;
        }

        if (Input.GetKeyDown(KeyCode.F8))
        {
            GL.wireframe = true;
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            GL.wireframe = false;
        }

        if (GameSettings.settings.mouseSensitivity == 0f && !showMenu)
        {
            GUI.contentColor = Color.red;
            GUI.Label(new Rect(xRes / 2 - 125, yRes / 2 + yRes / -16, 250f, 50f), "Your sensitivity is set to  0 ");
            GUI.contentColor = Color.white;
        }

        float num = 0f;
        num += (Time.deltaTime - num) * 0.1f;
        int width = Screen.width;
        int height = Screen.height;
        checked
        {
            if ((SceneManager.GetActiveScene().name == "MainMenu") | (SceneManager.GetActiveScene().name == "LevelSelector"))
            {
                InitSeumVelocity();
                keyopen = "【\ufeffＹ】";
                GUI.contentColor = Color.white;
                GUI.Label(new Rect(unchecked(Screen.currentResolution.width / 2) - 200, Screen.currentResolution.height - 50, 520f, 50f), string.Concat(new object[3] { "VelocityMeter 4 | Press", keyopen, "in game to open the menu | 【\ufeff F１】 for the info" }));
                GUI.contentColor = Color.white;
            }

            if (!seumVelInitiated)
            {
                return;
            }

            if (showInfoScreen)
            {
                GUI.backgroundColor = Color.black;
                GUI.skin.window.fontSize = 27;
                GUI.Window(1, new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height), modInfo, "");
            }

            if (!(manager != null))
            {
                return;
            }

            GUILayout.BeginHorizontal();
            if (sensWarning)
            {
                GUI.contentColor = Color.red;
                GUI.Label(new Rect(unchecked(xRes / 2) - 300, unchecked(yRes / 2) + unchecked(yRes / 8) + 20, 600f, 50f), "The game was not quit properly while the SEUM velocity menu was running. Mouse sensitivity has been changed from 0 to 1.23. Please change it in the game options");
                GUI.contentColor = Color.white;
            }

            if (!hideVelocityMeter)
            {
                if (doingRun)
                {
                    unchecked
                    {
                        if (GhostMod && gameManager.gameplayState == GameManager.GameplayState.IN_GAME && savedReplay != null)
                        {
                            float x = (float)((double)(xRes / 2) - 72.5);
                            float y = yRes / 2 + 30;
                            string text;
                            if (playerIsAhead)
                            {
                                GUI.contentColor = Color.green;
                                text = "-" + timeGap.ToString("F2") + "s";
                            }
                            else
                            {
                                GUI.contentColor = Color.red;
                                text = "+" + timeGap.ToString("F2") + "s";
                            }

                            if (timeGap > 0.01f)
                            {
                                GUI.Label(new Rect(x, y, 150f, 30f), "TIME: " + text);
                            }

                            GUI.contentColor = Color.white;
                        }
                    }

                    if (JumpHeight)
                    {
                        if (character2 != null)
                        {
                            GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16) + 160), 250f, 50f), "Jump height : " + peak2);
                        }

                        if (onGround)
                        {
                            groundPos = PluginState.YAxis;
                            peaktet = 0f;
                        }
                        else
                        {
                            if (highestPos < PluginState.YAxis)
                            {
                                highestPos = PluginState.YAxis;
                            }

                            peaktet = highestPos - groundPos;
                        }
                    }

                    if (calculateRealtime)
                    {
                        if (realTime > realLbTime)
                        {
                            GUI.contentColor = Color.cyan;
                            GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16) + 120), 250f, 50f), "Real Time: " + Math.Round(realTime, 3) + "s");
                            GUI.contentColor = Color.white;
                        }
                        else
                        {
                            GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16) + 120), 250f, 50f), "Real Time: " + Math.Round(realTime, 3) + "s");
                        }
                    }

                    if (showVerticalVelocity)
                    {
                        if (speedDipWall)
                        {
                            GUI.contentColor = Color.red;
                            GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16)), 250f, 50f), "hSpd: " + Math.Round(horizontalVelocity, 3));
                            GUI.contentColor = Color.white;
                        }
                        else if (accelerating && !enableVelometerGreenLimit)
                        {
                            GUI.contentColor = Color.green;
                            GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16)), 250f, 50f), "hSpd: " + Math.Round(horizontalVelocity, 3));
                            GUI.contentColor = Color.white;
                        }
                        else if (speedDip && !enableVelometerGreenLimit)
                        {
                            GUI.contentColor = Color.yellow;
                            GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16)), 250f, 50f), "hSpd: " + Math.Round(horizontalVelocity, 3));
                            GUI.contentColor = Color.white;
                        }
                        else if (enableVelometerGreenLimit)
                        {
                            if (horizontalVelocity > velometerGreenThresholdFloat)
                            {
                                GUI.contentColor = Color.Lerp(Color.green, Color.yellow, 0.5f);
                                GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16)), 250f, 50f), "hSpd: " + Math.Round(horizontalVelocity, 3));
                                GUI.contentColor = Color.white;
                            }
                            else if (horizontalVelocity < velometerGreenThresholdFloat && horizontalVelocity > velometerGreenThresholdFloat2)
                            {
                                GUI.contentColor = Color.yellow;
                                GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16)), 250f, 50f), "hSpd: " + Math.Round(horizontalVelocity, 3));
                                GUI.contentColor = Color.white;
                            }
                            else
                            {
                                GUI.contentColor = Color.red;
                                GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16)), 250f, 50f), "hSpd: " + Math.Round(horizontalVelocity, 3));
                                GUI.contentColor = Color.white;
                            }
                        }
                        else
                        {
                            GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16)), 250f, 50f), "hSpd: " + Math.Round(horizontalVelocity, 3));
                        }

                        GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16) + 20), 250f, 50f), "vSpd: " + Math.Round(verticalVelocity, 3));
                        if (showAngleX)
                        {
                            if (Game.currentLevel == 136)
                            {
                                GUI.contentColor = Color.grey;
                            }

                            GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16) + 60), 250f, 50f), "X Ang: " + Math.Round(playerAngleX, anglePrecision));
                            GUI.contentColor = Color.white;
                        }

                        if (showAngleY)
                        {
                            if (Game.currentLevel == 136)
                            {
                                GUI.contentColor = Color.grey;
                            }

                            GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16) + 80), 250f, 50f), "Y Ang: " + Math.Round(playerAngleY, anglePrecision));
                            GUI.contentColor = Color.white;
                        }
                    }
                    else
                    {
                        if (showAngleX)
                        {
                            if (Game.currentLevel == 136)
                            {
                                GUI.contentColor = Color.grey;
                            }

                            GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16) + 60), 250f, 50f), "X Ang: " + Math.Round(playerAngleX, anglePrecision));
                            GUI.contentColor = Color.white;
                        }

                        if (showAngleY)
                        {
                            if (Game.currentLevel == 136)
                            {
                                GUI.contentColor = Color.grey;
                            }

                            GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16) + 80), 250f, 50f), "Y Ang: " + Math.Round(playerAngleY, anglePrecision));
                            GUI.contentColor = Color.white;
                        }

                        if (speedDipWall)
                        {
                            GUI.contentColor = Color.red;
                            GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16)), 250f, 50f), string.Concat(Math.Round(horizontalVelocity, 3)));
                            GUI.contentColor = Color.white;
                        }
                        else if (accelerating && !enableVelometerGreenLimit)
                        {
                            GUI.contentColor = Color.green;
                            GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16)), 250f, 50f), string.Concat(Math.Round(horizontalVelocity, 3)));
                            GUI.contentColor = Color.white;
                        }
                        else if (speedDip && !enableVelometerGreenLimit)
                        {
                            GUI.contentColor = Color.yellow;
                            GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16)), 250f, 50f), string.Concat(Math.Round(horizontalVelocity, 3)));
                            GUI.contentColor = Color.white;
                        }
                        else if (enableVelometerGreenLimit)
                        {
                            if (horizontalVelocity > velometerGreenThresholdFloat)
                            {
                                GUI.contentColor = Color.green;
                                GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16)), 250f, 50f), string.Concat(Math.Round(horizontalVelocity, 3)));
                                GUI.contentColor = Color.white;
                            }
                            else if (horizontalVelocity < velometerGreenThresholdFloat && horizontalVelocity > velometerGreenThresholdFloat2)
                            {
                                GUI.contentColor = Color.yellow;
                                GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16)), 250f, 50f), string.Concat(Math.Round(horizontalVelocity, 3)));
                                GUI.contentColor = Color.white;
                            }
                            else
                            {
                                GUI.contentColor = Color.red;
                                GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16)), 250f, 50f), string.Concat(Math.Round(horizontalVelocity, 3)));
                                GUI.contentColor = Color.white;
                            }
                        }
                        else
                        {
                            GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16)), 250f, 50f), string.Concat(Math.Round(horizontalVelocity, 3)));
                        }
                    }

                    if (showPeakSpeed | alwaysShowPeakSpeed)
                    {
                        if (!showVerticalVelocity)
                        {
                            GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16) + 20), 250f, 50f), "peak: " + Math.Round(peakSpeed, 3));
                        }
                        else
                        {
                            GUI.Label(new Rect(infox + (float)(unchecked(xRes / 2) - 125), infoy + (float)(unchecked(yRes / 2) + unchecked(yRes / 16) + 40), 250f, 50f), "peak: " + Math.Round(peakSpeed, 3));
                        }
                    }
                }
                else if (failedRun)
                {
                    GUI.Label(new Rect(unchecked(xRes / 2) - 125, unchecked(yRes / 2) + unchecked(yRes / 8), 250f, 50f), "Last run timer: " + Math.Round(realTime, 3) + "s");
                }
                else if (!inReplay)
                {
                    int num2 = 0;
                    int num3 = 0;
                    if (showStatSpeedDips)
                    {
                        num2++;
                    }

                    if (showStatWallTouches)
                    {
                        num2++;
                    }

                    if (showStatSpeedDipLowest)
                    {
                        num2++;
                    }

                    if (showstatHighestPeak)
                    {
                        num2++;
                    }

                    if (showStatDistance)
                    {
                        num2++;
                    }

                    if (showStatAverageSpeed)
                    {
                        num2++;
                    }

                    if (calculateLastRunRank)
                    {
                        num2++;
                    }

                    if (enableDoublePeakList)
                    {
                        num3++;
                    }

                    if (num2 != 0 && !Game.isEndless() && !showMenu)
                    {
                        GUI.backgroundColor = Color.black;
                        GUI.skin.label.fontSize = 16;
                        GUI.Window(2, new Rect(75f, unchecked(yRes / 4) + unchecked(yRes / 16) + 100, 300f, 20 + 30 * num2), lastRunInfo, "Last run stats");
                    }
                    else if (Game.isEndless() && !showMenu)
                    {
                        GUI.backgroundColor = Color.black;
                        GUI.Window(2, new Rect(75f, unchecked(yRes / 4) + unchecked(yRes / 4) + 100, 300f, 110f), lastRunInfo, "Endless stats");
                    }

                    if (num3 != 0 && !Game.isEndless() && !showMenu)
                    {
                        GUI.backgroundColor = Color.black;
                        GUI.skin.label.fontSize = 16;
                        GUI.Window(10, new Rect(75f, unchecked(yRes / 4) + unchecked(yRes / 16) - 10, 300f, 15 + 30 * num3), LastPeak, "Last run Peaks");
                    }
                    else if (Game.isEndless() && !showMenu)
                    {
                        GUI.backgroundColor = Color.black;
                        GUI.Window(10, new Rect(75f, unchecked(yRes / 4) + unchecked(yRes / 4) - 10, 300f, 110f), LastPeak, "Endless stats");
                    }

                    if (isWorkshop)
                    {
                        scale.x = (float)Screen.width / orginalWidth2;
                        scale.y = (float)Screen.height / orginalHeight2;
                        scale.z = 1f;
                        Matrix4x4 matrix = GUI.matrix;
                        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
                        LevelData currentLevelData = Game.getCurrentLevelData();
                        int timesStarted = currentLevelData.timesStarted;
                        int timesReset = currentLevelData.timesReset;
                        int timesDied = currentLevelData.timesDied;
                        int timesFinished = currentLevelData.timesFinished;
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                        GUI.backgroundColor = Color.black;
                        GUI.color = new Color(1f, 1f, 1f, 0.9f);
                        GUI.DrawTexture(new Rect(1350f, 23f, 140f, 70f), Resources.Load<Texture2D>("optionspsd/box"));
                        GUI.color = new Color(1f, 1f, 1f, 1f);
                        GUI.Label(new Rect(1360f, -33f, 100f, 140f), "Tries: " + timesStarted);
                        GUI.Label(new Rect(1360f, -18f, 100f, 140f), "Resets: " + timesReset);
                        GUI.Label(new Rect(1360f, -2f, 100f, 140f), "Deaths: " + timesDied);
                        GUI.Label(new Rect(1360f, 12f, 100f, 140f), "Finishes: " + timesFinished);
                        GUI.matrix = matrix;
                    }

                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                    if (manager.gameplayState == GameManager.GameplayState.START_LEVEL_AIM)
                    {
                        if (showAngleX)
                        {
                            if (Game.currentLevel == 136)
                            {
                                GUI.contentColor = Color.grey;
                            }

                            GUI.Label(new Rect(unchecked(xRes / 2) - 125, unchecked(yRes / 2) + unchecked(yRes / 16) + 60, 250f, 50f), "X Ang: " + Math.Round(playerAngleX, anglePrecision));
                            GUI.contentColor = Color.white;
                        }

                        if (showAngleY)
                        {
                            if (Game.currentLevel == 136)
                            {
                                GUI.contentColor = Color.grey;
                            }

                            GUI.Label(new Rect(unchecked(xRes / 2) - 125, unchecked(yRes / 2) + unchecked(yRes / 16) + 80, 250f, 50f), "Y Ang: " + Math.Round(playerAngleY, anglePrecision));
                            GUI.contentColor = Color.white;
                        }

                        if (!Game.isSpeedrun() && calculateLastRunRank)
                        {
                            GUI.contentColor = Color.yellow;
                            GUI.contentColor = Color.white;
                        }
                        else if (calculateLastRunRank)
                        {
                            GUI.contentColor = Color.yellow;
                            GUI.contentColor = Color.white;
                        }

                        if (showCycleSnapshotTooltip && Game.currentLevel != 43)
                        {
                            GUI.contentColor = Color.yellow;
                            GUI.Label(new Rect(unchecked(xRes / 2) - 125, unchecked(yRes / 2) + unchecked(yRes / 16) + 100, 250f, 50f), "Saved cycle snapshot at: " + Math.Round(restartOffset, 3) + "s");
                            GUI.Label(new Rect(unchecked(xRes / 2) - 260, unchecked(yRes / 2) + unchecked(yRes / 16) + 120, 550f, 50f), string.Concat("Press '", cycleSaveBoundKey, "' to clear the snapshot, or press '", cycleLoadBoundKey, "' / Scrollwheel to load it"));
                            GUI.contentColor = Color.white;
                        }
                    }

                    if (manager.gameplayState == GameManager.GameplayState.FINISH_LEVEL && (Game.currentLevel == 0 || Game.currentLevel == 1 || Game.currentLevel == 8 || Game.currentLevel == 46 || Game.currentLevel == 77))
                    {
                        GUI.contentColor = Color.green;
                        GUI.Label(new Rect(unchecked(xRes / 2) - 125, unchecked(yRes / 2) + unchecked(yRes / 16) + 60, 250f, 50f), "Press '" + replayButtonBoundKey.ToString() + "' to check the replays");
                        GUI.contentColor = Color.white;
                    }
                }
            }

            if (showMenu)
            {
                scale.x = (float)Screen.width / orginalWidth;
                scale.y = (float)Screen.height / orginalHeight;
                scale.z = 1f;
                Matrix4x4 matrix2 = GUI.matrix;
                GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
                GUI.skin.box.fontSize = 24;
                int height2 = Screen.height;
                if (Image == null)
                {
                    byte[] data = new byte[4042]
                    {
                        137, 80, 78, 71, 13, 10, 26, 10, 0, 0,
                        0, 13, 73, 72, 68, 82, 0, 0, 0, 155,
                        0, 0, 0, 38, 8, 3, 0, 0, 0, 228,
                        235, 25, 21, 0, 0, 2, 142, 80, 76, 84,
                        69, 71, 112, 76, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 214, 113, 31, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 229,
                        121, 34, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 66, 31, 4, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 232, 122, 35,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 221, 116, 32, 0, 0,
                        0, 0, 0, 0, 68, 31, 4, 0, 0, 0,
                        0, 0, 0, 145, 74, 18, 0, 0, 0, 227,
                        119, 34, 0, 0, 0, 0, 0, 0, 197, 103,
                        28, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        130, 66, 14, 222, 117, 33, 70, 33, 4, 0,
                        0, 0, 0, 0, 0, 233, 123, 35, 199, 104,
                        28, 0, 0, 0, 219, 115, 32, 0, 0, 0,
                        138, 70, 16, 213, 111, 31, 224, 118, 33, 0,
                        0, 0, 235, 124, 36, 0, 0, 0, 66, 31,
                        4, 207, 108, 30, 0, 0, 0, 201, 105, 29,
                        230, 121, 34, 217, 115, 32, 67, 31, 4, 163,
                        84, 21, 235, 124, 36, 206, 108, 29, 229, 121,
                        34, 121, 61, 12, 184, 96, 25, 145, 74, 18,
                        145, 74, 18, 151, 77, 18, 182, 95, 24, 210,
                        110, 30, 174, 90, 23, 148, 76, 18, 190, 99,
                        26, 143, 73, 17, 199, 104, 28, 212, 111, 31,
                        227, 120, 34, 212, 111, 31, 198, 104, 28, 219,
                        115, 32, 159, 82, 20, 224, 118, 33, 149, 77,
                        18, 159, 82, 20, 198, 104, 28, 200, 105, 28,
                        177, 92, 24, 173, 89, 23, 209, 110, 30, 234,
                        124, 35, 224, 118, 33, 213, 112, 31, 165, 86,
                        22, 227, 120, 34, 233, 123, 35, 191, 100, 27,
                        197, 103, 28, 196, 103, 28, 164, 85, 21, 131,
                        66, 15, 63, 29, 4, 199, 104, 28, 131, 67,
                        15, 73, 34, 5, 43, 18, 2, 201, 105, 29,
                        214, 113, 31, 155, 80, 19, 223, 118, 33, 191,
                        99, 26, 111, 55, 10, 182, 95, 25, 109, 54,
                        10, 177, 92, 24, 133, 68, 15, 190, 99, 26,
                        84, 40, 6, 177, 92, 24, 168, 87, 22, 209,
                        109, 30, 232, 123, 35, 130, 66, 14, 0, 0,
                        0, 64, 29, 4, 227, 120, 34, 205, 108, 29,
                        179, 93, 24, 40, 16, 2, 141, 72, 17, 215,
                        113, 31, 206, 108, 30, 152, 78, 19, 188, 98,
                        26, 218, 115, 32, 169, 88, 22, 220, 116, 33,
                        80, 38, 6, 203, 107, 29, 70, 32, 4, 192,
                        100, 27, 167, 86, 22, 139, 71, 16, 215, 113,
                        31, 127, 64, 14, 148, 76, 18, 172, 89, 23,
                        49, 21, 3, 46, 19, 2, 186, 97, 26, 116,
                        58, 12, 39, 15, 2, 160, 82, 20, 123, 62,
                        13, 75, 35, 5, 77, 37, 5, 64, 29, 4,
                        65, 30, 4, 98, 49, 9, 87, 42, 6, 83,
                        40, 6, 95, 47, 8, 223, 118, 33, 230, 122,
                        34, 170, 88, 22, 141, 72, 17, 174, 90, 23,
                        188, 98, 26, 76, 36, 5, 136, 69, 16, 140,
                        71, 16, 225, 118, 34, 102, 51, 9, 104, 52,
                        9, 96, 47, 8, 37, 14, 2, 119, 60, 12,
                        227, 119, 34, 142, 73, 17, 164, 85, 22, 84,
                        40, 6, 236, 125, 36, 153, 102, 47, 139, 0,
                        0, 0, 217, 116, 82, 78, 83, 0, 17, 6,
                        183, 109, 13, 29, 1, 158, 3, 9, 198, 35,
                        60, 142, 67, 192, 79, 139, 195, 219, 163, 21,
                        215, 180, 211, 251, 155, 146, 51, 99, 130, 166,
                        25, 40, 103, 152, 206, 202, 11, 116, 188, 38,
                        125, 121, 86, 112, 225, 90, 71, 176, 209, 246,
                        32, 223, 55, 172, 94, 133, 62, 77, 214, 135,
                        169, 146, 233, 237, 74, 149, 248, 174, 82, 106,
                        169, 244, 205, 64, 47, 226, 90, 44, 250, 68,
                        233, 141, 111, 237, 241, 42, 222, 83, 227, 241,
                        231, 214, 219, 120, 224, 190, 215, 189, 175, 217,
                        144, 238, 102, 247, 109, 209, 168, 220, 245, 202,
                        169, 162, 202, 192, 184, 185, 133, 219, 134, 176,
                        212, 197, 227, 190, 74, 40, 237, 145, 209, 236,
                        158, 228, 210, 210, 240, 66, 160, 194, 194, 150,
                        238, 229, 205, 190, 215, 231, 110, 160, 81, 215,
                        146, 186, 167, 20, 158, 246, 241, 235, 54, 234,
                        78, 207, 226, 122, 105, 202, 97, 232, 57, 228,
                        173, 216, 104, 141, 203, 154, 178, 44, 139, 223,
                        166, 176, 123, 100, 223, 31, 230, 189, 178, 115,
                        229, 127, 228, 81, 133, 223, 196, 246, 113, 137,
                        242, 183, 198, 62, 221, 179, 233, 197, 235, 146,
                        218, 222, 90, 240, 19, 37, 25, 211, 0, 0,
                        12, 18, 73, 68, 65, 84, 88, 195, 149, 152,
                        137, 91, 83, 199, 22, 192, 179, 144, 16, 2,
                        137, 105, 246, 125, 145, 36, 102, 33, 105, 8,
                        9, 139, 64, 34, 144, 2, 6, 16, 68, 20,
                        80, 81, 16, 81, 43, 46, 40, 162, 40, 80,
                        197, 5, 125, 117, 197, 181, 46, 109, 95, 125,
                        173, 90, 235, 218, 197, 165, 251, 110, 91, 187,
                        216, 229, 45, 243, 223, 188, 51, 51, 247, 38,
                        55, 1, 191, 239, 189, 249, 132, 59, 28, 231,
                        204, 252, 102, 206, 204, 57, 103, 134, 199, 155,
                        189, 188, 133, 50, 203, 82, 169, 128, 17, 93,
                        145, 10, 242, 242, 114, 105, 171, 43, 175, 103,
                        182, 234, 107, 149, 10, 224, 127, 175, 204, 207,
                        20, 191, 126, 5, 164, 140, 250, 62, 233, 1,
                        180, 52, 171, 243, 189, 210, 107, 228, 123, 128,
                        104, 231, 230, 110, 163, 90, 60, 94, 106, 108,
                        44, 222, 199, 54, 222, 155, 165, 126, 205, 161,
                        125, 129, 214, 222, 115, 240, 69, 82, 6, 110,
                        27, 202, 46, 203, 181, 124, 169, 96, 134, 120,
                        191, 150, 47, 218, 203, 170, 247, 161, 253, 89,
                        236, 173, 14, 42, 232, 115, 104, 249, 243, 0,
                        227, 21, 134, 45, 55, 53, 182, 8, 22, 100,
                        31, 219, 120, 121, 86, 239, 187, 45, 115, 91,
                        105, 237, 66, 139, 207, 193, 151, 230, 145, 101,
                        155, 129, 134, 118, 182, 21, 104, 63, 205, 144,
                        244, 119, 130, 206, 175, 5, 108, 143, 23, 44,
                        139, 80, 54, 251, 160, 165, 131, 124, 23, 89,
                        64, 91, 36, 96, 216, 62, 224, 229, 165, 198,
                        22, 195, 140, 247, 177, 141, 95, 206, 82, 63,
                        223, 208, 62, 72, 107, 231, 170, 42, 187, 28,
                        34, 193, 108, 134, 135, 178, 170, 74, 214, 245,
                        94, 166, 104, 96, 215, 221, 53, 63, 201, 186,
                        240, 212, 254, 217, 137, 206, 53, 244, 160, 29,
                        89, 74, 187, 26, 14, 145, 111, 79, 160, 78,
                        230, 211, 206, 19, 80, 182, 53, 44, 27, 140,
                        109, 137, 242, 165, 251, 216, 198, 189, 89, 234,
                        35, 254, 194, 93, 180, 118, 209, 83, 209, 236,
                        211, 10, 176, 81, 247, 206, 100, 155, 240, 84,
                        204, 201, 156, 215, 107, 227, 145, 241, 200, 229,
                        64, 51, 72, 167, 87, 129, 122, 81, 45, 218,
                        188, 103, 85, 70, 147, 133, 254, 1, 242, 173,
                        181, 5, 3, 101, 5, 124, 41, 101, 27, 230,
                        9, 82, 99, 215, 181, 56, 68, 111, 177, 141,
                        23, 100, 141, 185, 73, 98, 91, 79, 107, 95,
                        36, 138, 26, 44, 14, 41, 176, 229, 206, 194,
                        118, 213, 233, 15, 13, 114, 5, 70, 248, 249,
                        5, 141, 4, 3, 189, 104, 122, 8, 234, 183,
                        157, 55, 209, 105, 84, 27, 225, 182, 89, 43,
                        89, 200, 84, 228, 193, 186, 54, 237, 167, 157,
                        228, 143, 195, 60, 56, 54, 184, 140, 73, 108,
                        21, 178, 168, 104, 63, 109, 243, 200, 196, 52,
                        238, 46, 45, 85, 171, 117, 58, 93, 137, 170,
                        222, 252, 62, 21, 53, 213, 43, 43, 100, 226,
                        121, 28, 182, 213, 106, 40, 186, 13, 164, 190,
                        82, 159, 8, 50, 107, 94, 77, 196, 139, 17,
                        122, 103, 37, 186, 97, 179, 246, 30, 218, 131,
                        165, 199, 194, 18, 229, 69, 101, 66, 146, 36,
                        109, 150, 224, 254, 13, 170, 122, 211, 29, 170,
                        109, 183, 5, 44, 204, 193, 48, 30, 231, 73,
                        41, 206, 70, 97, 34, 167, 221, 199, 167, 171,
                        185, 81, 165, 95, 75, 251, 255, 251, 178, 154,
                        21, 165, 234, 12, 182, 213, 10, 103, 97, 115,
                        84, 4, 108, 121, 47, 112, 27, 81, 182, 173,
                        110, 187, 127, 23, 87, 12, 108, 91, 141, 232,
                        134, 60, 120, 110, 15, 157, 154, 55, 110, 142,
                        201, 19, 26, 202, 246, 198, 155, 203, 86, 148,
                        234, 128, 237, 34, 229, 57, 98, 182, 86, 254,
                        74, 151, 237, 29, 3, 79, 122, 157, 246, 244,
                        182, 189, 168, 170, 75, 75, 197, 147, 6, 5,
                        195, 246, 183, 55, 107, 212, 6, 119, 190, 202,
                        27, 150, 216, 24, 51, 191, 152, 47, 201, 153,
                        83, 192, 101, 163, 141, 78, 145, 250, 75, 6,
                        125, 108, 61, 71, 236, 2, 54, 248, 183, 201,
                        228, 103, 164, 213, 48, 75, 155, 45, 102, 78,
                        18, 249, 27, 111, 214, 148, 26, 242, 245, 18,
                        165, 255, 116, 36, 137, 38, 151, 172, 124, 24,
                        44, 134, 83, 12, 24, 73, 157, 154, 39, 253,
                        144, 234, 156, 9, 219, 2, 45, 204, 9, 219,
                        160, 206, 103, 217, 106, 116, 249, 122, 187, 196,
                        148, 80, 166, 206, 66, 54, 219, 202, 151, 160,
                        145, 74, 31, 63, 76, 217, 116, 66, 229, 122,
                        142, 110, 156, 174, 207, 58, 77, 236, 107, 186,
                        110, 213, 138, 184, 210, 19, 204, 241, 212, 162,
                        147, 120, 221, 106, 212, 110, 161, 221, 236, 15,
                        90, 191, 140, 156, 28, 61, 101, 72, 142, 21,
                        125, 12, 110, 167, 7, 161, 45, 58, 29, 79,
                        224, 232, 39, 58, 167, 244, 177, 144, 133, 89,
                        195, 111, 212, 42, 214, 166, 165, 249, 118, 185,
                        63, 167, 208, 90, 209, 80, 220, 251, 156, 117,
                        155, 252, 83, 173, 178, 43, 253, 151, 88, 54,
                        249, 2, 142, 174, 173, 150, 178, 149, 203, 61,
                        17, 134, 205, 110, 179, 6, 170, 26, 106, 81,
                        228, 42, 176, 149, 186, 195, 9, 127, 97, 160,
                        174, 184, 10, 31, 213, 131, 103, 209, 197, 243,
                        143, 7, 183, 195, 178, 185, 117, 46, 158, 64,
                        187, 189, 243, 38, 50, 110, 56, 37, 148, 87,
                        148, 81, 23, 104, 44, 213, 121, 25, 182, 175,
                        12, 66, 121, 48, 80, 92, 41, 43, 179, 116,
                        189, 252, 60, 155, 126, 226, 10, 43, 115, 66,
                        155, 233, 126, 51, 80, 182, 36, 214, 213, 203,
                        131, 21, 61, 68, 188, 69, 104, 250, 235, 60,
                        195, 166, 41, 10, 52, 91, 44, 32, 191, 177,
                        24, 45, 209, 121, 157, 69, 161, 118, 75, 91,
                        91, 89, 21, 158, 197, 145, 228, 72, 4, 29,
                        250, 13, 42, 110, 151, 130, 151, 199, 191, 134,
                        34, 99, 43, 87, 87, 123, 205, 86, 217, 54,
                        212, 3, 45, 158, 150, 150, 232, 153, 115, 250,
                        149, 43, 238, 15, 201, 186, 162, 98, 177, 67,
                        219, 58, 11, 219, 158, 8, 56, 137, 238, 124,
                        24, 174, 242, 4, 135, 173, 22, 141, 65, 67,
                        55, 232, 182, 83, 182, 119, 20, 154, 163, 145,
                        1, 202, 38, 241, 84, 89, 162, 209, 69, 224,
                        249, 70, 39, 151, 184, 235, 99, 21, 115, 218,
                        196, 90, 71, 65, 217, 79, 120, 89, 126, 158,
                        64, 104, 243, 192, 229, 251, 194, 124, 85, 152,
                        151, 39, 250, 7, 221, 111, 199, 19, 133, 205,
                        29, 29, 216, 101, 79, 170, 221, 113, 134, 237,
                        69, 183, 198, 83, 215, 5, 225, 12, 202, 242,
                        217, 214, 237, 114, 242, 236, 98, 131, 55, 209,
                        216, 222, 214, 193, 101, 27, 30, 67, 198, 110,
                        149, 36, 88, 7, 17, 138, 248, 138, 124, 251,
                        81, 144, 82, 182, 96, 241, 92, 45, 127, 17,
                        90, 67, 153, 177, 183, 20, 73, 231, 105, 125,
                        115, 176, 67, 190, 145, 68, 151, 160, 217, 47,
                        122, 175, 222, 201, 203, 149, 138, 59, 241, 106,
                        110, 60, 14, 3, 116, 62, 198, 29, 125, 166,
                        83, 105, 142, 102, 131, 48, 22, 52, 102, 176,
                        61, 57, 52, 112, 123, 221, 25, 183, 94, 89,
                        81, 86, 48, 31, 117, 158, 62, 201, 176, 13,
                        221, 186, 127, 230, 105, 181, 23, 156, 77, 23,
                        195, 150, 154, 45, 176, 97, 117, 233, 162, 29,
                        24, 229, 95, 94, 19, 144, 242, 243, 120, 121,
                        82, 71, 203, 143, 216, 199, 31, 126, 6, 43,
                        55, 97, 15, 131, 99, 224, 229, 10, 180, 59,
                        167, 177, 210, 104, 34, 231, 91, 52, 248, 241,
                        64, 4, 189, 237, 18, 38, 222, 127, 14, 219,
                        202, 12, 182, 3, 157, 104, 226, 200, 22, 69,
                        220, 22, 42, 139, 206, 239, 199, 65, 113, 171,
                        75, 143, 217, 96, 195, 156, 122, 91, 47, 183,
                        202, 230, 118, 244, 100, 179, 153, 8, 219, 147,
                        221, 116, 31, 58, 115, 8, 91, 174, 64, 20,
                        149, 173, 185, 244, 44, 94, 254, 26, 138, 160,
                        181, 241, 184, 68, 217, 200, 227, 9, 248, 29,
                        8, 79, 225, 164, 51, 248, 227, 99, 56, 179,
                        119, 214, 185, 243, 235, 149, 11, 158, 195, 118,
                        118, 113, 183, 202, 212, 152, 178, 233, 246, 129,
                        17, 132, 62, 151, 248, 27, 44, 226, 214, 237,
                        132, 221, 165, 7, 221, 61, 223, 141, 63, 56,
                        227, 181, 199, 66, 101, 190, 142, 19, 155, 199,
                        71, 79, 125, 51, 131, 237, 250, 238, 33, 48,
                        215, 58, 125, 130, 178, 193, 194, 105, 219, 192,
                        125, 60, 3, 87, 52, 53, 21, 183, 107, 228,
                        158, 0, 143, 108, 184, 211, 88, 235, 161, 255,
                        46, 158, 227, 216, 148, 66, 40, 241, 127, 68,
                        3, 196, 39, 51, 108, 186, 161, 91, 97, 106,
                        76, 199, 5, 220, 238, 11, 179, 167, 170, 69,
                        124, 125, 231, 7, 152, 205, 29, 86, 174, 143,
                        224, 51, 59, 85, 111, 42, 10, 88, 10, 10,
                        44, 1, 91, 92, 229, 246, 74, 142, 102, 178,
                        189, 74, 220, 252, 58, 189, 147, 218, 148, 151,
                        155, 39, 138, 150, 109, 70, 11, 141, 201, 181,
                        135, 31, 197, 53, 78, 155, 117, 14, 140, 48,
                        79, 220, 185, 121, 60, 137, 30, 60, 82, 158,
                        6, 23, 52, 28, 57, 44, 172, 55, 7, 207,
                        17, 144, 223, 241, 239, 29, 36, 91, 21, 8,
                        168, 199, 216, 168, 243, 38, 10, 43, 83, 108,
                        152, 226, 182, 50, 167, 174, 69, 12, 81, 240,
                        22, 176, 193, 154, 127, 77, 220, 137, 209, 110,
                        14, 86, 181, 68, 163, 109, 85, 158, 68, 121,
                        216, 46, 103, 67, 158, 138, 97, 163, 126, 79,
                        104, 242, 212, 17, 54, 188, 112, 93, 247, 208,
                        200, 248, 125, 132, 236, 26, 147, 50, 216, 96,
                        1, 96, 169, 99, 231, 111, 184, 47, 189, 243,
                        206, 170, 200, 208, 45, 244, 172, 94, 19, 179,
                        126, 140, 137, 14, 110, 37, 29, 128, 177, 222,
                        131, 196, 149, 156, 211, 131, 147, 213, 66, 112,
                        54, 36, 214, 3, 91, 255, 247, 160, 248, 80,
                        14, 16, 98, 26, 161, 49, 27, 141, 11, 201,
                        135, 242, 156, 186, 54, 177, 195, 39, 171, 240,
                        203, 205, 74, 207, 71, 44, 27, 177, 132, 0,
                        122, 195, 54, 21, 74, 192, 15, 16, 54, 88,
                        56, 241, 110, 116, 25, 187, 112, 141, 196, 108,
                        179, 54, 251, 120, 116, 195, 225, 114, 92, 115,
                        103, 193, 192, 229, 90, 20, 143, 59, 139, 2,
                        189, 23, 166, 209, 68, 211, 98, 154, 242, 117,
                        162, 11, 109, 5, 98, 236, 223, 238, 111, 68,
                        213, 122, 112, 210, 12, 219, 54, 188, 199, 174,
                        222, 54, 123, 26, 44, 209, 12, 182, 161, 75,
                        201, 171, 183, 49, 155, 67, 43, 110, 107, 15,
                        21, 54, 90, 3, 76, 88, 233, 102, 216, 246,
                        19, 123, 172, 243, 130, 183, 163, 108, 188, 60,
                        1, 255, 218, 142, 115, 183, 48, 155, 73, 9,
                        61, 138, 121, 188, 148, 135, 59, 242, 232, 53,
                        248, 125, 243, 100, 57, 54, 198, 133, 19, 63,
                        53, 78, 164, 51, 172, 115, 117, 178, 182, 119,
                        17, 138, 64, 139, 234, 176, 50, 84, 38, 198,
                        249, 91, 222, 171, 52, 191, 250, 89, 226, 15,
                        148, 165, 216, 194, 108, 60, 253, 217, 9, 219,
                        208, 49, 127, 254, 252, 142, 19, 135, 126, 108,
                        151, 13, 102, 156, 46, 193, 235, 100, 123, 110,
                        1, 255, 198, 178, 1, 200, 135, 143, 123, 112,
                        244, 128, 221, 86, 88, 220, 165, 197, 162, 121,
                        52, 220, 159, 217, 68, 51, 185, 122, 187, 188,
                        177, 24, 2, 212, 249, 218, 52, 219, 197, 96,
                        168, 121, 16, 77, 127, 119, 31, 231, 111, 49,
                        150, 141, 137, 89, 199, 226, 177, 10, 89, 1,
                        195, 230, 78, 177, 29, 211, 248, 3, 22, 241,
                        43, 76, 202, 223, 146, 25, 242, 4, 252, 157,
                        40, 155, 45, 87, 0, 155, 112, 1, 101, 179,
                        54, 23, 136, 176, 72, 170, 197, 225, 126, 180,
                        201, 72, 148, 127, 9, 195, 118, 171, 132, 158,
                        134, 70, 140, 41, 182, 47, 228, 158, 134, 102,
                        28, 150, 206, 158, 58, 142, 189, 89, 6, 91,
                        19, 56, 178, 74, 31, 195, 6, 254, 141, 176,
                        77, 141, 30, 143, 227, 73, 236, 239, 156, 157,
                        109, 41, 154, 206, 102, 227, 129, 176, 31, 103,
                        59, 113, 19, 73, 116, 137, 136, 191, 29, 13,
                        175, 125, 112, 246, 32, 81, 254, 67, 175, 177,
                        133, 100, 96, 64, 28, 83, 154, 92, 37, 37,
                        6, 67, 137, 91, 40, 177, 133, 122, 239, 253,
                        147, 44, 135, 29, 179, 205, 227, 178, 9, 33,
                        220, 49, 108, 91, 75, 8, 91, 228, 38, 136,
                        195, 16, 46, 162, 125, 132, 237, 222, 12, 155,
                        46, 253, 21, 239, 213, 45, 94, 46, 91, 30,
                        176, 173, 1, 95, 81, 175, 1, 54, 177, 148,
                        218, 249, 58, 222, 73, 198, 35, 228, 232, 123,
                        133, 26, 72, 140, 129, 237, 55, 194, 102, 48,
                        232, 116, 6, 55, 200, 66, 189, 59, 14, 69,
                        102, 101, 171, 86, 56, 27, 155, 125, 173, 12,
                        91, 42, 71, 34, 113, 161, 160, 143, 212, 47,
                        167, 206, 2, 203, 182, 175, 227, 219, 194, 241,
                        195, 127, 8, 211, 103, 129, 176, 145, 211, 171,
                        183, 199, 210, 108, 31, 226, 69, 26, 61, 75,
                        156, 132, 10, 166, 210, 208, 2, 35, 237, 25,
                        62, 9, 108, 46, 23, 164, 189, 194, 184, 217,
                        19, 24, 164, 87, 183, 207, 53, 177, 44, 182,
                        213, 216, 27, 251, 230, 111, 235, 228, 228, 72,
                        152, 77, 136, 217, 104, 60, 29, 81, 122, 238,
                        126, 159, 193, 118, 0, 221, 195, 225, 51, 236,
                        100, 253, 27, 203, 6, 69, 88, 14, 87, 146,
                        40, 97, 131, 112, 223, 143, 51, 174, 7, 36,
                        199, 4, 23, 14, 46, 167, 149, 186, 213, 38,
                        183, 74, 88, 111, 151, 152, 99, 248, 106, 247,
                        132, 178, 225, 73, 69, 185, 108, 221, 216, 157,
                        178, 54, 77, 179, 85, 99, 54, 31, 101, 219,
                        100, 87, 126, 55, 128, 13, 13, 9, 21, 101,
                        235, 67, 211, 0, 187, 41, 206, 198, 44, 46,
                        155, 34, 12, 154, 148, 13, 231, 151, 196, 156,
                        120, 239, 127, 230, 242, 226, 145, 90, 201, 226,
                        162, 38, 21, 228, 242, 69, 57, 214, 80, 85,
                        101, 203, 187, 59, 79, 224, 45, 120, 172, 28,
                        43, 114, 115, 75, 50, 92, 106, 191, 165, 217,
                        188, 137, 20, 219, 150, 84, 204, 74, 179, 97,
                        91, 221, 208, 128, 79, 240, 101, 179, 229, 235,
                        211, 108, 121, 140, 247, 37, 73, 108, 9, 246,
                        251, 81, 38, 145, 108, 82, 216, 99, 57, 129,
                        98, 89, 89, 139, 79, 220, 218, 65, 162, 210,
                        177, 176, 153, 19, 179, 82, 102, 154, 201, 134,
                        47, 100, 149, 115, 217, 252, 141, 141, 245, 172,
                        239, 5, 54, 216, 189, 55, 36, 202, 194, 246,
                        25, 108, 46, 130, 192, 176, 49, 222, 23, 135,
                        203, 82, 72, 176, 67, 22, 49, 147, 72, 54,
                        121, 37, 69, 1, 156, 247, 58, 248, 252, 229,
                        7, 24, 143, 145, 200, 186, 3, 254, 47, 108,
                        174, 242, 133, 25, 49, 75, 218, 199, 216, 90,
                        222, 56, 147, 205, 128, 87, 156, 97, 195, 249,
                        37, 195, 246, 67, 41, 100, 210, 13, 45, 233,
                        183, 154, 36, 245, 192, 251, 69, 162, 172, 231,
                        155, 87, 82, 153, 48, 183, 112, 216, 184, 229,
                        185, 108, 225, 217, 246, 155, 78, 225, 76, 179,
                        65, 126, 137, 216, 235, 159, 74, 131, 79, 78,
                        122, 88, 146, 63, 161, 109, 14, 71, 43, 231,
                        21, 1, 202, 147, 180, 132, 203, 102, 248, 191,
                        216, 32, 15, 9, 114, 206, 233, 171, 239, 238,
                        65, 83, 153, 108, 169, 112, 143, 208, 127, 212,
                        216, 89, 21, 240, 95, 72, 191, 210, 144, 119,
                        149, 15, 44, 109, 236, 19, 204, 74, 250, 153,
                        182, 180, 252, 123, 118, 182, 175, 103, 138, 63,
                        75, 177, 49, 251, 77, 186, 136, 185, 227, 164,
                        242, 16, 204, 246, 105, 127, 207, 37, 99, 22,
                        91, 106, 195, 25, 151, 193, 253, 210, 90, 38,
                        22, 165, 217, 146, 119, 32, 77, 71, 107, 82,
                        247, 83, 52, 201, 188, 102, 5, 170, 190, 157,
                        9, 241, 20, 216, 190, 156, 41, 254, 38, 205,
                        150, 207, 101, 91, 226, 182, 115, 227, 2, 51,
                        106, 9, 103, 191, 65, 184, 143, 222, 187, 59,
                        62, 250, 212, 248, 82, 77, 73, 56, 6, 207,
                        37, 210, 20, 219, 70, 250, 25, 46, 106, 220,
                        197, 154, 157, 138, 38, 108, 65, 235, 208, 12,
                        136, 223, 225, 94, 223, 56, 67, 188, 113, 133,
                        43, 117, 115, 203, 98, 139, 207, 194, 230, 226,
                        178, 65, 126, 185, 27, 213, 78, 145, 163, 0,
                        19, 105, 227, 51, 105, 41, 188, 89, 208, 20,
                        14, 141, 153, 98, 127, 177, 162, 13, 79, 201,
                        107, 150, 70, 94, 244, 229, 112, 38, 195, 226,
                        55, 106, 224, 61, 196, 122, 119, 77, 166, 248,
                        135, 63, 87, 184, 237, 71, 103, 101, 115, 205,
                        194, 102, 116, 11, 57, 108, 176, 225, 230, 214,
                        5, 157, 66, 151, 206, 5, 23, 159, 118, 159,
                        72, 32, 242, 181, 7, 37, 42, 117, 205, 178,
                        21, 234, 18, 183, 74, 165, 240, 198, 19, 254,
                        156, 28, 155, 68, 161, 174, 169, 89, 161, 43,
                        129, 80, 150, 175, 55, 217, 114, 26, 139, 156,
                        66, 93, 13, 121, 48, 82, 171, 75, 87, 212,
                        212, 192, 220, 252, 161, 170, 134, 70, 179, 208,
                        192, 136, 65, 186, 172, 70, 231, 53, 21, 21,
                        22, 250, 37, 42, 131, 129, 62, 67, 73, 197,
                        101, 21, 74, 189, 203, 64, 79, 30, 203, 6,
                        119, 45, 171, 50, 172, 80, 133, 229, 56, 158,
                        254, 23, 246, 94, 142, 163, 69, 155, 157, 147,
                        0, 0, 0, 0, 73, 69, 78, 68, 174, 66,
                        96, 130
                    };
                    Image = new Texture2D(1, 1);
                    Image.LoadImage(data);
                }

                if (Image2 == null)
                {
                    byte[] data2 = new byte[3547]
                    {
                        137, 80, 78, 71, 13, 10, 26, 10, 0, 0,
                        0, 13, 73, 72, 68, 82, 0, 0, 0, 142,
                        0, 0, 0, 38, 8, 3, 0, 0, 0, 37,
                        108, 83, 185, 0, 0, 2, 106, 80, 76, 84,
                        69, 71, 112, 76, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 219, 56, 23, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        190, 46, 18, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        195, 48, 18, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 142, 33, 11, 203, 50,
                        20, 143, 32, 10, 212, 53, 21, 0, 0, 0,
                        220, 56, 23, 0, 0, 0, 101, 20, 5, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 215, 54,
                        22, 0, 0, 0, 143, 33, 11, 0, 0, 0,
                        0, 0, 0, 98, 19, 5, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 209, 52,
                        21, 179, 43, 16, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 100, 20, 5, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0, 218, 55,
                        22, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 106, 21, 5, 186, 46, 17, 175,
                        42, 16, 201, 50, 20, 0, 0, 0, 0, 0,
                        0, 186, 45, 17, 182, 44, 16, 194, 47, 18,
                        142, 32, 10, 96, 19, 5, 205, 51, 20, 159,
                        37, 13, 157, 37, 13, 145, 33, 10, 207, 52,
                        20, 220, 56, 23, 148, 34, 11, 178, 43, 16,
                        171, 41, 15, 218, 55, 22, 219, 55, 22, 200,
                        49, 19, 104, 21, 5, 115, 24, 6, 0, 0,
                        0, 172, 41, 15, 185, 45, 17, 194, 48, 19,
                        196, 48, 19, 217, 55, 22, 167, 40, 14, 95,
                        18, 4, 0, 0, 0, 202, 50, 20, 202, 50,
                        19, 211, 52, 21, 167, 39, 14, 191, 47, 18,
                        211, 53, 21, 187, 46, 17, 174, 42, 15, 159,
                        37, 13, 107, 22, 6, 167, 39, 14, 202, 50,
                        20, 210, 52, 21, 219, 55, 22, 204, 51, 20,
                        216, 54, 22, 197, 48, 19, 163, 38, 13, 105,
                        22, 6, 155, 36, 12, 203, 50, 20, 218, 55,
                        22, 214, 54, 22, 200, 50, 20, 110, 23, 6,
                        126, 27, 8, 134, 30, 9, 213, 53, 21, 211,
                        53, 21, 173, 41, 15, 100, 20, 5, 137, 31,
                        10, 136, 30, 9, 195, 48, 18, 111, 23, 6,
                        168, 40, 14, 152, 35, 12, 179, 43, 16, 154,
                        36, 12, 146, 33, 11, 143, 32, 10, 100, 20,
                        5, 202, 50, 20, 200, 50, 19, 210, 52, 21,
                        148, 34, 11, 214, 54, 22, 203, 50, 20, 213,
                        53, 22, 204, 51, 20, 179, 43, 16, 217, 55,
                        22, 42, 4, 1, 187, 46, 17, 173, 41, 15,
                        209, 52, 21, 139, 32, 10, 167, 39, 14, 131,
                        29, 9, 36, 4, 1, 215, 54, 22, 182, 44,
                        17, 117, 25, 7, 215, 54, 22, 193, 48, 18,
                        81, 14, 3, 87, 16, 4, 130, 29, 9, 160,
                        37, 13, 114, 24, 6, 207, 51, 21, 196, 48,
                        19, 103, 20, 5, 39, 4, 1, 95, 18, 4,
                        200, 49, 19, 150, 34, 12, 162, 38, 14, 74,
                        12, 3, 134, 30, 9, 38, 4, 1, 65, 10,
                        3, 129, 28, 8, 110, 23, 6, 206, 51, 20,
                        102, 20, 5, 90, 17, 4, 221, 56, 23, 18,
                        233, 122, 151, 0, 0, 0, 205, 116, 82, 78,
                        83, 0, 6, 38, 165, 4, 172, 194, 16, 189,
                        1, 185, 202, 2, 9, 72, 35, 181, 110, 13,
                        116, 156, 162, 60, 237, 191, 133, 188, 103, 159,
                        106, 130, 136, 27, 83, 43, 205, 218, 208, 150,
                        31, 54, 113, 153, 197, 119, 177, 66, 69, 234,
                        215, 132, 127, 20, 188, 143, 222, 228, 99, 87,
                        250, 47, 232, 127, 62, 232, 83, 211, 23, 76,
                        130, 166, 233, 94, 214, 226, 199, 91, 223, 140,
                        227, 22, 238, 124, 79, 212, 244, 171, 79, 112,
                        146, 221, 92, 209, 237, 238, 249, 113, 203, 224,
                        226, 240, 126, 238, 101, 244, 121, 240, 216, 185,
                        200, 176, 155, 230, 246, 246, 209, 246, 51, 137,
                        197, 239, 108, 235, 220, 177, 198, 198, 204, 184,
                        176, 185, 208, 212, 141, 225, 238, 209, 217, 234,
                        175, 104, 39, 198, 161, 205, 36, 92, 49, 121,
                        247, 143, 144, 193, 230, 120, 65, 208, 193, 100,
                        89, 59, 219, 204, 157, 198, 73, 218, 19, 212,
                        158, 177, 198, 161, 111, 240, 26, 228, 216, 214,
                        137, 178, 53, 232, 226, 207, 154, 84, 139, 208,
                        184, 162, 192, 183, 166, 175, 247, 172, 183, 204,
                        204, 195, 98, 152, 55, 198, 68, 206, 127, 70,
                        0, 0, 10, 83, 73, 68, 65, 84, 88, 195,
                        133, 152, 249, 67, 19, 215, 22, 199, 9, 73,
                        38, 100, 200, 190, 9, 217, 73, 72, 8, 132,
                        4, 20, 10, 89, 16, 8, 160, 128, 178, 40,
                        168, 44, 82, 17, 196, 5, 17, 41, 160, 160,
                        86, 197, 29, 247, 189, 90, 247, 125, 183, 174,
                        180, 182, 174, 109, 95, 251, 180, 125, 239, 254,
                        79, 239, 204, 189, 51, 97, 146, 129, 215, 251,
                        11, 151, 51, 103, 102, 62, 115, 239, 89, 190,
                        55, 41, 41, 199, 191, 130, 145, 198, 142, 131,
                        105, 188, 241, 92, 36, 82, 156, 225, 95, 133,
                        113, 83, 116, 240, 171, 51, 137, 166, 180, 19,
                        39, 210, 18, 13, 55, 69, 162, 31, 147, 238,
                        59, 118, 92, 36, 146, 40, 228, 242, 148, 164,
                        113, 22, 251, 245, 199, 47, 202, 251, 251, 246,
                        163, 248, 232, 67, 188, 145, 85, 44, 83, 222,
                        67, 137, 35, 171, 56, 13, 21, 29, 76, 178,
                        101, 9, 156, 100, 109, 40, 121, 28, 148, 233,
                        148, 34, 69, 50, 79, 51, 185, 86, 12, 23,
                        37, 204, 69, 121, 255, 35, 52, 203, 88, 239,
                        82, 87, 183, 9, 108, 119, 209, 223, 201, 182,
                        245, 194, 27, 87, 10, 31, 247, 31, 117, 181,
                        76, 153, 196, 243, 35, 185, 244, 214, 165, 118,
                        202, 148, 18, 184, 166, 56, 209, 55, 27, 206,
                        90, 149, 201, 149, 252, 216, 181, 53, 183, 209,
                        111, 73, 182, 121, 107, 133, 55, 174, 23, 62,
                        238, 178, 201, 229, 148, 137, 20, 9, 56, 100,
                        157, 155, 91, 85, 220, 53, 197, 189, 217, 104,
                        208, 41, 109, 164, 34, 249, 177, 167, 244, 115,
                        209, 194, 15, 7, 18, 109, 191, 8, 110, 108,
                        184, 44, 124, 220, 47, 117, 170, 50, 167, 78,
                        194, 167, 145, 147, 189, 218, 147, 175, 109, 8,
                        186, 150, 40, 25, 156, 182, 89, 113, 54, 149,
                        232, 99, 171, 201, 116, 7, 55, 126, 245, 238,
                        186, 181, 249, 86, 87, 23, 49, 111, 196, 182,
                        63, 166, 146, 157, 74, 242, 231, 241, 29, 200,
                        124, 202, 30, 14, 166, 22, 139, 228, 194, 189,
                        250, 195, 208, 148, 219, 186, 213, 137, 113, 138,
                        48, 225, 156, 249, 133, 133, 203, 241, 181, 229,
                        133, 133, 133, 221, 120, 118, 88, 234, 8, 179,
                        95, 233, 169, 170, 242, 184, 211, 211, 211, 41,
                        141, 216, 91, 158, 223, 226, 232, 32, 230, 69,
                        245, 96, 246, 211, 223, 39, 59, 89, 237, 23,
                        136, 169, 148, 113, 112, 147, 249, 132, 181, 37,
                        2, 239, 148, 11, 246, 106, 35, 173, 49, 150,
                        71, 124, 44, 14, 54, 205, 15, 213, 134, 158,
                        225, 217, 119, 181, 181, 33, 2, 182, 200, 156,
                        167, 101, 195, 34, 20, 106, 204, 174, 114, 19,
                        28, 251, 52, 206, 170, 90, 48, 15, 114, 56,
                        211, 78, 214, 204, 11, 60, 7, 15, 139, 166,
                        41, 9, 155, 114, 116, 188, 224, 57, 67, 114,
                        186, 215, 237, 151, 218, 89, 82, 22, 231, 235,
                        218, 165, 161, 175, 9, 206, 210, 165, 44, 78,
                        229, 98, 163, 158, 93, 244, 165, 181, 141, 30,
                        191, 153, 214, 4, 140, 222, 76, 189, 173, 61,
                        63, 19, 108, 81, 132, 182, 44, 13, 213, 167,
                        211, 210, 77, 201, 78, 37, 249, 44, 14, 118,
                        160, 200, 188, 146, 50, 104, 131, 106, 62, 206,
                        217, 52, 188, 199, 47, 235, 7, 197, 45, 173,
                        101, 120, 31, 37, 28, 78, 168, 190, 151, 197,
                        9, 101, 99, 156, 238, 202, 116, 49, 23, 3,
                        181, 33, 15, 45, 206, 43, 105, 114, 148, 235,
                        45, 13, 177, 72, 157, 13, 150, 152, 121, 91,
                        168, 62, 67, 154, 231, 56, 149, 236, 164, 109,
                        88, 203, 173, 14, 56, 24, 241, 52, 90, 57,
                        104, 204, 45, 72, 149, 77, 227, 200, 31, 49,
                        56, 61, 151, 170, 170, 50, 242, 180, 112, 133,
                        137, 114, 201, 2, 130, 211, 88, 159, 113, 132,
                        224, 132, 234, 41, 38, 248, 134, 17, 220, 207,
                        225, 132, 178, 205, 6, 187, 54, 220, 16, 137,
                        213, 20, 152, 124, 190, 10, 21, 66, 71, 129,
                        103, 85, 54, 37, 118, 228, 214, 9, 156, 84,
                        166, 237, 44, 78, 163, 95, 236, 104, 129, 201,
                        186, 110, 84, 153, 110, 212, 171, 18, 112, 250,
                        142, 33, 180, 171, 179, 183, 42, 61, 224, 168,
                        35, 161, 195, 225, 20, 102, 251, 165, 187, 241,
                        236, 25, 204, 190, 65, 209, 75, 195, 104, 145,
                        63, 190, 89, 141, 30, 105, 166, 165, 198, 228,
                        219, 90, 230, 74, 85, 231, 168, 213, 101, 96,
                        251, 18, 69, 149, 30, 77, 137, 54, 86, 177,
                        90, 232, 196, 22, 136, 202, 122, 186, 67, 219,
                        128, 80, 23, 176, 47, 90, 108, 204, 77, 192,
                        57, 139, 142, 163, 39, 15, 209, 103, 119, 134,
                        53, 191, 198, 69, 114, 142, 195, 169, 50, 231,
                        237, 33, 235, 84, 101, 54, 116, 173, 217, 5,
                        145, 81, 74, 89, 57, 156, 236, 65, 163, 190,
                        213, 167, 174, 174, 174, 46, 46, 150, 193, 112,
                        130, 109, 244, 210, 228, 155, 197, 226, 150, 136,
                        41, 117, 165, 208, 137, 53, 85, 122, 164, 246,
                        134, 2, 52, 190, 134, 201, 12, 191, 53, 1,
                        71, 254, 8, 237, 191, 124, 7, 237, 240, 83,
                        129, 166, 112, 133, 154, 84, 36, 14, 199, 29,
                        104, 218, 76, 102, 240, 197, 183, 182, 49, 104,
                        165, 25, 134, 92, 14, 199, 111, 176, 5, 83,
                        101, 34, 110, 200, 224, 131, 89, 15, 149, 171,
                        58, 75, 224, 164, 100, 77, 149, 110, 113, 126,
                        141, 239, 245, 109, 156, 89, 148, 181, 157, 31,
                        59, 15, 16, 218, 247, 244, 221, 232, 105, 179,
                        198, 144, 31, 43, 171, 198, 123, 21, 199, 73,
                        151, 102, 178, 56, 110, 169, 99, 46, 26, 233,
                        68, 104, 192, 108, 104, 231, 112, 168, 60, 139,
                        41, 71, 25, 95, 103, 37, 218, 203, 124, 240,
                        0, 237, 101, 0, 178, 4, 78, 92, 113, 133,
                        108, 208, 171, 202, 246, 145, 68, 207, 48, 36,
                        224, 28, 71, 99, 251, 208, 72, 207, 99, 82,
                        37, 87, 234, 64, 63, 40, 120, 56, 246, 21,
                        113, 176, 185, 8, 29, 138, 162, 101, 116, 158,
                        118, 54, 156, 230, 215, 140, 121, 153, 166, 195,
                        18, 84, 255, 35, 14, 153, 15, 36, 226, 200,
                        143, 45, 96, 202, 78, 167, 244, 39, 130, 227,
                        44, 134, 166, 62, 59, 206, 151, 137, 79, 26,
                        239, 172, 56, 99, 23, 199, 143, 66, 221, 14,
                        148, 204, 130, 115, 133, 143, 115, 247, 21, 193,
                        177, 182, 243, 98, 231, 193, 126, 252, 230, 107,
                        226, 81, 236, 185, 186, 44, 21, 154, 250, 108,
                        56, 115, 63, 26, 63, 253, 87, 218, 100, 89,
                        59, 51, 206, 77, 210, 120, 15, 7, 58, 108,
                        51, 227, 156, 219, 126, 127, 26, 167, 153, 221,
                        172, 132, 80, 62, 243, 195, 216, 222, 3, 183,
                        162, 143, 197, 163, 157, 204, 190, 207, 107, 13,
                        150, 57, 101, 12, 206, 6, 40, 176, 113, 156,
                        225, 23, 128, 211, 181, 226, 233, 173, 209, 211,
                        143, 141, 246, 186, 191, 72, 182, 21, 246, 246,
                        108, 190, 62, 86, 212, 47, 81, 40, 176, 106,
                        123, 78, 190, 254, 176, 198, 107, 131, 245, 63,
                        39, 112, 146, 180, 245, 13, 49, 182, 201, 207,
                        87, 247, 12, 93, 188, 72, 112, 168, 132, 68,
                        63, 142, 141, 157, 82, 168, 42, 88, 5, 228,
                        135, 11, 92, 213, 128, 211, 252, 30, 117, 162,
                        249, 241, 213, 129, 89, 230, 249, 121, 43, 70,
                        160, 163, 123, 243, 35, 55, 198, 153, 26, 134,
                        171, 244, 248, 94, 132, 100, 58, 38, 226, 228,
                        41, 39, 96, 65, 70, 24, 28, 92, 78, 207,
                        237, 75, 118, 130, 204, 194, 177, 78, 140, 175,
                        176, 66, 42, 245, 243, 203, 224, 25, 162, 179,
                        78, 7, 2, 4, 103, 147, 183, 5, 186, 104,
                        138, 228, 135, 187, 79, 161, 200, 204, 247, 139,
                        203, 227, 56, 246, 46, 180, 97, 207, 238, 135,
                        83, 14, 109, 235, 234, 235, 223, 162, 111, 126,
                        38, 61, 236, 213, 62, 164, 206, 1, 69, 7,
                        42, 82, 116, 12, 189, 103, 112, 104, 28, 157,
                        109, 111, 239, 39, 59, 101, 161, 219, 239, 142,
                        162, 229, 111, 136, 241, 192, 19, 1, 206, 189,
                        52, 220, 63, 31, 107, 88, 156, 9, 105, 137,
                        165, 66, 157, 34, 201, 106, 30, 7, 249, 212,
                        75, 241, 113, 32, 148, 209, 174, 135, 83, 118,
                        139, 234, 250, 246, 191, 238, 92, 67, 221, 147,
                        189, 76, 139, 106, 190, 95, 225, 3, 213, 6,
                        10, 83, 148, 118, 247, 3, 147, 89, 102, 28,
                        14, 87, 86, 222, 72, 114, 202, 89, 217, 183,
                        119, 197, 199, 147, 151, 208, 240, 142, 67, 204,
                        42, 109, 131, 244, 121, 73, 241, 112, 228, 253,
                        152, 166, 83, 67, 179, 155, 5, 85, 5, 22,
                        58, 69, 114, 174, 232, 254, 157, 209, 238, 94,
                        218, 218, 50, 141, 131, 213, 222, 159, 31, 1,
                        103, 59, 218, 112, 158, 167, 200, 194, 145, 2,
                        23, 40, 58, 185, 232, 220, 171, 13, 211, 56,
                        109, 2, 167, 178, 245, 176, 21, 35, 191, 70,
                        121, 214, 201, 42, 126, 25, 124, 128, 206, 50,
                        59, 124, 58, 131, 195, 33, 107, 7, 29, 189,
                        111, 239, 249, 93, 157, 39, 165, 94, 125, 18,
                        206, 182, 41, 135, 173, 6, 90, 225, 138, 93,
                        211, 207, 116, 228, 215, 85, 48, 138, 238, 193,
                        89, 244, 45, 31, 7, 45, 76, 116, 10, 226,
                        22, 122, 117, 221, 240, 156, 57, 196, 54, 252,
                        34, 219, 99, 206, 179, 197, 5, 198, 241, 230,
                        71, 205, 227, 40, 74, 83, 180, 248, 80, 34,
                        14, 188, 240, 247, 206, 171, 198, 38, 237, 194,
                        132, 205, 98, 196, 105, 46, 35, 78, 119, 194,
                        52, 195, 79, 101, 152, 105, 58, 96, 117, 104,
                        85, 32, 106, 229, 39, 178, 222, 126, 72, 192,
                        73, 114, 170, 193, 93, 117, 20, 29, 206, 160,
                        137, 178, 156, 108, 204, 78, 15, 76, 203, 47,
                        249, 177, 99, 140, 245, 167, 195, 135, 79, 78,
                        29, 37, 245, 101, 96, 19, 135, 115, 126, 4,
                        245, 228, 101, 218, 146, 113, 78, 26, 202, 27,
                        88, 28, 51, 229, 167, 168, 140, 12, 141, 184,
                        36, 183, 198, 85, 13, 213, 234, 220, 216, 141,
                        59, 163, 59, 126, 214, 24, 180, 211, 56, 60,
                        167, 86, 22, 103, 130, 166, 137, 108, 233, 6,
                        217, 34, 46, 143, 139, 211, 51, 168, 89, 168,
                        205, 65, 130, 65, 102, 161, 219, 192, 177, 187,
                        164, 60, 188, 144, 95, 149, 113, 176, 99, 173,
                        188, 176, 235, 40, 188, 137, 249, 112, 141, 212,
                        224, 176, 169, 64, 10, 164, 72, 250, 208, 107,
                        200, 172, 137, 128, 151, 73, 244, 54, 161, 19,
                        198, 185, 186, 227, 19, 173, 57, 77, 222, 245,
                        166, 138, 246, 106, 85, 172, 140, 72, 233, 159,
                        233, 168, 144, 217, 224, 131, 213, 185, 123, 125,
                        3, 131, 211, 18, 30, 106, 78, 196, 25, 208,
                        116, 216, 214, 50, 31, 30, 141, 154, 33, 228,
                        196, 6, 111, 147, 93, 27, 49, 169, 101, 128,
                        131, 208, 125, 200, 152, 147, 210, 14, 91, 1,
                        187, 58, 137, 78, 76, 236, 68, 143, 64, 97,
                        146, 138, 201, 187, 158, 185, 165, 153, 117, 176,
                        87, 18, 146, 87, 28, 66, 55, 15, 167, 195,
                        166, 2, 156, 223, 110, 192, 116, 119, 7, 183,
                        58, 243, 227, 56, 165, 68, 186, 51, 137, 10,
                        47, 242, 218, 245, 150, 186, 72, 141, 41, 181,
                        26, 30, 9, 56, 140, 194, 248, 94, 12, 61,
                        11, 227, 36, 57, 185, 176, 252, 218, 8, 61,
                        86, 236, 237, 36, 177, 156, 46, 110, 137, 203,
                        8, 121, 17, 135, 176, 156, 135, 35, 46, 111,
                        96, 154, 196, 16, 84, 94, 136, 29, 139, 0,
                        135, 178, 114, 122, 135, 162, 173, 229, 150, 88,
                        208, 228, 43, 83, 87, 235, 224, 180, 40, 98,
                        50, 143, 193, 105, 226, 181, 80, 158, 147, 147,
                        149, 95, 3, 26, 131, 131, 168, 58, 244, 111,
                        35, 35, 249, 36, 120, 175, 20, 113, 156, 117,
                        60, 30, 218, 219, 14, 101, 240, 111, 211, 211,
                        61, 209, 30, 131, 67, 155, 140, 195, 19, 167,
                        126, 141, 183, 61, 102, 74, 117, 58, 171, 101,
                        58, 17, 60, 82, 68, 204, 39, 165, 120, 117,
                        178, 4, 78, 156, 105, 64, 147, 215, 242, 110,
                        4, 79, 95, 136, 49, 14, 78, 115, 197, 243,
                        149, 171, 47, 76, 77, 148, 86, 174, 170, 124,
                        89, 58, 73, 36, 250, 150, 202, 197, 226, 114,
                        8, 229, 43, 175, 208, 182, 63, 191, 88, 155,
                        132, 56, 139, 227, 39, 137, 222, 35, 215, 54,
                        15, 93, 92, 80, 84, 116, 83, 137, 123, 150,
                        146, 61, 201, 5, 112, 236, 100, 9, 156, 56,
                        53, 88, 74, 123, 245, 225, 109, 100, 29, 164,
                        229, 177, 178, 37, 36, 146, 21, 50, 117, 208,
                        210, 161, 113, 103, 55, 102, 215, 123, 214, 113,
                        71, 32, 72, 233, 20, 201, 89, 38, 128, 31,
                        246, 252, 127, 28, 216, 26, 98, 91, 128, 127,
                        22, 81, 176, 56, 203, 104, 156, 89, 89, 2,
                        39, 206, 4, 225, 167, 111, 216, 73, 14, 54,
                        116, 19, 119, 92, 72, 81, 232, 212, 166, 186,
                        76, 35, 237, 95, 12, 165, 106, 14, 135, 131,
                        87, 7, 111, 227, 161, 127, 196, 65, 191, 51,
                        93, 1, 141, 169, 115, 150, 232, 148, 28, 14,
                        232, 205, 4, 156, 184, 19, 247, 131, 202, 34,
                        202, 160, 143, 220, 88, 67, 216, 243, 226, 167,
                        62, 133, 206, 185, 181, 85, 155, 233, 181, 26,
                        173, 86, 43, 105, 18, 91, 106, 179, 113, 236,
                        176, 56, 51, 197, 78, 2, 78, 231, 147, 3,
                        240, 174, 161, 160, 15, 82, 75, 164, 67, 92,
                        116, 49, 85, 57, 75, 232, 180, 157, 115, 176,
                        234, 35, 170, 157, 93, 76, 243, 186, 4, 71,
                        77, 54, 150, 21, 202, 98, 181, 175, 181, 206,
                        150, 171, 207, 213, 230, 222, 98, 79, 100, 233,
                        56, 179, 48, 78, 244, 170, 145, 195, 41, 156,
                        25, 135, 236, 48, 218, 28, 142, 85, 164, 46,
                        81, 114, 56, 131, 137, 56, 113, 167, 8, 171,
                        34, 225, 228, 168, 143, 5, 239, 147, 18, 19,
                        112, 52, 248, 72, 85, 150, 75, 116, 75, 212,
                        101, 166, 96, 129, 170, 160, 64, 245, 158, 61,
                        145, 153, 73, 221, 193, 255, 29, 145, 150, 204,
                        136, 195, 117, 234, 127, 13, 147, 191, 187, 29,
                        185, 49, 95, 142, 142, 195, 33, 7, 203, 43,
                        2, 39, 253, 133, 105, 173, 220, 106, 26, 34,
                        255, 124, 246, 130, 162, 193, 187, 37, 87, 136,
                        116, 197, 206, 28, 117, 42, 12, 215, 109, 242,
                        40, 79, 0, 87, 101, 89, 106, 208, 86, 34,
                        213, 0, 78, 164, 181, 174, 69, 76, 185, 161,
                        183, 88, 234, 108, 142, 192, 160, 27, 78, 81,
                        49, 85, 44, 215, 64, 101, 215, 194, 175, 1,
                        20, 173, 209, 64, 7, 176, 195, 17, 77, 166,
                        115, 154, 194, 29, 180, 167, 138, 194, 177, 35,
                        75, 45, 208, 38, 58, 101, 182, 135, 45, 229,
                        70, 170, 190, 30, 28, 84, 190, 173, 5, 54,
                        47, 237, 118, 155, 227, 39, 113, 224, 145, 136,
                        148, 58, 29, 115, 130, 44, 134, 168, 110, 10,
                        164, 123, 40, 166, 46, 253, 15, 190, 3, 195,
                        226, 207, 203, 57, 145, 0, 0, 0, 0, 73,
                        69, 78, 68, 174, 66, 96, 130
                    };
                    Image2 = new Texture2D(1, 1);
                    Image2.LoadImage(data2);
                }

                BoxStyle2 = new GUIStyle(GUI.skin.box);
                BoxStyle2.normal.background = MakeTex(2, 2, new Color(0f, 0f, 1f, 1f));
                BoxStyle3 = new GUIStyle(GUI.skin.box);
                BoxStyle3.normal.background = MakeTex(2, 2, Color.green);
                BoxStyle = new GUIStyle(GUI.skin.box);
                BoxStyle.normal.background = MakeTex(2, 2, new Color(1f, 1f, 1f, 0.1f));
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                GUIStyle gUIStyle = new GUIStyle();
                GUIStyle gUIStyle2 = new GUIStyle();
                GUIStyle gUIStyle3 = new GUIStyle();
                gUIStyle3.fontSize = 20;
                gUIStyle3.normal.textColor = Color.red;
                gUIStyle.fontSize = 15;
                gUIStyle2.font = font;
                gUIStyle2.normal.textColor = Color.white;
                gUIStyle2.fontSize = 30;
                gUIStyle.normal.textColor = Color.white;
                GUI.contentColor = Color.white;
                if (Theme1)
                {
                    GUI.DrawTexture(new Rect(20f, 100f, 470f, 770f), Resources.Load<Texture2D>("leveleditorpsd/start_menu_box"));
                }
                else if (Theme2)
                {
                    GUI.DrawTexture(new Rect(464f, 120f, -420f, 700f), Resources.Load<Texture2D>("environment/materials/hell3sky"));
                }
                else if (Theme3)
                {
                    GUI.DrawTexture(new Rect(44f, 120f, 420f, 700f), Resources.Load<Texture2D>("environment/materials/farfog"));
                }
                else if (Theme4)
                {
                    GUI.DrawTexture(new Rect(20f, 100f, 470f, 770f), Resources.Load<Texture2D>("levels/textures/detail/door_slidingwarehouse_1k_s"));
                }
                else if (Theme5)
                {
                    GUI.DrawTexture(new Rect(40f, 135f, 420f, 700f), Resources.Load<Texture2D>("stripsprites/intropage2/3-background"));
                }
                else if (Theme6)
                {
                    GUI.DrawTexture(new Rect(40f, 120f, 470f, 700f), Resources.Load<Texture2D>("decals/ham"));
                }
                else if (Theme7)
                {
                    GUI.DrawTexture(new Rect(44f, 120f, 420f, 700f), Resources.Load<Texture2D>("stripsprites/intropage3/panel2background"));
                }
                else if (Theme8)
                {
                    GUI.DrawTexture(new Rect(44f, 120f, 420f, 700f), Resources.Load<Texture2D>("levels/textures/portalborder"));
                }
                else if (Theme9)
                {
                    GUI.DrawTexture(new Rect(44f, 120f, 420f, 700f), Resources.Load<Texture2D>("environment/textures/tongues of fire"));
                }
                else if (Theme10)
                {
                    GUI.DrawTexture(new Rect(44f, 120f, 420f, 700f), Resources.Load<Texture2D>("stripsprites/intropage2/2-background"));
                }
                else
                {
                    GUI.color = new Color(1f, 1f, 1f, 1f);
                    GUI.DrawTexture(new Rect(20f, 100f, 470f, 770f), Resources.Load<Texture2D>("leveleditorpsd/start_menu_box"));
                }

                GUI.color = new Color(1f, 1f, 1f, 1f);
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.DrawTexture(new Rect(160f, 165f, 200f, 32f), Image);
                GUI.DrawTexture(new Rect(200f, 190f, 120f, 26f), Image2);
                GUI.DrawTexture(new Rect(120f, 120f, -87f, 90f), Resources.Load<Texture2D>("leaderboardpsd/tape"));
                GUI.DrawTexture(new Rect(59f, 237f, 390f, -5f), Resources.Load<Texture2D>("timerneopsd/fulluber"));
                GUI.DrawTexture(new Rect(59f, 268f, 390f, 5f), Resources.Load<Texture2D>("timerneopsd/fulluber"));
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                if (GUI.Button(new Rect(60f, 240f, 90f, 25f), Sub1String))
                {
                    MenuSub1 = true;
                    MenuSub2 = false;
                    MenuSub3 = false;
                    MenuSub4 = false;
                }

                if (GUI.Button(new Rect(160f, 240f, 90f, 25f), Sub2String))
                {
                    MenuSub2 = true;
                    MenuSub1 = false;
                    MenuSub3 = false;
                    MenuSub4 = false;
                }

                if (GUI.Button(new Rect(260f, 240f, 90f, 25f), Sub3String))
                {
                    MenuSub3 = true;
                    MenuSub1 = false;
                    MenuSub2 = false;
                    MenuSub4 = false;
                }

                if (GUI.Button(new Rect(360f, 240f, 90f, 25f), Sub4String))
                {
                    MenuSub4 = true;
                    MenuSub1 = false;
                    MenuSub2 = false;
                    MenuSub3 = false;
                }

                if (!MenuSub1 && !MenuSub2 && !MenuSub3 && !MenuSub4)
                {
                    MenuSub1 = true;
                }

                if (MenuSub1)
                {
                    Sub1String = "<color=lime>Main Options</color>";
                    GUI.Box(new Rect(65f, 276f, 250f, 50f), "Meter options:", gUIStyle);
                    if (!showVerticalVelocity)
                    {
                        GUI.contentColor = Color.grey;
                        if (GUI.Button(new Rect(60f, 298f, 90f, 25f), "Vspeed"))
                        {
                            showVerticalVelocity = true;
                        }
                    }
                    else if (showVerticalVelocity)
                    {
                        GUI.contentColor = Color.green;
                        if (GUI.Button(new Rect(60f, 298f, 90f, 25f), "Vspeed"))
                        {
                            showVerticalVelocity = false;
                        }
                    }

                    if (!alwaysShowPeakSpeed)
                    {
                        GUI.contentColor = Color.grey;
                        if (GUI.Button(new Rect(160f, 298f, 90f, 25f), "PeakSpeed"))
                        {
                            alwaysShowPeakSpeed = true;
                        }
                    }
                    else if (alwaysShowPeakSpeed)
                    {
                        GUI.contentColor = Color.green;
                        if (GUI.Button(new Rect(160f, 298f, 90f, 25f), "PeakSpeed"))
                        {
                            alwaysShowPeakSpeed = false;
                        }
                    }

                    if (!resetPeakOnGround)
                    {
                        GUI.contentColor = Color.grey;
                        if (GUI.Button(new Rect(260f, 298f, 190f, 25f), "Auto Reset Peak"))
                        {
                            resetPeakOnGround = true;
                        }
                    }
                    else if (resetPeakOnGround)
                    {
                        GUI.contentColor = Color.green;
                        if (GUI.Button(new Rect(260f, 298f, 190f, 25f), "Auto Reset Peak"))
                        {
                            resetPeakOnGround = false;
                        }
                    }

                    if (!enableVelometerGreenLimit)
                    {
                        GUI.contentColor = Color.grey;
                        if (GUI.Button(new Rect(60f, 328f, 190f, 25f), "Hspd: Green threshold"))
                        {
                            enableVelometerGreenLimit = true;
                        }
                    }
                    else if (enableVelometerGreenLimit)
                    {
                        GUI.contentColor = Color.green;
                        if (GUI.Button(new Rect(60f, 328f, 190f, 25f), "Hspd: Green threshold"))
                        {
                            enableVelometerGreenLimit = false;
                        }
                    }

                    if (enableVelometerGreenLimit)
                    {
                        GUI.contentColor = Color.white;
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        velometerGreenThreshold = GUI.TextField(new Rect(260f, 328f, 40f, 25f), velometerGreenThreshold, 7);
                        velometerGreenThreshold2 = GUI.TextField(new Rect(310f, 328f, 40f, 25f), velometerGreenThreshold2, 7);
                        if (GUI.Button(new Rect(360f, 328f, 90f, 25f), "Set"))
                        {
                            if (float.TryParse(velometerGreenThreshold, out velometerGreenThresholdFloat))
                            {
                                velometerGreenThresholdFloat = float.Parse(velometerGreenThreshold);
                            }
                            else
                            {
                                velometerGreenThreshold = "ERROR";
                            }

                            if (float.TryParse(velometerGreenThreshold2, out velometerGreenThresholdFloat2))
                            {
                                velometerGreenThresholdFloat2 = float.Parse(velometerGreenThreshold2);
                            }
                            else
                            {
                                stringToEditRez2 = "ERROR";
                            }
                        }
                    }

                    if (!enableCycleSet)
                    {
                        GUI.contentColor = Color.grey;
                        if (GUI.Button(new Rect(260f, 328f, 190f, 25f), "Set Cycle"))
                        {
                            enableCycleSet = true;
                        }
                    }
                    else if (enableCycleSet)
                    {
                        GUI.contentColor = Color.green;
                        if (GUI.Button(new Rect(260f, 328f, 190f, 25f), "Set Cycle"))
                        {
                            enableCycleSet = false;
                        }
                    }

                    if (enableCycleSet)
                    {
                        GUI.contentColor = Color.white;
                        setCycle = GUI.TextField(new Rect(500f, 328f, 40f, 25f), setCycle, 7);
                        if (GUI.Button(new Rect(500f, 358f, 60f, 25f), "-"))
                        {
                            setCycleFloat -= 0.0166666657f;
                            setCycle = setCycleFloat.ToString("F3");
                        }

                        if (GUI.Button(new Rect(580f, 358f, 60f, 25f), "+"))
                        {
                            GUI.contentColor = Color.white;
                            setCycleFloat += 0.0166666657f;
                            setCycle = setCycleFloat.ToString("F3");
                        }

                        if (GUI.Button(new Rect(550f, 328f, 90f, 25f), "Set"))
                        {
                            string s = setCycle.Replace(',', '.');
                            if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out setCycleFloat))
                            {
                                GUI.contentColor = Color.white;
                                float num4 = 0.0166666657f;
                                float num5 = Mathf.Round(setCycleFloat / num4);
                                if (num5 < 1f)
                                {
                                    num5 = 1f;
                                }

                                setCycleFloat = num5 * num4;
                                if (setCycleFloat <= 100f)
                                {
                                    restartOffset = Mathf.Round(setCycleFloat * 1000f) / 1000f;
                                    setCycle = restartOffset.ToString("F3", CultureInfo.InvariantCulture);
                                    snapShotLevel = Game.currentLevel;
                                }
                                else
                                {
                                    setCycle = "ERROR";
                                }
                            }
                            else
                            {
                                setCycle = "ERROR";
                            }
                        }
                    }

                    if (!calculateRealtime)
                    {
                        GUI.contentColor = Color.grey;
                        if (GUI.Button(new Rect(60f, 358f, 190f, 25f), "Calculate Realtime"))
                        {
                            calculateRealtime = true;
                        }
                    }
                    else if (calculateRealtime)
                    {
                        GUI.contentColor = Color.green;
                        if (GUI.Button(new Rect(60f, 358f, 190f, 25f), "Calculate Realtime"))
                        {
                            calculateRealtime = false;
                        }
                    }

                    GUI.contentColor = (PluginState.RestartBlockEnabled ? Color.green : Color.grey);
                    if (GUI.Button(new Rect(260f, 578f, 190f, 25f), "Block Restart (On Bounce)"))
                    {
                        PluginState.RestartBlockEnabled = !PluginState.RestartBlockEnabled;
                    }

                    GUI.contentColor = Color.white;
                    GUILayout.BeginArea(new Rect(260f, 608f, 600f, 30f));
                    GUILayout.BeginHorizontal();
                    GUI.contentColor = (enableAngleSet ? Color.green : Color.grey);
                    if (GUILayout.Button("Set Angles", GUILayout.Width(190f), GUILayout.Height(25f)))
                    {
                        enableAngleSet = !enableAngleSet;
                    }

                    if (enableAngleSet)
                    {
                        GUI.contentColor = Color.white;
                        GUILayout.Label("X:", GUILayout.Width(20f));
                        setAngleXStr = GUILayout.TextField(setAngleXStr, 7, GUILayout.Width(40f));
                        GUILayout.Label("Y:", GUILayout.Width(20f));
                        setAngleYStr = GUILayout.TextField(setAngleYStr, 7, GUILayout.Width(40f));
                        if (GUILayout.Button("Set", GUILayout.Width(50f)))
                        {
                            string s2 = setAngleXStr.Replace(',', '.');
                            string s3 = setAngleYStr.Replace(',', '.');
                            if (float.TryParse(s2, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) && float.TryParse(s3, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
                            {
                                targetAngleX = result;
                                targetAngleY = result2;
                                setAngleXStr = result.ToString(CultureInfo.InvariantCulture);
                                setAngleYStr = result2.ToString(CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                setAngleXStr = "Err";
                                setAngleYStr = "Err";
                            }
                        }

                        if (manager != null && manager.gameplayState == GameManager.GameplayState.REPLAY && GUILayout.Button("Copy Angle", GUILayout.Width(85f)))
                        {
                            targetAngleX = playerAngleX;
                            targetAngleY = playerAngleY;
                            setAngleXStr = playerAngleX.ToString(CultureInfo.InvariantCulture);
                            setAngleYStr = playerAngleY.ToString(CultureInfo.InvariantCulture);
                        }
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();
                    if (!JumpHeight)
                    {
                        GUI.contentColor = Color.grey;
                        if (GUI.Button(new Rect(260f, 358f, 190f, 25f), "Jump Meter"))
                        {
                            JumpHeight = true;
                        }
                    }
                    else if (JumpHeight)
                    {
                        GUI.contentColor = Color.green;
                        if (GUI.Button(new Rect(260f, 358f, 190f, 25f), "Jump Meter"))
                        {
                            JumpHeight = false;
                        }
                    }

                    GUI.contentColor = Color.white;
                    GUI.Label(new Rect(65f, 386f, 250f, 50f), "Angles options:\u00a0\u00a0\u00a0\u00a0\u00a0\u00a0\u00a0\u00a0\u00a0\u00a0\u00a0\u00a0\u00a0\u00a0\u00a0\u00a0\u00a0\u00a0\u00a0\u00a0\u00a0\u00a0\u00a0precision:\u00a0" + anglePrecision, gUIStyle);
                    if (!showAngleX)
                    {
                        GUI.contentColor = Color.grey;
                        if (GUI.Button(new Rect(60f, 408f, 90f, 25f), "Display X"))
                        {
                            showAngleX = true;
                        }
                    }
                    else if (showAngleX)
                    {
                        GUI.contentColor = Color.green;
                        if (GUI.Button(new Rect(60f, 408f, 90f, 25f), "Display X"))
                        {
                            showAngleX = false;
                        }
                    }

                    if (!showAngleY)
                    {
                        GUI.contentColor = Color.grey;
                        if (GUI.Button(new Rect(160f, 408f, 90f, 25f), "Display Y"))
                        {
                            showAngleY = true;
                        }
                    }
                    else if (showAngleY)
                    {
                        GUI.contentColor = Color.green;
                        if (GUI.Button(new Rect(160f, 408f, 90f, 25f), "Display Y"))
                        {
                            showAngleY = false;
                        }
                    }

                    GUI.contentColor = Color.white;
                    if (GUI.Button(new Rect(260f, 408f, 90f, 25f), "Less -") && anglePrecision > 0)
                    {
                        anglePrecision--;
                    }

                    GUI.contentColor = Color.white;
                    if (GUI.Button(new Rect(360f, 408f, 90f, 25f), "More +") && anglePrecision < 3)
                    {
                        anglePrecision++;
                    }

                    GUI.contentColor = Color.white;
                    GUI.Box(new Rect(65f, 436f, 250f, 50f), "Last run stats:", gUIStyle);
                    if (!showStatSpeedDips)
                    {
                        GUI.contentColor = Color.grey;
                        if (GUI.Button(new Rect(60f, 458f, 190f, 25f), "Number of speed dips"))
                        {
                            showStatSpeedDips = true;
                        }
                    }
                    else if (showStatSpeedDips)
                    {
                        GUI.contentColor = Color.green;
                        if (GUI.Button(new Rect(60f, 458f, 190f, 25f), "Number of speed dips"))
                        {
                            showStatSpeedDips = false;
                        }
                    }

                    if (!showStatWallTouches)
                    {
                        GUI.contentColor = Color.grey;
                        if (GUI.Button(new Rect(260f, 458f, 190f, 25f), "Number of wall touches"))
                        {
                            showStatWallTouches = true;
                        }
                    }
                    else if (showStatWallTouches)
                    {
                        GUI.contentColor = Color.green;
                        if (GUI.Button(new Rect(260f, 458f, 190f, 25f), "Number of wall touches"))
                        {
                            showStatWallTouches = false;
                        }
                    }

                    if (!showStatSpeedDipLowest)
                    {
                        GUI.contentColor = Color.grey;
                        if (GUI.Button(new Rect(60f, 488f, 190f, 25f), "Lowest speed dip"))
                        {
                            showStatSpeedDipLowest = true;
                        }
                    }
                    else if (showStatSpeedDipLowest)
                    {
                        GUI.contentColor = Color.green;
                        if (GUI.Button(new Rect(60f, 488f, 190f, 25f), "Lowest speed dip"))
                        {
                            showStatSpeedDipLowest = false;
                        }
                    }

                    if (!showstatHighestPeak)
                    {
                        GUI.contentColor = Color.grey;
                        if (GUI.Button(new Rect(260f, 488f, 190f, 25f), "Highest peak speed"))
                        {
                            showstatHighestPeak = true;
                        }
                    }
                    else if (showstatHighestPeak)
                    {
                        GUI.contentColor = Color.green;
                        if (GUI.Button(new Rect(260f, 488f, 190f, 25f), "Highest peak speed"))
                        {
                            showstatHighestPeak = false;
                        }
                    }

                    if (!showStatDistance)
                    {
                        GUI.contentColor = Color.grey;
                        if (GUI.Button(new Rect(60f, 518f, 190f, 25f), "Distance traveled"))
                        {
                            showStatDistance = true;
                        }
                    }
                    else if (showStatDistance)
                    {
                        GUI.contentColor = Color.green;
                        if (GUI.Button(new Rect(60f, 518f, 190f, 25f), "Distance traveled"))
                        {
                            showStatDistance = false;
                        }
                    }

                    if (!showStatAverageSpeed)
                    {
                        GUI.contentColor = Color.grey;
                        if (GUI.Button(new Rect(260f, 518f, 190f, 25f), "Average Speed"))
                        {
                            showStatAverageSpeed = true;
                        }
                    }
                    else if (showStatAverageSpeed)
                    {
                        GUI.contentColor = Color.green;
                        if (GUI.Button(new Rect(260f, 518f, 190f, 25f), "Average Speed"))
                        {
                            showStatAverageSpeed = false;
                        }
                    }

                    if (!enableDoublePeakList)
                    {
                        GUI.contentColor = Color.grey;
                        if (GUI.Button(new Rect(60f, 548f, 190f, 25f), "Peaks list"))
                        {
                            enableDoublePeakList = true;
                        }
                    }
                    else if (enableDoublePeakList)
                    {
                        GUI.contentColor = Color.green;
                        if (GUI.Button(new Rect(60f, 548f, 190f, 25f), "Peaks list"))
                        {
                            enableDoublePeakList = false;
                        }
                    }

                    if (enableDoublePeakList)
                    {
                        GUI.contentColor = Color.white;
                        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                        PeakList = GUI.TextField(new Rect(260f, 548f, 40f, 25f), PeakList, 6);
                        if (GUI.Button(new Rect(310f, 548f, 140f, 25f), "Set max peak"))
                        {
                            if (float.TryParse(PeakList, out PeakListFloat))
                            {
                                PeakListFloat = float.Parse(PeakList);
                            }
                            else
                            {
                                PeakList = "ERROR";
                            }
                        }
                    }

                    GUI.contentColor = Color.white;
                    GUI.Box(new Rect(65f, 576f, 250f, 50f), "Edit range on the leaderboard:", gUIStyle);
                    StringRange2 = GUI.TextField(new Rect(60f, 598f, 75f, 25f), StringRange2, 5);
                    if (GUI.Button(new Rect(140f, 598f, 75f, 25f), "Set"))
                    {
                        if (int.TryParse(StringRange2, out StringRangeInt2))
                        {
                            StringRangeInt2 = int.Parse(StringRange2);
                            PluginState.NumberStartS = StringRangeInt2;
                        }
                        else
                        {
                            StringRange2 = "ERROR";
                        }

                        SceneManager.LoadScene("Game");
                        showMenu = false;
                        GameSettings.settings.mouseSensitivity = oldMouseSense;
                    }

                    GUI.Box(new Rect(65f, 626f, 250f, 50f), "FPS Boost:", gUIStyle);
                    if (!ParticlesO)
                    {
                        GUI.contentColor = Color.grey;
                    }
                    else
                    {
                        GUI.contentColor = Color.green;
                    }

                    if (GUI.Button(new Rect(60f, 648f, 60f, 25f), "No Part"))
                    {
                        ParticlesO = !ParticlesO;
                        PluginState.OnParticles = ParticlesO;
                        SceneManager.LoadScene("Game");
                        showMenu = false;
                        GameSettings.settings.mouseSensitivity = oldMouseSense;
                    }

                    if (!menutest)
                    {
                        GUI.contentColor = Color.grey;
                    }
                    else
                    {
                        GUI.contentColor = Color.green;
                    }

                    if (GUI.Button(new Rect(125f, 648f, 60f, 25f), "No FX"))
                    {
                        menutest = !menutest;
                        PluginState.OnEffect = menutest;
                        SceneManager.LoadScene("Game");
                        showMenu = false;
                        GameSettings.settings.mouseSensitivity = oldMouseSense;
                    }

                    if (!notheme)
                    {
                        GUI.contentColor = Color.grey;
                    }
                    else
                    {
                        GUI.contentColor = Color.green;
                    }

                    if (GUI.Button(new Rect(190f, 648f, 60f, 25f), "No Sky"))
                    {
                        notheme = !notheme;
                        PluginState.DlcSky = notheme;
                        PluginState.DlcNoTheme = notheme;
                        string text2 = (notheme ? "Hell6" : "Hell1");
                        string text3 = (notheme ? "Hell6" : "Hell3");
                        string text4 = (notheme ? "Hell6" : "Hell4");
                        LevelSelector.zoneEnvironment[0] = text2;
                        LevelSelector.zoneEnvironment[1] = text2;
                        LevelSelector.zoneEnvironment[2] = text2;
                        LevelSelector.zoneEnvironment[3] = text3;
                        LevelSelector.zoneEnvironment[4] = text3;
                        LevelSelector.zoneEnvironment[5] = text3;
                        LevelSelector.zoneEnvironment[6] = text4;
                        LevelSelector.zoneEnvironment[7] = text4;
                        LevelSelector.zoneEnvironment[8] = text4;
                        SceneManager.LoadScene("Game");
                        showMenu = false;
                        GameSettings.settings.mouseSensitivity = oldMouseSense;
                    }

                    if (!menutest1)
                    {
                        GUI.contentColor = Color.grey;
                    }
                    else
                    {
                        GUI.contentColor = Color.green;
                    }

                    if (GUI.Button(new Rect(255f, 648f, 60f, 25f), "No Decal"))
                    {
                        menutest1 = !menutest1;
                        Decals = menutest1;
                        SceneManager.LoadScene("Game");
                        showMenu = false;
                        GameSettings.settings.mouseSensitivity = oldMouseSense;
                    }

                    if (!NoFireballs)
                    {
                        GUI.contentColor = Color.grey;
                    }
                    else
                    {
                        GUI.contentColor = Color.green;
                    }

                    if (GUI.Button(new Rect(320f, 648f, 60f, 25f), "No Fire"))
                    {
                        NoFireballs = !NoFireballs;
                        PluginState.NoFireballs = NoFireballs;
                    }

                    if (!NoBlockBreak)
                    {
                        GUI.contentColor = Color.grey;
                    }
                    else
                    {
                        GUI.contentColor = Color.green;
                    }

                    if (GUI.Button(new Rect(385f, 648f, 60f, 25f), "No Break"))
                    {
                        NoBlockBreak = !NoBlockBreak;
                        PluginState.NoBlockBreak = NoBlockBreak;
                    }

                    GUI.contentColor = Color.white;
                    GUI.Box(new Rect(65f, 676f, 250f, 50f), "Ghost Replays:", gUIStyle);
                    if (!EnableGhost)
                    {
                        GUI.contentColor = Color.grey;
                        if (GUI.Button(new Rect(60f, 698f, 90f, 25f), "Ghost mode"))
                        {
                            EnableGhost = true;
                            GhostMod = true;
                            SceneManager.LoadScene("Game");
                            showMenu = false;
                            GameSettings.settings.mouseSensitivity = oldMouseSense;
                        }
                    }
                    else if (EnableGhost)
                    {
                        GUI.contentColor = Color.green;
                        if (GUI.Button(new Rect(60f, 698f, 90f, 25f), "Ghost mode"))
                        {
                            EnableGhost = false;
                            GhostMod = false;
                            SceneManager.LoadScene("Game");
                            showMenu = false;
                            GameSettings.settings.mouseSensitivity = oldMouseSense;
                        }
                    }

                    GUI.contentColor = Color.white;
                    GUI.Box(new Rect(165f, 676f, 250f, 50f), "Color:", gUIStyle);
                    if (GUI.Button(new Rect(160f, 698f, 37f, 23f), "■"))
                    {
                        GhostWhite = true;
                        GhostRed = false;
                        GhostBlue = false;
                        GhostGreen = false;
                        GhostYellow = false;
                        GhostPurple = false;
                    }

                    GUI.contentColor = Color.red;
                    if (GUI.Button(new Rect(200f, 698f, 37f, 23f), "■"))
                    {
                        GhostWhite = false;
                        GhostRed = true;
                        GhostBlue = false;
                        GhostGreen = false;
                        GhostYellow = false;
                        GhostPurple = false;
                    }

                    GUI.contentColor = Color.blue;
                    if (GUI.Button(new Rect(240f, 698f, 37f, 23f), "■"))
                    {
                        GhostWhite = false;
                        GhostRed = false;
                        GhostBlue = true;
                        GhostGreen = false;
                        GhostYellow = false;
                        GhostPurple = false;
                    }

                    GUI.contentColor = Color.green;
                    if (GUI.Button(new Rect(160f, 729f, 37f, 23f), "■"))
                    {
                        GhostWhite = false;
                        GhostRed = false;
                        GhostBlue = false;
                        GhostGreen = true;
                        GhostYellow = false;
                        GhostPurple = false;
                    }

                    GUI.contentColor = Color.yellow;
                    if (GUI.Button(new Rect(200f, 729f, 37f, 23f), "■"))
                    {
                        GhostWhite = false;
                        GhostRed = false;
                        GhostBlue = false;
                        GhostGreen = false;
                        GhostYellow = true;
                        GhostPurple = false;
                    }

                    GUI.contentColor = Color.magenta;
                    if (GUI.Button(new Rect(240f, 729f, 37f, 23f), "■"))
                    {
                        GhostWhite = false;
                        GhostRed = false;
                        GhostBlue = false;
                        GhostGreen = false;
                        GhostYellow = false;
                        GhostPurple = true;
                    }

                    GUI.contentColor = Color.white;
                    GUI.Box(new Rect(285f, 676f, 250f, 50f), "Opacity:", gUIStyle);
                    if (GUI.Button(new Rect(280f, 698f, 37f, 23f), "Def"))
                    {
                        GOpcaity1 = true;
                        GOpcaity2 = false;
                        GOpcaity3 = false;
                        GOpcaity4 = false;
                    }

                    if (GUI.Button(new Rect(320f, 698f, 37f, 23f), "Op1"))
                    {
                        GOpcaity1 = false;
                        GOpcaity2 = true;
                        GOpcaity3 = false;
                        GOpcaity4 = false;
                    }

                    if (GUI.Button(new Rect(360f, 698f, 37f, 23f), "Op2"))
                    {
                        GOpcaity1 = false;
                        GOpcaity2 = false;
                        GOpcaity3 = true;
                        GOpcaity4 = false;
                    }

                    if (GUI.Button(new Rect(400f, 698f, 37f, 23f), "Op3"))
                    {
                        GOpcaity1 = false;
                        GOpcaity2 = false;
                        GOpcaity3 = false;
                        GOpcaity4 = true;
                    }

                    GUI.contentColor = Color.white;
                    if (GUI.Button(new Rect(180f, 760f, 150f, 28f), "Save all settings"))
                    {
                        IniFile iniFile = new IniFile("Settings.ini");
                        iniFile.Write("ParticlesO", ParticlesO.ToString());
                        iniFile.Write("menutest", menutest.ToString());
                        iniFile.Write("notheme", notheme.ToString());
                        iniFile.Write("menutest1", menutest1.ToString());
                        iniFile.Write("NoFireballs", NoFireballs.ToString());
                        iniFile.Write("NoBlockBreak", NoBlockBreak.ToString());
                        iniFile.Write("BlockRestart", PluginState.RestartBlockEnabled.ToString());
                        iniFile.Write("EnableGhost", EnableGhost.ToString());
                        iniFile.Write("JumpHeight", JumpHeight.ToString());
                        IniFile iniFile2 = new IniFile("Settings.ini");
                        iniFile2.Write("JumpHeight", JumpHeight.ToString());
                        iniFile2.Write("ParticlesO", ParticlesO.ToString());
                        iniFile2.Write("notheme", notheme.ToString());
                        iniFile2.Write("menutest", menutest.ToString());
                        iniFile2.Write("CycleSaveBoundKey", cycleSaveBoundKey.ToString());
                        iniFile2.Write("ModMenuBoundKey", modMenuBoundKey.ToString());
                        iniFile2.Write("HideMeterBoundKey", hideMeterBoundKey.ToString());
                        iniFile2.Write("CalculateRealTime", calculateRealtime.ToString());
                        iniFile2.Write("CycleLoadBoundKey", cycleLoadBoundKey.ToString());
                        iniFile2.Write("ShortcutsBoundKey", shortcutsBoundKey.ToString());
                        iniFile2.Write("CalculateLastRunRank", calculateLastRunRank.ToString());
                        iniFile2.Write("ShowVerticalVelocity", showVerticalVelocity.ToString());
                        iniFile2.Write("AlwaysShowPeakspeed", alwaysShowPeakSpeed.ToString());
                        iniFile2.Write("ResetPeakOnGround", resetPeakOnGround.ToString());
                        iniFile2.Write("enableDoublePeakList", enableDoublePeakList.ToString());
                        iniFile2.Write("PeakList", PeakList.ToString());
                        iniFile2.Write("PeakListFloat", PeakListFloat.ToString());
                        iniFile2.Write("EnableVelometerGreenLimit", enableVelometerGreenLimit.ToString());
                        iniFile2.Write("VelometerGreenThreshold", velometerGreenThreshold.ToString());
                        iniFile2.Write("VelometerGreenThreshold2", velometerGreenThreshold2.ToString());
                        iniFile2.Write("VelometerGreenThresholdFloat", velometerGreenThresholdFloat.ToString());
                        iniFile2.Write("VelometerGreenThresholdFloat2", velometerGreenThresholdFloat2.ToString());
                        iniFile2.Write("ShowAngleX", showAngleX.ToString());
                        iniFile2.Write("ShowAngleY", showAngleY.ToString());
                        iniFile2.Write("AnglePrecision", anglePrecision.ToString());
                        iniFile2.Write("MouseSensitivity", mouseSens.ToString());
                        iniFile2.Write("ShowStatSpeedDips", showStatSpeedDips.ToString());
                        iniFile2.Write("ShowStatWallTouches", showStatWallTouches.ToString());
                        iniFile2.Write("ShowStatSpeedDipLowest", showStatSpeedDipLowest.ToString());
                        iniFile2.Write("ShowStatHighestPeak", showstatHighestPeak.ToString());
                        iniFile2.Write("ShowStatDistance", showStatDistance.ToString());
                        iniFile2.Write("ShowStatAverageSpeed", showStatAverageSpeed.ToString());
                        iniFile2.Write("orginalWidth", orginalWidth.ToString());
                        iniFile2.Write("orginalHeight", orginalHeight.ToString());
                        iniFile2.Write("infox", infox.ToString());
                        iniFile2.Write("infoy", infoy.ToString());
                        iniFile2.Write("CrossColor2", CrossColor2.ToString());
                        iniFile2.Write("CrossColor3", CrossColor3.ToString());
                        iniFile2.Write("CrossColor4", CrossColor4.ToString());
                        iniFile2.Write("CrossColor5", CrossColor5.ToString());
                        iniFile2.Write("CrossColor6", CrossColor6.ToString());
                        iniFile2.Write("CrossColor1", CrossColor1.ToString());
                        iniFile2.Write("CrossColor7", CrossColor7.ToString());
                        iniFile2.Write("CrossColor8", CrossColor8.ToString());
                        iniFile2.Write("CrossColor9", CrossColor9.ToString());
                        iniFile2.Write("TrailLerp1", TrailLerp1.ToString());
                        iniFile2.Write("TrailLerp2", TrailLerp2.ToString());
                        iniFile2.Write("TrailLerp3", TrailLerp3.ToString());
                        iniFile2.Write("TrailLerp4", TrailLerp4.ToString());
                        iniFile2.Write("TrailLerp5", TrailLerp5.ToString());
                        iniFile2.Write("TrailLerp6", TrailLerp6.ToString());
                        iniFile2.Write("TrailGreen", TrailGreen.ToString());
                        iniFile2.Write("TrailYellow", TrailYellow.ToString());
                        iniFile2.Write("TrailRed", TrailRed.ToString());
                        iniFile2.Write("TrailMagenta", TrailMagenta.ToString());
                        iniFile2.Write("TrailBlue", TrailBlue.ToString());
                        iniFile2.Write("TrailCyan", TrailCyan.ToString());
                        iniFile2.Write("TrailBlack", TrailBlack.ToString());
                        iniFile2.Write("TrailWhite", TrailWhite.ToString());
                        iniFile2.Write("GhostRed", GhostRed.ToString());
                        iniFile2.Write("GhostBlue", GhostBlue.ToString());
                        iniFile2.Write("GhostGreen", GhostGreen.ToString());
                        iniFile2.Write("GhostYellow", GhostYellow.ToString());
                        iniFile2.Write("GhostPurple", GhostPurple.ToString());
                        iniFile2.Write("GOpcaity1", GOpcaity1.ToString());
                        iniFile2.Write("GOpcaity2", GOpcaity2.ToString());
                        iniFile2.Write("GOpcaity4", GOpcaity4.ToString());
                        iniFile2.Write("GOpcaity3", GOpcaity3.ToString());
                    }
                }
                else
                {
                    Sub1String = "<color=white>Main Options</color>";
                }

                if (MenuSub2)
                {
                    Sub2String = "<color=lime>Maps Menu</color>";
                    if (!Game.isSpeedrun() && !Game.isEndless() && Game.startedFrom != StartedFrom.WORKSHOP)
                    {
                        GUI.contentColor = Color.white;
                        GUI.Box(new Rect(65f, 276f, 250f, 50f), "Heliku Levels:", gUIStyle);
                        GUI.contentColor = Color.green;
                        if (GUI.Button(new Rect(60f, 295f, 75f, 25f), "h1") && Game.currentLevel != 147)
                        {
                            Game.startLevel(147);
                        }

                        if (GUI.Button(new Rect(137f, 295f, 75f, 25f), "h2") && Game.currentLevel != 148)
                        {
                            Game.startLevel(148);
                        }

                        if (GUI.Button(new Rect(217f, 295f, 75f, 25f), "h3") && Game.currentLevel != 60)
                        {
                            Game.startLevel(60);
                        }

                        if (GUI.Button(new Rect(297f, 295f, 75f, 25f), "h4") && Game.currentLevel != 32)
                        {
                            Game.startLevel(32);
                        }

                        if (GUI.Button(new Rect(377f, 295f, 75f, 25f), "h5") && Game.currentLevel != 119)
                        {
                            Game.startLevel(119);
                        }

                        if (GUI.Button(new Rect(60f, 325f, 75f, 25f), "h6") && Game.currentLevel != 128)
                        {
                            Game.startLevel(128);
                        }

                        if (GUI.Button(new Rect(137f, 325f, 75f, 25f), "h7") && Game.currentLevel != 26)
                        {
                            Game.startLevel(26);
                        }

                        if (GUI.Button(new Rect(217f, 325f, 75f, 25f), "h8") && Game.currentLevel != 127)
                        {
                            Game.startLevel(127);
                        }

                        GUI.contentColor = Color.white;
                        GUI.Box(new Rect(65f, 355f, 250f, 50f), "Secret Levels:", gUIStyle);
                        GUI.contentColor = Color.yellow;
                        if (GUI.Button(new Rect(60f, 374f, 75f, 25f), "S1") && Game.currentLevel != 149)
                        {
                            Game.startLevel(149);
                        }

                        if (GUI.Button(new Rect(137f, 374f, 75f, 25f), "S2") && Game.currentLevel != 3)
                        {
                            Game.startLevel(3);
                        }

                        if (GUI.Button(new Rect(217f, 374f, 75f, 25f), "S3") && Game.currentLevel != 4)
                        {
                            Game.startLevel(4);
                        }

                        if (GUI.Button(new Rect(297f, 374f, 75f, 25f), "S4") && Game.currentLevel != 2)
                        {
                            Game.startLevel(2);
                        }

                        if (GUI.Button(new Rect(377f, 374f, 75f, 25f), "S5") && Game.currentLevel != 80)
                        {
                            Game.startLevel(80);
                        }

                        if (GUI.Button(new Rect(60f, 404f, 75f, 25f), "S6") && Game.currentLevel != 165)
                        {
                            Game.startLevel(165);
                        }

                        if (GUI.Button(new Rect(137f, 404f, 75f, 25f), "S7") && Game.currentLevel != 181)
                        {
                            Game.startLevel(181);
                        }

                        GUI.contentColor = Color.magenta;
                        if (GUI.Button(new Rect(217f, 404f, 75f, 25f), "S9") && Game.currentLevel != 167 && Game.hasDLC(DLC.DRUNK_SIDE_OF_THE_MOON))
                        {
                            Game.startLevel(167);
                        }

                        if (GUI.Button(new Rect(297f, 404f, 75f, 25f), "S10") && Game.currentLevel != 143 && Game.hasDLC(DLC.DRUNK_SIDE_OF_THE_MOON))
                        {
                            Game.startLevel(143);
                        }

                        if (GUI.Button(new Rect(377f, 404f, 75f, 25f), "S11") && Game.currentLevel != 111 && Game.hasDLC(DLC.DRUNK_SIDE_OF_THE_MOON))
                        {
                            Game.startLevel(111);
                        }

                        GUI.contentColor = Color.white;
                        GUI.Box(new Rect(65f, 434f, 250f, 50f), "666 level:", gUIStyle);
                        GUI.contentColor = Color.red;
                        if (GUI.Button(new Rect(60f, 453f, 75f, 25f), "6:6:6") && Game.currentLevel != 91)
                        {
                            Game.startLevel(91);
                        }

                        GUI.contentColor = Color.white;
                        GUI.Box(new Rect(65f, 483f, 250f, 50f), "Unreleased levels:", gUIStyle);
                        if (GUI.Button(new Rect(60f, 502f, 190f, 25f), "Head Against the Wall") && Game.currentLevel != 46)
                        {
                            Game.startLevel(46);
                        }

                        if (GUI.Button(new Rect(260f, 502f, 190f, 25f), "Looong Jump") && Game.currentLevel != 110)
                        {
                            Game.startLevel(110);
                        }

                        if (GUI.Button(new Rect(60f, 532f, 190f, 25f), "Super Street Crosser") && Game.currentLevel != 95)
                        {
                            Game.startLevel(95);
                        }

                        if (GUI.Button(new Rect(260f, 532f, 190f, 25f), "Walkthrough") && Game.currentLevel != 185)
                        {
                            Game.startLevel(185);
                        }

                        if (GUI.Button(new Rect(60f, 562f, 190f, 25f), "Abandon All Hope") && Game.currentLevel != 0)
                        {
                            Game.startLevel(0);
                        }

                        if (GUI.Button(new Rect(260f, 562f, 190f, 25f), "Wall Street") && Game.currentLevel != 141)
                        {
                            Game.startLevel(141);
                        }

                        if (GUI.Button(new Rect(60f, 592f, 190f, 25f), "Christmas Tree") && Game.currentLevel != 108)
                        {
                            Game.startLevel(108);
                        }

                        if (GUI.Button(new Rect(260f, 592f, 190f, 25f), "TempleVar") && Game.currentLevel != 20)
                        {
                            Game.startLevel(20);
                        }

                        if (GUI.Button(new Rect(60f, 622f, 190f, 25f), "Fall Through 2") && Game.currentLevel != 124)
                        {
                            Game.startLevel(124);
                        }

                        if (GUI.Button(new Rect(260f, 622f, 190f, 25f), "Clockwork") && Game.currentLevel != 51)
                        {
                            Game.startLevel(51);
                        }

                        if (GUI.Button(new Rect(60f, 652f, 190f, 25f), "Chimney Sweep") && Game.currentLevel != 105)
                        {
                            Game.startLevel(105);
                        }

                        if (GUI.Button(new Rect(260f, 652f, 190f, 25f), "Blink-old") && Game.currentLevel != 7)
                        {
                            Game.startLevel(7);
                        }

                        if (GUI.Button(new Rect(60f, 682f, 190f, 25f), "Selector Menu") && Game.currentLevel != 118)
                        {
                            Game.startLevel(118);
                        }

                        if (GUI.Button(new Rect(260f, 682f, 190f, 25f), "Incoming") && Game.currentLevel != 77)
                        {
                            Game.startLevel(77);
                        }

                        if (GUI.Button(new Rect(60f, 712f, 190f, 25f), "Flytrap Old") && Game.currentLevel != 21)
                        {
                            Game.startLevel(21);
                        }

                        if (GUI.Button(new Rect(260f, 712f, 190f, 25f), "Vladekov") && Game.currentLevel != 8)
                        {
                            Game.startLevel(8);
                        }

                        if (GUI.Button(new Rect(60f, 742f, 190f, 25f), "Hit and Fly") && Game.currentLevel != 152)
                        {
                            Game.startLevel(152);
                        }

                        if (GUI.Button(new Rect(260f, 742f, 190f, 25f), "Gateway") && Game.currentLevel != 1)
                        {
                            Game.startLevel(1);
                        }

                        GUI.contentColor = Color.white;
                        GUI.Box(new Rect(65f, 772f, 250f, 50f), "Prototype levels:", gUIStyle);
                        if (GUI.Button(new Rect(60f, 791f, 35f, 25f), "#1") && Game.currentLevel != 75)
                        {
                            Game.startLevel(75);
                        }

                        if (GUI.Button(new Rect(100f, 791f, 35f, 25f), "#2") && Game.currentLevel != 100)
                        {
                            Game.startLevel(100);
                        }

                        if (GUI.Button(new Rect(140f, 791f, 35f, 25f), "#3") && Game.currentLevel != 131)
                        {
                            Game.startLevel(131);
                        }

                        if (GUI.Button(new Rect(180f, 791f, 35f, 25f), "#4") && Game.currentLevel != 138)
                        {
                            Game.startLevel(138);
                        }

                        if (GUI.Button(new Rect(220f, 791f, 35f, 25f), "#5") && Game.currentLevel != 163)
                        {
                            Game.startLevel(163);
                        }

                        if (GUI.Button(new Rect(260f, 791f, 35f, 25f), "#6") && Game.currentLevel != 164)
                        {
                            Game.startLevel(164);
                        }

                        if (GUI.Button(new Rect(300f, 791f, 35f, 25f), "#7") && Game.currentLevel != 125)
                        {
                            Game.startLevel(125);
                        }

                        if (GUI.Button(new Rect(340f, 791f, 35f, 25f), "#8") && Game.currentLevel != 176)
                        {
                            Game.startLevel(176);
                        }

                        if (GUI.Button(new Rect(380f, 791f, 35f, 25f), "#9") && Game.currentLevel != 22)
                        {
                            Game.startLevel(22);
                        }
                    }
                    else
                    {
                        GUI.contentColor = Color.red;
                        GUI.Box(new Rect(100f, 300f, 250f, 60f), "This menu is not available in this mode.", gUIStyle3);
                        GUI.contentColor = Color.white;
                    }
                }
                else
                {
                    Sub2String = "<color=white>Maps Menu</color>";
                }

                if (MenuSub3)
                {
                    Sub3String = "<color=lime>Game tips</color>";
                    GUI.contentColor = Color.white;
                    GUI.Box(new Rect(65f, 276f, 250f, 50f), "Cycles and Setups:", gUIStyle);
                    scrollPosition = GUI.BeginScrollView(new Rect(60f, 296f, 390f, 200f), scrollPosition, new Rect(0f, 0f, 190f, 850f));
                    innerText = "Cycles: \n• Swingers (11) = 0.367 - 0.4 \n• Sawmill (18) = 4.65 @179.1 angle\n• Centrifuge (23) = 1.7-1.75\n• Dive (29) = 1.4-1.65\n• Fan (37) = 69.55 - 69.6\n• Locksmith (40) = ~4.45\n• Lab Rat (41) = 0.65-1.3\n• Playground (42) = ~0.4 \n• Gutenburg (47) = 7.25-7.35\n• Birdies = Cycle cant be set with gentle meter, watch the middle cage and go just between when it stops turning left and starts turning right\n• Heist (50) = 1.4-1.9\n• Devils Pass (56) = 1.2-1.233\n• Mine Field (68) = 0.3 ( Cycle is locked !)\n• Lamb On The Spit (70) = 1.5-1.6\n• Broadsword (80) = 0-0.1\n• Satan (81) = 1.2-1.6\n• Circus (X7) = ~2.7\n• Waffle (X10) = 0.3-0.45\n• Easy Dizzy (Y7) = ~0.2\n• Hexagon (D11) = 0\n• Ballbearing (D22) = 0\n• Butterfly (D26) = 0.4-0.7\n• Casper (H2) = ~ 0.9\n• Patience (H3) = 1.45-1.483\n• Immortality Machine (H4) = 1.3-1.9\n• Burnt Waffle (S9) = 0.3-0.4 right way / ~0.9 straight \n• Turntable (S10) = 13.8-14.4\n\nSetups:\n• Sleigh (79) = 253, 48\n• Beer Heaven (82) = 180, -16.75 ( 80% stick)\n• Farming (H5) = 186 , 75.7";
                    innerText = GUI.TextArea(new Rect(0f, 0f, 372f, 850f), innerText);
                    GUI.EndScrollView();
                    GUI.DrawTexture(new Rect(60f, 296.2f, 372f, 1.5f), Resources.Load<Texture2D>("speedrunhudpsd/scoreselection"));
                    GUI.DrawTexture(new Rect(60f, 494.5f, 372f, 1.5f), Resources.Load<Texture2D>("speedrunhudpsd/scoreselection"));
                    GUI.DrawTexture(new Rect(60.1f, 296f, 1.5f, 200f), Resources.Load<Texture2D>("speedrunhudpsd/scoreselection"));
                    GUI.DrawTexture(new Rect(430.9f, 296f, 1.5f, 200f), Resources.Load<Texture2D>("speedrunhudpsd/scoreselection"));
                    GUI.Box(new Rect(65f, 506f, 250f, 50f), "Helikus secrets:", gUIStyle);
                    scrollPosition3 = GUI.BeginScrollView(new Rect(60f, 526f, 390f, 200f), scrollPosition3, new Rect(0f, 0f, 190f, 600f));
                    innerText3 = "• H1: Go to 67-The walk and do a ballerina (jump and do a 360) 6 times on the tightrope, then finish the level within the time limit\n\n• H2: Go to 24-Babel and finish the Level without using any ghost blocks, before time runs out.\n\n• H3: Go to 10-Charge and get “salmon” (Swaying your mouse left and right quickly) a few times during the level then finish the level within the time limit.\n\n• H4: Go to B8-Firecraft and break 66 blocks only using headbangs (quick vertical motion with the mouse).\n\n• H5: Go to 13-Jumping Jacks and press your taunt key on every platform (including the one with the portal) before you get a “too slow”.\n\n• H6: Go to 31-The Long Road and cover the second platform in blood,  a small spot will do, like 10 deaths.\n\n• H7: Go to 41-Lab Rat and finish the level without pressing the W key and before time runs out.(you are allowed to turn around so long as you don’t walk forward).\n\n• H8: Go to 66-Spider and finish the level without using WASD.";
                    innerText3 = GUI.TextArea(new Rect(0f, 0f, 372f, 600f), innerText3);
                    GUI.EndScrollView();
                    GUI.DrawTexture(new Rect(60f, 526.2f, 372f, 1.5f), Resources.Load<Texture2D>("speedrunhudpsd/scoreselection"));
                    GUI.DrawTexture(new Rect(60f, 723.5f, 372f, 1.5f), Resources.Load<Texture2D>("speedrunhudpsd/scoreselection"));
                    GUI.DrawTexture(new Rect(60.1f, 526f, 1.5f, 200f), Resources.Load<Texture2D>("speedrunhudpsd/scoreselection"));
                    GUI.DrawTexture(new Rect(430.9f, 526f, 1.5f, 200f), Resources.Load<Texture2D>("speedrunhudpsd/scoreselection"));
                }
                else
                {
                    Sub3String = "<color=white>Game tips</color>";
                }

                if (MenuSub4)
                {
                    Sub4String = "<color=lime>Settings</color>";
                    GUI.Box(new Rect(65f, 275f, 240f, 25f), "Resolutions:", gUIStyle);
                    if (GUI.Button(new Rect(60f, 295f, 74f, 25f), "1920x1080"))
                    {
                        Screen.SetResolution(1920, 1080, fullscreen: true);
                    }

                    if (GUI.Button(new Rect(137f, 295f, 74f, 25f), "1600x900"))
                    {
                        Screen.SetResolution(1600, 900, fullscreen: true);
                    }

                    if (GUI.Button(new Rect(217f, 295f, 74f, 25f), "1440x1080"))
                    {
                        Screen.SetResolution(1440, 1080, fullscreen: true);
                    }

                    if (GUI.Button(new Rect(297f, 295f, 74f, 25f), "1024x800"))
                    {
                        Screen.SetResolution(1024, 800, fullscreen: true);
                    }

                    if (GUI.Button(new Rect(377f, 295f, 74f, 25f), "960x540"))
                    {
                        Screen.SetResolution(960, 540, fullscreen: true);
                    }

                    if (GUI.Button(new Rect(60f, 325f, 74f, 25f), "800x640"))
                    {
                        Screen.SetResolution(800, 640, fullscreen: true);
                    }

                    stringToEditRez1 = GUI.TextField(new Rect(137f, 325f, 74f, 25f), stringToEditRez1, 4);
                    stringToEditRez2 = GUI.TextField(new Rect(217f, 325f, 74f, 25f), stringToEditRez2, 4);
                    if (GUI.Button(new Rect(297f, 325f, 74f, 25f), "Set"))
                    {
                        if (int.TryParse(stringToEditRez1, out stringToEditRez1Int))
                        {
                            stringToEditRez1Int = int.Parse(stringToEditRez1);
                        }
                        else
                        {
                            stringToEditRez1 = "ERROR";
                        }

                        if (int.TryParse(stringToEditRez2, out stringToEditRez2Int))
                        {
                            stringToEditRez2Int = int.Parse(stringToEditRez2);
                        }
                        else
                        {
                            stringToEditRez2 = "ERROR";
                        }

                        if (stringToEditRez1Int < 600 || stringToEditRez2Int < 600)
                        {
                            Screen.SetResolution(600, 600, fullscreen: true);
                        }
                        else
                        {
                            Screen.SetResolution(stringToEditRez1Int, stringToEditRez2Int, fullscreen: true);
                        }
                    }

                    GUI.Box(new Rect(65f, 355f, 240f, 25f), "Binds:", gUIStyle);
                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUI.Label(new Rect(202f, 375f, 250f, 25f), "to Open/Close the Velocity Meter");
                    GUI.skin.textField.alignment = TextAnchor.MiddleCenter;
                    GUI.TextField(new Rect(147f, 375f, 50f, 24f), modMenuBoundKey.ToString(), 20);
                    GUI.skin.textField.alignment = TextAnchor.MiddleLeft;
                    if (!bindMenuButton)
                    {
                        if (GUI.Button(new Rect(65f, 375f, 75f, 25f), "Bind") && !bindingButton)
                        {
                            bindMenuButton = true;
                            bindingButton = true;
                        }
                    }
                    else if (bindMenuButton)
                    {
                        if (!keybound)
                        {
                            modMenuBoundKey = bindKey(modMenuBoundKey);
                            GUI.Button(new Rect(65f, 375f, 75f, 25f), "Key?");
                        }
                        else
                        {
                            new IniFile("Settings.ini").Write("ModMenuBoundKey", modMenuBoundKey.ToString());
                            keybound = false;
                            bindingButton = false;
                            bindMenuButton = false;
                        }
                    }

                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUI.Label(new Rect(202f, 405f, 250f, 25f), "to save the cycle");
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                    GUI.skin.textField.alignment = TextAnchor.MiddleCenter;
                    GUI.TextField(new Rect(147f, 405f, 50f, 24f), cycleSaveBoundKey.ToString(), 20);
                    GUI.skin.textField.alignment = TextAnchor.MiddleLeft;
                    if (!bindCycleSaveButton)
                    {
                        if (GUI.Button(new Rect(65f, 405f, 75f, 25f), "Bind") && !bindingButton)
                        {
                            bindCycleSaveButton = true;
                            bindingButton = true;
                        }
                    }
                    else if (bindCycleSaveButton)
                    {
                        if (!keybound)
                        {
                            cycleSaveBoundKey = bindKey(cycleSaveBoundKey);
                            GUI.Button(new Rect(65f, 405f, 75f, 25f), "Key?");
                        }
                        else
                        {
                            new IniFile("Settings.ini").Write("CycleSaveBoundKey", cycleSaveBoundKey.ToString());
                            keybound = false;
                            bindingButton = false;
                            bindCycleSaveButton = false;
                        }
                    }

                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUI.Label(new Rect(202f, 435f, 250f, 25f), "or ScrollWheel to load the cycle");
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                    GUI.skin.textField.alignment = TextAnchor.MiddleCenter;
                    GUI.TextField(new Rect(147f, 435f, 50f, 24f), cycleLoadBoundKey.ToString(), 20);
                    GUI.skin.textField.alignment = TextAnchor.MiddleLeft;
                    if (!bindCycleLoadButton)
                    {
                        if (GUI.Button(new Rect(65f, 435f, 75f, 25f), "Bind") && !bindingButton)
                        {
                            bindCycleLoadButton = true;
                            bindingButton = true;
                        }
                    }
                    else if (bindCycleLoadButton)
                    {
                        if (!keybound)
                        {
                            new IniFile("Settings.ini").Write("CycleLoadBoundKey", cycleLoadBoundKey.ToString());
                            cycleLoadBoundKey = bindKey(cycleLoadBoundKey);
                            GUI.Button(new Rect(65f, 435f, 75f, 25f), "Key?");
                        }
                        else
                        {
                            keybound = false;
                            bindingButton = false;
                            bindCycleLoadButton = false;
                        }
                    }

                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    GUI.Label(new Rect(202f, 465f, 250f, 25f), "to hide the meter");
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                    GUI.skin.textField.alignment = TextAnchor.MiddleCenter;
                    GUI.TextField(new Rect(147f, 465f, 50f, 25f), hideMeterBoundKey.ToString(), 20);
                    GUI.skin.textField.alignment = TextAnchor.MiddleLeft;
                    if (!bindHideMeter)
                    {
                        if (GUI.Button(new Rect(65f, 465f, 75f, 25f), "Bind") && !bindingButton)
                        {
                            bindHideMeter = true;
                            bindingButton = true;
                        }
                    }
                    else if (bindHideMeter)
                    {
                        new IniFile("Settings.ini").Write("HideMeterBoundKey", hideMeterBoundKey.ToString());
                        if (!keybound)
                        {
                            hideMeterBoundKey = bindKey(hideMeterBoundKey);
                            GUI.Button(new Rect(65f, 465f, 75f, 25f), "Key?");
                        }
                        else
                        {
                            keybound = false;
                            bindingButton = false;
                            bindHideMeter = false;
                        }
                    }

                    if (GUI.Button(new Rect(65f, 495f, 150f, 25f), "Reset binds settings") && !bindingButton)
                    {
                        modMenuBoundKey = KeyCode.Y;
                        shortcutsBoundKey = KeyCode.I;
                        cycleSaveBoundKey = KeyCode.C;
                        cycleLoadBoundKey = KeyCode.Q;
                        hideMeterBoundKey = KeyCode.H;
                        IniFile iniFile3 = new IniFile("Settings.ini");
                        iniFile3.Write("ModMenuBoundKey", modMenuBoundKey.ToString());
                        iniFile3.Write("ShortcutsBoundKey", shortcutsBoundKey.ToString());
                        iniFile3.Write("CycleSaveBoundKey", cycleSaveBoundKey.ToString());
                        iniFile3.Write("CycleLoadBoundKey", cycleLoadBoundKey.ToString());
                        iniFile3.Write("HideMeterBoundKey", hideMeterBoundKey.ToString());
                    }

                    GUI.contentColor = Color.white;
                    GUI.Box(new Rect(65f, 527f, 240f, 25f), "Menu size:", gUIStyle);
                    if (GUI.Button(new Rect(60f, 547f, 90f, 25f), "Default"))
                    {
                        orginalWidth = 1920f;
                        orginalHeight = 1080f;
                        SceneManager.LoadScene("Game");
                        showMenu = false;
                        GameSettings.settings.mouseSensitivity = oldMouseSense;
                    }

                    if (GUI.Button(new Rect(160f, 547f, 90f, 25f), "Little"))
                    {
                        orginalWidth = 2560f;
                        orginalHeight = 1440f;
                        SceneManager.LoadScene("Game");
                        showMenu = false;
                        GameSettings.settings.mouseSensitivity = oldMouseSense;
                    }

                    GUI.Box(new Rect(265f, 527f, 240f, 25f), "Info position:", gUIStyle);
                    if (GUI.Button(new Rect(260f, 547f, 37f, 23f), "Def"))
                    {
                        infox = 0f;
                        infoy = 0f;
                    }

                    if (GUI.Button(new Rect(298f, 547f, 37f, 23f), "+ x"))
                    {
                        infox += 5f;
                    }

                    if (GUI.Button(new Rect(336f, 547f, 37f, 23f), "- x"))
                    {
                        infox += -5f;
                    }

                    if (GUI.Button(new Rect(374f, 547f, 37f, 23f), "+ y"))
                    {
                        infoy += 5f;
                    }

                    if (GUI.Button(new Rect(412f, 547f, 37f, 23f), "- y"))
                    {
                        infoy += -5f;
                    }

                    GUI.Box(new Rect(65f, 577f, 240f, 25f), "Trails color:", gUIStyle);
                    if (GUI.Button(new Rect(60f, 597f, 37f, 23f), "Def"))
                    {
                        TrailLerp1 = false;
                        TrailLerp2 = false;
                        TrailLerp3 = false;
                        TrailLerp4 = false;
                        TrailLerp5 = false;
                        TrailLerp6 = false;
                        TrailGreen = false;
                        TrailYellow = false;
                        TrailRed = false;
                        TrailMagenta = false;
                        TrailBlue = false;
                        TrailCyan = false;
                        TrailBlack = false;
                        TrailWhite = true;
                    }

                    GUI.contentColor = Color.green;
                    if (GUI.Button(new Rect(98f, 597f, 37f, 23f), "■"))
                    {
                        TrailLerp1 = false;
                        TrailLerp2 = false;
                        TrailLerp3 = false;
                        TrailLerp4 = false;
                        TrailLerp5 = false;
                        TrailLerp6 = false;
                        TrailWhite = false;
                        TrailYellow = false;
                        TrailRed = false;
                        TrailMagenta = false;
                        TrailBlue = false;
                        TrailCyan = false;
                        TrailBlack = false;
                        TrailGreen = true;
                    }

                    GUI.contentColor = Color.red;
                    if (GUI.Button(new Rect(136f, 597f, 37f, 23f), "■"))
                    {
                        TrailLerp1 = false;
                        TrailLerp2 = false;
                        TrailLerp3 = false;
                        TrailLerp4 = false;
                        TrailLerp5 = false;
                        TrailLerp6 = false;
                        TrailWhite = false;
                        TrailGreen = false;
                        TrailYellow = false;
                        TrailMagenta = false;
                        TrailBlue = false;
                        TrailCyan = false;
                        TrailBlack = false;
                        TrailRed = true;
                    }

                    GUI.contentColor = Color.magenta;
                    if (GUI.Button(new Rect(174f, 597f, 37f, 23f), "■"))
                    {
                        TrailLerp1 = false;
                        TrailLerp2 = false;
                        TrailLerp3 = false;
                        TrailLerp4 = false;
                        TrailLerp5 = false;
                        TrailLerp6 = false;
                        TrailWhite = false;
                        TrailGreen = false;
                        TrailYellow = false;
                        TrailBlue = false;
                        TrailCyan = false;
                        TrailBlack = false;
                        TrailRed = false;
                        TrailMagenta = true;
                    }

                    GUI.contentColor = Color.blue;
                    if (GUI.Button(new Rect(212f, 597f, 37f, 23f), "■"))
                    {
                        TrailLerp1 = false;
                        TrailLerp2 = false;
                        TrailLerp3 = false;
                        TrailLerp4 = false;
                        TrailLerp5 = false;
                        TrailLerp6 = false;
                        TrailWhite = false;
                        TrailGreen = false;
                        TrailYellow = false;
                        TrailMagenta = false;
                        TrailRed = false;
                        TrailCyan = false;
                        TrailBlack = false;
                        TrailBlue = true;
                    }

                    GUI.contentColor = Color.black;
                    if (GUI.Button(new Rect(250f, 597f, 37f, 23f), "■"))
                    {
                        TrailLerp1 = false;
                        TrailLerp2 = false;
                        TrailLerp3 = false;
                        TrailLerp4 = false;
                        TrailLerp5 = false;
                        TrailLerp6 = false;
                        TrailWhite = false;
                        TrailGreen = false;
                        TrailYellow = false;
                        TrailMagenta = false;
                        TrailRed = false;
                        TrailCyan = false;
                        TrailBlue = false;
                        TrailBlack = true;
                    }

                    GUI.contentColor = Color.Lerp(Color.green, Color.magenta, Mathf.PingPong(Time.time, 1f));
                    if (GUI.Button(new Rect(288f, 597f, 37f, 23f), "■"))
                    {
                        TrailWhite = false;
                        TrailGreen = false;
                        TrailYellow = false;
                        TrailMagenta = false;
                        TrailRed = false;
                        TrailCyan = false;
                        TrailBlue = false;
                        TrailBlack = false;
                        TrailLerp6 = false;
                        TrailLerp2 = false;
                        TrailLerp3 = false;
                        TrailLerp4 = false;
                        TrailLerp5 = false;
                        TrailLerp1 = true;
                    }

                    GUI.contentColor = Color.Lerp(Color.blue, Color.magenta, Mathf.PingPong(Time.time, 1f));
                    if (GUI.Button(new Rect(326f, 597f, 37f, 23f), "■"))
                    {
                        TrailWhite = false;
                        TrailGreen = false;
                        TrailYellow = false;
                        TrailMagenta = false;
                        TrailRed = false;
                        TrailCyan = false;
                        TrailBlue = false;
                        TrailBlack = false;
                        TrailLerp1 = false;
                        TrailLerp6 = false;
                        TrailLerp3 = false;
                        TrailLerp4 = false;
                        TrailLerp5 = false;
                        TrailLerp2 = true;
                    }

                    GUI.contentColor = Color.Lerp(Color.blue, Color.green, Mathf.PingPong(Time.time, 1f));
                    if (GUI.Button(new Rect(364f, 597f, 37f, 23f), "■"))
                    {
                        TrailWhite = false;
                        TrailGreen = false;
                        TrailYellow = false;
                        TrailMagenta = false;
                        TrailRed = false;
                        TrailCyan = false;
                        TrailBlue = false;
                        TrailBlack = false;
                        TrailLerp1 = false;
                        TrailLerp2 = false;
                        TrailLerp6 = false;
                        TrailLerp4 = false;
                        TrailLerp5 = false;
                        TrailLerp3 = true;
                    }

                    GUI.contentColor = Color.Lerp(Color.yellow, Color.magenta, Mathf.PingPong(Time.time, 1f));
                    if (GUI.Button(new Rect(402f, 597f, 37f, 23f), "■"))
                    {
                        TrailWhite = false;
                        TrailGreen = false;
                        TrailYellow = false;
                        TrailMagenta = false;
                        TrailRed = false;
                        TrailCyan = false;
                        TrailBlue = false;
                        TrailBlack = false;
                        TrailLerp1 = false;
                        TrailLerp2 = false;
                        TrailLerp3 = false;
                        TrailLerp4 = false;
                        TrailLerp6 = false;
                        TrailLerp5 = true;
                    }

                    GUI.contentColor = Color.white;
                    GUI.Box(new Rect(65f, 627f, 310f, 25f), "Crosshair color:", gUIStyle);
                    if (GUI.Button(new Rect(60f, 647f, 37f, 25f), "Def"))
                    {
                        CrossColor2 = false;
                        CrossColor3 = false;
                        CrossColor4 = false;
                        CrossColor5 = false;
                        CrossColor6 = false;
                        CrossColor7 = false;
                        CrossColor8 = false;
                        CrossColor9 = false;
                        CrossColor1 = true;
                    }

                    GUI.contentColor = Color.green;
                    if (GUI.Button(new Rect(98f, 647f, 37f, 25f), "■"))
                    {
                        CrossColor1 = false;
                        CrossColor3 = false;
                        CrossColor4 = false;
                        CrossColor5 = false;
                        CrossColor6 = false;
                        CrossColor7 = false;
                        CrossColor8 = false;
                        CrossColor9 = false;
                        CrossColor2 = true;
                    }

                    GUI.contentColor = Color.yellow;
                    if (GUI.Button(new Rect(136f, 647f, 37f, 25f), "■"))
                    {
                        CrossColor1 = false;
                        CrossColor2 = false;
                        CrossColor4 = false;
                        CrossColor5 = false;
                        CrossColor6 = false;
                        CrossColor7 = false;
                        CrossColor8 = false;
                        CrossColor9 = false;
                        CrossColor3 = true;
                    }

                    GUI.contentColor = Color.red;
                    if (GUI.Button(new Rect(174f, 647f, 37f, 25f), "■"))
                    {
                        CrossColor1 = false;
                        CrossColor2 = false;
                        CrossColor3 = false;
                        CrossColor5 = false;
                        CrossColor6 = false;
                        CrossColor7 = false;
                        CrossColor8 = false;
                        CrossColor9 = false;
                        CrossColor4 = true;
                    }

                    GUI.contentColor = Color.blue;
                    if (GUI.Button(new Rect(212f, 647f, 37f, 25f), "■"))
                    {
                        CrossColor1 = false;
                        CrossColor2 = false;
                        CrossColor3 = false;
                        CrossColor4 = false;
                        CrossColor6 = false;
                        CrossColor7 = false;
                        CrossColor8 = false;
                        CrossColor9 = false;
                        CrossColor5 = true;
                    }

                    GUI.contentColor = Color.black;
                    if (GUI.Button(new Rect(250f, 647f, 37f, 25f), "■"))
                    {
                        CrossColor1 = false;
                        CrossColor2 = false;
                        CrossColor3 = false;
                        CrossColor4 = false;
                        CrossColor5 = false;
                        CrossColor7 = false;
                        CrossColor8 = false;
                        CrossColor9 = false;
                        CrossColor6 = true;
                    }

                    GUI.contentColor = new Color(0.8f, 0.058f, 0.682f);
                    if (GUI.Button(new Rect(288f, 647f, 37f, 25f), "■"))
                    {
                        CrossColor2 = false;
                        CrossColor3 = false;
                        CrossColor4 = false;
                        CrossColor5 = false;
                        CrossColor6 = false;
                        CrossColor1 = false;
                        CrossColor8 = false;
                        CrossColor9 = false;
                        CrossColor7 = true;
                    }

                    GUI.contentColor = new Color(0.125f, 0.811f, 0.784f);
                    if (GUI.Button(new Rect(326f, 647f, 37f, 25f), "■"))
                    {
                        CrossColor2 = false;
                        CrossColor3 = false;
                        CrossColor4 = false;
                        CrossColor5 = false;
                        CrossColor6 = false;
                        CrossColor1 = false;
                        CrossColor7 = false;
                        CrossColor9 = false;
                        CrossColor8 = true;
                    }

                    GUI.contentColor = new Color(0.811f, 0.427f, 0.125f);
                    if (GUI.Button(new Rect(364f, 647f, 37f, 25f), "■"))
                    {
                        CrossColor1 = false;
                        CrossColor2 = false;
                        CrossColor3 = false;
                        CrossColor4 = false;
                        CrossColor5 = false;
                        CrossColor6 = false;
                        CrossColor7 = false;
                        CrossColor8 = false;
                        CrossColor9 = true;
                    }

                    GUI.contentColor = Color.white;
                    GUI.Box(new Rect(65f, 677f, 250f, 50f), "Edit theme:", gUIStyle);
                    if (GUI.Button(new Rect(60f, 697f, 75f, 25f), "Def"))
                    {
                        Theme4 = false;
                        Theme3 = false;
                        Theme2 = false;
                        Theme5 = false;
                        Theme6 = false;
                        Theme7 = false;
                        Theme8 = false;
                        Theme9 = false;
                        Theme10 = false;
                        Theme1 = true;
                    }

                    if (GUI.Button(new Rect(137f, 697f, 75f, 25f), "T2"))
                    {
                        Theme5 = false;
                        Theme6 = false;
                        Theme4 = false;
                        Theme3 = false;
                        Theme2 = true;
                        Theme7 = false;
                        Theme8 = false;
                        Theme9 = false;
                        Theme10 = false;
                        Theme1 = false;
                    }

                    if (GUI.Button(new Rect(214f, 697f, 75f, 25f), "T3"))
                    {
                        Theme5 = false;
                        Theme6 = false;
                        Theme4 = false;
                        Theme2 = false;
                        Theme1 = false;
                        Theme3 = true;
                        Theme7 = false;
                        Theme8 = false;
                        Theme9 = false;
                        Theme10 = false;
                    }

                    if (GUI.Button(new Rect(291f, 697f, 75f, 25f), "T4"))
                    {
                        Theme5 = false;
                        Theme6 = false;
                        Theme3 = false;
                        Theme2 = false;
                        Theme1 = false;
                        Theme4 = true;
                        Theme7 = false;
                        Theme8 = false;
                        Theme9 = false;
                        Theme10 = false;
                    }

                    if (GUI.Button(new Rect(368f, 697f, 75f, 25f), "T5"))
                    {
                        Theme4 = false;
                        Theme6 = false;
                        Theme3 = false;
                        Theme2 = false;
                        Theme1 = false;
                        Theme5 = true;
                        Theme7 = false;
                        Theme8 = false;
                        Theme9 = false;
                        Theme10 = false;
                    }

                    if (GUI.Button(new Rect(60f, 727f, 75f, 25f), "T6"))
                    {
                        Theme5 = false;
                        Theme4 = false;
                        Theme3 = false;
                        Theme2 = false;
                        Theme1 = false;
                        Theme6 = true;
                        Theme7 = false;
                        Theme8 = false;
                        Theme9 = false;
                        Theme10 = false;
                    }

                    if (GUI.Button(new Rect(137f, 727f, 75f, 25f), "T7"))
                    {
                        Theme5 = false;
                        Theme6 = false;
                        Theme4 = false;
                        Theme3 = false;
                        Theme2 = false;
                        Theme7 = true;
                        Theme8 = false;
                        Theme9 = false;
                        Theme10 = false;
                        Theme1 = false;
                    }

                    if (GUI.Button(new Rect(214f, 727f, 75f, 25f), "T8"))
                    {
                        Theme5 = false;
                        Theme6 = false;
                        Theme4 = false;
                        Theme2 = false;
                        Theme1 = false;
                        Theme3 = false;
                        Theme7 = false;
                        Theme8 = true;
                        Theme9 = false;
                        Theme10 = false;
                    }

                    if (GUI.Button(new Rect(291f, 727f, 75f, 25f), "T9"))
                    {
                        Theme5 = false;
                        Theme6 = false;
                        Theme3 = false;
                        Theme2 = false;
                        Theme1 = false;
                        Theme4 = false;
                        Theme7 = false;
                        Theme8 = false;
                        Theme9 = true;
                        Theme10 = false;
                    }

                    if (GUI.Button(new Rect(368f, 727f, 75f, 25f), "T10"))
                    {
                        Theme4 = false;
                        Theme6 = false;
                        Theme3 = false;
                        Theme2 = false;
                        Theme1 = false;
                        Theme5 = false;
                        Theme7 = false;
                        Theme8 = false;
                        Theme9 = false;
                        Theme10 = true;
                    }

                    GUI.contentColor = Color.white;
                    if (GUI.Button(new Rect(180f, 760f, 150f, 28f), "Save all settings"))
                    {
                        IniFile iniFile4 = new IniFile("Settings.ini");
                        iniFile4.Write("JumpHeight", JumpHeight.ToString());
                        iniFile4.Write("CycleSaveBoundKey", cycleSaveBoundKey.ToString());
                        iniFile4.Write("ModMenuBoundKey", modMenuBoundKey.ToString());
                        iniFile4.Write("HideMeterBoundKey", hideMeterBoundKey.ToString());
                        iniFile4.Write("CalculateRealTime", calculateRealtime.ToString());
                        iniFile4.Write("CycleLoadBoundKey", cycleLoadBoundKey.ToString());
                        iniFile4.Write("ShortcutsBoundKey", shortcutsBoundKey.ToString());
                        iniFile4.Write("CalculateLastRunRank", calculateLastRunRank.ToString());
                        iniFile4.Write("ShowVerticalVelocity", showVerticalVelocity.ToString());
                        iniFile4.Write("AlwaysShowPeakspeed", alwaysShowPeakSpeed.ToString());
                        iniFile4.Write("ResetPeakOnGround", resetPeakOnGround.ToString());
                        iniFile4.Write("enableDoublePeakList", enableDoublePeakList.ToString());
                        iniFile4.Write("PeakList", PeakList.ToString());
                        iniFile4.Write("PeakListFloat", PeakListFloat.ToString());
                        iniFile4.Write("EnableVelometerGreenLimit", enableVelometerGreenLimit.ToString());
                        iniFile4.Write("VelometerGreenThreshold", velometerGreenThreshold.ToString());
                        iniFile4.Write("VelometerGreenThreshold2", velometerGreenThreshold2.ToString());
                        iniFile4.Write("VelometerGreenThresholdFloat", velometerGreenThresholdFloat.ToString());
                        iniFile4.Write("VelometerGreenThresholdFloat2", velometerGreenThresholdFloat2.ToString());
                        iniFile4.Write("ShowAngleX", showAngleX.ToString());
                        iniFile4.Write("ShowAngleY", showAngleY.ToString());
                        iniFile4.Write("AnglePrecision", anglePrecision.ToString());
                        iniFile4.Write("MouseSensitivity", mouseSens.ToString());
                        iniFile4.Write("ShowStatSpeedDips", showStatSpeedDips.ToString());
                        iniFile4.Write("ShowStatWallTouches", showStatWallTouches.ToString());
                        iniFile4.Write("ShowStatSpeedDipLowest", showStatSpeedDipLowest.ToString());
                        iniFile4.Write("ShowStatHighestPeak", showstatHighestPeak.ToString());
                        iniFile4.Write("ShowStatDistance", showStatDistance.ToString());
                        iniFile4.Write("ShowStatAverageSpeed", showStatAverageSpeed.ToString());
                        iniFile4.Write("orginalWidth", orginalWidth.ToString());
                        iniFile4.Write("orginalHeight", orginalHeight.ToString());
                        iniFile4.Write("infox", infox.ToString());
                        iniFile4.Write("infoy", infoy.ToString());
                        iniFile4.Write("Theme1", Theme1.ToString());
                        iniFile4.Write("Theme2", Theme2.ToString());
                        iniFile4.Write("Theme3", Theme3.ToString());
                        iniFile4.Write("Theme4", Theme4.ToString());
                        iniFile4.Write("Theme5", Theme5.ToString());
                        iniFile4.Write("Theme6", Theme6.ToString());
                        iniFile4.Write("Theme7", Theme7.ToString());
                        iniFile4.Write("Theme8", Theme8.ToString());
                        iniFile4.Write("Theme9", Theme9.ToString());
                        iniFile4.Write("Theme10", Theme10.ToString());
                        iniFile4.Write("CrossColor2", CrossColor2.ToString());
                        iniFile4.Write("CrossColor3", CrossColor3.ToString());
                        iniFile4.Write("CrossColor4", CrossColor4.ToString());
                        iniFile4.Write("CrossColor5", CrossColor5.ToString());
                        iniFile4.Write("CrossColor6", CrossColor6.ToString());
                        iniFile4.Write("CrossColor1", CrossColor1.ToString());
                        iniFile4.Write("CrossColor7", CrossColor7.ToString());
                        iniFile4.Write("CrossColor8", CrossColor8.ToString());
                        iniFile4.Write("CrossColor9", CrossColor9.ToString());
                        iniFile4.Write("TrailLerp1", TrailLerp1.ToString());
                        iniFile4.Write("TrailLerp2", TrailLerp2.ToString());
                        iniFile4.Write("TrailLerp3", TrailLerp3.ToString());
                        iniFile4.Write("TrailLerp4", TrailLerp4.ToString());
                        iniFile4.Write("TrailLerp5", TrailLerp5.ToString());
                        iniFile4.Write("TrailLerp6", TrailLerp6.ToString());
                        iniFile4.Write("TrailGreen", TrailGreen.ToString());
                        iniFile4.Write("TrailYellow", TrailYellow.ToString());
                        iniFile4.Write("TrailRed", TrailRed.ToString());
                        iniFile4.Write("TrailMagenta", TrailMagenta.ToString());
                        iniFile4.Write("TrailBlue", TrailBlue.ToString());
                        iniFile4.Write("TrailCyan", TrailCyan.ToString());
                        iniFile4.Write("TrailBlack", TrailBlack.ToString());
                        iniFile4.Write("TrailWhite", TrailWhite.ToString());
                        iniFile4.Write("GhostRed", GhostRed.ToString());
                        iniFile4.Write("GhostBlue", GhostBlue.ToString());
                        iniFile4.Write("GhostGreen", GhostGreen.ToString());
                        iniFile4.Write("GhostYellow", GhostYellow.ToString());
                        iniFile4.Write("GhostPurple", GhostPurple.ToString());
                        iniFile4.Write("GOpcaity1", GOpcaity1.ToString());
                        iniFile4.Write("GOpcaity2", GOpcaity2.ToString());
                        iniFile4.Write("GOpcaity4", GOpcaity4.ToString());
                        iniFile4.Write("GOpcaity3", GOpcaity3.ToString());
                        iniFile4.Write("BlockRestart", PluginState.RestartBlockEnabled.ToString());
                        iniFile4.Write("NoFireballs", NoFireballs.ToString());
                        iniFile4.Write("NoBlockBreak", NoBlockBreak.ToString());
                    }
                }
                else
                {
                    Sub4String = "<color=white>Settings</color>";
                }

                GUI.matrix = matrix2;
            }

            if (!inReplay)
            {
                return;
            }

            Matrix4x4 matrix3 = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
            Crosshair.draw();
            GUI.matrix = matrix3;
        }

        float num6 = xRes / 2 - 125;
        float num7 = yRes / 2 + yRes / 8 + 120;
        GUI.Label(new Rect(num6, num7, 250f, 50f), "Replay ticks: " + replayFrameCount);
        GUI.Label(new Rect(num6, num7 + 20f, 250f, 50f), "hSpd: " + Math.Round(PluginState.CalculatedHSpd, 3));
        GUI.Label(new Rect(num6, num7 + 40f, 250f, 50f), "vSpd: " + Math.Round(PluginState.CalculatedVSpd, 3));
        GUI.Label(new Rect(num6, num7 + 65f, 250f, 50f), "X Ang: " + Math.Round(playerAngleX, anglePrecision));
        GUI.Label(new Rect(num6, num7 + 85f, 250f, 50f), "Y Ang: " + Math.Round(playerAngleY, anglePrecision));
        if (Replay.replay != null)
        {
            string text5 = Mathf.Max(0f, Replay.replay.waitTime - Time.fixedDeltaTime).ToString("F7", CultureInfo.InvariantCulture);
            int num8 = text5.IndexOf('.');
            string text6 = text5.Substring(0, num8 + 4);
            string text7 = text5.Substring(num8 + 4);
            string text8 = "Cycle: <color=lime>" + text6 + "</color><color=#888888>" + text7 + "</color>s";
            GUI.Label(new Rect(num6, num7 + 105f, 250f, 50f), text8);
        }

        float x2 = num6 + 260f;
        float num9 = num7 + 20f;
        GUI.contentColor = Color.white;
        GUI.Label(new Rect(x2, num9 - 20f, 200f, 30f), "Last Replay Peaks");
        GUI.contentColor = Color.white;
        if (PluginState.SpeedPeaks != null && PluginState.SpeedPeaks.Count > 0)
        {
            int num10 = Mathf.Min(PluginState.SpeedPeaks.Count, 15);
            for (int i = 0; i < num10; i++)
            {
                GUI.contentColor = Color.white;
                GUI.Label(new Rect(x2, num9 + (float)(i * 20), 200f, 30f), i + 1 + ". Peak: " + PluginState.SpeedPeaks[i].ToString("F3"));
            }
        }
        else
        {
            GUI.contentColor = Color.gray;
            GUI.Label(new Rect(x2, num9, 200f, 30f), "No peaks found");
        }

        GUI.contentColor = Color.white;
    }

    private void modInfo(int wId)
    {
        GUI.Box(new Rect(0f, 0f, 2000f, 1200f), "");
        Matrix4x4 matrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
        int fontSize = GUI.skin.label.fontSize;
        GUI.color = new Color(1f, 1f, 1f, 1f);
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        GUI.skin.label.fontSize = 20;
        GUILayout.Space(85f);
        GUILayout.Label("<color=yellow>SEASON SELECTOR:</color>");
        GUI.skin.label.fontSize = fontSize;
        GUILayout.Label("You are able to change your season");
        GUILayout.Label("You can't upload a run in the older seasons");
        GUILayout.Label("<color=lime>+Season 8</color>");
        GUILayout.Space(30f);
        GUI.skin.label.fontSize = 20;
        GUILayout.Label("<color=yellow>LEVEL EDITOR:</color>");
        GUI.skin.label.fontSize = fontSize;
        GUILayout.Label("New items (<color=lime>+12</color>)");
        GUILayout.Label("New themes(<color=lime>+11</color>)");
        GUILayout.Label("New binds(<color=lime>+6</color>)");
        GUILayout.Label("<color=lime>+Level Editor Enhanced menu</color>");
        GUILayout.Label("<color=red>-Multiple hands (To avoid one bug)</color>");
        GUILayout.Label("<color=lime>+New binds (Moving items by 0.1f)</color>");
        GUILayout.Label("Use 1920x1080 res for less bugs with the LEE");
        GUILayout.Label("<color=lime>+Location editor (Camera + grid)</color>");
        GUILayout.Label("<color=lime>+Expand the limit of the grid (Scrollwheel)</color>");
        GUILayout.Label("<color=lime>+Entity counter</color>");
        GUILayout.Label("<color=lime>+Selected entity counter</color>");
        GUILayout.Space(30f);
        GUI.skin.label.fontSize = 20;
        GUILayout.Label("<color=yellow>ORIGINAL HOTKEYS</color>");
        GUI.skin.label.fontSize = fontSize;
        GUILayout.Label("'Y' - Opens the mod configuration menu");
        GUILayout.Label("'H' - Hides the mod");
        GUILayout.Label("'C' - During pre run aim saves the current cycle");
        GUILayout.Label("'Q' - During pre run aim loads the saved cycle");
        GUILayout.Label("'F5' - Refresh level");
        GUILayout.Label("<color=lime>+'F8' - Wireframe ON</color> (Use it with no theme)");
        GUILayout.Label("<color=lime>+'F9' - Wireframe OFF</color>");
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        GUI.skin.label.fontSize = 20;
        GUILayout.Label("<color=yellow>Mod information</color>");
        GUI.skin.label.fontSize = fontSize;
        GUILayout.Label("The main goal of this mod is to deliver more information, optimize gameplay and make game interaction more convenient for the end user ");
        GUILayout.Space(30f);
        GUI.skin.label.fontSize = 20;
        GUILayout.Label("<color=yellow>SEUM VELOCITYMETER 4 FEATURES:</color>");
        GUI.skin.label.fontSize = fontSize;
        GUILayout.Label("• MAIN OPTIONS:");
        GUILayout.Label("Meter options");
        GUILayout.Label("Angles options");
        GUILayout.Label("Last run stats");
        GUILayout.Label("<color=lime>+Fps boost options (No particles/ No effects</color><color=red> (-S6)</color><color=lime>/ No Theme (Keep death barriers)</color>");
        GUILayout.Label("<color=lime>+'Scrollable' leaderboard by range editor</color>");
        GUILayout.Label("<color=lime>+Ghost replay (By MrGentle)</color>");
        GUILayout.Label("<color=lime>+Last run peaks list</color>");
        GUILayout.Label("<color=lime>+Fixed threshold</color>");
        GUILayout.Label("<color=lime>+resized menu and move text</color>");
        GUILayout.Space(10f);
        GUILayout.Label("• MAPS MENU:");
        GUILayout.Label("Easier way to go on the secret levels");
        GUILayout.Label("<color=lime>+Patched S9-S11 shortcuts for non dlc players</color>");
        GUILayout.Label("Replay bind 'U' (<color=red>-Workshop</color><color=lime> +Unreleased maps</color>)");
        GUILayout.Space(10f);
        GUILayout.Label("• TIPS MENU:");
        GUILayout.Label("Cycle list (<color=lime>+s7 cycles</color>)");
        GUILayout.Label("<color=lime>+Hellikus secrets tips</color>");
        GUILayout.Space(10f);
        GUILayout.Label("• SETTINGS MENU:");
        GUILayout.Label("Resolution editor (with custom option)");
        GUILayout.Label("Binds options");
        GUILayout.Label("<color=lime>+Bind scrollwheel to start the cycles</color>");
        GUILayout.Label("Colors editor");
        GUILayout.Label("<color=lime>+Theme editor</color>");
        GUILayout.Space(30f);
        GUI.skin.label.fontSize = 20;
        GUILayout.Label("<color=yellow>OTHER:</color>");
        GUI.skin.label.fontSize = fontSize;
        GUILayout.Label("<color=lime>+Smoother replays (Thanks to MrGentle)</color>");
        GUILayout.Label("<color=lime>+Seum tool button</color>");
        GUILayout.Space(150f);
        GUILayout.Label("PRESS F1 TO CLOSE");
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        GUILayout.Space(85f);
        GUI.skin.label.fontSize = 20;
        GUILayout.Label("<color=yellow>CREDITS:</color>");
        GUI.skin.label.fontSize = fontSize;
        GUILayout.Label("MrGentle (ZoomZoom tm) - Mod creator");
        GUILayout.Label("Judgy - Modloader creator (not in use anymore)");
        GUILayout.Label("chrisstar123 - General goto bro for coding help");
        GUILayout.Label("Sirius - Help with XML parsing");
        GUILayout.Label("Pine Studios - For a great game!");
        GUILayout.Label("The SEUM discord family - I love you all");
        GUILayout.Label("Link ( Z_Link) - VelocityMeter 3 and 4 | Seasons selector | LEE");
        GUILayout.Label("Snail - Replays fix");
        GUILayout.Label("ZM - Some level editor stuff");
        GUILayout.Label("Royal - Beta tester");
        GUILayout.Label("Discord community - Ideas + Cycles");
        GUILayout.Space(30f);
        GUI.skin.label.fontSize = fontSize;
        GUILayout.Label("Any ideas or issues ? pm or @ Link on discord");
        GUILayout.Label("Need help ? Ask on the discord");
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUI.matrix = matrix;
        GUI.skin.label.fontSize = 16;
    }

    private void lastRunInfo(int wId)
    {
        if (!Game.isEndless())
        {
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUILayout.BeginVertical();
            if (showStatSpeedDips)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Speed dips: " + speedDipCounter);
                GUILayout.EndHorizontal();
            }

            if (showStatWallTouches)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Wall Touches: " + wallTouches);
                GUILayout.EndHorizontal();
            }

            if (showStatSpeedDipLowest)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Lowest Dip: " + Math.Round(lowestSpeedDip, 3));
                GUILayout.EndHorizontal();
            }

            if (showstatHighestPeak)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Highest Peak: " + Math.Round(highestPeakSpeed, 3));
                GUILayout.EndHorizontal();
            }

            if (showStatDistance)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Distance Traveled: " + Math.Round(distanceTraveled, 1) * 0.1 + "m over " + ticksThisRun + " ticks");
                GUILayout.EndHorizontal();
            }

            if (showStatAverageSpeed)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Average Speed: " + Math.Round(averageSpeed, 3));
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        else
        {
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Total distance ran: " + totalMetersRan + "m");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Total distance this session: " + thisSessionTotalMetersRan + "m");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Real time: " + Math.Round(realTime, 3));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }

    private void workshopLevelInfo(int wId)
    {
        LevelData currentLevelData = Game.getCurrentLevelData();
        int timesStarted = currentLevelData.timesStarted;
        int timesReset = currentLevelData.timesReset;
        int timesDied = currentLevelData.timesDied;
        int timesFinished = currentLevelData.timesFinished;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Tries: " + timesStarted);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Resets: " + timesReset);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Deaths: " + timesDied);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Finishes: " + timesFinished);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private KeyCode bindKey(KeyCode currentKey)
    {
        KeyCode result = currentKey;
        foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKey(value))
            {
                result = value;
                keybound = true;
            }
        }

        return result;
    }

    private KeyCode getKeyFromIni(KeyCode currentKey, string stringKeyName)
    {
        foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
        {
            if (value.ToString() == stringKeyName)
            {
                return value;
            }
        }

        return currentKey;
    }

    public ModLoader()
    {
        StringRange2 = "1";
        menutest1 = true;
        Decals = true;
        doublePeakList = new List<double>();
        setCycle = "0,016";
        num6 = Screen.currentResolution.width;
        orginalWidth = 1920f;
        orginalHeight = 1080f;
        orginalWidth2 = 1920f;
        orginalHeight2 = 1080f;
        num90 = (float)Screen.width / 21f;
        num190 = (float)Screen.width / 10.5f;
        uiBaseScreenHeight = 500f;
        scrollPosition = Vector2.zero;
        Closed = "<color=red>Close</color>";
        onlineRevUrl = "";
        currentRev = 1.1f;
        lowestSpeedDip = 12.18f;
        distanceList = new List<float>();
        menuLeftRect = 75;
        menuTopRect = 105;
        menuWidthRect = 300;
        menuWidthRectGame = 300;
        scrollPos = new Vector2(556f, 516f);
        scrollViewVector = Vector2.zero;
        scrollRect = new Rect(10f, 10f, 460f, 300f);
        scrollViewRect = new Rect(0f, 0f, 800f, 600f);
        menuHeightRect = 300;
        lastRunStatsLeftRect = 75;
        lastRunStatsTopRect = 450;
        lastRunStatsWidthRect = 300;
        speeds = new List<float>();
        replayDistanceList = new List<double>();
        lbUrlExtRandom = new UnityEngine.Random();
        anglePrecision = 1;
        stringToEditRez1 = "2560";
        stringToEditRez2 = "1440";
        Converter = "";
        velometerGreenThreshold2 = "10";
        velometerGreenThreshold = "12";
        velometerGreenThresholdFloat = 12f;
        velometerGreenThresholdFloat2 = 10f;
        PeakList = "16";
        PeakListFloat = 16f;
        mouseSens = GameSettings.settings.mouseSensitivity;
        addToStats = true;
        readStats = true;
        workshopLevelInfoWidthRect = 150;
        workshopLevelInfoLeftRect = checked(Screen.currentResolution.width - 400 - 75);
        modMenuBoundKey = KeyCode.Y;
        replayButtonBoundKey = KeyCode.U;
        shortcutsBoundKey = KeyCode.I;
        cycleSaveBoundKey = KeyCode.C;
        cycleLoadBoundKey = KeyCode.Q;
        hideMeterBoundKey = KeyCode.H;
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        checked
        {
            Color[] array = new Color[width * height];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = col;
            }

            Texture2D texture2D = new Texture2D(width, height);
            texture2D.SetPixels(array);
            texture2D.Apply();
            return texture2D;
        }
    }

    private void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
    {
        GameObject obj = new GameObject();
        obj.transform.position = start;
        obj.AddComponent<LineRenderer>();
        LineRenderer component = obj.GetComponent<LineRenderer>();
        component.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        component.SetColors(color, color);
        component.SetWidth(0.1f, 0.1f);
        component.SetPosition(0, start);
        component.SetPosition(1, end);
        UnityEngine.Object.Destroy(obj, duration);
    }

    private int GetScaledFontSize(int baseFontSize)
    {
        float num = (float)Screen.height / uiBaseScreenHeight;
        return Mathf.RoundToInt((float)baseFontSize * num);
    }

    static ModLoader()
    {
        checked
        {
            windowRect = new Rect((float)Screen.width - (float)Screen.width / 4.7f, 0f, (float)Screen.width / 4.7f, unchecked(Screen.height / 4) * 3 - 2);
            Image = null;
            Image2 = null;
            showHitboxes = false;
        }
    }

    private void LastPeak(int wId)
    {
        if (Game.isEndless())
        {
            return;
        }

        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
        if (!enableDoublePeakList)
        {
            return;
        }

        GUILayout.BeginHorizontal();
        foreach (double doublePeak in doublePeakList)
        {
            GUILayout.Label(string.Concat(Math.Round(doublePeak, 3)));
        }

        GUILayout.EndHorizontal();
    }

    private void LateUpdate()
    {
        if (!showHitboxes)
        {
            GameObject gameObject;
            while ((gameObject = GameObject.Find("HOLO_EDGE")) != null)
            {
                gameObject.SetActive(value: false);
            }
        }
    }
}
#if false // Журнал декомпиляции
Элементов в кэше: "81"
------------------
Разрешить: "UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Найдена одна сборка: "UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Загрузить из: "C:\Users\ampil\.nuget\packages\unityengine.modules\2018.3.7\lib\net45\UnityEngine.CoreModule.dll"
------------------
Разрешить: "netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51"
Найдена одна сборка: "netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51"
Загрузить из: "C:\Users\ampil\.nuget\packages\microsoft.netframework.referenceassemblies.net472\1.0.2\build\.NETFramework\v4.7.2\Facades\netstandard.dll"
------------------
Разрешить: "UnityEngine.IMGUIModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Найдена одна сборка: "UnityEngine.IMGUIModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Загрузить из: "C:\Users\ampil\.nuget\packages\unityengine.modules\2018.3.7\lib\net45\UnityEngine.IMGUIModule.dll"
------------------
Разрешить: "UnityEngine.VideoModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Найдена одна сборка: "UnityEngine.VideoModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Загрузить из: "C:\Users\ampil\.nuget\packages\unityengine.modules\2018.3.7\lib\net45\UnityEngine.VideoModule.dll"
------------------
Разрешить: "UnityEngine.TextRenderingModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Найдена одна сборка: "UnityEngine.TextRenderingModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Загрузить из: "C:\Users\ampil\.nuget\packages\unityengine.modules\2018.3.7\lib\net45\UnityEngine.TextRenderingModule.dll"
------------------
Разрешить: "UnityEngine.AudioModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Найдена одна сборка: "UnityEngine.AudioModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Загрузить из: "C:\Users\ampil\.nuget\packages\unityengine.modules\2018.3.7\lib\net45\UnityEngine.AudioModule.dll"
------------------
Разрешить: "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Найдена одна сборка: "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Загрузить из: "C:\Users\ampil\.nuget\packages\microsoft.netframework.referenceassemblies.net472\1.0.2\build\.NETFramework\v4.7.2\mscorlib.dll"
------------------
Разрешить: "UnityEngine.PhysicsModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Найдена одна сборка: "UnityEngine.PhysicsModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Загрузить из: "C:\Users\ampil\.nuget\packages\unityengine.modules\2018.3.7\lib\net45\UnityEngine.PhysicsModule.dll"
------------------
Разрешить: "Rewired_Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
Не удалось найти по имени: "Rewired_Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
------------------
Разрешить: "UnityEngine.AnimationModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Найдена одна сборка: "UnityEngine.AnimationModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Загрузить из: "C:\Users\ampil\.nuget\packages\unityengine.modules\2018.3.7\lib\net45\UnityEngine.AnimationModule.dll"
------------------
Разрешить: "UnityEngine.ParticleSystemModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Найдена одна сборка: "UnityEngine.ParticleSystemModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Загрузить из: "C:\Users\ampil\.nuget\packages\unityengine.modules\2018.3.7\lib\net45\UnityEngine.ParticleSystemModule.dll"
------------------
Разрешить: "Assembly-CSharp-firstpass, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Найдена одна сборка: "Assembly-CSharp-firstpass, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Загрузить из: "D:\SteamLibrary\steamapps\common\SEUM Speedrunners from Hell\Seum_Data\Managed\Assembly-CSharp-firstpass.dll"
------------------
Разрешить: "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"
Найдена одна сборка: "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Загрузить из: "C:\Users\ampil\.nuget\packages\microsoft.netframework.referenceassemblies.net472\1.0.2\build\.NETFramework\v4.7.2\System.Core.dll"
------------------
Разрешить: "UnityEngine.AssetBundleModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Найдена одна сборка: "UnityEngine.AssetBundleModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Загрузить из: "C:\Users\ampil\.nuget\packages\unityengine.modules\2018.3.7\lib\net45\UnityEngine.AssetBundleModule.dll"
------------------
Разрешить: "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"
Найдена одна сборка: "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Загрузить из: "C:\Users\ampil\.nuget\packages\microsoft.netframework.referenceassemblies.net472\1.0.2\build\.NETFramework\v4.7.2\System.dll"
------------------
Разрешить: "UnityEngine.UnityWebRequestWWWModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Найдена одна сборка: "UnityEngine.UnityWebRequestWWWModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Загрузить из: "C:\Users\ampil\.nuget\packages\unityengine.modules\2018.3.7\lib\net45\UnityEngine.UnityWebRequestWWWModule.dll"
------------------
Разрешить: "UnityEngine.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
Не удалось найти по имени: "UnityEngine.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
------------------
Разрешить: "UnityEngine.UIModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Найдена одна сборка: "UnityEngine.UIModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Загрузить из: "C:\Users\ampil\.nuget\packages\unityengine.modules\2018.3.7\lib\net45\UnityEngine.UIModule.dll"
------------------
Разрешить: "System.Xml.Linq, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"
Найдена одна сборка: "System.Xml.Linq, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Загрузить из: "C:\Users\ampil\.nuget\packages\microsoft.netframework.referenceassemblies.net472\1.0.2\build\.NETFramework\v4.7.2\System.Xml.Linq.dll"
------------------
Разрешить: "UnityEngine.JSONSerializeModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Найдена одна сборка: "UnityEngine.JSONSerializeModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Загрузить из: "C:\Users\ampil\.nuget\packages\unityengine.modules\2018.3.7\lib\net45\UnityEngine.JSONSerializeModule.dll"
------------------
Разрешить: "UnityEngine.ImageConversionModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Найдена одна сборка: "UnityEngine.ImageConversionModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Загрузить из: "C:\Users\ampil\.nuget\packages\unityengine.modules\2018.3.7\lib\net45\UnityEngine.ImageConversionModule.dll"
------------------
Разрешить: "UnityEngine.VRModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Найдена одна сборка: "UnityEngine.VRModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Загрузить из: "C:\Users\ampil\.nuget\packages\unityengine.modules\2018.3.7\lib\net45\UnityEngine.VRModule.dll"
------------------
Разрешить: "UnityEngine.UnityWebRequestModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Найдена одна сборка: "UnityEngine.UnityWebRequestModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Загрузить из: "C:\Users\ampil\.nuget\packages\unityengine.modules\2018.3.7\lib\net45\UnityEngine.UnityWebRequestModule.dll"
------------------
Разрешить: "UnityEngine.ScreenCaptureModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Найдена одна сборка: "UnityEngine.ScreenCaptureModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
Загрузить из: "C:\Users\ampil\.nuget\packages\unityengine.modules\2018.3.7\lib\net45\UnityEngine.ScreenCaptureModule.dll"
------------------
Разрешить: "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Найдена одна сборка: "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Загрузить из: "C:\Users\ampil\.nuget\packages\microsoft.netframework.referenceassemblies.net472\1.0.2\build\.NETFramework\v4.7.2\System.Core.dll"
------------------
Разрешить: "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Найдена одна сборка: "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Загрузить из: "C:\Users\ampil\.nuget\packages\microsoft.netframework.referenceassemblies.net472\1.0.2\build\.NETFramework\v4.7.2\System.dll"
------------------
Разрешить: "System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Найдена одна сборка: "System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Загрузить из: "C:\Users\ampil\.nuget\packages\microsoft.netframework.referenceassemblies.net472\1.0.2\build\.NETFramework\v4.7.2\System.Data.dll"
------------------
Разрешить: "System.Diagnostics.Tracing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
Не удалось найти по имени: "System.Diagnostics.Tracing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
------------------
Разрешить: "System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
Найдена одна сборка: "System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
Загрузить из: "C:\Users\ampil\.nuget\packages\microsoft.netframework.referenceassemblies.net472\1.0.2\build\.NETFramework\v4.7.2\System.Drawing.dll"
------------------
Разрешить: "System.IO.Compression, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Не удалось найти по имени: "System.IO.Compression, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
------------------
Разрешить: "System.IO.Compression.FileSystem, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Найдена одна сборка: "System.IO.Compression.FileSystem, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Загрузить из: "C:\Users\ampil\.nuget\packages\microsoft.netframework.referenceassemblies.net472\1.0.2\build\.NETFramework\v4.7.2\System.IO.Compression.FileSystem.dll"
------------------
Разрешить: "System.ComponentModel.Composition, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Не удалось найти по имени: "System.ComponentModel.Composition, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
------------------
Разрешить: "System.Net.Http, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
Не удалось найти по имени: "System.Net.Http, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
------------------
Разрешить: "System.Numerics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Найдена одна сборка: "System.Numerics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Загрузить из: "C:\Users\ampil\.nuget\packages\microsoft.netframework.referenceassemblies.net472\1.0.2\build\.NETFramework\v4.7.2\System.Numerics.dll"
------------------
Разрешить: "System.Runtime.Serialization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Найдена одна сборка: "System.Runtime.Serialization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Загрузить из: "C:\Users\ampil\.nuget\packages\microsoft.netframework.referenceassemblies.net472\1.0.2\build\.NETFramework\v4.7.2\System.Runtime.Serialization.dll"
------------------
Разрешить: "System.Transactions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Не удалось найти по имени: "System.Transactions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
------------------
Разрешить: "System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
Не удалось найти по имени: "System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
------------------
Разрешить: "System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Найдена одна сборка: "System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Загрузить из: "C:\Users\ampil\.nuget\packages\microsoft.netframework.referenceassemblies.net472\1.0.2\build\.NETFramework\v4.7.2\System.Xml.dll"
------------------
Разрешить: "System.Xml.Linq, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Найдена одна сборка: "System.Xml.Linq, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
Загрузить из: "C:\Users\ampil\.nuget\packages\microsoft.netframework.referenceassemblies.net472\1.0.2\build\.NETFramework\v4.7.2\System.Xml.Linq.dll"
------------------
Разрешить: "System.Runtime.InteropServices, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null"
Не удалось найти по имени: "System.Runtime.InteropServices, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null"
------------------
Разрешить: "System.Runtime.CompilerServices.Unsafe, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null"
Не удалось найти по имени: "System.Runtime.CompilerServices.Unsafe, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null"
#endif
