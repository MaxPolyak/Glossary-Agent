# Glossary Extraction Agent

Сервис, который читает неразмеченный текст, находит в нём определения терминов и расшифровки сокращений и пополняет ими глоссарий (без дублей).

**Стек:** C# / .NET 9, Microsoft Agent Framework, EF Core + SQLite, LM Studio (любой OpenAI-совместимый сервер).

## Что нужно установить
1. [.NET 9 SDK](https://dotnet.microsoft.com/download).
2. [LM Studio](https://lmstudio.ai/).

## Шаг 1. Подготовить модель в LM Studio
1. В LM Studio откройте поиск моделей и скачайте instruct-модель, например Qwen2.5-7B-Instruct или Qwen3-8B.
2. Загрузите модель.
3. Запустите сервер:
   - в левой вертикальной панели LM Studio найдите значок **Developer** (иконка консоли `>_`; остальные значки: Chat, My Models, Discover);
   - откройте его и в левом верхнем углу переключите тумблер **Status** из положения *Stopped* в *Running* (в некоторых версиях он подписан *Start server*);
   - адрес по умолчанию: `http://localhost:1234`;
   - если значка Developer нет - включите режим разработчика: **Settings** (шестерёнка) → переключатель **Developer Mode**, либо переключатель режима интерфейса внизу окна (User / Power User / Developer) поставьте в Developer;
   - проверка: откройте в браузере `http://localhost:1234/v1/models` - должен показаться JSON со списком моделей (там же виден идентификатор модели для шага 4).
4. Скопируйте **идентификатор модели** (показан в LM Studio, например `qwen2.5-7b-instruct`).
5. В настройках загрузки модели поставьте контекст не меньше 8192 токенов.

## Шаг 2. Настроить проект
Откройте `appsettings.json` и впишите модель:

```json
"Llm": { "BaseUrl": "http://localhost:1234/v1", "Model": "qwen2.5-7b-instruct", "ApiKey": "lm-studio", "MaxChunkChars": 6000 }
```

Либо через переменную окружения (PowerShell): `$env:Llm__Model = "qwen2.5-7b-instruct"`.

## Шаг 3. Запустить
```bash
dotnet restore
dotnet run
```
Сервис слушает `http://localhost:5080`. При первом старте создаётся файл БД `glossary.db`.

## Шаг 4. Проверить
```bash
curl -X POST http://localhost:5080/api/extract -H "Content-Type: application/json" \
  -d "{\"text\": \"Под СЭД далее понимается система электронного документооборота.\"}"
# -> 200 OK

curl http://localhost:5080/api/glossary
# -> [{"name":"СЭД","definition":"система электронного документооборота"}]
```
Готовый набор проверочных текстов: папка `tests/` (`EXPECTED.md` - что ожидать, `run-samples.ps1` / `run-samples.sh` - прогон всех текстов, `requests.http` - запросы для VS Code / Rider / Visual Studio).

## API
`POST /api/extract`, тело: `{"text": "..."}`
- `200 OK` - текст обработан;
- `500` + текст ошибки в теле - любой сбой (пустой text, модель недоступна, ошибка БД).

Дополнительно (не из ТЗ): `GET /api/glossary` - показать содержимое глоссария.

## Сброс данных
Остановите сервис и удалите `glossary.db`.

## Если что-то не работает
| Симптом | Причина / решение |
|---|---|
| `500`, «Не задана модель…» | Пустой `Llm:Model` - впишите идентификатор из LM Studio |
| `500`, «Connection refused» / «No connection» | Не запущен сервер LM Studio (шаг 1, пункт 3) или другой порт в `BaseUrl` |
| `500`, ошибка про `tools` / `function calling` | Модель не поддерживает инструменты - возьмите другую |
| `200`, но глоссарий пуст | Смотрите лог в консоли: строки `[tool] ...` показывают, вызывала ли модель инструменты. Если нет - модель слишком слабая или инструкция требует правки |
| Ошибка сборки про версии пакетов | Выполните `dotnet add package Microsoft.Agents.AI.OpenAI --prerelease` и `dotnet restore` |
