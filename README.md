# MSBuild tasks for Wix Toolset
[![Build status](https://stpatrick.visualstudio.com/Tools/_apis/build/status/Wix.AdvancedHarvestTask%20Build)](https://stpatrick.visualstudio.com/Tools/_build/latest?definitionId=-1)

## Install
Use Nuget to install it in your project:
```
Install-Package Wix.AdvancedHarvestTask
```

## HarvestDirectory
This task is similar to original HeatDirectory, but allows greater control of which files should be included or excluded. For example, the code below, adds all files in ..\MyComponent\bin\Release (or Debug or whatever your configuration is) excluding PDB files:
##### Minimalistic example
```
<HarvestDirectory 
  SourceFolder="..\MyComponent\bin\$(Configuration)" 
  DirectoryRefId="MyComponentDir" 
  OutputFile="MyComponentFiles.wxs" 
  ComponentGroupName="MyComponentGroup" 
  ExcludeMask="*.pdb" />
```
##### Full example
```
<HarvestDirectory 
  SourceFolder="..\MyComponent\bin\$(Configuration)" 
  DirectoryRefId="MyComponentDir" 
  OutputFile="MyComponentFiles.wxs" 
  ComponentGroupName="MyComponentGroup" 
  IncludeMask="*.dll;*.exe;*.config;specific-file.xml"
  ExcludeMask="some-excluded.dll" 
  ComponentPrefix="mycomp_"
  DiskId="1"
  DefaultFileVersion="1.0.0.0"
  IncludeEmptyDirectories="true" />
```

### Parameters:
* *SourceFolder* - path to the folder to search for files. Required.
* *DirectoryRefId* - directory ID, must be declared elsewhere in the project. Required.
* *OutputFile* - path to file to be generated. Required.
* *GroupComponentName* - name of the component group to generate. This name must be then added to one of the features with [ComponentGroupRef](http://wixtoolset.org/documentation/manual/v3/xsd/wix/componentgroupref.html). Required.
* *IncludeMask* - semicolon-delimited list of masks to use when searching files to include. Optional, default is \*.\*.
* *ExcludeMask* - semicolon-delimited list of masks to exclude files. Optional, defailt is empty string. Exclude masks take precedence, means when file matches both IncludeMask and ExcludeMask, it will be excluded.
* *ComponentPrefix* - needed when multiple HarvestDirectory tasks are added into project file and one or more files exists in both folders. In such case, same ID will be generated for those files and compiler will refuse to compile. When it happens, set this property and all components in this task will be prefixed with it, making them unique. Optional.
* *DiskId* - id of the disk harvested files should be added to. Optional, default is Wix default, [which is 1](http://wixtoolset.org/documentation/manual/v3/xsd/wix/component.html).
* *DefaultFileVersion* - what should be the default file version when no version information found in files. See [DefaultVersion](DefaultVersion) in Wix Toolset documentation. Optional.
* *IncludeEmptyDirectories* - should empty directories be added to generated file. Optional, default is *false*.

## How to use
1. Unload your Wix project and Edit it (both operations can be performed from context menu). 
2. Somewhere after all Wix imports, add new target, call it anything you want, for example *HarvestAllInput*. Make sure it is configured to run before target named *BeforeBuild*.
3. Add *HarvestDirectory* with relevant parameters
4. Reload project and compile it.
5. When compile it for the first time, a new file (named *MyComponentFiles.wxs* in the example above) will be created. Add this file to the project so it will be also compiled next time.

Example of *.wixproj* file:
```
..................
  <Import Project="$(WixTargetsPath)" Condition=" '$(WixTargetsPath)' != '' " />
  <Import Project="$(MSBuildExtensionsPath32)\Microsoft\WiX\v3.x\Wix.targets" Condition=" '$(WixTargetsPath)' == '' AND Exists('$(MSBuildExtensionsPath32)\Microsoft\WiX\v3.x\Wix.targets') " />
  <Target Name="EnsureWixToolsetInstalled" Condition=" '$(WixTargetsImported)' != 'true' ">
    <Error Text="The WiX Toolset v3.11 (or newer) build tools must be installed to build this project. To download the WiX Toolset, see http://wixtoolset.org/releases/" />
  </Target>
  <!-- ADDED CODE BELOW -->
  <Target Name="HarvestAllInput" BeforeTargets="BeforeBuild">
    <HarvestDirectory SourceFolder="..\MyServiceProject\bin\$(Configuration)" DirectoryRefId="MyService" OutputFile="MyComponentFiles.wxs" ComponentGroupName="MyComponentFiles" ExcludeMask="*.pdb;MyService.exe" />
  </Target> 
  <!-- END OF ADDED CODE -->
  <Target Name="EnsureNuGetPackageBuildImports" BeforeTargets="PrepareForBuild">
..................
```
