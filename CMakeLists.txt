cmake_minimum_required(VERSION 3.20)

project(DdsAmbassadorDDSClient NONE)

option(DDS_GENERATE "Run RTI rtiddsgen before validating/building." OFF)
option(DDS_CLEAN_GENERATED "Remove generated C# files before rtiddsgen when DDS_GENERATE is ON." ON)
option(DDS_SKIP_TESTS "Skip the dds_test step in dds_all." OFF)

set(DDS_RUNTIME_ID "" CACHE STRING "Optional runtime identifier used by dds_publish_cli. Leave empty for a portable framework-dependent publish.")
set(DDS_PACKAGE_VERSION "" CACHE STRING "Optional NuGet package version passed to dotnet pack.")
set(DDS_DOTNET_CONFIGURATION "Release" CACHE STRING "dotnet build/test/pack configuration.")

set(DDS_LIBRARY_PROJECT "${CMAKE_SOURCE_DIR}/src/DdsAmbassador.DDSClient/DdsAmbassador.DDSClient.csproj")
set(DDS_CLI_PROJECT "${CMAKE_SOURCE_DIR}/src/DdsAmbassador.DDSClient.Cli/DdsAmbassador.DDSClient.Cli.csproj")
set(DDS_TEST_PROJECT "${CMAKE_SOURCE_DIR}/tests/DdsAmbassador.DDSClient.Tests/DdsAmbassador.DDSClient.Tests.csproj")
set(DDS_PACKAGE_OUTPUT "${CMAKE_SOURCE_DIR}/artifacts/packages")
set(DDS_CLI_OUTPUT "${CMAKE_SOURCE_DIR}/artifacts/ddsclient")

find_program(DOTNET_EXECUTABLE dotnet REQUIRED)
find_package(Python3 REQUIRED COMPONENTS Interpreter)

set(DDS_PACK_VERSION_ARGS "")
if(NOT "${DDS_PACKAGE_VERSION}" STREQUAL "")
    list(APPEND DDS_PACK_VERSION_ARGS
        "-p:Version=${DDS_PACKAGE_VERSION}"
        "-p:PackageVersion=${DDS_PACKAGE_VERSION}")
endif()

set(DDS_PUBLISH_RUNTIME_ARGS "")
set(DDS_PUBLISH_DESCRIPTION "portable")
if(NOT "${DDS_RUNTIME_ID}" STREQUAL "")
    list(APPEND DDS_PUBLISH_RUNTIME_ARGS -r "${DDS_RUNTIME_ID}")
    set(DDS_PUBLISH_DESCRIPTION "${DDS_RUNTIME_ID}")
endif()

if(WIN32)
    find_program(POWERSHELL_EXECUTABLE NAMES pwsh powershell REQUIRED)
    set(DDS_GENERATE_COMMAND
        "${POWERSHELL_EXECUTABLE}" -NoProfile -ExecutionPolicy Bypass
        -File "${CMAKE_SOURCE_DIR}/tools/generate-dds.ps1")
    if(DDS_CLEAN_GENERATED)
        list(APPEND DDS_GENERATE_COMMAND -Clean)
    endif()
else()
    find_program(BASH_EXECUTABLE NAMES bash sh REQUIRED)
    set(DDS_GENERATE_COMMAND "${BASH_EXECUTABLE}" "${CMAKE_SOURCE_DIR}/tools/generate-dds.sh")
    if(DDS_CLEAN_GENERATED)
        list(APPEND DDS_GENERATE_COMMAND --clean)
    endif()
endif()

add_custom_target(dds_generate
    COMMAND ${DDS_GENERATE_COMMAND}
    WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}"
    COMMENT "Generating DDS C# types with RTI rtiddsgen"
    VERBATIM)

add_custom_target(dds_validate
    COMMAND "${Python3_EXECUTABLE}" "${CMAKE_SOURCE_DIR}/tools/validate-dds.py"
    WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}"
    COMMENT "Validating DDSSim.xml, topics.xml, and generated C# types"
    VERBATIM)

if(DDS_GENERATE)
    add_dependencies(dds_validate dds_generate)
endif()

add_custom_target(dds_restore
    COMMAND "${DOTNET_EXECUTABLE}" restore "${DDS_LIBRARY_PROJECT}"
    COMMAND "${DOTNET_EXECUTABLE}" restore "${DDS_CLI_PROJECT}"
    COMMAND "${DOTNET_EXECUTABLE}" restore "${DDS_TEST_PROJECT}"
    WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}"
    COMMENT "Restoring NuGet packages"
    VERBATIM)
add_dependencies(dds_restore dds_validate)

add_custom_target(dds_restore_publish
    COMMAND "${DOTNET_EXECUTABLE}" restore "${DDS_CLI_PROJECT}" ${DDS_PUBLISH_RUNTIME_ARGS}
    WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}"
    COMMENT "Restoring ddsclient CLI publish assets for ${DDS_PUBLISH_DESCRIPTION}"
    VERBATIM)
add_dependencies(dds_restore_publish dds_build)

add_custom_target(dds_build
    COMMAND "${DOTNET_EXECUTABLE}" build "${DDS_LIBRARY_PROJECT}" -c "${DDS_DOTNET_CONFIGURATION}" --no-restore
    COMMAND "${DOTNET_EXECUTABLE}" build "${DDS_CLI_PROJECT}" -c "${DDS_DOTNET_CONFIGURATION}" --no-restore
    COMMAND "${DOTNET_EXECUTABLE}" build "${DDS_TEST_PROJECT}" -c "${DDS_DOTNET_CONFIGURATION}" --no-restore
    WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}"
    COMMENT "Building DDSClient projects"
    VERBATIM)
add_dependencies(dds_build dds_restore)

add_custom_target(dds_test
    COMMAND "${DOTNET_EXECUTABLE}" test "${DDS_TEST_PROJECT}" -c "${DDS_DOTNET_CONFIGURATION}" --no-restore --no-build
    WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}"
    COMMENT "Running DDSClient tests"
    VERBATIM)
add_dependencies(dds_test dds_build)

add_custom_target(dds_pack
    COMMAND "${DOTNET_EXECUTABLE}" pack "${DDS_LIBRARY_PROJECT}" -c "${DDS_DOTNET_CONFIGURATION}" --no-restore --no-build -o "${DDS_PACKAGE_OUTPUT}" ${DDS_PACK_VERSION_ARGS}
    WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}"
    COMMENT "Packing DdsAmbassador.DDSClient NuGet package"
    VERBATIM)
if(DDS_SKIP_TESTS)
    add_dependencies(dds_pack dds_build)
else()
    add_dependencies(dds_pack dds_test)
endif()

add_custom_target(dds_publish_cli
    COMMAND "${DOTNET_EXECUTABLE}" publish "${DDS_CLI_PROJECT}" -c "${DDS_DOTNET_CONFIGURATION}" ${DDS_PUBLISH_RUNTIME_ARGS} --self-contained false --no-restore -o "${DDS_CLI_OUTPUT}"
    COMMAND "${CMAKE_COMMAND}" -E copy_directory "${CMAKE_SOURCE_DIR}/definitions" "${DDS_CLI_OUTPUT}/definitions"
    WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}"
    COMMENT "Publishing ddsclient CLI for ${DDS_PUBLISH_DESCRIPTION}"
    VERBATIM)
add_dependencies(dds_publish_cli dds_restore_publish)

add_custom_target(dds_all ALL
    DEPENDS dds_pack
    COMMENT "Validate, restore, build, test, and pack DDSClient")
