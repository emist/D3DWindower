using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syringe
{
    public enum CustomUnmanagedType
    {
        /// <summary>
        /// Pointer to a null-terminated ANSI string
        /// </summary>
        LPStr,
        /// <summary>
        /// Pointer to a null-terminated Unicode string
        /// </summary>
        LPWStr
    }

    public class CustomMarshalAsAttribute : CustomMarshalAttribute
    {
        private CustomUnmanagedType _val;

        public CustomMarshalAsAttribute(CustomUnmanagedType unmanagedType)
        {
            _val = unmanagedType;
        }

        public CustomMarshalAsAttribute(short unmanagedType)
        {
            _val = (CustomUnmanagedType)unmanagedType;
        }

        public CustomUnmanagedType Value { get { return _val; } }
    }
}
