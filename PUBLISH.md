# Publish to GitHub — Raid Intel Overlay

**Статус:** `ready`  
**GitHub:** Release + zip  
**Версия:** `1.2.0`  
**Deployment:** `(headless_all)`

## 1. Подготовка (уже сделано этим скриптом)

Папка: `github-repos/RaidIntelOverlay/`

## 2. Создать репозиторий и запушить

```powershell
cd github-repos/RaidIntelOverlay
git init
git add .
git commit -m "Source backup Raid Intel Overlay v1.2.0"
git branch -M main
git remote add origin https://github.com/kabzon93region/RaidIntelOverlay.git
git push -u origin main
```

Или автоматически:

```powershell
python CURSORAIMODING/tools/publish/publish_github_release.py RaidIntelOverlay --create-repo
```

## 3. GitHub Release

Прикрепить zip (только игровые файлы, без INSTALL.md):

`\\Servant\data\Games\EscapeFromTarkov4\CURSORAIMODING\releases\RaidIntelOverlay_(headless_all)_v1.2.0_2026-06-27.zip`

```powershell
gh release create v1.2.0 "\\Servant\data\Games\EscapeFromTarkov4\CURSORAIMODING\releases\RaidIntelOverlay_(headless_all)_v1.2.0_2026-06-27.zip" ^
  --title "Raid Intel Overlay v1.2.0" ^
  --notes-file CHANGELOG.md
```

## Описание репозитория (suggested)

Оверлей разведданных рейда + broadcast в headless-coop.

SPT 4.0 + Fika 2.3 headless stack. Deployment: `(headless_all)`.
