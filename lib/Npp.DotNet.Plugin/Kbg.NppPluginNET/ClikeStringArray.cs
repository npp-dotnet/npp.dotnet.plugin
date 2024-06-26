﻿/*
 * SPDX-FileCopyrightText: 2016 Kasper B. Graversen <https://github.com/kbilsted>
 *
 * SPDX-License-Identifier: Apache-2.0
 */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Npp.DotNet.Plugin
{
    public class ClikeStringArray : IDisposable
    {
        internal IntPtr _nativeArray;
        internal List<IntPtr> _nativeItems;
        bool _disposed = false;

        public ClikeStringArray(int num, int stringCapacity)
        {
            _nativeArray = Marshal.AllocHGlobal((num + 1) * IntPtr.Size);
            _nativeItems = new List<IntPtr>();
            for (int i = 0; i < num; i++)
            {
                IntPtr item = Marshal.AllocHGlobal(stringCapacity);
                Marshal.WriteIntPtr(_nativeArray + (i * IntPtr.Size), item);
                _nativeItems.Add(item);
            }
            Marshal.WriteIntPtr(_nativeArray + (num * IntPtr.Size), IntPtr.Zero);
        }
        public ClikeStringArray(List<string> lstStrings)
        {
            _nativeArray = Marshal.AllocHGlobal((lstStrings.Count + 1) * IntPtr.Size);
            _nativeItems = new List<IntPtr>();
            for (int i = 0; i < lstStrings.Count; i++)
            {
                IntPtr item = Marshal.StringToHGlobalUni(lstStrings[i]);
                Marshal.WriteIntPtr(_nativeArray + (i * IntPtr.Size), item);
                _nativeItems.Add(item);
            }
            Marshal.WriteIntPtr(_nativeArray + (lstStrings.Count * IntPtr.Size), IntPtr.Zero);
        }

        public IntPtr NativePointer { get { return _nativeArray; } }
        public List<string> ManagedStringsAnsi { get { return _getManagedItems(false); } }
        public List<string> ManagedStringsUnicode { get { return _getManagedItems(true); } }
        List<string> _getManagedItems(bool unicode)
        {
            List<string> _managedItems = new List<string>();
            for (int i = 0; i < _nativeItems.Count; i++)
            {
                if (unicode) _managedItems.Add(Marshal.PtrToStringUni(_nativeItems[i]));
                else _managedItems.Add(Marshal.PtrToStringAnsi(_nativeItems[i]));
            }
            return _managedItems;
        }

        public void Dispose()
        {
            try
            {
                if (!_disposed)
                {
                    for (int i = 0; i < _nativeItems.Count; i++)
                        if (_nativeItems[i] != IntPtr.Zero) Marshal.FreeHGlobal(_nativeItems[i]);
                    if (_nativeArray != IntPtr.Zero) Marshal.FreeHGlobal(_nativeArray);
                    GC.SuppressFinalize(this);
                    _disposed = true;
                }
            }
            catch (Exception e)
            {
                _ = Win32.MsgBoxDialog(IntPtr.Zero, $"{nameof(Dispose)}:{e.Message}", GetType().Name, (uint)Win32.MsgBox.ICONERROR);
            }
        }
        ~ClikeStringArray()
        {
            Dispose();
        }
    }
}
