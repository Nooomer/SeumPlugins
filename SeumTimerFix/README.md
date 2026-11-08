# SeumTimerFix — фикс таймербага

Чинит [Timer Bug](https://seum.online/docs#prohibited.10): после того как игра
работает достаточно долго (баг начинает проявляться примерно после **70 минут**),
игровой таймер постепенно рассинхронизируется с тикрейтом. На таймере появляются
неровные шаги (например 0.018, 0.032, 0.048 вместо ровного шага 1/60), и забег
начинает случайно выигрывать или терять несколько миллисекунд.

## Почему это происходит

Время уровня в игре считается так (весь расчёт в `float32`):

```
GameManager.calculateCurrentLevelTime() =
    Time.fixedTime - levelSpawnTime + levelFinishTimeOffset
```

- `levelSpawnTime` запоминается как `Time.fixedTime` на первом вводе в забеге;
- итоговый скор — это `(int)(time * 1000)` миллисекунд.

`Time.fixedTime` — `float32`, который накапливается прибавлением
`fixedDeltaTime` на каждом физическом тике и **не сбрасывается** за сессию.
Через ~70 минут (`≈ 4200 с`) точность `float32` падает до ~0.49 мс на единицу
младшего разряда: каждое прибавление тика округляется на всё более грубой сетке,
шаг таймера перестаёт совпадать с шагом 1/60, и погрешность случайно
накапливается в забеге — «сохраняя или ухудшая несколько миллисекунд», как
описано в теории.

## Что делает плагин

Плагин ведёт собственный счётчик физических тиков в `long` и возвращает игре
вместо float-вычитания точное значение:

```
время уровня = ticks * (double)Time.fixedDeltaTime + levelFinishTimeOffset
```

Точка отсчёта (`SpawnTick`) ставится ровно там же, где игра ставит
`levelSpawnTime`:

1. в `GameManager.FixedUpdate`, когда игра «съедает» флаг `captureLevelSpawnTime`;
2. в `GameManager.gameplayStateChanged`, когда забег заканчивается (учёт
   наигранного времени).

Между этими точками значения совпадают с оригиналом до долей микросекунды —
в короткой сессии поведение неотличимо от ванили, а после долгой сессии таймер
остаётся ровно на тиковой сетке.

Физика игры не трогается вообще: патчится только источник времени для
`calculateCurrentLevelTime()`, которым пользуются HUD, скор финиша, общее время
спидрана и учёт наигранного времени. `Time.fixedDeltaTime` игра нигде не меняет,
поэтому `ticks * fixedDeltaTime` — это ровно та сетка, на которой должен был
идти таймер.

## Диагностика

По клавише **F6** включается оверлей, который показывает дрейф таймера вживую:

```
SeumTimerFix   (F6 - toggle)
session 4213.7 s   float ULP 0.488 ms
fixedTime drift +11.234 ms   min -1.832   max +11.234   |max| 11.234
run drift -0.452 ms   |max| 0.512
```

- **fixedTime drift** — насколько `float32`-счётчик `Time.fixedTime` ушёл от точной
  тиковой сетки (`тик × fixedDeltaTime`). Это и есть таймербаг: у свежей игры
  дрейф ~0, после часа сессии он начинает расти, и именно эти миллисекунды
  случайно попадают в ванильное время забега.
- **run drift** — насколько «поплывшее» ванильное вычисление
  `Time.fixedTime - levelSpawnTime + offset` отличалось бы от тикового значения
  прямо сейчас в текущем забеге. Сразу после старта забега оно равно нулю и
  растёт вместе с ошибкой float.
- **float ULP** — шаг сетки float32 при текущей величине `Time.fixedTime`
  (~0.488 мс после часа сессии).

## Настройки

Файл `BepInEx/config/SeumTimerFix.cfg` создаётся автоматически при первом
запуске:

```ini
[01 - General]

## Master switch. When false no Harmony patch is applied at all.
# Setting type: Boolean
# Default value: true
Enabled = true

[08 - Diagnostics]

## Draw a small overlay with the timer drift: how far Unity's float Time.fixedTime
## has moved off the exact tick grid (the timer bug), and how far the vanilla
## level time would be off right now.
# Setting type: Boolean
# Default value: false
Overlay = false

## Toggles the overlay at runtime.
# Setting type: KeyCode
# Default value: F6
OverlayKey = F6
```

## Совместимость

- Плагин требует **оригинальный** `Assembly-CSharp.dll` — как и остальные
  плагины этого репозитория, он работает Harmony-патчами поверх него.
- Рекорды, поставленные с плагином, засчитываются так же, как ванильные: в
  короткой сессии значения совпадают с оригиналом (расхождение — только тот
  дрейф, который плагин и убирает).
