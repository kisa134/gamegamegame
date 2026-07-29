# Runbook для Claude Code — автономный git-исполнитель

Ты работаешь в репозитории `kisa134/gamegamegame` (папка MYGAME) с доступом к GitHub.
Оркестратор проекта — Claude в Cowork (COO): он ведёт Jira, ревьюит, пишет код и задачи.
Твоя работа — исполнять git/сборку/PR/мёрж автономно. Правила проекта — в `CLAUDE.md`.
Метод: Clean Architecture, ядро без Unity, гейты (build+test зелёные, ноль UnityEngine в Domain/Application).

## Задача этого запуска: свести текущий код в `main`

### Шаг 0 — состояние
```
git fetch origin
git status
```
На рабочей ветке есть неотслеживаемые файлы (docs/09, briefs/, cursor-brief.md, патч). Разберись аккуратно, ничего не теряя.

### Шаг 1 — смёржить PR #1 (SCRUM-14: каркас + Inventory, уже отревьюен COO, одобрен)
```
gh pr merge 1 --squash --delete-branch=false
git checkout main && git pull origin main
```

### Шаг 2 — довнести доки/брифы в main (аддитивно)
```
git add docs/ briefs/ cursor-brief.md CLAUDE.md README.md .gitignore
git commit -m "docs: orchestration map, agent briefs"
git push origin main
```
Примечание: если в корне есть случайная папка `MYgame/` (дубль) — проверь содержимое; если пусто/мусор, не коммить (в .gitignore её нет, просто не добавляй).

### Шаг 3 — вкатить Story 1 (мой готовый, проверенный код: Character, BuildingPiece, Event Bus + тесты)
```
git checkout -b feature/jira-SCRUM-15
git apply story1-core.patch      # если строгий контекст мешает: git apply --3way story1-core.patch
dotnet test Game.sln             # должно быть зелёно (я проверял логику: 17/17)
git add -A
git commit -m "SCRUM-15: Character, BuildingPiece, SupportKind, domain events, InMemoryEventBus + tests"
git push origin feature/jira-SCRUM-15
gh pr create --base main --head feature/jira-SCRUM-15 \
  --title "SCRUM-15: Character, BuildingPiece, Event Bus" \
  --body "Story 1 ядро. Аддитивно поверх каркаса. Гейты: build+test зелёные, ноль UnityEngine в Domain/Application. Ревью COO: одобрено."
```
Дождись зелёного CI и смёржи:
```
gh pr merge --squash --delete-branch
```

### Шаг 4 — отчитайся
Верни COO (в Cowork) список: смёрженные PR, финальный SHA `main`, вывод `dotnet test`. COO переведёт тикеты SCRUM-14 и SCRUM-15 в Done в Jira.

## Дальше (после этого запуска)
Следующая полоса — **Story 2, Use Cases** (`Game.Application`): `ChopTreeUseCase`, `GatherStoneUseCase`, `TalkToUseCase` + порты, каждый с headless-тестом (DTO → Entity → публикация события через `InMemoryEventBus`). COO заведёт тикеты и, если нужно, пришлёт заготовки. Бери из Jira (метка `agent-eligible`) или по указанию COO.

## Инварианты, которые нельзя нарушать
- В `main` вливается только зелёный CI.
- Ноль `using UnityEngine/UnityEditor/TMPro` в `Game.Domain` и `Game.Application`.
- Rich Entities: приватные поля, инварианты в методах. События публикуют Use Cases, не сущности.
- Не удаляй чужой код без причины; работай в своих ветках `feature/jira-SCRUM-*`.
