@REM =========================================================================
@REM (c) 2005-2014 Copyright, Real-Time Innovations. All rights reserved.
@REM
@REM No duplications, whole or partial, manual or electronic, may be made
@REM without express written permission.  Any such copies, or
@REM revisions thereof, must display this notice unaltered.
@REM This code contains trade secrets of Real-Time Innovations, Inc.
@REM =========================================================================
@REM  NAME: rticommon.bat
@REM  ------------------------------------------------------------------------
@REM  rticommon.bat is a common interface for RTI Connext DDS
@REM  scripts. It sets up the environment to achieve transparent execution
@REM  of applications.
@REM
@REM  USAGE:
@REM  ------------------------------------------------------------------------
@REM  To call the script use the following commands in your batch script:
@REM
@REM  call "%dir..\resource\scripts\rticommon.bat"
@REM  if not !ERRORLEVEL! == 0  (
@REM      exit /b 1
@REM  )
@REM
@REM  Parameters:
@REM
@REM    Note that you before calling the script you will need to set the
@REM    following parameters first:
@REM       - fileName      [Required]
@REM         Name of the script (with no path or extension)
@REM         set fileName=%~n0
@REM
@REM       - dir           [Required]
@REM         Directory where the script lives, i.e., path/to/installation/bin.
@REM         It can be set by using the following command:
@REM         set dir=%~dp0
@REM
@REM       - args          [Required]
@REM         Arguments for the command to run. The arguments of the script may
@REM         be obtained using the following command:
@REM         set args=%*
@REM
@REM       - appName       [Required]
@REM         Name of the application to be run. It must be spelled in lower
@REM         case using snake case without quotes -- no espaces are allowed.
@REM         For instance, for RTI Recorder:
@REM         set appName=rti_recorder
@REM
@REM       - scriptVersion       [Optional]
@REM         Version of the application to be run. This parameter is used to
@REM         load the right version of the Core and APIs dynamic libraries
@REM 	     by setting the path to the right folder.
@REM         For instance:
@REM         set Path=%libDir%\%scriptVersion%;%Path%
@REM
@REM       - executableName  [Required only for C/C++ Applications]
@REM         For example: set executableName=rtirecorder.exe
@REM
@REM       - libraryName  [Optional]
@REM         This is a library that the application needs to find before
@REM         running.
@REM         For example: set libraryeName=rtimonitoring.dll
@REM
@REM       - collectTelemetry       [Optional]
@REM         Indicates that telemetry should be collected.
@REM         Set to false on scrips that are part of the build (e.g. rtiddsgen)
@REM         By default is set to true. In addition if the variable is set to
@REM         true, then after returning from the command being measured call:
@REM         "%telemetryCommand%"  %telemetryCommandArgs%
@REM
@REM  Return variables:
@REM
@REM    The script will set in the environment of your script the following
@REM    variables if it runs successfully:
@REM
@REM       - binDir -- Directory where binary application live for your
@REM         platform. For instance, if you have i86Win32VS2010 installed
@REM         %binDir% == path\to\installation\resource\app\bin\i86Win32VS2010
@REM
@REM       - libDir -- Directory where RTI Connext DDS libraries for your
@REM         platform are installed. For instance, if you have i86Win32VS2010
@REM         installed:
@REM         %libDir% == path\to\installation\lib\i86Win32VS2010
@REM
@REM       - eclipseDir -- Directory where libraries required by
@REM         eclipse are installed.
@REM         eclipse_dir == path\to\installation\resource\app\eclipse
@REM
@REM       - executableWithPath -- Executable the application needs to be run
@REM         including its path. That is, if you want to run the application
@REM         you are writing a batch script for. After calling rticommon.bat
@REM         do:
@REM         call "%executableWithPath% %args%
@REM
@REM       - jreDir -- Directory where Java Runtime Environment for your
@REM         platform is installed. To run Java use %jreLib%\bin\java.exe.
@REM         %jreDir% == path\to\installation\lib\%jreArch%
@REM
@REM       - templateDir -- Directory with template files that applications
@REM         will copy to the user's %myDoc%.
@REM         %templateDir% ==
@REM           path\to\installation\resource\template\rti_workspace
@REM
@REM       - resourceExampleDir -- Directory containing all the
@REM         examples this directory will be copied to the user's home
@REM         directory the fisrt time any RTI tool or utility is run.
@REM         resourceExample_dir == %templateDir%\examples
@REM
@REM       - resourceUserConfigDir -- Directory containing all the
@REM         default user's configuration files. This directory will be
@REM         copied to the user's home directory the first time any
@REM         RTI tool or utility is run.
@REM         resourceUserConfigDir == %templateDir$\user_config
@REM
@REM       - appSupportDir -- Directory where configuration and application
@REM         specific files are located.
@REM         %appSupportDir == path\to\isntallation\resource\app\app_support
@REM
@REM       - binClassDir -- Directory where DDS Java applications (e.g.,
@REM         rtimonitor.jar) are located.
@REM         %rtiClassDir% == %libDir%\java
@REM
@REM       - libClassDir -- Directory where DDS Java libraries (e.g.,
@REM         rtiddsmonitoringlib.jar) are located.
@REM         %libClassDir% == %libDir%\java
@REM
@REM       - appLibDir -- Directory where third party libraries for
@REM         applications live.
@REM         %appLibClassDir% == path\to\installation\resource\app\lib\java
@REM
@REM       - appLibClassDir -- Directory where Java libraries for applications
@REM         live. For instance third party libraries needed by Java-based
@REM         tools.
@REM         %appLibClassDir% == %appLibDir%\java
@REM
@REM       - platformName -- Name of the RTI Connext DDS architecture
@REM         installed in your system for your environment (e.g.,
@REM         i86Win32VS2010).
@REM
@REM       - workspaceDir -- User directory to place RTI user-specific configurations
@REM         and documents. For instance:
@REM         C:\Users\fgarcia\Documents\rti_workspace.
@REM
@REM       - myDocsExampleDir -- Directory within $home_dir containing
@REM         examples. This directory is created by this script when run if
@REM         it does not previously exist. It is a copy of
@REM         $resource_examples_dir.
@REM         myDocsExampleDir == %workspaceDir%/examples
@REM
@REM       - myDocsUserConfigDir -- Directory within $home_dir containing
@REM         all the default user's configuration files. This directory is
@REM         created by this script when run if it does not previously exist.
@REM         It is a copy of %templateDir%\user_config.
@REM         myDocsUserConfigDir == %workspaceDir%/user_config
@REM
@REM       - installationPath -- Top level directory of the installation (Might
@REM         be a duplicate of NDDSHOME for some cases)
@REM
@REM       - NDDSHOME -- Top level directory of the installation.
@REM
@REM       - NODEJSHOME -- Directory where Node.js for your platform is
@REM         installed. To run Node.js use $NODEJSHOME/node.exe.
@REM
@REM       - RTI_LICENSE_FILE -- Points to the default location of the products
@REM         license file. This script will not override the RTI_LICENSE_FILE
@REM         environment variable if it is already set.
@REM
@REM       - jreArch -- Name of the RTI Connext DDS host architecture
@REM         for your environment (e.g., i86Win32).
@REM
@REM       - nodejsPlatform   -- Name of the Node.js host architecture for your
@REM         environment (e.g., node-v16.20.2-win-x64).
@REM
@REM       - telemetryCommand -- Command to execute to record telemetry. Only set
@REM         if telemetryAppName was set on input. Calling script should execute:
@REM         %programBeingExecuted% %programArgs% & "%telemetryCommand%" %telemetryCommandArgs%
@REM
@REM       - telemetryCommandArgs -- Arguments to pass to the telemetryCommand
@REM
@REM   Return codes:
@REM
@REM     If we find any problem while running the script we exit and return
@REM     an error number that can be retrieved using the ERRORLEVEL
@REM     environment variable. These are the ERRORLEVELS you will get:
@REM
@REM        0 -- The script run successfully.
@REM        1 -- Error processing the environment.
@REM  ------------------------------------------------------------------------

