#include "framework.h"
#include "resource.h"
#include "ModernArchiveThumbnail_h.h"
#include <initguid.h>
#include "ModernArchiveThumbnail_i.c"
#include "ThumbnailProvider.h"

using namespace ATL;

class CModernArchiveThumbnailModule : public ATL::CAtlDllModuleT< CModernArchiveThumbnailModule >
{
public:
	DECLARE_LIBID(LIBID_ModernArchiveThumbnailLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_MODERNARCHIVETHUMBNAIL, "{본인의-GUID}")
};

CModernArchiveThumbnailModule _AtlModule;

extern "C" STDAPI DllCanUnloadNow(void) {
	return _AtlModule.DllCanUnloadNow();
}

extern "C" STDAPI DllGetClassObject(_In_ REFCLSID rclsid, _In_ REFIID riid, _Outptr_ LPVOID* ppv) {
	return _AtlModule.DllGetClassObject(rclsid, riid, ppv);
}

extern "C" STDAPI DllRegisterServer(void) {
	return _AtlModule.DllRegisterServer();
}

extern "C" STDAPI DllUnregisterServer(void) {
	return _AtlModule.DllUnregisterServer();
}

extern "C" STDAPI DllInstall(BOOL bInstall, _In_opt_ LPCWSTR pszCmdLine) {
	HRESULT hr = E_FAIL;
	static const wchar_t szUser[] = L"user";
	if (pszCmdLine != NULL) {
		if (_wcsnicmp(pszCmdLine, szUser, ARRAYSIZE(szUser)) == 0)
			ATL::AtlSetPerUserRegistration(true);
	}
	if (bInstall) {
		hr = DllRegisterServer();
		if (FAILED(hr)) DllUnregisterServer();
	}
	else {
		hr = DllUnregisterServer();
	}
	return hr;
}