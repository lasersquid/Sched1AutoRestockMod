#!/usr/bin/env pwsh

param (
    [string]$arch = "IL2CPP",
    [string]$proj = "",
    [string]$game_dir = "~/s1"
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

if ("$proj" -eq "") {
    Write-Output 'Specify "-proj <projectname>"!'
    Exit -1
}

$dll_file = "$($proj)$($arch).dll"
$dll_path = Join-Path "bin" $arch $net_ver $dll_file
$mods_path = Join-Path $game_dir "Mods"

Remove-Item -ErrorAction Ignore $(Join-Path $mods_path "$($proj)*.dll")
Copy $dll_path $mods_path
