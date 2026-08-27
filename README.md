# SeumPlugins

Набор [BepInEx](https://github.com/BepInEx/BepInEx)-плагинов для игры **SEUM: Speedrunners from Hell**.

## Плагины

### VelocityMeter

Портирует и расширяет оригинальный мод отображения скорости для SEUM.
Показывает текущую горизонтальную/вертикальную скорость игрока и связанную
статистику во время игры и при просмотре реплеев.

Базовая функциональность (счётчик скорости, оверлей, режимы отображения)
соответствует оригинальному моду — подробное описание всех настроек и
возможностей см. на его странице:
**https://link-seum.github.io/velocitymeter/index.html**

Новое в этой версии (добавлено RSC — **Russian Seum Community**, https://seum.online/):
- визуализация траектории (trail) пройденного пути во время просмотра реплея;
- расчёт и подсветка пиковых значений скорости (speed peaks) по кадрам реплея;
- отдельный оверлей ввода/статистики для анализа забегов (спидранов) из реплея.

> Ядро логики (`ModLoader.cs`) перенесено из оригинального мода
> VelocityMeter и лишь обёрнуто в BepInEx-плагин через Harmony-патчи —
> авторство и лицензия базовой функциональности принадлежат оригинальному
> проекту: https://link-seum.github.io/velocitymeter/index.html. Лицензия
> MIT этого репозитория распространяется на код плагинов и добавления RSC,
> а не на перенесённый оригинальный код.

### SeumDiscordRPC

Discord Rich Presence для SEUM: показывает друзьям в Discord, во что вы сейчас
играете (текущий уровень/сцена и т.д.), используя Discord Game SDK.

### LiveScoreSender

Отправляет новые личные рекорды (PB) игрока в реальном времени на бэкенд
**RSC** (https://seum.online/) сразу после установки рекорда, с очередью и
повторными попытками отправки при сбоях сети — рекорды не теряются, даже если
сервер временно недоступен.

## Установка

### 1. BepInEx 5.4.19

Плагины из этого репозитория — это BepInEx-плагины, поэтому сначала нужно
поставить сам **BepInEx 5**. Важно: используется и проверена конкретная
версия — **5.4.19** ([страница релиза на GitHub](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.19)).
Более новые версии (5.4.20, 5.4.21 и т.д.) могут работать некорректно —
ставьте именно 5.4.19.

Установка (см. также [официальную документацию BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html)):

1. Скачайте архив `BepInEx_win_x64_5.4.19.0.zip` (или `x86`, если игра
   32-битная) со страницы релиза выше.
2. Распакуйте содержимое архива прямо в папку с игрой — рядом с
   `Seum.exe` (там же, где лежит `Seum_Data`). После распаковки в корне
   игры должны появиться `winhttp.dll`, `doorstop_config.ini` и папка
   `BepInEx`.
3. Запустите игру один раз и закройте её — BepInEx создаст структуру папок
   (`BepInEx/plugins`, `BepInEx/config` и т.д.) и файл
   `BepInEx/LogOutput.log`.

### 2. Плагины из этого репозитория

Готовые сборки плагинов публикуются на странице релизов репозитория:
**[github.com/Nooomer/SeumPlugins/releases](https://github.com/Nooomer/SeumPlugins/releases)**.

Скачайте нужные `.dll` (`VelocityMeter.dll`, `SeumDiscordRPC.dll`,
`LiveScoreSender.dll` — можно ставить как все сразу, так и по отдельности) и
положите их в папку

```
<папка игры>\BepInEx\plugins\
```

Важно: `Assembly-CSharp.dll` самой игры (`<папка игры>\Seum_Data\Managed\Assembly-CSharp.dll`)
должна оставаться **оригинальной**, без патчей/модификаций. Все плагины из
этого репозитория работают через Harmony-патчи поверх оригинального кода
игры, а не через подмену `Assembly-CSharp.dll` — если вы ранее ставили сюда
модифицированную сборку (например, от старой версии мода), верните
оригинальную из чистой установки/верификации файлов игры в Steam, иначе
плагины могут работать некорректно или не работать вовсе.

После этого запустите игру — в `BepInEx/LogOutput.log` должна появиться
запись о загрузке каждого плагина (`Loading [<имя плагина> <версия>]`), без
предупреждения о несовпадении версии BepInEx.

## Сборка

Проекты собираются под `net472` через SDK-style `.csproj` и зависят от
управляемых сборок самой игры (`Assembly-CSharp.dll`,
`Assembly-CSharp-firstpass.dll`, `Rewired_Core.dll`, `Newtonsoft.Json.dll` из
BepInEx). Путь к ним задаётся MSBuild-свойством `GameManagedDir` (и
`GameBepInExCoreDir` для `LiveScoreSender`), по умолчанию указывающим на
локальную установку Steam:

```
D:\SteamLibrary\steamapps\common\SEUM Speedrunners from Hell\Seum_Data\Managed
```

Для сборки с другим путём (например, в CI):

```bash
dotnet build VelocityMeter/VelocityMeter.csproj -c Release /p:GameManagedDir="<путь до Managed>"
```

## CI/CD

При каждом изменении `<Version>` в одном из `.csproj` на ветке `main`
запускается [`.github/workflows/release.yml`](.github/workflows/release.yml),
который:

1. Определяет, у каких из проектов (`VelocityMeter`, `SeumDiscordRPC`,
   `LiveScoreSender`) версия действительно изменилась по сравнению с
   предыдущим коммитом.
2. Пересобирает **только** изменившиеся проекты.
3. Для проектов без изменений версии — переиспользует DLL из предыдущего
   релиза (без пересборки).
4. Публикует GitHub Release, в который всегда попадают **все N DLL**
   (по числу проектов) — свежесобранные и перенесённые из предыдущего релиза.

### Одноразовая настройка (DLL самой игры)

Управляемые сборки игры не хранятся в репозитории (это код игры, а не
плагинов) и не могут быть получены обычным раннером GitHub Actions. Поэтому
они лежат отдельным приватным кэшем — asset-архивом `GameLibs.zip` в
специальном GitHub Release с тегом `gamelibs-cache` в этом же репозитории;
workflow скачивает его перед сборкой через `GITHUB_TOKEN`. Без этого шага
любой запуск workflow упадёт на шаге "Fetch game reference DLLs".

Архив `GameLibs.zip` должен содержать (без вложенных папок, файлы прямо в
корне архива) три файла из `Seum_Data/Managed` установленной игры:
`Assembly-CSharp.dll`, `Assembly-CSharp-firstpass.dll`, `Rewired_Core.dll`.

**Вариант A — через веб-интерфейс GitHub (без `gh` CLI):**

1. Откройте `https://github.com/Nooomer/SeumPlugins/releases/new`.
2. В поле "Choose a tag" введите `gamelibs-cache` и нажмите "Create new tag".
3. Заголовок — например `Game reference libraries (private cache)`.
4. Перетащите файл `GameLibs.zip` в область "Attach binaries".
5. Отметьте галочку **"Set as a pre-release"** (это служебный кэш, а не
   релиз плагинов).
6. Нажмите **"Publish release"**.

**Вариант B — через `gh` CLI** (один раз или при обновлении игры):

```bash
gh release create gamelibs-cache GameLibs.zip \
  --repo Nooomer/SeumPlugins \
  --title "Game reference libraries (private cache)" \
  --notes "Assembly-CSharp.dll, Assembly-CSharp-firstpass.dll, Rewired_Core.dll — служебный кэш для CI, не публичный релиз плагинов." \
  --prerelease
```

Если версия игры (а значит и её DLL) обновится — просто загрузите новый
`GameLibs.zip` тем же способом как новый asset в существующий релиз
`gamelibs-cache` (в вебе — открыть релиз → Edit → заменить файл; в CLI —
`gh release upload gamelibs-cache GameLibs.zip --clobber`).

### Как запустить релиз вручную

Обычный триггер — пуш в `main` с изменённой `<Version>` хотя бы в одном
`.csproj`. Если версии не менялись (например, для самого первого прогона
пайплайна), workflow не найдёт изменений и ничего не соберёт — это
ожидаемо, не баг.

Чтобы собрать и выпустить релиз со всеми текущими плагинами вручную,
независимо от версий:

1. Вкладка **Actions** в репозитории → workflow **"Build & release plugins"**.
2. Кнопка **"Run workflow"** (справа) → ветка `main`.
3. Поставьте галочку **`force`** — "Пересобрать и выпустить релиз со всеми
   плагинами, даже если версии не менялись".
4. **"Run workflow"**.

После успешного прогона в разделе **Releases** появится новый релиз с тремя
DLL (`VelocityMeter.dll`, `SeumDiscordRPC.dll`, `LiveScoreSender.dll`).
