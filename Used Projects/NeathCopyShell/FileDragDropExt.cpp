/******************************** Module Header ********************************\
Module Name:  FileDragDropExt.cpp
Project:      CppShellExtDragDropHandler
Copyright (c) Microsoft Corporation.

The code sample demonstrates creating a Shell drag-and-drop handler with C++. 

When a user right-clicks a Shell object to drag an object, a context menu is 
displayed when the user attempts to drop the object. A drag-and-drop handler 
is a context menu handler that can add items to this context menu.

The example drag-and-drop handler adds the menu item "Create hard link here" to 
the context menu. When you right-click a file and drag the file to a directory or 
a drive or the desktop, a context menu will be displayed with the "Create hard 
link here" menu item. By clicking the menu item, the handler will create a hard 
link for the dragged file in the dropped location. The name of the link is "Hard 
link to <source file name>". 

This source is subject to the Microsoft Public License.
See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
All other rights reserved.

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
\*******************************************************************************/

#include "FileDragDropExt.h"
#include "Reg.h"
#include "PathWriter.h"
#include "ShellLogger.h"

#include <comdef.h>
#include <new>
#include <exception>
#include <strsafe.h>
#include <Shlwapi.h>


#define MAX_WPATH		32000
#define MAX_PATH_EX		1024*8

#pragma comment(lib, "shlwapi.lib")

extern long g_cDllRef;


#define IDM_RUNFROMHERE            0  // The command's identifier offset

FileDragDropExt::FileDragDropExt() : m_cRef(1),
m_pszMenuText(L"Don't move") {
	InterlockedIncrement(&g_cDllRef);
	m_sourcesJson[0] = L'\0';
	m_isDefaultHandler = false;
	m_shouldHandle = false;

	ShellLogger log;
	SHELL_TRACE_ENTER(log, L"EXT", L"EXT-CTOR", L"constructor");
}


FileDragDropExt::~FileDragDropExt() {
	InterlockedDecrement(&g_cDllRef);
	ShellLogger log;
	SHELL_TRACE_ENTER(log, L"EXT", L"EXT-DTOR", L"destructor");
}


#pragma region IUnknown

// Query to the interface the component supported.
IFACEMETHODIMP FileDragDropExt::QueryInterface(REFIID riid, void** ppv) {
	static const QITAB qit[] =
	{
		QITABENT(FileDragDropExt, IContextMenu),
		QITABENT(FileDragDropExt, IShellExtInit),
		{ 0 },
	};
	return QISearch(this, qit, riid, ppv);
}

// Increase the reference count for an interface on an object.
IFACEMETHODIMP_(ULONG) FileDragDropExt::AddRef() {
	return InterlockedIncrement(&m_cRef);
}

// Decrease the reference count for an interface on an object.
IFACEMETHODIMP_(ULONG) FileDragDropExt::Release() {
	ULONG cRef = InterlockedDecrement(&m_cRef);
	if (0 == cRef) {
		delete this;
	}
	return cRef;
}

#pragma endregion


#pragma region IShellExtInit

static int fastmove;

WCHAR dir[260];
DWORD dirSize = sizeof(dir);

WCHAR logs[260];
DWORD logsSize = sizeof(dir);

static WCHAR list[260];
static DWORD listSize = sizeof(dir);

WCHAR app[260];
DWORD appSize = sizeof(dir);

static bool ReadRegSzValue(HKEY root, const wchar_t* subKey, const wchar_t* name, wchar_t* value, DWORD valueChars)
{
	if (!value || valueChars == 0)
		return false;

	value[0] = L'\0';

	HKEY hKey;
	LONG result = RegOpenKeyEx(root, subKey, 0, KEY_READ, &hKey);
	if (result != ERROR_SUCCESS)
		return false;

	DWORD type = 0;
	DWORD bufferSize = valueChars * sizeof(wchar_t);
	result = RegQueryValueEx(hKey, name, NULL, &type, (LPBYTE)value, &bufferSize);
	RegCloseKey(hKey);

	if (result != ERROR_SUCCESS || (type != REG_SZ && type != REG_EXPAND_SZ))
		return false;

	value[valueChars - 1] = L'\0';
	return true;
}

static bool IsTrueValue(const wchar_t* value)
{
	if (!value || value[0] == L'\0')
		return false;

	if (wcscmp(value, L"1") == 0)
		return true;

	if (_wcsicmp(value, L"true") == 0)
		return true;

	return false;
}

static bool AppendText(wchar_t* dest, size_t destCount, const wchar_t* text)
{
	if (!dest || !text || destCount == 0)
		return false;

	size_t current = wcslen(dest);
	size_t incoming = wcslen(text);
	if (current + incoming >= destCount)
		return false;

	wcscat_s(dest, destCount, text);
	return true;
}

