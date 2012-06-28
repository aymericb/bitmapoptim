$TargetPath = $Args[0]
Write-Host "Patching $TargetPath" -foregroundcolor Green

function Patch($filename, $search, $replace)
{
	$path = Join-Path $TargetPath $filename
	$content = Get-Content $path
	$filtered_content = ""
	$content | %{
		$str = $_
		if ($str.contains($search)) {
			$str = $replace
			Write-Host "* [$filename] patched $search -> $replace" -foregroundcolor Green
		}
		$filtered_content += $str + "`n"
	}
	Set-Content $path $filtered_content
}

# Simple patches
Patch "pngcrush.c" "include <utime.h>" "#include <sys/utime.h>"
Patch "zutil.c" "include `"gzguts.h`"" "#include `"../zlib/gzguts.h`""

# Difficult patch
$path = Join-Path $TargetPath "zconf.h"
$content = Get-Content $path
$filtered_content = ""
$last_line_match = $false
$prev=""
$content | %{
	$str = $_
	if ($str.contains("#if 1    /* was set to #if 1 by ./configure */")) {
		$last_line_match = $true
		$prev=$str
	} elseif ($str.contains("define Z_HAVE_UNISTD_H") -and $last_line_match) {
		Write-Host "* [zconf.h] #if 1  -> #if 0" -foregroundcolor Green
		Write-Host "#define Z_HAVE_UNISTD_H" -foregroundcolor Green
		$filtered_content += "#if 0`n" + $str + "`n"
		$last_line_match = $false
	} else {
		if ($last_line_match) {
			$filtered_content += $prev + "`n"
		}
		$last_line_match = $false
		$filtered_content += $str + "`n"
	}
}
Set-Content $path $filtered_content