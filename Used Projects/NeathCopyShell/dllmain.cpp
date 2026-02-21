/****************************** Module Header ******************************\
Module Name:  dllmain.cpp
Project:      CppShellExtPropSheetHandler
Copyright (c) Microsoft Corporation.

The file implements DllMain, and the DllGetClassObject, DllCanUnloadNow, 
DllRegisterServer, DllUnregisterServer functions that are necessary for a COM 
DLL. 

DllGetClassObject invokes the class factory defined in ClassFactory.h/cpp and 
queries to the specific interface.

DllCanUnloadNow checks if we can unload the component from the memory.

DllRegisterServer registers the COM server and the drag-and-drop handler in 
the registry by invoking the helper functions defined in Reg.h/cpp. The 
drag-and-drop handler is associated with the .exe file class.

DllUnregisterServer unregisters the COM server and the drag-and-drop handler. 

This source is subject to the Microsoft Public License.
See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
All other rights reserved.

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
\***************************************************************************/

#include <windows.h>
#include <Guiddef.h>
#include "ClassFactory.h"           // For the class factory
#include "Reg.h"
#include "ShellLogger.h"
#include <comdef.h>
#include <exception>
#include <new>


// {BB9D6769-6605-48D5-B144-065A1743296A}
static const GUID x86_clsid =
{ 0xbb9d6769, 0x6605, 0x48d5, { 0xb1, 0x44, 0x6, 0x5a, 0x17, 0x43, 0x29, 0x6a } };


// {F574437A-F944-4350-B7E9-95B6C7008029}
// When you write your own handler, you must create a new CLSID by using the 
// "Create GUID" tool in the Tools menu, and specify the CLSID value here. 
const CLSID CLSID_FileDragDropExt = 
{ 0xF574437A, 0xF944, 0x4350, { 0xB7, 0xE9, 0x95, 0xB6, 0xC7, 0x00, 0x80, 0x29 } };


HINSTANCE   g_hInst     = NULL;
long        g_cDllRef   = 0;

static HRESULT DllGetClassObjectCore(REFCLSID rclsid, REFIID riid, void **ppv)
{
    HRESULT hr = CLASS_E_CLASSNOTAVAILABLE;
    if (IsEqualCLSID(CLSID_FileDragDropExt, rclsid))
    {
        hr = E_OUTOFMEMORY;

        ClassFactory *pClassFactory = new (std::nothrow) ClassFactory();
        if (pClassFactory)
        {
            hr = pClassFactory->QueryInterface(riid, ppv);
            pClassFactory->Release();
        }
    }

    return hr;
}

static HRESULT DllGetClassObjectSehGuard(REFCLSID rclsid, REFIID riid, void **ppv, ShellLogger* log)
{
    HRESULT hr = CLASS_E_CLASSNOTAVAILABLE;
    __try
    {
        hr = DllGetClassObjectCore(rclsid, riid, ppv);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        DWORD code = GetExceptionCode();
        if (log)
            SHELL_LOG((*log), 0, L"DLL", L"DLL-GET-99", L"SEH exception code=0x%08X", code);
        return E_FAIL;
    }

    return hr;
}

static HRESULT DllRegisterServerCore()
{
    HRESULT hr = E_FAIL;

    wchar_t szModule[MAX_PATH];
    if (GetModuleFileName(g_hInst, szModule, ARRAYSIZE(szModule)) == 0)
    {
        hr = HRESULT_FROM_WIN32(GetLastError());
        return hr;
    }

    hr = RegisterInprocServer(szModule, CLSID_FileDragDropExt,
        L"CppShellExtDragDropHandler.FileDragDropExt Class",
        L"Apartment");
    if (SUCCEEDED(hr))
    {
        hr = RegisterShellExtDragDropHandler(CLSID_FileDragDropExt,
            L"CppShellExtDragDropHandler.FileDragDropExt");
    }

    return hr;
}

static HRESULT DllRegisterServerSehGuard(ShellLogger* log)
{
    HRESULT hr = E_FAIL;
    __try
    {
        hr = DllRegisterServerCore();
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        DWORD code = GetExceptionCode();
        if (log)
            SHELL_LOG((*log), 0, L"DLL", L"DLL-REG-99", L"SEH exception code=0x%08X", code);
        return E_FAIL;
    }

    return hr;
}

static HRESULT DllUnregisterServerCore()
{
    HRESULT hr = E_FAIL;

    wchar_t szModule[MAX_PATH];
    if (GetModuleFileName(g_hInst, szModule, ARRAYSIZE(szModule)) == 0)
    {
        hr = HRESULT_FROM_WIN32(GetLastError());
        return hr;
    }

    hr = UnregisterInprocServer(CLSID_FileDragDropExt);
    if (SUCCEEDED(hr))
    {
        hr = UnregisterShellExtDragDropHandler(CLSID_FileDragDropExt);
    }

    return hr;
}

