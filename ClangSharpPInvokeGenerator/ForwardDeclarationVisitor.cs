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

    internal sealed class ForwardDeclarationVisitor : ICXCursorVisitor
    {
        private readonly CXCursor beginningCursor;

        private bool beginningCursorReached;

        public ForwardDeclarationVisitor(CXCursor beginningCursor)
        {
            this.beginningCursor = beginningCursor;
        }

        public CXCursor ForwardDeclarationCursor { get; private set; }

        public CXChildVisitResult Visit(CXCursor cursor, CXCursor parent, IntPtr data)
        {
            if (cursor.IsInSystemHeader())
            {
                return CXChildVisitResult.CXChildVisit_Continue;
            }

            if (clang.equalCursors(cursor, this.beginningCursor) != 0)
            {
                this.beginningCursorReached = true;
                return CXChildVisitResult.CXChildVisit_Continue;
            }

            if (this.beginningCursorReached)
            {
                this.ForwardDeclarationCursor = cursor;
                return CXChildVisitResult.CXChildVisit_Break;
            }

            return CXChildVisitResult.CXChildVisit_Recurse;
        }
    }
}