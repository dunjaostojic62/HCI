using Common;
using System;
using System.Threading;
using System.Collections.Generic;

namespace ProcessingModule
{
    /// <summary>
    /// Class containing logic for automated work.
    /// </summary>
    public class AutomationManager : IAutomationManager, IDisposable
	{
		private Thread automationWorker;
        private AutoResetEvent automationTrigger;
        private IStorage storage;
		private IProcessingManager processingManager;
		private int delayBetweenCommands;
        private IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationManager"/> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        /// <param name="processingManager">The processing manager.</param>
        /// <param name="automationTrigger">The automation trigger.</param>
        /// <param name="configuration">The configuration.</param>
        public AutomationManager(IStorage storage, IProcessingManager processingManager, AutoResetEvent automationTrigger, IConfiguration configuration)
		{
			this.storage = storage;
			this.processingManager = processingManager;
            this.configuration = configuration;
            this.automationTrigger = automationTrigger;
        }

        /// <summary>
        /// Initializes and starts the threads.
        /// </summary>
		private void InitializeAndStartThreads()
		{
			InitializeAutomationWorkerThread();
			StartAutomationWorkerThread();
		}

        /// <summary>
        /// Initializes the automation worker thread.
        /// </summary>
		private void InitializeAutomationWorkerThread()
		{
			automationWorker = new Thread(AutomationWorker_DoWork);
			automationWorker.Name = "Aumation Thread";
		}

        /// <summary>
        /// Starts the automation worker thread.
        /// </summary>
		private void StartAutomationWorkerThread()
		{
			automationWorker.Start();
		}


        private void AutomationWorker_DoWork()
        {
            int step = 10;

            while (!disposedValue)
            {
                List<PointIdentifier> ids = new List<PointIdentifier>()
                 {
                    new PointIdentifier(PointType.ANALOG_OUTPUT, 1000),   // L - pozicija
                    new PointIdentifier(PointType.DIGITAL_OUTPUT, 3000),  // Open
                    new PointIdentifier(PointType.DIGITAL_OUTPUT, 3001),  // Close
                    new PointIdentifier(PointType.DIGITAL_INPUT, 2000)    // S - prepreka
                    };
                List<IPoint> points = storage.GetPoints(ids);

                IAnalogPoint gate = points[0] as IAnalogPoint;
                IDigitalPoint open = points[1] as IDigitalPoint;
                IDigitalPoint close = points[2] as IDigitalPoint;
                IDigitalPoint obstacle = points[3] as IDigitalPoint;

                double position = gate.EguValue;
                double lowLimit = gate.ConfigItem.LowLimit;
                double highLimit = gate.ConfigItem.HighLimit;

                // 1) HighAlarm -> ugasi Close
                if (position >= highLimit && close.State == DState.ON)
                {
                    processingManager.ExecuteWriteCommand(close.ConfigItem,
                        configuration.GetTransactionId(), configuration.UnitAddress, 3001, 0);
                }

                // 2) LowAlarm -> ugasi Open
                if (position <= lowLimit && open.State == DState.ON)
                {
                    processingManager.ExecuteWriteCommand(open.ConfigItem,
                        configuration.GetTransactionId(), configuration.UnitAddress, 3000, 0);
                }

                // 3) Open ON -> otvaranje (L opada)
                if (open.State == DState.ON && position > lowLimit)
                {
                    double newPosition = position - step;
                    if (newPosition < lowLimit) newPosition = lowLimit;
                    processingManager.ExecuteWriteCommand(gate.ConfigItem,
                        configuration.GetTransactionId(), configuration.UnitAddress, 1000, (int)newPosition);
                }

                // 4) Close ON -> zatvaranje (L raste), uz proveru prepreke
                if (close.State == DState.ON && position < highLimit)
                {
                    if (obstacle.State == DState.ON)
                    {
                        // prepreka: vrati nazad (otvori) do LowAlarm
                        if (position > lowLimit)
                        {
                            double newPosition = position - step;
                            if (newPosition < lowLimit) newPosition = lowLimit;
                            processingManager.ExecuteWriteCommand(gate.ConfigItem,
                                configuration.GetTransactionId(), configuration.UnitAddress, 1000, (int)newPosition);
                        }
                    }
                    else
                    {
                        double newPosition = position + step;
                        if (newPosition > highLimit) newPosition = highLimit;
                        processingManager.ExecuteWriteCommand(gate.ConfigItem,
                            configuration.GetTransactionId(), configuration.UnitAddress, 1000, (int)newPosition);
                    }
                }

                Thread.Sleep(delayBetweenCommands);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls


        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <param name="disposing">Indication if managed objects should be disposed.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
				}
				disposedValue = true;
			}
		}


		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// GC.SuppressFinalize(this);
		}

        /// <inheritdoc />
        public void Start(int delayBetweenCommands)
		{
			this.delayBetweenCommands = delayBetweenCommands*1000;
            InitializeAndStartThreads();
		}

        /// <inheritdoc />
        public void Stop()
		{
			Dispose();
		}
		#endregion
	}
}
