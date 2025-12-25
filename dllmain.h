#pragma once

/**
 * ModernArchiveThumbnail - ATL Module Definition
 * * This class defines the main ATL module for the project,
 * managing the registration and lifecycle of the COM server.
 */
class CModernArchiveThumbnailModule : public ATL::CAtlDllModuleT< CModernArchiveThumbnailModule >
{
public:
    /**
     * Declares the Library ID (LIBID) for the type library.
     */
    DECLARE_LIBID(LIBID_ModernArchiveThumbnailLib)

    /**
     * Declares the AppID and the resource ID for the registry.
     * The GUID below is specific to this project's AppID registration.
     */
    DECLARE_REGISTRY_APPID_RESOURCEID(IDR_MODERNARCHIVETHUMBNAIL, "{6e50881f-7489-4977-9831-50e50f385c57}")
};

/**
 * Global external instance of the ATL module,
 * accessible throughout the DLL.
 */
extern class CModernArchiveThumbnailModule _AtlModule;