static bool JsonEscapeToBuffer(const wchar_t* value, wchar_t* out, size_t outCount)
{
	if (!value || !out || outCount == 0)
		return false;

	out[0] = L'\0';

	for (size_t i = 0; value[i] != L'\0'; ++i)
	{
		wchar_t ch = value[i];
		switch (ch)
		{
		case L'\\':
			if (!AppendText(out, outCount, L"\\\\"))
				return false;
			break;
		case L'"':
			if (!AppendText(out, outCount, L"\\\""))
				return false;
			break;
		case L'\b':
			if (!AppendText(out, outCount, L"\\b"))
				return false;
			break;
		case L'\f':
			if (!AppendText(out, outCount, L"\\f"))
				return false;
			break;
		case L'\n':
			if (!AppendText(out, outCount, L"\\n"))
				return false;
			break;
		case L'\r':
			if (!AppendText(out, outCount, L"\\r"))
				return false;
			break;
		case L'\t':
			if (!AppendText(out, outCount, L"\\t"))
				return false;
			break;
		default:
			if (ch < 0x20)
			{
				wchar_t encoded[7] = { 0 };
				swprintf_s(encoded, _countof(encoded), L"\\u%04x", (int)ch);
				if (!AppendText(out, outCount, encoded))
					return false;
			}
			else
			{
				wchar_t one[2] = { ch, L'\0' };
				if (!AppendText(out, outCount, one))
					return false;
			}
			break;
		}
	}

	return true;
}

static bool BuildJsonMessage(const wchar_t* operation, const wchar_t* sourcesJson, const wchar_t* destination, wchar_t* out, size_t outCount, ShellLogger* log, const wchar_t* stepId)
{
	if (log)
		SHELL_LOG((*log), 2, L"JSON", stepId, L"BuildJsonMessage start");

	if (!operation || !sourcesJson || !destination || !out || outCount == 0)
		return false;

	wchar_t escapedOp[MAX_PATH_EX] = { 0 };
	wchar_t escapedDest[MAX_PATH_EX] = { 0 };
	if (!JsonEscapeToBuffer(operation, escapedOp, _countof(escapedOp)))
		return false;
	if (!JsonEscapeToBuffer(destination, escapedDest, _countof(escapedDest)))
		return false;

	out[0] = L'\0';
	if (!AppendText(out, outCount, L"{\"Operation\":\""))
		return false;
	if (!AppendText(out, outCount, escapedOp))
		return false;
	if (!AppendText(out, outCount, L"\",\"Sources\":["))
		return false;
	if (!AppendText(out, outCount, sourcesJson))
		return false;
	if (!AppendText(out, outCount, L"],\"Destination\":\""))
		return false;
	if (!AppendText(out, outCount, escapedDest))
		return false;
	if (!AppendText(out, outCount, L"\"}"))
		return false;

	if (log)
		SHELL_LOG((*log), 2, L"JSON", stepId, L"BuildJsonMessage success length=%u", (UINT)wcslen(out));

	return true;
}

