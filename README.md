# ModernArchiveThumbnail

![Platform](https://img.shields.io/badge/Platform-Windows-blue) ![License](https://img.shields.io/badge/License-MIT-green) ![Language](https://img.shields.io/badge/Language-C%23-purple)

[🇺🇸 English](#-english) | [🇰🇷 한국어](#-korean-한국어)

---

## 🇺🇸 English

**ModernArchiveThumbnail** is a Windows Shell Extension that displays thumbnail previews of images inside archive files (ZIP, RAR, 7Z, etc.) directly in Windows File Explorer.

It creates a seamless browsing experience by providing **3 Performance Modes** and a **Smart Cache Management System** tailored to your workflow.

### ✨ Key Features

* **Wide Format Support**: Generates thumbnails for `zip`, `rar`, `7z`, `cbz`, `cbr`, and more.
* **Smart Cache System**:
    * **Mode Switching (Preserve Cache)**: When switching between *High Speed*, *Optimization*, and *Compatibility* modes, the existing thumbnail cache is **preserved**. This ensures no performance drop during transitions.
    * **Reset Logic (Auto-Clean)**: When switching from **'Disable'** back to an **active mode**, the cache is **automatically cleared**. This ensures a clean slate and fixes potential rendering issues (e.g., black or glitchy thumbnails).
* **User-Friendly Interface**: Simple configuration tool to change modes or troubleshoot instantly.

### 🚀 Modes Explanation

* **⚡ High Speed Mode (Default/Recommended)**: Delivers the fastest thumbnail loading. Best for high-spec PCs or folders with large archives.
* **⚖️ Optimization Mode**: Balances speed and stability. Use this if High Speed mode misses some files.
* **🛡️ Compatibility Mode**: Prioritizes maximum stability over speed to prevent Explorer crashes.
* **❌ Disable Thumbnail**: Turns off the extension. *Enabling the extension again from this state will trigger a cache reset.*

### 🛠 Installation & Troubleshooting

1. Download the latest `Setup.exe` from [Releases](https://github.com/dlxlqkfhd12/ModernArchiveThumbnail/releases).
2. Install and run **ModernArchiveThumbnail Config**.
3. **Troubleshooting**: If thumbnails look broken or black, select **'Disable Thumbnail'** → Apply → select **'High Speed Mode'** → Apply. This forces a clean cache generation.

---

## 🇰🇷 Korean (한국어)

**ModernArchiveThumbnail**은 윈도우 파일 탐색기에서 압축 파일(ZIP, RAR, 7Z 등) 내부의 이미지를 미리보기(썸네일)로 표시해주는 쉘 확장 프로그램입니다.

사용자 환경에 맞춘 **3가지 동작 모드**와, 상황에 따라 캐시를 지능적으로 관리하는 **스마트 캐시 시스템**을 탑재했습니다.

### ✨ 주요 기능

* **다양한 포맷 지원**: `zip`, `rar`, `7z`, `cbz`, `cbr` 등 주요 압축 파일 지원.
* **스마트 캐시 시스템 (Smart Cache System)**:
    * **모드 간 전환 (캐시 유지)**: 고속↔최적화↔호환성 모드 사이를 변경할 때는 기존 캐시를 **유지**합니다. 덕분에 모드를 바꿔도 로딩 속도가 느려지지 않습니다.
    * **재활성화 시 초기화 (캐시 삭제)**: '기능 끄기' 상태에서 다시 모드를 **켤 때**는 캐시를 **자동으로 초기화**합니다. 이를 통해 썸네일이 깨지거나 검게 나오는 문제를 원천적으로 해결하며 깔끔한 이미지를 다시 생성합니다.
* **간편한 설정**: 전용 설정 툴을 통해 클릭 한 번으로 모드 변경 및 관리가 가능합니다.

### 🚀 동작 모드 설명

* **⚡ 고속 모드 (기본값/권장)**: 가장 빠른 속도로 썸네일을 로딩합니다. 최초 설치 시 기본 적용됩니다.
* **⚖️ 최적화 모드**: 속도와 안정성의 균형을 맞춘 모드입니다. 고속 모드에서 일부 이미지가 안 보일 때 사용하세요.
* **🛡️ 호환성 모드**: 안정성을 최우선으로 합니다. 속도는 다소 느릴 수 있으나 탐색기 오류를 최소화합니다.
* **❌ 썸네일 기능 끄기**: 기능을 비활성화합니다. *이 상태에서 다시 다른 모드로 변경 시 캐시가 초기화됩니다.*

### 🛠 설치 및 문제 해결

1. [Releases](https://github.com/dlxlqkfhd12/ModernArchiveThumbnail/releases) 탭에서 최신 `Setup.exe`를 다운로드하여 설치합니다.
2. 바탕화면의 **ModernArchiveThumbnail Config**를 실행하여 모드를 설정합니다.
3. **문제 해결**: 썸네일이 검은색으로 나오거나 꼬였을 경우, 설정에서 **'썸네일 기능 끄기'** 적용 후 다시 **'고속 모드'**를 적용하세요. 꼬인 캐시가 삭제되고 정상적으로 복구됩니다.

---

## 💻 Development Info

* **Language**: C# (.NET Framework / .NET Core)
* **IDE**: Visual Studio 2022
* **Installer**: Inno Setup

---

## 📜 Open Source Libraries

This project uses the following open-source libraries:

* **SharpShell** (MIT License)
* **ImageSharp** (Apache 2.0 License)
* **SharpCompress** (MIT License)

---

## 📜 라이선스 (License)

이 프로젝트는 **MIT 라이선스**에 따라 배포됩니다.
즉, 저작권 표시(LICENSE 파일)만 남기면 누구나 자유롭게 사용, 수정, 재배포할 수 있습니다.
자세한 내용은 `LICENSE` 파일을 참고하세요.

This project is licensed under the **MIT License**.
Feel free to use, modify, and distribute this project as long as you include the original copyright notice.
See the `LICENSE` file for more details.

---

<div align="center">
  Made with ❤️ by dlxlqkfhd12
</div>
