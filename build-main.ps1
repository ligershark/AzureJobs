 [cmdletbinding()]
param(
    [switch]
    $CleanOutputFolder,
    [switch]
    $LocalDeploy,
    [switch]
    $publishToProd,
    $nugetApiKey = ($env:NuGetApiKey),
    $siteExtNugetApiKey = ($env:SiteExtensionsNuGetApiKey),
    $dropboxOutputFolder = ("$dropBoxHome\public\azurejobs\output"),
    $LocalDeployFolder = ("$env:APPDATA\ligershark\AzureJobs\v0\")
)

function Write-Message{
    [cmdletbinding()]
    param(
        [Parameter(Position=0,Mandatory=$true,ValueFromPipeline=$true)]
        [string[]]$message
    )
    process{
        foreach($msg in $message){
            if($nugetApiKey){
                $msg = $msg.Replace($nugetApiKey,'REMOVED-FROM-LOG')
            }
            if($siteExtNugetApiKey){
                $msg = $msg.Replace($siteExtNugetApiKey,'REMOVED-FROM-LOG')
            }

            $msg | Write-Verbose
        }
    }
}
 
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
            'Downloading nuget.exe' | Write-Message
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
        'Copying files to local folder [{0}]' -f $destFolder | Write-Message

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

<#
.SYNOPSIS
This function can be used to publish the nuget packages to nuget.org and siteextensions.net.

.DESCRIPTION
Before calling this function you should make sure to call the following calls to nuget.exe
>nuget.exe setApiKey <your-key-here> -source https://www.nuget.org
>nuget.exe setApiKey <your-key-here> -source https://www.siteextensions.net

.EXAMPLE
Publish-AzureJobsToProd -version 0.0.19
#>
function Publish-AzureJobsToProd{ # Publish-AzureJobsToProd -outputpath $outputpath -version 0.0.19 -verbose -whatif
    [cmdletbinding(SupportsShouldProcess=$true)]
    param(
        $outputpath = (Join-Path $scriptDir 'OutputRoot\'),
        [Parameter(Mandatory=$True)]
        $version
    )
    process{
        $pkgs = @() 
        $pkgs += @{
            'path' = (join-path $outputpath ('AzureJobsShared.{0}-beta.nupkg' -f $version ))
            'source' = 'https://nuget.org'
        }
        $pkgs += @{
            'path' = (join-path $outputpath ('AzureImageOptimizer.{0}-beta.nupkg' -f $version ))
            'source' = 'https://nuget.org'
        }
        $pkgs += @{
            'path' = (join-path $outputpath ('AzureMinifier.{0}-beta.nupkg' -f $version ))
            'source' = 'https://nuget.org'
        }
        $pkgs += @{
            'path' = (join-path $outputpath ('site-extensions\AzureImageOptimizer.{0}.nupkg' -f $version ))
            'source' = 'https://www.siteextensions.net'
        }
        $pkgs += @{
            'path' = (join-path $outputpath ('site-extensions\AzureMinifier.{0}.nupkg' -f $version ))
            'source' = 'https://www.siteextensions.net'
        }

        $pkgs | ForEach-Object{
            # nuget push $_ -source
            if(!(Test-path $_.path)){ 'path not found [{0}]' -f $_.path|Write-Error }
            $pushArgs = @('push',$_.path,'-source',$_.source,'-NonInteractive')
            
            #'Calling [nuget.exe {0}]' -f ($pushArgs -join ' ') | Write-Message
            if($PSCmdlet.ShouldProcess($env:COMPUTERNAME,('nuget.exe {0}' -f ($pushArgs -join ' ')))){
                &(Get-NuGet) $pushArgs
            }
        }
    }
}

<#
.SYNOPSIS
    If nuget is in the tools
    folder then it will be downloaded there.
#>
function Get-Nuget(){
    [cmdletbinding()]
    param(
        $toolsDir = ("$env:LOCALAPPDATA\LigerShark\tools\"),
        $nugetDownloadUrl = 'http://nuget.org/nuget.exe'
    )
    process{
        $nugetDestPath = Join-Path -Path $toolsDir -ChildPath nuget.exe
        
        if(!(Test-Path $nugetDestPath)){
            $nugetDir = ([System.IO.Path]::GetDirectoryName($nugetDestPath))
            if(!(Test-Path $nugetDir)){
                New-Item -Path $nugetDir -ItemType Directory | Out-Null
            }

            'Downloading nuget.exe' | Write-Message
            (New-Object System.Net.WebClient).DownloadFile($nugetDownloadUrl, $nugetDestPath)

            # double check that is was written to disk
            if(!(Test-Path $nugetDestPath)){
                throw 'unable to download nuget'
            }
        }

        # return the path of the file
        $nugetDestPath
    }
}

function PublishNuGetPackage{
    [cmdletbinding()]
    param(
        [Parameter(Mandatory=$true,ValueFromPipeline=$true)]
        [string]$nugetPackages,

        [string]
        $nugetSource,

        [Parameter(Mandatory=$true)]
        $nugetApiKey
    )
    process{
        foreach($nugetPackage in $nugetPackages){
            $pkgPath = (get-item $nugetPackage).FullName
            $cmdArgs = @('push',$pkgPath,$nugetApiKey,'-NonInteractive')

            if($nugetSource){
                $cmdArgs += '-source'
                $cmdArgs += $nugetSource
            }

            'TEST: Publishing nuget package with the following args: [nuget.exe {0}]' -f ($cmdArgs -join ' ') | Write-Message
            # &(Get-Nuget) $cmdArgs
        }
    }
}

function PublishSiteExtensionNuGetPackage{
    [cmdletbinding()]
    param(
        [Parameter(Mandatory=$true,ValueFromPipeline=$true)]
        [string]$nugetPackages,

        [Parameter(Mandatory=$true)]
        $nugetApiKey
    )
    process{
        PublishNuGetPackage -nugetPackages $nugetPackages -nugetApiKey $nugetApiKey -nugetSource 'https://www.siteextensions.net'        
    }
}

###########################################################
# Begin script
###########################################################

'Begin started. This script uses psbuild which is available at http://aka.ms/psbuild' | Write-Host

EnsurePsbuildInstalled

if($CleanOutputFolder -or $publishToProd){
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
    'Copying files to local app data folder [{0}]' -f $LocalDeployFolder | Write-Message
    if(!(Test-Path $LocalDeployFolder)){
        md $LocalDeployFolder
    }

    CopyOutput-ToFolder -destFolder $LocalDeployFolder

    # dropbox deploy
    if(!(Test-Path $dropboxOutputFolder)){
        'dropbox folder not found at [{0}]' -f $dropboxOutputFolder | Write-Warning
    }
    else {
        'Copying files to local dropbox folder [{0}]' -f $dropboxOutputFolder | Write-Message
        CopyOutput-ToFolder -destFolder $dropboxOutputFolder
    }

}
if($publishToProd){   
    $outputRoot = $global:azurejobsbuild.OutputPath
   
    (Get-ChildItem -Path $outputRoot '*.nupkg').FullName | PublishNuGetPackage -nugetApiKey $nugetApiKey


    (Get-ChildItem -Path (join-path $outputRoot 'site-extensions') '*.nupkg').FullName | PublishSiteExtensionNuGetPackage -nugetApiKey $siteExtNugetApiKey
}

