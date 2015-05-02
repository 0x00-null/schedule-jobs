###MigrateMPData
This project contains the code and configuration necessary for migrating MinistryPlatform tables from one environment to another.

This builds an executable called MigrateMPData.exe, which is used as follows:
```
MigrateMPData -f fileName [-x] [--help]
  -f, --file       Required. Input file to be processed.

  -x, --execute    (Default: False) Execute mode - by default will run in
                   'test' mode

  --help           Display this help screen.
```
