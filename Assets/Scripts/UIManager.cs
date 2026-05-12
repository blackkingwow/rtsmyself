using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    [Header("引用")]
    public GridMap gridMap;
    public GameObject missileVehiclePrefab;
    public GameObject radarStationPrefab;
    public GameObject antiAirPrefab;
    public GameObject antiAirModelPrefab;
    public GameObject enemyPlanePrefab;
    public GameObject missileModelPrefab;

    private enum UIState { Normal, Deploying, Attacking }
    private UIState currentState = UIState.Normal;
    private string deployType = "";

    private SelectableUnit currentSelected;
    private UnitBase currentSelectedUnit;

    private Text goldText;
    private Text messageText;
    private float messageTimer = 0f;

    private Button deployMissileBtn;
    private Button deployRadarBtn;
    private Text deployMissileCD;
    private Text deployRadarCD;

    private Button hackBtn;
    private Text hackCDText;
    private Text empCDText;

    private GameObject bottomBar;
    private Button actionBtn1;
    private Button actionBtn2;
    private Text actionBtn1Text;
    private Text actionBtn2Text;
    private Text actionBtn1CD;
    private Text actionBtn2CD;

    private GameObject gameOverPanel;
    private GameObject winPanel;
    private Text waveText;
    private Text countdownText;

    private Font uiFont;

    private float waveSpawnTimer;
    private int waveSpawnCount;

    // 缓存查找结果，避免每帧Find
    private Transform unitsParent;
    private Transform enemiesParent;

    void Start()
    {
        // 安全加载字体，Windows上用Arial替代Helvetica
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiFont == null)
        {
            uiFont = Font.CreateDynamicFontFromOSFont("Arial", 14);
            if (uiFont == null) uiFont = Font.CreateDynamicFontFromOSFont("Helvetica", 14);
            if (uiFont == null) uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        // 预查找父物体，性能优化
        unitsParent = GameObject.Find("Units")?.transform;
        enemiesParent = GameObject.Find("Enemies")?.transform;

        Missile.modelPrefab = missileModelPrefab;

        CreateAllUI();
        HookEvents();
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.isGameOver) return;

        HandleInput();
        UpdateUI();
        UpdateWaveSpawning();

        if (messageTimer > 0)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0 && messageText != null)
                messageText.text = "";
        }
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                switch (currentState)
                {
                    case UIState.Normal:
                        HandleNormalClick(hit);
                        break;
                    case UIState.Deploying:
                        HandleDeployClick(hit.point);
                        break;
                    case UIState.Attacking:
                        HandleAttackClick(hit);
                        break;
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            CancelAllModes();
        }
    }

    void HandleNormalClick(RaycastHit hit)
    {
        SelectableUnit sel = hit.collider.GetComponentInParent<SelectableUnit>();
        if (sel != null)
        {
            SelectUnit(sel);
            return;
        }

        if (currentSelectedUnit != null && currentSelectedUnit is MissileVehicle)
        {
            MissileVehicle mv = currentSelectedUnit as MissileVehicle;
            if (!mv.isDead)
                mv.MoveTo(hit.point);
            return;
        }
        
        DeselectCurrent();
    }

    void HandleDeployClick(Vector3 point)
    {
        if (!gridMap.IsPlayerArea(point))
        {
            ShowMessage("必须在玩家部署区域（下半部分）部署！");
            return;
        }

        if (deployType == "missile")
        {
            if (!GameManager.Instance.SpendGold(200))
            {
                ShowMessage("金钱不足！部署导弹车需要200金钱");
                return;
            }
            if (missileVehiclePrefab != null)
            {
                GameObject go = Instantiate(missileVehiclePrefab, new Vector3(point.x, 0, point.z), Quaternion.identity);
                if (unitsParent != null) go.transform.SetParent(unitsParent);
                go.tag = "PlayerUnit";
                ShowMessage("导弹车已部署");
            }
        }
        else if (deployType == "radar")
        {
            if (!GameManager.Instance.SpendGold(100))
            {
                ShowMessage("金钱不足！部署雷达站需要100金钱");
                return;
            }
            if (radarStationPrefab != null)
            {
                GameObject go = Instantiate(radarStationPrefab, new Vector3(point.x, 0, point.z), Quaternion.identity);
                if (unitsParent != null) go.transform.SetParent(unitsParent);
                go.tag = "Building";
                ShowMessage("雷达站已部署");
            }
        }
        else if (deployType == "antiair")
        {
            if (!GameManager.Instance.SpendGold(100))
            {
                ShowMessage("金钱不足！部署防空导弹发射器需要100金钱");
                return;
            }
            GameObject go;
            if (antiAirPrefab != null)
            {
                go = Instantiate(antiAirPrefab, new Vector3(point.x, 0, point.z), Quaternion.identity);
            }
            else
            {
                // 无预制体时自动创建
                go = new GameObject("防空导弹发射器");
                go.transform.position = new Vector3(point.x, 0, point.z);
                go.AddComponent<SelectableUnit>();
                go.AddComponent<AntiAirMissileLauncher>();
                if (antiAirModelPrefab != null)
                {
                    GameObject model = Instantiate(antiAirModelPrefab, go.transform);
                    model.transform.localPosition = Vector3.zero;
                }
            }
            if (unitsParent != null) go.transform.SetParent(unitsParent);
            go.tag = "Building";
            ShowMessage("防空导弹发射器已部署");
        }

        currentState = UIState.Normal;
        deployType = "";
    }

    void HandleAttackClick(RaycastHit hit)
    {
        if (currentSelectedUnit is MissileVehicle mv)
        {
            EnemyPlane enemy = hit.collider.GetComponentInParent<EnemyPlane>();
            if (enemy != null && !enemy.isDead)
            {
                if (mv.CanAttack())
                {
                    mv.Attack(enemy);
                    ShowMessage("导弹发射！");
                }
                else
                {
                    ShowMessage("导弹冷却中...");
                }
            }
        }
        else if (currentSelectedUnit is PlayerBase pb)
        {
            UnitBase target = hit.collider.GetComponentInParent<UnitBase>();
            
            // 修复2：逻辑错误，只能修复友方建筑
            if (target != null && !target.isDead)
            {
                if ((target is MissileVehicle || target is RadarStation || target is AntiAirMissileLauncher) && (target.CompareTag("PlayerUnit") || target.CompareTag("Building")))
                {
                    pb.Repair(target);
                    ShowMessage("已修复：" + target.unitName);
                }
            }
        }

        currentState = UIState.Normal;
    }

    void SelectUnit(SelectableUnit sel)
    {
        DeselectCurrent();
        currentSelected = sel;
        currentSelected.Select();
        currentSelectedUnit = sel.GetComponent<UnitBase>();
        UpdateBottomBar();
    }

    void DeselectCurrent()
    {
        if (currentSelected != null)
        {
            currentSelected.Deselect();
            currentSelected = null;
            currentSelectedUnit = null;
        }
        UpdateBottomBar();
    }

    void CancelAllModes()
    {
        currentState = UIState.Normal;
        deployType = "";
        DeselectCurrent();
        ShowMessage("已取消操作");
    }

    void UpdateBottomBar()
    {
        if (bottomBar == null) return;
        bottomBar.SetActive(currentSelectedUnit != null);

        if (currentSelectedUnit == null) return;

        if (currentSelectedUnit is MissileVehicle)
        {
            actionBtn1.gameObject.SetActive(true);
            actionBtn1Text.text = "发射导弹";
            actionBtn2.gameObject.SetActive(false);
        }
        else if (currentSelectedUnit is RadarStation)
        {
            actionBtn1.gameObject.SetActive(true);
            RadarStation rs = currentSelectedUnit as RadarStation;
            actionBtn1Text.text = rs.isActiveMode ? "切换静默" : "主动侦察";
            actionBtn2.gameObject.SetActive(false);
        }
        else if (currentSelectedUnit is AntiAirMissileLauncher)
        {
            actionBtn1.gameObject.SetActive(true);
            AntiAirMissileLauncher aa = currentSelectedUnit as AntiAirMissileLauncher;
            actionBtn1Text.text = aa.isActive ? "关机" : "开机";
            actionBtn2.gameObject.SetActive(false);
        }
        else if (currentSelectedUnit is PlayerBase)
        {
            actionBtn1.gameObject.SetActive(true);
            actionBtn2.gameObject.SetActive(true);
            actionBtn1Text.text = "修复";
            actionBtn2Text.text = "升级(" + GameManager.Instance.GetBaseUpgradeCost() + "金)";
        }
    }

    void UpdateUI()
    {
        if (goldText != null && GameManager.Instance != null)
            goldText.text = "金钱：" + GameManager.Instance.gold;

        if (waveText != null && GameManager.Instance != null)
            waveText.text = GameManager.Instance.currentWave + "/" + GameManager.Instance.maxWaves;

        if (countdownText != null && GameManager.Instance != null)
        {
            if (!GameManager.Instance.isWaveActive && GameManager.Instance.currentWave > 0)
                countdownText.text = "下一波：" + Mathf.CeilToInt(GameManager.Instance.GetWaveCountdown()) + "秒";
            else if (GameManager.Instance.currentWave == 0)
                countdownText.text = "准备中：" + Mathf.CeilToInt(GameManager.Instance.GetWaveCountdown()) + "秒";
            else
                countdownText.text = "第" + GameManager.Instance.currentWave + "波 剩余：" + GameManager.Instance.enemiesAliveInWave + "架";
        }

        if (hackCDText != null && GameManager.Instance != null)
        {
            if (GameManager.Instance.isHacked)
            {
                hackCDText.text = "全图视野: " + Mathf.CeilToInt(GameManager.Instance.GetHackDurationRemaining()) + "秒";
                hackBtn.interactable = false;
            }
            else if (!GameManager.Instance.CanUseHack())
            {
                hackCDText.text = "CD: " + Mathf.CeilToInt(GameManager.Instance.GetHackCooldownRemaining()) + "秒";
                hackBtn.interactable = false;
            }
            else
            {
                hackCDText.text = "就绪";
                hackBtn.interactable = true;
            }
        }

        if (empCDText != null && GameManager.Instance != null)
        {
            if (GameManager.Instance.isEmpActive)
            {
                empCDText.text = "减速中: " + Mathf.CeilToInt(GameManager.Instance.GetEmpDurationRemaining()) + "秒";
            }
            else if (!GameManager.Instance.CanUseEmp())
            {
                empCDText.text = "CD: " + Mathf.CeilToInt(GameManager.Instance.GetEmpCooldownRemaining()) + "秒";
            }
            else
            {
                empCDText.text = "就绪";
            }
        }

        UpdateActionButtonCDs();
    }

    void UpdateActionButtonCDs()
    {
        if (currentSelectedUnit is MissileVehicle mv)
        {
            float cd = mv.GetAttackCooldownRemaining();
            actionBtn1CD.text = cd > 0 ? Mathf.CeilToInt(cd) + "秒" : "";
            actionBtn1.interactable = mv.CanAttack();
        }
        else if (currentSelectedUnit is RadarStation rs)
        {
            actionBtn1CD.text = "";
            actionBtn1.interactable = rs.CanSwitchMode();
        }
        else if (currentSelectedUnit is AntiAirMissileLauncher)
        {
            actionBtn1CD.text = "";
            actionBtn1.interactable = true;
        }
        else if (currentSelectedUnit is PlayerBase pb)
        {
            float repairCD = pb.GetRepairCooldownRemaining();
            actionBtn1CD.text = repairCD > 0 ? Mathf.CeilToInt(repairCD) + "秒" : "";
            actionBtn1.interactable = pb.CanRepair() && GameManager.Instance.gold >= 50;
            
            actionBtn2CD.text = "";
            actionBtn2.interactable = pb.CanUpgrade();
        }
    }

    void UpdateWaveSpawning()
    {
        if (!GameManager.Instance.isWaveActive) return;
        if (waveSpawnCount >= GameManager.Instance.enemiesTotalInWave) return;

        waveSpawnTimer -= Time.deltaTime;
        if (waveSpawnTimer <= 0)
        {
            waveSpawnTimer = 0.3f;
            waveSpawnCount++;
            SpawnEnemyPlane();
            GameManager.Instance.OnEnemySpawned();
        }
    }

    void SpawnEnemyPlane()
    {
        if (enemyPlanePrefab == null) return;

        float spawnX = Random.Range(gridMap.MapMinX + 2f, gridMap.MapMaxX - 2f);
        float spawnZ = gridMap.MapMaxZ - 1f;
        Vector3 spawnPos = new Vector3(spawnX, 1f, spawnZ);

        GameObject enemy = Instantiate(enemyPlanePrefab, spawnPos, Quaternion.identity);
        if (enemiesParent != null) enemy.transform.SetParent(enemiesParent);
        enemy.tag = "Enemy";
    }

    #region UI创建

    void CreateAllUI()
    {
        CreateMainCanvas();
        CreateWaveUI();
        CreateFogOverlay();
        CreateGameOverPanel();
        CreateWinPanel();
    }

    void CreateMainCanvas()
    {
        if (GameObject.Find("GameCanvas") != null) return;

        GameObject canvasGo = new GameObject("GameCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();
        }

        CreateGoldDisplay(canvasGo.transform);
        CreateDeployPanel(canvasGo.transform);
        CreateSkillPanel(canvasGo.transform);
        CreateBottomBar(canvasGo.transform);
        CreateMessageText(canvasGo.transform);
    }

    void CreateGoldDisplay(Transform parent)
    {
        GameObject go = new GameObject("GoldText");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-30, -30);
        rt.sizeDelta = new Vector2(300, 50);
        goldText = go.AddComponent<Text>();
        goldText.text = "金钱：" + (GameManager.Instance != null ? GameManager.Instance.gold.ToString() : "500");
        goldText.fontSize = 28;
        goldText.font = uiFont;
        goldText.color = new Color(1f, 0.85f, 0f);
        goldText.alignment = TextAnchor.MiddleRight;
    }

    void CreateDeployPanel(Transform parent)
    {
        GameObject panel = new GameObject("DeployPanel");
        panel.transform.SetParent(parent, false);
        RectTransform prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(1, 0.5f);
        prt.anchorMax = new Vector2(1, 0.5f);
        prt.pivot = new Vector2(1, 0.5f);
        prt.anchoredPosition = new Vector2(-30, 0);
        prt.sizeDelta = new Vector2(200, 300);

        prt.sizeDelta = new Vector2(200, 420);

        CreateText("部署单位", panel.transform, new Vector2(0, 180), new Vector2(200, 40), 22, TextAnchor.MiddleCenter);

        deployMissileBtn = CreateButton("部署导弹车(200金)", panel.transform, new Vector2(0, 120), new Vector2(200, 50));
        deployMissileBtn.onClick.AddListener(() =>
        {
            if (GameManager.Instance.isGameOver) return;
            currentState = UIState.Deploying;
            deployType = "missile";
            DeselectCurrent();
            ShowMessage("点击下半部分地图放置导弹车");
        });
        deployMissileCD = CreateText("", deployMissileBtn.transform, new Vector2(0, -30), new Vector2(200, 20), 14, TextAnchor.MiddleCenter);

        deployRadarBtn = CreateButton("部署雷达站(100金)", panel.transform, new Vector2(0, 40), new Vector2(200, 50));
        deployRadarBtn.onClick.AddListener(() =>
        {
            if (GameManager.Instance.isGameOver) return;
            currentState = UIState.Deploying;
            deployType = "radar";
            DeselectCurrent();
            ShowMessage("点击下半部分地图放置雷达站");
        });
        deployRadarCD = CreateText("", deployRadarBtn.transform, new Vector2(0, -30), new Vector2(200, 20), 14, TextAnchor.MiddleCenter);

        Button deployAntiAirBtn = CreateButton("部署防空导弹(100金)", panel.transform, new Vector2(0, -50), new Vector2(200, 50));
        deployAntiAirBtn.onClick.AddListener(() =>
        {
            if (GameManager.Instance.isGameOver) return;
            currentState = UIState.Deploying;
            deployType = "antiair";
            DeselectCurrent();
            ShowMessage("点击下半部分地图放置防空导弹发射器");
        });
    }

    void CreateSkillPanel(Transform parent)
    {
        GameObject panel = new GameObject("SkillPanel");
        panel.transform.SetParent(parent, false);
        RectTransform prt = panel.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(0, 0.5f);
        prt.anchorMax = new Vector2(0, 0.5f);
        prt.pivot = new Vector2(0, 0.5f);
        prt.anchoredPosition = new Vector2(30, 0);
        prt.sizeDelta = new Vector2(200, 200);

        prt.sizeDelta = new Vector2(200, 320);

        CreateText("技能", panel.transform, new Vector2(0, 120), new Vector2(200, 40), 22, TextAnchor.MiddleCenter);

        hackBtn = CreateButton("骇入敌方网络", panel.transform, new Vector2(0, 60), new Vector2(200, 60));
        hackBtn.onClick.AddListener(() =>
        {
            if (GameManager.Instance == null || GameManager.Instance.isGameOver) return;
            if (GameManager.Instance.CanUseHack())
            {
                GameManager.Instance.ActivateHack();
                ShowMessage("骇入成功！全图视野");
            }
            else
            {
                ShowMessage("骇入冷却中...");
            }
        });

        hackCDText = CreateText("就绪", hackBtn.transform, new Vector2(0, -40), new Vector2(200, 20), 14, TextAnchor.MiddleCenter);

        Button empBtn = CreateButton("电磁波干扰(200金)", panel.transform, new Vector2(0, -30), new Vector2(200, 60));
        empBtn.onClick.AddListener(() =>
        {
            if (GameManager.Instance == null || GameManager.Instance.isGameOver) return;
            if (GameManager.Instance.CanUseEmp())
            {
                GameManager.Instance.ActivateEmp();
                ShowMessage("电磁波干扰启动！敌机减速20秒");
            }
            else if (GameManager.Instance.gold < 200)
            {
                ShowMessage("金钱不足！电磁波干扰需要200金钱");
            }
            else
            {
                ShowMessage("电磁波干扰冷却中...");
            }
        });

        empCDText = CreateText("就绪", empBtn.transform, new Vector2(0, -40), new Vector2(200, 20), 14, TextAnchor.MiddleCenter);
    }

    void CreateBottomBar(Transform parent)
    {
        bottomBar = new GameObject("BottomBar");
        bottomBar.transform.SetParent(parent, false);
        RectTransform brt = bottomBar.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.5f, 0);
        brt.anchorMax = new Vector2(0.5f, 0);
        brt.pivot = new Vector2(0.5f, 0);
        brt.anchoredPosition = new Vector2(0, 30);
        brt.sizeDelta = new Vector2(500, 100);

        CreateText("操作", bottomBar.transform, new Vector2(0, 40), new Vector2(500, 30), 20, TextAnchor.MiddleCenter);

        actionBtn1 = CreateButton("操作1", bottomBar.transform, new Vector2(-120, -10), new Vector2(200, 50));
        actionBtn1.onClick.AddListener(OnActionButton1);
        actionBtn1Text = actionBtn1.GetComponentInChildren<Text>();
        actionBtn1CD = CreateText("", actionBtn1.transform, new Vector2(0, -35), new Vector2(200, 20), 14, TextAnchor.MiddleCenter);

        actionBtn2 = CreateButton("操作2", bottomBar.transform, new Vector2(120, -10), new Vector2(200, 50));
        actionBtn2.onClick.AddListener(OnActionButton2);
        actionBtn2Text = actionBtn2.GetComponentInChildren<Text>();
        actionBtn2CD = CreateText("", actionBtn2.transform, new Vector2(0, -35), new Vector2(200, 20), 14, TextAnchor.MiddleCenter);

        bottomBar.SetActive(false);
    }

    void OnActionButton1()
    {
        if (currentSelectedUnit == null || GameManager.Instance.isGameOver) return;

        if (currentSelectedUnit is MissileVehicle mv)
        {
            // 自动攻击最近的敌人
            EnemyPlane nearest = FindNearestEnemy(mv.transform.position);
            if (nearest != null && mv.CanAttack())
            {
                mv.Attack(nearest);
                ShowMessage("导弹发射！");
            }
            else if (nearest == null)
            {
                ShowMessage("没有可攻击的敌方目标");
            }
            else
            {
                ShowMessage("导弹冷却中...");
            }
        }
        else if (currentSelectedUnit is RadarStation rs)
        {
            rs.ToggleMode();
            UpdateBottomBar();
            ShowMessage(rs.isActiveMode ? "雷达站：主动侦察" : "雷达站：静默模式");
        }
        else if (currentSelectedUnit is AntiAirMissileLauncher aa)
        {
            aa.ToggleMode();
            UpdateBottomBar();
            ShowMessage(aa.isActive ? "防空导弹发射器：开机" : "防空导弹发射器：关机");
        }
        else if (currentSelectedUnit is PlayerBase)
        {
            currentState = UIState.Attacking;
            ShowMessage("点击要修复的友方单位");
        }
    }

    void OnActionButton2()
    {
        if (currentSelectedUnit == null || GameManager.Instance.isGameOver) return;

        if (currentSelectedUnit is PlayerBase)
        {
            if (GameManager.Instance.SpendGold(300))
            {
                GameManager.Instance.UpgradeBase();
                ShowMessage("基地已升级！金钱产出提升");
            }
            else
            {
                ShowMessage("金钱不足，升级需要300金钱");
            }
        }
    }

    void CreateMessageText(Transform parent)
    {
        GameObject go = new GameObject("MessageText");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, 300);
        rt.sizeDelta = new Vector2(600, 40);
        messageText = go.AddComponent<Text>();
        messageText.text = "";
        messageText.fontSize = 22;
        messageText.font = uiFont;
        messageText.color = Color.white;
        messageText.alignment = TextAnchor.MiddleCenter;
    }

    void CreateFogOverlay()
    {
        if (GameObject.Find("FogOfWar") != null) return;

        // 世界空间雾效——FogOfWar组件自行创建网格和材质覆盖地图
        GameObject fogGo = new GameObject("FogOfWar");
        fogGo.transform.position = Vector3.zero;
        fogGo.AddComponent<FogOfWar>();
    }

    void CreateGameOverPanel()
    {
        GameObject mainCanvas = GameObject.Find("GameCanvas");
        if (mainCanvas == null) return;

        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(mainCanvas.transform, false);
        RectTransform rt = gameOverPanel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(400, 200);
        Image bg = gameOverPanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.85f);

        GameObject titleGo = new GameObject("GameOverTitle");
        titleGo.transform.SetParent(gameOverPanel.transform, false);
        RectTransform trt = titleGo.AddComponent<RectTransform>();
        trt.anchoredPosition = new Vector2(0, 30);
        trt.sizeDelta = new Vector2(400, 50);
        Text titleText = titleGo.AddComponent<Text>();
        titleText.text = "游戏结束";
        titleText.fontSize = 36;
        titleText.font = uiFont;
        titleText.color = Color.red;
        titleText.alignment = TextAnchor.MiddleCenter;

        GameObject descGo = new GameObject("GameOverDesc");
        descGo.transform.SetParent(gameOverPanel.transform, false);
        RectTransform drt = descGo.AddComponent<RectTransform>();
        drt.anchoredPosition = new Vector2(0, -30);
        drt.sizeDelta = new Vector2(400, 50);
        Text descText = descGo.AddComponent<Text>();
        descText.text = "你的基地已被摧毁";
        descText.fontSize = 20;
        descText.font = uiFont;
        descText.color = Color.white;
        descText.alignment = TextAnchor.MiddleCenter;

        gameOverPanel.SetActive(false);

        // 修复3、5：安全绑定事件，避免空对象与内存泄漏
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= OnGameOverTriggered;
            GameManager.Instance.OnGameOver += OnGameOverTriggered;
        }
    }

    // 安全的游戏结束触发
    void OnGameOverTriggered()
    {
        if (this == null || gameOverPanel == null) return;
        gameOverPanel.SetActive(true);
    }

    Button CreateButton(string label, Transform parent, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.3f, 1f);

        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.3f, 0.3f, 0.5f);
        cb.pressedColor = new Color(0.15f, 0.15f, 0.25f);
        btn.colors = cb;

        GameObject textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);
        RectTransform trt = textGo.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        Text text = textGo.AddComponent<Text>();
        text.text = label;
        text.fontSize = 16;
        text.font = uiFont;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        return btn;
    }

    Text CreateText(string content, Transform parent, Vector2 pos, Vector2 size, int fontSize, TextAnchor anchor)
    {
        GameObject go = new GameObject("Txt_" + content);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Text text = go.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.font = uiFont;
        text.color = Color.white;
        text.alignment = anchor;
        return text;
    }

    void CreateWaveUI()
    {
        GameObject mainCanvas = GameObject.Find("GameCanvas");
        if (mainCanvas == null) return;

        // 波次信息（左上角）
        GameObject waveGo = new GameObject("WaveText");
        waveGo.transform.SetParent(mainCanvas.transform, false);
        RectTransform wrt = waveGo.AddComponent<RectTransform>();
        wrt.anchorMin = new Vector2(0, 1);
        wrt.anchorMax = new Vector2(0, 1);
        wrt.pivot = new Vector2(0, 1);
        wrt.anchoredPosition = new Vector2(30, -10);
        wrt.sizeDelta = new Vector2(200, 40);
        waveText = waveGo.AddComponent<Text>();
        waveText.text = "0/20";
        waveText.fontSize = 26;
        waveText.font = uiFont;
        waveText.color = Color.white;
        waveText.alignment = TextAnchor.MiddleLeft;

        // 倒计时（顶部居中）
        GameObject cdGo = new GameObject("CountdownText");
        cdGo.transform.SetParent(mainCanvas.transform, false);
        RectTransform crt = cdGo.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0.5f, 1);
        crt.anchorMax = new Vector2(0.5f, 1);
        crt.pivot = new Vector2(0.5f, 1);
        crt.anchoredPosition = new Vector2(0, -10);
        crt.sizeDelta = new Vector2(400, 40);
        countdownText = cdGo.AddComponent<Text>();
        countdownText.text = "";
        countdownText.fontSize = 24;
        countdownText.font = uiFont;
        countdownText.color = new Color(1f, 0.85f, 0f);
        countdownText.alignment = TextAnchor.MiddleCenter;
    }

    void CreateWinPanel()
    {
        GameObject mainCanvas = GameObject.Find("GameCanvas");
        if (mainCanvas == null) return;

        winPanel = new GameObject("WinPanel");
        winPanel.transform.SetParent(mainCanvas.transform, false);
        RectTransform rt = winPanel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(400, 300);
        Image bg = winPanel.AddComponent<Image>();
        bg.color = new Color(0, 0.1f, 0.2f, 0.9f);

        GameObject titleGo = new GameObject("WinTitle");
        titleGo.transform.SetParent(winPanel.transform, false);
        RectTransform trt = titleGo.AddComponent<RectTransform>();
        trt.anchoredPosition = new Vector2(0, 60);
        trt.sizeDelta = new Vector2(400, 60);
        Text titleText = titleGo.AddComponent<Text>();
        titleText.text = "WIN";
        titleText.fontSize = 48;
        titleText.font = uiFont;
        titleText.color = Color.green;
        titleText.alignment = TextAnchor.MiddleCenter;

        Button menuBtn = CreateButton("返回菜单", winPanel.transform, new Vector2(0, -40), new Vector2(200, 60));
        menuBtn.onClick.AddListener(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        });

        winPanel.SetActive(false);
    }

    void HookEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnWaveChanged += () =>
            {
                waveSpawnCount = 0;
                waveSpawnTimer = 0f;
            };
            GameManager.Instance.OnVictory += () =>
            {
                if (winPanel != null) winPanel.SetActive(true);
            };
        }
    }

    #endregion

    EnemyPlane FindNearestEnemy(Vector3 from)
    {
        EnemyPlane nearest = null;
        float minDist = float.MaxValue;
        EnemyPlane[] all = FindObjectsOfType<EnemyPlane>();
        foreach (var e in all)
        {
            if (e.isDead) continue;
            // 迷雾中的敌人不可攻击
            if (FogOfWar.Instance != null && !FogOfWar.Instance.IsPositionRevealed(e.transform.position))
                continue;
            float d = Vector3.Distance(from, e.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = e;
            }
        }
        return nearest;
    }

    public void ShowMessage(string msg)
    {
        if (messageText != null)
        {
            messageText.text = msg;
            messageTimer = 2f;
        }
    }

    // 安全销毁事件
    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= OnGameOverTriggered;
        }
    }
}