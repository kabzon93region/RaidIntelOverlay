# Changelog — RaidIntelOverlay

## [1.2.2] — 2026-06-12

### Fixed
- **Headless с клиентского ПК (requester):** роль хоста/клиента через `FikaBackendUtils.IsServer` — requester всегда клиент, dedicated/headless всегда хост
- Синк на headless-хосте стартует при `IBotGame`, даже если `MainPlayer` ещё не готов
- Пакеты `ReliableOrdered` + отправка через тот же `IFikaNetworkManager`, что регистрирует handlers
- Запрос snapshot сразу при `FikaRaidStartedEvent` на клиенте
- Подпись «ожидание данных хоста» только у реальных клиентов, не у хоста рейда

## [1.2.1] — 2026-06-21

### Fixed
- Подсчёт **игроков на headless-хосте** через `CoopHandler.HumanPlayers` (клиенты не попадают в `RegisteredPlayers`)
- USEC/BEAR в snapshot больше не 0 при подключённых игроках на dedicated headless

## [1.2.0] — 2026-06-21

### Added
- Поддержка **Fika Headless**: хост определяется через `IsServer` + `IsHeadless`/`FikaServer`
- Периодическая рассылка snapshot с хоста каждые 3 с (не только по запросу клиента)
- Клиент кэширует последний snapshot — нет гонки request/display в одном кадре
- Лог `[RAID_INTEL] Snapshot from host` при получении данных
- Индикатор устаревших данных хоста и «ожидание данных хоста»

### Fixed
- Raid Intel на клиентах headless-рейда: боты/игроки берутся с dedicated-хоста

## [1.1.6] — 2026-06-12

### Added
- Секция **МЕСТНОСТИ**: координаты, ближайшая зона, активные TriggerZones
- Список зон из трёх источников: точки входа (SpawnPointParams), квестовые триггеры (TriggerWithId), AI-зоны (AIPlaceInfo)
- Заголовок **БОТЫ НА КАРТЕ (N)** — общее число живых ботов

## [1.1.5] — 2026-06-14

### Fixed
- Убраны из виджета устаревшие Tpl из cfg (`5447a9e4…`, автомобильная аптечка) — фильтр legacy + скрытие неизвестных шаблонов
- Дефолт `Template Ids` в cfg пустой: основной список только в `tracked-items.txt`

## [1.1.4] — 2026-06-14

### Changed
- Дефолтный набор отслеживаемых предметов: видеокарта, Bitcoin, LEDX, REAP-IR, FLIR RS-32
- `tracked-items.txt` и значение по умолчанию в cfg обновлены для тестов и распространения

## [1.1.3] — 2026-06-14

### Fixed
- **Штурман:** `bossKojaniy` / `followerKojaniy` отображаются как «Штурман» / «Свита Штурмана» (в коде игры Kojaniy = Shturman)
- **Мёртвые боты:** в счётчик не попадают боты с `IsDead`, `EBotState.Disposed` или `HealthController.IsAlive == false`

### Changed
- Расширены русские подписи для свит боссов (Глухарь, Решала, Кабан и др.)

## [1.1.2] — 2026-06-14

- Fika sync игроков/ботов хост→клиент
- ItemNameResolver через игровой API
- Уменьшена ширина виджета, 2 колонки ботов
