// // Copyright (c) Dennis Aikara. All rights reserved.
// // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Zel.Classes;

namespace Zel
{
    public static class CoreExtensions
    {
        /// <summary>
        ///     Converts a stream to byte array
        /// </summary>
        /// <param name="stream">Stream to convert</param>
        /// <returns>Stream content as byte array</returns>
        public static byte[] ToByteArray(this Stream stream)
        {
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        /// <summary>
        ///     Serialize object to json
        /// </summary>
        /// <param name="objectToSerialize"> Object to serialize </param>
        /// <returns> Json string </returns>
        public static string ToJson(this object objectToSerialize)
        {
            return JsonConvert.SerializeObject(objectToSerialize);
        }

        /// <summary>
        ///     Deserialize json string to the specified type
        /// </summary>
        /// <typeparam name="T"> Object type </typeparam>
        /// <param name="json"> Json string </param>
        /// <returns> Object representation of the json </returns>
        public static T FromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        ///     Deserialize json string to the specified type
        /// </summary>
        /// <param name="type"> Object type </param>
        /// <param name="json"> Json string </param>
        /// <returns> Object representation of the json </returns>
        public static object FromJson(this string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type);
        }

        /// <summary>
        ///     Serialize object to bson
        /// </summary>
        /// <param name="objectToSerialize"> Object to serialize </param>
        /// <returns> Json string </returns>
        public static MemoryStream ToBson(this object objectToSerialize)
        {
            var memoryStream = new MemoryStream();
            new JsonSerializer().Serialize(new BsonWriter(memoryStream), objectToSerialize);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        /// <summary>
        ///     Deserialize bson to object
        /// </summary>
        /// <param name="bsonStream"> Bson stream</param>
        /// <returns> Json string </returns>
        public static T FromBson<T>(this Stream bsonStream)
        {
            bsonStream.Seek(0, SeekOrigin.Begin);
            var bsonReader = new BsonReader(bsonStream);
            return new JsonSerializer().Deserialize<T>(bsonReader);
        }

        /// <summary>
        ///     Process items in the enumberable collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="parallelProcessOptions"></param>
        /// <param name="actions"></param>
        public static void ParallelProcess<T>(this IEnumerable<T> items, ParallelProcessOptions parallelProcessOptions,
            params Action<ParallelProcessToken>[] actions)
        {
            var parallelOptions = new ParallelOptions();
            if (parallelProcessOptions.MaxConcurrency > 0)
            {
                parallelOptions.MaxDegreeOfParallelism = parallelProcessOptions.MaxConcurrency;
            }

            var processStartTime = Environment.TickCount;
            Parallel.ForEach(items, parallelOptions, (item, parallelLoopState) =>
            {
                var processObject =
                    new ParallelProcessToken(
                        parallelProcessOptions.ItemTimeoutInMilliseconds)
                    {
                        Data = item
                    };

                foreach (var action in actions)
                {
                    var processTimedout =
                        (parallelProcessOptions.ProcessTimeoutInMilliseconds > 0)
                        && (Environment.TickCount - processStartTime
                            >= parallelProcessOptions
                                .ProcessTimeoutInMilliseconds);
                    if (processTimedout)
                    {
                        parallelLoopState.Stop();
                        return;
                    }

                    if (processObject.IsCancellationRequested())
                    {
                        return;
                    }
                    action(processObject);
                }
            });
        }

        public static byte[] GetMd5Hash(this byte[] bytes)
        {
            using (var md5 = MD5.Create())
            {
                return md5.ComputeHash(bytes);
            }
        }

        public static byte[] GetMd5Hash(this Stream stream)
        {
            stream.Position = 0;
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(stream);
                stream.Position = 0;
                return hash;
            }
        }
    }
}