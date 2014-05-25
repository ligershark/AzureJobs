param($rootPath, $toolsPath, $package, $project)

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

# Save everything so that nothing is lost
$DTE.ExecuteCommand("File.SaveAll")

CheckoutIfUnderScc -filePath $jobsPropsFile -project $project
EnsureFileIsWriteable -filePath $jobsPropsFile

$pgLabel = 'ls-AzureTextMin'
RemoveExistingKnownPropertyGroups -projectRootElement $jobsProps -importLabel $pgLabel
$relPathToToolsFolder = ComputeRelativePathToTargetsFile -startPath $projDir -targetPath (Get-Item ("{0}\tools\" -f $rootPath))

$propertyGroup = $jobsProps.AddPropertyGroup()
$propertyGroup.Label = $pgLabel
$propertyGroup.AddProperty('ls-AzureMinToolsPath', ('$([System.IO.Path]::GetFullPath( $(MSBuildProjectDirectory)\{0}\))') -f $relPathToToolsFolder);

$jobsProps.Save()
