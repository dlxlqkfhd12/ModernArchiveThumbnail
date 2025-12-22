Modern Archive Thumbnail Handler (v3.0.0)
[English] A high-performance Windows Shell Extension that generates thumbnails for various archive files. v3.0.0 introduces a Deep Scan Engine that explores nested folders and optimized memory management for extreme stability.

[한국어] 윈도우 탐색기에서 압축 파일의 썸네일을 고속으로 보여주는 쉘 확장 프로그램입니다. v3.0.0에서는 하위 폴더 탐색(Deep Scan) 기능과 메모리 최적화 로직이 도입되어 대용량 및 복잡한 압축 파일에서도 완벽한 성능을 발휘합니다.

🚀 Key Features / 주요 기능
🇺🇸 English
Deep Scan (NNN): Automatically explores subdirectories within archives to find the best representative image (Cover, Front, 001).

Index-Based Engine: Transitioned to IArchive indexing, allowing instant access to images regardless of their position in the file.

Hybrid Decoding: Dual-engine strategy using GDI+ for speed and WPF (WIC) for modern/large formats (WebP, AVIF, HEIC).

Smart Memory Management: Automatically releases large memory buffers (>8MB) immediately after processing to keep Windows Explorer lightweight.

Expanded Format Support: Added official support for .alz and .egg (popular Korean formats) alongside .zip, .rar, .7z, .cbz, and .cbr.

🇰🇷 한국어
딥 스캔 (NNN): 압축 파일 내부의 복잡한 하위 폴더를 추적하여 최적의 표지 이미지(Cover, Front, 001 등)를 찾아냅니다.

색인 기반 엔진: IArchive 인덱싱 방식을 채택하여, 파일 위치와 상관없이 대용량 압축 파일에서도 즉시 썸네일을 추출합니다.

하이브리드 디코딩: 속도를 위한 **GDI+**와 안정성을 위한 WPF(WIC) 엔진을 결합하여 WebP, AVIF, HEIC 등 최신 포맷을 완벽하게 지원합니다.

지능형 메모리 관리: 8MB 이상의 버퍼 사용 시 작업 직후 즉시 메모리를 회수하여 탐색기의 점유율을 최소화합니다.

확장자 지원 확대: 기존 포맷 외에 국내 사용자가 많은 .alz, .egg 확장자에 대한 지원을 공식 추가했습니다.

📥 Installation / 설치 방법
Go to the Releases page.

Download ModernArchiveThumbnail_v3.0.0_Setup.exe.

Run the installer (Administrator privileges required).

The installer will automatically register the server and clear the thumbnail cache.

Note: If thumbnails do not appear, use the included Settings app to "Clear Thumbnail Cache".

⚖️ License & Credits / 라이선스 및 저작권
Copyright
Modern Archive Thumbnail Handler Copyright (c) 2025

This software is provided 'as-is', without any express or implied warranty.

Open Source Libraries
SharpShell (MIT License) - Link

SharpCompress (MIT License) - Link

Technical Note
v3.0.0 has been optimized with BmpBitmapEncoder for faster processing and a strategic AssemblyResolve logic to ensure stable library loading in any environment.

📒 Developer's Note (v3.0.0)
"The transition from v2.0.0 to v3.0.0 is a complete rewrite of the scanning logic. We moved from a simple sequential reader to a sophisticated indexing system that handles nested folders (NNN). This version is built to be the most stable and performant thumbnail handler for Windows 10/11."
