cd /D %TEMP%
IF EXIST AzureImageOptimizer (
  rd /S /q AzureImageOptimizer
)
mkdir AzureImageOptimizer
cd AzureImageOptimizer
nuget install AzureImageOptimizer -Pre

SET JOB_FOLDER="%WEBROOT_PATH%\App_Data\jobs\continuous\AzureImageOptimizer"
IF EXIST %JOB_FOLDER% (
  rd /S /q %JOB_FOLDER%
)
mkdir %JOB_FOLDER%
cd AzureImageOptimizer*
xcopy tools %JOB_FOLDER% /E /C