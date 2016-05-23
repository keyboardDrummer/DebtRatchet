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

Below is a screenshot of this plugin in action. It demonstrates four scenario's related to the rule 'Method has too many parameters'. The assembly level annotations specify that a method may not have more than five parameters, and that voilations of this rule result in a compile error. The four scenario show the following cases:

1. A method has exactly five parameters so it is within the limit.
2. A method has six parameters, so it is over the limit and gives an error.
3. A legacy method has more than six parameters. Since it is legacy we don't wish to fix this error right now and have annotated the method so that it does not give an error.
4. A legacy method with too many parameters has gained even more parameters after we started using DebtRatchet and annotated it with the DebtMethod annotations. Because we don't want our legacy technical debt to grow, this method does give an error.

![Screenshot](http://i.imgur.com/Dw43jz2.png)
