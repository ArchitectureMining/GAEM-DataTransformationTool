# GAEM-DataTransformationTool
A tool to transform and Pseudonymize data from various tools into a form to be used by the GAEM-PseudoETWToNeo4jImport. 

## Code quality
This is a prototype for research purposes, it's not pretty, it not robust and it's not flexible. However it works well enough for the purpose for which it was intended.

## Requirements
- ETL logs
- ETW manifest file
- Doxygen xml output

## Usage
Run the exe with a settings file.
```cmd
>GAEM-DataTransformationTool.exe -s "settings.xml"
```

## Options
```cmd
-s --settings [settings file]
```

## Settings format
```xml
<?xml version="1.0" encoding="utf-8"?>
<Settings xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <GeneralSettings>
    <PathPrefix>[full directory path]</PathPrefix>
    <OutputDirectory>[full directory path]</OutputDirectory>
    <PseudonymizationSalt>[string]</PseudonymizationSalt>
  </GeneralSettings>
  <ETWSettings>
    <ProcessETL>[true/false]</ProcessETL>
    <ETLRootDirectory>[full directory path]</ETLRootDirectory>
    <ETLManifestFile>[full *.man file path]</ETLManifestFile>
    <PseudonymizeETL>[true/false]</PseudonymizeETL>
  </ETWSettings>
  <DoxygenSettings>
    <ProcessDoxygen>[true/false]</ProcessDoxygen>
    <DoxygenRootDirectory>[full directory path]</DoxygenRootDirectory>
    <PseudonymizeDoxygen>[true/false]</PseudonymizeDoxygen>
  </DoxygenSettings>
</Settings>
```

## People
*empty for now, communicating who wants/should be here*

## License
[ISC](LICENSE)
