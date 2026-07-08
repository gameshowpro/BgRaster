using System.CommandLine;
using System.CommandLine.Help;

var root = new RootCommand("test");
var hb = new HelpBuilder();
hb.CustomizeLayout(ctx => ctx);
