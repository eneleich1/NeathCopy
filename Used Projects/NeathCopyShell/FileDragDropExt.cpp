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

#include <comdef.h>
#include "atlstr.h" 


#define MAX_WPATH		32000
#define MAX_PATH_EX		1024*8

#pragma comment(lib, "shlwapi.lib")

extern long g_cDllRef;


#define IDM_RUNFROMHERE            0  // The command's identifier offset

FileDragDropExt::FileDragDropExt() : m_cRef(1),
m_pszMenuText(L"Don't move") {
	InterlockedIncrement(&g_cDllRef);
}


FileDragDropExt::~FileDragDropExt() {
	InterlockedDecrement(&g_cDllRef);
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

static _bstr_t sources;
static int fastmove;

WCHAR dir[260];
DWORD dirSize = sizeof(dir);

WCHAR logs[260];
DWORD logsSize = sizeof(dir);

static WCHAR list[260];
static DWORD listSize = sizeof(dir);

WCHAR app[260];
DWORD appSize = sizeof(dir);

BOOL FastMove(LPCTSTR source, LPCTSTR destiny)
{
	_bstr_t rootSource("");
	GetVolumePathName(source, rootSource, 4);

	_bstr_t rootDestiny("");
	GetVolumePathName(destiny, rootDestiny, 4);

	return StrCmp(rootSource, rootDestiny) == 0;
}

// Initialize the drag and drop handler.  If we return S_OK, then QueryContextMenu will be called. 
// Otherwise this handler will not be used in this event.
IFACEMETHODIMP FileDragDropExt::Initialize(LPCITEMIDLIST pidlFolder, LPDATAOBJECT pDataObj, HKEY hKeyProgID)
{
	//Testing????????????????????????????????????????????????????????????????????????????????????????????????????????????????/
	//MessageBox(0, L"Initialize Method", L"Testing", MB_ICONERROR);

	WCHAR	path[MAX_PATH_EX];
	wchar_t* auxPath = new wchar_t[MAX_PATH_EX + 1];
	wcscpy_s(auxPath, 1, L"");
	BOOL fastMoveSeted = FALSE;

	// Get the directory where the file is dropped to.
	if (!SHGetPathFromIDList(pidlFolder, this->m_szTargetDir)) {
		return E_FAIL;
	}

	//Testing????????????????????????????????????????????????????????????????????????????????????????????????????????????????/
	//MessageBox(0, m_szTargetDir, L"Destiny", MB_ICONERROR);

	// Get the file(s) being dragged.
	if (NULL == pDataObj) {
		return E_INVALIDARG;
	}

	HRESULT hr = E_FAIL;

	FORMATETC fe = { CF_HDROP, NULL, DVASPECT_CONTENT, -1, TYMED_HGLOBAL };
	STGMEDIUM stm;
	

	// The pDataObj pointer contains the objects being acted upon. In this 
	// example, we get an HDROP handle for enumerating the dragged files and 
	// folders.
	if (SUCCEEDED(pDataObj->GetData(&fe, &stm))) {
		// Get an HDROP handle.
		HDROP hDrop = static_cast<HDROP>(GlobalLock(stm.hGlobal));
		if (hDrop != NULL)
		{
			// if there's at least one file or folder, then we should use this handler.
			UINT nFiles = DragQueryFile(hDrop, 0xFFFFFFFF, NULL, 0);

			if (nFiles > 0)
			{
				//Testing????????????????????????????????????????????????????????????????????????????????????????????????????????????????/
				//MessageBox(0, L"nFiles > 0: Initialize Method", L"Testing", MB_ICONERROR);

				//Retrieve the Register Information=================================================================================
				HKEY hKey;
				LONG lResult;

				//Open SubKey
				lResult = RegOpenKeyEx(HKEY_CURRENT_USER, TEXT("SOFTWARE\\Eneleich\\NeathCopy"), 0, KEY_READ, &hKey);

				// Get the data for the specified value name.
				if (lResult == ERROR_SUCCESS)
					RegQueryValueEx(hKey, L"FilesList", NULL, NULL, (LPBYTE)list, &listSize);

				//Testing????????????????????????????????????????????????????????????????????????????????????????????????????????????????/
				//MessageBox(0, list, L"Find FilesList Path in to Registry", MB_ICONERROR);

				_bstr_t destiny(m_szTargetDir);
				_bstr_t filesListPath(list);

				//========Writing Sources==========================
				PathWriter pathWriter(filesListPath);

				pathWriter.ClearFilesList();

				pathWriter.OpenFile("a,ccs=UNICODE");

				for (int i = 0; i < nFiles; i++)
				{
					DragQueryFileW(hDrop, i, path, MAX_PATH_EX);

					wcscat(auxPath, path);
					wcscat(auxPath, L"|");
					pathWriter.RegisterPath(auxPath);
					wcscpy_s(auxPath, 1, L"");

					if (!fastMoveSeted) {
						//Check if has the same root => Fastmove
						_bstr_t source(path);
						fastmove = FastMove(source, destiny);
						fastMoveSeted = TRUE;
					}
				}

				//If UseClipboard =true
				//SetClipboardData()

				pathWriter.WriteBuffer(false);
				pathWriter.CloseFile();

				//Testing????????????????????????????????????????????????????????????????????????????????????????????????????????????????/
				//MessageBox(0, L"FilesList Created:Initialize", L"Testing", MB_ICONERROR);

				//MessageBox(0, L"Finish", L"Timing", MB_ICONERROR);
				//========End Writing Sources======================

				// our QueryContextMenu will be called.
				hr = S_OK;
			}
			GlobalUnlock(stm.hGlobal);
		}

		ReleaseStgMedium(&stm);
	}

	//Free Memory
	delete auxPath;

	// If any value other than S_OK is returned from the method, the QueryContextMenu will not be called.
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
	// If the flags include CMF_DEFAULTONLY then we shouldn't do anything.
	if (uFlags & CMF_DEFAULTONLY)
		return MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_NULL, USHORT(0));

	int cmd = idCmdFirst;

	//Insert Menu for Copy
	InsertMenu(hmenu, indexMenu++, MF_STRING | MF_BYPOSITION, cmd++, L"NeathCopy Here");

	//Insert Menu for Move
	InsertMenu(hmenu, indexMenu++, MF_STRING | MF_BYPOSITION, cmd++, L"NeathMove Here");

	// Set the copy/paste default handler to the menu item create above
	int defItem = GetMenuDefaultItem(hmenu, false, 0);
	if (defItem == 1) // 1: Copy
	{
		SetMenuDefaultItem(hmenu, idCmdFirst + defItem - 1, false);
	}
	else if (defItem == 2) // 2: Move
	{
		//If FastMove them let windows works.
		if (fastmove == 1)
			return MAKE_HRESULT(SEVERITY_SUCCESS, 0, USHORT(0));

		SetMenuDefaultItem(hmenu, idCmdFirst + defItem - 1, false);
	}

	// Return 2 to tell the shell that we added 2 top-level menu items. mMove - mCopy + 1 for multiple
	return MAKE_HRESULT(SEVERITY_SUCCESS, FACILITY_NULL, 2);
	
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
	// If lpVerb really points to a string, ignore this function call and bail out.
	if (0 != HIWORD(pCmdInfo->lpVerb))
		return E_INVALIDARG;

	// The offset will determine which option was requested :
	// 0 for Register, 1 for Unregister
	UINT idOffset = LOWORD(pCmdInfo->lpVerb);

	//Steep1: Retrieve the Register Information
	HKEY hKey;
	LONG lResult;
	
	//Open SubKey
	lResult = RegOpenKeyEx(HKEY_CURRENT_USER, TEXT("SOFTWARE\\Eneleich\\NeathCopy"), 0, KEY_READ, &hKey);

	// Get the data for the specified value name.
	if (lResult == ERROR_SUCCESS)
	{
		RegQueryValueEx(hKey, L"InstallDir", NULL, NULL, (LPBYTE)dir, &dirSize);
		_bstr_t installDir(dir);

		RegQueryValueEx(hKey, L"LogsDir", NULL, NULL, (LPBYTE)logs, &logsSize);
		_bstr_t logsDir(logs);

		RegQueryValueEx(hKey, L"FilesList", NULL, NULL, (LPBYTE)list, &listSize);

		RegQueryValueEx(hKey, L"CopyHandler", NULL, NULL, (LPBYTE)app, &appSize);
	}

	//Steep2: Initialize structs to call NeathCopy Process.
	STARTUPINFO si;
	PROCESS_INFORMATION pi;

	ZeroMemory(&si, sizeof(si));
	si.cb = sizeof(si);
	ZeroMemory(&pi, sizeof(pi));

	//NeathCopy
	_bstr_t program(app);

	//Destiny
	_bstr_t destiny(m_szTargetDir);

	//Args
	_bstr_t args("");

	//Files List Path
	_bstr_t filesListPath(list);

	//Steep3: Check if our menu was selected.

	switch (idOffset)
	{
	case 0://Copy

		//" copy "*sourcesPath" "destiny""
		args += " copy \"*" + filesListPath + "\" \"" + destiny + "\"";
		CreateProcess(program, args, 0, 0, TRUE, CREATE_DEFAULT_ERROR_MODE, 0, 0, &si, &pi);

		return S_OK;

	case 1://Move

		//" move "*sourcesPath" "destiny""
		args += " move \"*" + filesListPath + "\" \"" + destiny + "\"";
		CreateProcess(program, args, 0, 0, TRUE, CREATE_DEFAULT_ERROR_MODE, 0, 0, &si, &pi);

		return S_OK;

	default:

		return E_INVALIDARG;
	}
}



#pragma endregion