static bool SendPipeMessage(const wchar_t* json, DWORD timeoutMs, ShellLogger* log, const wchar_t* stepId)
{
	if (!json || json[0] == L'\0')
		return false;

	const wchar_t* pipeName = L"\\\\.\\pipe\\NeathCopyPipe";
	if (log)
		SHELL_LOG((*log), 2, L"PIPE", stepId, L"SendPipeMessage start timeout=%lu", timeoutMs);

	const DWORD maxWaitMs = timeoutMs == 0 ? 2000 : timeoutMs;
	DWORD waited = 0;
	while (waited < maxWaitMs)
	{
		if (WaitNamedPipeW(pipeName, 50))
			break;

		DWORD lastError = GetLastError();
		if (log && log->Verbosity() >= 3)
			SHELL_LOG((*log), 3, L"PIPE", stepId, L"WaitNamedPipeW retry waited=%lu error=%lu", waited, lastError);

		Sleep(50);
		waited += 100;
	}

	if (waited >= maxWaitMs)
	{
		if (log)
			SHELL_LOG((*log), 1, L"PIPE", stepId, L"WaitNamedPipeW timeout after %lu ms error=%lu", waited, GetLastError());
		return false;
	}

	HANDLE pipe = CreateFileW(pipeName, GENERIC_WRITE, 0, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
	if (pipe == INVALID_HANDLE_VALUE)
	{
		if (log)
			SHELL_LOG((*log), 1, L"PIPE", stepId, L"CreateFileW failed error=%lu", GetLastError());
		return false;
	}

	int wcharCount = (int)wcslen(json);
	int byteCount = WideCharToMultiByte(CP_UTF8, 0, json, wcharCount, NULL, 0, NULL, NULL);
	if (byteCount <= 0)
	{
		if (log)
			SHELL_LOG((*log), 1, L"PIPE", stepId, L"WideCharToMultiByte sizing failed error=%lu", GetLastError());
		CloseHandle(pipe);
		return false;
	}

	char* payload = new (std::nothrow) char[byteCount];
	if (!payload)
	{
		if (log)
			SHELL_LOG((*log), 1, L"PIPE", stepId, L"Payload allocation failed bytes=%d", byteCount);
		CloseHandle(pipe);
		return false;
	}

	if (!WideCharToMultiByte(CP_UTF8, 0, json, wcharCount, payload, byteCount, NULL, NULL))
	{
		if (log)
			SHELL_LOG((*log), 1, L"PIPE", stepId, L"WideCharToMultiByte failed error=%lu", GetLastError());
		delete[] payload;
		CloseHandle(pipe);
		return false;
	}

	DWORD written = 0;
	BOOL ok = WriteFile(pipe, payload, (DWORD)byteCount, &written, NULL);

	delete[] payload;
	CloseHandle(pipe);
	if (!ok)
	{
		if (log)
			SHELL_LOG((*log), 1, L"PIPE", stepId, L"WriteFile failed error=%lu", GetLastError());
		return false;
	}

	if (log)
		SHELL_LOG((*log), 2, L"PIPE", stepId, L"WriteFile success bytes=%lu", written);

	return true;
}

static bool GetLocalAppDataLogPath(wchar_t* outPath, size_t outCount)
{
	if (!outPath || outCount == 0)
		return false;

	outPath[0] = L'\0';

	wchar_t* path = NULL;
	if (!SUCCEEDED(SHGetKnownFolderPath(FOLDERID_LocalAppData, 0, NULL, &path)))
		return false;

	wcscpy_s(outPath, outCount, path);
	CoTaskMemFree(path);
	if (!AppendText(outPath, outCount, L"\\Eneleich\\NeathCopy\\logs\\shell.log"))
		return false;

	return true;
}

static void EnsureDirectoryForFile(const wchar_t* filePath)
{
	if (!filePath || filePath[0] == L'\0')
		return;

	const wchar_t* lastSlash = wcsrchr(filePath, L'\\');
	const wchar_t* lastSlash2 = wcsrchr(filePath, L'/');
	if (lastSlash2 && (!lastSlash || lastSlash2 > lastSlash))
		lastSlash = lastSlash2;

	if (!lastSlash)
		return;

	size_t dirLen = (size_t)(lastSlash - filePath);
	if (dirLen == 0 || dirLen >= MAX_PATH_EX)
		return;

	wchar_t dir[MAX_PATH_EX] = { 0 };
	wcsncpy_s(dir, _countof(dir), filePath, dirLen);
	CreateDirectoryW(dir, NULL);
}

static void LogShellError(const wchar_t* message, const wchar_t* logsDir)
{
	if (!message)
		return;

	wchar_t logPath[MAX_PATH_EX] = { 0 };
	if (logsDir && logsDir[0] != L'\0')
	{
		wcscpy_s(logPath, _countof(logPath), logsDir);
		size_t len = wcslen(logPath);
		if (len > 0 && logPath[len - 1] != L'\\' && logPath[len - 1] != L'/')
			AppendText(logPath, _countof(logPath), L"\\");
		AppendText(logPath, _countof(logPath), L"shell.log");
	}
	else
	{
		if (!GetLocalAppDataLogPath(logPath, _countof(logPath)))
			return;
	}

	if (logPath[0] == L'\0')
		return;

	EnsureDirectoryForFile(logPath);

	HANDLE file = CreateFileW(logPath, FILE_APPEND_DATA, FILE_SHARE_READ, NULL, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
	if (file == INVALID_HANDLE_VALUE)
		return;

	SYSTEMTIME st;
	GetLocalTime(&st);
	wchar_t timeBuf[64];
	swprintf_s(timeBuf, 64, L"%04d-%02d-%02d %02d:%02d:%02d ",
		st.wYear, st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond);

	wchar_t line[1024] = { 0 };
	wcscpy_s(line, _countof(line), timeBuf);
	AppendText(line, _countof(line), message);
	AppendText(line, _countof(line), L"\r\n");

	DWORD written = 0;
	WriteFile(file, line, (DWORD)(wcslen(line) * sizeof(wchar_t)), &written, NULL);
	CloseHandle(file);
}

BOOL FastMove(LPCWSTR source, LPCWSTR destiny)
{
	wchar_t rootSource[MAX_PATH] = { 0 };
	wchar_t rootDestiny[MAX_PATH] = { 0 };

	if (!source || !destiny)
		return FALSE;

	if (!GetVolumePathNameW(source, rootSource, _countof(rootSource)))
		return FALSE;

	if (!GetVolumePathNameW(destiny, rootDestiny, _countof(rootDestiny)))
		return FALSE;

	return StrCmp(rootSource, rootDestiny) == 0;
}

// Initialize the drag and drop handler.  If we return S_OK, then QueryContextMenu will be called. 
// Otherwise this handler will not be used in this event.
IFACEMETHODIMP FileDragDropExt::Initialize(LPCITEMIDLIST pidlFolder, LPDATAOBJECT pDataObj, HKEY hKeyProgID)
{
	ShellLogger log;
	log.ResetMsgBox();
	SHELL_TRACE_ENTER(log, L"INIT", L"INIT-01", L"start pidlFolder=%p pDataObj=%p hKeyProgID=%p", pidlFolder, pDataObj, hKeyProgID);

	HRESULT hr = E_FAIL;
	try
	{
		hr = InitializeSehGuard(pidlFolder, pDataObj, hKeyProgID, log);
	}
	catch (const _com_error& ex)
	{
		SHELL_LOG(log, 0, L"INIT", L"INIT-97", L"_com_error hr=0x%08X", ex.Error());
		return E_FAIL;
	}
	catch (const std::exception& ex)
	{
		SHELL_LOG(log, 0, L"INIT", L"INIT-96", L"std::exception: %hs", ex.what());
		return E_FAIL;
	}
	catch (...)
	{
		SHELL_LOG(log, 0, L"INIT", L"INIT-95", L"unknown C++ exception");
		return E_FAIL;
	}

	SHELL_TRACE_EXIT(log, L"INIT", L"INIT-98", hr);
	return hr;
}

HRESULT FileDragDropExt::InitializeImpl(LPCITEMIDLIST pidlFolder, LPDATAOBJECT pDataObj, HKEY hKeyProgID, ShellLogger& log)
{
	WCHAR path[MAX_PATH_EX] = { 0 };
	wchar_t escapedPath[MAX_PATH_EX] = { 0 };
	wchar_t* auxPath = new (std::nothrow) wchar_t[MAX_PATH_EX + 1];
	if (!auxPath)
	{
		SHELL_LOG(log, 0, L"INIT", L"INIT-00", L"auxPath allocation failed");
		return E_OUTOFMEMORY;
	}

	auxPath[0] = L'\0';
	BOOL fastMoveSeted = FALSE;
	m_sourcesJson[0] = L'\0';
	m_shouldHandle = false;
	wcscpy_s(m_integrationMode, _countof(m_integrationMode), L"TrayIPC");
	m_isDefaultHandler = false;

	wchar_t defaultFlag[32] = { 0 };
	if (ReadRegSzValue(HKEY_CURRENT_USER, L"SOFTWARE\\Eneleich\\NeathCopy", L"IsDefaultCopyHandler", defaultFlag, _countof(defaultFlag)))
		m_isDefaultHandler = IsTrueValue(defaultFlag);

	wchar_t modeValue[64] = { 0 };
	if (ReadRegSzValue(HKEY_CURRENT_USER, L"SOFTWARE\\Eneleich\\NeathCopy", L"IntegrationMode", modeValue, _countof(modeValue)) && modeValue[0] != L'\0')
		wcscpy_s(m_integrationMode, _countof(m_integrationMode), modeValue);

	if (m_integrationMode[0] == L'\0')
		wcscpy_s(m_integrationMode, _countof(m_integrationMode), L"TrayIPC");

	m_shouldHandle = m_isDefaultHandler;

	wchar_t copyHandlerPath[MAX_PATH_EX] = { 0 };
	ReadRegSzValue(HKEY_CURRENT_USER, L"SOFTWARE\\Eneleich\\NeathCopy", L"CopyHandler", copyHandlerPath, _countof(copyHandlerPath));

	SHELL_LOG(log, 2, L"INIT", L"INIT-02", L"Registry IsDefaultCopyHandler=%d IntegrationMode=%s CopyHandler=%s",
		m_isDefaultHandler ? 1 : 0,
		m_integrationMode,
		copyHandlerPath[0] != L'\0' ? copyHandlerPath : L"(empty)");

	if (!pidlFolder)
	{
		SHELL_LOG(log, 1, L"INIT", L"INIT-03", L"pidlFolder is null");
		delete[] auxPath;
		return E_INVALIDARG;
	}

	if (!SHGetPathFromIDList(pidlFolder, m_szTargetDir))
	{
		SHELL_LOG(log, 1, L"INIT", L"INIT-04", L"SHGetPathFromIDList failed error=%lu", GetLastError());
		delete[] auxPath;
		return E_FAIL;
	}

	SHELL_LOG(log, 2, L"INIT", L"INIT-05", L"TargetDir=%s", m_szTargetDir);

	if (NULL == pDataObj)
	{
		SHELL_LOG(log, 1, L"INIT", L"INIT-06", L"pDataObj is null");
		delete[] auxPath;
		return E_INVALIDARG;
	}

	if (!m_shouldHandle)
	{
		SHELL_LOG(log, 2, L"INIT", L"INIT-07", L"Handler disabled (IsDefaultCopyHandler=0)");
		delete[] auxPath;
		return E_FAIL;
	}

	HRESULT hr = E_FAIL;
	FORMATETC fe = { CF_HDROP, NULL, DVASPECT_CONTENT, -1, TYMED_HGLOBAL };
	STGMEDIUM stm = {};
	bool gotData = false;
	HDROP hDrop = NULL;

	HRESULT hrData = pDataObj->GetData(&fe, &stm);
	SHELL_LOG(log, 2, L"INIT", L"INIT-08", L"GetData hr=0x%08X", hrData);
	if (FAILED(hrData))
		goto cleanup;

	gotData = true;
	hDrop = static_cast<HDROP>(GlobalLock(stm.hGlobal));
	if (hDrop == NULL)
	{
		SHELL_LOG(log, 1, L"INIT", L"INIT-09", L"GlobalLock failed error=%lu", GetLastError());
		goto cleanup;
	}

	UINT nFiles = DragQueryFile(hDrop, 0xFFFFFFFF, NULL, 0);
	SHELL_LOG(log, 2, L"INIT", L"INIT-10", L"DragQueryFile count=%u", nFiles);

	if (nFiles > 0)
	{
		HKEY hKey = nullptr;
		LONG lResult = RegOpenKeyEx(HKEY_CURRENT_USER, TEXT("SOFTWARE\\Eneleich\\NeathCopy"), 0, KEY_READ, &hKey);
		if (lResult != ERROR_SUCCESS)
		{
			SHELL_LOG(log, 1, L"INIT", L"INIT-11", L"RegOpenKeyEx failed error=%ld", lResult);
		}
		else
		{
			LONG listResult = RegQueryValueEx(hKey, L"FilesList", NULL, NULL, (LPBYTE)list, &listSize);
			SHELL_LOG(log, 2, L"INIT", L"INIT-12", L"FilesList read result=%ld path=%s", listResult, list);
			RegCloseKey(hKey);
			if (listResult != ERROR_SUCCESS || list[0] == L'\0')
			{
				SHELL_LOG(log, 1, L"INIT", L"INIT-12A", L"FilesList missing or empty result=%ld", listResult);
				goto cleanup;
			}
		}

		wchar_t filesListPath[MAX_PATH_EX] = { 0 };
		wcscpy_s(filesListPath, _countof(filesListPath), list);

		char filesListPathA[MAX_PATH_EX] = { 0 };
		int listBytes = WideCharToMultiByte(CP_ACP, 0, filesListPath, -1, filesListPathA, (int)_countof(filesListPathA), NULL, NULL);
		if (listBytes <= 0 || filesListPathA[0] == '\0')
		{
			SHELL_LOG(log, 1, L"INIT", L"INIT-12B", L"Failed to convert FilesList path to ANSI error=%lu", GetLastError());
			goto cleanup;
		}

		PathWriter pathWriter(filesListPathA);
		if (!pathWriter.ClearFilesList())
		{
			SHELL_LOG(log, 1, L"INIT", L"INIT-12C", L"Failed to clear FilesList");
			goto cleanup;
		}
		if (!pathWriter.OpenFile("a,ccs=UNICODE"))
		{
			SHELL_LOG(log, 1, L"INIT", L"INIT-12D", L"Failed to open FilesList for append");
			goto cleanup;
		}

		for (UINT i = 0; i < nFiles; i++)
		{
			UINT copied = DragQueryFileW(hDrop, i, path, MAX_PATH_EX);
			if (copied == 0)
			{
				SHELL_LOG(log, 1, L"INIT", L"INIT-13", L"DragQueryFileW failed index=%u error=%lu", i, GetLastError());
				continue;
			}

			if (JsonEscapeToBuffer(path, escapedPath, _countof(escapedPath)))
			{
				if (m_sourcesJson[0] != L'\0')
					AppendText(m_sourcesJson, _countof(m_sourcesJson), L",");
				AppendText(m_sourcesJson, _countof(m_sourcesJson), L"\"");
				AppendText(m_sourcesJson, _countof(m_sourcesJson), escapedPath);
				AppendText(m_sourcesJson, _countof(m_sourcesJson), L"\"");
			}
			else
			{
				SHELL_LOG(log, 1, L"INIT", L"INIT-14", L"JsonEscape failed path=%s", path);
			}

			if (wcslen(auxPath) + wcslen(path) + 2 < MAX_PATH_EX)
			{
				wcscat_s(auxPath, MAX_PATH_EX, path);
				wcscat_s(auxPath, MAX_PATH_EX, L"|");
				pathWriter.RegisterPath(auxPath);
			}
			else
			{
				SHELL_LOG(log, 1, L"INIT", L"INIT-15", L"auxPath overflow prevented path=%s", path);
			}

			wcscpy_s(auxPath, 1, L"");

			if (!fastMoveSeted)
			{
				fastmove = FastMove(path, m_szTargetDir);
				fastMoveSeted = TRUE;
			}
		}

		pathWriter.WriteBuffer(false);
		pathWriter.CloseFile();

		SHELL_LOG(log, 2, L"INIT", L"INIT-16", L"Wrote FilesList to disk path=%s", filesListPath);
		SHELL_LOG(log, 2, L"INIT", L"INIT-17", L"Sources JSON length=%u", (UINT)wcslen(m_sourcesJson));

		hr = S_OK;
	}
	else
	{
		SHELL_LOG(log, 1, L"INIT", L"INIT-18", L"No files in drop");
	}

cleanup:
	if (hDrop != NULL)
		GlobalUnlock(stm.hGlobal);
	if (gotData)
		ReleaseStgMedium(&stm);

	delete[] auxPath;
	return hr;
}

HRESULT FileDragDropExt::InitializeSehGuard(LPCITEMIDLIST pidlFolder, LPDATAOBJECT pDataObj, HKEY hKeyProgID, ShellLogger& log)
{
	HRESULT hr = E_FAIL;
	__try
	{
		hr = InitializeImpl(pidlFolder, pDataObj, hKeyProgID, log);
	}
	__except (EXCEPTION_EXECUTE_HANDLER)
	{
		DWORD code = GetExceptionCode();
		SHELL_LOG(log, 0, L"INIT", L"INIT-99", L"SEH exception code=0x%08X", code);
		return E_FAIL;
	}

	return hr;
}

#pragma endregion


#pragma region IContextMenu


enum {
	CMDID_FIRST = 0,

	CMDID_NCOPY = CMDID_FIRST,
	CMDID_NMOVE,
};

//
//   FUNCTION: FileDragDropExt::QueryContextMenu
//
//   PURPOSE: The Shell calls IContextMenu::QueryContextMenu to allow the 
//            context menu handler to add its menu items to the menu. It 
//            passes in the HMENU handle in the hmenu parameter. The 
//            indexMenu parameter is set to the index to be used for the 
//            first menu item that is to be added.
//
IFACEMETHODIMP FileDragDropExt::QueryContextMenu(HMENU hmenu,UINT  indexMenu,UINT  idCmdFirst,UINT  idCmdLast,UINT  uFlags)
{
	ShellLogger log;
	log.ResetMsgBox();
	SHELL_TRACE_ENTER(log, L"QCM", L"QCM-01", L"start uFlags=0x%08X idCmdFirst=%u idCmdLast=%u", uFlags, idCmdFirst, idCmdLast);

	HRESULT hr = E_FAIL;
	try
	{
		hr = QueryContextMenuSehGuard(hmenu, indexMenu, idCmdFirst, idCmdLast, uFlags, log);
	}
	catch (const _com_error& ex)
	{
		SHELL_LOG(log, 0, L"QCM", L"QCM-97", L"_com_error hr=0x%08X", ex.Error());
		return MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_NULL, USHORT(0));
	}
	catch (const std::exception& ex)
	{
		SHELL_LOG(log, 0, L"QCM", L"QCM-96", L"std::exception: %hs", ex.what());
		return MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_NULL, USHORT(0));
	}
	catch (...)
	{
		SHELL_LOG(log, 0, L"QCM", L"QCM-95", L"unknown C++ exception");
		return MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_NULL, USHORT(0));
	}

	SHELL_TRACE_EXIT(log, L"QCM", L"QCM-98", hr);
	return hr;
	
}

