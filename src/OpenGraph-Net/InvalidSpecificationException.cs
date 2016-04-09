// <copyright file="InvalidSpecificationException.cs" company="SHHH Innovations LLC">
// Copyright SHHH Innovations LLC
// </copyright>

namespace OpenGraphNet
{
    using System;

    /// <summary>
    /// An invalid specification exception
    /// </summary>
    public class InvalidSpecificationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSpecificationException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public InvalidSpecificationException(string message)
            : base(message)
        {
        }
    }
}
