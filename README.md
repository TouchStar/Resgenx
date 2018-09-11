# Resgenx
Converts the given XML based resource file (.resx) to a strongly typed resource class file in C#.

# Why is this needed ?
The Resgen.exe program shipped with Mono does not support generating strongly typed resource classes.

# Usage
```
USAGE: resgenx [OPTIONS] input_file namespace
Converts the given XML based resource file (.resx) to a strongly typed resource class file in C#.

OPTIONS:
  -h, --help, -?             show this message and exit
      --publicClass          generate a 'public' class instead of an 'internal'
                               class
      --pcl2                 generate a class that can be used in projects that
                               target the PCL 2 framework, .Net Core 1.x or .
                               Net Standard 1.x
  -n, --className=VALUE      name of the class to be generated
  -o, --outputPath=VALUE     path to write the generated file
```
