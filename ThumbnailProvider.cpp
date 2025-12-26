#include <windows.h>
#include <wincodec.h>
#include <shlwapi.h>
#include <shlobj.h>
#include <thumbcache.h>
#include <archive.h>
#include <archive_entry.h>
#include <string>
#include <vector>
#include <algorithm>
#include <new>
#include <stdio.h>
#include "ThumbnailProvider.h"

#pragma comment(lib, "windowscodecs.lib")
#pragma comment(lib, "shlwapi.lib")
#pragma comment(lib, "gdi32.lib")
#pragma comment(lib, "user32.lib")

using namespace std;

const double MIN_ASPECT_RATIO = 0.15;
const double MAX_ASPECT_RATIO = 4.5;
const long long MAX_FILE_SIZE = 40 * 1024 * 1024;
const int SCAN_TIMEOUT_MS = 3000;
const int MAX_SCAN_FILES = 200;

static const char* IMAGE_EXTENSIONS[] = {
    ".jpg", ".jpeg", ".png", ".webp", ".jfif",
    ".gif", ".bmp", ".heic", ".heif", ".avif",
    ".tiff", ".tif", ".ico", ".jpe"
};

template <typename T>
class ThumbnailRawPtr {
    T* ptr = nullptr;
public:
    ThumbnailRawPtr() {}
    ThumbnailRawPtr(T* p) : ptr(p) {}
    ~ThumbnailRawPtr() { if (ptr) ptr->Release(); }
    ThumbnailRawPtr(ThumbnailRawPtr&& o) noexcept : ptr(o.ptr) { o.ptr = nullptr; }
    ThumbnailRawPtr& operator=(ThumbnailRawPtr&& o) noexcept {
        if (this != &o) { if (ptr) ptr->Release(); ptr = o.ptr; o.ptr = nullptr; }
        return *this;
    }
    T* operator->() const { return ptr; }
    T* Get() const { return ptr; }
    T** operator&() { if (ptr) { ptr->Release(); ptr = nullptr; } return &ptr; }
    operator bool() const { return ptr != nullptr; }
};

static INIT_ONCE g_WicOnce = INIT_ONCE_STATIC_INIT;
static IWICImagingFactory* g_pWICFactory = nullptr;
BOOL CALLBACK InitWicFactory(PINIT_ONCE, PVOID, PVOID*) {
    return SUCCEEDED(CoCreateInstance(CLSID_WICImagingFactory, NULL, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&g_pWICFactory)));
}
IWICImagingFactory* GetWIC() {
    if (!InitOnceExecuteOnce(&g_WicOnce, InitWicFactory, NULL, NULL)) return nullptr;
    return g_pWICFactory;
}

bool IsImageFile(const string& path) {
    auto ends = [&](const string& t, const string& e) { return t.size() >= e.size() && t.compare(t.size() - e.size(), e.size(), e) == 0; };
    for (const char* ext : IMAGE_EXTENSIONS) { if (ends(path, ext)) return true; }
    return false;
}

int GetFirstNumberFromUtf8(const string& fn) {
    int num = 0; bool found = false;
    for (char c : fn) {
        if (c >= '0' && c <= '9') { num = num * 10 + (c - '0'); found = true; if (num > 999999) break; }
        else if (found) break;
    }
    return found ? num : 1000000;
}

int GetFilePriority(const string& fn) {
    if (fn.find("cover") != string::npos) return 1;
    if (fn.find("front") != string::npos) return 2;
    if (fn.find("index") != string::npos) return 3;
    if (fn.find("folder") != string::npos) return 4;
    return 100;
}

int GetDepth(const string& path) {
    int d = 0; for (char c : path) if (c == '/' || c == '\\') d++; return d;
}

