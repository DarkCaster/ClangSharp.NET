#if LINUX_X86_64
namespace ClangSharp_LINUX_X86_64
#elif LINUX_X86
namespace ClangSharp_LINUX_X86
#elif WINDOWS_X86_64
namespace ClangSharp_WINDOWS_X86_64
#else
namespace ClangSharp
#endif
{
    public partial struct CXString
    {
        public override string ToString()
        {
            string retval = clang.getCString(this);
            clang.disposeString(this);
            return retval;
        }
    }

    public partial struct CXType
    {
        public override string ToString()
        {
            return clang.getTypeSpelling(this).ToString();
        }
    }

    public partial struct CXCursor
    {
        public override string ToString()
        {
            return clang.getCursorSpelling(this).ToString();
        }
    }

    public partial struct CXDiagnostic
    {
        public override string ToString()
        {
            return clang.getDiagnosticSpelling(this).ToString();
        }
    }
}