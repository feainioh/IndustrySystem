Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

$ae = [System.Windows.Automation.AutomationElement]
$ts = [System.Windows.Automation.TreeScope]
$ct = [System.Windows.Automation.ControlType]

$automationIdProp = [System.Windows.Automation.AutomationElement]::AutomationIdProperty
$controlTypeProp = [System.Windows.Automation.AutomationElement]::ControlTypeProperty
$processIdProp = [System.Windows.Automation.AutomationElement]::ProcessIdProperty

function New-AutomationIdCondition([string]$automationId) {
    return [System.Windows.Automation.PropertyCondition]::new($automationIdProp, $automationId)
}

function New-ControlTypeCondition($controlType) {
    return [System.Windows.Automation.PropertyCondition]::new($controlTypeProp, $controlType)
}

function New-ProcessIdCondition([int]$processId) {
    return [System.Windows.Automation.PropertyCondition]::new($processIdProp, $processId)
}

function New-AndCondition([System.Windows.Automation.Condition[]]$conditions) {
    if ($conditions.Length -eq 1) {
        return $conditions[0]
    }
    return [System.Windows.Automation.AndCondition]::new($conditions)
}

function Wait-Element([scriptblock]$finder, [int]$timeoutSeconds = 30) {
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    while ($sw.Elapsed.TotalSeconds -lt $timeoutSeconds) {
        $element = & $finder
        if ($null -ne $element) {
            return $element
        }
        [System.Threading.Thread]::Yield() | Out-Null
    }
    return $null
}

function Invoke-Element($element) {
    if ($null -eq $element) {
        return $false
    }

    try {
        $invokePattern = $element.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
        if ($null -ne $invokePattern) {
            ([System.Windows.Automation.InvokePattern]$invokePattern).Invoke()
            return $true
        }
    } catch { }

    try {
        $selectionPattern = $element.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
        if ($null -ne $selectionPattern) {
            ([System.Windows.Automation.SelectionItemPattern]$selectionPattern).Select()
            return $true
        }
    } catch { }

    try {
        $expandPattern = $element.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
        if ($null -ne $expandPattern) {
            ([System.Windows.Automation.ExpandCollapsePattern]$expandPattern).Expand()
            return $true
        }
    } catch { }

    return $false
}

function Set-ElementTextValue($element, [string]$value) {
    if ($null -eq $element) {
        return $false
    }

    try {
        $valuePattern = $element.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
        if ($null -eq $valuePattern) {
            return $false
        }
        ([System.Windows.Automation.ValuePattern]$valuePattern).SetValue($value)
        return $true
    } catch {
        return $false
    }
}

function Get-ElementTextValue($element) {
    if ($null -eq $element) {
        return $null
    }

    try {
        $valuePattern = $element.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
        if ($null -eq $valuePattern) {
            return $null
        }
        return ([System.Windows.Automation.ValuePattern]$valuePattern).Current.Value
    } catch {
        return $null
    }
}

function Find-ByAutomationId {
    param(
        [Parameter(Mandatory = $true)][string]$AutomationId,
        [Parameter(Mandatory = $true)][int]$ProcessId,
        [Parameter(Mandatory = $false)]$ControlType,
        [Parameter(Mandatory = $false)]$Root = $null
    )

    if ($null -eq $Root) {
        $Root = $ae::RootElement
    }

    $conditions = [System.Collections.Generic.List[System.Windows.Automation.Condition]]::new()
    $conditions.Add((New-AutomationIdCondition $AutomationId))
    $conditions.Add((New-ProcessIdCondition $ProcessId))

    if ($null -ne $ControlType) {
        $conditions.Add((New-ControlTypeCondition $ControlType))
    }

    $condition = New-AndCondition $conditions.ToArray()
    return $Root.FindFirst($ts::Descendants, $condition)
}

function Get-TopWindowsByProcessId([int]$processId) {
    $condition = New-AndCondition @((New-ControlTypeCondition $ct::Window), (New-ProcessIdCondition $processId))
    $collection = $ae::RootElement.FindAll($ts::Children, $condition)
    $windows = @()
    for ($i = 0; $i -lt $collection.Count; $i++) {
        $windows += $collection.Item($i)
    }
    return ,$windows
}

function Dismiss-AnyModalDialog([int]$processId, [int]$mainWindowHandle) {
    $windows = Get-TopWindowsByProcessId -processId $processId
    for ($i = 0; $i -lt $windows.Count; $i++) {
        $window = $windows.Item($i)
        if ($window.Current.NativeWindowHandle -eq $mainWindowHandle) {
            continue
        }

        $button = $window.FindFirst($ts::Descendants, (New-ControlTypeCondition $ct::Button))
        if ($null -ne $button) {
            return (Invoke-Element $button)
        }
    }

    return $false
}

