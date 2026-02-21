/****************************** Module Header ******************************\
Module Name:  ClassFactory.cpp
Project:      CppShellExtDragDropHandler
Copyright (c) Microsoft Corporation.

The file implements the class factory for the FileDragDropExt COM class. 

This source is subject to the Microsoft Public License.
See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
All other rights reserved.

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
\***************************************************************************/

#include "ClassFactory.h"
#include "FileDragDropExt.h"
#include "ShellLogger.h"
#include <new>
#include <Shlwapi.h>
#include <comdef.h>
#include <exception>
#pragma comment(lib, "shlwapi.lib")

extern long g_cDllRef;

static HRESULT CreateInstanceCore(IUnknown *pUnkOuter, REFIID riid, void **ppv)
{
    HRESULT hr = CLASS_E_NOAGGREGATION;

    if (pUnkOuter == NULL)
    {
        hr = E_OUTOFMEMORY;

        FileDragDropExt *pExt = new (std::nothrow) FileDragDropExt();
        if (pExt)
        {
            hr = pExt->QueryInterface(riid, ppv);
            pExt->Release();
        }
    }

    return hr;
}

static HRESULT CreateInstanceSehGuard(IUnknown *pUnkOuter, REFIID riid, void **ppv, ShellLogger* log)
{
    HRESULT hr = E_FAIL;
    __try
    {
        hr = CreateInstanceCore(pUnkOuter, riid, ppv);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        DWORD code = GetExceptionCode();
        if (log)
            SHELL_LOG((*log), 0, L"FACTORY", L"FACT-CREATE-99", L"SEH exception code=0x%08X", code);
        return E_FAIL;
    }

    return hr;
}

static HRESULT LockServerSehGuard(BOOL fLock, ShellLogger* log)
{
    __try
    {
        if (fLock)
            InterlockedIncrement(&g_cDllRef);
        else
            InterlockedDecrement(&g_cDllRef);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        DWORD code = GetExceptionCode();
        if (log)
            SHELL_LOG((*log), 0, L"FACTORY", L"FACT-LOCK-99", L"SEH exception code=0x%08X", code);
        return E_FAIL;
    }

    return S_OK;
}

ClassFactory::ClassFactory() : m_cRef(1)
{
    InterlockedIncrement(&g_cDllRef);
    ShellLogger log;
    SHELL_TRACE_ENTER(log, L"FACTORY", L"FACT-CTOR", L"constructor");
}

ClassFactory::~ClassFactory()
{
    InterlockedDecrement(&g_cDllRef);
    ShellLogger log;
    SHELL_TRACE_ENTER(log, L"FACTORY", L"FACT-DTOR", L"destructor");
}


//
// IUnknown
//

IFACEMETHODIMP ClassFactory::QueryInterface(REFIID riid, void **ppv)
{
    static const QITAB qit[] =
    {
        QITABENT(ClassFactory, IClassFactory),
        { 0 },
    };
    return QISearch(this, qit, riid, ppv);
}

IFACEMETHODIMP_(ULONG) ClassFactory::AddRef()
{
    return InterlockedIncrement(&m_cRef);
}

IFACEMETHODIMP_(ULONG) ClassFactory::Release()
{
    ULONG cRef = InterlockedDecrement(&m_cRef);
    if (0 == cRef)
    {
        delete this;
    }
    return cRef;
}


// 
// IClassFactory
//

IFACEMETHODIMP ClassFactory::CreateInstance(IUnknown *pUnkOuter, REFIID riid, void **ppv)
{
    ShellLogger log;
    SHELL_TRACE_ENTER(log, L"FACTORY", L"FACT-CREATE-01", L"CreateInstance pUnkOuter=%p riid=%p ppv=%p", pUnkOuter, &riid, ppv);

    HRESULT hr = CLASS_E_NOAGGREGATION;
    try
    {
        hr = CreateInstanceSehGuard(pUnkOuter, riid, ppv, &log);
    }
    catch (const _com_error& ex)
    {
        SHELL_LOG(log, 0, L"FACTORY", L"FACT-CREATE-97", L"_com_error hr=0x%08X", ex.Error());
        return E_FAIL;
    }
    catch (const std::exception& ex)
    {
        SHELL_LOG(log, 0, L"FACTORY", L"FACT-CREATE-96", L"std::exception: %hs", ex.what());
        return E_FAIL;
    }
    catch (...)
    {
        SHELL_LOG(log, 0, L"FACTORY", L"FACT-CREATE-95", L"unknown C++ exception");
        return E_FAIL;
    }

    SHELL_TRACE_EXIT(log, L"FACTORY", L"FACT-CREATE-98", hr);
    return hr;
}

IFACEMETHODIMP ClassFactory::LockServer(BOOL fLock)
{
    ShellLogger log;
    SHELL_TRACE_ENTER(log, L"FACTORY", L"FACT-LOCK-01", L"LockServer fLock=%d", fLock ? 1 : 0);

    HRESULT hr = E_FAIL;
    try
    {
        hr = LockServerSehGuard(fLock, &log);
    }
    catch (const _com_error& ex)
    {
        SHELL_LOG(log, 0, L"FACTORY", L"FACT-LOCK-97", L"_com_error hr=0x%08X", ex.Error());
        return E_FAIL;
    }
    catch (const std::exception& ex)
    {
        SHELL_LOG(log, 0, L"FACTORY", L"FACT-LOCK-96", L"std::exception: %hs", ex.what());
        return E_FAIL;
    }
    catch (...)
    {
        SHELL_LOG(log, 0, L"FACTORY", L"FACT-LOCK-95", L"unknown C++ exception");
        return E_FAIL;
    }

    SHELL_TRACE_EXIT(log, L"FACTORY", L"FACT-LOCK-98", hr);
    return hr;
}
