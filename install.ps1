[cmdletbinding()]
param(
    $imgOptInstallFolder = ("$env:APPDATA\ligershark\AzureJobs\ImageOptimizer\v0\")
)

# we need to download a few things
# .psm1 file,*.exe,*.dll

$moduleName = 'ligershark-helpers'

function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value
    Split-Path $Invocation.MyCommand.Path
}

$script:scriptDir = ((Get-ScriptDirectory) + "\")

$modulePath = (Join-Path -Path $scriptDir -ChildPath ("{0}.psm1" -f $moduleName))

if(Test-Path $modulePath){
    "Importing [{0}] module from [{1}]" -f $moduleName, $modulePath | Write-Verbose

    if((Get-Module $moduleName)){
        Remove-Module $moduleName
    }
    
    Import-Module $modulePath -PassThru -DisableNameChecking | Out-Null
}
else{
    'Unable to find [{0}] module at [{1}]' -f $moduleName, $modulePath | Write-Error
	return
}




