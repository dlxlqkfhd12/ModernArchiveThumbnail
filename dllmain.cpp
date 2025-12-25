#include "framework.h"
#include "resource.h"
#include "ModernArchiveThumbnail_h.h"
#include "ModernArchiveThumbnail_i.c"

class CModernArchiveThumbnailModule : public ATL::CAtlDllModuleT< CModernArchiveThumbnailModule >
{
public:
	DECLARE_LIBID(LIBID_ModernArchiveThumbnailLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_MODERNARCHIVETHUMBNAIL, "{∫ª¿Œ¿«-GUID}")
};

extern CModernArchiveThumbnailModule _AtlModule;

extern "C" BOOL WINAPI DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID lpReserved)
{
	return _AtlModule.DllMain(dwReason, lpReserved);
}