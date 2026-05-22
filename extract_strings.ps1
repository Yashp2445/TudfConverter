$content = Get-Content -Path 'd:\TudfConverter\docs\sharedStrings.xml' -Raw
$matches = [regex]::Matches($content, '<t[^>]*>(.*?)</t>')
$result = @()
foreach ($m in $matches) {
    $result += $m.Groups[1].Value
}
$result | Select-Object -First 200 | Out-File d:\TudfConverter\docs\strings.txt -Encoding UTF8
