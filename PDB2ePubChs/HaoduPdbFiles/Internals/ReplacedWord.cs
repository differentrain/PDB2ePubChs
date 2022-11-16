using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PDB2ePubChs.HaoduPdbFiles.Internals
{
    internal sealed class ReplacedWord
    {
        public ReplacedWord(ReplacedChar date)
        {
            Reg = new Regex(date.Org, RegexOptions.Compiled);
            Pattern = date.Rep;
        }

        public readonly Regex Reg;
        public readonly string Pattern;
    }
}
