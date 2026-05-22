Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead('d:\TudfConverter\docs\CU11880001_30042026__04052026_1131_F2_1.xlsx')
$entry = $zip.GetEntry('xl/sharedStrings.xml')
$stream = $entry.Open()
$reader = New-Object System.IO.StreamReader($stream)
$xmlStr = $reader.ReadToEnd()
$xml = [xml]$xmlStr
$stream.Close()
$zip.Dispose()

$strings = @()
foreach ($si in $xml.sst.si) {
    if ($si.t) {
        if ($si.t.GetType().Name -eq "String") {
            $strings += $si.t
        } elseif ($si.t.'#text') {
            $strings += $si.t.'#text'
        }
    } elseif ($si.r) {
        $text = ""
        foreach ($r in $si.r) {
            if ($r.t.GetType().Name -eq "String") {
                $text += $r.t
            } elseif ($r.t.'#text') {
                $text += $r.t.'#text'
            }
        }
        $strings += $text
    }
}

$strings | Out-File d:\TudfConverter\docs\strings.txt -Encoding UTF8
