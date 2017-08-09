﻿using System;
using System.Drawing;
using System.IO;
using System.Threading;
using libzkfpcsharp;

namespace fingerprintServer
{
  class Program
  {
    zkfp fpInstance = new zkfp();
    IntPtr FormHandle = IntPtr.Zero;
    bool bIsTimeToDie = false;
    bool IsRegister = false;
    bool bIdentify = true;
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
      program.StartMenuLoop();
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
          Handler("MESSAGE_CAPTURED_OK");
        }
        Thread.Sleep(200);
      }
    }

    private void Handler(string message)
    {
      switch (message)
      {
        case "MESSAGE_CAPTURED_OK":
          {
            MemoryStream ms = new MemoryStream();
            BitmapFormat.GetBitmap(FPBuffer, fpInstance.imageWidth, fpInstance.imageHeight, ref ms);
            Bitmap bmp = new Bitmap(ms);
            // this.picFPImg.Image = bmp;

            // We scanned a finger and we're trying to register a new finger.
            if (IsRegister)
            {
              int ret;
              int fid = 0;
              int score = 0;

              ret = fpInstance.Identify(CapTmp, ref fid, ref score);
              if (ret == zkfp.ZKFP_ERR_OK)
              {
                Console.WriteLine("This finger is already registered by " + fid);
                return;
              }
              if (RegisterCount > 0 && fpInstance.Match(CapTmp, RegTmps[RegisterCount - 1]) <= 0)
              {
                Console.WriteLine("Press finger 3 times to register");
                return;
              }

              Array.Copy(CapTmp, RegTmps[RegisterCount], cbCapTmp);
              RegisterCount++;

              // If the new finger has been scanned enough times
              if (RegisterCount >= REGISTER_FINGER_COUNT)
              {
                RegisterCount = 0;
                // If we successfully generate a template from the finger and save it
                if (fpInstance.GenerateRegTemplate(RegTmps[0], RegTmps[1], RegTmps[2], RegTmp, ref cbRegTmp) ==
                    zkfp.ZKFP_ERR_OK && fpInstance.AddRegTemplate(iFid, RegTmp) == zkfp.ZKFP_ERR_OK)
                {
                  iFid++;
                  Console.WriteLine("Saved finger successfully");
                }
                else
                {
                  Console.WriteLine("Failed to register finger code=" + ret);
                }

                // End register
                IsRegister = false;
                return;
              }
              else
              {
                Console.WriteLine("Press the finger " + (REGISTER_FINGER_COUNT - RegisterCount) + " times to register");
              }
            }
            // Check finger
            else
            {
              if (cbRegTmp <= 0)
              {
                Console.WriteLine("No fingers registered yet, register first");
                return;
              }
              if (bIdentify)
              {
                int fid = 0;
                int score = 0;
                int ret = fpInstance.Identify(CapTmp, ref fid, ref score);

                if (ret == zkfp.ZKFP_ERR_OK)
                {
                  Console.WriteLine("Successfully identified finger fid=" + fid + ", score=" + score);
                  return;
                }
                else
                {
                  Console.WriteLine("Identifying failed");
                  return;
                }
              }
              else
              {
                int ret = fpInstance.Match(CapTmp, RegTmp);
                if (ret > 0)
                {
                  Console.WriteLine("Matching finger successfully score=" + ret);
                  return;
                }
                else
                {
                  Console.WriteLine("Matching finger failed");
                  return;
                }
              }
            }
          }
          break;

        default:
          Handler(message);
          break;
      }
    }

    private void StartMenuLoop()
    {
      Console.WriteLine("------------------------------------");
      Console.WriteLine("Press ESC to stop");
      Console.WriteLine("Press r key to register a new finger");
      Console.WriteLine("Press i key to identify your finger");
      Console.WriteLine("Press v key to verify your finger");
      Console.WriteLine("------------------------------------");
      do
      {
        switch (Console.ReadKey(true).Key)
        {
          case ConsoleKey.R:
          {
            if (!IsRegister)
            {
              IsRegister = true;
              RegisterCount = 0;
              cbRegTmp = 0;
              Console.WriteLine("Registering your finger... Press finger 3 times");
            }
          }
            break;
          case ConsoleKey.I:
          {
            if (!bIdentify)
            {
              bIdentify = true;
              Console.WriteLine("Identifying your finger... Press finger once");
            }
          }
            break;
          case ConsoleKey.V:
          {
            if (bIdentify)
            {
              bIdentify = false;
              Console.WriteLine("Verifying your finger... Press finger once");
            }
          }
            break;
        }
      } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
    }
  }
}
