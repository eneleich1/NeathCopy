#include "ShellLogger.h"

#include <strsafe.h>
#include <shlobj.h>

namespace
{
    const wchar_t* kRegPath = L"SOFTWARE\\Eneleich\\NeathCopy";
    const wchar_t* kDefaultLogFile = L"shell-ext.log";

    struct ShellLoggerConfig
    {
        bool initialized;
        bool debugEnabled;
        int verbosity;
        ShellLogMode mode;
        bool msgBoxEnabled;
        bool eventLogEnabled;
        wchar_t logPath[MAX_PATH];
    };

    ShellLoggerConfig g_config = { false };
    CRITICAL_SECTION g_logLock;
    INIT_ONCE g_logLockInit = INIT_ONCE_STATIC_INIT;

    BOOL CALLBACK InitLogLock(PINIT_ONCE, PVOID, PVOID*)
    {
        InitializeCriticalSection(&g_logLock);
        return TRUE;
    }

    void EnsureLogLock()
    {
        InitOnceExecuteOnce(&g_logLockInit, InitLogLock, nullptr, nullptr);
    }

    bool ReadRegStringValue(HKEY root, const wchar_t* name, wchar_t* buffer, DWORD bufferChars)
    {
        if (!buffer || bufferChars == 0)
            return false;

        buffer[0] = L'\0';

        HKEY key = nullptr;
        if (RegOpenKeyExW(root, kRegPath, 0, KEY_READ, &key) != ERROR_SUCCESS)
            return false;

        DWORD type = 0;
        DWORD sizeBytes = bufferChars * sizeof(wchar_t);
        LONG result = RegQueryValueExW(key, name, nullptr, &type, reinterpret_cast<LPBYTE>(buffer), &sizeBytes);
        RegCloseKey(key);

        if (result != ERROR_SUCCESS || (type != REG_SZ && type != REG_EXPAND_SZ))
            return false;

        buffer[bufferChars - 1] = L'\0';
        return true;
    }

    bool ReadRegDwordValue(HKEY root, const wchar_t* name, DWORD& value)
    {
        HKEY key = nullptr;
        if (RegOpenKeyExW(root, kRegPath, 0, KEY_READ, &key) != ERROR_SUCCESS)
            return false;

        DWORD type = 0;
        DWORD sizeBytes = sizeof(DWORD);
        DWORD data = 0;
        LONG result = RegQueryValueExW(key, name, nullptr, &type, reinterpret_cast<LPBYTE>(&data), &sizeBytes);
        RegCloseKey(key);

        if (result != ERROR_SUCCESS)
            return false;

        if (type == REG_DWORD)
        {
            value = data;
            return true;
        }

        if (type == REG_SZ || type == REG_EXPAND_SZ)
        {
            wchar_t text[32] = { 0 };
            DWORD size = sizeof(text);
            if (RegOpenKeyExW(root, kRegPath, 0, KEY_READ, &key) == ERROR_SUCCESS)
            {
                LONG res2 = RegQueryValueExW(key, name, nullptr, &type, reinterpret_cast<LPBYTE>(text), &size);
                RegCloseKey(key);
                if (res2 == ERROR_SUCCESS)
                {
                    value = wcstoul(text, nullptr, 10);
                    return true;
                }
            }
        }

        return false;
    }

    bool IsTrueValue(const wchar_t* value)
    {
        if (!value || value[0] == L'\0')
            return false;

        if (wcscmp(value, L"1") == 0)
            return true;

        if (_wcsicmp(value, L"true") == 0)
            return true;

        return false;
    }

    void EnsureDirectoryForFile(const wchar_t* filePath)
    {
        if (!filePath || filePath[0] == L'\0')
            return;

        const wchar_t* lastSlash = wcsrchr(filePath, L'\\');
        const wchar_t* lastSlash2 = wcsrchr(filePath, L'/');
        if (lastSlash2 && (!lastSlash || lastSlash2 > lastSlash))
            lastSlash = lastSlash2;

        if (!lastSlash)
            return;

        size_t dirLen = static_cast<size_t>(lastSlash - filePath);
        if (dirLen == 0 || dirLen >= MAX_PATH)
            return;

        wchar_t dir[MAX_PATH] = { 0 };
        wcsncpy_s(dir, _countof(dir), filePath, dirLen);
        CreateDirectoryW(dir, nullptr);
    }