function Select-FirstItemInList($listElement) {
    if ($null -eq $listElement) {
        return $false
    }

    $items = $listElement.FindAll($ts::Descendants, (New-ControlTypeCondition $ct::ListItem))
    if ($items.Count -eq 0) {
        return $false
    }

    return (Invoke-Element $items.Item(0))
}

function Try-SelectTemplate($comboElement, [int]$processId) {
    if ($null -eq $comboElement) {
        return $false
    }

    if (-not (Invoke-Element $comboElement)) {
        return $false
    }

    $comboRect = $comboElement.Current.BoundingRectangle
    $items = $ae::RootElement.FindAll(
        $ts::Descendants,
        (New-AndCondition @((New-ControlTypeCondition $ct::ListItem), (New-ProcessIdCondition $processId)))
    )

    $candidates = @()
    for ($i = 0; $i -lt $items.Count; $i++) {
        $item = $items.Item($i)
        if (-not $item.Current.IsEnabled) {
            continue
        }

        $rect = $item.Current.BoundingRectangle
        if ($rect.Width -le 0 -or $rect.Height -le 0) {
            continue
        }

        $score = [Math]::Abs($rect.Left - $comboRect.Left) + [Math]::Abs($rect.Top - $comboRect.Bottom)
        $candidates += [pscustomobject]@{
            Score = $score
            Item = $item
        }
    }

    if ($candidates.Count -eq 0) {
        return $false
    }

    $best = $candidates | Sort-Object Score | Select-Object -First 1
    return (Invoke-Element $best.Item)
}

$result = [ordered]@{
    Login = $false
    NavigateExperimentConfig = $false
    ExistingFlowSave = $false
    NewFlowApplyTemplate = $false
    NewFlowSave = $false
    NewFlowSaveButtonEnabled = $false
    NewFlowNameValue = ''
    ModalDismissed = $false
    Errors = [System.Collections.Generic.List[string]]::new()
}

$exePath = Join-Path $PSScriptRoot '..\src\Presentation\IndustrySystem.Presentation.Wpf\bin\Debug\net9.0-windows7.0\IndustrySystem.Presentation.Wpf.exe'
$exePath = [System.IO.Path]::GetFullPath($exePath)

if (-not (Test-Path -LiteralPath $exePath)) {
    throw "Executable not found: $exePath"
}

$process = Start-Process -FilePath $exePath -PassThru

