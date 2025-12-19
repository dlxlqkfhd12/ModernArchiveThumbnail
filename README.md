# Modern Archive Thumbnail Handler (v2.0.0)

![Platform](https://img.shields.io/badge/Platform-Windows-blue) ![License](https://img.shields.io/badge/License-MIT-green) ![Version](https://img.shields.io/badge/Version-2.0.0-orange)

**[English]** A high-performance Windows Shell Extension that generates thumbnails for archive files (CBZ, ZIP, RAR, 7Z).  
Built with C# and optimized with a new streaming engine (IReader) for instant loading.

**[한국어]** 윈도우 탐색기에서 압축 파일(CBZ, ZIP, RAR, 7Z)의 썸네일을 고속으로 보여주는 쉘 확장 프로그램입니다.  
C#으로 개발되었으며, 새로운 스트리밍 엔진(IReader)을 탑재하여 대용량 파일도 즉시 로딩됩니다.

---

## 🚀 Key Features / 주요 기능

### 🇺🇸 English
* **Instant Loading:** Uses `IReader` streaming technology to extract the first image without scanning the entire archive directory.
* **Modern Format Support:** Supports **WebP, AVIF, HEIC** thumbnails inside archives (via WIC).
* **Performance:** Pre-compiled with NGEN for zero startup latency.
* **Stability:** Includes a strict timeout (2000ms) and "Magic Number" validation to prevent Windows Explorer crashes.
* **Supported Formats:** `.cbz`, `.zip`, `.rar`, `.7z`, `.cbr` (and more via SharpCompress).

### 🇰🇷 한국어
* **초고속 로딩:** 전체 압축 목록을 읽지 않고, 스트리밍 방식(`IReader`)으로 첫 번째 이미지만 즉시 추출합니다.
* **최신 포맷 지원:** 압축 파일 내부의 **WebP, AVIF, HEIC** 이미지도 썸네일로 표시합니다.
* **성능 최적화:** NGEN(네이티브 이미지) 설치 방식을 적용하여 초기 구동 딜레이가 없습니다.
* **안정성:** 타임아웃(2초) 및 매직 넘버 검증 로직이 적용되어, 파일 오류 시에도 탐색기가 멈추지 않습니다.
* **지원 확장자:** `.cbz`, `.zip`, `.rar`, `.7z`, `.cbr` 등.

---

## 📥 Installation / 설치 방법

1.  Go to the **[Releases](https://github.com/dlxlqkfhd12/ModernArchiveThumbnail/releases)** page.
2.  Download **`ModernArchiveThumbnail_Setup.exe`**.
3.  Run the installer (Administrator privileges required).
4.  The thumbnail cache will be automatically cleared. Enjoy!

> **Note:** If thumbnails do not appear immediately, use the included **Settings** app to "Clear Thumbnail Cache".

---

## ⚖️ License & Credits / 라이선스 및 저작권

### Copyright
**Modern Archive Thumbnail Handler** Copyright (c) 2025

This software is provided 'as-is', without any express or implied warranty.  
In no event will the authors be held liable for any damages arising from the use of this software.

---

### Open Source Libraries
This software uses the following open source libraries.  
본 소프트웨어는 다음의 오픈 소스 라이브러리를 사용합니다.

#### 1. SharpShell
* **License:** MIT License
* **Author:** Dave Kerr
* **Source:** [https://github.com/dwmkerr/sharpshell](https://github.com/dwmkerr/sharpshell)

#### 2. SharpCompress
* **License:** MIT License
* **Author:** Adam Hathcock
* **Source:** [https://github.com/adamhathcock/sharpcompress](https://github.com/adamhathcock/sharpcompress)

---

### Technical Note
Support for modern image formats (AVIF, HEIC, WebP) is natively provided via **Microsoft Windows Imaging Component (WIC)** and .NET Framework.  
(ImageSharp library has been removed in v2.0.0 for better performance and native compatibility.)
