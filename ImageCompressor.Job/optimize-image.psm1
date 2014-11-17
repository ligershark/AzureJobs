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
function Get-ImageOptimizer{
    [cmdletbinding()]
    param()
    process{
        (get-item (join-path $script:optImgRootDir 'ImageCompressor.Job.exe')).FullName
    }
}
<#
.SYNOPSIS
You can use this to optimize one or more folder with images. This is a PowerShell wrapper for ImageCompressor.Job.exe. The directory provided in $dir, and all child directories,
will have images optimzed.

.DESCRIPTION
This is a PowerShell wrapper for ImageCompressor.Job.exe. ImageCompressor.Job.exe relies on the following tools to optimize the images.
    gifsicle.exe
    jpegtran.exe
    optipng.exe
    png.cmd
    pngout.exe

.PARAMETER dir
The directory containing files you want to compress. Subdirectories will be included. 
This property is required. You can pass in either a single value for several values. The directory, and all child directories,
will have images optimzed.
Corresponds to --dir

.PARAMETER cacheFile
Specify the file to keep working set of files. If unspecified, then %APPDATA%\LigerShark\....
Corresponds to --cache

.PARAMETER force
Force the optimizer to recompress any files it's marked as processed in a previous run.
Corresponds to --force

.PARAMETER noreport
Won't output a .csv file with compression results.
Corresponds to --noreport

.PARAMETER startlistener
When passed after optimizing images in the dir folder, a file watcher will run on the folder to optimize any new or modified images.
Corresponds to --startlistener

.PARAMETER imgOptExePath
Path to the image optimizer ImageCompressor.Job.exe file. If not provided the default value of join-path $script:optImgRootDir 'ImageCompressor.Job.exe' will be used.

.EXAMPLE
Optimize-ImageDir -dir C:\temp\images\to-optimize-new\
Basic usage, optimizes images in the folder and it's child folders.

.EXAMPLE
Optimize-ImageDir -dir C:\temp\images\to-optimize-new\ -force -verbose
Using force will ignore the results of previous runs and re-optimze all image files.

.EXAMPLE
Get-Item .\01 | Optimize-ImageDir
Here is an example showing how to pipe a single folder

.EXAMPLE
Get-ChildItem .\ -Exclude '04' -Directory | Optimize-ImageDir
Here is an example pipling several folders.

.EXAMPLE
Get-ChildItem .\ -Exclude '04' -Directory | Optimize-ImageDir -force -Verbose
Here is an example piping several folders and selecting additional options like -force and -verbose.
#>
function Optimize-ImageDir{
    [cmdletbinding()]
    param(
        [Parameter(Mandatory=$true,Position=0,ValueFromPipeline=$true)]
        [string[]]$dir,
        [ValidateScript({Test-Path $_ -PathType 'File'})] 
        [string]$cacheFile,
        [switch]$force,
        [switch]$noreport,
        [switch]$startlistener,
        [ValidateScript({Test-Path $_ -PathType 'File'})] 
        [string]$imgOptExePath = (Get-ImageOptimizer)
    )
    process{
        foreach($dirToOptimize in $dir){
            'Starting image optimizer on folder [{0}]' -f $dir | Write-Verbose
            [string]$folderToOptimize = ((get-item $dir).FullName)

            $cmdArgs = @('--dir', ('{0}\' -f (get-item $folderToOptimize).FullName))
            if($force){ $cmdArgs += '--force' }
            if($cacheFile){ $cmdArgs += @('--cache',$cacheFile) }
            if($noreport){$cmdArgs += '--noreport' }
            if($startlistener){$cmdArgs += '--startlistener' }

            'Calling img optimizer with the following args [{0} {1}]' -f $imgOptExe, ($cmdArgs -join ' ') | Write-Verbose
            &$imgOptExePath $cmdArgs

            'Images optimized' | Write-Verbose
        }
    }
}

Export-ModuleMember -function Optimize-*,Get-*