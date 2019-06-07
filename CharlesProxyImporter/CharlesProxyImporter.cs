using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using Fiddler;
using Newtonsoft.Json;
using CharlesProxy;

[ProfferFormat("Charles Proxy", "Session List in Charles JSON Format. This is the general format for mobile devices using Charles on mobile devices.", ".chlsj")]
public class CharlesProxyImporter : ISessionImporter
{
    public void Dispose() { /*no-op*/ }

    public Session[] ImportSessions(string sImportFormat, Dictionary<string, object> dictOptions, EventHandler<ProgressCallbackEventArgs> evtProgressNotifications)
    {
        string sFilename = string.Empty;

        // Check for command-line, FiddlerScript import, etc
        if (null != dictOptions)
        {
            if (dictOptions.ContainsKey("Filename"))
            {
                sFilename = dictOptions["Filename"] as string;
            }
        }

        if (string.IsNullOrEmpty(sFilename))
        {
            // Example: "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            sFilename = Fiddler.Utilities.ObtainOpenFilename("Open Charles JSON Session File", "Charles JSON Session File (*.chlsj)|*.chlsj");
        }

        if (string.IsNullOrEmpty(sFilename))
        {
            Log("[CharlesProxyTranscoder] User cancelled import.");
            return null;
        }

        try
        {
            var jsonContent = System.IO.File.ReadAllText(sFilename);
            CharlesSession[] charlesSessions = CharlesProxy.CharlesSession.FromJson(jsonContent);

            var sessions = new List<Session>();

            for (int i = 0; i < charlesSessions.Length; i++)
            {
                var session = charlesSessions[i];

                if (session.Tunnel || session.Status != SessionStatus.COMPLETE)
                {
                    // TODO: Likely SSL, display this properly
                    // I'll need to rebuild the SSL display in Fiddler based on other metadata
                    // Charles just gives the encrypted bytes in the response body
                    continue;
                }

                var reqBytes = ExtractData(session.Request);
                var respBytes = ExtractData(session.Response);                

                var newSession = new Session(reqBytes, respBytes);

                if (newSession.ResponseHeaders.ExistsAndContains("Transfer-Encoding", "chunked"))
                {
                    newSession.ResponseHeaders.Remove("Transfer-Encoding");
                    newSession.ResponseHeaders.Add("Transfer-Encoding-Removed", "CharlesProxyImporter removed this header as .chlsj are already de-chunked.");
                }

                sessions.Add(newSession);

                // Notify the caller of our progress
                if (null != evtProgressNotifications)
                {
                    ProgressCallbackEventArgs PCEA =
                    new ProgressCallbackEventArgs(((i + 1) / (float)charlesSessions.Length),
                    "wrote " + (i + 1).ToString() + " records.");
                    evtProgressNotifications(null, PCEA);

                    // If the caller tells us to cancel, abort quickly
                    if (PCEA.Cancel) { Log("[CharlesProxyTranscoder] Import aborted."); return null; }
                }
            }

            return sessions.ToArray();
        }
        catch (Exception ex)
        {
            FiddlerApplication.Log.LogString($"[CharlesProxyTranscoder] Failed to import Charles JSON Session File: {ex.Message}");
            return null;
        }
    }

    private byte[] ExtractData(HttpSession session)
    {
        byte[] headerBytes = new byte[0];
        byte[] bodyBytes = new byte[0];

        // Header
        if (session.SizeInfo.headers > 0)
        {
            if (null == session.Header)
            {
                headerBytes = Encoding.UTF8.GetBytes($"Session request is corrupt.  Headers are null but expected {session.SizeInfo.headers} bytes.");
            }
            else
            {
                headerBytes = Encoding.UTF8.GetBytes(session.Header.ToString() + Environment.NewLine + Environment.NewLine);
            }
        }

        // Body
        if (session.SizeInfo.body > 0)
        {
            if (session.Body != null)
            {
                if (session.Body.Text != null)
                {
                    bodyBytes = Encoding.UTF8.GetBytes(session.Body.Text);
                }
                else if (session.Body.Encoded != null && session.Body.Encoding != null && session.Body.Encoding == "base64")
                {
                    bodyBytes = Convert.FromBase64String(session.Body.Encoded);
                }
            }
            else
            {
                bodyBytes = Encoding.UTF8.GetBytes($"Session request is corrupt.  Headers are null but expected {session.SizeInfo.body} bytes.");
            }
        }

        byte[] sessionBytes = new byte[headerBytes.Length + bodyBytes.Length];
        headerBytes.CopyTo(sessionBytes, 0);
        bodyBytes.CopyTo(sessionBytes, headerBytes.Length);

        return sessionBytes;
    }

    private void Log(string text)
    {
        FiddlerApplication.Log.LogString(text);
    }
}