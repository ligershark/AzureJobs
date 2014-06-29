[cmdletbinding()]
param(
    
    $installPath = ("$env:APPDATA\ligershark\AzureJobs\ImageOptimizer\v0\")
    
)

function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value
    Split-Path $Invocation.MyCommand.Path
}

$script:scriptDir = ((Get-ScriptDirectory) + "\")

$global:azurejobssettings = New-Object psobject -Property @{
<#
    MessagePrefix = '  '
    TemplateRoot = (Join-Path ($script:scriptDir) -ChildPath 'templates-v1\Add Project')
    IndentLevel = 1
    WhatIf = $true
    QuitResultKey = 'rps-quit'
#>
}

function Optimize-Images{
    [cmdletbinding()]
    param(
        [Parameter(Mandatory=$true)]
        $folder,

        $logfile

    )
    process{
        
        $imgoptArgs = @()
        $imgoptArgs += '--folder'
        $imgoptArgs += $folder
        
        if($logfile){
            $imgoptArgs += '--logfile'
            $imgoptArgs += $logfile
        }

        $imgOptPath = ImageOptimizerExe

        'Calling image optizer [{0}] with the following args [{1}]' -f $imgOptPath, ($imgoptArgs -join ' ') | Write-Output

        & "$imgOptPath" $imgoptArgs        
    }
}

function Get-ImageOptimizerExe{
    [cmdletbinding()]
    param()
    process{
        (resolve-path ('{0}ImageCompressor.Job.exe' -f $installPath)).ToString()
    }
}