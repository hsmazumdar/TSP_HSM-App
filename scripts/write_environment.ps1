param(
    [Parameter(Mandatory = $true)][string]$OutFile,
    [string]$DllPath = ""
)

$ErrorActionPreference = "Stop"
$cs = Get-CimInstance Win32_Processor | Select-Object -First 1
$os = Get-CimInstance Win32_OperatingSystem
$dllInfo = $null
if ($DllPath -and (Test-Path $DllPath)) {
    $hash = (Get-FileHash $DllPath -Algorithm SHA256).Hash
    $fi = Get-Item $DllPath
    $dllInfo = @{
        filename = $fi.Name
        path = $fi.FullName
        size_bytes = $fi.Length
        sha256 = $hash
    }
}

$git = $null
try {
    $git = (git -C (Split-Path -Parent (Split-Path -Parent $OutFile)) rev-parse HEAD 2>$null)
} catch { }

$obj = [ordered]@{
    timestamp_utc = (Get-Date).ToUniversalTime().ToString("o")
    windows_version = [System.Environment]::OSVersion.VersionString
    cpu_name = $cs.Name
    logical_processors = [System.Environment]::ProcessorCount
    installed_ram_gb = [math]::Round($os.TotalVisibleMemorySize / 1MB, 2)
    dotnet = (dotnet --version 2>$null)
    process_architecture = [System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString()
    solver_dll = $dllInfo
    git_commit = $git
    notes = @(
        "Paper principal hardware: Intel Core i9-13900K, 32 GB DDR5, Windows 10, .NET 8 — reviewer machines may differ.",
        "Seed control is not exposed by TSP_CS.Core.dll; runs are intentionally non-deterministic."
    )
}

$dir = Split-Path -Parent $OutFile
if ($dir) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
($obj | ConvertTo-Json -Depth 6) | Set-Content -Path $OutFile -Encoding UTF8
Write-Host "Wrote $OutFile"
