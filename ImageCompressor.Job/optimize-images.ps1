[cmdletbinding()]
param(
    $folderToOptimize = ($pwd),

    $toolsDir = ("$env:LOCALAPPDATA\LigerShark\tools\"),

    $nugetDownloadUrl = 'http://nuget.org/nuget.exe'
)

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
            'Downloading nuget.exe' | Write-Verbose
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

<#
.SYNOPSIS
    If the image optimizer in the .ools
    folder then it will be downloaded there.
#>
function GetImageOptimizer(){
    [cmdletbinding()]
    param(
        $toolsDir = ("$env:LOCALAPPDATA\LigerShark\tools\"),
        $nugetDownloadUrl = 'http://nuget.org/nuget.exe'
    )
    process{
        
        if(!(Test-Path $toolsDir)){
            New-Item $toolsDir -ItemType Directory | Out-Null
        }

        $imgOptimizer = (Get-ChildItem -Path $toolsDir -Include 'ImageCompressor.Job.exe' -Recurse)

        if(!$imgOptimizer){
            'Downloading image optimizer to the .tools folder' | Write-Verbose
            # nuget install AzureImageOptimizer -Prerelease -OutputDirectory C:\temp\nuget\out\
            $cmdArgs = @('install','AzureImageOptimizer','-Prerelease','-OutputDirectory',(Resolve-Path $toolsDir).ToString())

            'Calling nuget to install image optimzer with the following args. [{0}]' -f ($cmdArgs -join ' ') | Write-Verbose
            &(Get-Nuget -toolsDir $toolsDir -nugetDownloadUrl $nugetDownloadUrl) $cmdArgs | Out-Null
        }

        $imgOptimizer = Get-ChildItem -Path $toolsDir -Include 'ImageCompressor.Job.exe' -Recurse | select -first 1
        if(!$imgOptimizer){ throw 'Image optimizer not found' }       

        $imgOptimizer
    }
}

function OptimizeImages(){
    [cmdletbinding()]
    param(
        [Parameter(Mandatory=$true,Position=0)]
        $dir,
        $toolsDir = ("$env:LOCALAPPDATA\LigerShark\tools\"),
        $nugetDownloadUrl = 'http://nuget.org/nuget.exe'
    )
    process{        
        [string]$imgOptExe = (GetImageOptimizer -toolsDir $toolsDir -nugetDownloadUrl $nugetDownloadUrl)

        [string]$folderToOptimize = (Resolve-path $dir)

        'Starting image optimizer on folder [{0}]' -f $dir | Write-Host
        # .\.tools\AzureImageOptimizer.0.0.10-beta\tools\ImageCompressor.Job.exe --dir M:\temp\images\opt\to-optimize
        $cmdArgs = @('--dir', $folderToOptimize)

        'Calling img optimizer with the following args [{0} {1}]' -f $imgOptExe, ($cmdArgs -join ' ') | Write-Host
        &$imgOptExe $cmdArgs

        'Images optimized' | Write-Host
    }
}

OptimizeImages -dir $folderToOptimize -toolsDir $toolsDir -nugetDownloadUrl $nugetDownloadUrl
