cd /D %TEMP%
IF EXIST AzureMinifier (
  rd /S /q AzureMinifier
)
mkdir AzureMinifier
cd AzureMinifier
nuget install AzureMinifier -Pre

SET JOB_FOLDER="%WEBROOT_PATH%\App_Data\jobs\continuous\AzureMinifier"
IF EXIST %JOB_FOLDER% (
  rd /S /q %JOB_FOLDER%
)
mkdir %JOB_FOLDER%
cd AzureMinifier*
xcopy tools %JOB_FOLDER% /E /C