HRESULT CreateHBITMAPFromData(const vector<BYTE>& buf, UINT cx, HBITMAP* phbmp) {
    if (buf.empty() || !GetWIC()) return E_FAIL;
    ThumbnailRawPtr<IStream> spStream(SHCreateMemStream(buf.data(), (UINT)buf.size()));
    if (!spStream) return E_OUTOFMEMORY;
    ThumbnailRawPtr<IWICBitmapDecoder> spDec;
    if (FAILED(GetWIC()->CreateDecoderFromStream(spStream.Get(), NULL, WICDecodeMetadataCacheOnLoad, &spDec))) return E_FAIL;
    ThumbnailRawPtr<IWICBitmapFrameDecode> spFrame;
    if (FAILED(spDec->GetFrame(0, &spFrame)) || !spFrame) return E_FAIL;
    UINT w, h; spFrame->GetSize(&w, &h);
    if (w == 0 || h == 0) return E_FAIL;
    double r = (double)w / h;
    if (r < MIN_ASPECT_RATIO || r > MAX_ASPECT_RATIO) return S_FALSE;
    UINT tw = cx, th = max((UINT)((double)h * cx / w), 1U);
    ThumbnailRawPtr<IWICBitmapScaler> spScaler;
    if (FAILED(GetWIC()->CreateBitmapScaler(&spScaler)) || !spScaler) return E_FAIL;
    spScaler->Initialize(spFrame.Get(), tw, th, WICBitmapInterpolationModeFant);
    ThumbnailRawPtr<IWICFormatConverter> spConv;
    if (FAILED(GetWIC()->CreateFormatConverter(&spConv)) || !spConv) return E_FAIL;
    spConv->Initialize(spScaler.Get(), GUID_WICPixelFormat32bppBGRA, WICBitmapDitherTypeNone, NULL, 0.0, WICBitmapPaletteTypeCustom);
    BITMAPINFO bmi = { 0 };
    bmi.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
    bmi.bmiHeader.biWidth = tw; bmi.bmiHeader.biHeight = -(long)th;
    bmi.bmiHeader.biPlanes = 1; bmi.bmiHeader.biBitCount = 32;
    bmi.bmiHeader.biCompression = BI_RGB;
    void* dstPtr = nullptr;
    *phbmp = CreateDIBSection(NULL, &bmi, DIB_RGB_COLORS, &dstPtr, NULL, 0);
    if (*phbmp && dstPtr) {
        UINT stride = tw * 4;
        if (FAILED(spConv->CopyPixels(NULL, stride, stride * th, (BYTE*)dstPtr))) {
            DeleteObject(*phbmp); *phbmp = NULL; return E_FAIL;
        }
    }
    return *phbmp ? S_OK : E_FAIL;
}

struct StreamContext { IStream* pStream; BYTE buffer[65536]; };
la_ssize_t StreamRead(struct archive* a, void* cd, const void** b) {
    StreamContext* ctx = (StreamContext*)cd; ULONG r = 0;
    if (FAILED(ctx->pStream->Read(ctx->buffer, sizeof(ctx->buffer), &r))) return -1;
    *b = ctx->buffer; return (la_ssize_t)r;
}
la_int64_t StreamSeek(struct archive* a, void* cd, la_int64_t req, int w) {
    StreamContext* ctx = (StreamContext*)cd; LARGE_INTEGER li; li.QuadPart = req; ULARGE_INTEGER np;
    DWORD m = (w == SEEK_SET) ? 0 : (w == SEEK_CUR ? 1 : 2);
    ctx->pStream->Seek(li, m, &np); return (la_int64_t)np.QuadPart;
}

STDMETHODIMP CThumbnailProvider::Initialize(IStream* pStream, DWORD grfMode) {
    m_spStream = pStream; return S_OK;
}

HRESULT CThumbnailProvider::GetThumbnailImpl(UINT cx, HBITMAP* phbmp, WTS_ALPHATYPE* pdwAlpha) {
    struct archive* a = archive_read_new();
    if (!a) return E_FAIL;
    archive_read_support_format_all(a);
    archive_read_support_filter_all(a);
    archive_read_set_seek_callback(a, StreamSeek);
    StreamContext ctx = { m_spStream.Get() };
    LARGE_INTEGER liZero = { 0 }; m_spStream->Seek(liZero, 0, NULL);
    if (archive_read_open(a, &ctx, NULL, StreamRead, NULL) != ARCHIVE_OK) { archive_read_free(a); return E_FAIL; }
    struct archive_entry* entry;
    ULONGLONG start = GetTickCount64();
    int scanCount = 0;
    vector<BYTE> bestData;
    int minD = 1000, bestP = 1001, minN = 1000001;
    long long maxSz = -1;
    while (archive_read_next_header(a, &entry) == ARCHIVE_OK) {
        if (GetTickCount64() - start > SCAN_TIMEOUT_MS || ++scanCount > MAX_SCAN_FILES) break;
        if (archive_entry_filetype(entry) != AE_IFREG) { archive_read_data_skip(a); continue; }
        const char* p = archive_entry_pathname(entry);
        if (!p) { archive_read_data_skip(a); continue; }
        string s = p;
        transform(s.begin(), s.end(), s.begin(), ::tolower);
        if (!IsImageFile(s)) { archive_read_data_skip(a); continue; }
        long long sz = archive_entry_size(entry);
        if (sz < 1024 || sz > MAX_FILE_SIZE) { archive_read_data_skip(a); continue; }
        string fn = s.substr(s.find_last_of("/\\") + 1);
        int d = GetDepth(s), pr = GetFilePriority(fn), n = GetFirstNumberFromUtf8(fn);
        bool better = false;
        if (d < minD) better = true;
        else if (d == minD) {
            if (pr < bestP) better = true;
            else if (pr == bestP) {
                if (n < minN) better = true;
                else if (n == minN && sz > maxSz) better = true;
            }
        }
        if (better) {
            try {
                vector<BYTE> tmp;
                tmp.reserve((size_t)sz);
                tmp.resize((size_t)sz);
                if (archive_read_data(a, tmp.data(), (size_t)sz) == (la_ssize_t)sz) {
                    if (d == 0 && pr == 1) {
                        HRESULT hrTest = CreateHBITMAPFromData(tmp, cx, phbmp);
                        if (SUCCEEDED(hrTest)) { archive_read_free(a); return hrTest; }
                    }
                    bestData = move(tmp); minD = d; bestP = pr; minN = n; maxSz = sz;
                }
            }
            catch (const bad_alloc&) { archive_read_data_skip(a); continue; }
        }
        else { archive_read_data_skip(a); }
    }

#ifdef _DEBUG
    ULONGLONG elapsed = GetTickCount64() - start;
    wchar_t dbgBuf[256];
    swprintf_s(dbgBuf, L"[Thumbnail] Scan: %d files, %llu ms\n", scanCount, elapsed);
    OutputDebugStringW(dbgBuf);
#endif

    archive_read_free(a);
    return bestData.empty() ? E_FAIL : CreateHBITMAPFromData(bestData, cx, phbmp);
}

