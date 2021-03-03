# Download latest dotnet/codeformatter release from github
$repo = "ObjectivityLtd/DBTestCompare"
$filenamePattern = "DBTestCompare*.zip"
$pathExtract = "./"

$preRelease = $false

if ($preRelease) {
    $releasesUri = "https://api.github.com/repos/$repo/releases"
    $downloadUri = ((Invoke-RestMethod -Method GET -Uri $releasesUri)[0].assets | Where-Object name -like $filenamePattern ).browser_download_url
}
else {
    $releasesUri = "https://api.github.com/repos/$repo/releases/latest"
    $downloadUri = ((Invoke-RestMethod -Method GET -Uri $releasesUri).assets | Where-Object name -like $filenamePattern ).browser_download_url
}

$pathZip = Join-Path -Path $pathExtract -ChildPath $(Split-Path -Path $downloadUri -Leaf)
echo pathZip $pathZip
echo pathExtract $pathExtract
Invoke-WebRequest -Uri $downloadUri -Out $pathZip

If($IsWindows){
	Expand-Archive -LiteralPath $pathZip -DestinationPath $pathExtract -Force
} else {
unzip -o '*.zip' -x 'test-definitions/**/*.*'
}

Remove-Item -Path  $pathZip  -Force

New-Item -Path './target' -ItemType Directory -Force
