# LegacyRatchet
LegacyRatchet is a Roslyn diagnostic, meaning it's a plugin for the .NET compiler that runs while you program. Using the principle of a ratchet, it allows you to systematically reduce technical debt in legacy codebases. 

When the plugin detects an increase in technical debt, an error or warning is given depending on your configuration.

The plugin detects technical debt using these simple rules:
* Method has too many parameters
* Method is too long
* Type has too many fields
* Type is too long

The thresholds for these rules can be configured using assembly level attributes.

For the plugin to work it must be allowed to automatically annotate your codebase in places where technical debt occurs.
This allows the plugin to report only *increases* in technical debt.

Here is a screenshot of this plugin in action.
![Screenshot](http://i.imgur.com/Bwj9E8h.png)
