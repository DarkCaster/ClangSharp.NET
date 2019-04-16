using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

#if LINUX_X86_64
using ClangSharp_LINUX_X86_64;
#elif LINUX_X86
using ClangSharp_LINUX_X86;
#elif WINDOWS_X86_64
using ClangSharp_WINDOWS_X86_64;
#else
using ClangSharp;
#endif

namespace ClangSharp.Test
{
    [TestFixture]
    public class ModuleMapDescriptor
    {
        [Test]
        public void Basic()
        {
            var contents =
                "framework module TestFrame {\n"
                + "  umbrella header \"TestFrame.h\"\n"
                + "\n"
                + "  export *\n"
                + "  module * { export * }\n"
                + "}\n";

            CXModuleMapDescriptor mmd = clang.ModuleMapDescriptor_create(0);

            clang.ModuleMapDescriptor_setFrameworkModuleName(mmd, "TestFrame");
            clang.ModuleMapDescriptor_setUmbrellaHeader(mmd, "TestFrame.h");

            IntPtr bufPtr;
            uint bufSize = 0;
            clang.ModuleMapDescriptor_writeToBuffer(mmd, 0, out bufPtr, out bufSize);
            var bufStr = Marshal.PtrToStringAnsi(bufPtr, (int)bufSize);
            Assert.AreEqual(contents, bufStr);
            clang.free(bufPtr);
            clang.ModuleMapDescriptor_dispose(mmd);
        }
    }
}
