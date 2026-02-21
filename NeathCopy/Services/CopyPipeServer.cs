using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeathCopy.Services
{
    [DataContract]
    public class CopyPipeRequest
    {
        [DataMember]
        public string Operation { get; set; }

        [DataMember]
        public List<string> Sources { get; set; }

        [DataMember]
        public string Destination { get; set; }
    }

    public class CopyPipeServer : IDisposable
    {
        public const string PipeName = "NeathCopyPipe";

        private CancellationTokenSource cts;
        private Task listenTask;
        private Action<CopyPipeRequest> onRequest;

        public bool IsRunning => listenTask != null && !listenTask.IsCompleted;

        public void Start(Action<CopyPipeRequest> handler)
        {
            if (IsRunning)
                return;

            onRequest = handler;
            cts = new CancellationTokenSource();
            listenTask = Task.Run(() => ListenLoop(cts.Token), cts.Token);
        }

        public void Stop()
        {
            if (cts == null)
                return;

            cts.Cancel();
            try
            {
                listenTask?.Wait(500);
            }
            catch (Exception)
            {
            }
            finally
            {
                cts.Dispose();
                cts = null;
                listenTask = null;
            }
        }

        private async Task ListenLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                using (var server = new NamedPipeServerStream(PipeName, PipeDirection.In, 1,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                {
                    try
                    {
                        await server.WaitForConnectionAsync(token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    CopyPipeRequest request = null;
                    try
                    {
                        using (var reader = new StreamReader(server, Encoding.UTF8, true, 4096, true))
                        {
                            var json = await reader.ReadToEndAsync().ConfigureAwait(false);
                            request = Deserialize(json);
                        }
                    }
                    catch (Exception)
                    {
                        request = null;
                    }

                    try
                    {
                        if (request != null)
                            onRequest?.Invoke(request);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private static CopyPipeRequest Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            var serializer = new DataContractJsonSerializer(typeof(CopyPipeRequest));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return serializer.ReadObject(ms) as CopyPipeRequest;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
