# Redirect Manager for Umbraco

[![Build status](https://ci.appveyor.com/api/projects/status/95e8264cttd50qg3/branch/master)](https://ci.appveyor.com/project/ahwm/redirectmanager-umbraco/branch/master)
[![NuGet Status](https://buildstats.info/nuget/RedirectManager.Umbraco8)](https://www.nuget.org/packages/RedirectManager.Umbraco8/)

## NuGet

```cmd
Install-Package RedirectManager.Umbraco8
```

## CI Builds

https://nuget.pkg.github.com/ahwm/index.json

[Details](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry)

## Usage

![image](https://user-images.githubusercontent.com/20478373/129051682-48203c60-3075-44c0-8c52-8aaf1f78a6af.png)

This package gives the admin/editor the ability to define any page redirect quickly and easily.

![image](https://user-images.githubusercontent.com/20478373/127723706-64b02699-cc25-4f39-a893-8058ada09bbf.png)

Quickly import a large number of redirects using a `.xlsx`, `.xls`, `.csv`, or tab-delimited via `.txt` or `.tsv` file with the following format (illustrated in CSV):

```
OldUrl,NewUrl
/old-url,/new-url/
```
