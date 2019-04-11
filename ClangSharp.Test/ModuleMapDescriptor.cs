using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

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
