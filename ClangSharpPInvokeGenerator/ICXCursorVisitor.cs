namespace ClangSharpPInvokeGenerator
{
    using System;

#if LINUX_X86_64
    using ClangSharp_LINUX_X86_64;
#elif LINUX_X86
    using ClangSharp_LINUX_X86;
#elif WINDOWS_X86_64
    using ClangSharp_WINDOWS_X86_64;
#else
    using ClangSharp;
#endif

    internal interface ICXCursorVisitor
    {
        CXChildVisitResult Visit(CXCursor cursor, CXCursor parent, IntPtr data);
    }
}