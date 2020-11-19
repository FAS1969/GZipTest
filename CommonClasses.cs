using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;

namespace GZipTest
{
    public static class Zip
    {
        public const int BUFFER_SIZE = 1000000;
        public const int ARRAY_SIZE = 100000000;

        public static string CompressToBase64(this string data)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(data).Compress());
        }

        public static string DecompressFromBase64(this string data)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(data).Decompress());
        }

        public static byte[] Compress(this byte[] data)
        {
            using (var sourceStream = new MemoryStream(data))
            using (var destinationStream = new MemoryStream())
            {
                sourceStream.CompressTo(destinationStream);
                return destinationStream.ToArray();
            }
        }

        public static byte[] Decompress(this byte[] data)
        {
            using (var sourceStream = new MemoryStream(data))
            using (var destinationStream = new MemoryStream())
            {
                sourceStream.DecompressTo(destinationStream);
                return destinationStream.ToArray();
            }
        }

        public static void CompressTo(this Stream stream, Stream outputStream)
        {
            using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                stream.CopyTo(gZipStream);
                gZipStream.Flush();
            }
        }

        public static void DecompressTo(this Stream stream, Stream outputStream)
        {
            using (var gZipStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                gZipStream.CopyTo(outputStream);
            }
        }

        public static string MatrixToString(long[,] matrix)
        {
            String myStr = "{";
            for (int i = 0; i <= matrix.GetUpperBound(0); i++)
            {
                myStr += "{";
                for (int j = 0; j < 2; j++)
                {
                    myStr += matrix[i, j];
                    myStr += j == 0 ? "," : "";
                }
                myStr += i == matrix.GetUpperBound(0) ? "}" : "},";

            }
            myStr += "}";
            return myStr;
        }

        public static long[,] StringToMatrix(string strMatrix)
        {
            string[] separ = { "},{" };
            var arrTemp = strMatrix.Trim().TrimStart('{').TrimEnd('}').Split(separ, System.StringSplitOptions.None);
            long[,] matrix = new long[arrTemp.Length, 2];
            long cnt = 0;
            foreach (var itm in arrTemp)
            {
                var arr = itm.Split(',');
                matrix[cnt, 0] = Int64.Parse(arr[0]);
                matrix[cnt, 1] = Int64.Parse(arr[1]);
                cnt++;
            }
            return matrix;
        }

    }

    public class ThreadBuffer
    {
        private object lockOn;
        private byte[] buffer, zipBuffer;

        public int NumBlock { get; set; }
        public ManualResetEvent ManualEvent { get; set; }

        public ThreadBuffer()
        {
            lockOn = new object();
        }
        public void ReadFromFile()
        {
            //Console.WriteLine("Begin ReadFromFile {0}", NumBlock);
            lock (lockOn)
            {
                if (BaseFile.name == "compress")
                {
                    using (FileStream fs = new FileStream(BaseFile.fileNameSrc, FileMode.Open, FileAccess.Read))
                    {
                        long offset = (long)NumBlock * Zip.BUFFER_SIZE;
                        buffer = new byte[Math.Min(Zip.BUFFER_SIZE, fs.Length - offset)];
                        fs.Seek(offset, SeekOrigin.Begin);
                        fs.Read(buffer, 0, buffer.Length);
                        fs.Close();
                    }
                }
                else
                {
                    using (BinaryReader reader = new BinaryReader(new FileStream(BaseFile.fileNameSrc, FileMode.Open, FileAccess.Read, FileShare.Read)))
                    {
                        reader.BaseStream.Position = BaseFile.dataBlocks[NumBlock, 0];
                        zipBuffer = reader.ReadBytes((int)BaseFile.dataBlocks[NumBlock, 1]);
                    }
                }
                Monitor.Pulse(lockOn);
            }
            Console.WriteLine("End ReadFromFile {0}", NumBlock);
        }

        public void RunOp()
        {
            //Console.WriteLine("Begin RunOp {0}", NumBlock);
            lock (lockOn)
            {
                if (BaseFile.name == "compress")
                {
                    if (buffer == null) Monitor.Wait(lockOn);
                    zipBuffer = buffer.Compress();
                }
                else
                {
                    if (zipBuffer == null) Monitor.Wait(lockOn);
                    buffer = zipBuffer.Decompress();
                }
                Monitor.Pulse(lockOn);
            }
            Console.WriteLine("End RunOp {0}", NumBlock);
        }

        public void SaveFile()
        {
            lock (lockOn)
            {
                if (BaseFile.name == "compress")
                {
                    if (zipBuffer == null) Monitor.Wait(lockOn); 
                    BaseFile.waitHandler.WaitOne();
                    //Console.WriteLine("Begin SaveFile {0}", NumBlock);
                    using (BinaryWriter bw = new BinaryWriter(new FileStream(BaseFile.fileNameDest, FileMode.Append)))
                    {
                        BaseFile.dataBlocks[NumBlock, 0] = bw.BaseStream.Position;
                        BaseFile.dataBlocks[NumBlock, 1] = zipBuffer.Length;
                        bw.Write(zipBuffer);
                        bw.Flush();
                    }
                    BaseFile.waitHandler.Set();
                }
                else
                {
                    if (buffer == null) Monitor.Wait(lockOn);
                    BaseFile.waitHandler.WaitOne();
                    using (BinaryWriter bw = new BinaryWriter(new FileStream(BaseFile.fileNameDest, FileMode.Open, FileAccess.Write, FileShare.None)))
                    {
                        bw.BaseStream.Position = (long)NumBlock * Zip.BUFFER_SIZE;
                        bw.Write(buffer);
                        bw.Flush();
                    }
                    BaseFile.waitHandler.Set();
                }
                Monitor.PulseAll(lockOn);
                ManualEvent.Set();
                BaseFile.cde.Signal();
            }
            Console.WriteLine("End SaveFile {0}", NumBlock);

        }
    }

    public abstract class BaseFile
    {
        public const int Limit = 64;
        public static string fileNameSrc;
        public static string fileNameDest;
        public static long[,] dataBlocks;
        public static string name;
        public static CountdownEvent cde;
        public static AutoResetEvent waitHandler = new AutoResetEvent(true);

        protected int _curBlocks = 0;
        protected int _fileBlocks = 0;
        protected long _sourceSize = 0;
        protected ManualResetEvent[] manualEvents;

        public BaseFile(string fileSource, string fileDestination)
        {
            if (!File.Exists(fileSource)) throw new Exception("Файл источник не найден.");

            String file = Path.GetFileName(fileDestination);
            if (String.IsNullOrEmpty(file)) throw new Exception("Не задано имя выходного файла.");
            String dir = Path.GetDirectoryName(fileDestination);
            if (!String.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (File.Exists(fileSource)) File.Delete(fileDestination);
            using (new FileStream(fileDestination, FileMode.CreateNew)) { }
            fileNameSrc = fileSource;
            fileNameDest = fileDestination;
        }

        private static int GetNumberOfTakedElements(int skip, WaitHandle[] waitHandles)
        {
            if (((int)Math.Ceiling(waitHandles.Length / (double)Limit) - skip) == 1)
            {
                return waitHandles.Length - skip * Limit;
            }

            return Limit;
        }

        protected void ThreadsStart()
        {
            manualEvents = new ManualResetEvent[_fileBlocks];
            cde = new CountdownEvent(_fileBlocks);
            for (int i = 0; i < _fileBlocks; i++)
            {
                manualEvents[i] = new ManualResetEvent(false);
                ThreadBuffer thrBuf = new ThreadBuffer();
                thrBuf.NumBlock = i;
                thrBuf.ManualEvent = manualEvents[i];
                Thread tt;
                tt = new Thread(thrBuf.ReadFromFile);
                tt.Name = $"ReadFromFile {i}";
                tt.Start();
                tt = new Thread(thrBuf.RunOp);
                tt.Name = $"RunOp {i}";
                tt.Start();
                tt = new Thread(thrBuf.SaveFile);
                tt.Name = $"SaveFile {i}";
                tt.Start();
            }
            WaitAllHandled(manualEvents);
            cde.Wait();
        }

        public static void WaitAllHandled(WaitHandle[] waitHandles)
        {
            if (waitHandles.Length <= Limit)
            {
                WaitHandle.WaitAll(waitHandles);
                return;
            }

            ManualResetEvent resetEvent = new ManualResetEvent(false);

            for (int i = 0; i < waitHandles.Length / Limit + 1; i++)
            {
                int localI = i;

                new Thread(() =>
                {
                    WaitAllHandled(waitHandles.Skip(localI * Limit).Take(
                      GetNumberOfTakedElements(localI, waitHandles)).ToArray());

                    resetEvent.Set();

                }).Start();
            }

            resetEvent.WaitOne();
        }
    }

}