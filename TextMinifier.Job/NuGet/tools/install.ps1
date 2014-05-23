param($rootPath, $toolsPath, $package, $project)


if((Get-Module azure-jobs)){
    Remove-Module azure-jobs
}
Import-Module (Join-Path -Path ($toolsPath) -ChildPath 'azure-jobs.psm1')

#########################
# Start of script here
#########################

$projDir = (Get-Item $project.FullName).Directory.FullName
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

# Save everything so that nothing is lost
$DTE.ExecuteCommand("File.SaveAll")

CheckoutIfUnderScc -filePath $jobsPropsFile -project $project
EnsureFileIsWriteable -filePath $jobsPropsFile

$pgLabel = 'ls-AzureTextMin'
RemoveExistingKnownPropertyGroups -projectRootElement $jobsProps -importLabel $pgLabel
$relPathToToolsFolder = ComputeRelativePathToTargetsFile -startPath (Get-Item $project.FullName) -targetPath (Get-Item ("{0}\tools\" -f $rootPath))

$propertyGroup = $jobsProps.AddPropertyGroup()
$propertyGroup.Label = $pgLabel
$propertyGroup.AddProperty('ls-AzureTextMinToolsPath', ('$([System.IO.Path]::GetFullPath( $(MSBuildProjectDirectory)\{0}\))') -f $relPathToToolsFolder);

$jobsProps.Save()

<#
if((Get-Module azure-jobs)){
    Remove-Module azure-jobs
}
Import-Module (Join-Path -Path ($toolsPath) -ChildPath 'azure-jobs.psm1')

#########################
# Start of script here
#########################

$projFile = $project.FullName

# Make sure that the project file exists
if(!(Test-Path $projFile)){
    throw ("Project file not found at [{0}]" -f $projFile)
}

# use MSBuild to load the project and add the property

# This is what we want to add to the project
#  <PropertyGroup Label="VsixCompress">
#    <VsixCompressTargets Condition=" '$(VsixCompressTargets)'=='' ">$([System.IO.Path]::GetFullPath( $(MSBuildProjectDirectory)\..\packages\VsixCompress.1.0.0.6\tools\vsix-compress.targets ))</VsixCompressTargets>
#  </PropertyGroup>

# Before modifying the project save everything so that nothing is lost
$DTE.ExecuteCommand("File.SaveAll")


CheckoutProjFileIfUnderScc -project $project
EnsureProjectFileIsWriteable -project $project

$pgLabel = 'ls-AzureTextMin'
$projectMSBuild = [Microsoft.Build.Construction.ProjectRootElement]::Open($project.FullName)

RemoveExistingKnownPropertyGroups -projectRootElement $projectMSBuild -importLabel $pgLabel
# add the ls-AzureImageCompressToolsPath property to the project
$relPathToToolsFolder = ComputeRelativePathToTargetsFile -startPath (Get-Item $project.FullName) -targetPath (Get-Item ("{0}\tools\" -f $rootPath))

$propertyGroup = $projectMSBuild.AddPropertyGroup()
$propertyGroup.Label = $pgLabel
$propertyGroup.AddProperty('ls-AzureTextMinToolsPath', ('$([System.IO.Path]::GetFullPath( $(MSBuildProjectDirectory)\{0}\))') -f $relPathToToolsFolder);

$projectMSBuild.Save()
#>