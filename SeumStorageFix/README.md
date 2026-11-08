# SeumStorageFix — фикс стораджей

Чинит семейство багов «storage» — перенос чего-либо из одного забега в
следующий ([Teleport Storage](https://seum.online/docs#prohibited.7),
[Death Barrier Teleport Storage](https://seum.online/docs#prohibited.8),
[Storage](https://seum.online/docs#prohibited.9)):

- **портал-сторадж**: если рестартовать уровень достаточно быстро, закешированное
  завершение старого забега срабатывает уже в новом — уровень можно «пройти» за
  0.000;
- **телепорт-сторадж**: активация телепорта (или рёв) ровно в миллисекунду
  касания земли тратит заряд, а приземление в тот же тик возвращает его обратно —
  в воздухе остаются полные заряды и бесплатные телепорты на полной мощности;
- **сторадж предметов**: часть полей состояния персонажа переживает рестарт,
  потому что штатный сброс игры их не очищает.

## Что делает плагин

### 1. Глубокая очистка при рестарте

После `Game.restartLevel` (свои сбросы игра уже выполнила) плагин дочищает то,
что оригинал пропускает:

- **очередь отложенных переходов состояния** (`nextStateStack`) очищается —
  закешированный `FINISH_LEVEL` старого забега больше не может завершить новый
  (это и есть 0.000-финиш портал-стораджа);
- **защита от повторного финиша**: `lastFinishSessionId` приравнивается к
  `gameSessionId`, так что поздний вызов `finishLevel()` старого забега
  отклоняется штатной проверкой игры;
- незавершённое состояние таймера (`levelFinishTimeOffset`, флаги захвата
  времени спавна) сбрасывается вместе с забегом;
- на персонаже очищаются ссылки, которые `RestartLevelCallback` не трогает:
  летящие телепорт/платформа (`activeTeleportProjectile`,
  `activePlatformProjectile`), сигналы телепорта, `portalPassTime`,
  `waitForNextCycleTimestamp`, состояние руки (`handEqupied`), тайминги рёва и
  телепорта, заряды (`teleportsUsedInAir`, `chargesActive`);
- сбрасываются остатки финишного/смертного UI (`winUIFinishedFadingIn`,
  `deathTimer`, `portalReachedTimer`).

### 2. Защита окна активации от «бесплатного» возврата заряда

`FPSInputController.performLand` при каждом приземлении возвращает заряды
телепорта и рёва — это штатная механика игры (на ней же построен легальный
речардж на клетке в 16 — Peekaboo). Баг в том, что возврат срабатывает и в тот
же тик, когда активация только что потратила заряд.

Плагин запоминает счётчики до приземления и восстанавливает их, если
приземление случилось в пределах **0.2 с** после активации — это то же окно,
которое сама игра использует как кулдаун телепорта. Приземление вне окна
речарджит ровно как в ванили, легальный речардж на клетке не затрагивается
(выстрел в воздухе ставит `teleportedInAirTime = -1`, и окно защиты не
срабатывает).

## Лог предотвращённых стораджей

Плагин по умолчанию пишет в `BepInEx/LogOutput.log` каждый реально
предотвращённый сторадж. Примеры записей:

```
[Info :SeumStorageFix] SeumStorageFix: restart cleaned up 3 leftover(s) from the previous run:
[Info :SeumStorageFix]   dropped 1 pending state transition(s) on restart: FINISH_LEVEL (portal storage)
[Info :SeumStorageFix]   cleared leftover teleport projectile
[Info :SeumStorageFix]   cleared portal-pass timer 0.412
[Info :SeumStorageFix] SeumStorageFix: landing 0.017 s after teleport activation - charge refund suppressed (restored teleportsUsedInAir 1, chargesActive 2)
```

События редкие — в обычной игре лог молчит; каждая строка означает, что
конкретный сторадж в этот момент был бы возможен без плагина. Отключается
параметром `LogPreventedStorages`.

## Настройки

Файл `BepInEx/config/SeumStorageFix.cfg` создаётся автоматически при первом
запуске:

```ini
[01 - General]

## Master switch. When false no Harmony patch is applied at all.
# Setting type: Boolean
# Default value: true
Enabled = true

[02 - Restart]

## Do not let a landing refund the teleport/roar charge that an activation spent on
## the same instant (the teleport-storage window). A normal landing recharges as
## usual - only the 0.2 s window right after an activation is protected.
# Setting type: Boolean
# Default value: true
FixRechargeRace = true

[08 - Diagnostics]

## Log every storage the plugin prevented: dropped pending state transitions on
## restart, cleared leftovers of the previous run, and suppressed charge refunds
## inside the activation window. These events are rare, so the log stays quiet in
## normal play.
# Setting type: Boolean
# Default value: true
LogPreventedStorages = true
```

## Совместимость

- Плагин требует **оригинальный** `Assembly-CSharp.dll` — как и остальные
  плагины этого репозитория, он работает Harmony-патчами поверх него.
- На обычную игру плагин не влияет: рестарт и так почти всё сбрасывает, патч
  закрывает только известные дыры; приземления с речарджем работают как раньше
  за единственным исключением — приземления в первые 0.2 с после активации.
