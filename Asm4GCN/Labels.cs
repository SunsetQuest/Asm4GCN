// Asm4GCN Assembler by Ryan S White (sunsetquest) http://www.codeproject.com/Articles/872477/Assembler-for-AMD-s-GCN-GPU
// Released under the Code Project Open License (CPOL) http://www.codeproject.com/info/cpol10.aspx 
// Source & Executable can be used in commercial applications and is provided AS-IS without any warranty.
using System;
using System.Collections.Generic;

namespace GcnTools
{
    
    /// <summary>
    /// The Labels class contains a dictionary of all the labels with their names and locations.
    /// </summary>
    public class Labels
    {
        Dictionary<string, List<Label>> labelDic = new Dictionary<string, List<Label>>();
        
        public Label GetNearestLabel(string name, int line, Log log)
        {
            List<Label> ids;
            if (!labelDic.TryGetValue(name, out ids))
            {
                log.Error("Cannot find Label '{0}'", name);
                return new Label();
            }
            else
            {
                // if only one match lets shortcut and take only that one
                if (ids.Count == 1)
                    return ids[0];

                int closestDistance = int.MaxValue;
                Label closestMatch = new Label();
                foreach (Label l in ids)
	            {
		            int thisDistance = Math.Abs(line - l.sourceLineNum);
		            if (thisDistance < closestDistance)
                    {
                        closestDistance = thisDistance;
                        closestMatch = l;
                    }
	            }
                return closestMatch;
            }
        }

        /// <summary>
        /// Adds a label to the label dictionary.
        /// </summary>
        /// <param name="name">The name of the label. (i.e "myLabel" in myLabel:)</param>
        /// <param name="stmtLoc">Where is the label located.</param>
        /// <param name="sourceLineNum">The original source code location. (for displaying errors)</param>
        /// <param name="log">Location to write errors and warnings.</param>
        public void AddLabel(string name, int stmtLoc, int sourceLineNum, Log log)
        {
            Label newLabel = new Label { labelName = name, stmtLoc = stmtLoc, sourceLineNum = sourceLineNum };

            List<Label> ids;
            if (labelDic.TryGetValue(name, out ids))
            {
                log.Warning("A label named '{0}' already exists. References will use the nearest label based on line number.", name); 
                ids.Add(newLabel);
            }
            else
            {
                ids = new List<Label>();
                ids.Add(newLabel);
                labelDic.Add(name, ids);
            }
        }
    }


    /// <summary>
    /// Holds information for a single label.
    /// </summary>
    public class Label
    {
        /// <summary>This is the identifier of the label used in a Asm4GCN statements block.</summary>
        public string labelName;

        /// <summary>The original source code line. When there are multiple matches this value is used to find the nearest matching label.</summary>
        public int sourceLineNum;

        /// <summary>The location(or id) of where the label is. (what statement is it directly before)</summary>
        public int stmtLoc;
    }
}
