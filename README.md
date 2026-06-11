# StoneHearth 🪨🔥


*A punishing 3D Souls-like Action RPG where you play as an ancient Golem Protector.*

**StoneHearth** is a challenging, atmospheric action RPG. You awaken as a powerful golem created by a Celestial Entity to protect a peaceful village. After centuries of slumber, you rise to face a demon invasion that has devastated your world. Wield the dual powers of Earth-bending and Celestial magic in intense, strategic combat. Every death is a lesson — master your abilities, evolve through souls, and reclaim your home from the demonic forces.

*Note: This repository serves as a technical portfolio piece showcasing core gameplay engineering, decoupled systems architecture, and RPG progression loops. The game is an unfinished academic prototype.*

---

## 🛠️ Tech Stack & Systems
* **Engine:** Unity (URP - Universal Render Pipeline)[cite: 1]
* **Language:** C#[cite: 3]
* **Input System:** Unity Legacy Input API & Input System Package infrastructure[cite: 1, 2, 3]
* **AI & Pathfinding:** Unity AI Navigation (NavMesh runtime infrastructure configured)[cite: 1]

---

## 🌟 Core Architecture & Technical Highlights

### 1. Decoupled Combat System (C# Reflection)
To maximize code scalability and avoid tight coupling between projectiles and various enemy AI classes, magic projectiles (`MagicPebble.cs`, `CrystalProjectile.cs`) utilize **C# Reflection** to dynamically invoke damage logic[cite: 4, 5]:
* Projectiles scan overlapping colliders or triggers for *any* script component containing a `TakeDamage(float)` method[cite: 4, 5].
* If found, the method is dynamically invoked at runtime[cite: 4, 5]. This allows new enemy or destructible types to be introduced without ever modifying projectile scripts[cite: 4, 5].

### 2. Hybrid Ability & Physics Framework
The Golem features a diverse kit of modular combat abilities managed via individual component scripts and cooldown timers[cite: 3]:
* **Iron Body:** A defensive utility that grants temporary invincibility frames handled via asynchronous C# Coroutines[cite: 3].
* **Earth Gauntlet & Earth Shatter:** Area-of-effect dynamic melee attacks driven by trigger-volume interaction loops[cite: 3, 6, 7].
* **Magic Pebble & Crystal Shotgun:** Physics-based projectiles utilizing velocity vector forces and rigid-body collision handling[cite: 3, 4, 5].

### 3. Progressive RPG Stat Scaling
Features an exponential RPG economy loop managed through an interactive totem system[cite: 3]. When the player accumulates "Souls" dropped by enemies, they can upgrade core character modules (HP, Earth Magic, Celestial Magic)[cite: 3].
* **Dynamic Pricing Formula:** Upgrade costs scale globally using an aggressive progression curve[cite: 3]:
  $$\text{globalBaseCost} = (\text{globalBaseCost} + 200) \times 1.3$$
* **Hybrid Class Unlocks:** Reaching specific cross-disciplinary milestones (e.g., Level 2 in both Earth and Magic) automatically unlocks advanced hybrid skills like the `CrystalShotgun`[cite: 3].

### 4. Punishing RPG Game Loop
Implements standard high-stakes Souls-like loops including permanent currency loss upon death (`soulCount = 0` on respawn), checkpoint tracking, and automatic dynamic UI opacity updates corresponding to skill cooldown durations[cite: 3].

---

## 👿 Taurus Demon (Boss) Mechanics
The game features a multi-phase boss encounter with custom tracking behavior:
* **Ground Slam:** A high-impact vertical leap dealing heavy AoE damage upon landing.
* **Horizontal Slash (AirSlash):** Launches a slicing projectile hazard traveling toward the player's position.
* **Summon Enemies:** Dynamic tactical reinforcement loop spawning 1–3 minions to aid in battle.

---

## 🎮 Controls
* **WASD:** Move[cite: 3]
* **Mouse:** Camera Orbit + Targeting[cite: 1, 2]
* **E:** Interact with Totem (Opens Leveling Canvas)
* **Spacebar:** Iron Body (Shield)[cite: 3]
* **Alpha 1:** Earth Gauntlet[cite: 3]
* **Alpha 2:** Magic Pebble[cite: 3]
* **Alpha 3:** Earth Shatter *(Requires Earth Level 3)*[cite: 3]
* **Alpha 4:** Crystal Shotgun *(Requires Earth Level 2 & Magic Level 2)*[cite: 3]
* **Y:** Debug Damage Test[cite: 3]

---

## 🏃‍♂️ Installation & How to Run

### Prerequisites
* **Unity Hub** installed.
* **Unity Editor** (Unity 2022.3+ LTS recommended, URP compatibility required)[cite: 1].

### Steps to Open
1. **Clone the Repository:**
```bash
   git clone [https://github.com/yourusername/StoneHearth.git](https://github.com/yourusername/StoneHearth.git)
```
2. **Open in Unity Hub:**

* Launch Unity Hub.

* Click Add > Add project from disk.

* Select the root StoneHearth folder.

3. **Editor Setup:**

* Allow Unity to fully regenerate the local Library/ cache and dependency folders (this may take a couple of minutes on first boot).

* In the Project window, navigate to Assets/Scenes/ and open the main demo scene file.

* Play: Press the Play button at the top of the Unity Editor.

## 🗺️ Project Roadmap
* Polish & Visual Effects: Completed June 30, 2025

* Vertical Slice Level: Full demo level with Golem + 1 Boss + Totem functional integration

* Early Access Launch: May 14, 2026 (Steam)

## 📌 References & Inspiration
Dark Souls I (World design & environmental challenge parameters)

Classic Souls-like action combat loops

## 📄 License
This project is private during active development. All rights reserved by Ashen Forge.

Made with passion and struggle.
