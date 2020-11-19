using System;
using System.IO;
using System.Threading;

namespace GZipTest
{
    class DecompressFile: BaseFile
    {

        public DecompressFile(string fileSource, string fileDestination) : base(fileSource, fileDestination)
        {
            name = "decompress";
            Console.WriteLine("Начало чтения архива.");
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileNameSrc, FileMode.Open)))
                {
                    reader.BaseStream.Position = reader.BaseStream.Length - 8;
                    long pos = reader.ReadInt64();
                    reader.BaseStream.Position = pos;
                    _fileBlocks = reader.ReadInt32();
                    _sourceSize = reader.ReadInt64();
                    dataBlocks = Zip.StringToMatrix(reader.ReadString().DecompressFromBase64());
                }
                using (BinaryWriter bw = new BinaryWriter(new FileStream(fileNameDest, FileMode.Append)))
                {
                    long curBlockByte = _sourceSize;
                    while (curBlockByte > 0)
                    {
                        long bb = Math.Min(Zip.ARRAY_SIZE, curBlockByte);
                        bw.Write(new byte[bb]);
                        curBlockByte -= bb;
                        bw.Flush();
                    }
                }

            }
            catch (InvalidDataException)
            {
                throw new Exception("Ошибка формата файла архива.");
            }
        }

        public void Processing()
        {
            ThreadsStart();

            Console.WriteLine("end all threads");
        }
    }
}
