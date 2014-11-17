[cmdletbinding()]
param()

function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value
    Split-Path $Invocation.MyCommand.Path
}

$scriptDir = ((Get-ScriptDirectory) + "\")

# the folder where the .exes called by optimize-image are located
$script:optImgRootDir = $env:optImgRootDir
if(!$script:optImgRootDir){
    $script:optImgRootDir = (Join-Path $scriptDir "tools\")
}

'$script:optImgRootDir: [{0}]' -f $script:optImgRootDir | Write-Verbose

if(!(Test-Path $script:optImgRootDir)){
    'Optimize images tools dir ($script:optImgRootDir) not found at [{0}]' -f $script:optImgRootDir | Write-Error
}

function Optimize-Image{
    [cmdletbinding()]
    param(
        [Parameter(Mandatory=$true,Position=0)]
        $dir,
        [ValidateScript({Test-Path $_ -PathType 'File'})] 
        $imgOptExePath = (join-path $script:optImgRootDir 'ImageCompressor.Job.exe')
    )
    process{
        'Starting image optimizer on folder [{0}]' -f $dir | Write-Verbose
        [string]$folderToOptimize = ((get-item $dir).FullName)

        # .\.tools\AzureImageOptimizer.0.0.10-beta\tools\ImageCompressor.Job.exe --dir M:\temp\images\opt\to-optimize
        $cmdArgs = @('--dir', ('{0}\' -f (get-item $folderToOptimize).FullName))
        if($force){
            $cmdArgs += '--force'
        }

        'Calling img optimizer with the following args [{0} {1}]' -f $imgOptExe, ($cmdArgs -join ' ') | Write-Output
        &$imgOptExePath $cmdArgs

        'Images optimized' | Write-Output
    }
}