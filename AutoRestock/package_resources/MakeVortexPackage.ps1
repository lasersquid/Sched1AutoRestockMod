#!/usr/bin/env pwsh

param (
    [string]$ver = "1.0.0",
    [string]$arch = "IL2CPP",
    [string]$proj = ""
 )

 # Check params
 if ("$arch" -eq "IL2CPP") {
    $net_ver = "net6"
}
elseif ("$arch" -eq "Mono") {
    $net_ver = "netstandard2.1"
}
else {
    Write-Output 'Specify "-arch IL2CPP" or "-arch Mono"!'
    Exit -1
}

if ("$($proj)" -eq "") {
    Write-Output 'Specify "-proj <projectname>"!'
    Exit -1
}

$arch_lower = "$($arch)".ToLower()
$zip_file = "$($proj)_$($arch)-$($ver).zip"
$dll_file = "$($proj)$($arch).dll"
$pkg_base = Join-Path "package" "vortex" $arch_lower
$zip_path = Join-Path "package" "vortex" $zip_file
$mods_path = Join-Path $pkg_base "Mods"
$dll_path = Join-Path "bin" $arch $net_ver $dll_file
$extras_path = Join-Path "package_resources" "extras"

# Clean and create directory structure
Remove-Item -Recurse -ErrorAction Ignore $pkg_base
Remove-Item -ErrorAction Ignore $zip_path
New-Item -ItemType Directory -Path $mods_path

# Copy the files
Copy $dll_path $mods_path
if (Test-Path -Path $extras_path) {
    Copy $(Join-Path $extras_path "*") $mods_path
}

# Zip it all up
Compress-Archive -Path $(Join-Path $pkg_base "*") -DestinationPath $zip_path
Exit 0
