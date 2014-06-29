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
	This will return the path to msbuild.exe. If the path has not yet been set
	then the highest installed version of msbuild.exe will be returned.
#>
function Get-MSBuild{
    [cmdletbinding()]
        param()
        process{
	    $path = $script:defaultMSBuildPath

	    if(!$path){
	        $path =  Get-ChildItem "hklm:\SOFTWARE\Microsoft\MSBuild\ToolsVersions\" | 
				        Sort-Object {[double]$_.PSChildName} -Descending | 
				        Select-Object -First 1 | 
				        Get-ItemProperty -Name MSBuildToolsPath |
				        Select -ExpandProperty MSBuildToolsPath
        
            $path = (Join-Path -Path $path -ChildPath 'msbuild.exe')
	    }

        return Get-Item $path
    }
}

function Get-NugetExe{
    [cmdletbinding()]
    param()
    process{
        return (get-item (Join-Path $scriptDir '\BuildTools\NuGet.exe'))
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

if($CleanOutputFolder){
    Clean-OutputFolder
}

'Restoring nuget packages' | Write-Host
$slnFile = Get-Item (Join-Path $scriptDir 'AzureJobs.sln')
# restore nuget packages
$nugetArgs = @()
$nugetArgs += 'restore'
$nugetArgs += $slnFile.FullName

& ((Get-NugetExe).FullName) $nugetArgs


$msbuildArgs = @()
$msbuildArgs += 'build-main.proj'
$msbuildArgs += ('/p:Configuration={0}' -f $global:azurejobsbuild.Configuration)
$msbuildArgs += '/p:VisualStudioVersion=12.0'
$msbuildArgs += '/p:RestorePackages=true'
$msbuildArgs += '/flp1:v=d;logfile=build.d.log'
$msbuildArgs += '/flp2:v=diag;logfile=build.diag.log'
$msbuildArgs += '/m'

& ((Get-MSBuild).FullName) $msbuildArgs

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