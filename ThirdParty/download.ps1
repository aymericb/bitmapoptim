#
# BitmapOptim. The GUI frontend for lossless image file size optimization
# Copyright (C) 2012 - Aymeric Barthe
#
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program.  If not, see <http://www.gnu.org/licenses/>.
#

# Enable errors
trap {
	break;
}

# Constants
$TempDir = "$PWD\Temp"
$BuildExtraDir = "$PWD\_Build"
$PatchFilename = "patch.ps1"

# Utility functions
function DownloadFile($url, $output_path)
{
	Write-Host "Downloading $url ... " -nonewline
	$web_client = New-Object System.Net.WebClient
	$web_client.DownloadFile($url, $output_path)
	Write-Host "DONE"
}
function GetConfig($param, $module=$null)
{
	$node = $null
	if ($module) {
		$node = $XmlConfig.SelectSingleNode("/Config/Module[@name='$module']/"+$param)
	} else {
		$node = $XmlConfig.SelectSingleNode("/Config/"+$param)
	}
	if (!$node) {
		throw "Could not find config parameter $param"
	}
	return $node.get_InnerText()
}

function CleanDirectory($path)
{
	if (Test-Path $path)
	{
		Remove-Item -Recurse $path -ErrorAction Stop -Force
	}
	New-Item $path -type directory > $null
}

function ExtractFile($path, $output_dir, $postprocess=$false)
{
	Write-Host "Extracting $path ... " -nonewline
	$output_log = Join-Path $TempDir output.txt
	& $z7 x $path "-o$output_dir" -y > $output_log
	if ($LastExitCode -eq 0) {
		Write-Host "DONE"
	} else {
		Get-Content $output_log
		throw "7zip failed with error $LastExitCode"
	}
	if ($postprocess)
	{
		Write-Host "Moving files to $output_dir ... " -nonewline
		$count = 0
		$found_dir = ""
		Get-ChildItem $output_dir | %{ $count=$count+1; $found_dir = $_.FullName }
		if ($count -eq 1)
		{
			Move-Item $found_dir\* $output_dir  -Force
			Remove-Item $found_dir\*.suo -Force # Workaround weird bug
			Remove-Item $found_dir -Force
		}
		Write-Host "DONE"
	}
}

function DownloadThirdparty($module)
{
	Write-Host -foregroundcolor cyan "Processing $module..."

	# Parse configuration file
	$dir = GetConfig "Directory" $module
	$url = GetConfig "URL" $module

	# Download file
	$filename = $url.SubString($url.LastIndexOf("/")+1)
	$download_path = Join-Path $TempDir $filename
	DownloadFile $url $download_path
	
	# Erase output dir
	$output_dir = Join-Path $PWD $dir
	if (Test-Path $output_dir)
	{
		Write-Host "Erasing output directory $output_dir ... " -nonewline
		Remove-Item -Recurse $output_dir -ErrorAction Stop -Force
		Write-Host "DONE"
	}
	
	# Extract file
	if ($download_path.EndsWith(".tar.bz2") -or $download_path.EndsWith(".tar.gz"))
	{
		ExtractFile $download_path $TempDir
		$download_path = $download_path.SubString(0, $download_path.LastIndexOf("."))
	}	
	ExtractFile $download_path $output_dir $true
	
	# Add extra files
	$extra_dir = Join-Path $BuildExtraDir $dir
	if (Test-Path $extra_dir)
	{
		Get-ChildItem $extra_dir | %{
			$source_path = $_.FullName
			$source_filename = $source_path.SubString($source_path.LastIndexOf("\")+1)
			if ($source_filename -ne $PatchFileName) {
				$dest_path = Join-Path $output_dir $source_filename
				Copy-Item $source_path $dest_path
			}
		}
		$patch_file = Join-Path $extra_dir $PatchFileName
		if (Test-Path $patch_file) {
			& $patch_file $output_dir
		}		
	}

	Write-Host -foregroundcolor cyan "DONE $module"
}

# Clear Temp directory
CleanDirectory $TempDir

# Retrieve configuration file
$XmlConfigPath = "$PWD\BuildConfig.xml"
$XmlConfig = New-Object System.Xml.XmlDocument;
$XmlConfig.Load($XmlConfigPath)
$z7 = GetConfig("Zip7Path")
$cmake = GetConfig("CMakePath")

DownloadThirdparty zlib
DownloadThirdparty libpng
DownloadThirdparty pngcrush
DownloadThirdparty ObjectListView