static HRESULT DllUnregisterServerSehGuard(ShellLogger* log)
{
    HRESULT hr = E_FAIL;
    __try
    {
        hr = DllUnregisterServerCore();
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        DWORD code = GetExceptionCode();
        if (log)
            SHELL_LOG((*log), 0, L"DLL", L"DLL-UNREG-99", L"SEH exception code=0x%08X", code);
        return E_FAIL;
    }

    return hr;
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD dwReason, LPVOID lpReserved)
{
    ShellLogger log;
	switch (dwReason)
	{
	case DLL_PROCESS_ATTACH:
        SHELL_TRACE_ENTER(log, L"DLL", L"DLL-ATTACH", L"Process attach hModule=%p", hModule);
        // Hold the instance of this DLL module, we will use it to get the 
        // path of the DLL to register the component.
        g_hInst = hModule;
        DisableThreadLibraryCalls(hModule);
        break;
	case DLL_THREAD_ATTACH:
        SHELL_TRACE_ENTER(log, L"DLL", L"DLL-THREAD-ATTACH", L"Thread attach");
        break;
	case DLL_THREAD_DETACH:
        SHELL_TRACE_ENTER(log, L"DLL", L"DLL-THREAD-DETACH", L"Thread detach");
        break;
	case DLL_PROCESS_DETACH:
        SHELL_TRACE_ENTER(log, L"DLL", L"DLL-DETACH", L"Process detach");
		break;
	}
	return TRUE;
}


//
//   FUNCTION: DllGetClassObject
//
//   PURPOSE: Create the class factory and query to the specific interface.
//
//   PARAMETERS:
//   * rclsid - The CLSID that will associate the correct data and code.
//   * riid - A reference to the identifier of the interface that the caller 
//     is to use to communicate with the class object.
//   * ppv - The address of a pointer variable that receives the interface 
//     pointer requested in riid. Upon successful return, *ppv contains the 
//     requested interface pointer. If an error occurs, the interface pointer 
//     is NULL. 
//
STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, void **ppv)
{
    ShellLogger log;
    SHELL_TRACE_ENTER(log, L"DLL", L"DLL-GET-01", L"DllGetClassObject rclsid=%p riid=%p ppv=%p", &rclsid, &riid, ppv);

    HRESULT hr = CLASS_E_CLASSNOTAVAILABLE;
    try
    {
        hr = DllGetClassObjectSehGuard(rclsid, riid, ppv, &log);
    }
    catch (const _com_error& ex)
    {
        SHELL_LOG(log, 0, L"DLL", L"DLL-GET-97", L"_com_error hr=0x%08X", ex.Error());
        return E_FAIL;
    }
    catch (const std::exception& ex)
    {
        SHELL_LOG(log, 0, L"DLL", L"DLL-GET-96", L"std::exception: %hs", ex.what());
        return E_FAIL;
    }
    catch (...)
    {
        SHELL_LOG(log, 0, L"DLL", L"DLL-GET-95", L"unknown C++ exception");
        return E_FAIL;
    }

    SHELL_TRACE_EXIT(log, L"DLL", L"DLL-GET-98", hr);
    return hr;
}


//
//   FUNCTION: DllCanUnloadNow
//
//   PURPOSE: Check if we can unload the component from the memory.
//
//   NOTE: The component can be unloaded from the memory when its reference 
//   count is zero (i.e. nobody is still using the component).
// 
STDAPI DllCanUnloadNow(void)
{
    ShellLogger log;
    SHELL_TRACE_ENTER(log, L"DLL", L"DLL-CAN-00", L"DllCanUnloadNow g_cDllRef=%ld", g_cDllRef);
    HRESULT hr = g_cDllRef > 0 ? S_FALSE : S_OK;
    SHELL_TRACE_EXIT(log, L"DLL", L"DLL-CAN-01", hr);
    return hr;
}


//
//   FUNCTION: DllRegisterServer
//
//   PURPOSE: Register the COM server and the drag-and-drop handler.
// 
STDAPI DllRegisterServer(void)
{
    ShellLogger log;
    SHELL_TRACE_ENTER(log, L"DLL", L"DLL-REG-01", L"DllRegisterServer");

    HRESULT hr = E_FAIL;
    try
    {
        hr = DllRegisterServerSehGuard(&log);
    }
    catch (const _com_error& ex)
    {
        SHELL_LOG(log, 0, L"DLL", L"DLL-REG-97", L"_com_error hr=0x%08X", ex.Error());
        return E_FAIL;
    }
    catch (const std::exception& ex)
    {
        SHELL_LOG(log, 0, L"DLL", L"DLL-REG-96", L"std::exception: %hs", ex.what());
        return E_FAIL;
    }
    catch (...)
    {
        SHELL_LOG(log, 0, L"DLL", L"DLL-REG-95", L"unknown C++ exception");
        return E_FAIL;
    }

    SHELL_TRACE_EXIT(log, L"DLL", L"DLL-REG-98", hr);
    return hr;
}


//
//   FUNCTION: DllUnregisterServer
//
//   PURPOSE: Unregister the COM server and the drag-and-drop handler.
// 
STDAPI DllUnregisterServer(void)
{
    ShellLogger log;
    SHELL_TRACE_ENTER(log, L"DLL", L"DLL-UNREG-01", L"DllUnregisterServer");

    HRESULT hr = E_FAIL;
    try
    {
        hr = DllUnregisterServerSehGuard(&log);
    }
    catch (const _com_error& ex)
    {
        SHELL_LOG(log, 0, L"DLL", L"DLL-UNREG-97", L"_com_error hr=0x%08X", ex.Error());
        return E_FAIL;
    }
    catch (const std::exception& ex)
    {
        SHELL_LOG(log, 0, L"DLL", L"DLL-UNREG-96", L"std::exception: %hs", ex.what());
        return E_FAIL;
    }
    catch (...)
    {
        SHELL_LOG(log, 0, L"DLL", L"DLL-UNREG-95", L"unknown C++ exception");
        return E_FAIL;
    }

    SHELL_TRACE_EXIT(log, L"DLL", L"DLL-UNREG-98", hr);
    return hr;
}
