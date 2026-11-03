# Timer — spec

Спецификация этой фичи: [docs/prd.md](../../docs/prd.md). Термины: [GLOSSARY.md](../../GLOSSARY.md). Решения: [docs/adr/](../../docs/adr/).

## Тикеты

| № | Тикет | Зависит от |
|---|---|---|
| 01 | [Start and stop the Timer with a persisted Daily Session](issues/01-start-stop-persisted-daily-session.md) | — |
| 02 | [Run from the tray as a single instance](issues/02-tray-single-instance.md) | 01 |
| 03 | [Split the Daily Session at local midnight](issues/03-midnight-split.md) | 01 |
| 04 | [Add and remove minutes with an Adjustment](issues/04-adjustment.md) | 01 |
| 05 | [View the Timer Log by date](issues/05-timer-log-screen.md) | 04 |
| 06 | [Detect inactivity and enter Idle](issues/06-idle-detection.md) | 01 |
| 07 | [Stop counting in Lock and resume on unlock](issues/07-lock.md) | 06 |
| 08 | [Record Shutdown on Windows session end](issues/08-shutdown.md) | 02, 07 |
| 09 | [Add a Recovered Shutdown after an abnormal exit](issues/09-recovered-shutdown.md) | 08 |
| 10 | [Package a self-contained zip and document it](issues/10-packaging-readme.md) | 09 |