try {
    $quickLoginButton = Wait-Element {
        Find-ByAutomationId -AutomationId 'QuickLoginButton' -ProcessId $process.Id -ControlType $ct::Button
    } 40

    if ($null -eq $quickLoginButton) {
        throw 'Quick login button not found.'
    }

    if (-not (Invoke-Element $quickLoginButton)) {
        throw 'Quick login click failed.'
    }
    $result.Login = $true

    $mainWindowHandle = Wait-Element {
        $windows = Get-TopWindowsByProcessId -processId $process.Id
        for ($i = 0; $i -lt $windows.Count; $i++) {
            $w = $windows[$i]
            $navGroup = Find-ByAutomationId -AutomationId 'NavExperimentManagementGroup' -ProcessId $process.Id -Root $w
            if ($null -ne $navGroup) {
                return $w.Current.NativeWindowHandle
            }
        }
        return $null
    } 50

    if ($null -eq $mainWindowHandle) {
        throw 'Main shell window not found.'
    }

    $navExperimentConfig = Wait-Element {
        $group = Find-ByAutomationId -AutomationId 'NavExperimentManagementGroup' -ProcessId $process.Id
        if ($null -ne $group) {
            [void](Invoke-Element $group)
        }
        return (Find-ByAutomationId -AutomationId 'NavExperimentConfigItem' -ProcessId $process.Id)
    } 40

    if ($null -eq $navExperimentConfig) {
        throw 'ExperimentConfig navigation item not found.'
    }

    if (-not (Invoke-Element $navExperimentConfig)) {
        throw 'ExperimentConfig navigation click failed.'
    }
    $result.NavigateExperimentConfig = $true

    $newButton = Wait-Element {
        Find-ByAutomationId -AutomationId 'ExperimentConfigNewButton' -ProcessId $process.Id -ControlType $ct::Button
    } 25

    $saveButton = Wait-Element {
        Find-ByAutomationId -AutomationId 'ExperimentConfigSaveButton' -ProcessId $process.Id -ControlType $ct::Button
    } 25

    $nameTextBox = Wait-Element {
        Find-ByAutomationId -AutomationId 'ExperimentConfigNameTextBox' -ProcessId $process.Id -ControlType $ct::Edit
    } 25

    if ($null -eq $newButton -or $null -eq $saveButton -or $null -eq $nameTextBox) {
        throw 'ExperimentConfig key controls not found.'
    }

    $experimentList = Find-ByAutomationId -AutomationId 'ExperimentConfigExperimentList' -ProcessId $process.Id
    [void](Select-FirstItemInList $experimentList)

    $existingName = "SmokeExisting-$([DateTime]::Now.ToString('HHmmss'))"
    if (-not (Set-ElementTextValue -element $nameTextBox -value $existingName)) {
        throw 'Existing flow: failed to set experiment name.'
    }

    if (-not (Invoke-Element $saveButton)) {
        throw 'Existing flow: save click failed.'
    }

    $result.ModalDismissed = (Dismiss-AnyModalDialog -processId $process.Id -mainWindowHandle $mainWindowHandle)
    $result.ExistingFlowSave = $true

    if (-not (Invoke-Element $newButton)) {
        throw 'New flow: new button click failed.'
    }

    $nameTextBox = Wait-Element {
        Find-ByAutomationId -AutomationId 'ExperimentConfigNameTextBox' -ProcessId $process.Id -ControlType $ct::Edit
    } 20

    $saveButton = Wait-Element {
        Find-ByAutomationId -AutomationId 'ExperimentConfigSaveButton' -ProcessId $process.Id -ControlType $ct::Button
    } 20

    [void](Wait-Element {
        $tb = Find-ByAutomationId -AutomationId 'ExperimentConfigNameTextBox' -ProcessId $process.Id -ControlType $ct::Edit
        if ($null -eq $tb) {
            return $null
        }
        $value = Get-ElementTextValue -element $tb
        if ([string]::IsNullOrWhiteSpace($value)) {
            return $tb
        }
        return $null
    } 10)

    $templateCombo = Wait-Element {
        Find-ByAutomationId -AutomationId 'ExperimentConfigTemplateComboBox' -ProcessId $process.Id -ControlType $ct::ComboBox
    } 10

    if ($null -ne $templateCombo) {
        $result.NewFlowApplyTemplate = (Try-SelectTemplate -comboElement $templateCombo -processId $process.Id)
    }

    $newName = "SmokeNew-$([DateTime]::Now.ToString('HHmmss'))"
    if (-not (Set-ElementTextValue -element $nameTextBox -value $newName)) {
        throw 'New flow: failed to set experiment name.'
    }

    [void](Wait-Element {
        $tb = Find-ByAutomationId -AutomationId 'ExperimentConfigNameTextBox' -ProcessId $process.Id -ControlType $ct::Edit
        if ($null -eq $tb) {
            return $null
        }
        $value = Get-ElementTextValue -element $tb
        if ($value -like "*$newName*") {
            return $tb
        }
        return $null
    } 5)

    $saveButton = Wait-Element {
        $btn = Find-ByAutomationId -AutomationId 'ExperimentConfigSaveButton' -ProcessId $process.Id -ControlType $ct::Button
        if ($null -eq $btn) {
            return $null
        }
        if ($btn.Current.IsEnabled) {
            return $btn
        }
        return $null
    } 20

    if ($null -eq $saveButton) {
        throw 'New flow: save button did not become enabled.'
    }

    $result.NewFlowSaveButtonEnabled = $saveButton.Current.IsEnabled
    $result.NewFlowNameValue = (Get-ElementTextValue -element $nameTextBox)

    if (-not (Invoke-Element $saveButton)) {
        throw 'New flow: save click failed.'
    }

    [void](Dismiss-AnyModalDialog -processId $process.Id -mainWindowHandle $mainWindowHandle)
    $result.NewFlowSave = $true
}
catch {
    $result.Errors.Add($_.Exception.Message)
    if ($null -ne $_.InvocationInfo) {
        $result.Errors.Add(("Line {0}: {1}" -f $_.InvocationInfo.ScriptLineNumber, $_.InvocationInfo.Line.Trim()))
    }
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        try { $process.Kill($true) } catch { }
    }
}

Write-Output '--- ExperimentConfig Flow Smoke Result ---'
foreach ($entry in $result.GetEnumerator()) {
    if ($entry.Name -eq 'Errors') {
        if ($entry.Value.Count -eq 0) {
            Write-Output 'Errors: <none>'
        }
        else {
            Write-Output 'Errors:'
            foreach ($errorMessage in $entry.Value) {
                Write-Output ("  - {0}" -f $errorMessage)
            }
        }
    }
    else {
        Write-Output ("{0}: {1}" -f $entry.Name, $entry.Value)
    }
}
