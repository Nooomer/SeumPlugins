# SeumPerf

Плагин оптимизации производительности для **SEUM: Speedrunners from Hell**.

Работает через Harmony-патчи поверх оригинальной `Assembly-CSharp.dll` —
подменять файлы игры не нужно.

## Принцип

Все патчи разделены на две группы, и это отражено в значениях по умолчанию:

* **Включено по умолчанию** — патчи, которые *не меняют поведение игры*.
  Они убирают только повторный расчёт и повторные аллокации того, что игра уже
  посчитала. Кадр должен выглядеть и вести себя идентично ванили.
* **Выключено по умолчанию** — всё, что меняет картинку ради кадров
  (`05 - Rendering`, `06 - Quality`). В спидран-игре менять то, что видит
  игрок, без явного согласия — плохая идея.

Замерить эффект можно встроенным оверлеем (`08 - Diagnostics / Overlay`,
по умолчанию клавиша **F10**): frame time, 1% low, worst frame, объём
managed-кучи, скорость аллокаций (КБ/с) и число сборок мусора.

## Что именно оптимизируется

### Аллокации (мусор → просадки кадров при сборке GC)

| Патч | Что в ванили |
|---|---|
| `TrimProjectilePaths` | Каждый снаряд в конструкторе выделяет буфер траектории `PathPoint[4096]` — около **80 КБ на каждый выстрел**, при том что реально заполняется обычно менее сотни точек. Патч уменьшает стартовый размер (по умолчанию 128) и наращивает буфер по мере надобности, с тем же жёстким потолком 4096 — поведение `addPathPoint`/`generatePath` не меняется. |
| `ReuseTrailMeshBuffers` | `ProjectileTrail.LateUpdate` каждый кадр для каждого живого следа выделяет заново `Vector3[]`, `Vector2[]`, `Color[]` и `int[]`, размер которых растёт вместе с длиной траектории. Патч переиспользует буферы (`Mesh.SetVertices`/`SetUVs`/`SetColors`/`SetTriangles`) — та же геометрия, без мусора. |
| `StripCollisionDebugStrings` | `CollisionRule.reportCollision` на **каждый контакт каждого физического шага** склеивает строку `"Custom - " + DEBUG`, а `Projectile.castCollisions` — `DEBUG_ORIGIN + "Sphere"`. Результат читает только `generateCollisionReport()`, который в игре нигде не вызывается. Патч убирает саму конкатенацию, метка сохраняется. |
| `GuardInputString` | `Game.commonLateUpdate` читает `Input.inputString` каждый кадр ради сканирования чит-кодов; свойство выделяет новую строку на каждое обращение. Патч читает его только на кадрах, где реально зажата клавиша. |

### Повторные поиски

| Патч | Что в ванили |
|---|---|
| `CacheComponentLookups` | Кэширует беспараметрные `GetComponent` / `GetComponentInChildren` / `GetComponentInParent` в покадровых методах. Самый заметный случай — `HandEffects.Update`: **4 обхода иерархии `GetComponentInChildren<Animation>()` и 4 `GetComponent<MeshRenderer>()` за кадр**. Дальше — `GameManager.FixedUpdate` (4 поиска за физический шаг), `CharacterMotor.performFixedUpdate`, `Mine.Update`, `MaterialRenderQueue.Update` и др. Кэш перепроверяет себя: уничтоженный компонент и сменившаяся активная рука приводят к обычному поиску. |
| `CacheCameraMain` | `Camera.main` на Unity 2018 — это поиск по тегу по всей сцене (кэширование появилось только в 2020.2). `Projectile.Update` и `Dart.FixedUpdate` вызывают его по разу на экземпляр за кадр. |
| `CacheShaderPropertyIds` | Строковые перегрузки `Material.SetColor("_Color", …)` и т.п. в покадровом коде заменяются на целочисленные id. Больше всего вызовов — в `SSAOPro.OnRenderImage` (11) и в оверлейных эффектах. |
| `CachePortalMaterials` | `Renderer.materials` выделяет новый массив на **каждое** чтение. `PortalRenderer` читает его на каждый портал на каждую рендерящую камеру каждый кадр — только чтобы переприсвоить ту же render texture. |
| `CacheLevelNames` | `Hud.levelNameString` собирает отображаемое имя уровня заново на каждый вызов — несколько `Array.IndexOf`, `ToUpper()` и склейки, — хотя ответ зависит только от номера уровня. Мемоизация. Уровень 136 не кешируется: его имя зависит от черепа, который можно получить по ходу сессии. |
| `CacheLevelData` | `Game.getCurrentLevelData()` мемоизируется в пределах кадра. На воркшоп-уровнях он линейно обходит список и может аллоцировать; `Helliku.Update` дёргает его каждый кадр. |

