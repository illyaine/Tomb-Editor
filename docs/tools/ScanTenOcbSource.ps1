param(
    [Parameter(Mandatory = $false)]
    [string]$TombEngineRoot = "E:\00GitHub\TombEngine",

    [Parameter(Mandatory = $false)]
    [string]$OutputPath = "E:\00GitHub\Tomb-Editor\docs\status\TEN_Object_OCB_SourceScan_Generated.md",

    [Parameter(Mandatory = $false)]
    [int]$ContextLines = 3
)

Set-StrictMode -Version Latest
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
    if ($path -ieq "Game\items.cpp" -or $path -ieq "TombEngine\Game\items.cpp") { return $true }
    if ($path -ieq "Game\items.h" -or $path -ieq "TombEngine\Game\items.h") { return $true }
    if ($path -like "*\Game\items.cpp" -or $path -like "*\Game\items.h") { return $true }

    return $false
}

function Get-MatchKind {
    param([string]$Line)

    if ($Line -match "TestOcb\s*\(") { return "TestOcb" }
    if ($Line -match "RemoveOcb\s*\(") { return "RemoveOcb" }
    if ($Line -match "ClearAllOcb\s*\(") { return "ClearAllOcb" }
    if ($Line -match "TriggerFlags\s*(&|\|)") { return "TriggerFlags bitwise" }
    if ($Line -match "TriggerFlags\s*(==|!=|<=|>=|<|>)") { return "TriggerFlags comparison" }
    if ($Line -match "switch\s*\([^\)]*TriggerFlags") { return "TriggerFlags switch" }
    if ($Line -match "\b[A-Za-z0-9_]*OCB[A-Za-z0-9_]*\b") { return "OCB symbol/comment" }
    if ($Line -match "case\s+[-]?[0-9]+\s*:") { return "Numeric case" }
    if ($Line -match "ItemFlags|GetFlagField|TestFlagField|SetFlagField|ClearFlags") { return "ItemFlags related" }

    return "Other"
}

function Get-NumericEvidence {
    param([string]$Line)

    $matches = [regex]::Matches($Line, "(?<![A-Za-z0-9_])-?\d+(?![A-Za-z0-9_])")
    if ($matches.Count -eq 0) { return "" }

    $values = New-Object System.Collections.Generic.List[string]
    foreach ($match in $matches) {
        if (!$values.Contains($match.Value)) {
            $values.Add($match.Value)
        }
    }

    return [string]::Join(", ", $values.ToArray())
}

function Add-Line {
    param(
        [System.Collections.Generic.List[string]]$Lines,
        [string]$Text
    )

    $Lines.Add($Text)
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

$patterns = @(
    "TriggerFlags",
    "TestOcb\s*\(",
    "RemoveOcb\s*\(",
    "ClearAllOcb\s*\(",
    "\b[A-Za-z0-9_]*OCB[A-Za-z0-9_]*\b",
    "ItemFlags",
    "GetFlagField\s*\(",
    "TestFlagField\s*\(",
    "SetFlagField\s*\(",
    "ClearFlags\s*\("
)

$findings = New-Object System.Collections.Generic.List[object]

foreach ($file in $sourceFiles) {
    $relativePath = Get-RelativePath -BasePath $TombEngineRoot -FullPath $file.FullName
    $lines = @(Get-Content -Path $file.FullName)

    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        $hasMatch = $false

        foreach ($pattern in $patterns) {
            if ($line -match $pattern) {
                $hasMatch = $true
                break
            }
        }

        if (!$hasMatch) {
            continue
        }

        $start = [Math]::Max(0, $i - $ContextLines)
        $end = [Math]::Min($lines.Count - 1, $i + $ContextLines)
        $contextLines = New-Object System.Collections.Generic.List[string]

        for ($j = $start; $j -le $end; $j++) {
            $prefix = " "
            if ($j -eq $i) {
                $prefix = ">"
            }

            $lineNumber = $j + 1
            $contextLines.Add(($prefix + $lineNumber + ": " + $lines[$j].TrimEnd()))
        }

        $finding = New-Object PSObject
        Add-Member -InputObject $finding -MemberType NoteProperty -Name Group -Value (Get-SourceGroup -RelativePath $relativePath)
        Add-Member -InputObject $finding -MemberType NoteProperty -Name File -Value $relativePath
        Add-Member -InputObject $finding -MemberType NoteProperty -Name Line -Value ($i + 1)
        Add-Member -InputObject $finding -MemberType NoteProperty -Name Kind -Value (Get-MatchKind -Line $line)
        Add-Member -InputObject $finding -MemberType NoteProperty -Name NumericEvidence -Value (Get-NumericEvidence -Line $line)
        Add-Member -InputObject $finding -MemberType NoteProperty -Name Code -Value $line.Trim()
        Add-Member -InputObject $finding -MemberType NoteProperty -Name Context -Value ([string]::Join([Environment]::NewLine, $contextLines.ToArray()))
        $findings.Add($finding)
    }
}

