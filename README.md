[한국어 설명]
ModernArchiveThumbnail은 윈도우 탐색기에서 압축 파일의 썸네일을 빠르고 정확하게 생성해주는 셸 확장 프로그램입니다. 기존 C# 기반 엔진(v3.0.0)을 완전히 폐기하고, 성능과 안정성을 극대화하기 위해 **Native C++**로 바닥부터 다시 제작되었습니다.

✨ 주요 특징
초고속 네이티브 엔진: C++ 및 Win32 Native API를 사용하여 시스템 자원 소모를 최소화하고 응답 속도를 극대화했습니다.

지능형 스토리지 제어 (New): SSD, HDD, USB, 네트워크 드라이브(NAS) 등 저장장치의 유형을 자동으로 식별합니다. 고속 SSD에서는 최대 성능을 발휘하고, 느린 HDD나 NAS에서는 탐색기가 멈추지(Freezing) 않도록 스캔 속도를 자동으로 조절합니다.

올인원 설치 (C++ Runtime 포함): 별도의 라이브러리 설치 없이 실행 가능한 통합 인스톨러를 제공합니다. 설치 프로그램이 시스템 환경을 분석하여 필요한 런타임을 자동으로 구성합니다.

스마트 표지 선택: 숫자 기반 정렬과 키워드(cover, front 등) 분석을 통해 압축 파일 내 가장 적합한 이미지를 첫 페이지로 선택합니다.

폭넓은 포맷 지원: Zip, RAR, 7z, TAR, LZH, ALZ, EGG 등 현대의 거의 모든 압축 포맷을 지원합니다.

최신 이미지 코덱: WebP, HEIC, AVIF, PNG, JPG 등 최신 이미지 포맷을 WIC(Windows Imaging Component)를 통해 완벽하게 렌더링합니다.

안정적인 설계: 엄격한 메모리 관리와 단일 스레드 최적화를 통해 탐색기 충돌이나 속도 저하를 근본적으로 방지합니다.

🔄 버전 안내 (중요)
본 프로젝트는 **v3.0.0 (C# 버전)**에서 **v1.x (C++ 버전)**로 엔진이 전면 교체되었습니다. 새로운 시작과 성능의 비약적인 도약을 기념하기 위해 버전 체계를 재설정(Reset)하였으며, 현재 v1.x 버전이 이전의 모든 v3.x 버전을 대체하는 최신 권장 버전입니다.

🛠 Installation / 설치 방법
Releases 탭에서 최신 .exe 파일을 다운로드합니다.

관리자 권한으로 설치 프로그램을 실행합니다. (기존 구버전 찌꺼기와 런타임 문제를 자동으로 해결합니다.)

설치 완료 후 탐색기가 자동으로 재시작되며, 즉시 썸네일 기능을 사용할 수 있습니다.

[English Description]
ModernArchiveThumbnail is a powerful Windows Shell Extension that generates fast and accurate thumbnails for archive files directly within File Explorer. The legacy C#-based engine (v3.0.0) has been deprecated and completely rebuilt from the ground up in Native C++ for ultimate performance and stability.

✨ Key Features
High-Performance Native Engine: Built with C++ and Native Win32 APIs for minimal resource usage and near-instant response times.

Adaptive Storage Throttling (New): Automatically detects storage types (SSD, HDD, USB, NAS). It maximizes scanning speed on SSDs while carefully optimizing I/O on HDDs and network drives to completely eliminate File Explorer freezing.

All-in-One Installer (Includes C++ Runtime): No more manual library hunting. The integrated installer automatically detects and configures the necessary C++ Redistributables for your system.

Smart Cover Selection: Uses advanced numerical sorting and keyword analysis (cover, front, etc.) to intelligently select the best cover image from the archive.

Broad Format Support: Seamlessly handles various formats including Zip, RAR, 7z, TAR, LZH, ALZ, EGG, and more.

Modern Image Rendering: Perfectly renders modern image formats such as WebP, HEIC, AVIF, PNG, and JPG using the Windows Imaging Component (WIC).

Rock-Solid Stability: Optimized memory management ensures zero lag and prevents potential crashes.

🔄 Versioning Note
The project has officially migrated from v3.0.0 (C#) to v1.x (C++). To mark this fresh start and the massive performance leap, the versioning has been reset to v1.x. This release is the most advanced and stable version available, superseding all previous v3.x releases.

🛠 Installation
Download the latest .exe from the Releases tab.

Run the installer as Administrator. (It automatically handles legacy cleanup and runtime issues.)

File Explorer will restart automatically, and thumbnails will begin generating instantly.

<img width="1421" height="985" alt="썸네일1" src="https://github.com/user-attachments/assets/c786a411-8c35-41ba-be36-65a933fa44f3" />

<img width="1421" height="985" alt="썸네일2" src="https://github.com/user-attachments/assets/fedcde69-a8a2-4317-9938-b7110f77f55c" />

