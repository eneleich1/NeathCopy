#pragma once

#include <windows.h>
#include <stdarg.h>

enum class ShellLogMode
{
    File,
    DebugView,
    MsgBox
};

class ShellLogger
{
public:
    ShellLogger();

    void ResetMsgBox();
    void Log(int level, const wchar_t* component, const wchar_t* stepId, const wchar_t* format, ...);
    bool IsDebugEnabled() const;
    int Verbosity() const;
    bool IsMsgBoxEnabled() const;
    bool IsEventLogEnabled() const;

private:
    void LogInternal(int level, const wchar_t* component, const wchar_t* stepId, const wchar_t* message, bool allowMsgBox);
    void LogEventViewer(WORD eventType, const wchar_t* message);
    void EnsureConfig();

    bool m_msgBoxShown;
};

#define SHELL_LOG(logger, level, component, stepId, fmt, ...) \
    (logger).Log(level, component, stepId, fmt, __VA_ARGS__)

#define SHELL_TRACE_ENTER(logger, component, stepId, fmt, ...) \
    SHELL_LOG(logger, 2, component, stepId, fmt, __VA_ARGS__)

#define SHELL_TRACE_EXIT(logger, component, stepId, hr) \
    SHELL_LOG(logger, 2, component, stepId, L"exit hr=0x%08X", hr)
