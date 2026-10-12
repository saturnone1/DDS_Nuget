cmake_minimum_required(VERSION 3.20)

project(DdsAmbassadorDDSClient NONE)

option(DDS_GENERATE "Run RTI rtiddsgen before validating/building." OFF)
option(DDS_CLEAN_GENERATED "Remove generated C# files before rtiddsgen when DDS_GENERATE is ON." ON)
option(DDS_BUILD_CLI "Build the explicit DDSSim example CLI." OFF)
option(DDS_BUILD_TESTS "Build the explicit sample-dependent tests." OFF)
option(DDS_SKIP_TESTS "Skip the dds_test step in dds_all." OFF)

set(DDS_RUNTIME_ID "" CACHE STRING "Optional runtime identifier used by dds_publish_cli. Leave empty for a portable framework-dependent publish.")
set(DDS_PACKAGE_VERSION "" CACHE STRING "Optional NuGet package version passed to dotnet pack.")
set(DDS_DOTNET_CONFIGURATION "Release" CACHE STRING "dotnet build/test/pack configuration.")

set(DDS_LIBRARY_PROJECT "${CMAKE_CURRENT_SOURCE_DIR}/core/DdsAmbassador.DDSClient.Core.csproj")
set(DDS_CLI_PROJECT "${CMAKE_CURRENT_SOURCE_DIR}/../cli/csharp/DdsAmbassador.DDSClient.Cli.csproj")
set(DDS_TEST_PROJECT "${CMAKE_CURRENT_SOURCE_DIR}/tests/DdsAmbassador.DDSClient.Tests.csproj")
set(DDS_PACKAGE_OUTPUT "${CMAKE_CURRENT_SOURCE_DIR}/artifacts/packages")
set(DDS_CLI_OUTPUT "${CMAKE_CURRENT_SOURCE_DIR}/artifacts/ddsclient")

find_program(DOTNET_EXECUTABLE dotnet REQUIRED)
find_package(Python3 REQUIRED COMPONENTS Interpreter)