HRESULT FileDragDropExt::QueryContextMenuImpl(HMENU hmenu, UINT indexMenu, UINT idCmdFirst, UINT idCmdLast, UINT uFlags, ShellLogger& log)
{
	if (uFlags & CMF_DEFAULTONLY)
	{
		SHELL_LOG(log, 3, L"QCM", L"QCM-02", L"CMF_DEFAULTONLY set");
		return MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_NULL, USHORT(0));
	}

	if (!m_shouldHandle)
	{
		SHELL_LOG(log, 2, L"QCM", L"QCM-03", L"Handler disabled");
		return MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_NULL, USHORT(0));
	}

	SHELL_LOG(log, 2, L"QCM", L"QCM-03B", L"IntegrationMode=%s Default=%d", m_integrationMode, m_isDefaultHandler ? 1 : 0);

	int cmd = idCmdFirst;

	InsertMenu(hmenu, indexMenu++, MF_STRING | MF_BYPOSITION, cmd++, L"NeathCopy Here");
	InsertMenu(hmenu, indexMenu++, MF_STRING | MF_BYPOSITION, cmd++, L"NeathMove Here");

	int defItem = GetMenuDefaultItem(hmenu, false, 0);
	SHELL_LOG(log, 2, L"QCM", L"QCM-04", L"Default menu item=%d fastmove=%d", defItem, fastmove);
	if (defItem == 1)
	{
		SetMenuDefaultItem(hmenu, idCmdFirst + defItem - 1, false);
	}
	else if (defItem == 2)
	{
		if (fastmove == 1)
		{
			// Same-volume move makes Windows prefer Move as the default drop action.
			// We must still report the two NeathCopy menu items that were inserted;
			// otherwise Explorer treats the extension as if it added no commands and
			// the custom NeathCopy Copy/Move entries stop invoking our handler.
			SHELL_LOG(log, 2, L"QCM", L"QCM-05", L"Fast move detected; keeping NeathCopy commands registered and leaving Windows default unchanged");
		}
		else
		{
			SetMenuDefaultItem(hmenu, idCmdFirst + defItem - 1, false);
		}
	}

	return MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_NULL, 2);
}

