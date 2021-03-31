using CliFx.Infrastructure;
using CliFx.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CliFx.Suggestions
{
    class BashEnvironment : ISuggestEnvironment
    {
        public string Version => "V1";

        public string[] SupportedShellPaths => new[] { @"/usr/bin/bash" };

        public string InstallPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".bashrc");

        public string GetInstallCommand(string commandName)
        {
            var safeName = commandName.Replace(" ", "_");
            return $@"
### clifx-suggest-begins-here-{commandName}-{Version}
# this block provides auto-complete for the {commandName} command
# and assumes that {commandName} is on the path
_{safeName}_complete()
{{
  local word=${{COMP_WORDS[COMP_CWORD]}}

  # generate unique environment variable
  CLIFX_CMD_CACHE=""clifx-suggest-$(uuidgen)""
  # replace hyphens with underscores to make it valid
  CLIFX_CMD_CACHE=${{CLIFX_CMD_CACHE//\-/_}}

  export $CLIFX_CMD_CACHE=${{COMP_LINE}}

  local completions
  completions=""$({commandName} ""[suggest]"" --cursor ""${{COMP_POINT}}"" --envvar $CLIFX_CMD_CACHE 2>/dev/null)""
  if [ $? -ne 0]; then
    completions=""""
  fi

  unset $CLIFX_CMD_CACHE

  COMPREPLY=( $(compgen -W ""$completions"" -- ""$word"") )
}}

complete -f -F _{commandName}_complete ""{commandName}""

### clifx-suggest-ends-here-{safeName}";
        }
    }
}
