# Raid Intel Overlay

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![EFT](https://img.shields.io/badge/EFT-16%2E9-orange)](https://www.escapefromtarkov.com/)
[![SPT](https://img.shields.io/badge/SPT-4.0.13-blue)](https://sp-tarkov.com/)
[![Fika](https://img.shields.io/badge/Fika-2%2E3%2Ex-purple)](https://github.com/project-fika/Fika-Plugin)
[![BepInEx](https://img.shields.io/badge/BepInEx-5%2E4%2Ex-yellow)](https://github.com/BepInEx/BepInEx)
![Deployment](https://img.shields.io/badge/deployment-headless_all-lightgrey)

Клиентский мод для SPT 4 + Fika. Оверлей разведданных рейда с broadcast-синхронизацией в headless-coop.

| | |
|---|---|
| **Разработчик** | [kabzon93region](https://github.com/kabzon93region) |
| **Версия** | 1.2.0 |
| **GitHub** | [RaidIntelOverlay](https://github.com/kabzon93region/RaidIntelOverlay) |
| **Deployment** | `(headless_all)` |
| **Тип** | client |

## Возможности

- Отображение разведданных рейда в реальном времени
- Broadcast данных между хостом и клиентами
- Интеграция с Fika headless

## Установка

1. Скопировать `RaidIntelOverlay.dll` в `BepInEx/plugins/`

## Требования

- **Fika** headless-coop
- **SPT**: 4.0.x
- **BepInEx**: 5.4.x

## Известные проблемы

- Синк на хосте стартует при `IBotGame` до готовности `MainPlayer` — мониторить тайминги на медленных картах

## Совместимость

- `headless_all` — и на хосте, и на клиентах

## Поддержать проект

Разовый донат картой РФ, СБП, ЮMoney, VK Pay:
**[DonationAlerts → kabzon93region](https://www.donationalerts.com/r/kabzon93region)**
