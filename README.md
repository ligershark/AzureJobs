Azure Webjobs and Site Extensions
=========

[![Build status](https://ci.appveyor.com/api/projects/status/pg4isaw6y9dhih5u)](https://ci.appveyor.com/project/sayedihashimi/azurejobs)

This is home of a few Azure Webjobs and matching Site Extensions.

## Image Optimizer
Optimizes all images in your website included images uploaded by
the users.

Deploy manually: [NuGet package](http://www.nuget.org/packages/AzureImageOptimizer/)  
Deploy automatically: [Azure Site Extension](https://www.siteextensions.net/packages/AzureImageOptimizer/)

## Text Minifier
Minifies all .css and .js files in your website. It doesn't touch 
already minified files.

Deploy manually: [NuGet package](http://www.nuget.org/packages/AzureMinifier/)  
Deploy automatically: [Azure Site Extension](https://www.siteextensions.net/packages/AzureMinifier/)

## Configuration
By default, the Image Optimizer and Text Minifier WebJobs automatically process any files and folders in *D:\home\site\wwwroot\*

If you wish to change the paths, you can override them by adding the following keys to you App Settings in the Azure Portal and setting their values to the appropriate paths:
* AZURE_MINIFIER_PATH
* AZURE_IMAGE_OPTIMIZER_PATH

An example of this is as follows:

![Azure App Settings](https://cloud.githubusercontent.com/assets/6916342/6949110/d19b0698-d8b2-11e4-9657-7ef30561091e.png)
