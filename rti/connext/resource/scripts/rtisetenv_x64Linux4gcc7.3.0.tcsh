#!/bin/tcsh

# Detect script directory
set script=($_)
set script=`echo $script | sed -e 's/.*source //'`
set script_dir=`dirname "$script"`
set host_version=7.3.1
set openssl_version=
set openssl3_version=3.5.1
set civetweb_version=

# Get installation directory
set install_dir=`echo "$script_dir/../.." | sed -e 's/\\ / /g'`

# Get architecture name and host os
set architecture_name=`echo $script | sed -e 's/.*rtisetenv_\(.*\).tcsh/\1/'`
set host_os=`uname -s`

# Set NDDSHOME, CONNEXTDDS_DIR, and PATH
setenv NDDSHOME `cd "$install_dir"; pwd`
setenv CONNEXTDDS_DIR "$NDDSHOME"
setenv CONNEXTDDS_ARCH $architecture_name
setenv PATH "$NDDSHOME/bin":"$PATH"
setenv RTI_LD_LIBRARY_PATH "$NDDSHOME/lib/$architecture_name":"$NDDSHOME/third_party/openssl-$openssl3_version/$architecture_name/release/lib":"$NDDSHOME/third_party/openssl-$openssl_version/$architecture_name/release/lib":"$NDDSHOME/third_party/civetweb-$civetweb_version/$architecture_name/release/lib"

# Set Library Path
if ($host_os == "Darwin") then
    if ( ! $?DYLD_LIBRARY_PATH ) then
        setenv DYLD_LIBRARY_PATH "$RTI_LD_LIBRARY_PATH"
    else
        setenv DYLD_LIBRARY_PATH "$RTI_LD_LIBRARY_PATH":"$DYLD_LIBRARY_PATH"
    endif
else
    if ( ! $?LD_LIBRARY_PATH ) then
        setenv LD_LIBRARY_PATH "$RTI_LD_LIBRARY_PATH"
    else
        setenv LD_LIBRARY_PATH "$RTI_LD_LIBRARY_PATH":"$LD_LIBRARY_PATH"
    endif
endif

echo "###############################################################################"
echo ""
echo "         (c) Copyright, Real-Time Innovations, All rights reserved.            "
echo "                                                                               "
echo "                           RTI Connext DDS $host_version                       "
echo ""
echo "###############################################################################"

