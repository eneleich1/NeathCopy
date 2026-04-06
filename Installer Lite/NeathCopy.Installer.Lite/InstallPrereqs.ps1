# Downloads and installs .NET Framework 4.8 web bootstrapper and VC++ 2015-2022 x64 when missing.
# Invoked deferred (elevated) by NeathCopy Lite MSI after files are installed. Requires network access.

$ErrorActionPreference = 'Stop'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

function Test-Net48 {
    $k = Get-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full' -ErrorAction SilentlyContinue
    if ($null -eq $k -or $null -eq $k.Release) { return $false }
    return [int]$k.Release -ge 528040
}

function Test-VcRedistX64 {
    $k = Get-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64' -ErrorAction SilentlyContinue
    if ($null -eq $k) { return $false }
    return [int]$k.Installed -eq 1
}

function Test-ExitOk([int]$code) {
    if ($code -eq 0) { return $true }
    if ($code -eq 3010) { return $true }
    if ($code -eq 1641) { return $true }
    return $false
}

$tmp = $env:TEMP

if (-not (Test-Net48)) {
    $url = 'https://go.microsoft.com/fwlink/?linkid=2085155'
    $exe = Join-Path $tmp 'NeathCopy-ndp48-web.exe'
    Invoke-WebRequest -Uri $url -OutFile $exe -UseBasicParsing
    $p = Start-Process -FilePath $exe -ArgumentList '/q', '/norestart' -Wait -PassThru
    Remove-Item $exe -ErrorAction SilentlyContinue
    if (-not (Test-ExitOk $p.ExitCode)) { exit $p.ExitCode }
}

if (-not (Test-VcRedistX64)) {
    $url = 'https://aka.ms/vs/17/release/vc_redist.x64.exe'
    $exe = Join-Path $tmp 'NeathCopy-vc_redist.x64.exe'
    Invoke-WebRequest -Uri $url -OutFile $exe -UseBasicParsing
    $p = Start-Process -FilePath $exe -ArgumentList '/install', '/quiet', '/norestart' -Wait -PassThru
    Remove-Item $exe -ErrorAction SilentlyContinue
    if (-not (Test-ExitOk $p.ExitCode)) { exit $p.ExitCode }
}

exit 0
