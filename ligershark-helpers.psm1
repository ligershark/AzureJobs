function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value
    Split-Path $Invocation.MyCommand.Path
}

$script:scriptDir = ((Get-ScriptDirectory) + "\")

$global:azurejobssettings = New-Object psobject -Property @{
    InstallPath = ("$env:APPDATA\ligershark\AzureJobs\v0\")
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
        (resolve-path ('{0}ImageCompressor.Job.exe' -f $global:azurejobssettings.installPath)).ToString()
    }
}