@REM If the script has been called with an argument run goto :%~1
set callArgument=%~1
if defined callArgument (
    goto :%callArgument%
)

set appDir=%dir%..\resource\app

@REM Set libDir and binDir depending on whether it is run from within the
@REM module, ndds.4.1, or the end installation directory.
if "%runWithinModule%"=="true" (
    set "defaultLibDir=%dir%..\lib"
    set "defaultBinDir=%dir%..\lib"
) else if exist "%dir%..\..\ndds.4.1" (
    set "defaultLibDir=%dir%..\lib"
    set "defaultBinDir=%appDir%\bin"
) else (
   set "defaultLibDir=%dir%\..\lib"
   set "defaultBinDir=%appDir%\bin"
   if not defined RTI_COPY_WORKSPACE (
       set copyWorkspace=true
   ) else (
       set copyWorkspace=%RTI_COPY_WORKSPACE%
   )
)

@REM Do not overwrite libDir and binDir if they were previously defined
if not defined libDir (
   set "libDir=%defaultLibDir%"
)
if not defined binDir (
   set "binDir=%defaultBinDir%"
)

set hostVersion=7.3.1
set docDir=%dir%..\doc
set jreDir=%appDir%\jre
set libClassDir=%libDir%\java
set appLibDir=%appDir%\lib
set appLibClassDir=%appLibDir%\java
set binClassDir=%appLibClassDir%
set appSupportDir=%dir%..\resource\app\app_support
set eclipseDir=%appDir%\eclipse
set templateDir=%dir%..\resource\template\rti_workspace
set resourceExampleDir=%templateDir%\examples
set resourceUserConfigDir=%templateDir%\user_config
set xmlDir=%dir%..\resource\xml

