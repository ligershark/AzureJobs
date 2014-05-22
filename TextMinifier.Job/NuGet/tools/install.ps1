param($rootPath, $toolsPath, $package, $project)


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
<#
# Update the Project file to import the .targets file
$relPathToTargets = ComputeRelativePathToTargetsFile -startPath ($projItem = Get-Item $project.FullName) -targetPath (Get-Item ("{0}\tools\{1}" -f $rootPath, $targetsFileToAddImport))

$projectMSBuild = [Microsoft.Build.Construction.ProjectRootElement]::Open($projFile)

RemoveExistingKnownPropertyGroups -projectRootElement $projectMSBuild
$propertyGroup = $projectMSBuild.AddPropertyGroup()
$propertyGroup.Label = $importLabel

$importStmt = ('$([System.IO.Path]::GetFullPath( $(MSBuildProjectDirectory)\{0} ))' -f $relPathToTargets)
$propNuGetImportPath = $propertyGroup.AddProperty($targetsPropertyName, "$importStmt");
$propNuGetImportPath.Condition = ' ''$(TemplateBuilderTargets)''=='''' ';

AddImportElementIfNotExists -projectRootElement $projectMSBuild

$projectMSBuild.Save()

UpdateVsixManifest -project $project

"    TemplateBuilder has been installed into project [{0}]" -f $project.FullName| Write-Host -ForegroundColor DarkGreen
"    `nFor more info how to enable TemplateBuilder on build servers see http://sedodream.com/2013/06/06/HowToSimplifyShippingBuildUpdatesInANuGetPackage.aspx" | Write-Host -ForegroundColor DarkGreen
#>