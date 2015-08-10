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
        
        /// <summary>
        /// Finds the nearest matching label with the same name.
        /// </summary>
        /// <param name="name">The label to search for.</param>
        /// <param name="line">Search for matches near this line number.</param>
        /// <param name="log">A log to write warnings/errors to.</param>
        /// <returns>Returns True if found.</returns>
        public bool GetNearestLabel(string name, int line, out Label closestMatch)
        {
            List<Label> ids;
            closestMatch = null;
            bool found = labelDic.TryGetValue(name, out ids);

            if (found)
            {
                // if only one match lets shortcut and take only that one
                if (ids.Count == 1)
                    closestMatch = ids[0];
                else
                {
                    int closestDistance = int.MaxValue;
                    foreach (Label l in ids)
                    {
                        int thisDistance = Math.Abs(line - l.lineNum);
                        if (thisDistance <= closestDistance)
                        {
                            closestDistance = thisDistance;
                            closestMatch = l;
                        }
                    }
                }
            }

            return found;
        }

        /// <summary>
        /// Adds a label to the label dictionary.
        /// </summary>
        /// <param name="newLabel">The new label to add to the Labels container.</param>
        /// <param name="log">Location to write errors and warnings.</param>
        public void AddLabel(Label newLabel, Log log)
        {

            List<Label> ids;
            if (labelDic.TryGetValue(newLabel.labelName, out ids))
            {
                log.Warning("The label, {0}, already exists. References will use the nearest label based on line number.", newLabel.labelName); 
                ids.Add(newLabel);
            }
            else
            {
                ids = new List<Label>();
                ids.Add(newLabel);
                labelDic.Add(newLabel.labelName, ids);
            }
        }

        /// <summary>
        /// Adds a label to the label dictionary.
        /// </summary>
        /// <param name="name">The name of the label. (i.e "myLabel" in myLabel:)</param>
        /// <param name="firstStmt">The next statement following this label.</param>
        /// <param name="log">Location to write errors and warnings.</param>
        public void AddLabel(string name, int lineNum, Stmt firstStmt, Log log)
        {
            Label newLabel = new Label { labelName = name, lineNum = lineNum, firstStmt = firstStmt };
            AddLabel(newLabel, log);
        }
    }


    /// <summary>
    /// Holds information for a single label.
    /// </summary>
    public class Label
    {
        /// <summary>This is the identifier of the label used in a Asm4GCN statements block.</summary>
        public string labelName;

        ///// <summary>The original source code line. When there are multiple matches this value is used to find the nearest matching label.</summary>
        public int lineNum;

        /// <summary>The Statement immediately following the label.</summary>
        public Stmt firstStmt;

        /// <summary>True if this label jumps toe the start of the kernel.</summary>
        public bool isAtBeginning = false;

        /// <summary>True if this label terminates the kernel.</summary>
        public bool isAtEnd = false;

    }
}