@REM RTI Environment variable
set "NDDSHOME=%dir%.."

@REM Get User My Documents folder from the Registry
FOR /F "tokens=2*" %%a in ('C:\Windows\System32\reg.exe QUERY "HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders" /v "Personal" 2^>nul') DO (
  if not "%%b" == "" (
    set "myDocsDir=%%b"
  )
)

@REM Default workspace directory. It can be overriden in the following
@REM configuration files:
@REM
@REM - %myDocsDir%\rti\rticommon_config.bat (for the current user)
@REM - %NDDSHOME%\resource\scripts\rticommon_config.bat (for all users)
set "workspaceDir=%myDocsDir%\rti_workspace\%hostVersion%"

@REM By default we copy examples into the workspace. Users can override
@REM this behavior editing the files listed above.
set copyExamples=true

if exist "%myDocsDir%\rti\rticommon_config.bat" (
    @REM Override defaults if %myDocsDir%\rti\rticommon_config.bat exists
    call "%myDocsDir%\rti\rticommon_config.bat"
) else if exist "%NDDSHOME%\resource\scripts\rticommon_config.bat" (
    @REM Override defaults if %NDDSHOME%\resource\scripts\rticommon_config.bat exists
    call "%NDDSHOME%\resource\scripts\rticommon_config.bat"
)

set "myDocsUserConfigDir=%workspaceDir%\user_config"
set "myDocsExampleDir=%workspaceDir%\examples"

@REM Copy examples to workspace dir if they are not there already
if "%copyWorkspace%"=="true" (
    if "%copyExamples%"=="true" (
        if exist "%templateDir%\examples" (
            if not exist "%myDocsExampleDir%" (
                echo First time running RTI Connext DDS...
                echo Copying examples into "%myDocsExampleDir%"
                if not exist "%workspaceDir%" (
                    md "%workspaceDir%"
                )
                md "%myDocsExampleDir%"
                xcopy /q /e "%templateDir%\examples" "%myDocsExampleDir%"
            )
        )
    )
)

@REM Copy user configuration files to workspace dir if they do not exist
if "%copyWorkspace%"=="true" (
    if exist "%templateDir%\user_config" (
        if not exist "%myDocsUserConfigDir%" (
            echo Copying user configuration files into "%myDocsUserConfigDir%"
            if not exist "%workspaceDir%" (
                md "%workspaceDir%"
            )
            md "%myDocsUserConfigDir%"
            xcopy /q /e "%templateDir%\user_config" "%myDocsUserConfigDir%"
        ) else (
            @REM Copy user configuration files that have not been copied yet
            pushd "%templateDir%\user_config"
            for /d %%d in (*) do (
                if not exist "%myDocsUserConfigDir%\%%d" (
                    echo Copying missing "%%d" user configuration files into "%myDocsUserConfigDir%"
                    md "%myDocsUserConfigDir%\%%d"
                    xcopy /q /e "%templateDir%\user_config\%%d" "%myDocsUserConfigDir%\%%d"
                )
            )
            popd
        )
    )
)

@REM Do not overwrite the RTI_LICENSE_FILE value if it is already set.
if not defined RTI_LICENSE_FILE (
    if exist "%workspaceDir%\rti_license.dat" (
        set "RTI_LICENSE_FILE=%workspaceDir%\rti_license.dat"
    ) else if exist "%workspaceDir%..\rti_license.dat" (
        set "RTI_LICENSE_FILE=%workspaceDir%..\rti_license.dat"
    ) else (
        set "RTI_LICENSE_FILE=%NDDSHOME%\rti_license.dat"
    )
)


