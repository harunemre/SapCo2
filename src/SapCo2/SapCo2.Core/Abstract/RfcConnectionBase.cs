using System;
using SapCo2.Core.Extensions;
using SapCo2.Wrapper.Abstract;
using SapCo2.Wrapper.Enumeration;
using SapCo2.Wrapper.Extension;
using SapCo2.Wrapper.Interop;

namespace SapCo2.Core.Abstract
{
    public abstract class RfcConnectionBase:IRfcConnection
    {
        private readonly IRfcInterop _interop;
        private readonly RfcConnectionOption _options;
        private IntPtr _rfcConnectionHandle = IntPtr.Zero;

        protected RfcConnectionBase(IRfcInterop interop, RfcConnectionOption options)
        {
            _interop = interop;
            _options = options;
        }

        #region Interface Implementation

        public virtual bool IsValid
        {
            get
            {
                if (_rfcConnectionHandle == IntPtr.Zero)
                    return false;

                RfcResultCodes resultCode = _interop.IsConnectionHandleValid(_rfcConnectionHandle, out int isValid, out _);
                return resultCode == RfcResultCodes.RFC_OK && isValid > 0;
            }
        }
        public virtual bool Ping()
        {
            if (_rfcConnectionHandle == IntPtr.Zero)
                return false;

            RfcResultCodes resultCode = _interop.Ping(rfcHandle: _rfcConnectionHandle, errorInfo: out _);
            return resultCode == RfcResultCodes.RFC_OK;
        }
        public virtual void Connect()
        {
            RfcConnectionParameter[] interopParameters = _options.ToInterop();

            _rfcConnectionHandle = _interop.OpenConnection(connectionParams: interopParameters, paramCount: (uint)interopParameters.Length,
                errorInfo: out RfcErrorInfo errorInfo);

            errorInfo.ThrowOnError(beforeThrow: Clear);
        }
        public virtual void Disconnect()
        {
            Disconnect(disposing: false);
        }
        public virtual void Dispose()
        {
            Disconnect(disposing: true);
        }

        public virtual IntPtr GetConnectionHandle()
        {
            return _rfcConnectionHandle;
        }
        #endregion

        #region Private Methods

        private void Disconnect(bool disposing)
        {
            if (_rfcConnectionHandle == IntPtr.Zero)
                return;

            RfcResultCodes resultCode = _interop.CloseConnection(
                rfcHandle: _rfcConnectionHandle,
                errorInfo: out RfcErrorInfo errorInfo);

            Clear();

            if (!disposing)
                resultCode.ThrowOnError(errorInfo);
        }
        private void Clear()
        {
            _rfcConnectionHandle = IntPtr.Zero;
        }

        #endregion
    }
}
