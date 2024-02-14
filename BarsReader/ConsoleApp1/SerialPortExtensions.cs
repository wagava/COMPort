using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.IO.Ports;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

namespace ConsoleApp1
{

    internal static class SerialPortExtensions
    {
        /// <summary>
        /// Class <c>SerialPortExtensions</c> as additional class expanding port capability.
        /// </summary>

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void SetXonXoffChars(this SerialPort port, byte xon, byte xoff, byte err, byte eof, byte evt)
        {
            if (port == null)
                throw new NullReferenceException();
            if (port.BaseStream == null)
                throw new InvalidOperationException("Cannot change X chars until after the port has been opened.");

            try
            {
                // Get the base stream and its type which is System.IO.Ports.SerialStream
                object baseStream = port.BaseStream;
                Type baseStreamType = baseStream.GetType();

                // Get the Win32 file handle for the port
                SafeFileHandle portFileHandle = (SafeFileHandle)baseStreamType.GetField("_handle", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(baseStream);

                // Get the value of the private DCB field (a value type)
                FieldInfo dcbFieldInfo = baseStreamType.GetField("dcb", BindingFlags.NonPublic | BindingFlags.Instance);
                object dcbValue = dcbFieldInfo.GetValue(baseStream);
                // The type of dcb is Microsoft.Win32.UnsafeNativeMethods.DCB which is an internal type. We can only access it through reflection.
                Type dcbType = dcbValue.GetType();

                dcbType.GetField("XonChar").SetValue(dcbValue, xon);
                dcbType.GetField("XoffChar").SetValue(dcbValue, xoff);
                dcbType.GetField("ErrorChar").SetValue(dcbValue, err);
                dcbType.GetField("EofChar").SetValue(dcbValue, eof);
                dcbType.GetField("EvtChar").SetValue(dcbValue, evt);
                UInt16 valZ = 0;
                dcbType.GetField("XonLim").SetValue(dcbValue, valZ);

                dcbType.GetField("XoffLim").SetValue(dcbValue, valZ);
                ////////////////////////
                /*
                FieldInfo CommTFieldInfo = baseStreamType.GetField("commTimeouts", BindingFlags.NonPublic | BindingFlags.Instance);
                object CommTValue = CommTFieldInfo.GetValue(baseStream);
                Type CommTType = CommTValue.GetType();
                
                Int32 valInterval = 100;
                CommTType.GetField("ReadIntervalTimeout").SetValue(CommTValue, valInterval);
                CommTType.GetField("ReadTotalTimeoutMultiplier").SetValue(CommTValue, 0);
                CommTType.GetField("ReadTotalTimeoutConstant").SetValue(CommTValue, 2000);
                CommTType.GetField("WriteTotalTimeoutMultiplier").SetValue(CommTValue, 0);
                CommTType.GetField("WriteTotalTimeoutConstant").SetValue(CommTValue, 0);
                *///////////////////////
                // We need to call SetCommState but because dcbValue is a private type, we don't have enough
                //  information to create a p/Invoke declaration for it. We have to do the marshalling manually.

                // Create unmanaged memory to copy DCB into
                IntPtr hGlobal = Marshal.AllocHGlobal(Marshal.SizeOf(dcbValue));
                try
                {
                    // Copy their DCB value to unmanaged memory
                    Marshal.StructureToPtr(dcbValue, hGlobal, false);

                    // Call SetCommState
                    if (!SetCommState(portFileHandle, hGlobal))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    // Update the BaseStream.dcb field if SetCommState succeeded
                    dcbFieldInfo.SetValue(baseStream, dcbValue);
                }
                finally
                {
                    if (hGlobal != IntPtr.Zero)
                        Marshal.FreeHGlobal(hGlobal);
                }

                /*IntPtr h1Global = Marshal.AllocHGlobal(Marshal.SizeOf(CommTValue));
                try
                {
                    // Copy their DCB value to unmanaged memory
                    Marshal.StructureToPtr(CommTValue, h1Global, false);

                    // Call SetCommState
                   // if (!SetCommState(portFileHandle, h1Global))
                     //   throw new Win32Exception(Marshal.GetLastWin32Error());

                    // Update the BaseStream.dcb field if SetCommState succeeded
                    CommTFieldInfo.SetValue(baseStream, CommTValue);
                }
                catch (Exception ex)
                { }
                finally
                {
                    if (h1Global != IntPtr.Zero)
                        Marshal.FreeHGlobal(h1Global);
                }*/

            }
            catch (SecurityException) { throw; }
            catch (OutOfMemoryException) { throw; }
            catch (Win32Exception) { throw; }
            catch (Exception ex)
            {
                throw new ApplicationException("SetXonXoffChars has failed due to incorrect assumptions about System.IO.Ports.SerialStream which is an internal type.", ex);
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetCommState(SafeFileHandle hFile, IntPtr lpDCB);
    }
}
