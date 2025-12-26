#pragma once
#include <shobjidl.h>
#include <thumbcache.h>
#include <atlbase.h>
#include <atlcom.h>
#include <wrl/client.h>
#include "ModernArchiveThumbnail_h.h" 

#define THUMBNAIL_VERSION L"1.1.2"

#ifdef _DEBUG
#define LOG_DEBUG(msg) OutputDebugStringW(L"[Thumbnail] " msg L"\n")
#else
#define LOG_DEBUG(msg)
#endif

using namespace Microsoft::WRL;

class ATL_NO_VTABLE CThumbnailProvider :
    public ATL::CComObjectRootEx<ATL::CComSingleThreadModel>,
    public ATL::CComCoClass<CThumbnailProvider, &CLSID_ThumbnailProvider>,
    public IInitializeWithStream,
    public IThumbnailProvider
{
public:
    CThumbnailProvider() {}

    DECLARE_REGISTRY_RESOURCEID(106)

    BEGIN_COM_MAP(CThumbnailProvider)
        COM_INTERFACE_ENTRY(IInitializeWithStream)
        COM_INTERFACE_ENTRY(IThumbnailProvider)
    END_COM_MAP()

    DECLARE_PROTECT_FINAL_CONSTRUCT()

    HRESULT FinalConstruct() { return S_OK; }
    void FinalRelease() {}

    STDMETHODIMP Initialize(IStream* pStream, DWORD grfMode);
    STDMETHODIMP GetThumbnail(UINT cx, HBITMAP* phbmp, WTS_ALPHATYPE* pdwAlpha);

private:
    ComPtr<IStream> m_spStream;
    HRESULT GetThumbnailImpl(UINT cx, HBITMAP* phbmp, WTS_ALPHATYPE* pdwAlpha);
};

OBJECT_ENTRY_AUTO(__uuidof(ThumbnailProvider), CThumbnailProvider)