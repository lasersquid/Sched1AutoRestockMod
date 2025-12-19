
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

if ("$proj" -eq "") {
    Write-Output 'Specify "-proj <projectname>""!'
    Exit -1
}

$arch_lower = "$arch".ToLower()
$dll_file = "$($proj)$($arch).dll"
$zip_file = "$($proj)_$($arch)-$($ver).zip"
$pkg_base = "package\thunderstore\$($arch_lower)"

# Clean and create directory structure
Remove-Item -Recurse -ErrorAction Ignore "$($pkg_base)"
Remove-Item -ErrorAction Ignore "$($pkg_base)\..\$($zip_file)"
mkdir "$($pkg_base)\Mods"

# Copy the files
Copy "bin\$($arch)\$($net_ver)\$($dll_file)" "$($pkg_base)\Mods"
Copy 'package_resources\icon.png' "$($pkg_base)\icon.png"
Copy 'package_resources\README.md' "$($pkg_base)\README.md"
Copy 'package_resources\manifest.json' "$($pkg_base)\manifest.json"
if (Test-Path -Path 'package_resources\extras') {
    Copy 'package_resources\extras\*' "$($pkg_base)\Mods"
}

# Set version and arch strings
$json = [System.IO.File]::ReadAllText("$($pkg_base)\manifest.json")
$json = $json.Replace('%%VERSION%%', $ver)
$json = $json.Replace('%%ARCH%%', $arch)
[System.IO.File]::WriteAllText("$($pkg_base)\manifest.json", $json)

# Zip it all up
Compress-Archive -Path "$($pkg_base)\*" -DestinationPath "$($pkg_base)\..\$($zip_file)"
