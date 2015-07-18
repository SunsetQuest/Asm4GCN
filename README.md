# Asm4GCN - GCN Assembler for AMD GPUs
## an assembler/compiler for AMD’s GCN (Generation Core Next Architecture) Assembly Language

See the Article on CodeProject at http://www.codeproject.com/Articles/872477/Assembler-for-AMD-s-GCN-GPU for more details.

###History
- Feb 16 2015 - Initial Public Release
- Mar 1 2015 - General Fixes and Changes
  - Fix: Variables would always use register 0
  - Fix: Removed single __asm4GCN block limitation - There can now be many __asm4GCN kernels in one program.
  - Change: Parameter names removed - Since the Parameter names are not used having them there could be confusing.  Function headers are now in the form: __asm4GCN myAsmAddFunc (float*,float*){...}
  - Change: merged #ref command into normal variable declarations. Since the #ref command is almost identical to normal variable declarations except that it specifies a register it is best to combine these.  It is cleaner and less confusing. Instead of the format for a ref being #ref s8u myVar s[2:3]  is now just s8u myVar s[2:3].
  - Improved: enlarged the autocomplete box - it now fits the code a little better.
  - Improved: Cleaned up example code.
  - Improved: Sytax Highlighting - it now highlights, labels, registers, and defines.  It also highlights matching words.
  - Removed: Auto compile skip function.  This function would skip a re-compile if there were no changes in the code windows. It was removed because it added complexity in the code and there was hardly any performance benefit since the compile process is so fast anyway. 
   - Added: ren command - A rename command has been added.  This allows a variable to be renamed as its use changes.
- April 22 2015 - Posted on GitHub
- June 1 2015 - Changed the OpenCL event wrapper library to NOpenCL by Sam Harwell plus other minor updates.