HRESULT FileDragDropExt::QueryContextMenuSehGuard(HMENU hmenu, UINT indexMenu, UINT idCmdFirst, UINT idCmdLast, UINT uFlags, ShellLogger& log)
{
	HRESULT hr = E_FAIL;
	__try
	{
		hr = QueryContextMenuImpl(hmenu, indexMenu, idCmdFirst, idCmdLast, uFlags, log);
	}
	__except (EXCEPTION_EXECUTE_HANDLER)
	{
		DWORD code = GetExceptionCode();
		SHELL_LOG(log, 0, L"QCM", L"QCM-99", L"SEH exception code=0x%08X", code);
		return MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_NULL, USHORT(0));
	}

	return hr;
}

//
//   FUNCTION: FileDragDropExt::GetCommandString
//
//   PURPOSE: If a user highlights one of the items added by a context menu 
//            handler, the handler's IContextMenu::GetCommandString method is 
//            called to request a Help text string that will be displayed on 
//            the Windows Explorer status bar. This method can also be called 
//            to request the verb string that is assigned to a command. 
//            Either ANSI or Unicode verb strings can be requested. This 
//            example does not need to specify any verb for the command, so 
//            the method returns E_NOTIMPL directly. 
//
IFACEMETHODIMP FileDragDropExt::GetCommandString(UINT_PTR idCommand,
	UINT uFlags, UINT* pwReserved, LPSTR pszName, UINT cchMax) {
	return E_NOTIMPL;
}


