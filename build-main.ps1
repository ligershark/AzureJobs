[cmdletbinding()]
param(
    [switch]
    $CleanOutputFolder
)
 
 function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value
    Split-Path $Invocation.MyCommand.Path
}

$scriptDir = ((Get-ScriptDirectory) + "\")

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
            Remove-Item $outputFolder -Recurse
        }

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
$msbuildArgs += '/p:Configuration=Release'
$msbuildArgs += '/p:VisualStudioVersion=12.0'
$msbuildArgs += '/p:RestorePackages=true'
$msbuildArgs += '/flp1:v=d;logfile=build.d.log'
$msbuildArgs += '/flp2:v=diag;logfile=build.diag.log'

& ((Get-MSBuild).FullName) $msbuildArgs