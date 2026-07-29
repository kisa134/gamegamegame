# 06 · Jira-бэклог — эпик «Первая ночь» (готов к заливке)

> Зафиксированные решения + готовый бэклог. Как только подключишь Jira (коннектор
> Atlassian Rovo) — я залью это структурой Epic → Story → Task, либо ты импортируешь руками.
> Каждая задача несёт фазу пайплайна и **Gate** (критерий приёмки).

---

## Зафиксированные решения (проектные)

- **Биом первого среза:** Альпы (Alpine_Mountain) — эпично, горы и хребты-задники, ближе к пути героя.
- **Нарратив:** крючок Камня Силы закладываем **сразу** — странный артефакт у первого NPC, чтобы тон читался с первой ночи (сам сюжет разворачиваем позже, слоем).
- **Старт исполнения:** dev-контур — Jira + агенты; код Фазы 0 стартует после подключения папки проекта.

---

## Роли dev-агентов (метки в Jira)

`architect` (Design-ревью) · `coder` (Implement) · `qa` (гейты: batchmode-сборка + юнит-тесты) · `art` (сцена/свет/ассеты) · `content` (NPC/диалоги/крючок нарратива). Ты — Principal/PO: аппрувишь переходы фаз и мёрж в main.

**Правило потока:** задача не закрывается без своего Gate. Implement не мёржит в main без прохождения QA-агентом и твоего ревью. Реализация — в изоляции (ветка/worktree).

---

## EPIC: Вертикальный срез «Первая ночь» (Альпы)

Определение готовности эпика = 5 гейтов приёмки: Dependency Audit · Logic Integrity · Headless Testing · AI Consistency · Feel («первая ночь реально приносит удовольствие»).

### STORY 0 — Каркас проекта · фаза Research→Plan
- **0.1** Новый Unity 6000.5.3f1 (URP), 3 asmdef (`Domain`/`Application`/`Unity`) + `Tests`. `art`/`coder`. *Gate: batchmode-сборка пустого решения.*
- **0.2** Перенос из архива в Domain/Application с очисткой от Unity: `GameState`, реестры Item/Recipe/Crop, `WorldClock`, `WeatherSystem`. `coder`. *Gate: Dependency Audit — ноль `using UnityEngine` в Domain/Application.*
- **0.3** Перенос `SyntyMaterialFixer` + `AssetLibrary`/`AssetMap` в Unity-слой; копирование паков Synty; прогон фиксера на тест-сцене Альп. `art`. *Gate: модели не серые на 1 сцене.*

### STORY 1 — Доменное ядро · фаза Implement
- **1.1** `Inventory` (вес/вместимость, приватные поля, `AddItem`/`RemoveItem` с инвариантами). *Gate: Logic Integrity + юнит-тест.*
- **1.2** `Character` (`ApplyDamage`, ХП≥0, смерть, `System.Numerics.Vector3`). *Gate: тест «урон не уводит ХП в минус».*
- **1.3** `Structure`/`BuildingPiece` (стыковка, «не в воздухе», стоимость ресурсов). *Gate: тест «нельзя поставить без ресурсов/в воздухе».*
- **1.4** Event Bus + реестр событий: `TreeChoppedEvent`, `StructurePlacedEvent`, `CharacterDamagedEvent`, `EntityKilledEvent`, `DialogueStartedEvent`, `NightFellEvent`, `ArtifactSensedEvent` (крючок Камня). *Gate: события публикуются только из Use Cases, не из Entity.*

### STORY 2 — Use Cases · фаза Implement
- **2.1** `ChopTreeUseCase`, `GatherStoneUseCase` (DTO→Entity→событие). *Gate: Headless-тест на каждый.*
- **2.2** `PlaceBuildingPieceUseCase`. *Gate: Headless-тест (успех + отказы).*
- **2.3** `TalkToUseCase` (+ ветка «осмотреть артефакт» → `ArtifactSensedEvent`). *Gate: Headless-тест.*
- **2.4** Порты (интерфейсы) мира/спавна/времени. *Gate: реализации только в Unity-слое.*

### STORY 3 — Живой NPC (ORA, без LLM) · фаза Implement
- **3.1** Перенос `NpcBrain`/`NpcTicker`/`NpcWorld`; Observe = окрестность (радиус). `coder`. *Gate: NPC видит не весь мир.*
- **3.2** Reason = дерево поведений (ночью спать, идти к костру, реагировать на игрока). *Gate: AI Consistency — действия через те же Use Cases.*
- **3.3** `Genome`/`Phenotype` → уникальное лицо; `Dialogue` → реплики; у NPC артефакт (крючок). `content`. *Gate: NPC уникален и говорит.*
- **3.4** `GraveViews` → именная могила при смерти (подписчик на `EntityKilledEvent`). *Gate: смерть → могила с именем.*

### STORY 4 — Unity-визуализатор · фаза Implement
- **4.1** Движение + камера заново; `InputRouter`→DTO. `coder`. *Gate: физика — только сенсор, ноль расчётов в `Update`/`OnCollision`.*
- **4.2** Презентеры: подписка на события, спавн через `AssetLibrary`, Humanoid-ретаргет анимаций. *Gate: «HP<0→Destroy» в презентере, не в домене.*
- **4.3** Атмосфера: `DayNightController`, `AmbienceController`+`ProceduralAudio`, `AmbiencePostFx`; `AdminPanel` (F1). `art`. *Gate: цикл суток + звук работают.*

### STORY 5 — Мир и свет Альп · фаза Implement
- **5.1** Вживить авторскую сцену Alpine_Mountain (`AuthoredWorldSetup`), не процедурка. `art`. *Gate: сцена грузится, играбельна.*
- **5.2** Свет золотого часа: солнце (1,0.77,0.48) I=1.2; тёплый туман (1,0.83,0.62) 5→500; прохладный ambient; Bloom+Vignette (HDR вкл). *Gate: скриншот-ревью — кадр как открытка.*
- **5.3** Плотные осмысленные кластеры (лагерь игрока + точка NPC) на фоне горных хребтов-рамок. *Gate: fps в норме на целевом железе.*

---

## Что нужно от тебя для заливки

Подключить коннектор **Atlassian Rovo** (Jira) в claude.ai. После этого скажи «заливай» — создам эпик, истории и задачи с метками и гейтами. Альтернатива, если Jira тяжеловата для соло-старта: **Linear** (легче, быстрее) — тот же бэклог ляжет туда один в один.
