using System;
using System.Threading;

namespace vMixStreamDeckLibrary
{
	// Token: 0x02000012 RID: 18
	internal class ThreadTask : IDisposable
	{
		// Token: 0x14000002 RID: 2
		// (add) Token: 0x0600003A RID: 58 RVA: 0x00002166 File Offset: 0x00000366
		// (remove) Token: 0x0600003B RID: 59 RVA: 0x0000217F File Offset: 0x0000037F
		public event ThreadTask.ThreadErrorEventHandler ThreadError;

		// Token: 0x0600003C RID: 60 RVA: 0x00002198 File Offset: 0x00000398
		public ThreadTask(ThreadStart t)
		{
			this.m_Delegate = t;
			this.m_Thread = new Thread(new ThreadStart(this._Thread));
			this.m_Thread.IsBackground = true;
			this.m_Thread.Start();
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600003D RID: 61 RVA: 0x00003160 File Offset: 0x00001360
		public bool ThreadExit
		{
			get
			{
				return this.m_ThreadExit;
			}
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00003174 File Offset: 0x00001374
		private void _Thread()
		{
			try
			{
				while (!this.m_ThreadExit)
				{
					this.m_Delegate();
				}
			}
			catch (Exception t)
			{
				//ThreadTask.ThreadErrorEventHandler threadErrorEvent = this.ThreadErrorEvent;
				if (this.ThreadError != null)
				{
                    this.ThreadError(this, new ThreadExceptionEventArgs(t));
				}
			}
		}

		// Token: 0x0600003F RID: 63 RVA: 0x000031E8 File Offset: 0x000013E8
		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposedValue && disposing && this.m_Thread != null)
			{
				this.m_ThreadExit = true;
				this.m_Thread.Join(5000);
				this.m_Thread = null;
			}
			this.disposedValue = true;
		}

		// Token: 0x06000040 RID: 64 RVA: 0x000021D6 File Offset: 0x000003D6
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		// Token: 0x0400002B RID: 43
		private Thread m_Thread;

		// Token: 0x0400002C RID: 44
		private volatile bool m_ThreadExit;

		// Token: 0x0400002D RID: 45
		private ThreadStart m_Delegate;

		// Token: 0x0400002F RID: 47
		public const int CLOSE_THREAD_TIMEOUT = 5000;

		// Token: 0x04000030 RID: 48
		private bool disposedValue;

		// Token: 0x02000013 RID: 19
		// (Invoke) Token: 0x06000044 RID: 68
		public delegate void ThreadErrorEventHandler(object sender, ThreadExceptionEventArgs e);
	}
}