### Лишняя работа каждый кадр

| Патч | Что в ванили |
|---|---|
| `RingTriggerOnStateChange` | `RingTrigger.Update` каждый кадр вызывает `SetActive` на **всех** объектах трёх массивов, даже когда состояние не менялось. Патч применяет состояние только при его смене — плюс периодическое перепроставление (со сдвигом по экземплярам), чтобы всё восстановилось, если объекты переключил кто-то ещё. |
| `HellikuEarlyOut` | `Helliku.Update` — одноразовая замена материала, но проверка `getCurrentLevelData()` выполняется раньше проверки «уже сделано». |
| `DedupeAudioMixerWrites` | `AudioManager.LateUpdate` пишет пять громкостей в `AudioMixer` каждый кадр независимо от того, менялись ли они. Раз в 120 кадров запись принудительно повторяется — на случай, если микшер молча не принял значение. |
| `SkipUnusedAimPath` | `FPSInputController.Update` каждый кадр безусловно вызывает `Projectile.generatePath` — цикл до 100 итераций, в каждой `Physics.SphereCastNonAlloc`. Результат читается только при slow-motion прицеливании (`slowTimeTimestamp > 0`): иначе `characterView.trail` и `trailHit` выключены, а `characterView.path` больше никто не читает (единственный потребитель — `projectileTrail.path = path` в `CharacterView`, и трейл в этот момент неактивен). Патч пропускает расчёт на таких кадрах, читая то же поле, что и следующая строка оригинала — то есть без рассинхрона. |
| `CacheSteamUserId` | `LeaderboardsBackend.isScoreCurrentUser` спрашивает у Steam id локального игрока **на каждую строку таблицы рекордов на каждое событие OnGUI**. Замер на экране старта уровня: ~40 вызовов и **~3,09 мс за кадр** — больше, чем всё остальное на этом экране вместе взятое, потому что каждый вызов идёт через `InteropHelp.TestIfAvailableClient()` и два нативных P/Invoke. Id не может измениться за время работы процесса, поэтому читается один раз. Пока Steam не инициализирован, отрабатывает оригинал — поведение «не инициализирован → false» сохраняется. |
| `CachePostEffectResourceChecks` | `GhostOverlayEffect`, `ShadowWorldOverlayEffect` и `ExposureCorrection` внутри `OnRenderImage` каждый кадр заново гоняют проверку возможностей через `SystemInfo`. Как только материал создан, ответ измениться не может. |

### Рендер и качество (по умолчанию выключено)

| Настройка | Эффект |
|---|---|
| `DisableIdleOverlayEffects` | Отключает ghost- и shadow-world-оверлеи, пока их `intensity` равен нулю, чтобы камера оружия не гоняла полноэкранный проход впустую. Выключено по умолчанию: патч исходит из того, что при `intensity = 0` шейдер — точная копия входа; это вероятно, но не проверено. |
| `PortalTextureSize` | Портал перерисовывает всю сцену в текстуру 1024×1024 на каждый видимый портал на каждую камеру. Уменьшение вдвое — примерно четырёхкратная экономия на этом проходе. |
| `ShadowDistance`, `ShadowCascades`, `PixelLightCount`, `LodBias`, `MasterTextureLimit`, `ParticleRaycastBudget`, `DisableAnisotropicFiltering`, `DisableSsao` | Обычные рычаги `QualitySettings`, переприменяются раз в 60 кадров, потому что игра сбрасывает пресет качества при заходе в настройки. |
| `TargetFrameRate`, `VSyncCount` | `-1` — не трогать, оставить как есть. |

## Профайлер

`08 - Diagnostics / Profiler = true` вешает `Stopwatch` на конкретные методы и показывает в оверлее
разбивку «мс за кадр». Клавиша оверлея — та же (`OverlayKey`, по умолчанию **F10**).

Инструментируются, среди прочего: `Hud.OnGUI` и его внутренности (`startLevelAimUI`, `inGameUI`,
`drawScores`, `drawLevelInfoContent`), `MainMenu.OnGUI`, `LevelSelector.OnGUI`,
`FPSInputController.Update` и вложенный `Projectile.generatePath`, `GameManager.FixedUpdate` и
вложенные `CharacterMotor.performFixedUpdate` / `handleCollisions`, `PortalRenderer.OnWillRenderObject`,
все `OnRenderImage` (SSAO Pro, SMAA, Colorful RadialBlur/GaussianBlur, ghost/shadow-world/exposure),
а также `OnGUI` соседних плагинов (`ModLoader` из VelocityMeter).

