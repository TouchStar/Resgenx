// 
// CustomTool.cs
//  
// Author:
//       Michael Hutchinson <mhutchinson@novell.com>
// 
// Copyright (c) 2010 Novell, Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.CodeDom.Compiler;

namespace TouchStar.Resgenx.ResXGenerators
{
    public class ResXGeneratorResult
    {
        /// <summary>
        /// Errors and warnings from the generator.
        /// </summary>
        public CompilerErrorCollection Errors { get; } = new CompilerErrorCollection();

        /// <summary>
        /// The absolute name of the generated file. Must be in same directory as source file.
        /// </summary>
        public string GeneratedFilePath { get; set; }

        /// <summary>
        /// Any unhandled exception from the generator.
        /// </summary>
        public Exception UnhandledException { get; set; }

        public bool Success {
            get {
                return UnhandledException == null && !Errors.HasErrors && !Errors.HasWarnings;
            }
        }

        public bool SuccessWithWarnings {
            get {
                return UnhandledException == null && !Errors.HasErrors && Errors.HasWarnings;
            }
        }
    }

    public class ResXGeneratorOptions
    {
        public string InputFilePath { get; }

        public string ResultNamespace { get; }
        public string ResultClassName { get; set; }
        public string OutputFilePath { get; set; }
        public bool GenerateInternalClass { get; set; }
        /// <summary>
        /// When true, generates a class that can be used in projects that target the PCL 2 Framework, .Net Core 1.x or
        /// .Net Standard 1.x
        /// </summary>
        public bool TargetPcl2Framework { get; set; }

        public ResXGeneratorOptions(string inputFilePath, string resultNamespace)
        {
            if (string.IsNullOrEmpty(inputFilePath))
                throw new ArgumentNullException($"Argument {nameof(inputFilePath)} cannot be null or empty");

            if (string.IsNullOrEmpty(resultNamespace))
                throw new ArgumentNullException($"Argument {nameof(resultNamespace)} cannot be null or empty");

            InputFilePath = inputFilePath;
            ResultNamespace = resultNamespace;
        }

    }
}

