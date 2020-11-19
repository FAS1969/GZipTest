using System;
using System.IO;
using System.Threading;

namespace GZipTest
{
    class CompressFile: BaseFile
    {

        public CompressFile(string fileSource, string fileDestination) : base(fileSource, fileDestination)
        {
            name = "compress";
        }

        public void Processing()
        {
            FileInfo fi = new FileInfo(fileNameSrc);
            long blocks = fi.Length / Zip.BUFFER_SIZE + (fi.Length % Zip.BUFFER_SIZE == 0 ? 0 : 1);
            if (blocks > Int32.MaxValue) throw new Exception("Слишком большое количество блоков для архивации.");
            _fileBlocks = (int)blocks;
            Console.WriteLine("Файл разбит на {0} блоков ({1})", _fileBlocks, fi.Length);
            dataBlocks = new long[_fileBlocks, 2];
            _sourceSize = fi.Length;

            ThreadsStart();

            string strArrDataBlock = Zip.MatrixToString(dataBlocks);
            using (BinaryWriter bw = new BinaryWriter(new FileStream(fileNameDest, FileMode.Append)))
            {
                long pos = bw.BaseStream.Position;
                bw.Write(_fileBlocks);
                bw.Write(_sourceSize);
                bw.Write(strArrDataBlock.CompressToBase64());
                bw.Write(pos);
            }
            Console.WriteLine("end all threads");
        }
    }
}
