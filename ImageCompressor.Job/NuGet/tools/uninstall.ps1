param($rootPath, $toolsPath, $package, $project)


#########################
# Start of script here
#########################

'Uninstalling AzureImageOptimizer' | Write-Verbose

$projDir = GetProjectDirectory -project $project
$jobsPropsFile = Join-Path $projDir 'azurejobs.props'

$jobsProps = $null

if(Test-Path $jobsPropsFile){
    $jobsProps = [Microsoft.Build.Construction.ProjectRootElement]::Open($jobsPropsFile)
    # Before modifying the project save everything so that nothing is lost
    $DTE.ExecuteCommand("File.SaveAll")

    CheckoutIfUnderScc -filePath $jobsPropsFile -project $project
    EnsureFileIsWriteable -filePath $jobsPropsFile

    $pgLabel = 'ls-azurejobs-imageopt'
    RemoveExistingKnownPropertyGroups -projectRootElement $jobsProps -importLabel $pgLabel
    $jobsProps.Save()
}
