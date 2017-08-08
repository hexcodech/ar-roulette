using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using libzkfpcsharp;

namespace fingerprintServer
{
  class Program
  {
    zkfp fpInstance = new zkfp();
    IntPtr FormHandle = IntPtr.Zero;
    bool bIsTimeToDie = false;
    bool IsRegister = false;
    byte[] FPBuffer;
    int RegisterCount = 0;
    const int REGISTER_FINGER_COUNT = 3;

    byte[][] RegTmps = new byte[3][];
    byte[] RegTmp = new byte[2048];
    byte[] CapTmp = new byte[2048];
    int cbCapTmp = 2048;
    int cbRegTmp = 0;
    int iFid = 1;

    const int MESSAGE_CAPTURED_OK = 0x0400 + 6;

    static void Main(string[] args)
    {
      Program program = new Program();

      program.InitializeDevice();
      program.OpenDevice();

      Console.ReadLine();
    }

    private void InitializeDevice()
    {
      if (fpInstance.Initialize() == zkfp.ZKFP_ERR_OK)
      {
        Console.WriteLine("Initialized");
        if (fpInstance.GetDeviceCount() != 1)
        {
          fpInstance.Finalize();
          Console.WriteLine("No device connected");
          return;
        }
      }
      else
      {
        Console.WriteLine("Failed to initialize... Is the device connected?");
        Console.ReadLine();
        return;
      }

    }

    private void OpenDevice()
    {
      if (zkfp.ZKFP_ERR_OK != fpInstance.OpenDevice(0))
      {
        Console.WriteLine("Failed to open device");
        return;
      }

      cbRegTmp = 0;
      iFid = 1;
      for (int i = 0; i < 3; i++)
      {
        RegTmps[i] = new byte[2048];
      }
      FPBuffer = new byte[fpInstance.imageWidth * fpInstance.imageHeight];
      Thread captureThread = new Thread(DoCapture);
      captureThread.IsBackground = true;
      captureThread.Start();
      bIsTimeToDie = false;
    }

    private void DoCapture()
    {
      while (!bIsTimeToDie)
      {
        cbCapTmp = 2048;
        int ret = fpInstance.AcquireFingerprint(FPBuffer, CapTmp, ref cbCapTmp);
        if (ret == zkfp.ZKFP_ERR_OK)
        {
          Console.WriteLine("Finger captured: OK");
        }
        Thread.Sleep(200);
      }
    }
  }
}