@REM To add support for new 64-bit architectures to the script, just add the
@REM platform name to the x64Win64 libs lists.
set i86Win32INtimeLibs=i86INtime6.3VS2017
set x64Win64Libs=%i86Win32INtimeLibs% x64Win64VS2017 x64Win64VS2015 x64Win64VS2013 x64Win64VS2012 x64Win64VS2010 AMD64Windows  arm64Win64VS2022

@REM Identify environment. Currently we support x86 (i86Win32) and AMD64
@REM (x64Win64) platforms.
if "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
    set "platformsToTry=%x64Win64Libs%"
    set "jreArch=x64Win64"
) else if "%PROCESSOR_ARCHITEW6432%"=="AMD64" (
    set "platformsToTry=%x64Win64Libs%"
    set "jreArch=x64Win64"
) else if "%PROCESSOR_ARCHITECTURE%"=="x86" (
    set "jreArch=i86Win32"
) else if "%PROCESSOR_ARCHITECTURE%"=="ARM64" (
    set "platformsToTry=%x64Win64Libs%"
    set "jreArch=x64Win64"
) else (
    echo Processor "%PROCESSOR_ARCHITECTURE%" not supported. Please contact support@rti.com.
    exit /b 1
)

set "nodejsPlatform=node-v20.10.0-win-x64"

for /F "delims=." %%f in ("%executableName%") do (
    set executableName=%%f
)

if defined CONNEXTDDS_ARCH (
    set "platformsToTry=%CONNEXTDDS_ARCH% %platformsToTry%"
    set customArchitecture=%CONNEXTDDS_ARCH%
) else (
    if defined connextddsArchitecture (
        set "platformsToTry=%connextddsArchitecture% %platformsToTry%"
        set customArchitecture=%connextddsArchitecture%
    )
)

@REM Check if there is a libraryName provided, otherwise assing 'nddscore' to it
for /F "delims=." %%f in ("%libraryName%") do (
    set libraryName=%%f
)

if not defined libraryName (
    set libraryName=nddscore
)

for %%a in (%platformsToTry%) do (
    if defined executableName (

        if exist "%binDir%\%%a\%executableName%.rta" (
            if defined needsSharedLibraries (
                if not "%needsSharedLibraries%"=="false" (
                    if exist "%appLibDir%\%%a\%libraryName%.rsl" (
                        set platformName=%%a
                        set executableName=%executableName%.rta
                        goto break
                    )
                    if "%customArchitecture%"=="%%a" (
                        if exist "%libDir%\%%a\%libraryName%.rsl" (
                            set platformName=%%a
                            set executableName=%executableName%.rta
                            set "appLibDir=%libDir%"
                            goto break
                        )
                    )
                )
            ) else (
                @REM This is the case of rtiddsgen, which does not require
                @REM any libraries to run.
                set platformName=%%a
                goto break
            )
        )
        if exist "%binDir%\%%a\%executableName%.exe" (
            @REM By default all executables require shared libraries, unless
            @REM explicitly disabled.
            if not "%needsSharedLibraries%"=="false" (
                if exist "%appLibDir%\%%a\%libraryName%.dll" (
                    set platformName=%%a
                    set executableName=%executableName%.exe
                    goto break
                )
                if "%customArchitecture%"=="%%a" (
                    @REM If the user selected a custom architecture, look
                    @REM for %libraryName%.dll in %libDir%, because we may need
                    @REM to load runtime libraries form the target's lib
                    @REM directory.
                    if exist "%libDir%\%%a\%libraryName%.dll" (
                        set platformName=%%a
                        set "appLibDir=%libDir%"
                        set executableName=%executableName%.exe
                        goto break
                    )
                )
            ) else (
                @REM This is the case of rtiddsgen, which does not require
                @REM any libraries to run.
                set platformName=%%a
                set executableName=%executableName%.exe
                goto break
            )
        )
    ) else (
        if defined needsSharedLibraries (
            if "%needsSharedLibraries%"=="true" (
                if exist "%appLibDir%\%%a\%libraryName%.dll" (
                    set platformName=%%a
                    goto break
                )
            )
        ) else (
            @REM This is the case of rtiddsgen, which does not require
            @REM any libraries to run.
            set platformName=%%a
            goto break
        )
    )
)

