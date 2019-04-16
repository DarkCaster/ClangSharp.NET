namespace ClangSharpPInvokeGenerator
{
    using System;
    using System.Collections.Generic;
    using System.IO;

#if LINUX_X86_64
    using ClangSharp_LINUX_X86_64;
#elif LINUX_X86
    using ClangSharp_LINUX_X86;
#elif WINDOWS_X86_64
    using ClangSharp_WINDOWS_X86_64;
#else
    using ClangSharp;
#endif

    internal sealed class StructVisitor : ICXCursorVisitor
    {
        public static volatile bool isSequential = false;

        public static volatile bool isAnsi = false;

        public static Stack<bool> selectedUnion = new Stack<bool>();

        private readonly ISet<string> visitedStructs = new HashSet<string>();

        private readonly TextWriter tw;

        private int indentLevel = 1;

        private int fieldPosition;

        private const int indentMultiplier = 4;

        public StructVisitor(TextWriter tw)
        {
            this.tw = tw;
        }

        public CXChildVisitResult Visit(CXCursor cursor, CXCursor parent, IntPtr data)
        {
            if (cursor.IsInSystemHeader())
            {
                return CXChildVisitResult.CXChildVisit_Continue;
            }

            CXCursorKind curKind = clang.getCursorKind(cursor);
            if (curKind == CXCursorKind.CXCursor_StructDecl || curKind == CXCursorKind.CXCursor_UnionDecl)
            {
                this.fieldPosition = 0;
                var structName = clang.getCursorSpelling(cursor).ToString();

                // struct names can be empty, and so we visit its sibling to find the name
                if (string.IsNullOrEmpty(structName))
                {
                    var forwardDeclaringVisitor = new ForwardDeclarationVisitor(cursor);
                    clang.visitChildren(clang.getCursorSemanticParent(cursor), forwardDeclaringVisitor.Visit, new CXClientData(IntPtr.Zero));
                    structName = clang.getCursorSpelling(forwardDeclaringVisitor.ForwardDeclarationCursor).ToString();

                    if (string.IsNullOrEmpty(structName))
                    {
                        structName = "_";
                    }
                }

                if (!this.visitedStructs.Contains(structName))
                {
                    if(curKind == CXCursorKind.CXCursor_UnionDecl)
                        this.IndentedWriteLine("[StructLayout(LayoutKind.Explicit" + (isAnsi?", CharSet=CharSet.Ansi":"")+")]");
                    else if(isSequential)
                        this.IndentedWriteLine("[StructLayout(LayoutKind.Sequential" + (isAnsi?", CharSet=CharSet.Ansi":"")+")]");
                    this.IndentedWriteLine("public partial struct " + structName);
                    this.IndentedWriteLine("{");

                    this.indentLevel++;
                    selectedUnion.Push(curKind == CXCursorKind.CXCursor_UnionDecl);
                    clang.visitChildren(cursor, this.Visit, new CXClientData(IntPtr.Zero));
                    selectedUnion.Pop();
                    this.indentLevel--;

                    this.IndentedWriteLine("}");
                    this.tw.WriteLine();

                    this.visitedStructs.Add(structName);
                }

                return CXChildVisitResult.CXChildVisit_Continue;
            }

            if (curKind == CXCursorKind.CXCursor_FieldDecl)
            {
                var fieldName = clang.getCursorSpelling(cursor).ToString();
                if (string.IsNullOrEmpty(fieldName))
                {
                    fieldName = "field" + this.fieldPosition; // what if they have fields called field*? :)
                }

                this.fieldPosition++;
                if (selectedUnion.Count > 0 && selectedUnion.Peek())
                    this.IndentedWriteLine("[FieldOffset(0)]");
                this.IndentedWriteLine(cursor.ToMarshalString(fieldName));
                return CXChildVisitResult.CXChildVisit_Continue;
            }

            return CXChildVisitResult.CXChildVisit_Recurse;
        }

        private void IndentedWriteLine(string s)
        {
            for (int i = 0; i < indentMultiplier * indentLevel; ++i)
            {
                this.tw.Write(" ");
            }

            this.tw.WriteLine(s);
        }
    }
}