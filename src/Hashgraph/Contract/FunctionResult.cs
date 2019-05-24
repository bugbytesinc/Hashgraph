using Hashgraph.Implementation;
using System;

namespace Hashgraph
{
    /// <summary>
    /// Provides services to decode smart contract ABI data into .net primitives.
    /// Typically represents data returned from a smart contract invocation.
    /// </summary>
    public class FunctionResult
    {
        /// <summary>
        /// The raw bytes returned from a function call (in ABI format).
        /// </summary>
        private readonly ReadOnlyMemory<byte> _data;
        internal FunctionResult(ReadOnlyMemory<byte> data)
        {
            _data = data;
        }
        /// <summary>
        /// Retrieves the first value returned from the contract cast to the 
        /// desired native type.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the first argument, must be known to the caller.
        /// </typeparam>
        /// <returns>
        /// The value of the first argument decoded from the ABI results.
        /// </returns>
        public T As<T>()
        {
            return (T)Abi.DecodeArguments(_data, typeof(T))[0];
        }
        /// <summary>
        /// Retrieves the first and second values from the contract function result cast to the desired types.
        /// </summary>
        /// <typeparam name="T1">
        /// Type of the first argument, must be known to the caller.
        /// </typeparam>
        /// <typeparam name="T2">
        /// Type of the second argument, must be known to the caller.
        /// </typeparam>
        /// <returns>
        /// A tuple of the first two arguments decoded from the contract function ABI results.
        /// </returns>
        public (T1, T2) As<T1, T2>()
        {
            var args = Abi.DecodeArguments(_data, typeof(T1), typeof(T2));
            return ((T1)args[0], (T2)args[1]);
        }
        /// <summary>
        /// Retrieves the three values from the contract function result cast to the desired types.
        /// </summary>
        /// <typeparam name="T1">
        /// Type of the first argument, must be known to the caller.
        /// </typeparam>
        /// <typeparam name="T2">
        /// Type of the second argument, must be known to the caller.
        /// </typeparam>
        /// <typeparam name="T3">
        /// Type of the third argument, must be known to the caller.
        /// </typeparam>
        /// <returns>
        /// A tuple of the first three arguments decoded from the contract function ABI results.
        /// </returns>
        public (T1, T2, T3) As<T1, T2, T3>()
        {
            var args = Abi.DecodeArguments(_data, typeof(T1), typeof(T2), typeof(T3));
            return ((T1)args[0], (T2)args[1], (T3)args[2]);
        }
        /// <summary>
        /// Retrieves the four values from the contract function result cast to the desired types.
        /// </summary>
        /// <typeparam name="T1">
        /// Type of the first argument, must be known to the caller.
        /// </typeparam>
        /// <typeparam name="T2">
        /// Type of the second argument, must be known to the caller.
        /// </typeparam>
        /// <typeparam name="T3">
        /// Type of the third argument, must be known to the caller.
        /// </typeparam>
        /// <typeparam name="T4">
        /// Type of the fourth argument, must be known to the caller.
        /// </typeparam>
        /// <returns>
        /// A tuple of the first four arguments decoded from the contract function ABI results.
        /// </returns>
        public (T1, T2, T3, T4) As<T1, T2, T3, T4>()
        {
            var args = Abi.DecodeArguments(_data, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
            return ((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3]);
        }
        /// <summary>
        /// Retrieves an arbitrary number of values decoded from the contract return data.
        /// </summary>
        /// <param name="types">
        /// An array of native types that should be returned.  Must be known by the caller.
        /// </param>
        /// <returns>
        /// An array of objects (which may be boxed) of the decoded parameters of the types desired.
        /// </returns>
        public object[] GetAll(params Type[] types)
        {
            return Abi.DecodeArguments(_data, types);
        }
    }
}
