# MSBuild tasks for Wix Toolset

## HarvestDirectory
This task is similar to original HeatDirectory, but allows greater control of which files should be included or excluded. For example, the code below, adds all files in ..\MyComponent\bin\Release (or Debug or whatever your configuration is) excluding PDB files:
```
<HarvestDirectory 
  SourceFolder="..\MyComponent\bin\$(Configuration)" 
  DirectoryRefId="MyComponentDir" 
  OutputFile="MyComponentFiles.wxs" 
  ComponentGroupName="MyComponentGroup" 
  ExcludeMask="*.pdb" />
```

Once you add this task and compile it for the first time, a new file (named *MyComponentFiles.wxs* in the example above) will be created. Add this file to the project so it will be compiled.
