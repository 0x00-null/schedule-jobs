###MigrateMPData
This solution contains the code and configuration necessary for migrating MinistryPlatform tables from one environment to another.

This builds an executable called MigrateMPData.exe, which is used as follows:
```
MigrateMPData -f fileName [-x] [--help]
  -f, --file       Required. Input file to be processed.

  -x, --execute    (Default: False) Execute mode - by default will run in
                   'test' mode

  --help           Display this help screen.
```
## Environment Variables
The following environment variables must be set:
1. MP_SOURCE_DB_USER - the username to use when connecting to the source MP database (a SQLServer user, not a Windows user)
2. MP_SOURCE_DB_PASSWORD - the password for the abover user
3. MP_TARGET_DB_USER - the username to use when connecting to the target MP database (a SQLServer user, not a Windows user)
4. MP_TARGET_DB_PASSWORD - the password for the abover user
