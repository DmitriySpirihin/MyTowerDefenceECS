# MyTowerDefenceECS
ECS system architecture demo

description here

---

## 📸 Скриншоты


| Главное меню | Бой | Победа | Log | Инфо |
| :--- | :--- | :--- | :--- | :--- |
| ![Главное меню](/Main%20menu.jpg) | ![Бой](/Battle.jpg) | ![Победа](/Final%20panel.jpg) | ![Победа](/Battle%20log.jpg) | ![7 batches](/info.jpg) |
| *Выбор формаций и предпросмотр* | *Ближний бой 20 на 20* | *Статистика* | *Log* | *Оптимизация* |

---

## 🏗 Архитектурный стек (Advanced Patterns)

Проект построен на принципах **Separation of Concerns** и слабой связанности систем:

*   **Dependency Injection ([Zenject](https://github.com)):** 

---

## ⚡ Оптимизация и ресурсы

Проект ориентирован на **60 FPS** на мобильных устройствах:

1.  **MaterialPropertyBlock:** Изменение цвета юнитов без потери GPU-батчинга + custom shader для URP использую `#pragma multi_compile_instancing`. Все 40 юнитов рендерятся в 2 Draw Call.


---

## ✨ Дополнительная функция: Система формаций



---

## 🚀 Как запустить

1

---

## ➕ Как добавить новых entities 

1.  
## 📱 Платформа и требования

*   **Целевая платформа:** Android / iOS
*   **Разрешение:** 1920×1080 (Альбомная)
*   **Render Pipeline:** URP (Universal Render Pipeline)

**Автор:** [Дмитрий Спирихин](https://github.com)