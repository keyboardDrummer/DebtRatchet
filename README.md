# RoslynDebtAnalyzer
Visual studio plugin that helps reduce technical debt in legacy codebases.

This plugin detects increases in your code's technical debt. Depending on your configuration, 
an error or warning is given when technical debt increases.

For the plugin to work it must be allowed to automatically annotate your codebase in places where technical debt currently occurs.
This allows the plugin to report only *increases* in technical debt.

The current technical debt checks that are available are the following:
* Method has too many parameters
* Method is too long
* Type has too many fields
* Type is too long

The plugin can be configured using assembly annotations. 