    bool GetLocalAppDataLogPath(wchar_t* outPath, size_t outCount)
    {
        if (!outPath || outCount == 0)
            return false;

        outPath[0] = L'\0';

        wchar_t* path = nullptr;
        if (FAILED(SHGetKnownFolderPath(FOLDERID_LocalAppData, 0, nullptr, &path)))
            return false;

        HRESULT hr = StringCchCopyW(outPath, outCount, path);
        CoTaskMemFree(path);
        if (FAILED(hr))
            return false;

        if (FAILED(StringCchCatW(outPath, outCount, L"\\Eneleich\\NeathCopy\\logs\\")))
            return false;

        if (FAILED(StringCchCatW(outPath, outCount, kDefaultLogFile)))
            return false;

        return true;
    }

    bool TryBuildLogPath(wchar_t* outPath, size_t outCount)
    {
        wchar_t logsDir[MAX_PATH] = { 0 };
        if (ReadRegStringValue(HKEY_CURRENT_USER, L"LogsDir", logsDir, _countof(logsDir)) && logsDir[0] != L'\0')
        {
            HRESULT hr = StringCchCopyW(outPath, outCount, logsDir);
            if (SUCCEEDED(hr))
            {
                size_t len = wcslen(outPath);
                if (len > 0 && outPath[len - 1] != L'\\' && outPath[len - 1] != L'/')
                    StringCchCatW(outPath, outCount, L"\\");
                StringCchCatW(outPath, outCount, kDefaultLogFile);
                return true;
            }
        }

        return GetLocalAppDataLogPath(outPath, outCount);
    }

    bool CanOpenLogFile(const wchar_t* path)
    {
        if (!path || path[0] == L'\0')
            return false;

        EnsureDirectoryForFile(path);

        HANDLE file = CreateFileW(path, FILE_APPEND_DATA, FILE_SHARE_READ, nullptr, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr);
        if (file == INVALID_HANDLE_VALUE)
            return false;

        CloseHandle(file);
        return true;
    }

    void LoadConfig()
    {
        g_config.initialized = true;
        g_config.debugEnabled = false;
        g_config.verbosity = 0;
        g_config.mode = ShellLogMode::File;
        g_config.msgBoxEnabled = false;
        g_config.eventLogEnabled = false;
        g_config.logPath[0] = L'\0';

        DWORD debugValue = 0;
        if (ReadRegDwordValue(HKEY_CURRENT_USER, L"ShellDebug", debugValue))
            g_config.debugEnabled = (debugValue != 0);
        else
        {
            wchar_t debugText[16] = { 0 };
            if (ReadRegStringValue(HKEY_CURRENT_USER, L"ShellDebug", debugText, _countof(debugText)))
                g_config.debugEnabled = IsTrueValue(debugText);
        }

        DWORD verbosity = 2;
        if (ReadRegDwordValue(HKEY_CURRENT_USER, L"ShellDebugVerbosity", verbosity))
            g_config.verbosity = static_cast<int>(verbosity);
        else
            g_config.verbosity = 2;

        if (g_config.verbosity < 0) g_config.verbosity = 0;
        if (g_config.verbosity > 3) g_config.verbosity = 3;

        wchar_t modeValue[32] = { 0 };
        if (ReadRegStringValue(HKEY_CURRENT_USER, L"ShellDebugMode", modeValue, _countof(modeValue)))
        {
            if (_wcsicmp(modeValue, L"debugview") == 0)
                g_config.mode = ShellLogMode::DebugView;
            else if (_wcsicmp(modeValue, L"msgbox") == 0)
                g_config.mode = ShellLogMode::MsgBox;
            else
                g_config.mode = ShellLogMode::File;
        }

        DWORD msgBox = 0;
        if (ReadRegDwordValue(HKEY_CURRENT_USER, L"ShellDebugMsgBox", msgBox))
            g_config.msgBoxEnabled = (msgBox != 0);
        else
        {
            wchar_t msgBoxText[16] = { 0 };
            if (ReadRegStringValue(HKEY_CURRENT_USER, L"ShellDebugMsgBox", msgBoxText, _countof(msgBoxText)))
                g_config.msgBoxEnabled = IsTrueValue(msgBoxText);
        }

        DWORD eventLog = 0;
        if (ReadRegDwordValue(HKEY_CURRENT_USER, L"ShellDebugEventLog", eventLog))
            g_config.eventLogEnabled = (eventLog != 0);
        else
        {
            wchar_t eventLogText[16] = { 0 };
            if (ReadRegStringValue(HKEY_CURRENT_USER, L"ShellDebugEventLog", eventLogText, _countof(eventLogText)))
                g_config.eventLogEnabled = IsTrueValue(eventLogText);
            else
                g_config.eventLogEnabled = g_config.debugEnabled;
        }

        if (!TryBuildLogPath(g_config.logPath, _countof(g_config.logPath)))
            g_config.logPath[0] = L'\0';
        else if (!CanOpenLogFile(g_config.logPath))
        {
            if (!GetLocalAppDataLogPath(g_config.logPath, _countof(g_config.logPath)) || !CanOpenLogFile(g_config.logPath))
                g_config.logPath[0] = L'\0';
        }
    }

