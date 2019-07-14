# How to use

Run DependencyWalker.exe with -? or --help flags for documentation on parameters.

## Main parameters
Broadly speaking, these are the parameters you are most likely interested in:

Parameter | Description | Example Usage
----------|-------------|------
-s | A path to a Visual Studio solution file to analyse | `-s "C:\dev\someproject\MySolution.sln"`

Modify the class properties in Program.cs and then execute. You need the following properties set:
- PackageSources
- Dependencies of Interest (if you want to filter the graph to a specific sub dependency only, otherwise leave empty)
- SolutionToAnalyse





#Attribution
This project was inspired by many blog posts and benefitted from the work of others so credit where credit is due:

https://stackoverflow.com/questions/6653715/view-nuget-package-dependency-hierarchy
https://icanhasdot.net/Console
https://github.com/ThomasArdal/NuGetPackageVisualizer
https://andydote.co.uk/2016/09/12/nuget-dependencies/
http://pascallaurin42.blogspot.com/2014/06/visualizing-nuget-packages-dependencies.html
https://gist.github.com/plaurin/b4bc53428f01dc722afb
https://gist.github.com/sergey-tihon/46824acffb8c288fc5fe