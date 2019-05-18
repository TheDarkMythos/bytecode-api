﻿using System;
using System.Diagnostics;

namespace BytecodeApi.IO.Interop
{
	/// <summary>
	/// Represents a programmatic interface to a cmd.exe <see cref="System.Diagnostics.Process" /> to read from and write to.
	/// </summary>
	public class CommandPrompt : IDisposable
	{
		private Process Process;
		/// <summary>
		/// Gets the ProcessID of the underlying cmd.exe <see cref="System.Diagnostics.Process" />, or 0 if it is not running.
		/// </summary>
		public int ProcessId => Process?.Id ?? 0;
		/// <summary>
		/// Gets a value indicating whether the underlying cmd.exe <see cref="System.Diagnostics.Process" /> is running.
		/// </summary>
		public bool Running => Process?.HasExited == false;
		/// <summary>
		/// Occurs when a message is received from the standard error stream or the standard output stream.
		/// </summary>
		public event EventHandler<CommandPromptEventArgs> MessageReceived;

		/// <summary>
		/// Releases all resources used by the current instance of the <see cref="CommandPrompt" /> class and kills the underlying cmd.exe <see cref="System.Diagnostics.Process" />.
		/// </summary>
		public void Dispose()
		{
			if (Process != null)
			{
				Process.OutputDataReceived -= Process_OutputDataReceived;
				Process.ErrorDataReceived -= Process_ErrorDataReceived;
				Process.Kill();
				Process.Dispose();
				Process = null;
			}
		}

		/// <summary>
		/// Starts a new cmd.exe <see cref="System.Diagnostics.Process" /> to read from and write to.
		/// </summary>
		public void Start()
		{
			Check.InvalidOperation(!Running, "The process is already running.");

			Process = new Process
			{
				StartInfo =
				{
					FileName = "cmd.exe",
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				},
				EnableRaisingEvents = true
			};

			Process.OutputDataReceived += Process_OutputDataReceived;
			Process.ErrorDataReceived += Process_ErrorDataReceived;

			Process.Start();
			Process.BeginOutputReadLine();
			Process.BeginErrorReadLine();
		}
		/// <summary>
		/// Writes a <see cref="string" /> to the standard input stream.
		/// </summary>
		/// <param name="value">The <see cref="string" /> to write to the stream. If <paramref name="value" /> is <see langword="null" />, nothing is written.</param>
		public void Write(string value)
		{
			Check.InvalidOperation(Running, "The process has exited.");

			Process.StandardInput.Write(value);
		}
		/// <summary>
		/// Writes a <see cref="string" /> followed by a line terminator to the standard input stream.
		/// </summary>
		/// <param name="value">The <see cref="string" /> to write to the stream. If <paramref name="value" /> is <see langword="null" />, only the line terminator is written.</param>
		public void WriteLine(string value)
		{
			Check.InvalidOperation(Running, "The process has exited.");

			Process.StandardInput.WriteLine(value);
		}

		private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			OnMessageReceived(new CommandPromptEventArgs(false, e.Data));
		}
		private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			OnMessageReceived(new CommandPromptEventArgs(true, e.Data));
		}

		/// <summary>
		/// Raises the <see cref="MessageReceived" /> event.
		/// </summary>
		/// <param name="e">The event data for the <see cref="MessageReceived" /> event.</param>
		protected void OnMessageReceived(CommandPromptEventArgs e)
		{
			MessageReceived?.Invoke(this, e);
		}
	}
}