param($rootPath, $toolsPath, $package, $project)


if((Get-Module azure-jobs)){
    Remove-Module azure-jobs
}
Import-Module (Join-Path -Path ($toolsPath) -ChildPath 'azure-jobs.psm1')

#########################
# Start of script here
#########################

'Uninstalling AzureImageOptimizer' | Write-Verbose

$projDir = (Get-Item $project.FullName).Directory.FullName
$jobsPropsFile = Join-Path $projDir 'azurejobs.props'

$jobsProps = $null

if(Test-Path $jobsPropsFile){
    $jobsProps = [Microsoft.Build.Construction.ProjectRootElement]::Open($jobsPropsFile)
    # Before modifying the project save everything so that nothing is lost
    $DTE.ExecuteCommand("File.SaveAll")

    CheckoutIfUnderScc -filePath $jobsPropsFile -project $project
    EnsureFileIsWriteable -filePath $jobsPropsFile

    $pgLabel = 'ls-AzureTextMin'
    RemoveExistingKnownPropertyGroups -projectRootElement $jobsProps -importLabel $pgLabel
    $jobsProps.Save()
}