# XML 추가·삭제는 다음 일반 빌드에서 자동으로 다시 발견한다.
set(DDS_SCHEMA_GENERATOR "${CMAKE_CURRENT_SOURCE_DIR}/../tools/codegen/schema_modules.py")
file(GLOB DDS_SCHEMA_INPUTS CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/../definitions/*.xml")
set_property(DIRECTORY APPEND PROPERTY CMAKE_CONFIGURE_DEPENDS ${DDS_SCHEMA_INPUTS} "${DDS_SCHEMA_GENERATOR}")
set_property(DIRECTORY APPEND PROPERTY CMAKE_CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/../tools/codegen/isolate_types.py")
file(GLOB DDS_CODEGEN_BACKENDS CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/../tools/codegen/backends/*.py")
set_property(DIRECTORY APPEND PROPERTY CMAKE_CONFIGURE_DEPENDS ${DDS_CODEGEN_BACKENDS}
    "${CMAKE_CURRENT_SOURCE_DIR}/../tools/codegen/codegen_io.py")
execute_process(COMMAND "${Python3_EXECUTABLE}" "${DDS_SCHEMA_GENERATOR}"
    --definitions "${CMAKE_CURRENT_SOURCE_DIR}/../definitions" --list --manifest "${CMAKE_CURRENT_BINARY_DIR}/message-modules.txt"
    --signal-forge-props "${CMAKE_CURRENT_BINARY_DIR}/signal-forge-modules.props"
    OUTPUT_VARIABLE DDS_AUTO_MODULES OUTPUT_STRIP_TRAILING_WHITESPACE COMMAND_ERROR_IS_FATAL ANY)
set(DDS_CLI_RESTORE_COMMANDS)
set(DDS_CLI_BUILD_COMMANDS)
set(DDS_TEST_RESTORE_COMMANDS)
set(DDS_TEST_BUILD_COMMANDS)
if(DDS_BUILD_CLI)
    if(NOT "DDSSim" IN_LIST DDS_AUTO_MODULES)
        message(FATAL_ERROR "DDSSim 예제 CLI를 선택했지만 해당 XML이 없습니다.")
    endif()
    list(APPEND DDS_CLI_RESTORE_COMMANDS COMMAND "${DOTNET_EXECUTABLE}" restore "${DDS_CLI_PROJECT}")
    list(APPEND DDS_CLI_BUILD_COMMANDS COMMAND "${DOTNET_EXECUTABLE}" build "${DDS_CLI_PROJECT}" -c "${DDS_DOTNET_CONFIGURATION}" --no-restore)
endif()
if(DDS_BUILD_TESTS)
    foreach(fixture IN ITEMS DDSSim Telemetry)
        if(NOT fixture IN_LIST DDS_AUTO_MODULES)
            message(FATAL_ERROR "선택한 예제 테스트의 모듈이 없습니다: ${fixture}")
        endif()
    endforeach()
    list(APPEND DDS_TEST_RESTORE_COMMANDS COMMAND "${DOTNET_EXECUTABLE}" restore "${DDS_TEST_PROJECT}")
    list(APPEND DDS_TEST_BUILD_COMMANDS COMMAND "${DOTNET_EXECUTABLE}" build "${DDS_TEST_PROJECT}" -c "${DDS_DOTNET_CONFIGURATION}" --no-restore)
endif()

set(DDS_RTI_HOME "$ENV{NDDSHOME}" CACHE PATH "RTI host installation for automatic message generation")
execute_process(COMMAND "${Python3_EXECUTABLE}" "${DDS_SCHEMA_GENERATOR}"
    --definitions "${CMAKE_CURRENT_SOURCE_DIR}/../definitions"
    --signal-forge-props "${CMAKE_CURRENT_SOURCE_DIR}/generated/BuiltModules.props"
    COMMAND_ERROR_IS_FATAL ANY)
if(NOT DDS_RTI_HOME AND WIN32)
    file(GLOB DDS_RTI_CANDIDATES "C:/Program Files/rti_connext_dds-*")
    list(SORT DDS_RTI_CANDIDATES COMPARE NATURAL ORDER DESCENDING)
    if(DDS_RTI_CANDIDATES)
        list(GET DDS_RTI_CANDIDATES 0 DDS_RTI_HOME)
    endif()
endif()
set(DDS_AUTO_RESTORE_COMMANDS)
set(DDS_AUTO_BUILD_COMMANDS)
set(DDS_AUTO_PACK_COMMANDS)
add_custom_target(dds_auto_modules)

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

foreach(module IN LISTS DDS_AUTO_MODULES)
    set(module_dir "${CMAKE_CURRENT_SOURCE_DIR}/generated/${module}")
    set(module_project "${module_dir}/DdsAmbassador.Messages.${module}.csproj")
    add_custom_command(TARGET dds_auto_modules POST_BUILD
        COMMAND "${Python3_EXECUTABLE}" "${DDS_SCHEMA_GENERATOR}"
            --xml "${CMAKE_CURRENT_SOURCE_DIR}/../definitions/${module}.xml" --output "${module_dir}"
            --language "c#" --rti-home "${DDS_RTI_HOME}"
            --core "${CMAKE_CURRENT_SOURCE_DIR}/core/DdsAmbassador.DDSClient.Core.csproj" VERBATIM)
    list(APPEND DDS_AUTO_RESTORE_COMMANDS COMMAND "${DOTNET_EXECUTABLE}" restore "${module_project}"
        --configfile "${CMAKE_CURRENT_SOURCE_DIR}/NuGet.config")
    list(APPEND DDS_AUTO_BUILD_COMMANDS COMMAND "${DOTNET_EXECUTABLE}" build "${module_project}"
        -c "${DDS_DOTNET_CONFIGURATION}" --no-restore)
    list(APPEND DDS_AUTO_PACK_COMMANDS COMMAND "${DOTNET_EXECUTABLE}" pack "${module_project}"
        -c "${DDS_DOTNET_CONFIGURATION}" --no-build --no-restore -o "${DDS_PACKAGE_OUTPUT}" ${DDS_PACK_VERSION_ARGS})
endforeach()

if(CMAKE_GENERATOR MATCHES "Visual Studio")
    list(APPEND CMAKE_VS_GLOBALS "ForceImportBeforeCppTargets=${CMAKE_CURRENT_SOURCE_DIR}/../tools/build/utf8.props")
endif()
add_custom_target(dds_generate DEPENDS dds_auto_modules)
add_custom_target(dds_core
    COMMAND "${DOTNET_EXECUTABLE}" restore "${DDS_LIBRARY_PROJECT}" --configfile "${CMAKE_CURRENT_SOURCE_DIR}/NuGet.config"
    COMMAND "${DOTNET_EXECUTABLE}" build "${DDS_LIBRARY_PROJECT}" -c "${DDS_DOTNET_CONFIGURATION}" --no-restore
    VERBATIM)

set(DDS_VALIDATION_TEST_COMMANDS)
if(DDS_BUILD_TESTS AND NOT DDS_SKIP_TESTS)
    list(APPEND DDS_VALIDATION_TEST_COMMANDS COMMAND "${Python3_EXECUTABLE}" "${CMAKE_CURRENT_SOURCE_DIR}/../tools/codegen/test_schema_modules.py")
endif()
add_custom_target(dds_validate
    COMMAND "${Python3_EXECUTABLE}" -X utf8 -c "print('메시지 XML·QoS·생성된 C# 타입을 검증합니다')"
    ${DDS_VALIDATION_TEST_COMMANDS}
    COMMAND "${Python3_EXECUTABLE}" "${CMAKE_CURRENT_SOURCE_DIR}/../tools/codegen/validate-dds.py"
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
    COMMENT ""
    VERBATIM)

add_dependencies(dds_validate dds_generate)

add_custom_target(dds_restore
    COMMAND "${Python3_EXECUTABLE}" -X utf8 -c "print('NuGet 의존 패키지를 복원합니다')"
    ${DDS_AUTO_RESTORE_COMMANDS}
    COMMAND "${DOTNET_EXECUTABLE}" restore "${DDS_LIBRARY_PROJECT}"
    ${DDS_CLI_RESTORE_COMMANDS}
    ${DDS_TEST_RESTORE_COMMANDS}
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
    COMMENT ""
    VERBATIM)
add_dependencies(dds_restore dds_validate dds_auto_modules)

if(DDS_BUILD_CLI)
add_custom_target(dds_restore_publish
    COMMAND "${Python3_EXECUTABLE}" -X utf8 -c "print('ddsclient CLI 배포 의존성을 복원합니다: ${DDS_PUBLISH_DESCRIPTION}')"
    COMMAND "${DOTNET_EXECUTABLE}" restore "${DDS_CLI_PROJECT}" ${DDS_PUBLISH_RUNTIME_ARGS}
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
    COMMENT ""
    VERBATIM)
add_dependencies(dds_restore_publish dds_build)
endif()

add_custom_target(dds_build
    COMMAND "${Python3_EXECUTABLE}" -X utf8 -c "print('DDSClient 프로젝트를 빌드합니다')"
    ${DDS_AUTO_BUILD_COMMANDS}
    COMMAND "${DOTNET_EXECUTABLE}" build "${DDS_LIBRARY_PROJECT}" -c "${DDS_DOTNET_CONFIGURATION}" --no-restore
    ${DDS_CLI_BUILD_COMMANDS}
    ${DDS_TEST_BUILD_COMMANDS}
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
    COMMENT ""
    VERBATIM)
add_dependencies(dds_build dds_restore)

if(DDS_BUILD_TESTS)
add_custom_target(dds_test
    COMMAND "${Python3_EXECUTABLE}" -X utf8 -c "print('DDSClient 테스트를 실행합니다')"
    COMMAND "${DOTNET_EXECUTABLE}" test "${DDS_TEST_PROJECT}" -c "${DDS_DOTNET_CONFIGURATION}" --no-restore --no-build
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
    COMMENT ""
    VERBATIM)
add_dependencies(dds_test dds_build)
endif()

add_custom_target(dds_pack
    COMMAND "${Python3_EXECUTABLE}" -X utf8 -c "print('DDSClient NuGet 패키지를 생성합니다')"
    ${DDS_AUTO_PACK_COMMANDS}
    COMMAND "${DOTNET_EXECUTABLE}" pack "${DDS_LIBRARY_PROJECT}" -c "${DDS_DOTNET_CONFIGURATION}" --no-restore --no-build -o "${DDS_PACKAGE_OUTPUT}" ${DDS_PACK_VERSION_ARGS}
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
    COMMENT ""
    VERBATIM)
if(DDS_SKIP_TESTS OR NOT DDS_BUILD_TESTS)
    add_dependencies(dds_pack dds_build)
else()
    add_dependencies(dds_pack dds_test)
endif()

if(DDS_BUILD_CLI)
add_custom_target(dds_publish_cli
    COMMAND "${Python3_EXECUTABLE}" -X utf8 -c "print('ddsclient CLI를 배포합니다: ${DDS_PUBLISH_DESCRIPTION}')"
    COMMAND "${DOTNET_EXECUTABLE}" publish "${DDS_CLI_PROJECT}" -c "${DDS_DOTNET_CONFIGURATION}" ${DDS_PUBLISH_RUNTIME_ARGS} --self-contained false --no-restore -o "${DDS_CLI_OUTPUT}"
    COMMAND "${CMAKE_COMMAND}" -E make_directory "${DDS_CLI_OUTPUT}/definitions"
    COMMAND "${CMAKE_COMMAND}" -E copy_if_different "${CMAKE_CURRENT_SOURCE_DIR}/../definitions/qos_profiles.xml" "${DDS_CLI_OUTPUT}/definitions/qos_profiles.xml"
    COMMAND "${CMAKE_COMMAND}" -E rm -f
        "${DDS_CLI_OUTPUT}/definitions/DDSSim.xml"
        "${DDS_CLI_OUTPUT}/definitions/dds_client.xml"
        "${DDS_CLI_OUTPUT}/definitions/dds_client.asap.xml"
        "${DDS_CLI_OUTPUT}/definitions/dds_client.rti-multicast.xml"
    WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
    COMMENT ""
    VERBATIM)
add_dependencies(dds_publish_cli dds_restore_publish)
endif()

add_custom_target(dds_all ALL
    COMMAND "${Python3_EXECUTABLE}" -X utf8 -c "print('DDSClient 검증·복원·빌드·테스트·패키징')"
    DEPENDS dds_pack
    VERBATIM
    COMMENT "")