$outputLines = New-Object System.Collections.Generic.List[string]
Add-Line -Lines $outputLines -Text "# TEN Object OCB Source Scan - Generated"
Add-Line -Lines $outputLines -Text ""
Add-Line -Lines $outputLines -Text "Generated by docs/tools/ScanTenOcbSource.ps1."
Add-Line -Lines $outputLines -Text "This report is source evidence only. It does not assign final meanings automatically."
Add-Line -Lines $outputLines -Text ""
Add-Line -Lines $outputLines -Text "TombEngine root: $TombEngineRoot"
Add-Line -Lines $outputLines -Text "Source files scanned: $($sourceFiles.Count)"
Add-Line -Lines $outputLines -Text "Findings: $($findings.Count)"
Add-Line -Lines $outputLines -Text ""
Add-Line -Lines $outputLines -Text "## Summary"
Add-Line -Lines $outputLines -Text ""
Add-Line -Lines $outputLines -Text "| Group | Matches |"
Add-Line -Lines $outputLines -Text "| --- | ---: |"

$groups = @($findings | Group-Object Group | Sort-Object Name)
foreach ($group in $groups) {
    Add-Line -Lines $outputLines -Text ("| " + $group.Name + " | " + $group.Count + " |")
}

Add-Line -Lines $outputLines -Text ""
Add-Line -Lines $outputLines -Text "## Findings"
Add-Line -Lines $outputLines -Text ""

foreach ($group in $groups) {
    Add-Line -Lines $outputLines -Text ("### " + $group.Name)
    Add-Line -Lines $outputLines -Text ""

    $entries = @($group.Group | Sort-Object File, Line)
    foreach ($entry in $entries) {
        Add-Line -Lines $outputLines -Text ("#### " + $entry.File + ":" + $entry.Line)
        Add-Line -Lines $outputLines -Text ("Kind: " + $entry.Kind)

        if (![string]::IsNullOrWhiteSpace($entry.NumericEvidence)) {
            Add-Line -Lines $outputLines -Text ("Numeric evidence: " + $entry.NumericEvidence)
        }

        Add-Line -Lines $outputLines -Text ("Code: " + $entry.Code)
        Add-Line -Lines $outputLines -Text ""
        Add-Line -Lines $outputLines -Text "Source context:"
        Add-Line -Lines $outputLines -Text $entry.Context
        Add-Line -Lines $outputLines -Text ""
    }
}

$outputDirectory = Split-Path -Path $OutputPath -Parent
if (![string]::IsNullOrWhiteSpace($outputDirectory) -and !(Test-Path $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
}

Set-Content -Path $OutputPath -Value $outputLines.ToArray() -Encoding UTF8
Write-Host ("Wrote " + $findings.Count + " findings to " + $OutputPath)