@REM We need to use goto to be able to break in the for loop.
:break

@REM Return error if platformName is not set. That is, the script could not
@REM find libraries for your environment in the install directory. If the
@REM executableName is not defined, we are going to run a java application,
@REM so we added an exception, to be able to run 32bits Windows systems.
if not defined platformName (
    if defined executableName (
        echo Cannot find libraries for your system under "%libDir%". Please contact support@rti.com.
            exit /b 1
    ) else (
        set platformName=""
    )
)

@REM Check that that executableWithPath exists in case %executableName% is
@REM already set.
if defined executableName (
    if not exist "%binDir%\%platformName%\%executableName%" (
        echo Cannot find "%executableName%" in "%binDir%\%platformName%". Please contact support@rti.com.
        exit /b 1
    )
    set "executableWithPath=%binDir%\%platformName%\%executableName%"

    @REM Platforms like INTIme use a utility to launch applications. In such
    @REM cases, the call command must execute the utility itself, which in turn
    @REM calls %executableWithPath%. We can add such utilities as optional
    @REM callers in scripts that use rticommon.bat:
    @REM call %optionalCaller% %executableWithPath% ...
    set "optionalCaller="
    if not x%platformName:INtime=%==x%platformName% (
        set "optionalCaller=piperta"
    )
)

if not defined JREHOME (
    set "JREHOME=%jreDir%\%jreArch%"
)
set jreServerLibDir=%JREHOME%\bin\server
set jreClientLibDir=%JREHOME%\bin\client
set jreBinDir=%JREHOME%\bin
set libDir=%libDir%\%platformName%
set appLibDir=%appLibDir%\%platformName%
set "connextVersionSystemProperty=-DCONNEXT_VERSION="%hostVersion%""
set "connextPlatformSystemProperty=-DCONNEXT_PLATFORM="%platformName%""
set "connextWorkspaceSystemProperty=-DCONNEXT_WORKSPACE="%workspaceDir%""

if not defined NODEJSHOME (
    set "NODEJSHOME=%appDir%\%nodejsPlatform%"
)

@REM Remove quotes before setting the path
set "Path=%path:"=%"
set "Path=%appLibDir%\%scriptVersion%;%appLibDir%;%binDir%\%platformName%;%jreServerLibDir%;%jreClientLibDir%;%jreBinDir%;%NDDSHOME%\bin;%RTI_LD_LIBRARY_PATH%;%Path%"

@REM Export other environment variables
set RTI_SHARED_LIB_PREFIX=\
set RTI_SHARED_LIB_SUFFIX=.dll
set RTI_EXAMPLES_DIR=%myDocsExampleDir%
set RTI_WORKSPACE_DIR=%workspaceDir%

@REM By default collect_telemetry is set to true
if not defined collectTelemetry (
    set collectTelemetry="true"
)

@REM Collect telemetry unless explicitly disabled
if %collectTelemetry%=="false" (
    goto :EOF
)

if not exist "%appLibClassDir%\rtiusagemetrics.jar" (
    goto :EOF
)

set "javaCmd=%JREHOME%\bin\java.exe"
set "javaCmdJVMArgsTelemetry=-DTELEMETRY_RESOURCE_DIR="%appSupportDir%/product_usage_metrics""
set "javaCmdArgsTelemetry=%jvmOpts% %javaCmdJVMArgsTelemetry% %connextVersionSystemProperty% %connextPlatformSystemProperty% %connextWorkspaceSystemProperty% -jar "%appLibClassDir%\rtiusagemetrics.jar""

if not defined fileName (
    @REM Do not call telemetry if fileName is not defined
    EXIT /B 0
)

@REM Command return status is timestamp in seconds
"%javaCmd%" %javaCmdArgsTelemetry% -begin %fileName%
set /a "startTimestamp=!ERRORLEVEL!"

@REM Set command the calling script should use to capture telemetry
set "telemetryCommand=call "%dir%..\resource\scripts\rticommon.bat" telemetry_update"

EXIT /B 0

:telemetry_update

@REM Command return status is timestamp in seconds
"%javaCmd%" %javaCmdArgsTelemetry% -end %fileName% %startTimestamp%
set /a "lastModified=!ERRORLEVEL!"

if %lastModified% GTR 604800 (
    START /B "RTI Terminal" CALL "%javaCmd%" %javaCmdArgsTelemetry% -push
)

EXIT /B 0