    void WriteLine(const wchar_t* line)
    {
        if (!line || line[0] == L'\0')
            return;

        __try
        {
            EnsureLogLock();

            EnterCriticalSection(&g_logLock);

            if (g_config.logPath[0] == L'\0')
            {
                LeaveCriticalSection(&g_logLock);
                return;
            }

            EnsureDirectoryForFile(g_config.logPath);

            HANDLE file = CreateFileW(g_config.logPath, FILE_APPEND_DATA, FILE_SHARE_READ, nullptr, OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, nullptr);
            if (file != INVALID_HANDLE_VALUE)
            {
                DWORD written = 0;
                WriteFile(file, line, static_cast<DWORD>(wcslen(line) * sizeof(wchar_t)), &written, nullptr);
                CloseHandle(file);
            }

            LeaveCriticalSection(&g_logLock);
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
        }
    }
}

ShellLogger::ShellLogger() : m_msgBoxShown(false)
{
}

void ShellLogger::ResetMsgBox()
{
    m_msgBoxShown = false;
}

bool ShellLogger::IsDebugEnabled() const
{
    return g_config.debugEnabled;
}

int ShellLogger::Verbosity() const
{
    return g_config.verbosity;
}

bool ShellLogger::IsMsgBoxEnabled() const
{
    return g_config.mode == ShellLogMode::MsgBox && g_config.msgBoxEnabled;
}

bool ShellLogger::IsEventLogEnabled() const
{
    return g_config.eventLogEnabled;
}

void ShellLogger::EnsureConfig()
{
    if (!g_config.initialized)
        LoadConfig();
}

void ShellLogger::Log(int level, const wchar_t* component, const wchar_t* stepId, const wchar_t* format, ...)
{
    EnsureConfig();

    int maxLevel = g_config.debugEnabled ? g_config.verbosity : 0;
    if (level > maxLevel)
        return;

    wchar_t message[1024] = { 0 };
    if (format && format[0] != L'\0')
    {
        va_list args;
        va_start(args, format);
        _vsnwprintf_s(message, _countof(message), _TRUNCATE, format, args);
        va_end(args);
    }

    LogInternal(level, component, stepId, message, true);
}

void ShellLogger::LogInternal(int level, const wchar_t* component, const wchar_t* stepId, const wchar_t* message, bool allowMsgBox)
{
    SYSTEMTIME st;
    GetLocalTime(&st);

    DWORD pid = GetCurrentProcessId();
    DWORD tid = GetCurrentThreadId();

    wchar_t line[1400] = { 0 };
    StringCchPrintfW(line, _countof(line), L"%04d-%02d-%02d %02d:%02d:%02d.%03d [pid:%lu tid:%lu] [L%d] [%s] [%s] %s\r\n",
        st.wYear, st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond, st.wMilliseconds,
        pid, tid, level,
        component ? component : L"?",
        stepId ? stepId : L"?",
        message ? message : L"");

    WriteLine(line);

    if (g_config.mode == ShellLogMode::DebugView)
        OutputDebugStringW(line);

    if (IsEventLogEnabled())
    {
        WORD eventType = EVENTLOG_INFORMATION_TYPE;
        if (level >= 3)
            eventType = EVENTLOG_ERROR_TYPE;
        else if (level >= 2)
            eventType = EVENTLOG_WARNING_TYPE;

        LogEventViewer(eventType, line);
    }

    if (allowMsgBox && IsMsgBoxEnabled() && !m_msgBoxShown)
    {
        m_msgBoxShown = true;
        MessageBoxW(nullptr, line, L"NeathCopyShell", MB_OK | MB_ICONINFORMATION);
    }
}

void ShellLogger::LogEventViewer(WORD eventType, const wchar_t* message)
{
    if (!message || message[0] == L'\0')
        return;

    __try
    {
        HANDLE eventSource = RegisterEventSourceW(nullptr, L"NeathCopyShell");
        if (!eventSource)
            return;

        LPCWSTR strings[1] = { message };
        ReportEventW(eventSource, eventType, 0, 0x1000, nullptr, 1, 0, strings, nullptr);
        DeregisterEventSource(eventSource);
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
    }
}
