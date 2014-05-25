param($rootPath, $toolsPath, $package, $project)

<#
if((Get-Module azure-jobs)){
    Remove-Module azure-jobs
}
Import-Module (Join-Path -Path ($toolsPath) -ChildPath 'azure-jobs.psm1')
#>
#########################
# Start of script here
#########################


$projDir = GetProjectDirectory -project $project

$jobsPropsFile = Join-Path $projDir 'azurejobs.props'

$jobsProps = $null
if(!(Test-Path $jobsPropsFile)){
    # create the file since it is not there
    $jobsProps = New-Project -filePath $jobsPropsFile
    $project.ProjectItems.AddFromFile($jobsPropsFile) | Out-Null
}

if(!$jobsProps){
    $jobsProps = [Microsoft.Build.Construction.ProjectRootElement]::Open($jobsPropsFile)
}

# Before modifying the project save everything so that nothing is lost
$DTE.ExecuteCommand("File.SaveAll")

CheckoutIfUnderScc -filePath $jobsPropsFile -project $project
EnsureFileIsWriteable -filePath $jobsPropsFile

$pgLabel = 'ls-azurejobs-imageopt'
RemoveExistingKnownPropertyGroups -projectRootElement $jobsProps -importLabel $pgLabel
$relPathToToolsFolder = ComputeRelativePathToTargetsFile -startPath (Get-Item $projDir) -targetPath (Get-Item ("{0}\tools\" -f $rootPath))

$propertyGroup = $jobsProps.AddPropertyGroup()
$propertyGroup.Label = $pgLabel
$propertyGroup.AddProperty('ls-AzureImageOptimizerToolsPath', ('$([System.IO.Path]::GetFullPath( $(MSBuildProjectDirectory)\{0}\))') -f $relPathToToolsFolder);

$jobsProps.Save()
