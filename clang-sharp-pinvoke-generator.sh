#!/bin/bash
#

# Wrapper on top of ClangSharpPInvokeGenerator utility
# that will detect clang's default include directories and automatically pass it to generator-utility
# This wrapper script may be also run inside git-for-windows' "Git Bash" shell

set -e

script_dir="$( cd "$( dirname "$0" )" && pwd )"

[[ -z $arch ]] && arch=`uname -m`

# assume that you have clang utility installed
clang=`which clang 2>/dev/null`

if [[ -z $clang && ! -z $MSYSTEM ]]; then
  echo "Trying to detect clang location for windows"
  if [[ $arch == "x86" ]]; then
    #TODO: detect clang in program files (x86)
    true
  elif [[ $arch == "x86_64" ]]; then
    #TODO: detect clang in program files
    true
  fi
  [[ -z $clang ]] && echo "failed to detect clang utility path" && exit 1
fi

echo "Using clang binary at $clang"
resource_dir=`clang -print-resource-dir 2>/dev/null || true`

cmdline=()
cmdline_cnt=0

add_cmdline_param() {
  cmdline[$cmdline_cnt]="$1"
  ((cmdline_cnt=cmdline_cnt+1))
}

useC="false"
arch32bit="false"
pending=""

while (( "$#" )); do
  add_cmdline_param "$1"
  if [[ $1 == "--useC" ]]; then
    pending="useC"
  elif [[ $1 == "--arch32bit" ]]; then
    pending="arch32bit"
  elif [[ $1 == "true" || $1 == "false" ]]; then
    if [[ ! -z $pending ]]; then
      eval $pending="$1"
    fi
    pending=""
  fi
  shift
done

[[ $useC == "true" ]] && useC="-x c" || useC="-x c++"
[[ $arch32bit == "true" && $arch == "x86_64" ]] && arch32bit="-m32" || arch32bit=""

test_source=`mktemp -t CLANG_TEST_XXXXXXXXXXX`

echo "detecting default include directories for clang"
echo "int main(void) { return 0; };" >> $test_source

int_includes=()
int_includes_cnt=0

add_int_include() {
  int_includes[$int_includes_cnt]="--iSystem"
  ((int_includes_cnt=int_includes_cnt+1))
  int_includes[$int_includes_cnt]="$1"
  ((int_includes_cnt=int_includes_cnt+1))
}

include_state="0"

while read line; do
  [[ $include_state == 0 && $line != "#include <...> search starts here:" ]] && continue
  [[ $include_state == 0 && $line == "#include <...> search starts here:" ]] && include_state="1" && continue
  [[ $include_state == 1 && $line == "End of search list." ]] && include_state="2" && continue
  if [[ $include_state == 1 ]]; then
    trline=`echo "$line" | sed -e 's/^[ \t]*//'`
    if [[ ! -z $resource_dir && $trline =~ ^$resource_dir.*$ ]]; then
      add_int_include "$trline"
      echo "found clang-internal system include: $trline"
    fi
  fi
done < <($clang -E $useC $arch32bit -v $test_source 2>&1)

#invoke ClangSharpPInvokeGenerator
[[ $arch == "x86_64" ]] && arch="x64"
mono=`which mono 2>/dev/null`
[[ ! -z $MSYSTEM ]] && profile="Debug_Windows" || profile="Debug_Linux"
echo running $mono "$script_dir/ClangSharpPInvokeGenerator/bin/$arch/$profile/ClangSharpPInvokeGenerator.exe" "${int_includes[@]}" "${cmdline[@]}"

$mono "$script_dir/ClangSharpPInvokeGenerator/bin/$arch/$profile/ClangSharpPInvokeGenerator.exe" "${int_includes[@]}" "${cmdline[@]}"
rm "$test_source"
