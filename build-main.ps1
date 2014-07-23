[cmdletbinding()]
param(
    [switch]
    $CleanOutputFolder,

    [switch]
    $LocalDeploy,

    $dropboxOutputFolder = ("$dropBoxHome\public\azurejobs\output"),

    $LocalDeployFolder = ("$env:APPDATA\ligershark\AzureJobs\v0\")
)
 
 function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value
    Split-Path $Invocation.MyCommand.Path
}

$scriptDir = ((Get-ScriptDirectory) + "\")

$global:azurejobsbuild = New-Object psobject -Property @{    
    OutputPath = ('{0}OutputRoot\' -f $scriptDir)
    Configuration = 'Release'
}


<#
.SYNOPSIS 
    This will throw an error if the psbuild module is not installed and available.
#>
function EnsurePsbuildInstalled(){
    [cmdletbinding()]
    param()
    process{

        if(!(Get-Module -listAvailable 'psbuild')){
            $msg = ('psbuild is required for this script, but it does not look to be installed. Get psbuild from here: https://github.com/ligershark/psbuild')
            throw $msg
        }

        if(!(Get-Module 'psbuild')){
            # add psbuild to the currently loaded session modules
            import-module psbuild -Global;
        }
    }
}

<#
.SYNOPSIS
    If nuget is not in the tools
    folder then it will be downloaded there.
#>
function Get-Nuget(){
    [cmdletbinding()]
    param(
        $toolsDir = ("$env:LOCALAPPDATA\LigerShark\AzureJobs\tools\"),

        $nugetDownloadUrl = 'http://nuget.org/nuget.exe'
    )
    process{
        $nugetDestPath = Join-Path -Path $toolsDir -ChildPath nuget.exe

        if(!(Test-Path $toolsDir)){
            New-Item -Path $toolsDir -ItemType Directory | Out-Null
        }
        
        if(!(Test-Path $nugetDestPath)){
            'Downloading nuget.exe' | Write-Verbose
            # download nuget
            $webclient = New-Object System.Net.WebClient
            $webclient.DownloadFile($nugetDownloadUrl, $nugetDestPath)

            # double check that is was written to disk
            if(!(Test-Path $nugetDestPath)){
                throw 'unable to download nuget'
            }
        }

        # return the path of the file
        $nugetDestPath
    }
}

function Clean-OutputFolder{
    [cmdletbinding()]
    param()
    process{
        $outputFolder = (Join-Path $scriptDir '\OutputRoot\')

        if(Test-Path $outputFolder){
            'Deleting output folder [{0}]' -f $outputFolder | Write-Host
            Remove-Item $outputFolder -Recurse -Force
        }

    }
}

function CopyOutput-ToFolder{
    [cmdletbinding()]
    param(
        [Parameter(Mandatory=$true)]
        [ValidateScript({Test-Path $_ -PathType Container})]
        $destFolder
    )
    process{
        'Copying files to local folder [{0}]' -f $destFolder | Write-Verbose

        $objFolder = Resolve-Path (Join-Path $global:azurejobsbuild.OutputPath 'obj')

        $toolsDestFolder = ('{0}\Tools' -f $destFolder)
        if(!(Test-Path $toolsDestFolder)){
            md $toolsDestFolder
        }

        $filesToCopy = @()
        Copy-Item "$objFolder\ImageCompressor.Job\*.exe" -Destination $destFolder
        Copy-Item "$objFolder\ImageCompressor.Job\*.dll" -Destination $destFolder
        Copy-Item "$objFolder\ImageCompressor.Job\*.pdb" -Destination $destFolder
        Copy-Item "$objFolder\ImageCompressor.Job\*.config" -Destination $destFolder
        Copy-Item "$objFolder\ImageCompressor.Job\Tools\*.*" -Destination $toolsDestFolder
        Copy-Item "$objFolder\TextMinifier.Job\*.exe" -Destination $destFolder
        Copy-Item "$objFolder\TextMinifier.Job\*.dll" -Destination $destFolder
        Copy-Item "$objFolder\TextMinifier.Job\*.pdb" -Destination $destFolder
        Copy-Item "$objFolder\TextMinifier.Job\*.config" -Destination $destFolder
    }
}

###########################################################
# Begin script
###########################################################

'Begin started. This script uses psbuild which is available at http://aka.ms/psbuild' | Write-Host

EnsurePsbuildInstalled

if($CleanOutputFolder){
    Clean-OutputFolder
}

'Restoring nuget packages' | Write-Host
$slnFile = Get-Item (Join-Path $scriptDir 'AzureJobs.sln')
# restore nuget packages
$nugetArgs = @('restore',$slnFile.FullName)

&(Get-Nuget) $nugetArgs

$projToBuild = (Resolve-Path (Join-Path -Path $scriptDir -ChildPath 'build-main.proj')).ToString()
Invoke-MSBuild  -projectsToBuild $projToBuild `
                -visualStudioVersion 12.0 `
                -configuration $global:azurejobsbuild.Configuration `
                -properties @{'RestorePackages'='true'}

if($LocalDeploy){
    'Copying files to local app data folder [{0}]' -f $LocalDeployFolder | Write-Output
    if(!(Test-Path $LocalDeployFolder)){
        md $LocalDeployFolder
    }

    CopyOutput-ToFolder -destFolder $LocalDeployFolder

    # dropbox deploy
    if(!(Test-Path $dropboxOutputFolder)){
        'dropbox folder not found at [{0}]' -f $dropboxOutputFolder | Write-Warning
    }
    else {
        'Copying files to local dropbox folder [{0}]' -f $dropboxOutputFolder | Write-Output
        CopyOutput-ToFolder -destFolder $dropboxOutputFolder
    }

}