Для разбора проблем с высокой частотой опроса мыши инструментирован ещё и путь ввода:
`Rewired.InputManager_Base.Update` / `FixedUpdate`, `GameCursor.performUpdate`,
`Game.commonLateUpdate` и `FPSInputController.handleMouseLook`. Если время кадра растёт,
а `measured (top level)` остаётся прежним — стоимость внутри движка, а не в скриптах игры.

Как читать вывод:

- строки с `·` — **вложенные**: их время уже входит в строку выше по стеку, это разбивка, а не
  добавка. В сумму `measured (top level)` они не попадают;
- `xN` — сколько раз метод вызывался за кадр (для `OnGUI` это обычно 2: Layout + Repaint);
- разница между `measured (top level)` и полным временем кадра — то, что мод не измеряет: нативный
  рендер, физика, `Update` неинструментированных MonoBehaviour.

Патчи профайлера применяются не в `Awake`, а в `Start`: BepInEx грузит плагины по очереди, и на
момент `Awake` сборка VelocityMeter может быть ещё не загружена.

Профайлер не бесплатный — два чтения таймера и поиск в словаре на вызов, поэтому по умолчанию он
выключен, а числа для самых горячих методов слегка завышены.

## Замеры на этом железе

Профайлер на экране ожидания старта уровня (RTX 4070 SUPER, 2560×1440, ~200 fps до правок):

```
Hud.OnGUI                     3,58  x2,1
· Hud.startLevelAimUI         3,53  x2,1
· Hud.drawScoresSinglePlayer  3,42  x2,1
· Hud.drawScores              3,41  x2,1
· LeaderboardsBackend.isScore 3,09  x39,6   ← Steam P/Invoke на каждую строку
· Hud.drawScoreLine           0,24  x29,2
· SeumUI.label                0,23  x58,5   ← вся отрисовка текста IMGUI
· Hud.inGameUI                0,06  x2,1
  ModLoader.OnGUI             0,05  x2,1
  FPSInputController.Update   0,03  x1
· Hud.levelNameString         0,01  x6,3
```

Вывод: 3,09 мс из ~5 мс кадра — это опрос Steam, а не отрисовка. IMGUI-текст стоит 0,23 мс,
то есть оптимизировать его смысла нет.

## Конфигурация

Файл создаётся при первом запуске:

```
<папка игры>\BepInEx\config\SeumPerf.cfg
```

Значение `-1` в числовых настройках означает «не вмешиваться».
`VerboseLogging = true` печатает в `BepInEx/LogOutput.log` каждый применённый
патч — полезно, если после обновления игры какой-то метод перестал находиться:
такие случаи логируются как предупреждение и просто пропускаются, остальные
патчи продолжают работать.

<details>
<summary>Пример конфига (SeumPerf.cfg) со всеми параметрами</summary>

