# MyTowerDefenceECS
ECS system architecture demo

description here

---

## 📸 Скриншоты


| Entities editor|
| :--- |
| ![Tool](/bandicam%202026-03-23%2009-09-50-495.jpg) |
| *Entities editor* |
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