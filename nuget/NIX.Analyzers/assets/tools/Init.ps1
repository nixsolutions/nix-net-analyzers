param($installPath, $toolsPath, $package, $project)

$file = ([IO.Path]::Combine(($installPath), 'content\Rules\.editorconfig'))
$dest = (Get-Location)
$exclude = Get-ChildItem -recurse $dest

Copy-Item -Recurse $file $dest -Verbose -Exclude $exclude