```ini
[01 - General]

## Общий выключатель. При false не применяется ни один патч.
Enabled = true

[02 - Allocations]

## Каждый снаряд при спавне выделяет буфер траектории на 4096 точек (~80 КБ),
## хотя реально заполняется обычно меньше сотни. Старт с малого размера,
## рост по мере надобности, потолок тот же.
TrimProjectilePaths = true

## Стартовый размер буфера траектории снаряда. 128 хватает на обычный полёт
## фаербола без единого доращивания.
# Допустимые значения: 16-4096
ProjectilePathInitialCapacity = 128

## Следы снарядов каждый кадр пересобирают меш и выделяют 4 новых массива —
## для каждого живого следа. Переиспользовать буферы вместо этого.
ReuseTrailMeshBuffers = true

## Каждый обработанный контакт склеивает отладочную строку, которую нигде не
## читают. Метка остаётся, склейка убирается.
StripCollisionDebugStrings = true

## Input.inputString выделяет строку каждый кадр ради проверки чит-кодов.
## Читать её только на кадрах с реально зажатой клавишей.
GuardInputString = true

[03 - Lookups]

## Кэшировать беспараметрные GetComponent/GetComponentInChildren/
## GetComponentInParent в покадровых методах, которые вызывают их повторно.
CacheComponentLookups = true

## Camera.main на Unity 2018 — поиск по тегу по всей сцене. Снаряды и дротики
## зовут его по разу на кадр на экземпляр.
CacheCameraMain = true

## Заменить строковые Material.SetColor("_Name", ...) в покадровом коде на
## версии с целочисленным id свойства.
CacheShaderPropertyIds = true

## Renderer.materials выделяет новый массив на каждое чтение. Порталы читают
## его на каждый портал на каждую камеру каждый кадр.
CachePortalMaterials = true

## Мемоизировать Game.getCurrentLevelData() в пределах кадра. На воркшоп-
## уровнях он обходит список и может аллоцировать.
CacheLevelData = true

## Hud.levelNameString каждый раз пересобирает имя уровня с нуля, хотя ответ
## зависит только от номера уровня. Мемоизация.
CacheLevelNames = true

[04 - Redundant work]

## Кольцевые триггеры каждый кадр зовут SetActive на всех объектах трёх
## массивов, даже когда ничего не изменилось. Применять состояние только
## при его смене (плюс периодическая переприменение на всякий случай).
RingTriggerOnStateChange = true

## Пропускать Helliku.Update, если его одноразовая замена материала уже
## случилась.
HellikuEarlyOut = true

## Аудио-менеджер каждый кадр пишет пять громкостей в микшер независимо от
## того, менялись ли они.
DedupeAudioMixerWrites = true

## Оверлейные эффекты каждый кадр заново гоняют проверку возможностей через
## SystemInfo внутри OnRenderImage. Ответ не может измениться, когда материал
## уже создан.
CachePostEffectResourceChecks = true

## isScoreCurrentUser спрашивает у Steam id локального игрока на каждую
## строку таблицы рекордов на каждое событие OnGUI — замерено ~40 вызовов и
## ~3 мс за кадр на экране старта уровня. Id не меняется, читать один раз.
CacheSteamUserId = true

## FPSInputController.Update каждый кадр безусловно строит линию прицеливания
## (до 100 шагов с Physics.SphereCastNonAlloc). Результат нужен только при
## активном прицеливании в slow-motion — на остальных кадрах пропускать.
SkipUnusedAimPath = true

[05 - Rendering]

## Отключать ghost- и shadow-world-оверлеи, пока их intensity равен нулю,
## чтобы камера оружия не гоняла полноэкранный проход впустую.
DisableIdleOverlayEffects = false

## Размер текстуры портала. Портал перерисовывает всю сцену в текстуру на
## каждый видимый портал на каждую камеру; уменьшение вдвое — примерно
## четырёхкратная экономия. -1 — не трогать.
# Допустимые значения: -1, 128, 256, 512, 1024
PortalTextureSize = -1

[06 - Quality]

## Application.targetFrameRate. -1 — не трогать (без ограничения).
TargetFrameRate = -1

## QualitySettings.vSyncCount. -1 — оставить в ведении игровых настроек.
# Допустимые значения: -1-4
VSyncCount = -1

## Дальность теней в метрах. Обычно самая большая бесплатная экономия GPU.
## -1 — не трогать.
ShadowDistance = -1

## QualitySettings.shadowCascades. -1 — не трогать.
# Допустимые значения: -1, 0, 1, 2, 4
ShadowCascades = -1

## Число попиксельных источников света. Меньше — меньше проходов рендера.
## -1 — не трогать.
PixelLightCount = -1

## QualitySettings.particleRaycastBudget. -1 — не трогать.
ParticleRaycastBudget = -1

## Ограничение разрешения текстур: 0 — полное, 1 — половина, 2 — четверть.
## Помогает при нехватке VRAM. -1 — не трогать.
# Допустимые значения: -1-3
MasterTextureLimit = -1

## QualitySettings.lodBias. Меньше 1 — более дешёвые LOD раньше. -1 — не трогать.
LodBias = -1

## Принудительно выключить анизотропную фильтрацию.
DisableAnisotropicFiltering = false

## Выключить SSAO на всех камерах, где он есть.
DisableSsao = false

[08 - Diagnostics]

## Показывать небольшой оверлей с frame time и сборкой мусора.
Overlay = false

## Замерять отдельные методы и показывать в оверлее разбивку «мс за кадр».
## Сам по себе немного нагружает — включайте только для замеров.
Profiler = false

## Клавиша переключения оверлея.
OverlayKey = F10

## Логировать каждый применённый патч в BepInEx/LogOutput.log.
VerboseLogging = false
```

</details>

## Сборка

```bash
dotnet build SeumPerf/SeumPerf.csproj -c Release
```
