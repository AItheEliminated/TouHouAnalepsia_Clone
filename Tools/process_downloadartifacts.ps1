$ErrorActionPreference = "Stop"
$download_manifest = "https://fs.hinageshi.org/touhouanalepsia/manifest.txt" + "?nocache=" + [String][DateTime]::Now.Ticks.ToString()
$download_archive = "https://fs.hinageshi.org/touhouanalepsia/artifacts.zip" + "?nocache=" + [String][DateTime]::Now.Ticks.ToString()
$noPause = $false
if ($args[0] -eq "NOPAUSE") {
	$noPause = $true
}

# if the script is running in the root folder of mod
if (Test-Path -Path ".\ModuleData") {
	if ((Test-Path -Path ".\bin") -eq $false) {
		Write-Host "Creating bin folder..." -ForegroundColor Yellow
		New-Item -ItemType Directory -Path ".\bin"
	}
	cd -Path "bin"
} # if the script is running in the sub-folder of mod
elseif (Test-Path -Path ".\..\ModuleData") {
	if ((Test-Path -Path ".\..\bin") -eq $false) {
		Write-Host "Creating bin folder..." -ForegroundColor Yellow
		New-Item -ItemType Directory -Path ".\..\bin"
	}
	cd -Path "..\bin"
} else {
	Write-Host "Wrong work directory. This script should be either running in the root folder of mod, or 'Tools' folder." -ForegroundColor Red
	if ($noPause -eq $false) {
		[void][System.Console]::ReadKey($true)
	}
	return
}

$location = Get-Location
$update = $false

try
{
	Write-Host "Checking the latest artifacts..." -ForegroundColor Yellow
	$result = (New-Object Net.WebClient).DownloadString($download_manifest) | Select-String -Pattern '(\w+)\s+(.+)' -AllMatches
}
catch [System.Net.WebException]
{
	Write-Host "Unable to download the manifest." -ForegroundColor Red
	if ($noPause -eq $false) {
		[void][System.Console]::ReadKey($true)
	}
	return
}

for ($i=0; $i -lt $result.Matches.Length; $i++){
	$item = $result.Matches[$i]
	$sha1 = $item.Groups[1].Value
	$file = $item.Groups[2].Value
	if ((Test-Path -Path $file) -eq $false) {
		Write-Host "File $file is missing." -ForegroundColor Magenta
		$update = $true
		break
	}
	if ((Get-FileHash $file -Algorithm SHA1).Hash -ne $sha1) {
		Write-Host "File $file is out-of-date." -ForegroundColor Magenta
		$update = $true
		break
	}
}

if ($update) {
	$stage = 0
	Write-Host "Preparing to update binary files..." -ForegroundColor Yellow
	try
	{
		Write-Host "Downloading the artifacts..." -ForegroundColor Yellow
		(New-Object System.Net.WebClient).DownloadFile($download_archive, $location.ToString() + "\artifacts.zip")
		
		Write-Host "Clearing bin folder..." -ForegroundColor Yellow
		Get-ChildItem * -Include *.dll -Recurse | Remove-Item
		
		Write-Host "Unziping the archive..." -ForegroundColor Yellow
		$stage = 1
		Expand-Archive -Path ".\artifacts.zip" -DestinationPath ".\"
		Remove-Item ".\artifacts.zip"
	}
	catch [System.Net.WebException]
	{
		Write-Host "Unable to download the archive." -ForegroundColor Red
		if ($noPause -eq $false) {
			[void][System.Console]::ReadKey($true)
		}
		return
	}
	catch [System.IO.IOException]
	{
		if ($stage -eq 0) {
			Write-Host "Unable to clear bin folder. Have you closed the game?" -ForegroundColor Red
		} else {
			Write-Host "Unable to unzip the archive." -ForegroundColor Red
		}
		if ($noPause -eq $false) {
			[void][System.Console]::ReadKey($true)
		}
		return
	}
	Write-Host "Update completed." -ForegroundColor Green
} else {
	Write-Host "All binary files are up-to-date." -ForegroundColor Green
}

if ($noPause -eq $false) {
	Start-Sleep -Seconds 2
}
