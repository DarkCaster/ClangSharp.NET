#!/bin/bash
#

# generate mappings for linux version of clang library

set -e

script_dir="$( cd "$( dirname "$0" )" && pwd )"

# copy clang includes
clang_dir=`mktemp -d -t CLANG_INCLUDES_XXXXXXXXXXX`

arch=`uname -m`
[[ $arch == "x86_64" ]] && arch = "x64"

cp -r /usr/include/clang-c "$clang_dir"

build_generator() {
  return 0
}

#--excludeFunctions

generate_mappings() {
  mono "$script_dir/ClangSharpPInvokeGenerator/bin/$arch/Debug_Linux/ClangSharpPInvokeGenerator.exe" \
  --namespace ClangSharp_Linux_$arch \
  --l libclang \
  --file "$clang_dir/clang-c/Platform.h" \
  --file "$clang_dir/clang-c/CXErrorCode.h" \
  --file "$clang_dir/clang-c/CXString.h" \
  --file "$clang_dir/clang-c/BuildSystem.h" \
  --file "$clang_dir/clang-c/Documentation.h" \
  --file "$clang_dir/clang-c/CXCompilationDatabase.h" \
  --file "$clang_dir/clang-c/Index.h" \
  --include "$clang_dir" \
  --include "/usr/include" \
  --output "$script_dir/ClangSharp/Generated_Linux_$arch.cs.tmp"

  echo "#if LINUX_$arch" | cat - "$script_dir/ClangSharp/Generated_Linux_$arch.cs.tmp" > \
  "$script_dir/ClangSharp/Generated_Linux_$arch.cs"
  echo "#endif" >> "$script_dir/ClangSharp/Generated_Linux_$arch.cs"
  rm "$script_dir/ClangSharp/Generated_Linux_$arch.cs.tmp"

  sed -i "s| clang_| |g" "$script_dir/ClangSharp/Generated_Linux_$arch.cs"
  sed -i "s|Methods|clang|g" "$script_dir/ClangSharp/Generated_Linux_$arch.cs"
}

build_generator
generate_mappings