STDMETHODIMP CThumbnailProvider::GetThumbnail(UINT cx, HBITMAP* phbmp, WTS_ALPHATYPE* pdwAlpha) {
    if (!m_spStream || !phbmp || cx == 0 || cx > 4096) return E_INVALIDARG;
    *phbmp = NULL;
    if (pdwAlpha) *pdwAlpha = WTSAT_ARGB;
    HRESULT hr = E_FAIL;
    HRESULT hrCo = CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);
    bool needUninit = (hrCo == S_OK || hrCo == S_FALSE);

    __try {
        __try {
            hr = GetThumbnailImpl(cx, phbmp, pdwAlpha);
            if (FAILED(hr)) {
                BITMAPINFO bmi = { 0 };
                bmi.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
                bmi.bmiHeader.biWidth = cx; bmi.bmiHeader.biHeight = -(long)cx;
                bmi.bmiHeader.biPlanes = 1; bmi.bmiHeader.biBitCount = 32;
                bmi.bmiHeader.biCompression = BI_RGB;
                void* pBits = nullptr;
                *phbmp = CreateDIBSection(NULL, &bmi, DIB_RGB_COLORS, &pBits, NULL, 0);
                if (*phbmp && pBits) {
                    HDC hdc = CreateCompatibleDC(NULL);
                    if (hdc) {
                        HBITMAP hOld = (HBITMAP)SelectObject(hdc, *phbmp);
                        RECT rc = { 0, 0, (long)cx, (long)cx };
                        HBRUSH hBr = CreateSolidBrush(RGB(242, 242, 247));
                        FillRect(hdc, &rc, hBr); DeleteObject(hBr);
                        SetTextColor(hdc, RGB(110, 110, 120)); SetBkMode(hdc, TRANSPARENT);
                        int fSz = max(MulDiv(cx, 16, 100), 12);
                        HFONT hFont = CreateFontW(fSz, 0, 0, 0, FW_BOLD, FALSE, FALSE, FALSE, DEFAULT_CHARSET, OUT_DEFAULT_PRECIS, CLIP_DEFAULT_PRECIS, CLEARTYPE_QUALITY, DEFAULT_PITCH, L"Segoe UI");
                        if (hFont) {
                            HFONT hOldF = (HFONT)SelectObject(hdc, hFont);
                            DrawTextW(hdc, L"ARCHIVE", -1, &rc, DT_CENTER | DT_VCENTER | DT_SINGLELINE);
                            SelectObject(hdc, hOldF); DeleteObject(hFont);
                        }
                        SelectObject(hdc, hOld);
                        DeleteDC(hdc);
                        hr = S_OK;
                    }
                }
            }
        }
        __except (EXCEPTION_EXECUTE_HANDLER) {
            if (*phbmp) { DeleteObject(*phbmp); *phbmp = NULL; }
            hr = E_FAIL;
        }
    }
    __finally {
        if (needUninit) CoUninitialize();
    }
    return hr;
}