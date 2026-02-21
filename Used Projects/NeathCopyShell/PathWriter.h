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
		outFile = nullptr;
		buf = nullptr;
		Reset();
	}
	~PathWriter() {
		delete[] buf;
		buf = nullptr;
	}

	//Methods
	bool OpenFile(char* access)
	{
		//Open the File
		outFile = nullptr;
		if (fopen_s(&outFile, fileListPath, access) != 0)
			outFile = nullptr;
		return outFile != nullptr;
	}

	void CloseFile()
	{
		if (outFile)
		{
			fclose(outFile);
			outFile = nullptr;
		}
	}

	bool ClearFilesList()
	{
		if (!OpenFile("w+,ccs=UNICODE"))
			return false;

		if (outFile)
			fwrite(L"|", wcslen(L"|") * sizeof(wchar_t), 1, outFile);

		CloseFile();
		return true;
	}

	void RegisterPath(wchar_t* path)
	{
		//Write path or store in the buffer
		if (wcslen(buf) + wcslen(path) > capacity)
		{
			if (outFile)
				fwrite(buf, wcslen(buf) * sizeof(wchar_t), 1, outFile);

			Reset();

			wcscat_s(buf, capacity, path);
		}
		else
			wcscat_s(buf, capacity, path);

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
			if (outFile)
				fwrite(buf, wcslen(buf) * sizeof(wchar_t), 1, outFile);
		}
	}

private:
	void Reset() {
		if (buf)
		{
			delete[] buf;
			buf = nullptr;
		}
		buf = new wchar_t[capacity];
		if (buf)
			buf[0] = L'\0';
	}
};
