SET JOB_FOLDER="%WEBROOT_PATH%\App_Data\jobs\continuous\AzureMinifier"
IF EXIST %JOB_FOLDER% (
  rd /S /q %JOB_FOLDER%
)