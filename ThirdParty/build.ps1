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
$BinDir = "$PWD\bin"
$BuildDir = "$PWD\build"

# Utility functions
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

function Build($module)
{
	Write-Host "Building $module ... " -foregroundcolor Cyan
	
	# CMake
	$dir = GetConfig "Directory" $module
	$source_path = Join-Path $PWD $dir
	$output_dir = Join-Path $BuildDir $dir
	mkdir $output_dir > $null
	$prev = $PWD
	cd $output_dir
	$prefix="-DCMAKE_INSTALL_PREFIX=$BinDir"
	& $cmake $source_path $prefix -G "Visual Studio 10"
	cd $prev
	if ($LastExitCode -ne 0)
	{
		throw "CMake failed to run"
	}
	
	# DevEnv
	$sln = (Join-Path $output_dir "$dir.sln")
	Write-Host "Running devenv ... " -nonewline
	& $devenv $sln /build "Release|Win32"
	Write-Host "DONE"
	if ($LastExitCode -ne 0) {
		throw "Build failed"
	}
	
	# DevEnv Install
	Write-Host "Running devenv INSTALL ... " -nonewline
	& $devenv $sln /build "Release|Win32" /project INSTALL
	Write-Host "DONE"
	if ($LastExitCode -ne 0) {
		throw "Install failed"
	}	

	Write-Host "Building $module ... DONE" -foregroundcolor Cyan
}

# Clear Bin directory
#CleanDirectory $BinDir
CleanDirectory $BuildDir

# Retrieve configuration file
$XmlConfigPath = "$PWD\BuildConfig.xml"
$XmlConfig = New-Object System.Xml.XmlDocument;
$XmlConfig.Load($XmlConfigPath)
$z7 = GetConfig("Zip7Path")
$cmake = GetConfig("CMakePath")
$devenv = GetConfig("VisualStudio2010")

# Build zlib
#Build zlib
#Build libpng
Build pngcrush
