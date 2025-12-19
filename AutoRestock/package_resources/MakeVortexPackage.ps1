
param (
    [string]$ver = "1.0.2",
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

$arch_lower = "$arch".ToLower()
$zip_file = "$($proj)_$($arch)-$($ver).zip"
$dll_file = "$($proj)$($arch).dll"
$pkg_base = "package\vortex\$($arch_lower)"

# Clean and create directory structure
Remove-Item -Recurse -ErrorAction Ignore "$($pkg_base)"
Remove-Item -ErrorAction Ignore "$($pkg_base)\..\$($zip_file)"
mkdir "$($pkg_base)\mods"

# Copy the files
Copy "bin\$($arch)\$($net_ver)\$($dll_file)" "$($pkg_base)\mods"
if (Test-Path -Path 'package_resources\extras') {
    Copy 'package_resources\extras\*' "$($pkg_base)\Mods"
}

# Zip it all up
Compress-Archive -Path "$($pkg_base)\*" -DestinationPath "$($pkg_base)\..\$($zip_file)"
