#!/bin/bash

# Detect script directory
script="${BASH_SOURCE[0]}"
script_dir=`dirname "$script"`
host_version=7.3.1
openssl_version=
openssl3_version=3.5.1
civetweb_version=

# Get installation directory
install_dir=`cd "$script_dir/../.."; pwd`

# Get architecture name and host os
architecture_name=`echo $script | sed -e 's/.*rtisetenv_\(.*\).bash/\1/'`
host_os=`uname -s`

# Export NDDSHOME, CONNEXTDDS_DIR, and PATH
NDDSHOME="$install_dir"; export NDDSHOME
CONNEXTDDS_DIR="$install_dir"; export CONNEXTDDS_DIR
CONNEXTDDS_ARCH=$architecture_name; export CONNEXTDDS_ARCH
PATH="$NDDSHOME/bin":$PATH; export PATH
RTI_LD_LIBRARY_PATH="$NDDSHOME/lib/$architecture_name":"$NDDSHOME/third_party/openssl-$openssl3_version/$architecture_name/release/lib":"$NDDSHOME/third_party/openssl-$openssl_version/$architecture_name/release/lib":"$NDDSHOME/third_party/civetweb-$civetweb_version/$architecture_name/release/lib"; export RTI_LD_LIBRARY_PATH

# Export library path
if [ "$host_os" != "Darwin" ]; then
    LD_LIBRARY_PATH=$RTI_LD_LIBRARY_PATH:$LD_LIBRARY_PATH; export LD_LIBRARY_PATH
else
    DYLD_LIBRARY_PATH=$RTI_LD_LIBRARY_PATH:$DYLD_LIBRARY_PATH; export DYLD_LIBRARY_PATH
fi

echo "###############################################################################"
echo ""
echo "         (c) Copyright, Real-Time Innovations, All rights reserved.            "
echo "                                                                               "
echo "                           RTI Connext DDS $host_version                       "
echo ""
echo "###############################################################################"
