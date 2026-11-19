# ⚡ K-Tacho Studio

[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-blue.svg)]()
[![Target](https://img.shields.io/badge/Target-VAG%20EDC15%20%2F%20EDC16%20%7C%20K--Line-red.svg)]()

> **The comprehensive, open-source engineering workstation for VAG EDC15 & EDC16 vehicles.**  
> Effortlessly manage ECU flashing, cluster EEPROMs, immobilizer adaptations, and low-level K-Line diagnostics.

---

## 🎯 Vision

For years, owners, mechanics, and tuners working on legendary VAG platforms (VW Golf Mk4 / Bora, Passat B5 / B5.5, Audi A3/A4/A6/TT, Seat Leon, Skoda Octavia) have relied on fragmented, dated Windows XP-era tools or command-line scripts to read EEPROMs, retrieve SKC/PIN codes, adapt clusters, or flash ECUs.

**K-Tacho Studio** is built to unite all these critical operations into a single, modern, reliable, and user-friendly workstation. Built upon the robust protocol foundations of [`kw1281test`](https://github.com/gmenounos/kw1281test) by Greg Menounos, K-Tacho Studio expands the engine into a complete diagnostic, tuning, and bench workstation tailored specifically for the **EDC15** and **EDC16** ecosystems.

---

## 🚀 Key Modules & Capabilities

### 1. ⚙️ EDC15 & EDC16 Engine Workstation
* **Flash Reading & Writing**:
  * High-speed K-Line dump of 512 KB (EDC15) and 2 MB (EDC16) flash images.
  * Multi-speed link negotiation (**low**, **medium**, **high** baud).
  * Safe sector-by-sector verification and checksum-aware writing.
  * **C167 Hardware Boot Mode**: Direct recovery flashing for bricked or unresponsive EDC15 ECUs.
  * **ISO 14230 Fast Init**: Cold-init wake-up support for late CAN-init EDC16 ECUs.
* **ECU EEPROM Management**:
  * Read, dump, and write ECU EEPROM (95P08 / 24C04 / 95160 / 95320).
  * Direct access for immo status, login code, and odometer synchronization.

### 2. 🛡️ Cluster & Immobilizer Engineering (Address 17)
* **Automated Cluster Identification & Extraction**:
  * Auto-detects cluster variant and memory layout on connection.
  * **VDO**: Full EEPROM read/write, high-bank ROM dumps for FIS clusters (µPD784038 NEC displays).
  * **Magneti Marelli**: Flash & EEPROM extraction for M73 and MC9S12H256 architectures.
  * **Bosch RB4 / RB8**: Crypto-mode detection, memory dumping, and mode toggling.
* **Instant Immobilizer Services**:
  * Retrieve 4/5-digit **Secret Key Codes (SKC / PIN)** in seconds.
  * Immobilizer 2 and Immobilizer 3 adaptation assistance (Channel 50 / Channel 00 workflows).
  * Mileage / odometer backup and recalibration verification.

### 3. 🎛️ Comfort Control & Auxiliary Modules
* **Comfort Control Module (CCM - Address 46)**:
  * Complete EEPROM reading and writing.
  * Easily toggle features such as remote window roll-up/down, selective unlocking, and alarm chirps.
* **Airbag Controllers (Address 15)**:
  * Full EEPROM read/write capabilities.
  * Fault code analysis and soft crash data clearing on supported modules.
* **Radio Code Extraction (Address 56)**:
  * Direct extraction of SAFE codes for Delco VW Premium 5 and Clarion VW Premium 4 head units.

### 4. 🔍 Live Diagnostics & Testing
* **Fault Codes**: Read and clear Diagnostic Trouble Codes (DTCs) across all supported modules.
* **Measuring Blocks & Basic Settings**: Group-based live data monitoring (Group 0–255).
* **Actuator Tests**: Cycle fuel pump relays, instrument cluster needles, warning buzzers, and solenoids.

---

## 🔌 Hardware Compatibility

K-Tacho Studio is designed to work with cost-effective, readily available diagnostic hardware:

| Interface Type | Supported Hardware | Notes |
| :--- | :--- | :--- |
| **USB KKL Cable** | FTDI FT232RL, CH340, CP2102 | Standard blue OBD2 cables |
| **FTDI High-Speed** | Genuine FTDI FT232R / D2XX | Recommended for high-speed EDC15/16 flashing |
| **Ross-Tech Legacy** | HEX-USB, KII-USB | Supported via Ross-Tech Virtual COM Port (VCP) drivers |
| **Serial / RS-232** | Standard PC COM ports | Supported for classic bench setups |

---

## 🧭 Architecture & Design

K-Tacho Studio is structured as an open, decoupled ecosystem:

* **Core Engine**: A standalone, cross-platform protocol library communicating over serial/FTDI handling KW1281, KWP2000, and hardware boot protocols.
* **Studio Client**: A desktop interface built for cross-platform execution (Windows, Linux, macOS) featuring:
  * Real-time K-Line packet visualizer & raw hex inspector.
  * One-click action wizards with automatic safety checks (battery voltage warnings, backup prompts).
  * Integrated file managers for EEPROMs, flash bins, and logs.

---

## ⚠️ Safety Disclaimer

> **PLEASE NOTE**: Interfacing with automotive ECUs and instrument clusters carries inherent risk. Writing corrupted data, interrupting power during flash sequences, or modifying sensitive EEPROM bytes can render control units non-operational (bricked).
>
> * Always connect a stable **battery maintainer / 12V bench power supply** before reading or writing.
> * Always **backup your original EEPROM and Flash** before performing any write operations.
> * This software is provided "as is", without warranty of any kind.

---

## 🤝 Credits & Acknowledgements

K-Tacho Studio is built on the shoulders of giants in the open-source automotive reverse engineering community:

* **[Greg Menounos](https://github.com/gmenounos)** for the outstanding work on [kw1281test](https://github.com/gmenounos/kw1281test), which powers the core communication logic.
* **[Blafusel.de](https://www.blafusel.de/obd/obd2_kw1281.html)** for foundational KW1281 protocol documentation.
* **[fjvva/ecu-tool](https://github.com/fjvva/ecu-tool)** for EDC15 flashing mechanics and seed/key algorithm research.
* **[mnaberez/vwradio](https://github.com/mnaberez/vwradio)** for Delco/Clarion radio reverse engineering.
* The open-source community contributors to VAG K-Line diagnostic tools.

---

## 📄 License

This project is licensed under the **[MIT License](LICENSE)** — free and open for personal, educational, and commercial use.