//
//   FUNCTION: FileDragDropExt::InvokeCommand
//
//   PURPOSE: This method is called when a user clicks a menu item to tell 
//            the handler to run the associated command. It seems to not be called unless
//			  we actually add a menu item in QueryContextMenu.  That's ok, we don't do
//            anything here anyway.
//
IFACEMETHODIMP FileDragDropExt::InvokeCommand(LPCMINVOKECOMMANDINFO pCmdInfo) 
{
	ShellLogger log;
	log.ResetMsgBox();
	SHELL_TRACE_ENTER(log, L"INV", L"INV-01", L"start pCmdInfo=%p", pCmdInfo);

	HRESULT hr = E_FAIL;
	try
	{
		hr = InvokeCommandSehGuard(pCmdInfo, log);
	}
	catch (const _com_error& ex)
	{
		SHELL_LOG(log, 0, L"INV", L"INV-97", L"_com_error hr=0x%08X", ex.Error());
		return E_FAIL;
	}
	catch (const std::exception& ex)
	{
		SHELL_LOG(log, 0, L"INV", L"INV-96", L"std::exception: %hs", ex.what());
		return E_FAIL;
	}
	catch (...)
	{
		SHELL_LOG(log, 0, L"INV", L"INV-95", L"unknown C++ exception");
		return E_FAIL;
	}

	SHELL_TRACE_EXIT(log, L"INV", L"INV-98", hr);
	return hr;
}

