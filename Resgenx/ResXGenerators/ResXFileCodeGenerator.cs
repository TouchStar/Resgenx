//
// ResXFileCodeGenerator.cs
//
// Author:
//   Kenneth Skovhede <kenneth@hexad.dk>
//   Michael Hutchinson <m.j.hutchinson@gmail.com>
//   Bernhard Johannessen <bernhard@voytsje.com>
//   Matthew Diamond <matthewdiamond96@gmail.com>
//
// Copyright (C) 2013 Kenneth Skovhede
// Copyright (C) 2013 Xamarin Inc.
// Copyright (C) 2014 Bernhard Johannessen
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.CodeDom.Compiler;
using System.Linq;
using System.Resources;
using System.Resources.Tools;
using Microsoft.CSharp;

namespace TouchStar.Resgenx.ResXGenerators
{
    public class ResXFileCodeGenerator
    {
        public static async Task GenerateFile(ResXGeneratorOptions options, ResXGeneratorResult result)
        {
            var inputFilePath = options.InputFilePath;
            var targetsPcl2Framework = options.TargetPcl2Framework;
            var outputFilePath = !string.IsNullOrEmpty(options.OutputFilePath) ? options.OutputFilePath
                : $"{Path.GetFileNameWithoutExtension(inputFilePath)}.Designer.cs";

            //no need to escape/cleanup, StronglyTypedResourceBuilder does that
            var className = !string.IsNullOrEmpty(options.ResultClassName) ? options.ResultClassName
                : Path.GetFileNameWithoutExtension(inputFilePath);

            var provider = new CSharpCodeProvider();
            var resourceList = new Dictionary<object, object>();

            await Task.Run(() =>
            {
                if (className == null)
                {
                    result.Errors.Add(new CompilerError(inputFilePath, 0, 0, null,
                        "Class name cannot be null"));
                    return;
                }

                using (var r = new ResXResourceReader(inputFilePath))
                {
                    r.BasePath = Path.GetDirectoryName(inputFilePath);
                    foreach (DictionaryEntry e in r)
                    {
                        resourceList.Add(e.Key, e.Value);
                    }
                }

                string[] unmatchable;
                var ccu = StronglyTypedResourceBuilder.Create(resourceList, className, options.ResultNamespace, provider,
                    options.GenerateInternalClass, out unmatchable);

                if (targetsPcl2Framework)
                {
                    FixupPclTypeInfo(ccu, result);
                }


                foreach (var p in unmatchable)
                {
                    var msg = $"Could not generate property for resource ID '{p}'";
                    result.Errors.Add(new CompilerError(inputFilePath, 0, 0, null, msg));
                }

                // Avoid race if ResXFileCodeGenerator is called more than once for the same file
                using (var fw = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    using (var w = new StreamWriter(fw, Encoding.UTF8))
                        provider.GenerateCodeFromCompileUnit(ccu, w, new CodeGeneratorOptions());
                }
                result.GeneratedFilePath = outputFilePath;
            });
        }

        static CodeObjectCreateExpression GetInitExpr(CodeCompileUnit ccu)
        {
            ccu.Namespaces[0].Imports.Add(new CodeNamespaceImport("System.Reflection"));
            var assignment = ccu.Namespaces[0].Types[0]
                .Members.OfType<CodeMemberProperty>().Single(t => t.Name == "ResourceManager")
                .GetStatements.OfType<CodeConditionStatement>().Single()
                .TrueStatements.OfType<CodeVariableDeclarationStatement>().Single();
            var initExpr = (CodeObjectCreateExpression) assignment.InitExpression;
            return initExpr;
        }

        //works with .NET 4.5.1 and Mono 3.4.0
        static void FixupPclTypeInfo(CodeCompileUnit ccu, ResXGeneratorResult result)
        {
            try
            {
                CodeObjectCreateExpression initExpr = GetInitExpr(ccu);
                var typeofExpr = (CodePropertyReferenceExpression) initExpr.Parameters[1];
                typeofExpr.TargetObject = new CodeMethodInvokeExpression(typeofExpr.TargetObject, "GetTypeInfo");
            }
            catch (Exception ex)
            {
                result.UnhandledException = ex;
            }
        }
    }
}
