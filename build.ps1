$ErrorActionPreference = 'Stop'

$ProjectDir = Join-Path $PSScriptRoot "src\FancyZonesHotkeys"
$ProjectFile = Join-Path $ProjectDir "FancyZonesHotkeys.csproj"
$PublishDir = Join-Path $PSScriptRoot "dist"

if (Test-Path $PublishDir) {
    Remove-Item -Recurse -Force $PublishDir
}
New-Item -ItemType Directory -Path $PublishDir | Out-Null

Write-Host "Publishing FancyZonesHotkeys (v2.0.0, .NET 10.0, Self-Contained, Trimmed, Single-File) ..."

C:\Users\jsp0\AppData\Local\Microsoft\dotnet\dotnet.exe publish $ProjectFile -c Release -r win-x64 --self-contained true -o $PublishDir

Write-Host "Copying preset.yaml to output directory..."
Copy-Item (Join-Path $PSScriptRoot "preset.yaml") $PublishDir

Write-Host "Build finished. Output in '$PublishDir'."
