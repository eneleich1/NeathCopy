#pragma once
#pragma once
#include "comutil.h" 

class PathWriter
{
	int capacity = 4096;
	wchar_t* buf;
	char* fileListPath;
	FILE* outFile;

public:
	PathWriter(char* fileListPath)
	{
		this->fileListPath = fileListPath;

		Reset();
	}
	~PathWriter() {
		delete buf;
	}

	//Methods
	void OpenFile(char* access)
	{
		//Open the File
		outFile = fopen(fileListPath, access);
	}

	void CloseFile()
	{
		fclose(outFile);
	}

	void ClearFilesList()
	{
		OpenFile("w+,ccs=UNICODE");

		fwrite(L"|", wcslen(L"|") * sizeof(wchar_t), 1, outFile);

		CloseFile();
	}

	void RegisterPath(wchar_t* path)
	{
		//Write path or store in the buffer
		if (wcslen(buf) + wcslen(path) > capacity)
		{
			fwrite(buf, wcslen(buf) * sizeof(wchar_t), 1, outFile);

			Reset();

			wcscat(buf, path);
		}
		else
			wcscat(buf, path);

	}

	void WriteBuffer(bool useClipboard)
	{
		if (useClipboard) 
		{
			const char* output = "Test";
			const size_t len = strlen(output) + 1;
			HGLOBAL hMem = GlobalAlloc(GMEM_MOVEABLE, len);
			memcpy(GlobalLock(hMem), output, len);
			GlobalUnlock(hMem);
			OpenClipboard(0);
			EmptyClipboard();
			SetClipboardData(CF_TEXT, hMem);
			CloseClipboard();
		}
		else
		{
			fwrite(buf, wcslen(buf) * sizeof(wchar_t), 1, outFile);
		}
	}

private:
	void Reset() {
		buf = new wchar_t[capacity];
		wcscpy_s(buf, 1, L"");
	}
};