HRESULT FileDragDropExt::InvokeCommandImpl(LPCMINVOKECOMMANDINFO pCmdInfo, ShellLogger& log)
{
	if (!pCmdInfo)
	{
		SHELL_LOG(log, 1, L"INV", L"INV-02", L"pCmdInfo is null");
		return E_INVALIDARG;
	}

	if (0 != HIWORD(pCmdInfo->lpVerb))
	{
		SHELL_LOG(log, 1, L"INV", L"INV-03", L"lpVerb is a string");
		return E_INVALIDARG;
	}

	if (!m_shouldHandle)
	{
		SHELL_LOG(log, 2, L"INV", L"INV-04", L"Handler disabled");
		return E_FAIL;
	}

	UINT idOffset = LOWORD(pCmdInfo->lpVerb);
	SHELL_LOG(log, 2, L"INV", L"INV-05", L"idOffset=%u IntegrationMode=%s Default=%d",
		idOffset, m_integrationMode, m_isDefaultHandler ? 1 : 0);

	HKEY hKey = nullptr;
	LONG lResult = RegOpenKeyEx(HKEY_CURRENT_USER, TEXT("SOFTWARE\\Eneleich\\NeathCopy"), 0, KEY_READ, &hKey);
	if (lResult != ERROR_SUCCESS)
	{
		SHELL_LOG(log, 1, L"INV", L"INV-06", L"RegOpenKeyEx failed error=%ld", lResult);
	}
	else
	{
		LONG resInstall = RegQueryValueEx(hKey, L"InstallDir", NULL, NULL, (LPBYTE)dir, &dirSize);
		LONG resLogs = RegQueryValueEx(hKey, L"LogsDir", NULL, NULL, (LPBYTE)logs, &logsSize);
		LONG resList = RegQueryValueEx(hKey, L"FilesList", NULL, NULL, (LPBYTE)list, &listSize);
		LONG resApp = RegQueryValueEx(hKey, L"CopyHandler", NULL, NULL, (LPBYTE)app, &appSize);

		SHELL_LOG(log, 2, L"INV", L"INV-07", L"InstallDir result=%ld value=%s", resInstall, dir);
		SHELL_LOG(log, 2, L"INV", L"INV-08", L"LogsDir result=%ld value=%s", resLogs, logs);
		SHELL_LOG(log, 2, L"INV", L"INV-09", L"FilesList result=%ld value=%s", resList, list);
		SHELL_LOG(log, 2, L"INV", L"INV-10", L"CopyHandler result=%ld value=%s", resApp, app);

		RegCloseKey(hKey);
	}

	STARTUPINFO si;
	PROCESS_INFORMATION pi;
	ZeroMemory(&si, sizeof(si));
	si.cb = sizeof(si);
	ZeroMemory(&pi, sizeof(pi));

	wchar_t program[MAX_PATH_EX] = { 0 };
	wchar_t filesListPath[MAX_PATH_EX] = { 0 };
	wchar_t args[MAX_PATH_EX] = { 0 };
	wchar_t destiny[MAX_PATH_EX] = { 0 };

	wcscpy_s(program, _countof(program), app);
	wcscpy_s(filesListPath, _countof(filesListPath), list);
	wcscpy_s(destiny, _countof(destiny), m_szTargetDir);

	if (program[0] == L'\0')
	{
		SHELL_LOG(log, 1, L"INV", L"INV-10", L"CopyHandler path missing");
		return E_FAIL;
	}

	if (filesListPath[0] == L'\0')
	{
		SHELL_LOG(log, 1, L"INV", L"INV-10A", L"FilesList path missing");
		return E_FAIL;
	}

	SHELL_LOG(log, 2, L"INV", L"INV-10B", L"Destination=%s SourcesJsonLen=%u", destiny, (UINT)wcslen(m_sourcesJson));

	switch (idOffset)
	{
	case 0:
		{
			if (_wcsicmp(m_integrationMode, L"LegacyFileQueue") != 0)
			{
				wchar_t json[MAX_WPATH] = { 0 };
				bool built = BuildJsonMessage(L"Copy", m_sourcesJson, destiny, json, _countof(json), &log, L"INV-11B");
				SHELL_LOG(log, 2, L"INV", L"INV-11", L"BuildJsonMessage Copy built=%d length=%u", built ? 1 : 0, (UINT)wcslen(json));
				bool sent = built && SendPipeMessage(json, 2000, &log, L"INV-12");
				if (!sent)
					SHELL_LOG(log, 1, L"INV", L"INV-13", L"Pipe send failed for Copy");

				return S_OK;
			}

			HRESULT argHr = StringCchPrintfW(args, _countof(args), L" copy \"*%s\" \"%s\"", filesListPath, destiny);
			if (FAILED(argHr))
			{
				SHELL_LOG(log, 1, L"INV", L"INV-14A", L"Failed to build args hr=0x%08X", argHr);
				return E_FAIL;
			}
			BOOL created = CreateProcessW(program, args, 0, 0, TRUE, CREATE_DEFAULT_ERROR_MODE, 0, 0, &si, &pi);
			if (!created)
			{
				SHELL_LOG(log, 1, L"INV", L"INV-14", L"CreateProcess failed error=%lu", GetLastError());
			}
			else
			{
				SHELL_LOG(log, 2, L"INV", L"INV-15", L"CreateProcess success");
				CloseHandle(pi.hProcess);
				CloseHandle(pi.hThread);
			}
			return S_OK;
		}

	case 1:
		{
			if (_wcsicmp(m_integrationMode, L"LegacyFileQueue") != 0)
			{
				wchar_t json[MAX_WPATH] = { 0 };
				bool built = BuildJsonMessage(L"Move", m_sourcesJson, destiny, json, _countof(json), &log, L"INV-21B");
				SHELL_LOG(log, 2, L"INV", L"INV-21", L"BuildJsonMessage Move built=%d length=%u", built ? 1 : 0, (UINT)wcslen(json));
				bool sent = built && SendPipeMessage(json, 2000, &log, L"INV-22");
				if (!sent)
					SHELL_LOG(log, 1, L"INV", L"INV-23", L"Pipe send failed for Move");

				return S_OK;
			}

			HRESULT argHr = StringCchPrintfW(args, _countof(args), L" move \"*%s\" \"%s\"", filesListPath, destiny);
			if (FAILED(argHr))
			{
				SHELL_LOG(log, 1, L"INV", L"INV-24A", L"Failed to build args hr=0x%08X", argHr);
				return E_FAIL;
			}
			BOOL created = CreateProcessW(program, args, 0, 0, TRUE, CREATE_DEFAULT_ERROR_MODE, 0, 0, &si, &pi);
			if (!created)
			{
				SHELL_LOG(log, 1, L"INV", L"INV-24", L"CreateProcess failed error=%lu", GetLastError());
			}
			else
			{
				SHELL_LOG(log, 2, L"INV", L"INV-25", L"CreateProcess success");
				CloseHandle(pi.hProcess);
				CloseHandle(pi.hThread);
			}
			return S_OK;
		}

	default:
		SHELL_LOG(log, 1, L"INV", L"INV-30", L"Invalid idOffset=%u", idOffset);
		return E_INVALIDARG;
	}
}

HRESULT FileDragDropExt::InvokeCommandSehGuard(LPCMINVOKECOMMANDINFO pCmdInfo, ShellLogger& log)
{
	HRESULT hr = E_FAIL;
	__try
	{
		hr = InvokeCommandImpl(pCmdInfo, log);
	}
	__except (EXCEPTION_EXECUTE_HANDLER)
	{
		DWORD code = GetExceptionCode();
		SHELL_LOG(log, 0, L"INV", L"INV-99", L"SEH exception code=0x%08X", code);
		return E_FAIL;
	}

	return hr;
}



#pragma endregion
