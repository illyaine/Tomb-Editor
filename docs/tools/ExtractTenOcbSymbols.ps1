param(
    [Parameter(Mandatory = $false)]
    [string]$TombEngineRoot = "E:\00GitHub\TombEngine",

    [Parameter(Mandatory = $false)]
    [string]$OutputJsonPath = "E:\00GitHub\Tomb-Editor\docs\status\TEN_Object_OCB_SymbolCatalog_Generated.json",

    [Parameter(Mandatory = $false)]
    [string]$OutputMarkdownPath = "E:\00GitHub\Tomb-Editor\docs\status\TEN_Object_OCB_SymbolCatalog_Generated.md"
)

$ErrorActionPreference = "Stop"

function Get-RelativePath {
    param(
        [string]$BasePath,
        [string]$FullPath
    )

    $basePathResolved = (Resolve-Path $BasePath).Path.TrimEnd([System.IO.Path]::DirectorySeparatorChar)
    $filePathResolved = (Resolve-Path $FullPath).Path

    if ($filePathResolved.StartsWith($basePathResolved)) {
        return $filePathResolved.Substring($basePathResolved.Length).TrimStart('\', '/')
    }

    return $filePathResolved
}

function Get-SourceGroup {
    param([string]$RelativePath)

    $path = $RelativePath.Replace('/', '\')

    if ($path -like "Objects\TR1\*" -or $path -like "*\Objects\TR1\*") { return "TR1" }
    if ($path -like "Objects\TR2\*" -or $path -like "*\Objects\TR2\*") { return "TR2" }
    if ($path -like "Objects\TR3\*" -or $path -like "*\Objects\TR3\*") { return "TR3" }
    if ($path -like "Objects\TR4\*" -or $path -like "*\Objects\TR4\*") { return "TR4" }
    if ($path -like "Objects\TR5\*" -or $path -like "*\Objects\TR5\*") { return "TR5" }
    if ($path -like "Objects\Effects\*" -or $path -like "*\Objects\Effects\*") { return "Effects / Emitters" }
    if ($path -like "Objects\Generic\*" -or $path -like "*\Objects\Generic\*") { return "Generic" }
    if ($path -like "Game\*" -or $path -like "*\Game\*") { return "Core" }

    return "Other"
}

function Is-TargetSourceFile {
    param([string]$RelativePath)

    $path = $RelativePath.Replace('/', '\')

    if ($path -like "Objects\*" -or $path -like "*\Objects\*") { return $true }
    if ($path -like "Game\*" -or $path -like "*\Game\*") { return $true }

    return $false
}

function Remove-LineComment {
    param([string]$Text)

    $index = $Text.IndexOf("//")
    if ($index -ge 0) {
        return $Text.Substring(0, $index)
    }

    return $Text
}

function Remove-IntegerSuffix {
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return ""
    }

    return ($Text -replace "[uUlL]+$", "")
}

function Convert-SimpleIntExpression {
    param(
        [string]$Expression,
        [hashtable]$KnownValues
    )

    if ([string]::IsNullOrWhiteSpace($Expression)) {
        return $null
    }

    $expr = Remove-LineComment -Text $Expression
    $expr = $expr.Trim()
    $expr = $expr.TrimEnd(',')
    $expr = $expr.TrimEnd(';')
    $expr = $expr.Trim()
    $expr = $expr -replace "\b(short|int|unsigned|signed|long|auto|constexpr|const|static)\b", ""
    $expr = $expr -replace "\s+", " "
    $expr = $expr.Trim()

    while ($expr.StartsWith("(") -and $expr.EndsWith(")") -and $expr.Length -gt 1) {
        $expr = $expr.Substring(1, $expr.Length - 2).Trim()
    }

    if (![string]::IsNullOrWhiteSpace($expr) -and $KnownValues.ContainsKey($expr)) {
        return $KnownValues[$expr]
    }

    if ($expr -match "^-?0x[0-9A-Fa-f]+[uUlL]*$") {
        $negative = $expr.StartsWith("-")
        $hex = Remove-IntegerSuffix -Text $expr.TrimStart('-')
        $value = [Convert]::ToInt64($hex, 16)
        if ($negative) { return -$value }
        return $value
    }

    if ($expr -match "^-?\d+[uUlL]*$") {
        return [Int64](Remove-IntegerSuffix -Text $expr)
    }

    if ($expr -match "^\(?\s*(?<left>-?(0x[0-9A-Fa-f]+|\d+))\s*<<\s*(?<shift>\d+)\s*\)?$") {
        $leftText = $Matches["left"]
        $shift = [int]$Matches["shift"]
        if ($leftText -match "^-?0x") {
            $negative = $leftText.StartsWith("-")
            $leftValue = [Convert]::ToInt64($leftText.TrimStart('-'), 16)
            if ($negative) { $leftValue = -$leftValue }
        }
        else {
            $leftValue = [Int64]$leftText
        }

        return ($leftValue -shl $shift)
    }

    if ($expr -match "^(?<left>[A-Za-z_][A-Za-z0-9_]*)\s*\+\s*(?<right>\d+)$") {
        $leftName = $Matches["left"]
        if ($KnownValues.ContainsKey($leftName)) {
            return ([Int64]$KnownValues[$leftName] + [Int64]$Matches["right"])
        }
    }

    if ($expr -match "^(?<left>[A-Za-z_][A-Za-z0-9_]*)\s*-\s*(?<right>\d+)$") {
        $leftName = $Matches["left"]
        if ($KnownValues.ContainsKey($leftName)) {
            return ([Int64]$KnownValues[$leftName] - [Int64]$Matches["right"])
        }
    }

    return $null
}

function Test-RelevantSymbol {
    param([string]$Symbol)

    if ([string]::IsNullOrWhiteSpace($Symbol)) { return $false }
    if ($Symbol -match "OCB") { return $true }
    if ($Symbol -match "^SWT_") { return $true }
    if ($Symbol -match "^SophiaOCB$") { return $true }
    if ($Symbol -match "^PulleyFlags$") { return $true }

    return $false
}

function Add-SymbolEntry {
    param(
        [System.Collections.ArrayList]$Entries,
        [string]$Kind,
        [string]$Symbol,
        [string]$Owner,
        [object]$Value,
        [string]$ValueExpression,
        [string]$File,
        [int]$Line,
        [string]$Group,
        [string]$Code,
        [string]$Confidence
    )

    if ([string]::IsNullOrWhiteSpace($Symbol)) {
        return
    }

    if ($null -eq $Owner) { $Owner = "" }
    if ($null -eq $ValueExpression) { $ValueExpression = "" }
    if ($null -eq $Code) { $Code = "" }

    $entry = [pscustomobject]@{
        Kind = $Kind
        Symbol = $Symbol
        Owner = $Owner
        Value = $Value
        ValueExpression = $ValueExpression
        File = $File
        Line = $Line
        Group = $Group
        Code = $Code.Trim()
        Confidence = $Confidence
    }

    [void]$Entries.Add($entry)
}

function Add-Line {
    param(
        [System.Collections.ArrayList]$Lines,
        [string]$Text
    )

    [void]$Lines.Add($Text)
}

if (!(Test-Path $TombEngineRoot)) {
    throw "TombEngineRoot does not exist: $TombEngineRoot"
}

$allowedExtensions = @(".cpp", ".h", ".hpp", ".inl")
$sourceFiles = @(Get-ChildItem -Path $TombEngineRoot -Recurse -File |
    Where-Object {
        $allowedExtensions -contains $_.Extension.ToLowerInvariant()
    } |
    Where-Object {
        $relative = Get-RelativePath -BasePath $TombEngineRoot -FullPath $_.FullName
        Is-TargetSourceFile -RelativePath $relative
    } |
    Sort-Object FullName)

$knownValues = @{}
$entries = New-Object System.Collections.ArrayList
$identifierPattern = "\b([A-Za-z_][A-Za-z0-9_]*(?:OCB[A-Za-z0-9_]*)|SWT_[A-Za-z0-9_]+|SophiaOCB|PulleyFlags)\b"
$definitionPattern = "(?<symbol>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*(?<expr>[^,;]+)"

foreach ($file in $sourceFiles) {
    $relativePath = Get-RelativePath -BasePath $TombEngineRoot -FullPath $file.FullName
    $group = Get-SourceGroup -RelativePath $relativePath
    $fileLines = @(Get-Content -Path $file.FullName)

    $insideEnum = $false
    $enumName = ""
    $enumBraceDepth = 0
    $lastEnumValue = $null

    for ($i = 0; $i -lt $fileLines.Count; $i++) {
        $lineNumber = $i + 1
        $line = $fileLines[$i]

        try {
            $codeNoComment = Remove-LineComment -Text $line
            $trimmed = $codeNoComment.Trim()

            if (!$insideEnum -and $trimmed -match "^enum(\s+class)?\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)") {
                $insideEnum = $true
                $enumName = $Matches["name"]
                $enumBraceDepth = 0
                $lastEnumValue = $null
            }

            if ($insideEnum) {
                $enumBraceDepth += ([regex]::Matches($line, "\{")).Count
                $enumBraceDepth -= ([regex]::Matches($line, "\}")).Count

                if (!$trimmed.StartsWith("enum") -and !$trimmed.StartsWith("{") -and !$trimmed.StartsWith("}")) {
                    if ($trimmed -match "^(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*(=\s*(?<expr>[^,]+))?\s*,?") {
                        $entryName = $Matches["name"]
                        $expr = ""
                        if ($Matches.ContainsKey("expr")) {
                            $expr = $Matches["expr"].Trim()
                        }

                        $isRelevant = (Test-RelevantSymbol -Symbol $entryName) -or (Test-RelevantSymbol -Symbol $enumName)
                        if ($isRelevant) {
                            $value = $null
                            if (![string]::IsNullOrWhiteSpace($expr)) {
                                $value = Convert-SimpleIntExpression -Expression $expr -KnownValues $knownValues
                                if ($value -ne $null) {
                                    $lastEnumValue = [Int64]$value
                                }
                            }
                            else {
                                if ($lastEnumValue -eq $null) {
                                    $value = 0
                                }
                                else {
                                    $value = [Int64]$lastEnumValue + 1
                                }
                                $lastEnumValue = [Int64]$value
                            }

                            if ($value -ne $null -and !$knownValues.ContainsKey($entryName)) {
                                $knownValues[$entryName] = $value
                            }

                            Add-SymbolEntry -Entries $entries -Kind "enum-member" -Symbol $entryName -Owner $enumName -Value $value -ValueExpression $expr -File $relativePath -Line $lineNumber -Group $group -Code $line -Confidence "source-symbol"
                        }
                    }
                }

                if ($enumBraceDepth -le 0 -and $line -match "\}") {
                    $insideEnum = $false
                    $enumName = ""
                    $lastEnumValue = $null
                }
            }

            foreach ($definitionMatch in [regex]::Matches($codeNoComment, $definitionPattern)) {
                $symbol = $definitionMatch.Groups["symbol"].Value
                $expr = $definitionMatch.Groups["expr"].Value.Trim()
                if (Test-RelevantSymbol -Symbol $symbol) {
                    $value = Convert-SimpleIntExpression -Expression $expr -KnownValues $knownValues
                    if ($value -ne $null -and !$knownValues.ContainsKey($symbol)) {
                        $knownValues[$symbol] = $value
                    }

                    Add-SymbolEntry -Entries $entries -Kind "definition" -Symbol $symbol -Owner "" -Value $value -ValueExpression $expr -File $relativePath -Line $lineNumber -Group $group -Code $line -Confidence "source-symbol"
                }
            }

            foreach ($symbolMatch in [regex]::Matches($line, $identifierPattern)) {
                $symbol = $symbolMatch.Groups[1].Value
                if (!(Test-RelevantSymbol -Symbol $symbol)) {
                    continue
                }

                $knownValue = $null
                if ($knownValues.ContainsKey($symbol)) {
                    $knownValue = $knownValues[$symbol]
                }

                Add-SymbolEntry -Entries $entries -Kind "reference" -Symbol $symbol -Owner $enumName -Value $knownValue -ValueExpression "" -File $relativePath -Line $lineNumber -Group $group -Code $line -Confidence "reference-only"
            }
        }
        catch {
            $message = "Failed while scanning {0}:{1}. Line: {2}. Error: {3}" -f $relativePath, $lineNumber, $line.Trim(), $_.Exception.Message
            throw $message
        }
    }
}

$metadata = [pscustomobject]@{
    TombEngineRoot = $TombEngineRoot
    SourceFilesScanned = $sourceFiles.Count
    Entries = $entries.Count
    GeneratedBy = "docs/tools/ExtractTenOcbSymbols.ps1"
    Note = "Source-backed symbol extraction only. Reference-only entries need curation before TE UI exposure."
}

$result = [pscustomobject]@{
    Metadata = $metadata
    Entries = @($entries)
}

$jsonDirectory = Split-Path -Path $OutputJsonPath -Parent
if (![string]::IsNullOrWhiteSpace($jsonDirectory) -and !(Test-Path $jsonDirectory)) {
    New-Item -ItemType Directory -Path $jsonDirectory | Out-Null
}

$result | ConvertTo-Json -Depth 8 | Set-Content -Path $OutputJsonPath -Encoding UTF8

$markdownLines = New-Object System.Collections.ArrayList
Add-Line -Lines $markdownLines -Text "# TEN OCB Symbol Catalog - Generated"
Add-Line -Lines $markdownLines -Text ""
Add-Line -Lines $markdownLines -Text "Generated by docs/tools/ExtractTenOcbSymbols.ps1."
Add-Line -Lines $markdownLines -Text ""
Add-Line -Lines $markdownLines -Text "This file is a review aid. Do not expose reference-only entries directly in Tomb Editor UI."
Add-Line -Lines $markdownLines -Text ""
Add-Line -Lines $markdownLines -Text "TombEngine root: $TombEngineRoot"
Add-Line -Lines $markdownLines -Text "Source files scanned: $($sourceFiles.Count)"
Add-Line -Lines $markdownLines -Text "Entries: $($entries.Count)"
Add-Line -Lines $markdownLines -Text ""
Add-Line -Lines $markdownLines -Text "## Summary by kind"
Add-Line -Lines $markdownLines -Text ""
Add-Line -Lines $markdownLines -Text "| Kind | Entries |"
Add-Line -Lines $markdownLines -Text "| --- | ---: |"

$kindGroups = @($entries | Group-Object Kind | Sort-Object Name)
foreach ($kindGroup in $kindGroups) {
    Add-Line -Lines $markdownLines -Text ("| " + $kindGroup.Name + " | " + $kindGroup.Count + " |")
}

Add-Line -Lines $markdownLines -Text ""
Add-Line -Lines $markdownLines -Text "## Definitions and enum members"
Add-Line -Lines $markdownLines -Text ""
Add-Line -Lines $markdownLines -Text "| Symbol | Owner | Value | Expression | Source | Status |"
Add-Line -Lines $markdownLines -Text "| --- | --- | ---: | --- | --- | --- |"

$definitionEntries = @($entries | Where-Object { $_.Kind -eq "definition" -or $_.Kind -eq "enum-member" } | Sort-Object Group, Symbol, File, Line)
foreach ($entry in $definitionEntries) {
    $valueText = ""
    if ($entry.Value -ne $null) {
        $valueText = [string]$entry.Value
    }

    $expressionText = $entry.ValueExpression
    if ([string]::IsNullOrWhiteSpace($expressionText)) {
        $expressionText = "implicit"
    }

    Add-Line -Lines $markdownLines -Text ("| " + $entry.Symbol + " | " + $entry.Owner + " | " + $valueText + " | " + $expressionText + " | " + $entry.File + ":" + $entry.Line + " | " + $entry.Confidence + " |")
}

Add-Line -Lines $markdownLines -Text ""
Add-Line -Lines $markdownLines -Text "## Reference-only symbols"
Add-Line -Lines $markdownLines -Text ""
Add-Line -Lines $markdownLines -Text "These entries show where symbols are used. They still need curation against source logic before being shown in the TE object parameter UI."
Add-Line -Lines $markdownLines -Text ""
Add-Line -Lines $markdownLines -Text "| Symbol | Value if known | Source |"
Add-Line -Lines $markdownLines -Text "| --- | ---: | --- |"

$referenceEntries = @($entries | Where-Object { $_.Kind -eq "reference" } | Sort-Object Symbol, File, Line)
foreach ($entry in $referenceEntries) {
    $valueText = ""
    if ($entry.Value -ne $null) {
        $valueText = [string]$entry.Value
    }

    Add-Line -Lines $markdownLines -Text ("| " + $entry.Symbol + " | " + $valueText + " | " + $entry.File + ":" + $entry.Line + " |")
}

$markdownDirectory = Split-Path -Path $OutputMarkdownPath -Parent
if (![string]::IsNullOrWhiteSpace($markdownDirectory) -and !(Test-Path $markdownDirectory)) {
    New-Item -ItemType Directory -Path $markdownDirectory | Out-Null
}

Set-Content -Path $OutputMarkdownPath -Value @($markdownLines) -Encoding UTF8

Write-Host ("Wrote " + $entries.Count + " symbol entries to " + $OutputJsonPath)
Write-Host ("Wrote markdown review file to " + $OutputMarkdownPath)
