// Asm4GCN Assembler by Ryan S White (sunsetquest) http://www.codeproject.com/Articles/872477/Assembler-for-AMD-s-GCN-GPU
// Released under the Code Project Open License (CPOL) http://www.codeproject.com/info/cpol10.aspx 
// Source & Executable can be used in commercial applications and is provided AS-IS without any warranty.
using System;
using System.Text;

namespace GcnTools
{
    /// <summary>
    /// This is a logging class for handing output. It can output to a StringBuilder or directly to a console.
    /// </summary>
    public class Log
    {
        /// <summary>This is the current committed log entries.</summary>
        StringBuilder commitedMessages = new StringBuilder();

        /// <summary>When paused, these are messages that have not been committed. It is still possible to undo the messages since being paused.</summary>
        StringBuilder pendingMessages = new StringBuilder();

        /// <summary>Indicates that the message log has an error message.</summary>
        public bool hasErrors = false;

        /// <summary>The current source line number.</summary>
        public int lineNum = 0;

        /// <summary>Causes new error messages to be put in a pending state.</summary>
        public bool paused { get; set; }

        /// <summary>To aid in debugging messages can be directly written to the console.</summary>
        public bool echoOnConsole = true; // for debugging

        public Log(int lineNum = 0, bool paused = false)
        {
            this.lineNum = lineNum;
            this.paused = paused;
        }

        public override string ToString()
        {
            return commitedMessages.ToString();
        }

        public void JoinLog(Log logToJoin)
        {
            hasErrors |= logToJoin.hasErrors;
            Append(logToJoin.pendingMessages.ToString());
        }

        public void Error(string text)
        {
            Append(String.Format("ERROR: [LINE:{0}] {1}\r\n", lineNum, text));
            hasErrors = true;
        }

        public void Error(string format, params object[] arg)
        {
            Append(String.Format("ERROR: [LINE:" + lineNum + "] " + format + "\r\n", arg));
            hasErrors = true;
        }

        public void Warning(string text)
        {
            Append(String.Format("WARN: [LINE:{0}] {1}\r\n", lineNum, text));
        }

        public void Warning(string format, params object[] arg)
        {
            Append(String.Format("WARN: [LINE:" + lineNum + "] " + format + "\r\n", arg));
        }

        public void Info(string text)
        {
            Append(String.Format("INFO: [LINE:{0}] {1}\r\n", lineNum, text));
        }

        public void Info(string format, params object[] arg)
        {
            Append(String.Format("INFO: [LINE:" + lineNum + "] " + format, arg));
        }

        public void WriteLine(string text)
        {
            Append(String.Format("{0}\r\n", text));
        }

        public void WriteLine(string format, params object[] arg)
        {
            Append(String.Format(format + "\r\n", arg));
        }

        public void Append(string text)
        {
            if (paused)
                pendingMessages.Append(text);
            else
            {
                if (echoOnConsole && !String.IsNullOrEmpty(text))
                    Console.Write(text);
                commitedMessages.Append(text);
            }
        }

        /// <summary>Writes all pending errors and un-pauses./// </summary>
        public void CommitMessagesAndUnPause()
        {
            if (echoOnConsole)
                Console.Write(pendingMessages);

            commitedMessages.Append(pendingMessages);

            pendingMessages.Clear();
            paused = false;
        }

        /// <summary>Writes all pending errors and un-pauses. </summary>
        public void DeletePendingMessagesAndUnPause()
        {
            pendingMessages.Clear();
            paused = false;
        }
    }
}
