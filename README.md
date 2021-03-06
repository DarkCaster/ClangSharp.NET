# ClangSharp

ClangSharp are strongly-typed safe Clang bindings written in C# for .NET and Mono, tested on Linux and Windows. ClangSharp is self-hosted and ClangSharp auto-generates itself parsing LLVM-C header files.

## About this fork

Main goal of this fork is to provide more reliable ClangSharpPInvokeGenerator utility
that will produce predictable results for my usecase of generating interop-mappings for my native libraries with complicated API.
Generator utility should work now with Mono/Linux (x86 and x86 archs).
For now I've ahieved reliable code-generation results for my usecase when using clang4 on Mono/Linux with x86_64 arch.
Other clang versions and OS'es combination may not to work so good.

## _Custom improvements to ClangSharpPInvokeGenerator utility compared to original project_

 * Project converted into regular .NET lib/app using .NET 4.7.2 profile (tested with Mono 5.20 on Linux x86_64).
 * Added support for unions (experimental, may be unstable), cannot be disabled for now.
 * Added option to convert all "char" fields to "byte". Cmdline option: --charToByte < true | false >
 * Fix generation for size_t/int64_t/long and similiar types when using 64bit libclang (tested on libclang.so.4 with Mono/Linux)
 * When running on 64-bit systems with 64-bit clang, you may force clang to generate code for 32-bit arch, this will alter code-generation for long types like size_t/int64_t/long e.t.c . Cmdline option: --arch32bit < true | false >
 * Added option to parse header files with C language instead of C++. Cmdline option: --useC < true | false >
 * Added option to generate StructLayout attributes for structures. Cmdline option: --seqStructs < true | false >
 * Added option to set CharSet=CharSet.Ansi property to StructLayout attributes. Cmdline option: --ansiStructs < true | false >
 * Added option to generate helper-properties for providing convenient access to arrays. Cmdline option: --arrayHelpers < true | false >
 * Fix parsing of some nested structs and unions, which sometimes mistakenly considered as anonymous. This feature may be unstable. Cmdline option: --fixNestedStructs < true | false >
 * Added option to generate delegates instead of static extern methods. May be used to dynamically access native library methods with kernel32.dll::GetProcAddress (or libdl.so::dlsym on Linux/Mono). Cmdline option: --genDelegates < true | false
 * Added option to insert system include directories to clang with options "--iSystem" and "--iSystemAfter" (equals to "-isystem" and "-isystem-after" clang options)
 * Added option to disable standard builtin include directories. Cmdline option: --noStdIncludes < true | false >
 *
## Building ClangSharp (example for Linux/Mono)

```bash
msbuild /target:ClangSharpPInvokeGenerator:Rebuild /property:Configuration=Debug_Linux,Platform="x64" ClangSharp.sln
```

## Features

 * Auto-generated using Clang C headers files, and supports all functionality exposed by them ~ which means you can build tooling around C/C++
 * Type safe (CXIndex and CXTranslationUnit are different types, despite being pointers internally)
 * Nearly identical to Clang C APIs, e.g. clang_getDiagnosticSpelling in C, vs. clang.getDiagnosticSpelling (notice the . in the C# API)

## ClangSharp PInvoke Generator

A great example of ClangSharp's use case is its self-hosting mechanism [Clang Sharp PInvoke Generator](https://github.com/mjsabby/ClangSharp/tree/master/ClangSharpPInvokeGenerator)

## Microsoft Open Source Code of Conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
