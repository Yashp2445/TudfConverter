Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead('d:\TudfConverter\docs\CU11880001_30042026__04052026_1131_F2_1.xlsx')
$entry = $zip.GetEntry('xl/sharedStrings.xml')
$stream = $entry.Open()
$reader = New-Object System.IO.StreamReader($stream)
$xml = [xml]$reader.ReadToEnd()
$xml.sst.si | Select-Object -ExpandProperty t | Select-Object -First 100
$zip.Dispose()
